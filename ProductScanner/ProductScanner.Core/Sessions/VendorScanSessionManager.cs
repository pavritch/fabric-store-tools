using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.Config;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Checkpoints;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.Scanning.Storage;
using Utilities;

namespace ProductScanner.Core.Sessions
{
    // session manager used as part of a scanning session
    public class VendorScanSessionManager<TVendor> : IVendorScanSessionManager<TVendor> where TVendor : Vendor, new()
    {
        private VendorSession _session;
        private string _loginUrl;
        private Guid _sessionId;

        // overall start time for when the scan was initially kicked off
        // not used for any duration calculations
        private DateTime _scanStarted;

        // accumulated duration from suspend/resumes 
        private TimeSpan _duration;

        // the most recent start/resume date time - used to calculate current duration
        private DateTime _mostRecentStartTime;

        private CancellationTokenSource _cancellationTokenSource;
        private ScanOptions _options; 

        // events that UIs can subscribe to
        public event EventHandler<EventLogRecord> LogAdded;
        public event EventHandler LogCleared;
        public event EventHandler<CheckpointData> CheckpointSaved;
        public event EventHandler CommitSubmitted;
        public event EventHandler ScanFinished;
        public event EventHandler ScanFailed;
        public event EventHandler ScanCancelled;
        public event EventHandler ScanSuspended;
        public event EventHandler<VendorSessionStats> StatusChange;

        private readonly IVendorAuthenticator<TVendor> _authenticator;
        private readonly IAppSettings _appSettings;
        private readonly IStorageProvider<TVendor> _storageProvider;

        public VendorSessionStats VendorSessionStats { get; set; }
        public bool CanSuspend { get; set; }
        public int UserDefinedThrottle { get; set; }
        public int MaximumScanningErrorCount { get; set; }
        public bool HasErrored { get; set; } 

        public List<EventLogRecord> _fullLog;

        private MultiTimeline _requestsTimeline;

        public MultiTimeline ScanningRequestsTimeline
        {
            get { return _requestsTimeline; }
        }

        public VendorScanSessionManager(IVendorAuthenticator<TVendor> authenticator, IAppSettings appSettings, IStorageProvider<TVendor> storageProvider)
        {
            _requestsTimeline = new MultiTimeline();

            VendorSessionStats = new VendorSessionStats();
            VendorSessionStats.StatsChanged += (sender, args) => NotifyStatusChange();

            _authenticator = authenticator;
            _appSettings = appSettings;
            _storageProvider = storageProvider;
            _duration = new TimeSpan();

            _session = VendorSession.Invalid();
            _fullLog = new List<EventLogRecord>();

            CanSuspend = false;
        }

        public void Log(EventLogRecord record)
        {
            _fullLog.Add(record);
            if (LogAdded != null) LogAdded(this, record);
        }

        public void ClearLog()
        {
            _fullLog.Clear();
            if (LogCleared != null) LogCleared(this, null);
        }

        public void ReplaceLog(List<EventLogRecord> logs)
        {
            ClearLog();
            foreach (var log in logs) Log(log);
        }

        public ReadOnlyCollection<EventLogRecord> GetFullLog()
        {
            return new ReadOnlyCollection<EventLogRecord>(_fullLog);
        }

        public TimeSpan GetTotalDuration()
        {
            if (_mostRecentStartTime == DateTime.MinValue) return new TimeSpan(0, 0, 0);
            return (DateTime.Now - _mostRecentStartTime) + _duration;
        }

        public DateTime GetStartTime()
        {
            return _scanStarted;
        }

