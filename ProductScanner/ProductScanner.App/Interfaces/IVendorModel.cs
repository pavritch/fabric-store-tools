using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;
using EventLogRecord = ProductScanner.Core.Scanning.EventLogs.EventLogRecord;

namespace ProductScanner.App
{
    /// <summary>
    /// Virtualization of the in-memory model which represents a vendor.
    /// </summary>
    /// <remarks>
    /// One of these models is created for each vendor supported in the code at the start of the 
    /// program. This acts like a middle man between the various UX view models and the core runtime modules
    /// which perform scanning, committing, etc.
    /// </remarks>
    public interface IVendorModel
    {
        Vendor Vendor { get;}

        #region General
        /// <summary>
        /// Indicates that initialization has been completed.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Populate the model.
        /// </summary>
        /// <remarks>
        /// Performed external to constructor since could be a long-running action (a second or two).
        /// Returns IsInitialized.
        /// </remarks>
        /// <returns></returns>
        Task<bool> InitializeAsync();

        /// <summary>
        /// The parent store for this vendor.
        /// </summary>
        IStoreModel ParentStore { get; }

        /// <summary>
        /// Display name used within UX.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates if the vendor is currently running a scan, is suspended, has batches to commit, etc.
        /// </summary>
        ScannerState ScannerState { get; }

        /// <summary>
        /// Indicates if this vendor is fully supported in code, store SQL and platform SQL.
        /// </summary>
        /// <remarks>
        /// Vendors which are not fully implemented will still be listed in the tree view, but disabled.
        /// Seeing a disabled vendor in the tree is a clue that this vendor is missing from one or both SQL databases.
        /// </remarks>
        bool IsFullyImplemented { get; }

        /// <summary>
        /// Indicates how this vendor participates in group scanning operations.
        /// </summary>
        VendorStatus Status { get; }

        /// <summary>
        /// The folder location, if any, where this vendor's static files are stored.
        /// </summary>
        /// <remarks>
        /// Leave null if no folder exists, but include if the folder exists but happens to be empty.
        /// </remarks>
        string StaticFilesFolder { get; }

        /// <summary>
        /// The folder location, if any, where this vendor's dynamic/cached files are stored.
        /// </summary>
        /// <remarks>
        /// Leave null if no folder exists, but include if the folder exists but happens to be empty.
        /// </remarks>
        string CachedFilesFolder { get; }

        /// <summary>
        /// Username needed to log into vendor's website.
        /// </summary>
        string VendorWebsiteUsername { get; }

        /// <summary>
        /// Password needed to log into vendor's website.
        /// </summary>
        string VendorWebsitePassword { get; }

        /// <summary>
        /// Url for vendor's website. This is for HUMAN login. 
        /// </summary>
        /// <remarks>
        /// If there is a separate url for some other FTP site, etc., then 
        /// that is totally different.
        /// </remarks>
        string VendorWebsiteUrl { get; }

        /// <summary>
        /// Indicates if we can login to vendor's website using url/username/password.
        /// </summary>
        /// <remarks>
        /// Null for not tested. Otherwise, true or false to indicate the tested result.
        /// </remarks>
        bool? IsVendorWebsiteLoginValid { get; }

        /// <summary>
        /// When true, means each variant is counted and stocked separately (like for rugs).
        /// When false, means default variant is main product, and the other variant is nearly always for swatches.
        /// </summary>
        /// <remarks>
        /// No critical math is based on this. Mostly used to determine which sets of numbers would be more meaningful
        /// for graphs, etc.
        /// </remarks>
        bool IsVariantCentricInventory { get; }

        /// <summary>
        /// The default number of MS delay to inject between scanner requests hitting vendor's website.
        /// </summary>
        int DefaultDelayMSBetweenVendorWebsiteRequests { get; }

        /// <summary>
        /// The current setting for number of MS delay to inject between scanner requests hitting vendor's website.
        /// </summary>
        /// <remarks>
        /// Can be adjusted while scanning - passed through to running operation.
        /// Should be persisted with checkpoints; read back in and repopulated.
        /// </remarks>
        int DelayMSBetweenVendorWebsiteRequests { get; set; }

        /// <summary>
        /// The default number of hard (but non-fatal) errors at which scanning terminates with failure.
        /// </summary>
        /// <remarks>
        /// Fatal errors still require immediate termination.
        /// </remarks>
        int DefaultMaximumScanningErrorCount { get; }

