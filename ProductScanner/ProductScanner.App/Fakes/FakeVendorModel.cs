using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.Core;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.App
{
    // SHANE: this file contains most of what will need changing to integrate with your vendor logic.

    // SHANE: You'll also need to recraft FileManager, ScannerDatabaseConnector and StoreDatabaseConnector.
    // These managers/connectors are virtualized APIs for access to disk/SQL storage using repository pattern
    // so we don't have any direct access logic anywhere else in the app.

    // SHANE: Note that more often than not, the ordering of logic in the methods below is critical, and you
    // should generally stick to what you see for the flow of things as you replace bits of things with yours.

    public partial class FakeVendorModel : ObservableObject, IVendorModel
    {
        #region Local Data
        private FakeScanner fakeScanner = null; // FAKE-remove for final 
        #endregion

        #region Ctor
        public FakeVendorModel(IStoreModel parent, Vendor info, bool isFullyImplemented = true)
        {
            ParentStore = parent;

            // if false from the start, then no attempt will be made to go further and the store
            // will appear in the UI as nothing more than a visual placeholder (as disabled).

            // TODO-LIVE: see if core vendor module reports that is not fully implemented, and when not,
            // then that trumps the input parameter (which is more so only knowing if has SQL records),
            // so will then be not implemented no matter what.

            IsFullyImplemented = isFullyImplemented;

            ResetPropertiesToDefaultValues();

            // when not fully implemented, we skip all the initialization stuff
            IsInitialized = !IsFullyImplemented;

            // TODO-LIVE: adjust ctor to take Vendor<T> rather than StoreModel.VendorInfo

            // SHANE - it is intended that this ctor be changed to take something like a Vendor<T> rather than VendorInfo,
            // and as such, can get the display name and vendorID from there (and store it too since assuming will need).

            VendorId = info.Id;
            Name = info.DisplayName;

            ScanningLogEvents = new ObservableCollection<EventLogRecord>();
            ScanningOptions = new ObservableCollection<ScanOptions>();
            ScanningRequestsTimeline = new MultiTimeline();

            if (!ViewModelBase.IsInDesignModeStatic)
            {
                HookMessages();
            }
        }


        #endregion

        #region Vendor Testing

        /// <summary>
        /// Gets the list of tests which can be performed for this vendor.
        /// </summary>
        /// <returns></returns>
        public List<TestDescriptor> GetTests()
        {
            var list = new List<TestDescriptor>()
            {
                new TestDescriptor("One", "This is my first test.", null),
                new TestDescriptor("Two", "This is another test which is the second.", null),
                new TestDescriptor("Three", "This third test is supposed to be the charm.", null),
                new TestDescriptor("Four", "Not sure why but this is my fourth test.", null),
                new TestDescriptor("Five", "And this fifth test is for good measure.", null),
            };

            return list;
        }


        /// <summary>
        /// Run a single test.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public Task<TestResultCode> RunTestAsync(IVendorTest test)
        {
            try
            {
                var rnd = new Random();

                var msDelay = rnd.Next(1000, 5000);

                //await Task.Delay(TimeSpan.FromMilliseconds(msDelay), cancelToken);

                // the Delay will throw exception if cancel during...so will not actually
                // hit this as being cancelled here
                //if (cancelToken.IsCancellationRequested)
                    //return TestResultCode.Cancelled;

                // fake out some to fail
                return Task.FromResult(rnd.Next(1, 5) == 3 ? TestResultCode.Failed : TestResultCode.Successful);
            }
            catch
            {
                // as faked out with delay, we get an execption if cancel during delay,
                // so need to differentiate on the reason we got here

                //if (cancelToken.IsCancellationRequested)
                    //return TestResultCode.Cancelled;

                // otherwise, so other exception....but if did that, then likely a program bug

                return Task.FromResult(TestResultCode.Failed);
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
        public async Task<bool> VerifyVendorWebsiteCredentialsAsync()
        {

            // must set IsCheckingCredentials while operation is in progress
            // must set IsVendorWebsiteLoginValid with result

            // Note that IsVendorWebsiteLoginValid will be null until validated for the first time during
            // a given app runtime session. Then it remains with some value for life of app or until retested.

            // TODO-LIVE: call login check

            // fake
            IsCheckingCredentials = true;
            IsVendorWebsiteLoginValid = null;
            SendVendorChangedNotification();

            await Task.Delay(3000);
            IsCheckingCredentials = false;


            IsVendorWebsiteLoginValid = new Random().Next(3) != 2; // fake result

            if (IsVendorWebsiteLoginValid.GetValueOrDefault())
                ClearWarning();
            else
                SetWarning("Invalid vendor website credentials.");

            SendVendorChangedNotification();

            return IsVendorWebsiteLoginValid.GetValueOrDefault();
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
            // fake

            // if cannot hit SQL (store or scanner) right off, then return DatabaseError.

            // if batch id no longer in scanner SQL, return NotFound

            // if batch id is not for "this vendor", return IncorrectVendor (indicative of a proramming error)

            // if batch status is InProgress, return AccessDenied

            // if batch is not currently pending, return NotPending

            try
            {
                // update scanner SQL to indicate discarded.
                // optional log output might simply report the date or something.
                await Task.Delay(50);

                // notify listeners
                await DiscardedOrDeletedCommitBatch();
                return CommitBatchResult.Successful;

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
            // fake

            // if cannot hit SQL (store or scanner) right off, then return DatabaseError.

            // if batch id no longer in scanner SQL, return NotFound

            // if batch id is not for "this vendor", return IncorrectVendor (indicative of a proramming error)

            // if batch status is InProgress, return AccessDenied


            try
            {
                // physically remove this row from scanner SQL.
                await Task.Delay(50);
                // notify listeners
                await DiscardedOrDeletedCommitBatch();
                return CommitBatchResult.Successful;
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
        /// <param name="estimatedRecordCount"></param>
        /// <param name="ignoreDuplicates">when true, treat duplicate SKU inserts as having inserted successfully (for re-run of cancelled operation).</param>
        /// <param name="cancelToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public Task<CommitBatchResult> CommitBatchAsync(int batchId, int estimatedRecordCount, bool ignoreDuplicates, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null)
        {
            // this is a completely fake simulation, but comments have important guidance for real system

            var tcs = new TaskCompletionSource<CommitBatchResult>();

            Task.Run(async () =>
            {
                try
                {
                    // if cannot hit SQL (store or scanner) right off, then return DatabaseError.

                    // if batch id no longer in scanner SQL, return NotFound

                    // if batch id is not for "this vendor", return IncorrectVendor (indicative of a proramming error)

                    // if batch status is InProgress, return AccessDenied

                    // if batch is not currently pending, return NotPending

                    // generally, a failure to commit any individual item is not a batch failure. A batch that has 1,000 items
                    // and successfully commits 995 and fails to commit 5 is still successful - but it will report to scanner
                    // sql that only 995 items were committed. Further, the log should contain more information about the problems
                    // encountered. It is anticipated that problem items will simply be re-included (when makes sense) in some
                    // future batches. There is no concept of trying to commit the partial left-overs of a batch.

                    // if a batch is cancelled (not sure if UX should allow), our policy will be that it remains Pending. The user
                    // will then decide to start again or discard.

                    // on cancel, some batch types are not idempotent - like new products, new variants. It could be some were
                    // already committed to SQL. What action do we take? If SKU already exists, do we simply assume it should
                    // be counted as already done on some interrupted operation and added to the qty committed reported for the
                    // batch (but do report in the log that it was found).

                    // with the above rules, we therefore are unable to tell the difference between a SKU which was already
                    // committed on a prior/cancelled run of this batch, or if it is truly a duplicate. The determination is
                    // then made based on the ignoreDuplicates flag. If a user cancels an operation, then restarts, they can
                    // pass in true for igoreDuplicates and the system will still include them in the final counts.

                    var rnd = new Random();
                    var fakeTotal = estimatedRecordCount; // number of items in the batch

                    int countTotal = fakeTotal;
                    int countCompleted = 0;
                    int countRemaining = fakeTotal;
                    double percentCompleted = 0;
                    double lastReportedPercentCompleted = -1;

                    Action reportProgress = () =>
                    {
                        percentCompleted = countTotal == 0 ? 0 : ((double)countCompleted * 100.0) / (double)countTotal;

                        if (percentCompleted != lastReportedPercentCompleted && progress != null)
                        {
                            progress.Report(new ActivityProgress(countTotal, countCompleted, percentCompleted));
                            lastReportedPercentCompleted = percentCompleted;
                        }
                    };

                    Action<string> reportMessage = (m) =>
                    {
                        if (progress != null)
                        {
                            var prog = new ActivityProgress(countTotal, countCompleted, percentCompleted)
                            {
                                Message = m,
                            };

                            progress.Report(prog);
                        }
                    };

                    // initial report with correct totals and zero progress
                    reportProgress();

                    //reportMessage("Reading batch details...");
                    //await Task.Delay(700);
                    //reportMessage("Verifying batch...");
                    //await Task.Delay(700);

                    var msg = string.Format("Committing {0:N0} records...", fakeTotal);
                    reportMessage(msg); // this msg will remain on screen while the operation runs

                    while (countRemaining > 0)
                    {
                        if (cancelToken != null && cancelToken.IsCancellationRequested)
                        {
                            tcs.SetResult(CommitBatchResult.Cancelled);
                            return;
                        }

                        var ms = rnd.Next(10, 20);
                        await Task.Delay(ms); // fake delay for SQL write

                        // any individual commit errors should be written to the log events collection
                        // and persisted back to scanner SQL at the end of the operation

                        // report at bottom of the loop
                        countCompleted++;
                        countRemaining--;
                        reportProgress();
                    }

                    await RefreshProductCounts();
                    SendVendorChangedNotification();

                    // update scanner SQL:
                    // log, status, committed qty, date committed

                    tcs.SetResult(CommitBatchResult.Successful);

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
        /// <param name="oldThanDays"></param>
        /// <param name="cancelToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public Task<ActivityResult> DeleteCachedFilesAsync(int olderThanDays, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null)
        {
            // this is a completely fake simulation

            // olderThanDays is not used for simulation, but in real logic, works as a filter.
            // 0 days means to delete everything.

            // will first need to determine how many files exist, establish the baseline progress report, then
            // proceed to delete the files and report progress along the way.

            // the vendor module is reponsible for ensuring that upon return that the metrics
            // are updated.

            if (CachedFilesFolder == null)
                return Task.FromResult(ActivityResult.Success);

            var tcs = new TaskCompletionSource<ActivityResult>();

            Task.Run(async () =>
            {
                try
                {
                    var rnd = new Random();
                    var fakeTotal = rnd.Next(100, 300);

                    int countTotal = fakeTotal;
                    int countCompleted = 0;
                    int countRemaining = fakeTotal;
                    double percentCompleted = 0;
                    double lastReportedPercentCompleted = -1;

                    Action reportProgress = () =>
                    {
                        percentCompleted = countTotal == 0 ? 0 : ((double)countCompleted * 100.0) / (double)countTotal;

                        if (percentCompleted != lastReportedPercentCompleted && progress != null)
                        {
                            progress.Report(new ActivityProgress(countTotal, countCompleted, percentCompleted));
                            lastReportedPercentCompleted = percentCompleted;
                        }
                    };

                    Action<string> reportMessage = (m) =>
                    {
                        if (progress != null)
                        {
                            var prog = new ActivityProgress(countTotal, countCompleted, percentCompleted)
                            {
                                Message = m,
                            };

                            progress.Report(prog);
                        }
                    };

                    // initial report with correct totals and zero progress
                    reportProgress();

                    var msg = string.Format("Deleting {0:N0} files...", fakeTotal);
                    reportMessage(msg); // this msg will remain on screen while the operation runs

                    while (countRemaining > 0)
                    {
                        if (cancelToken != null && cancelToken.IsCancellationRequested)
                        {
                            tcs.SetResult(ActivityResult.Cancelled);
                            return;
                        }

                        await Task.Delay(3);  // simulate delete latency

                        // report at bottom of the loop
                        countCompleted++;
                        countRemaining--;
                        reportProgress();
                    }

                    tcs.SetResult(ActivityResult.Success);

                    // update file metrics here

                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.Message);
                    tcs.SetResult(ActivityResult.Failed);

                    // update file metrics here
                }

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

                ScanningProgressStatusMessage = "Starting...";

                var ctx = new FakeScannerContext()
                {
                    SendLogEvent = async (etype, text) => { await AddScanningLogEvent(text, etype); },
                    SendProgressStatusMessage = (text) => { ScanningProgressStatusMessage = text; },
                    SendProgressSecondaryMessage = (text) => { ScanningProgressSecondaryMessage = text; },
                    SendProgressPercentComplete = (pct) => { ScanningProgressPercentComplete = pct; },
                    SendWebRequest = () => { NotifyHitVendorWebsite(); },
                    SendCreateCheckpoint = () => { CreatedScanningCheckpoint(); },
                    SendSubmitBatch = async (batchType) => { await SubmittedCommitBatch(batchType); },
                    SendReportError = async (text) => 
                        {
                            await AddScanningLogEvent(text, EventType.Error);
                            // do not overwrite any existing warning.
                            if (!HasWarning)
                                SetWarning(text);
                            ScanningErrorCount = ScanningErrorCount.GetValueOrDefault() + 1;
                            if (IsScanning)
                            {
                                ScanningOperationStatus = ScanningStatus.RunningWithErrors;
                                ScannerState = DetermineScannerState();
                            }
                            await SendScanningOperationNotification(ScanningOperationEvent.ReportedError);
                        },
                    SendOperationFailed = async () => { await FailedScanning(); },
                    SendOperationFinished = async () => { await FinishedScanning(); },
                };

                fakeScanner = new FakeScanner(ctx);

                IsCallingScanningOperation = true;

                ScanningOptions = new ObservableCollection<ScanOptions>(options.MaskToList<ScanOptions>());

                // our rule is that starting a scan instantly stales out any uncommitted batches.
                // Some UX methods prompt about this and will cancel the op before it gets here if
                // this is not desired.

                var pendingBatches = await GetPendingCommitBatchSummariesAsync();
                foreach (var batch in pendingBatches)
                    await DiscardBatchAsync(batch.Id);   
                
                // simulate some errors
                var rnd = new Random((Name + DateTime.Now.ToString()).ToSeed()); // need to see per vendor else clock is the same and get same results.
                var failed = rnd.Next(1, 6) == 4;
               
                if (failed)
                {
                    // we do not call FailedScanning() here because that is reserved for an operation
                    // that actually started up and terminated due to errors/exceptions
                    ClearProgressMeter();
                    return ScanningActionResult.Failed;
                }
                else
                {
                    var result = await fakeScanner.StartScanning(ScanningOptions, MaximumScanningErrorCount);
                    if (result == ScanningActionResult.Success)
                        await StartedScanning();
                    return result;
                }
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
                if (!IsScanningCancellable)
                    return ScanningActionResult.Success;

                IsCallingScanningOperation = true;

                // if there is an active operation (running or suspended), then there should be little if 
                // anything that should result in a failure here, since cancel means cancel.

                var result = await fakeScanner.CancelScanning();

                if (result == ScanningActionResult.Success)
                    await CancelledScanning();

                return result;
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
        public async Task<ScanningActionResult> SuspendScanning()
        {
            // see if invalid to have been called at this time
            // technically a programming bug to have been able to call this at the wrong time

            // likely want some better granularity on kinds of failures which are possible in real life

            // do not throw exception up the call stack

            try
            {
                if (IsSuspended)
                    return ScanningActionResult.Success;

                if (!IsScanningSuspendable)
                    return ScanningActionResult.Failed;

                IsCallingScanningOperation = true;

                var result = await fakeScanner.SuspendScanning();
                if (result == ScanningActionResult.Success)
                    await SuspendedScanning();
                return result;
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

                IsCallingScanningOperation = true;
                ScanningProgressStatusMessage = "Resuming...";
                
                var result = await fakeScanner.ResumeScanning();
                if (result ==  ScanningActionResult.Success)
                    await ResumedScanning();

                return result;
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

                // TODO: replace with true logic
                await Task.Delay(1);
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

            // StaticFilesFolder - from core vendor module
            // CachedFilesFolder - from core vendor module

            // These defaults can be different per vendor, comes from core vendor module
            // DefaultDelayMSBetweenVendorWebsiteRequests
            // DefaultMaximumScanningErrorCount

            // then set current values to be same as defaults for the vendor UNLESS rehydrating from a suspended operation
            // in which case the persisted values would be used for the current settings
            DelayMSBetweenVendorWebsiteRequests = DefaultDelayMSBetweenVendorWebsiteRequests;
            MaximumScanningErrorCount = DefaultMaximumScanningErrorCount;

            #region Fake Static Values (remove when addressed the real way)
            // ****** start fake static settings ********
            // these would generally come directly from Shane's core vendor module
            StaticFilesFolder = @"c:\temp";
            CachedFilesFolder = @"c:\temp";
            MaximumScanningErrorCount = 5;
            DelayMSBetweenVendorWebsiteRequests = 250;
            // ****** end fake static settings ******** 
            #endregion

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


        public Vendor Vendor
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public int StaticFileVersionTxt
        {
            get { throw new NotImplementedException(); }
        }
    }
}