        public void StartScan(ScanOptions options)
        {
            _options = options;
            if (_options.HasFlag(ScanOptions.DeleteCachedFilesBeforeStarting)) _storageProvider.DeleteCache(0);
            VendorSessionStats.Clear();
            _scanStarted = DateTime.Now;
            _mostRecentStartTime = DateTime.Now;
            _sessionId = Guid.NewGuid();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void ResumeScan(ScannerCheckpoint checkpoint)
        {
            _options = checkpoint.ScanOptions;
            _scanStarted = checkpoint.ScanStarted;
            _duration = new TimeSpan(0, 0, checkpoint.DurationInSeconds);
            _mostRecentStartTime = DateTime.Now;
            _fullLog = checkpoint.Log.FromJSON<List<EventLogRecord>>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> AuthAsync(ScanOptions options)
        {
            VendorSessionStats.CurrentTask = "Authenticating";

            var auth = await _authenticator.LoginAsync();

            _session = VendorSession.New(auth.Cookies);
            _loginUrl = auth.LoginUrl;
            return auth.IsSuccessful;
        }

        public async Task<bool> Reauthenticate()
        {
            var auth = await _authenticator.LoginAsync();
            _session = VendorSession.New(auth.Cookies);
            _loginUrl = auth.LoginUrl;
            return auth.IsSuccessful;
        }

        public bool HasFlag(ScanOptions flag)
        {
            return _options.HasFlag(flag);
        }

        public string GetLoginUrl()
        {
            return _loginUrl;
        }

        public void Cancel()
        {
            if (_cancellationTokenSource != null) _cancellationTokenSource.Cancel();
        }

        public void ThrowIfCancellationRequested()
        {
            try
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                if (HasErrored) NotifyScanFailed();
                else NotifyScanCancelled();
                throw;
            }
        }

        public CancellationToken GetCancellationToken()
        {
            return _cancellationTokenSource.Token;
        }

        public async Task<int> GetThrottleAsync()
        {
            if (UserDefinedThrottle != 0) return UserDefinedThrottle;
            var vendor = new TVendor();
            if (vendor.ThrottleInMs != 0) return vendor.ThrottleInMs;
            return await _appSettings.GetValueAsync<TVendor, int>(AppSettingType.ThrottleDelay);
        }

        public async Task ForEachNotifyAsync<T>(string taskName, IEnumerable<T> source, Func<T, Task> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            if (!source.Any()) return;

            var countCompleted = 1;
            VendorSessionStats.TotalItems = source.Count();
            VendorSessionStats.CurrentTask = taskName;
            foreach (var element in source)
            {
                ThrowIfCancellationRequested();
                try
                {
                    await action(element);
                }
                catch (Exception e)
                {
                    Log(EventLogRecord.Error(e.Message));
                    BumpErrorCount();
                }
                VendorSessionStats.CompletedItems = countCompleted++;
                NotifyStatusChange();
            }
            VendorSessionStats.CurrentTask = string.Empty;
            Log(new EventLogRecord("Completed: {0}: {1:N0} times", taskName, source.Count()));
        }

        public async Task ForEachNotifyAsync<T>(string taskName, IEnumerable<T> source, Func<int, T, Task> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            if (!source.Any()) return;

            var countCompleted = 1;
            VendorSessionStats.TotalItems = source.Count();
            VendorSessionStats.CurrentTask = taskName;
            foreach (var element in source)
            {
                ThrowIfCancellationRequested();
                try
                {
                    await action(countCompleted - 1, element);
                }
                catch (Exception e)
                {
                    Log(EventLogRecord.Error(e.Message));
                    BumpErrorCount();
                }
                VendorSessionStats.CompletedItems = countCompleted++;
                NotifyStatusChange();
            }
            VendorSessionStats.CurrentTask = string.Empty;
            Log(new EventLogRecord("Completed: {0}: {1:N0} times", taskName, source.Count()));
        }

        public void ForEachNotify<T>(string taskName, IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");
            if (!source.Any()) return;

            var countCompleted = 1;
            VendorSessionStats.TotalItems = source.Count();
            VendorSessionStats.CurrentTask = taskName;
            foreach (var element in source)
            {
                ThrowIfCancellationRequested();
                try
                {
                    action(element);
                }
                catch (Exception e)
                {
                    Log(EventLogRecord.Error(e.Message));
                    BumpErrorCount();
                }
                VendorSessionStats.CompletedItems = countCompleted++;
                NotifyStatusChange();
            }
            VendorSessionStats.CurrentTask = string.Empty;
            Log(new EventLogRecord("Completed: {0}: {1:N0} times", taskName, source.Count()));
        }

        public void BumpErrorCount()
        {
            VendorSessionStats.ErrorCount++;
            if (MaximumScanningErrorCount != 0 && VendorSessionStats.ErrorCount >= MaximumScanningErrorCount)
            {
                HasErrored = true;
                Cancel();
            }
            NotifyStatusChange();
        }

        public CookieCollection GetCookies()
        {
            return _session.CookieCollection;
        }

        public Guid GetSessionId()
        {
            return _sessionId;
        }

        public void BumpVendorRequest()
        {
            _requestsTimeline.Bump();
        }

        public void NotifyCheckpointSaved(CheckpointData checkpointData)
        {
            if (CheckpointSaved != null) CheckpointSaved(this, checkpointData);
        }

        public void NotifyCommitSubmitted()
        {
            if (CommitSubmitted != null) CommitSubmitted(this, null);
        }

        public void NotifyScanFinished()
        {
            if (ScanFinished != null) ScanFinished(this, null);
            Reset();
        }

        public void NotifyScanFailed()
        {
            if (ScanFailed != null) ScanFailed(this, null);
            Reset();
        }

        private void NotifyScanCancelled()
        {
            if (ScanCancelled != null) ScanCancelled(this, null);
            Reset();
        }

        public void NotifyScanSuspended()
        {
            if (ScanSuspended != null) ScanSuspended(this, null);
            Reset();
        }

        private void NotifyStatusChange()
        {
            if (StatusChange != null) StatusChange(this, VendorSessionStats);
        }

        private void Reset()
        {
            _mostRecentStartTime = DateTime.MinValue;
            _duration = new TimeSpan(0, 0, 0);
            _requestsTimeline = new MultiTimeline();
        }
    }
}