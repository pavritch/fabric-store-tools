using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;
using Utilities.Extensions;

namespace ProductScanner.App
{
    public partial class VendorModel : IVendorModel
    {
        private readonly IVendorRunner _vendorRunner;
        private readonly IVendorScanSessionManager _session;
        private readonly IStorageProvider _storageProvider;
        private readonly IBatchCommitter _batchCommitter;
        private CookieCollection _currentCookies;

        public VendorModel(IStoreModel parent, Vendor info, bool isFullyImplemented = true)
        {
            Vendor = info;

            // these really should be constructor dependencies
            _session = App.GetInstance(typeof(IVendorScanSessionManager<>).MakeGenericType(info.GetType())) as IVendorScanSessionManager;
            _storageProvider = App.GetInstance(typeof(IStorageProvider<>).MakeGenericType(info.GetType())) as IStorageProvider;
            _vendorRunner = App.GetInstance(typeof(IVendorRunner<>).MakeGenericType(Vendor.GetType())) as IVendorRunner;
            _batchCommitter = App.GetInstance(typeof(BatchCommitter<>).MakeGenericType(Vendor.GetType())) as IBatchCommitter;

            _session.LogAdded += async (sender, record) => await AddScanningLogEventAsync(record);
            _session.LogCleared += async (sender, item) => await ClearLogAsync();
            _session.CheckpointSaved += async (sender, checkpoint) => await CreatedScanningCheckpointAsync();
            _session.CommitSubmitted += async (sender, type) => await SubmittedCommitBatchAsync();
            _session.ScanFinished += async (sender, type) => await FinishedScanningAsync();
            _session.ScanFailed += async (sender, type) => await FailedScanningAsync();
            _session.ScanCancelled += async (sender, type) => await CancelledScanningAsync();
            _session.ScanSuspended += async (sender, type) => await SuspendedScanningAsync();
            _session.StatusChange +=  (sender, scanStats) => UpdateStatus(scanStats);

            CachedFilesFolder = _storageProvider.GetCacheFolder();
            StaticFilesFolder = _storageProvider.GetStaticFolder();

            ParentStore = parent;

            // if false from the start, then no attempt will be made to go further and the store
            // will appear in the UI as nothing more than a visual placeholder (as disabled).

            IsFullyImplemented = isFullyImplemented && info.IsFullyImplemented;

            ResetPropertiesToDefaultValues();

            // when not fully implemented, we skip all the initialization stuff
            IsInitialized = !IsFullyImplemented;

            Name = info.DisplayName;

            ScanningLogEvents = new ObservableCollection<EventLogRecord>();
            ScanningOptions = new ObservableCollection<ScanOptions>();

            if (!ViewModelBase.IsInDesignModeStatic)
            {
                HookMessages();
            }
        }

        #region Vendor Testing

        /// <summary>
        /// Gets the list of tests which can be performed for this vendor.
        /// </summary>
        /// <returns></returns>
        public List<TestDescriptor> GetTests()
        {
            var vendorTester = App.GetInstance(typeof(IVendorTestRunner<>).MakeGenericType(Vendor.GetType())) as IVendorTestRunner;
            var tests = vendorTester.GetVendorTests();
            return tests.Select(x => new TestDescriptor(x.GetType().Name, x.GetDescription(), x)).ToList();
        }


        /// <summary>
        /// Run a single test.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task<TestResultCode> RunTestAsync(IVendorTest test)
        {
            try
            {
                var result = await test.RunAsync(_currentCookies);
                return result.Code;
            }
            catch
            {
                return TestResultCode.Failed;
            }
        }

        /// <summary>
        /// Check to see if can log in to vendor's website using current username/password.
        /// </summary>
        /// <remarks>
        /// Upon completion, internally must set IsVendorWebsiteLoginValid to match the returned result.
        /// If failed, set the warning.
        /// </remarks>
        /// <returns></returns>
        public Task<bool> VerifyVendorWebsiteCredentialsAsync()
        {
            // must set IsCheckingCredentials while operation is in progress
            // must set IsVendorWebsiteLoginValid with result

            // Note that IsVendorWebsiteLoginValid will be null until validated for the first time during
            // a given app runtime session. Then it remains with some value for life of app or until retested.

            IsCheckingCredentials = true;
            IsVendorWebsiteLoginValid = null;
            SendVendorChangedNotification();

            var tcs = new TaskCompletionSource<bool>();
            Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        // TODO: review if any threading issues here, more so with UX

                        var vendorAuthenticator = App.GetInstance(typeof(IVendorAuthenticator<>).MakeGenericType(Vendor.GetType())) as IVendorAuthenticator;
                        var loginResult = await vendorAuthenticator.LoginAsync();
                        IsVendorWebsiteLoginValid = loginResult.IsSuccessful;

                        tcs.SetResult(loginResult.IsSuccessful);
                    }
                    catch (Exception)
                    {
                        IsVendorWebsiteLoginValid = false;
                        tcs.SetResult(false);
                    }
                    finally
                    {
                        IsCheckingCredentials = false;
                    }

