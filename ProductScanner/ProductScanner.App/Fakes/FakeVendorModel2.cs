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
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using Utilities;

namespace ProductScanner.App
{
    // SHANE: There is nearly nothing in this file you should or need to change. In fact,
    // if something appears to need a change, I'd recommend hitting me up on it because
    // most of what's here is very deliberate - in a very specific order.

    // Search this file for SHANE: to see what you may need to change.

    // Most of what's here could actually be turned into a base class if desired.
    // -- will need to mark a bunch as virtual, but otherwise, almost a copy and paste.

    /// <summary>
    /// A continuation (Partial) file for VendorModel which contains core logic which would
    /// not tend to change based on integration Shane's core vendor logic.
    /// </summary>
    public partial class FakeVendorModel : ObservableObject
    {
        #region Local Data
        /// <summary>
        /// Indicates that the scanning log has some real data that could be cleared (if conditions allow).
        /// </summary>
        /// <remarks>
        /// Intended mostly to help qualify commands in UX.
        /// </remarks>
        protected bool scanningLogHasData = false;

        /// <summary>
        /// When true, a scanning operation is wanting/needing to process 1s timer ticks.
        /// </summary>
        protected bool captureScanningIntervalTimer = false;

        /// <summary>
        /// Cached metrics for static files. Can be null when never computed or forced stale.
        /// </summary>
        protected FileStorageMetrics staticFilesMetrics = null;

        /// <summary>
        /// Cached metrics for cached files. Can be null when never computed or forced stale.
        /// </summary>
        protected FileStorageMetrics cachedFilesMetrics = null;

        /// <summary>
        /// The baseline time for this current scanning segment, to be used to compute a true duration.
        /// </summary>
        /// <remarks>
        /// Because over long periods of time, adding 1000ms on ticks might not be accurate enough.
        /// </remarks>
        protected DateTime scanningTimerBase;

        #endregion

        #region Refresh Helpers (async)


        protected async Task RefreshPendingBatchCount()
        {
            PendingCommitBatchCount = await GetPendingCommitBatchCount();
            HasPendingCommitBatches = PendingCommitBatchCount > 0;
        }

        protected async Task RefreshCheckpointDetails()
        {
            var cp = await GetCheckpointDetails();

            if (cp != null)
            {
                // there is a checkpoint
                IsSuspended = true;
                LastCheckpointDate = cp.Created;
                ScanningStartTime = cp.StartTime;
                ScanningDuration = cp.Duration;
                ScanningLogEvents = new ObservableCollection<EventLogRecord>(cp.LogEvents);
                ScanningOptions = new ObservableCollection<ScanOptions>(cp.Options);
                ScanningErrorCount = cp.ErrorCount;
                DelayMSBetweenVendorWebsiteRequests = cp.DelayMSBetweenVendorWebsiteRequests;
                MaximumScanningErrorCount = cp.MaximumScanningErrorCount;
                scanningLogHasData = true;
            }
            else
            {
                // no checkpoint
                LastCheckpointDate = null;
                this.RaisePropertyChanged(() => LastCheckpointDate);
                IsSuspended = false;
                ScanningStartTime = null;
                ScanningDuration = null;
                ScanningLogEvents.Clear();
                ScanningOptions.Clear();
                ScanningErrorCount = null;
                await AddScanningLogEvent(Name);
                await AddScanningLogEvent("Ready.");
                scanningLogHasData = false; // because two above lines set it true
            }
        }

        protected async Task RefreshVendorProperties()
        {
            // Status
            // VendorWebsiteUrl
            // VendorWebsiteUsername
            // VendorWebsitePassword

            var vp = await GetVendorPropertiesAsync();

            //Debug.Assert(vp != null);

            Status = vp.Status;
            VendorWebsiteUsername = vp.Username;
            VendorWebsitePassword = vp.Password;
        }

        protected async Task RefreshProductCounts()
        {
            // ProductCount
            // ProductVariantCount
            // DiscontinuedProductCount
            // ClearanceProductCount
            // InStockProductCount
            // OutOfStockProductCount
            // InStockProductVariantCount
            // OutOfStockProductVariantCount

            var metrics = await GetProductCountMetrics();

            ProductCount = metrics.ProductCount;
            ProductVariantCount = metrics.ProductVariantCount;
            DiscontinuedProductCount = metrics.DiscontinuedProductCount;
            ClearanceProductCount = metrics.ClearanceProductCount;
            InStockProductCount = metrics.InStockProductCount;
            OutOfStockProductCount = metrics.OutOfStockProductCount;
            InStockProductVariantCount = metrics.InStockProductVariantCount;
            OutOfStockProductVariantCount = metrics.OutOfStockProductVariantCount;
        }
        #endregion

        #region Local Methods

        protected async void ReportScanningError()
        {
            await SendScanningOperationNotification(ScanningOperationEvent.ReportedError);
        }

        /// <summary>
        /// General broadcast to give subscribers notice that something material has changed for the vendor.
        /// </summary>
        /// <remarks>
        /// Purposely intended to be broad in nature, in that does not say exactly what has changed. The subscriber
        /// will take a quick look at just the fields they are interested in - not intended to have Task actions called, etc.
        /// </remarks>
        protected void SendVendorChangedNotification()
        {
            Messenger.Default.Send(new VendorChangedNotification(this));
        }

        /// <summary>
        /// Set the warning indication to some string, else null.
        /// </summary>
        /// <param name="msg"></param>
        protected void SetWarning(string msg)
        {
            WarningText = msg;
            HasWarning = !string.IsNullOrWhiteSpace(WarningText);
        }

