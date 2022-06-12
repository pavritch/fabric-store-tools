using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;

namespace ProductScanner.App
{
    public class DesignVendorModel : ObservableObject, IVendorModel
    {
        public DesignVendorModel()
        {
            VendorId = 1;
            Name = "Kravet";
            ScannerState = ScannerState.Scanning;
            IsFullyImplemented = true;
            Status = VendorStatus.Manual;
            HasWarning = true;
            WarningText = "This is my very clear warning message.";

            IsScanning = false;
            IsPerformingTests = false;
            IsCheckingStock = false;
            IsCheckingCredentials = false;
            HasCheckpoint = true;
            HasPendingCommitBatches = true;
            PendingCommitBatchCount = 4;
            LastCheckpointDate = DateTime.Now;
            OldestCommitBatchDate = DateTime.Now;
            ProductCount = 11213;
            ProductVariantCount = 24891;
            DiscontinuedProductCount = 1345;
            CachedFilesFolder = @"c:\temp";
            StaticFilesFolder = @"c:\temp";

            DefaultMaximumScanningErrorCount= 10;
            MaximumScanningErrorCount = 12;

            DefaultDelayMSBetweenVendorWebsiteRequests = 700;
            DelayMSBetweenVendorWebsiteRequests = 1200;

            IsVendorWebsiteLoginValid = true;

            MakeFakeRecentCommits();

        }