                    if (IsVendorWebsiteLoginValid.GetValueOrDefault())
                        ClearWarning();
                    else
                        SetWarning("Invalid vendor website credentials.");

                    SendVendorChangedNotification();
                    return IsVendorWebsiteLoginValid.GetValueOrDefault();

                }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;
        }

        #endregion

        #region Commit Batches

        // SHANE: Peter will do the "final" true commit logic, however, like you sort of did a bit back on the other WPF app,
        // you should try to lay this all out for me with the right references, access to databases, etc - so I'm not needing
        // to get into your code. 

        /// <summary>
        /// Discard the specified batch. Just marked as discarded in scanner SQL.
        /// </summary>
        /// <param name="batchId">Batch must be associated with this vendor.</param>
        /// <returns></returns>
        public async Task<CommitBatchResult> DiscardBatchAsync(int batchId)
        {
            // if cannot hit SQL (store or scanner) right off, then return DatabaseError.

            // if batch id no longer in scanner SQL, return NotFound

            // if batch id is not for "this vendor", return IncorrectVendor (indicative of a proramming error)

            // if batch status is InProgress, return AccessDenied

            // if batch is not currently pending, return NotPending

            try
            {

                var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
                var result = await dbScanner.DiscardBatchAsync(batchId, Vendor.Id);

                await DiscardedOrDeletedCommitBatch();

                return result;

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return CommitBatchResult.Failed;
            }
        }

        /// <summary>
        /// Full delete of the indicated batch. Usually reserved for removing already-discarded batches to clean things up.
        /// </summary>
        /// <param name="batchId">Batch must be associated with this vendor.</param>
        /// <returns></returns>
        public async Task<CommitBatchResult> DeleteBatchAsync(int batchId)
        {
            // if cannot hit SQL (store or scanner) right off, then return DatabaseError.

            // if batch id no longer in scanner SQL, return NotFound

            // if batch id is not for "this vendor", return IncorrectVendor (indicative of a proramming error)

            // if batch status is InProgress, return AccessDenied

            try
            {

                var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
                var result = await dbScanner.DeleteBatchAsync(batchId, Vendor.Id);

                // notify listeners
                await DiscardedOrDeletedCommitBatch();

                return result;

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return CommitBatchResult.Failed;
            }
        }


        /// <summary>
        /// Commit a single batch from scanner SQL to store SQL.
        /// </summary>
        /// <remarks>
        /// The batch must be assoc with this vendor, still in scanner SQL, not marked discarded and not already in progress.
        /// </remarks>
        /// <param name="batchId"></param>
        /// <param name="estimatedRecordCount">used only to assist in simulations, so not needed to pass through to live cod</param>
        /// <param name="ignoreDuplicates">when true, treat duplicate SKU inserts as having inserted successfully (for re-run of cancelled operation).</param>
        /// <param name="cancelToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public Task<CommitBatchResult> CommitBatchAsync(int batchId, int estimatedRecordCount, bool ignoreDuplicates, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null)
        {
            // estimatedRecordCount is used only to assist in simulations, so not needed to pass through to live code.

            var tcs = new TaskCompletionSource<CommitBatchResult>();
            Task.Run(async () =>
            {
                try
                {
                    var result = await _batchCommitter.CommitBatchAsync(batchId, ignoreDuplicates, cancelToken, progress);

                    var tasks = new List<Task>()
                    {
                        RefreshRecentCommits(),
                        RefreshPendingBatchCount(),
                        RefreshProductCounts(),
                    };

                    await Task.WhenAll(tasks);
                    ScannerState = DetermineScannerState();
                    SendVendorChangedNotification();
                    await SendScanningOperationNotification(ScanningOperationEvent.CommittedBatch);

                    tcs.SetResult(result);
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.Message);
                    tcs.SetResult(CommitBatchResult.Failed);
                }
            });

            return tcs.Task;
        }
        
        #endregion

        #region Cached Files
        /// <summary>
        /// Delete downloaded cached files for vendor which are older than N days - 0 removes all.
        /// </summary>
        /// <remarks>
        /// Must ensure that file count metrics updated prior to return.
        /// </remarks>
        /// <param name="olderThanDays"></param>
        /// <param name="cancelToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public Task<ActivityResult> DeleteCachedFilesAsync(int olderThanDays, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null)
        {
            // 0 days means to delete everything.
            if (CachedFilesFolder == null)
                return Task.FromResult(ActivityResult.Success);

            var tcs = new TaskCompletionSource<ActivityResult>();
            Task.Run(() =>
            {
                _storageProvider.DeleteCache(olderThanDays);
                tcs.SetResult(ActivityResult.Success);
                RecalculateCachedFilesMetrics(); // the live code must include this call
            });
            return tcs.Task;
        }
        
        #endregion

        #region Scanning

        // There is a requirement to persist certain properties to checkpoints, and rehydrate back at app start
        // when Initialize() determines that there is a viable checkpoint. These properties include:
        //
        // ScanningStartTime
        // ScanningDuration
        // ScanningLogEvents
        // ScannionOptions
        // ScanningErrorCount

        // Many of the properties included below will persist in memory beyond the actual terminate of a scanning operation,
        // because it is necessary to show the final result and related attributes after the fact. Per above, some of these
        // will persist with checkpoints so can be rehydrated. Others will vanish at app exit, or be repopulated if a new
        // scanning operation is started.

        /// <summary>
        /// Initiate a fresh scanning operation based on the provided options.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<ScanningActionResult> StartScanning(ScanOptions options)
        {
            // do not throw exception up the call stack
            try
            {
                if (IsScanning)
                    return ScanningActionResult.Success;

                ClearScanningState();

                if (!IsScanningStartable)
                    return ScanningActionResult.Failed;

                IsCallingScanningOperation = true;
                ScanningOptions = new ObservableCollection<ScanOptions>(options.MaskToList<ScanOptions>());

                // our rule is that starting a scan instantly stales out any uncommitted batches.
                // Some UX methods prompt about this and will cancel the op before it gets here if
                // this is not desired.

                // this should be part of the core logic
                var pendingBatches = await GetPendingCommitBatchSummariesAsync();
                foreach (var batch in pendingBatches)
                    await DiscardBatchAsync(batch.Id);

                await StartedScanning();
                IsScanning = true;
                _vendorRunner.Start(options);
                return ScanningActionResult.Success;
            }
            finally
            {
                IsCallingScanningOperation = false;
            }
        }

        /// <summary>
        /// Cancel the running or suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        public async Task<ScanningActionResult> CancelScanning()
        {
            // see if invalid to have been called at this time
            // technically a programming bug to have been able to call this at the wrong time

            // do not throw exception up the call stack

            try
            {
                if (!IsScanning && !IsSuspended)
                    return ScanningActionResult.Success;

                if (!IsScanningCancellable)
                    return ScanningActionResult.Failed;

                IsCallingScanningOperation = true;

                // if there is an active operation (running or suspended), then there should be little if 
                // anything that should result in a failure here, since cancel means cancel.

                await _vendorRunner.Cancel();
                if (!IsScanning)
                {
                    await CancelledScanningAsync();
                }
                return ScanningActionResult.Success;
            }
            finally
            {
                IsCallingScanningOperation = false;
            }
        }

        /// <summary>
        /// Suspend the now running scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        public Task<ScanningActionResult> SuspendScanning()
        {
            // see if invalid to have been called at this time
            // technically a programming bug to have been able to call this at the wrong time

            // likely want some better granularity on kinds of failures which are possible in real life

            // do not throw exception up the call stack

            try
            {
                if (IsSuspended)
                    return Task.FromResult(ScanningActionResult.Success);

                if (!IsScanningSuspendable)
                    return Task.FromResult(ScanningActionResult.Failed);

                IsCallingScanningOperation = true;

                _vendorRunner.Suspend();
                return Task.FromResult(ScanningActionResult.Success);
            }
            finally
            {
                IsCallingScanningOperation = false;
            }
        }

        /// <summary>
        /// Resume the presently-suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        public async Task<ScanningActionResult> ResumeScanning()
        {
            // see if invalid to have been called at this time
            // technically a programming bug to have been able to call this at the wrong time

            // likely want some better granularity on kinds of failures which are possible in real life

            // do not throw exception up the call stack

            try
            {
                if (IsScanning)
                    return ScanningActionResult.Success;

                if (!IsScanningResumable)
                    return ScanningActionResult.Failed;

                //ScanningOptions = new ObservableCollection<ScanOptions>(options.MaskToList<ScanOptions>());
                IsCallingScanningOperation = true;
                ScanningProgressStatusMessage = "Resuming...";

                // this should be part of the core logic
                var pendingBatches = await GetPendingCommitBatchSummariesAsync();
                foreach (var batch in pendingBatches)
                    await DiscardBatchAsync(batch.Id);

                _vendorRunner.Resume();
                await ResumedScanning();

                return ScanningActionResult.Success;
            }
            finally
            {
                IsCallingScanningOperation = false;
            }
        }


        #endregion

        #region Initialization Logic

        /// <summary>
        /// Initializes this vendor instance.
        /// </summary>
        /// <remarks>
        /// Called from MainWindowViewModel when the splash screen is showing so we don't
        /// attempt to show any normal UX until we've populated our stores and vendors.
        /// </remarks>
        /// <returns>False to terminate application due to fatal error situation.</returns>
        public async Task<bool> InitializeAsync()
        {
            if (IsInitialized)
                return true;

            // failing to initialize is a drastic thing that will result in the app terminating itself,
            // so only return false here if truly unable to continue to run.

            try
            {
                var database = App.GetInstance<IPlatformDatabase>();
                var vendorExists = await database.DoesVendorExist(Vendor.Id);
                if (!vendorExists)
                {
                    await database.AddVendorData(new VendorData
                    {
                        Id = Vendor.Id,
                        Name = Vendor.DisplayName,
                        Status = VendorStatus.Manual,
                        Store = Vendor.Store,
                    });
                }

                IsInitialized = true;

                if (IsInitialized)
                    Refresh();
            }
            catch (Exception Ex)
            {
                // bomb the app - will terminate with a message
                Debug.WriteLine(Ex.Message);
                IsInitialized = false;
            }

            return IsInitialized;
        }


        /// <summary>
        /// Main internal entry point for establishing the state of a vendor.
        /// </summary>
        /// <remarks>
        /// Make calls to disk, sql, etc., to figure out where things stand for this vendor.
        /// Used mostly for starting up app, but could be called again, as long as no running operation.
        /// </remarks>
        protected async void Refresh()
        {
            // would likely not want to disturb things when there's a running operation.
            Debug.Assert(!IsScanning && !IsSuspended);

            // TODO: real values

            // These defaults can be different per vendor, comes from core vendor module
            // DefaultDelayMSBetweenVendorWebsiteRequests
            // DefaultMaximumScanningErrorCount

            // then set current values to be same as defaults for the vendor UNLESS rehydrating from a suspended operation
            // in which case the persisted values would be used for the current settings
            MaximumScanningErrorCount = DefaultMaximumScanningErrorCount;
            MaximumScanningErrorCount = 5;

            // Properties needing to be populated at startup/refresh

            IsScanning = false; // implicit since just starting up.
            IsPerformingTests = false;
            IsCheckingCredentials = false;
            SetWarning(null); // start each app run with no warnings since they are not persisted anywhere

            // this task list contains only tasks which have no data dependencies on each other
            // and can run perfectly well in parallel.
            var tasks = new List<Task>()
            {
                RefreshProductCounts(),
                RefreshVendorProperties(),
                RefreshRecentCommits(),
                RefreshCheckpointDetails(),
                RefreshPendingBatchCount(),
            };

            await Task.WhenAll(tasks);

            // below can only be populated once the above tasks have completed since 
            // depends on certain propertes being populated already

            ScannerState = DetermineScannerState();
            ScanningOperationStatus = DetermineInitialScanningOperationStatus();
            ClearProgressMeter();

            // NOTE: one thing that's (for the moment) not being done here is calculating static/cached
            // file storage metrics. The thought was that it might be risky firing off 40+ of these at the same
            // time on app start, so for now, not calc'd until needed the first time. We can see how this plays out.

            SendVendorChangedNotification();
        }

        protected void HookMessages()
        {

            Messenger.Default.Register<AnnouncementMessage>(this, (msg) =>
            {
                switch (msg.Kind)
                {
                    case Announcement.OneSecondIntervalTimer:
                        OneSecondInterval();
                        break;
                }
            });
        }
        #endregion

        #region Status Update During Scanning

        // the complexity here is actually just a simple way of not pushing 
        // all the tiny status updates through .... try to send immediately to WPF,
        // but then set a timer and don't send any more until the time elapses.

        private string _shadowScanningProgressSecondaryMessage;
        private string _shadowScanningProgressStatusMessage;
        private double? _shadowScanningProgressPercentComplete;
        private int _shadowScanningErrorCount;
        private bool _updateTimerRunning = false;
        private static object _updateLockObj = new object();
        private const int _updateTimerDelayMS = 100;
        private System.Threading.Timer _updateTimer;
        private bool _shadowHasData = false;

        private void UpdateStatus(VendorSessionStats stats)
        {
            bool triggerUpdateTimer = false;

            lock (_updateLockObj)
            {
                if (!_updateTimerRunning)
                {
                    triggerUpdateTimer = true;
                }
                else
                {
                    _shadowHasData = true;
                    // must capture values with the lock
                    _shadowScanningProgressSecondaryMessage = stats.SecondaryMessage;
                    _shadowScanningProgressStatusMessage = stats.CurrentTask;
                    _shadowScanningProgressPercentComplete = stats.PercentComplete;
                    _shadowScanningErrorCount = stats.ErrorCount;
                }
            }

            if (triggerUpdateTimer)
            {
                string _shadow3ScanningProgressSecondaryMessage = null;
                string _shadow3ScanningProgressStatusMessage = null;
                double? _shadow3ScanningProgressPercentComplete = null;
                int _shadow3ScanningProgressErrorCount = 0;

                lock (_updateLockObj)
                {
                    _shadow3ScanningProgressSecondaryMessage = stats.SecondaryMessage;
                    _shadow3ScanningProgressStatusMessage = stats.CurrentTask;
                    _shadow3ScanningProgressPercentComplete = stats.PercentComplete;
                    _shadow3ScanningProgressErrorCount = stats.ErrorCount;
                }

                // then timer was not running, so immediately send this status out to WPF
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // update WFP controls and allow binding to take place without worrying about the lock
                    ScanningProgressSecondaryMessage = _shadow3ScanningProgressSecondaryMessage;
                    ScanningProgressPercentComplete = _shadow3ScanningProgressPercentComplete;
                    ScanningProgressStatusMessage = _shadow3ScanningProgressStatusMessage;
                    ScanningProgressErrorCount = _shadow3ScanningProgressErrorCount;
                });

                // then set a timer so that 
                _updateTimer = new System.Threading.Timer((st) =>
                {
                    string _shadow2ScanningProgressSecondaryMessage = null;
                    string _shadow2ScanningProgressStatusMessage = null;
                    double? _shadow2ScanningProgressPercentComplete = null;
                    int _shadow2ScanningProgressErrorCount = 0;
                    bool _shadow2HasData = false;

                    lock (_updateLockObj)
                    {
                        // copy these within the lock so can use their values outside the lock to update the WPF controls
                        // and not worry about holding the lock

                        if (_shadowHasData)
                        {
                            _shadow2HasData = true;
                            _shadowHasData = false;
                            _shadow2ScanningProgressSecondaryMessage = _shadowScanningProgressSecondaryMessage;
                            _shadow2ScanningProgressStatusMessage = _shadowScanningProgressStatusMessage;
                            _shadow2ScanningProgressPercentComplete = _shadowScanningProgressPercentComplete;
                            _shadow2ScanningProgressErrorCount = _shadowScanningErrorCount;
                        }
                        _updateTimerRunning = false;
                        //_updateTimer.Dispose(); // internal windows throwing exceptions here.
                        _updateTimer = null;
                    }

                    if (_shadow2HasData)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // update WFP controls and allow binding to take place without worrying about the lock
                            ScanningProgressSecondaryMessage = _shadow2ScanningProgressSecondaryMessage;
                            ScanningProgressPercentComplete = _shadow2ScanningProgressPercentComplete;
                            ScanningProgressStatusMessage = _shadow2ScanningProgressStatusMessage;
                            ScanningProgressErrorCount = _shadow2ScanningProgressErrorCount;
                        });
                    }

                }, null, _updateTimerDelayMS, System.Threading.Timeout.Infinite);

                _updateTimerRunning = true;
                //System.Threading.Thread.Sleep(2);
            }
        } 
        #endregion

        public Vendor Vendor { get; private set; }

        public int StaticFileVersionTxt
        {
            get
            {
                // TODO: check when this is actually accessed - might need to cache it
                return _storageProvider.GetStaticDataVersion();
            }
        }
    }
}