        /// <summary>
        /// The current setting for number of hard (but non-fatal) errors at which scanning terminates with failure.
        /// </summary>
        /// <remarks>
        /// Fatal errors still require immediate termination.
        /// Can be adjusted while scanning - passed through to running operation.
        /// Should be persisted with checkpoints; read back in and repopulated.
        /// </remarks>
        int MaximumScanningErrorCount { get; set; }

        /// <summary>
        /// Is vendor in a state which would allow scanning to be started.
        /// </summary>
        bool IsScanningStartable { get; }

        /// <summary>
        /// Is vendor in a state which would allow an operation to be suspended
        /// </summary>
        bool IsScanningSuspendable { get; }

        /// <summary>
        /// Is vendor in a state which would allow and operation to be resumed.
        /// </summary>
        bool IsScanningResumable { get; }

        /// <summary>
        /// Is vendor in a state which would allow an operation to be cancelled.
        /// </summary>
        bool IsScanningCancellable { get; }

        /// <summary>
        /// Are we in a state where clearing the scanning log is allowed.
        /// </summary>
        bool IsScanningLogClearable { get; }

        /// <summary>
        /// Are we in a state where we can delete all the cached files.
        /// </summary>
        bool IsFileCacheClearable { get; }

        /// <summary>
        /// Are we in a state which would allow vendor testing to be performed.
        /// </summary>
        bool IsTestable { get; }

        #endregion

        #region State of Things

        /// <summary>
        /// True when there is a warning or error associated with this vendor.
        /// </summary>
        bool HasWarning { get; }

        /// <summary>
        /// When HasWarning is true, this property will hold some associated message with more information about the problem.
        /// </summary>
        string WarningText { get; }

        /// <summary>
        /// Indicates if the vendor is scanning right now.
        /// </summary>
        bool IsScanning { get; }

        /// <summary>
        /// Indicates if the vendor is presently suspended.
        /// </summary>
        /// <remarks>
        /// This is identical to checking the scan state to be suspended.
        /// </remarks>
        bool IsSuspended { get; }

        /// <summary>
        /// Indicates if the vendor is running sanity checks right now.
        /// </summary>
        bool IsPerformingTests { get; }

        /// <summary>
        /// Indicates if the vendor is checking stock right now.
        /// </summary>
        bool IsCheckingStock { get; }

        /// <summary>
        /// Indicates if the vendor is checking the website login credentials right now.
        /// </summary>
        bool IsCheckingCredentials { get; }

        /// <summary>
        /// Indicates if there is a checkpoint in SQL for this vendor.
        /// </summary>
        /// <remarks>
        /// Applies to both running and suspended scan operations. If there is a row in SQL,
        /// this returns true.
        /// </remarks>
        bool HasCheckpoint { get; }

        /// <summary>
        /// When there is a checkpoint, indicates the date when persisted.
        /// </summary>
        DateTime? LastCheckpointDate { get; }

        /// <summary>
        /// When the last commit was done.
        /// </summary>
        DateTime? LastCommitDate { get; }

        /// <summary>
        /// Indicates if there are one or more rows in SQL ready to be committed to the store SQL.
        /// </summary>
        bool HasPendingCommitBatches { get; }

        /// <summary>
        /// Number of commit batches ready to process.
        /// </summary>
        /// <remarks>
        /// Will be non-zero when HasCommitBatches is true.
        /// </remarks>
        int PendingCommitBatchCount { get; }

        /// <summary>
        /// Sets and gets the RecentCommits property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        ObservableCollection<CommitBatchSummary> RecentCommits { get; }


        #endregion

        #region Methods

        /// <summary>
        /// Clear state for any current warning.
        /// </summary>
        /// <remarks>
        /// For when user clicks dismiss in the UX.
        /// </remarks>
        void ClearWarning();

        /// <summary>
        /// Check to see if can log in to vendor's website using current username/password.
        /// </summary>
        /// <remarks>
        /// Upon completion, internally must set IsVendorWebsiteLoginValid to match the returned result.
        /// If failed, set the warning.
        /// </remarks>
        /// <returns></returns>
        Task<bool> VerifyVendorWebsiteCredentialsAsync();

        /// <summary>
        /// Return editable properties for the specified vendor.
        /// </summary>
        /// <returns></returns>
        Task<VendorProperties> GetVendorPropertiesAsync();

        /// <summary>
        /// Persist editable vendor properties back to SQL.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        Task<bool> SaveVendorPropertiesAsync(VendorProperties properties);

        /// <summary>
        /// Gets the ordered list of tests which can be performed for this vendor.
        /// </summary>
        /// <returns></returns>
        List<TestDescriptor> GetTests();