        private void MakeFakeRecentCommits()
        {
            var list = new List<CommitBatchSummary>()
            {
                new CommitBatchSummary
                {
                    Id = 100,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-1),
                    BatchType = CommitBatchType.Discontinued,
                    QtySubmitted = 1020,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 101,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-2),
                    BatchType = CommitBatchType.NewProducts,
                    QtySubmitted = 10234,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 102,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-3),
                    BatchType = CommitBatchType.InStock,
                    QtySubmitted = 233,
                    SessionStatus= CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 103,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-4),
                    BatchType = CommitBatchType.NewProducts,
                    QtySubmitted = 11456,
                    SessionStatus= CommitBatchStatus.Discarded,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },

                new CommitBatchSummary
                {
                    Id = 104,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-5),
                    BatchType = CommitBatchType.NewVariants,
                    QtySubmitted = 909,
                    SessionStatus= CommitBatchStatus.Committed,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },

            };

            RecentCommits = new ObservableCollection<CommitBatchSummary>(list);
        }

        /// <summary>
        /// Fetch commit batches in descending order.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="skip">Optional number of records to skip.</param>
        /// <param name="take">Optional number of records to take.</param>
        /// <returns></returns>
        public Task<List<CommitBatchSummary>> GetCommitBatchesAsync(int? skip = null, int? take = null)
        {
            return Task.FromResult(RecentCommits.ToList());
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Discard the specified batch. Just marked as discarded in scanner SQL.
        /// </summary>
        /// <param name="batchId">Batch must be associated with this vendor.</param>
        /// <returns></returns>
        public Task<CommitBatchResult> DiscardBatchAsync(int batchId)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Full delete of the indicated batch. Usually reserved for removing already-discarded batches to clean things up.
        /// </summary>
        /// <param name="batchId">Batch must be associated with this vendor.</param>
        /// <returns></returns>
        public Task<CommitBatchResult> DeleteBatchAsync(int batchId)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Delete downloaded cached files for vendor which are older than N days - 0 removes all.
        /// </summary>
        /// <param name="oldThanDays"></param>
        /// <param name="cancelToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public Task<ActivityResult> DeleteCachedFilesAsync(int olderThanDays, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve the details for a single batch. Can be any status.
        /// </summary>
        /// <param name="batchID"></param>
        /// <returns>Null if batchID does not exist.</returns>
        public Task<CommitBatchDetails> GetCommitBatchAsync(int batchID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return editable properties for the specified vendor.
        /// </summary>
        /// <returns></returns>
        public Task<VendorProperties> GetVendorPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Persist editable vendor properties back to SQL.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Task<bool> SaveVendorPropertiesAsync(VendorProperties properties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the list of tests which can be performed for this vendor.
        /// </summary>
        /// <returns></returns>
        public List<TestDescriptor> GetTests()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Indicate the beginning of a series of tests. The UX informs the model.
        /// </summary>
        /// <remarks>
        /// The tests are then run individually using RunTest(), then finally calling EndTest().
        /// </remarks>
        /// <returns>Return false if testing is blocked/restricted and not allowed.</returns>
        public Task<bool> BeginTestingAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The UX informs the model that it is done running tests.
        /// </summary>
        public void EndTesting()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Run a single test.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public Task<TestResultCode> RunTestAsync(IVendorTest test)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// The folder location, if any, where this vendor's static files are stored.
        /// </summary>
        /// <remarks>
        /// Leave null if no folder exists, but include if the folder exists but happens to be empty.
        /// </remarks>
        public string StaticFilesFolder { get; set; }

        /// <summary>
        /// The folder location, if any, where this vendor's dynamic/cached files are stored.
        /// </summary>
        /// <remarks>
        /// Leave null if no folder exists, but include if the folder exists but happens to be empty.
        /// </remarks>
        public string CachedFilesFolder { get; set; }


        /// <summary>
        /// The default number of MS delay to inject between scanner requests hitting vendor's website.
        /// </summary>
        public int DefaultDelayMSBetweenVendorWebsiteRequests { get; set; }

        /// <summary>
        /// The current setting for number of MS delay to inject between scanner requests hitting vendor's website.
        /// </summary>
        /// <remarks>
        /// Can be adjusted while scanning - passed through to running operation.
        /// Should be persisted with checkpoints; read back in and repopulated.
        /// </remarks>
        public int DelayMSBetweenVendorWebsiteRequests { get; set; }

        /// <summary>
        /// The default number of hard (but non-fatal) errors at which scanning terminates with failure.
        /// </summary>
        /// <remarks>
        /// Fatal errors still require immediate termination.
        /// </remarks>
        public int DefaultMaximumScanningErrorCount { get; set; }

        /// <summary>
        /// The current setting for number of hard (but non-fatal) errors at which scanning terminates with failure.
        /// </summary>
        /// <remarks>
        /// Fatal errors still require immediate termination.
        /// Can be adjusted while scanning - passed through to running operation.
        /// Should be persisted with checkpoints; read back in and repopulated.
        /// </remarks>
        public int MaximumScanningErrorCount { get; set; }



        public void ClearWarning()
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyVendorWebsiteCredentialsAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// True to indicate there is a cached set of metrics in memory, such that
        /// GetStaticFileStorageMetricsAsync(false) will return immediately with a valid result.
        /// </summary>
        public bool HasStaticFileStorageMetrics { get; set; }

        /// <summary>
        /// True to indicate there is a cached set of metrics in memory, such that
        /// GetCachedFileStorageMetricsAsync(false) will return immediately with a valid result.
        /// </summary>
        public bool HasCachedFileStorageMetrics { get; set; }


        /// <summary>
        /// Get the storage metrics for the cached files folder tree for this vendor.
        /// </summary>
        /// <returns></returns>
        public Task<FileStorageMetrics> GetCachedFileStorageMetricsAsync(bool recalculate)
        {
            var metrics = new FileStorageMetrics
            {
                TotalFiles = 1200,
                TotalSize = 20002 * 40912, // just anything...
                Oldest = DateTime.Now.AddDays(-10),
                Newest = DateTime.Now.AddDays(-2),
            };

            return Task.FromResult(metrics);
            
        }


        /// <summary>
        /// Get the storage metrics for teh static files folder tree for this vendor.
        /// </summary>
        /// <returns></returns>
        public Task<FileStorageMetrics> GetStaticFileStorageMetricsAsync(bool recalculate)
        {
            var metrics = new FileStorageMetrics
            {
                TotalFiles = 15,
                TotalSize = 1024 * 8,
                Oldest = DateTime.Now.AddDays(-50),
                Newest = DateTime.Now.AddDays(-48),
            };

            return Task.FromResult(metrics);
        }

        /// <summary>
        /// Is vendor in a state which would allow scanning to be started.
        /// </summary>
        public bool IsScanningStartable { get; set; }

        /// <summary>
        /// Is vendor in a state which would allow an operation to be suspended
        /// </summary>
        public bool IsScanningSuspendable { get; set; }

        /// <summary>
        /// Is vendor in a state which would allow and operation to be resumed.
        /// </summary>
        public bool IsScanningResumable { get; set; }

        /// <summary>
        /// Is vendor in a state which would allow an operation to be cancelled.
        /// </summary>
        public bool IsScanningCancellable { get; set; }

        /// <summary>
        /// Are we in a state where clearing the scanning log is allowed.
        /// </summary>
        public bool IsScanningLogClearable { get; set; }

        /// <summary>
        /// Are we in a state where we can delete all the cached files.
        /// </summary>
        public bool IsFileCacheClearable { get; set; }

        /// <summary>
        /// Are we in a state which would allow vendor testing to be performed.
        /// </summary>
        public bool IsTestable { get; set; }


        private IStoreModel _parentStore = null;

        public IStoreModel ParentStore
        {
            get
            {
                return _parentStore;
            }
            set
            {
                Set(() => ParentStore, ref _parentStore, value);
            }
        }

        private int _vendorID = 0;
        public int VendorId
        {
            get
            {
                return _vendorID;
            }
            set
            {
                Set(() => VendorId, ref _vendorID, value);
            }
        }

        private string _name = null;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        private ScannerState _scannerState = ScannerState.Idle;
        public ScannerState ScannerState
        {
            get
            {
                return _scannerState;
            }
            set
            {
                Set(() => ScannerState, ref _scannerState, value);
            }
        }

        private bool _isFullyImplemented = true;
        public bool IsFullyImplemented
        {
            get
            {
                return _isFullyImplemented;
            }
            set
            {
                Set(() => IsFullyImplemented, ref _isFullyImplemented, value);
            }
        }

        private VendorStatus _status = VendorStatus.Manual;

        public VendorStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                Set(() => Status, ref _status, value);
            }
        }


        private bool _hasWarning = false;
        public bool HasWarning
        {
            get
            {
                return _hasWarning;
            }
            set
            {
                Set(() => HasWarning, ref _hasWarning, value);
            }
        }

        private string _warningText = null;
        public string WarningText
        {
            get
            {
                return _warningText;
            }
            set
            {
                Set(() => WarningText, ref _warningText, value);
            }
        }


        private bool _isSuspended = false;
        public bool IsSuspended
        {
            get
            {
                return _isSuspended;
            }
            set
            {
                Set(() => IsSuspended, ref _isSuspended, value);
            }
        }

        /// <summary>
        /// For safety - don't allow much if anything unless we've been initialized.
        /// </summary>
        private bool _isInitialized = false;

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
            private set
            {
                Set(() => IsInitialized, ref _isInitialized, value);
            }
        }


        private string _vendorWebsiteUsername = null;
        public string VendorWebsiteUsername
        {
            get
            {
                return _vendorWebsiteUsername;
            }

            set
            {
                if (_vendorWebsiteUsername == value)
                {
                    return;
                }

                _vendorWebsiteUsername = value;
                RaisePropertyChanged(() => VendorWebsiteUsername);
            }
        }

        private string _vendorWebsitePassword = null;
        public string VendorWebsitePassword
        {
            get
            {
                return _vendorWebsitePassword;
            }

            set
            {
                if (_vendorWebsitePassword == value)
                {
                    return;
                }

                _vendorWebsitePassword = value;
                RaisePropertyChanged(() => VendorWebsitePassword);
            }
        }

        private string _vendorWebsiteUrl = null;
        public string VendorWebsiteUrl
        {
            get
            {
                return _vendorWebsiteUrl;
            }

            set
            {
                if (_vendorWebsiteUrl == value)
                {
                    return;
                }

                _vendorWebsiteUrl = value;
                RaisePropertyChanged(() => VendorWebsiteUrl);
            }
        }

        public bool? IsVendorWebsiteLoginValid { get; set; }

        /// <summary>
        /// Populate the model with stores and vendors.
        /// </summary>
        /// <remarks>
        /// Performed external to constructor since could be a long-running action (a second or two).
        /// </remarks>
        /// <returns></returns>
        public Task<bool> InitializeAsync()
        {
            IsInitialized = true;
            // nothing needed for design time.
            return Task.FromResult<bool>(IsInitialized);
        }

        /// <summary>
        /// Number of products in stock. Default variant.
        /// </summary>
        public int InStockProductCount { get; set; }

        /// <summary>
        /// Number of products out of stock. Default variant;
        /// </summary>
        public int OutOfStockProductCount { get; set; }

        /// <summary>
        /// Number of products in stock.
        /// </summary>
        public int InStockProductVariantCount { get; set; }

        /// <summary>
        /// Number of products out of stock.
        /// </summary>
        public int OutOfStockProductVariantCount { get; set; }



        public bool IsScanning { get; set; }
        public bool IsPerformingTests { get; set; }
        public bool IsCheckingStock { get; set; }
        public bool IsCheckingCredentials { get; set; }
        public bool HasCheckpoint { get; set; }
        public bool HasPendingCommitBatches { get; set; }

        public int PendingCommitBatchCount { get; set; }
        public DateTime? LastCheckpointDate { get; set; }
        public DateTime? LastCommitDate { get; set; }
        public DateTime? OldestCommitBatchDate { get; set; }

        public int ProductCount { get; set; }
        public int ProductVariantCount { get; set; }
        public int DiscontinuedProductCount { get; set; }


        private int _clearanceProductCount = 0;

        public int ClearanceProductCount
        {
            get
            {
                return _clearanceProductCount;
            }
            set
            {
                Set(() => ClearanceProductCount, ref _clearanceProductCount, value);
            }
        }

        private ObservableCollection<CommitBatchSummary> _recentCommits = null;

        /// <summary>
        /// Sets and gets the RecentCommits property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<CommitBatchSummary> RecentCommits
        {
            get
            {
                return _recentCommits;
            }
            set
            {
                Set(() => RecentCommits, ref _recentCommits, value);
            }
        }

        /// <summary>
        /// Fetch summaries for all pending batches for this vendor.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <returns></returns>
        public Task<List<CommitBatchSummary>> GetPendingCommitBatchSummariesAsync()
        {
            throw new NotImplementedException();
        }


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
        public Task<ScanningActionResult> StartScanning(ScanOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancel the running or suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        public Task<ScanningActionResult> CancelScanning()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resume the presently-suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        public Task<ScanningActionResult> ResumeScanning()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears out any residual state from a previously finished/terminated operation. Optional.
        /// </summary>
        /// <remarks>
        /// Should only be called when scanning is idle, if desired. Clears out things like start time, 
        /// duration, etc. Goes back to the most idle, never yet run anything state. 
        /// </remarks>
        public void ClearScanningState()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// The status of the now running/suspended or just completed/failed operatoin.
        /// </summary>
        /// <remarks>
        /// Remains in this state until app exists, new operation starts or cleared by user.
        /// </remarks>
        public ScanningStatus ScanningOperationStatus { get; private set; }


        /// <summary>
        /// Options used to initiate the recent scanning operation.
        /// </summary>
        /// <remarks>
        /// Will be populated internally when a new operation is started. Intended to allow external code
        /// to see what options were used to start a scan. Also to be able to repopulate the read-only checkbox list
        /// on the vendor scan page to show what was used for a then-suspended operation.
        /// Never null. Should be empty collection when nothing.
        /// </remarks>
        public ObservableCollection<ScanOptions> ScanningOptions { get; private set; }

        /// <summary>
        /// Event log associated with the current or suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Must be repopulated at startup from checkpoint data if the operation was suspended.
        /// </remarks>
        public ObservableCollection<EventLogRecord> ScanningLogEvents { get; private set; }

        /// <summary>
        /// The number of web requests per minute by the scanner. Zero when idle.
        /// </summary>
        /// <remarks>
        /// This becomes the dial value on the vendor scan page.
        /// </remarks>
        public double ScanningRequestsPerMinute { get; private set; }

        /// <summary>
        /// Indicates the value to show on the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation. -1 for indeterminate, else 0 to 100.
        /// </remarks>
        public double? ScanningProgressPercentComplete { get; private set; }

        /// <summary>
        /// The main progress status message - displayed above the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation or nothing to report.
        /// </remarks>
        public string ScanningProgressStatusMessage { get; private set; }

        /// <summary>
        /// The secondary progress message, displayed below the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation or nothing to report.
        /// </remarks>
        public string ScanningProgressSecondaryMessage { get; private set; }

        /// <summary>
        /// The time when the current operation was started, else null.
        /// </summary>
        /// <remarks>
        /// Must persist through for suspended operations so can rehydrate on next app start.
        /// Null when nothing to show. Remains with value after an operation completes, until either
        /// app terminated or another operation starts up.
        /// </remarks>
        public DateTime? ScanningStartTime { get; private set; }

        /// <summary>
        /// The time the most-recent operation ended.
        /// </summary>
        /// <remarks>
        /// Null when nothing to show. Remains with value after an operation completes, until either
        /// app terminated or another operation starts up.
        /// </remarks>
        public DateTime? ScanningEndTime { get; private set; }

        /// <summary>
        /// The accumulated actual running duration of the current operation. 
        /// </summary>
        /// <remarks>
        /// Since need to account for suspended states, cannot simple subtract Now from start time.
        /// Should persist with checkpoint, and pick up when scanning resumes.
        /// </remarks>
        public TimeSpan? ScanningDuration { get; private set; }

        /// <summary>
        /// The number of errors accumulated for the current/suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Null when no associated operation.
        /// </remarks>
        public int? ScanningErrorCount { get; private set; }

        /// <summary>
        /// Will be true for the fraction of a second or so needed while in the middle of calling start/cancel/suspend/resume.
        /// </summary>
        /// <remarks>
        /// Intended mostly for buttom commands so so they know to disable buttons when an operation is being attempted.
        /// </remarks>
        public bool IsCallingScanningOperation { get; private set; }

        #endregion


        public Core.Vendor Vendor
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

        public bool IsVariantCentricInventory
        {
            get
            {
                var variantCentricStores = new StoreType[] { StoreType.InsideRugs };
                return variantCentricStores.Contains(ParentStore.Key);
            }
        }



        public int StaticFileVersionTxt
        {
            get { throw new NotImplementedException(); }
        }
    }
}