        protected void RecalculateCachedFilesMetrics(bool announceWhenDone = true)
        {
            Task.Run(async () =>
            {
                await GetCachedFileStorageMetricsAsync(true);
                if (announceWhenDone)
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        Messenger.Default.Send(new ScanningOperationNotification(this, ScanningOperationEvent.CachedFilesChanged));
                    });
                }
            });
        }

        protected void RecalculateStaticFilesMetrics(bool announceWhenDone = true)
        {
            Task.Run(async () =>
            {
                await GetStaticFileStorageMetricsAsync(true);
                if (announceWhenDone)
                {
                    await DispatcherHelper.RunAsync(() =>
                    {
                        Messenger.Default.Send(new ScanningOperationNotification(this, ScanningOperationEvent.StaticFilesChanged));
                    });
                }
            });
        }


        /// <summary>
        /// Looks at various properties and sets scanner state appropriately.
        /// </summary>
        /// <remarks>
        /// Requires several dependent properties be populated/rehydrated in advance.
        /// </remarks>
        protected ScannerState DetermineScannerState()
        {
            // the order of tests here is important to ensure
            // the correct outputs

            ScannerState updatedScannerState = ScannerState.Idle;

            if (IsScanning)
            {
                updatedScannerState = ScannerState.Scanning;
            }
            else if (IsSuspended)
            {
                updatedScannerState = ScannerState.Suspended;
            }
            else if (HasPendingCommitBatches)
            {
                // cannot be committable if scanning/suspended, because both of those
                // states imply that there are not pending batches due to our rule that
                // says any start of an operation discards pending batches (if any).
                updatedScannerState = ScannerState.Committable;
            }
            else if (Status == VendorStatus.Disabled)
            {
                // if none of the above important actions are indicated, then it comes
                // down to showing as idle or disabled.
                updatedScannerState = ScannerState.Disabled;
            }
            else
            {
                // when none of the above, then we're idle and ready to begin a new operation
                updatedScannerState = ScannerState.Idle;
            }

            return updatedScannerState;
        }

        /// <summary>
        /// Determine the operation status for app start.
        /// </summary>
        /// <returns></returns>
        protected ScanningStatus DetermineInitialScanningOperationStatus()
        {
            // on app start, can only be either suspended, idle or disabled - since all other states require some
            // operation activity to have taken place during the current app session

            ScanningStatus initialStatus;

            if (IsSuspended)
            {
                initialStatus = ScanningStatus.Suspended;
            }
            else if (Status == VendorStatus.Disabled)
            {
                initialStatus = ScanningStatus.Disabled;
            }
            else
                initialStatus = ScanningStatus.Idle;

            return initialStatus;
        }


        /// <summary>
        /// Reset vendor properties to default state. Called from ctor.
        /// </summary>
        /// <remarks>
        /// For a not fully implemented vendor, these values will be left as set for the life of app.
        /// </remarks>
        protected void ResetPropertiesToDefaultValues()
        {
            ScannerState = ScannerState.Idle;
            ScanningOperationStatus = ScanningStatus.Idle;
            Status = VendorStatus.Manual;

            IsScanning = false;
            IsPerformingTests = false;
            IsCheckingStock = false;
            IsCheckingCredentials = false;
            IsSuspended = false;
            LastCheckpointDate = null;

            HasPendingCommitBatches = false;
            PendingCommitBatchCount = 0;

            ProductCount = 0;
            ProductVariantCount = 0;
            DiscontinuedProductCount = 0;
            InStockProductCount = 0;
            OutOfStockProductCount = 0;
            InStockProductVariantCount = 0;
            OutOfStockProductVariantCount = 0;
            ClearanceProductCount = 0;
        }

        /// <summary>
        /// Recent commits is presently fixed as the 6 most recent in descending order.
        /// </summary>
        /// <remarks>
        /// This database call will natively return in the correct descending order.
        /// </remarks>
        protected async Task RefreshRecentCommits()
        {
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            var list = await dbScanner.GetCommitBatchSummariesAsync(VendorId, 0, 6);
            RecentCommits = new ObservableCollection<CommitBatchSummary>(list);
        }

        protected async Task<CheckpointDetails> GetCheckpointDetails()
        {
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            var cp = await dbScanner.GetCheckpointDetailsAsync(null);
            return cp;
        }

        protected async Task<int> GetPendingCommitBatchCount()
        {
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            return await dbScanner.GetPendingCommitBatchCountAsync(VendorId);
        }


        protected async Task<ProductCountMetrics> GetProductCountMetrics()
        {
            var dbStore = App.GetInstance<IStoreDatabaseConnector>();
            var metrics = await dbStore.GetProductCountMetricsAsync(ParentStore.Key, VendorId);
            return metrics;
        }

        /// <summary>
        /// Fetch summaries for all pending batches for this vendor.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <returns></returns>
        public async Task<List<CommitBatchSummary>> GetPendingCommitBatchSummariesAsync()
        {
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            return await dbScanner.GetPendingCommitBatchSummariesAsync(VendorId);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Indicate the beginning of a series of tests. The UX informs the model.
        /// </summary>
        /// <remarks>
        /// The tests are then run individually using RunTest(), then finally calling EndTest().
        /// </remarks>
        /// <returns>Return false if testing is blocked/restricted and not allowed.</returns>
        public Task<bool> BeginTestingAsync()
        {
            if (IsScanning)
                return Task.FromResult(false);

            IsPerformingTests = true;
            return Task.FromResult(true);
        }

        /// <summary>
        /// The UX informs the model that it is done running tests.
        /// </summary>
        public void EndTesting()
        {
            IsPerformingTests = false;
        }


        public void ClearWarning()
        {
            WarningText = null;
            HasWarning = false;
        }

        /// <summary>
        /// Return editable properties for the specified vendor.
        /// </summary>
        /// <returns></returns>
        public async Task<VendorProperties> GetVendorPropertiesAsync()
        {
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            var vendorData = await dbScanner.GetVendorDataAsync(VendorId);
            return new VendorProperties()
            {

            };
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
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            return dbScanner.GetCommitBatchSummariesAsync(VendorId, skip, take);
        }

        /// <summary>
        /// Retrieve the details for a single batch. Can be any status.
        /// </summary>
        /// <param name="batchID"></param>
        /// <returns>Null if batchID does not exist.</returns>
        public Task<CommitBatchDetails> GetCommitBatchAsync(int batchID)
        {
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            return dbScanner.GetCommitBatchAsync(batchID);
        }



        /// <summary>
        /// Get the storage metrics for the cached files folder tree for this vendor.
        /// </summary>
        /// <returns>Null if no corresponding folder for this vendor.</returns>
        public async Task<FileStorageMetrics> GetCachedFileStorageMetricsAsync(bool recalculate)
        {
            if (CachedFilesFolder == null)
                return null;

            lock (this)
            {

                if (HasCachedFileStorageMetrics && !recalculate)
                    return cachedFilesMetrics;
            }

            var fileMgr = App.GetInstance<IFileManager>();
            var metrics = await fileMgr.GetFileStoreMetricsAsync(CachedFilesFolder);

            lock (this)
            {
                cachedFilesMetrics = metrics;
            }

            return metrics;
        }

        /// <summary>
        /// Get the storage metrics for the static files folder tree for this vendor.
        /// </summary>
        /// <returns>Null if no corresponding folder for this vendor.</returns>
        public async Task<FileStorageMetrics> GetStaticFileStorageMetricsAsync(bool recalculate)
        {
            if (StaticFilesFolder == null)
                return null;

            lock (this)
            {
                if (HasStaticFileStorageMetrics && !recalculate)
                    return staticFilesMetrics;
            }

            var fileMgr = App.GetInstance<IFileManager>();
            var metrics = await fileMgr.GetFileStoreMetricsAsync(StaticFilesFolder);

            lock (this)
            {
                staticFilesMetrics = metrics;
            }

            return metrics;
        }


        /// <summary>
        /// Persist editable vendor properties back to SQL.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public async Task<bool> SaveVendorPropertiesAsync(VendorProperties properties)
        {
            var dbScanner = App.GetInstance<IScannerDatabaseConnector>();
            var result = await dbScanner.SaveVendorPropertiesAsync(VendorId, properties);

            // if all is good, must update internal state to match

            if (result)
            {
                VendorWebsiteUsername = properties.Username;
                VendorWebsitePassword = properties.Password;
                Status = properties.Status;
                ScannerState = DetermineScannerState();

                if (!IsScanning && !IsSuspended)
                {
                    if (Status == VendorStatus.Disabled)
                        ScanningOperationStatus = ScanningStatus.Disabled;
                    else
                        ScanningOperationStatus = ScanningStatus.Idle;
                }
                SendVendorChangedNotification();
            }

            return result;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Unique manufacturer ID assigned to this vendor.
        /// </summary>
        /// <remarks>
        /// Immutable.
        /// </remarks>
        /// <value></value>
        public int VendorId { get; private set; }

        private string _name = null;
        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                Set(() => Name, ref _name, value);
            }
        }

        private IStoreModel _parentStore = null;

        public IStoreModel ParentStore
        {
            get
            {
                return _parentStore;
            }
            private set
            {
                Set(() => ParentStore, ref _parentStore, value);
            }
        }


        /// <summary>
        /// Indicates if this vendor is implemented to the point where the vendor model is operational. Required.
        /// </summary>
        /// <remarks>
        /// By operational, means won't blow up. Does not mean all vendors work, etc. If not operational,
        /// the UI will not do much of anything other than show the name of the store (but as disabled).
        /// </remarks>
        private bool _isFullyImplemented = false;

        public bool IsFullyImplemented
        {
            get
            {
                return _isFullyImplemented;
            }
            private set
            {
                Set(() => IsFullyImplemented, ref _isFullyImplemented, value);
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


        /// <summary>
        /// Indicates if the vendor is currently running a scan, is suspended, has batches to commit, etc.
        /// </summary>
        /// <value></value
        private ScannerState _scannerState = ScannerState.Idle;
        public ScannerState ScannerState
        {
            get
            {
                return _scannerState;
            }
            private set
            {
                Set(() => ScannerState, ref _scannerState, value);
            }
        }


        /// <summary>
        /// Indicates how this vendor participates in group scanning operations.
        /// </summary>
        private VendorStatus _status = VendorStatus.Manual;
        public VendorStatus Status
        {
            get
            {
                return _status;
            }
            private set
            {
                Set(() => Status, ref _status, value);
            }
        }

        /// <summary>
        /// True when there is a warning or error associated with this vendor.
        /// </summary>
        private bool _hasWarning = false;
        public bool HasWarning
        {
            get
            {
                return _hasWarning;
            }
            private set
            {
                Set(() => HasWarning, ref _hasWarning, value);
            }
        }


        /// <summary>
        /// True when there is a warning or error associated with this vendor.
        /// </summary>
        private string _warningText = null;
        public string WarningText
        {
            get
            {
                return _warningText;
            }
            private set
            {
                Set(() => WarningText, ref _warningText, value);
            }
        }


        private bool _isScanning = false;
        public bool IsScanning
        {
            get
            {
                return _isScanning;
            }
            private set
            {
                Set(() => IsScanning, ref _isScanning, value);
            }
        }

        private bool _isSuspended = false;
        public bool IsSuspended
        {
            get
            {
                return _isSuspended;
            }
            private set
            {
                Set(() => IsSuspended, ref _isSuspended, value);
            }
        }

        private bool _isPerformingTests = false;
        public bool IsPerformingTests
        {
            get
            {
                return _isPerformingTests;
            }
            private set
            {
                Set(() => IsPerformingTests, ref _isPerformingTests, value);
            }
        }

        private bool _isCheckingStock = false;
        public bool IsCheckingStock
        {
            get
            {
                return _isCheckingStock;
            }
            private set
            {
                Set(() => IsCheckingStock, ref _isCheckingStock, value);
            }
        }

        private bool _isCheckingCredentials = false;
        public bool IsCheckingCredentials
        {
            get
            {
                return _isCheckingCredentials;
            }
            private set
            {
                Set(() => IsCheckingCredentials, ref _isCheckingCredentials, value);
            }
        }

        private bool _hasCheckpoint = false;
        public bool HasCheckpoint
        {
            get
            {
                return _hasCheckpoint;
            }
            private set
            {
                Set(() => HasCheckpoint, ref _hasCheckpoint, value);
            }
        }

        private DateTime? _lastCheckpointDate = null;
        public DateTime? LastCheckpointDate
        {
            get
            {
                return _lastCheckpointDate;
            }
            private set
            {
                Set(() => LastCheckpointDate, ref _lastCheckpointDate, value);
                HasCheckpoint = value.HasValue;
            }
        }

        public DateTime? LastCommitDate
        {
            get
            {
                if (RecentCommits == null || RecentCommits.Count() == 0)
                    return null;

                return RecentCommits.Max(e => e.DateCommitted);
            }
        }

        private bool _hasCommitBatches = false;
        public bool HasPendingCommitBatches
        {
            get
            {
                return _hasCommitBatches;
            }
            private set
            {
                Set(() => HasPendingCommitBatches, ref _hasCommitBatches, value);
            }
        }

        private int _commitBatchCount = 0;
        public int PendingCommitBatchCount
        {
            get
            {
                return _commitBatchCount;
            }
            private set
            {
                Set(() => PendingCommitBatchCount, ref _commitBatchCount, value);
            }
        }




        private int _productCount = 0;
        public int ProductCount
        {
            get
            {
                return _productCount;
            }
            private set
            {
                Set(() => ProductCount, ref _productCount, value);
            }
        }

        private int _productVariantCount = 0;
        public int ProductVariantCount
        {
            get
            {
                return _productVariantCount;
            }
            private set
            {
                Set(() => ProductVariantCount, ref _productVariantCount, value);
            }
        }

        public const string DiscontinuedProductCountPropertyName = "DiscontinuedProductCount";
        private int _discontinuedProductCount = 0;

        public int DiscontinuedProductCount
        {
            get
            {
                return _discontinuedProductCount;
            }
            private set
            {
                Set(() => DiscontinuedProductCount, ref _discontinuedProductCount, value);
            }
        }


        /// <summary>
        /// Number of products in stock. Default variant.
        /// </summary>
        private int _inStockProductCount = 0;
        public int InStockProductCount
        {
            get
            {
                return _inStockProductCount;
            }
            private set
            {
                Set(() => InStockProductCount, ref _inStockProductCount, value);
            }
        }


        /// <summary>
        /// Number of products out of stock. Default variant;
        /// </summary>
        private int _outOfStockProductCount = 0;
        public int OutOfStockProductCount
        {
            get
            {
                return _outOfStockProductCount;
            }
            private set
            {
                Set(() => OutOfStockProductCount, ref _outOfStockProductCount, value);
            }
        }


        /// <summary>
        /// Number of product variants in stock.
        /// </summary>
        private int _inStockProductVariantCount = 0;
        public int InStockProductVariantCount
        {
            get
            {
                return _inStockProductVariantCount;
            }
            private set
            {
                Set(() => InStockProductVariantCount, ref _inStockProductVariantCount, value);
            }
        }


        /// <summary>
        /// Number of product variants out of stock.
        /// </summary>
        private int _outOfStockProductVariantCount = 0;
        public int OutOfStockProductVariantCount
        {
            get
            {
                return _outOfStockProductVariantCount;
            }
            private set
            {
                Set(() => OutOfStockProductVariantCount, ref _outOfStockProductVariantCount, value);
            }
        }

        private int _clearanceProductCount = 0;
        public int ClearanceProductCount
        {
            get
            {
                return _clearanceProductCount;
            }
            private set
            {
                Set(() => ClearanceProductCount, ref _clearanceProductCount, value);
            }
        }



        private string _vendorWebsiteUsername = null;
        public string VendorWebsiteUsername
        {
            get
            {
                return _vendorWebsiteUsername;
            }
            private set
            {
                Set(() => VendorWebsiteUsername, ref _vendorWebsiteUsername, value);
            }
        }

        private string _vendorWebsitePassword = null;
        public string VendorWebsitePassword
        {
            get
            {
                return _vendorWebsitePassword;
            }
            private set
            {
                Set(() => VendorWebsitePassword, ref _vendorWebsitePassword, value);
            }
        }

        private string _vendorWebsiteUrl = null;
        public string VendorWebsiteUrl
        {
            get
            {
                return _vendorWebsiteUrl;
            }
            private set
            {
                Set(() => VendorWebsiteUrl, ref _vendorWebsiteUrl, value);
            }
        }


        private bool? _isVendorWebsiteLoginValid = null;
        public bool? IsVendorWebsiteLoginValid
        {
            get
            {
                return _isVendorWebsiteLoginValid;
            }
            private set
            {
                Set(() => IsVendorWebsiteLoginValid, ref _isVendorWebsiteLoginValid, value);
            }
        }

        /// <summary>
        /// Keeps track of web request events.
        /// </summary>
        private MultiTimeline _requestsTimeline = null;
        public MultiTimeline ScanningRequestsTimeline
        {
            get
            {
                return _requestsTimeline;
            }
            set
            {
                Set(() => ScanningRequestsTimeline, ref _requestsTimeline, value);
            }
        }

        /// <summary>
        /// Will be true for the fraction of a second or so needed while in the middle of calling start/cancel/suspend/resume.
        /// </summary>
        /// <remarks>
        /// Intended mostly for buttom commands so so they know to disable buttons when an operation is being attempted.
        /// </remarks>
        private bool _isCallingScanningOperation = false;
        public bool IsCallingScanningOperation
        {
            get
            {
                return _isCallingScanningOperation;
            }
            set
            {
                Set(() => IsCallingScanningOperation, ref _isCallingScanningOperation, value);
            }
        }


        /// <summary>
        /// Sets and gets the RecentCommits property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        private ObservableCollection<CommitBatchSummary> _recentCommits = null;
        public ObservableCollection<CommitBatchSummary> RecentCommits
        {
            get
            {
                return _recentCommits;
            }
            private set
            {
                Set(() => RecentCommits, ref _recentCommits, value);
            }
        }


        /// <summary>
        /// The folder location, if any, where this vendor's dynamic/cached files are stored.
        /// </summary>
        /// <remarks>
        /// Leave null if no folder exists, but include if the folder exists but happens to be empty.
        /// </remarks>
        private string _cachedFilesFolder = null;
        public string CachedFilesFolder
        {
            get
            {
                return _cachedFilesFolder;
            }
            private set
            {
                Set(() => CachedFilesFolder, ref _cachedFilesFolder, value);
            }
        }


        /// <summary>
        /// The folder location, if any, where this vendor's static files are stored.
        /// </summary>
        /// <remarks>
        /// Leave null if no folder exists, but include if the folder exists but happens to be empty.
        /// </remarks>
        private string _staticFilesFolder = null;
        public string StaticFilesFolder
        {
            get
            {
                return _staticFilesFolder;
            }
            private set
            {
                Set(() => StaticFilesFolder, ref _staticFilesFolder, value);
            }
        }


        /// <summary>
        /// The default number of hard (but non-fatal) errors at which scanning terminates with failure.
        /// </summary>
        /// <remarks>
        /// Fatal errors still require immediate termination.
        /// </remarks>
        private int _defaultMaximumScanningErrorCount = 5;
        public int DefaultMaximumScanningErrorCount
        {
            get
            {
                return _defaultMaximumScanningErrorCount;
            }
            private set
            {
                Set(() => DefaultMaximumScanningErrorCount, ref _defaultMaximumScanningErrorCount, value);
            }
        }

        /// <summary>
        /// The current setting for number of hard (but non-fatal) errors at which scanning terminates with failure.
        /// </summary>
        /// <remarks>
        /// Fatal errors still require immediate termination.
        /// Can be adjusted while scanning - passed through to running operation.
        /// Should be persisted with checkpoints; read back in and repopulated.
        /// </remarks>
        private int _maximumScanningErrorCount = 0;
        public int MaximumScanningErrorCount
        {
            get
            {
                return _maximumScanningErrorCount;
            }
            set
            {
                Set(() => MaximumScanningErrorCount, ref _maximumScanningErrorCount, value);
            }
        }

        /// <summary>
        /// The default number of MS delay to inject between scanner requests hitting vendor's website.
        /// </summary>
        private int _defaultDelayMSBetweenVendorWebsiteRequests = 1000;
        public int DefaultDelayMSBetweenVendorWebsiteRequests
        {
            get
            {
                return _defaultDelayMSBetweenVendorWebsiteRequests;
            }
            private set
            {
                Set(() => DefaultDelayMSBetweenVendorWebsiteRequests, ref _defaultDelayMSBetweenVendorWebsiteRequests, value);
            }
        }


        /// <summary>
        /// The current setting for number of MS delay to inject between scanner requests hitting vendor's website.
        /// </summary>
        /// <remarks>
        /// Can be adjusted while scanning - passed through to running operation.
        /// Should be persisted with checkpoints; read back in and repopulated.
        /// </remarks>
        private int _delayMSBetweenVendorWebsiteRequests = 0;
        public int DelayMSBetweenVendorWebsiteRequests
        {
            get
            {
                return _delayMSBetweenVendorWebsiteRequests;
            }
            set
            {
                Set(() => DelayMSBetweenVendorWebsiteRequests, ref _delayMSBetweenVendorWebsiteRequests, value);
            }
        }


        /// <summary>
        /// True to indicate there is a cached set of metrics in memory, such that
        /// GetStaticFileStorageMetricsAsync(false) will return immediately with a valid result.
        /// </summary>
        public bool HasStaticFileStorageMetrics
        {
            get
            {
                return staticFilesMetrics != null;
            }
        }

        /// <summary>
        /// True to indicate there is a cached set of metrics in memory, such that
        /// GetCachedFileStorageMetricsAsync(false) will return immediately with a valid result.
        /// </summary>
        public bool HasCachedFileStorageMetrics
        {
            get
            {
                return cachedFilesMetrics != null;
            }
        }


        /// <summary>
        /// Is vendor in a state which would allow scanning to be started.
        /// </summary>
        public bool IsScanningStartable
        {
            get
            {
                return IsFullyImplemented && Status != VendorStatus.Disabled && !IsScanning && !IsSuspended && !IsPerformingTests && !IsCheckingCredentials && !IsCallingScanningOperation;
            }
        }

        /// <summary>
        /// Is vendor in a state which would allow an operation to be suspended
        /// </summary>
        public bool IsScanningSuspendable
        {
            get
            {
                return IsScanning && !IsCallingScanningOperation && IsFullyImplemented;
            }
        }

        /// <summary>
        /// Is vendor in a state which would allow and operation to be resumed.
        /// </summary>
        public bool IsScanningResumable
        {
            get
            {
                return IsSuspended && !IsCallingScanningOperation && IsFullyImplemented;
            }
        }

        /// <summary>
        /// Is vendor in a state which would allow an operation to be cancelled.
        /// </summary>
        public bool IsScanningCancellable
        {
            get
            {
                return (IsScanning || IsSuspended) && !IsCallingScanningOperation && IsFullyImplemented;
            }
        }

        /// <summary>
        /// Are we in a state where clearing the scanning log is allowed.
        /// </summary>
        public bool IsScanningLogClearable
        {
            get
            {
                return scanningLogHasData && !IsScanning && !IsSuspended && IsFullyImplemented && !IsCallingScanningOperation;
            }
        }

        /// <summary>
        /// Are we in a state where we can delete all the cached files.
        /// </summary>
        public bool IsFileCacheClearable
        {
            get
            {
                return IsFullyImplemented && !IsScanning && !IsSuspended && !IsCallingScanningOperation && !string.IsNullOrEmpty(CachedFilesFolder);
            }
        }

        /// <summary>
        /// Are we in a state which would allow vendor testing to be performed.
        /// </summary>
        public bool IsTestable
        {
            get
            {
                return IsFullyImplemented && !IsScanning && !IsCallingScanningOperation;
            }
        }


        /// <summary>
        /// The status of the now running/suspended or just completed/failed operatoin.
        /// </summary>
        /// <remarks>
        /// Remains in this state until app exists, new operation starts or cleared by user.
        /// </remarks>
        private ScanningStatus _scanningOperationStatus = default(ScanningStatus);
        public ScanningStatus ScanningOperationStatus
        {
            get
            {
                return _scanningOperationStatus;
            }
            private set
            {
                Set(() => ScanningOperationStatus, ref _scanningOperationStatus, value);
            }
        }


        /// <summary>
        /// Options used to initiate the recent scanning operation.
        /// </summary>
        /// <remarks>
        /// Will be populated internally when a new operation is started. Intended to allow external code
        /// to see what options were used to start a scan. Also to be able to repopulate the read-only checkbox list
        /// on the vendor scan page to show what was used for a then-suspended operation.
        /// Never null. Should be empty collection when nothing.
        /// </remarks>
        private ObservableCollection<ScanOptions> _scanningOptions = null;
        public ObservableCollection<ScanOptions> ScanningOptions
        {
            get
            {
                return _scanningOptions;
            }
            private set
            {
                Set(() => ScanningOptions, ref _scanningOptions, value);
            }
        }

        /// <summary>
        /// Event log associated with the current or suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Must be repopulated at startup from checkpoint data if the operation was suspended.
        /// </remarks>
        private ObservableCollection<EventLogRecord> _scanningLogEvents = null;
        public ObservableCollection<EventLogRecord> ScanningLogEvents
        {
            get
            {
                return _scanningLogEvents;
            }
            private set
            {
                Set(() => ScanningLogEvents, ref _scanningLogEvents, value);
            }
        }


        /// <summary>
        /// The number of web requests per minute by the scanner. Zero when idle.
        /// </summary>
        /// <remarks>
        /// This becomes the dial value on the vendor scan page.
        /// </remarks>
        private double _scanningRequestsPerMinute = 0;
        public double ScanningRequestsPerMinute
        {
            get
            {
                return _scanningRequestsPerMinute;
            }
            private set
            {
                Set(() => ScanningRequestsPerMinute, ref _scanningRequestsPerMinute, value);
            }
        }


        /// <summary>
        /// Indicates the value to show on the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation. -1 for indeterminate, else 0 to 100.
        /// </remarks>
        private double? _scanningProgressPercentComplete = null;
        public double? ScanningProgressPercentComplete
        {
            get
            {
                return _scanningProgressPercentComplete;
            }
            private set
            {
                Set(() => ScanningProgressPercentComplete, ref _scanningProgressPercentComplete, value);
            }
        }

        /// <summary>
        /// The main progress status message - displayed above the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation or nothing to report.
        /// </remarks>
        private string _scanningProgressStatusMessage = null;
        public string ScanningProgressStatusMessage
        {
            get
            {
                return _scanningProgressStatusMessage;
            }
            private set
            {
                Set(() => ScanningProgressStatusMessage, ref _scanningProgressStatusMessage, value);
            }
        }

        /// <summary>
        /// The secondary progress message, displayed below the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation or nothing to report.
        /// </remarks>
        private string _scanningProgressSecondaryMessage = null;
        public string ScanningProgressSecondaryMessage
        {
            get
            {
                return _scanningProgressSecondaryMessage;
            }
            private set
            {
                Set(() => ScanningProgressSecondaryMessage, ref _scanningProgressSecondaryMessage, value);
            }
        }

        /// <summary>
        /// The time when the current operation was started, else null.
        /// </summary>
        /// <remarks>
        /// Must persist through for suspended operations so can rehydrate on next app start.
        /// Null when nothing to show. Remains with value after an operation completes, until either
        /// app terminated or another operation starts up.
        /// </remarks>
        private DateTime? _scanningStartTime = null;
        public DateTime? ScanningStartTime
        {
            get
            {
                return _scanningStartTime;
            }
            private set
            {
                Set(() => ScanningStartTime, ref _scanningStartTime, value);
            }
        }

        /// <summary>
        /// The time the most-recent operation ended.
        /// </summary>
        /// <remarks>
        /// Null when nothing to show. Remains with value after an operation completes, until either
        /// app terminated or another operation starts up.
        /// </remarks>
        private DateTime? _scanningEndTime = null;
        public DateTime? ScanningEndTime
        {
            get
            {
                return _scanningEndTime;
            }
            private set
            {
                Set(() => ScanningEndTime, ref _scanningEndTime, value);
            }
        }

        /// <summary>
        /// The accumulated actual running duration of the current operation. 
        /// </summary>
        /// <remarks>
        /// Since need to account for suspended states, cannot simple subtract Now from start time.
        /// Should persist with checkpoint, and pick up when scanning resumes.
        /// </remarks>
        private TimeSpan? _scanningDuration = null;
        public TimeSpan? ScanningDuration
        {
            get
            {
                return _scanningDuration;
            }
            private set
            {
                Set(() => ScanningDuration, ref _scanningDuration, value);
            }
        }

        /// <summary>
        /// The number of errors accumulated for the current/suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Null when no associated operation.
        /// </remarks>
        private int? _scanningErrorCount = null;
        public int? ScanningErrorCount
        {
            get
            {
                return _scanningErrorCount;
            }
            private set
            {
                if (Set(() => ScanningErrorCount, ref _scanningErrorCount, value))
                {
                    if (value.GetValueOrDefault() > 0)
                        ReportScanningError();
                }
            }
        }

        #endregion

        #region Scanning Helpers
        /// <summary>
        /// Internal common logic to be called once a scanning operation has been successfully started.
        /// </summary>
        /// <remarks>
        /// Will send messages, etc.
        /// </remarks>
        protected async Task StartedScanning()
        {
            ScanningStartTime = DateTime.Now;
            ScanningErrorCount = 0;
            SetWarning(null);

            // TODO: at start of operations when not suspended, delete any checkpoint since no longer of value
            // and new operation needs a fresh start

            await AddScanningLogEvent(string.Format("Scanning started: {0}", ScanningStartTime));
            await AddScanningLogEvent("Options:");

            if (ScanningOptions.Count > 0)
            {
                foreach (var item in ScanningOptions)
                    await AddScanningLogEvent(string.Format("     {0}", item.DescriptionAttr()));
            }
            else
            {
                await AddScanningLogEvent("     None");
            }

            StartOneSecondIntervalTimer();

            ScanningOperationStatus = ScanningStatus.Running; // always start out with no errors
            IsSuspended = false;
            IsScanning = true;
            ScannerState = DetermineScannerState();
            LastCheckpointDate = null;
            this.RaisePropertyChanged(() => LastCheckpointDate);

            // the notification goes out last, since recipients will query various properties.
            await SendScanningOperationNotification(ScanningOperationEvent.Started);
        }

        protected void ClearProgressMeter()
        {
            ScanningProgressPercentComplete = 0;
            ScanningProgressStatusMessage = IsSuspended ? "Ready to resume." : "Ready to start.";
            ScanningProgressSecondaryMessage = null;
        }

        protected void StartOneSecondIntervalTimer()
        {
            // The Duration will always be accurate, even across scanning segments. So it becomes a simple
            // matter on start/re-start to set the baseline, then recalc a new duration along the way
            // by using Now minus the base.

            scanningTimerBase = DateTime.Now - ScanningDuration.GetValueOrDefault(); // on first start, duration is 0
            captureScanningIntervalTimer = true;
        }

        protected void EndOneSecondIntervalTimer()
        {
            captureScanningIntervalTimer = false;
        }

        /// <summary>
        /// Called every second via hook from app-wide 1000ms timer message.
        /// </summary>
        protected void OneSecondInterval()
        {
            if (!captureScanningIntervalTimer)
                return;

            // update duration - taking into account could have been suspended along the way here and there
            ScanningDuration = DateTime.Now - scanningTimerBase;

            // update requests per minute
            var rpm = ScanningRequestsTimeline.SecondsTimeline.Sum();
            ScanningRequestsPerMinute = rpm;
        }

        /// <summary>
        /// Internal common logic to be called once a scanning operation has been successfully cancelled.
        /// </summary>
        /// <remarks>
        /// Will send messages, etc.
        /// </remarks>
        protected async Task CancelledScanning()
        {
            RecalculateCachedFilesMetrics();

            IsSuspended = false;
            IsScanning = false;
            EndOneSecondIntervalTimer();
            ScanningOperationStatus = ScanningStatus.Cancelled;
            ScannerState = DetermineScannerState();
            ScanningRequestsPerMinute = 0;
            ClearProgressMeter();

            // TODO: at end of operations when not suspended, delete any checkpoint since no longer of value
            LastCheckpointDate = null;
            await AddScanningLogEvent(string.Format("Cancelled scanning: {0}", DateTime.Now));
            await AddScanningLogEvent(string.Format("Duration: {0}", ScanningDuration.GetValueOrDefault().ToString(@"d\.hh\:mm\:ss")));

            // TODO: update cached files metrics, async, do not wait for it, use Task.Run(), etc.

            // the notification goes out last, since recipients will query various properties.
            await SendScanningOperationNotification(ScanningOperationEvent.Cancelled);
        }

        /// <summary>
        /// Internal common logic to be called once a scanning operation has been successfully resumed.
        /// </summary>
        /// <remarks>
        /// Will send messages, etc.
        /// </remarks>
        protected async Task ResumedScanning()
        {
            IsSuspended = false;
            IsScanning = true;
            StartOneSecondIntervalTimer();
            SetWarning(null);

            // could be there were previously reported errors, so need to check on that to determine which state to go into

            ScanningOperationStatus = ScanningErrorCount.GetValueOrDefault() == 0 ? ScanningStatus.Running : ScanningStatus.RunningWithErrors;
            ScannerState = DetermineScannerState();
            await AddScanningLogEvent(string.Format("Resumed scanning: {0}", DateTime.Now));

            // the notification goes out last, since recipients will query various properties.
            await SendScanningOperationNotification(ScanningOperationEvent.Resumed);
        }

        /// <summary>
        /// Internal common logic to be called once a scanning operation has been successfully suspended.
        /// </summary>
        /// <remarks>
        /// Will send messages, etc.
        /// </remarks>
        protected async Task SuspendedScanning()
        {
            RecalculateCachedFilesMetrics();

            IsSuspended = true;
            IsScanning = false;
            EndOneSecondIntervalTimer();
            ScanningOperationStatus = ScanningStatus.Suspended;
            ScannerState = DetermineScannerState();
            ScanningRequestsPerMinute = 0;
            ClearProgressMeter();
            await AddScanningLogEvent(string.Format("Suspended scanning: {0}", DateTime.Now));
            await AddScanningLogEvent(string.Format("Duration: {0}", ScanningDuration.GetValueOrDefault().ToString(@"d\.hh\:mm\:ss")));

            // TODO: update cached files metrics

            // the notification goes out last, since recipients will query various properties.
            await SendScanningOperationNotification(ScanningOperationEvent.Suspended);
        }

        protected async Task SubmittedCommitBatch(CommitBatchType batchType)
        {
            await AddScanningLogEvent(string.Format("Submitted commit batch: {0}", batchType.DescriptionAttr()));

            var tasks = new List<Task>()
                {
                    RefreshRecentCommits(),
                    RefreshPendingBatchCount(),
                };

            await Task.WhenAll(tasks);
            await SendScanningOperationNotification(ScanningOperationEvent.SubmittedCommitBatch);
            SendVendorChangedNotification();
        }

        protected async Task DiscardedOrDeletedCommitBatch()
        {
            // SHANE: see comments here - update as indicated for live logic.

            // normally you would need to call the tasks below before this op, but due to how we've
            // faked things in the IScannerDatabase layer, needs to be done here now, but replaced for final live code.
            await SendScanningOperationNotification(ScanningOperationEvent.DiscardedOrDeletedCommitBatch);

            var tasks = new List<Task>()
                {
                    RefreshRecentCommits(),
                    RefreshPendingBatchCount(),
                };

            await Task.WhenAll(tasks);
            // this notification would normally go here in the flow, but moved up top due to how we're faking things
            //await SendScanningOperationNotification(ScanningOperationEvent.DiscardedOrDeletedCommitBatch);
            SendVendorChangedNotification();
        }

        protected async Task FinishedScanning()
        {
            RecalculateCachedFilesMetrics();

            IsSuspended = false;
            IsScanning = false;

            // TODO: at end of operations when not suspended, delete any checkpoint since no longer of value
            LastCheckpointDate = null;
            this.RaisePropertyChanged(() => LastCheckpointDate);

            EndOneSecondIntervalTimer();
            ScanningRequestsPerMinute = 0;
            ClearProgressMeter();
            ScanningOperationStatus = ScanningErrorCount.GetValueOrDefault() == 0 ? ScanningStatus.Finished : ScanningStatus.FinishedWithErrors;
            ScannerState = DetermineScannerState();
            await AddScanningLogEvent(string.Format("Finished scanning: {0}", DateTime.Now));
            await AddScanningLogEvent(string.Format("Errors: {0:N0}", ScanningErrorCount));
            await AddScanningLogEvent(string.Format("Duration: {0}", ScanningDuration.GetValueOrDefault().ToString(@"d\.hh\:mm\:ss")));


            // the notification goes out last, since recipients will query various properties.
            await SendScanningOperationNotification(ScanningOperationEvent.Finished);
            SendVendorChangedNotification();
        }

        /// <summary>
        /// A running operation just failed due to exceptions or high error count.
        /// </summary>
        /// <remarks>
        /// Not intended to be called when unable to start in the first place.
        /// </remarks>
        protected async Task FailedScanning()
        {
            RecalculateCachedFilesMetrics();

            IsSuspended = false;
            IsScanning = false;

            // TODO: at end of operations when not suspended, delete any checkpoint since no longer of value

            EndOneSecondIntervalTimer();
            ScanningRequestsPerMinute = 0;
            ClearProgressMeter();
            ScanningOperationStatus = ScanningStatus.Failed;
            ScannerState = DetermineScannerState();
            LastCheckpointDate = null;
            this.RaisePropertyChanged(() => LastCheckpointDate);

            await AddScanningLogEvent(string.Format("Failed scanning: {0}", DateTime.Now));
            await AddScanningLogEvent(string.Format("Errors: {0:N0}", ScanningErrorCount));
            await AddScanningLogEvent(string.Format("Duration: {0}", ScanningDuration.GetValueOrDefault().ToString(@"d\.hh\:mm\:ss")));

            // TODO: update cached files metrics, async, do not wait for it, use Task.Run(), etc.

            // the notification goes out last, since recipients will query various properties.
            await SendScanningOperationNotification(ScanningOperationEvent.Failed);
        }

        protected async Task SendScanningOperationNotification(ScanningOperationEvent scanningEvent)
        {
            // must be sent on the UX thread

            await DispatcherHelper.RunAsync(() =>
            {
                var msg = new ScanningOperationNotification(this, scanningEvent);
                Messenger.Default.Send(msg);
            });
        }

        protected async Task AddScanningLogEvent(string text, EventType eventType = EventType.General)
        {
            // must be done on UX thread

            scanningLogHasData = true;

            await DispatcherHelper.RunAsync(async () =>
            {
                var logEntry = new EventLogRecord(eventType, text);
                ScanningLogEvents.Add(logEntry);
                await SendScanningOperationNotification(ScanningOperationEvent.LoggedEvent);
            });
        }

        protected async void CreatedScanningCheckpoint()
        {
            LastCheckpointDate = DateTime.Now;
            await AddScanningLogEvent(string.Format("Created checkpoint: {0}", DateTime.Now));
            await SendScanningOperationNotification(ScanningOperationEvent.CreatedCheckpoint);
        } 



        /// <summary>
        /// Clears out any residual state from a previously finished/terminated operation. Optional.
        /// </summary>
        /// <remarks>
        /// Should only be called when scanning is idle, if desired. Clears out things like start time, 
        /// duration, etc. Goes back to the most idle, never yet run anything state.
        /// Called automatically at the start of a new operation, but can also be invoked by user.
        /// </remarks>
        public async void ClearScanningState()
        {
            // not allowed to clear anything if we're doing something.
            // technically a programming bug to have called this at the wrong time

            if (IsScanning || IsSuspended)
                return;

            // should be implied then that the present status is one of the not-doing-anything-right-now states

            // clear: start time, end time, duration, log, errors, set status to totally idle 

            ScanningStartTime = null;
            ScanningEndTime = null;
            ScanningDuration = null;
            ScanningLogEvents.Clear();
            ScanningRequestsTimeline = new MultiTimeline();
            ScanningErrorCount = null;
            ScanningRequestsPerMinute = 0; // should already be 0
            ScanningOperationStatus = ScanningStatus.Idle;
            this.RaisePropertyChanged(() => LastCheckpointDate);
            await AddScanningLogEvent(Name);
            await AddScanningLogEvent("Ready.");
            // note this is last so two above log events don't register.
            scanningLogHasData = false;
            SendVendorChangedNotification();
        }

        /// <summary>
        /// To be called each time the core scanning logic indicates that it hit the vendor's website.
        /// </summary>
        protected void NotifyHitVendorWebsite()
        {
            // update timeline
            ScanningRequestsTimeline.Bump();
        }

        #endregion


        public bool IsVariantCentricInventory
        {
            get
            {
                var variantCentricStores = new StoreType[] { StoreType.InsideRugs };
                return variantCentricStores.Contains(ParentStore.Key);
            }
        }

    }
}