        /// <summary>
        /// Run a single test.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        Task<TestResultCode> RunTestAsync(IVendorTest test);

        /// <summary>
        /// Indicate the beginning of a series of tests. The UX informs the model.
        /// </summary>
        /// <remarks>
        /// The tests are then run individually using RunTest(), then finally calling EndTest().
        /// </remarks>
        /// <returns>Return false if testing is blocked/restricted and not allowed.</returns>
        Task<bool> BeginTestingAsync();

        /// <summary>
        /// The UX informs the model that it is done running tests.
        /// </summary>
        void EndTesting();

        /// <summary>
        /// Retrieve the details for a single batch. Can be any status.
        /// </summary>
        /// <param name="batchID"></param>
        /// <returns>Null if batchID does not exist.</returns>
        Task<CommitBatchDetails> GetCommitBatchAsync(int batchID);

        /// <summary>
        /// Fetch commit batches in descending order.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="skip">Optional number of records to skip.</param>
        /// <param name="take">Optional number of records to take.</param>
        /// <returns></returns>
        Task<List<CommitBatchSummary>> GetCommitBatchesAsync(int? skip = null, int? take = null);

        /// <summary>
        /// Fetch summaries for all pending batches for this vendor.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <returns></returns>
        Task<List<CommitBatchSummary>> GetPendingCommitBatchSummariesAsync();

        #endregion

        #region Various Counts

        /// <summary>
        /// Number of products.
        /// </summary>
        /// <remarks>
        /// Include all for this vendorID - irrespective of if discontinued or in/out stock.
        /// </remarks>
        int ProductCount { get; }

        /// <summary>
        /// Number of product variants.
        /// </summary>
        /// <remarks>
        /// Include all for this vendorID - irrespective of if discontinued or in/out stock.
        /// </remarks>
        int ProductVariantCount { get; }

        /// <summary>
        /// The number of products in store SQL marked as discontinued.
        /// </summary>
        int DiscontinuedProductCount { get; }

        /// <summary>
        /// Number of products in stock. Default variant.
        /// </summary>
        int InStockProductCount { get; }

        /// <summary>
        /// Number of products out of stock. Default variant;
        /// </summary>
        int OutOfStockProductCount { get; }

        /// <summary>
        /// Number of product variants in stock.
        /// </summary>
        int InStockProductVariantCount { get; }

        /// <summary>
        /// Number of product variants out of stock.
        /// </summary>
        int OutOfStockProductVariantCount { get; }

        /// <summary>
        /// The number of products in store SQL marked as clearance.
        /// </summary>
        int ClearanceProductCount { get; }

        int StaticFileVersionTxt { get; }

        /// <summary>
        /// Get the storage metrics for the cached files folder tree for this vendor.
        /// </summary>
        /// <remarks>
        /// If recalculate is true, then compute even if have some metrics in memory.
        /// </remarks>
        /// <returns>Null if no corresponding folder for this vendor.</returns>
        Task<FileStorageMetrics> GetCachedFileStorageMetricsAsync(bool recalculate);

        /// <summary>
        /// Get the storage metrics for teh static files folder tree for this vendor.
        /// </summary>
        /// <remarks>
        /// If recalculate is true, then compute even if have some metrics in memory.
        /// </remarks>
        /// <returns>Null if no corresponding folder for this vendor.</returns>
        Task<FileStorageMetrics> GetStaticFileStorageMetricsAsync(bool recalculate);

        /// <summary>
        /// True to indicate there is a cached set of metrics in memory, such that
        /// GetStaticFileStorageMetricsAsync(false) will return immediately with a valid result.
        /// </summary>
        bool HasStaticFileStorageMetrics { get; }

        /// <summary>
        /// True to indicate there is a cached set of metrics in memory, such that
        /// GetCachedFileStorageMetricsAsync(false) will return immediately with a valid result.
        /// </summary>
        bool HasCachedFileStorageMetrics { get; }

        #endregion


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
        Task<CommitBatchResult> CommitBatchAsync(int batchId, int estimatedRecordCount, bool ignoreDuplicates, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null);

        /// <summary>
        /// Discard the specified batch. Just marked as discarded in scanner SQL.
        /// </summary>
        /// <param name="batchId">Batch must be associated with this vendor.</param>
        /// <returns></returns>
        Task<CommitBatchResult> DiscardBatchAsync(int batchId);

