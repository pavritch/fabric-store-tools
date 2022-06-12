using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Checkpoints;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using Utilities;

namespace ProductScanner.Core.Sessions
{
    public interface IVendorScanSessionManager
    {
        Task ForEachNotifyAsync<T>(string taskName, IEnumerable<T> source, Func<T, Task> action);
        Task ForEachNotifyAsync<T>(string taskName, IEnumerable<T> source, Func<int, T, Task> action);
        void ForEachNotify<T>(string taskName, IEnumerable<T> source, Action<T> action);

        CookieCollection GetCookies();
        Task<bool> AuthAsync(ScanOptions options = ScanOptions.None);
        Task<bool> Reauthenticate();
        void StartScan(ScanOptions options);
        void ResumeScan(ScannerCheckpoint checkpoint);

        void Cancel();

        void ClearLog();
        void Log(EventLogRecord record);
        void ReplaceLog(List<EventLogRecord> logs);
        ReadOnlyCollection<EventLogRecord> GetFullLog();

        // events for UI notifications
        event EventHandler<EventLogRecord> LogAdded;
        event EventHandler LogCleared;
        event EventHandler<CheckpointData> CheckpointSaved;
        event EventHandler CommitSubmitted;
        event EventHandler ScanFinished;
        event EventHandler ScanFailed;
        event EventHandler ScanCancelled;
        event EventHandler ScanSuspended;
        event EventHandler<VendorSessionStats> StatusChange;

        void BumpVendorRequest();
        void BumpErrorCount();
        VendorSessionStats VendorSessionStats { get; set; }
        MultiTimeline ScanningRequestsTimeline { get; }
        bool HasErrored { get; set; }
        bool CanSuspend { get; set; }
        int MaximumScanningErrorCount { get; set; }

        void ThrowIfCancellationRequested();
        Guid GetSessionId();
        TimeSpan GetTotalDuration();

        int UserDefinedThrottle { get; set; }
        Task<int> GetThrottleAsync();

        void NotifyCheckpointSaved(CheckpointData checkpointData);
        void NotifyCommitSubmitted();
        void NotifyScanFinished();
        void NotifyScanFailed();
        void NotifyScanSuspended();

        bool HasFlag(ScanOptions flag);
    }

    public interface IVendorScanSessionManager<TVendor> : IVendorScanSessionManager where TVendor : Vendor
    {
        string GetLoginUrl();
        CancellationToken GetCancellationToken();

        DateTime GetStartTime();
    }
}