        /// <summary>
        /// Full delete of the indicated batch. Usually reserved for removing already-discarded batches to clean things up
        /// but could in fact delete any kind of batch.
        /// </summary>
        /// <param name="batchId">Batch must be associated with this vendor.</param>
        /// <returns></returns>
        Task<CommitBatchResult> DeleteBatchAsync(int batchId);


        /// <summary>
        /// Delete downloaded cached files for vendor which are older than N days - 0 removes all.
        /// </summary>
        /// <param name="olderThanDays"></param>
        /// <param name="cancelToken"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        Task<ActivityResult> DeleteCachedFilesAsync(int olderThanDays, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null);


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
        Task<ScanningActionResult> StartScanning(ScanOptions options);

        /// <summary>
        /// Cancel the running or suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        Task<ScanningActionResult> CancelScanning();

        /// <summary>
        /// Suspend the now running scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        Task<ScanningActionResult> SuspendScanning();

        /// <summary>
        /// Resume the presently-suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        Task<ScanningActionResult> ResumeScanning();

        /// <summary>
        /// Clears out any residual state from a previously finished/terminated operation. Optional.
        /// </summary>
        /// <remarks>
        /// Should only be called when scanning is idle, if desired. Clears out things like start time, 
        /// duration, etc. Goes back to the most idle, never yet run anything state. 
        /// </remarks>
        void ClearScanningState();

        /// <summary>
        /// The status of the now running/suspended or just completed/failed operatoin.
        /// </summary>
        /// <remarks>
        /// Remains in this state until app exists, new operation starts or cleared by user.
        /// </remarks>
        ScanningStatus ScanningOperationStatus { get; }

        /// <summary>
        /// Options used to initiate the recent scanning operation.
        /// </summary>
        /// <remarks>
        /// Will be populated internally when a new operation is started. Intended to allow external code
        /// to see what options were used to start a scan. Also to be able to repopulate the read-only checkbox list
        /// on the vendor scan page to show what was used for a then-suspended operation.
        /// Never null. Should be empty collection when nothing.
        /// </remarks>
        ObservableCollection<ScanOptions> ScanningOptions { get; }

        /// <summary>
        /// Event log associated with the current or suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Must be repopulated at startup from checkpoint data if the operation was suspended.
        /// </remarks>
        ObservableCollection<EventLogRecord> ScanningLogEvents { get; }

        /// <summary>
        /// The number of web requests per minute by the scanner. Zero when idle.
        /// </summary>
        /// <remarks>
        /// This becomes the dial value on the vendor scan page.
        /// </remarks>
        double ScanningRequestsPerMinute { get; }

        /// <summary>
        /// Indicates the value to show on the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation. -1 for indeterminate, else 0 to 100.
        /// </remarks>
        double? ScanningProgressPercentComplete { get; }

        /// <summary>
        /// The main progress status message - displayed above the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation or nothing to report.
        /// </remarks>
        string ScanningProgressStatusMessage { get; }

        /// <summary>
        /// The secondary progress message, displayed below the meter bar.
        /// </summary>
        /// <remarks>
        /// Null when no operation or nothing to report.
        /// </remarks>
        string ScanningProgressSecondaryMessage { get; }

        /// <summary>
        /// The time when the current operation was started, else null.
        /// </summary>
        /// <remarks>
        /// Must persist through for suspended operations so can rehydrate on next app start.
        /// Null when nothing to show. Remains with value after an operation completes, until either
        /// app terminated or another operation starts up.
        /// </remarks>
        DateTime? ScanningStartTime { get; }

        /// <summary>
        /// The time the most-recent operation ended.
        /// </summary>
        /// <remarks>
        /// Null when nothing to show. Remains with value after an operation completes, until either
        /// app terminated or another operation starts up.
        /// </remarks>
        DateTime? ScanningEndTime { get; }

        /// <summary>
        /// The accumulated actual running duration of the current operation. 
        /// </summary>
        /// <remarks>
        /// Since need to account for suspended states, cannot simple subtract Now from start time.
        /// Should persist with checkpoint, and pick up when scanning resumes.
        /// </remarks>
        TimeSpan? ScanningDuration { get; }

        /// <summary>
        /// The number of errors accumulated for the current/suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Null when no associated operation.
        /// </remarks>
        int? ScanningErrorCount { get; }

        /// <summary>
        /// Will be true for the fraction of a second or so needed while in the middle of calling start/cancel/suspend/resume.
        /// </summary>
        /// <remarks>
        /// Intended mostly for buttom commands so so they know to disable buttons when an operation is being attempted.
        /// </remarks>
        bool IsCallingScanningOperation { get; }

        #endregion

    }
}
