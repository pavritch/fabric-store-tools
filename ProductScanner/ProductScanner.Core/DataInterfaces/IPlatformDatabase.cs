using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Checkpoints;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.Core.DataInterfaces
{

    #region VendorProperties Class

    public interface IVendorProperties
    {
        string Name { get; }
        string Username { get; }
        string Password { get; }
        VendorStatus Status { get; }
    }

    /// <summary>
    /// Indicates the status for how vendors participate in scanning.
    /// </summary>
    /// <remarks>
    /// This state is persisted in SQL.
    /// </remarks>
    public enum VendorStatus
    {
        /// <summary>
        /// Scanning operations permitted only on demand. No Autopilot allowed.
        /// </summary>
        Manual,

        /// <summary>
        /// Autopilot enabled. 
        /// </summary>
        AutoPilot,

        /// <summary>
        /// No scanning operations allowed. Vendor is likely broken and needs programming fix.
        /// </summary>
        Disabled
    }

    /// <summary>
    /// Vendor properties to edit within the UI. 
    /// </summary>
    /// <remarks>
    /// Consider this to be a DTO subset for only what's needed by the UI editor. 
    /// </remarks>
    public class VendorProperties : IVendorProperties
    {
        public string Name {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}
        public VendorStatus Status { get; set; }

        public VendorProperties()
        {

        }

        public VendorProperties(VendorStatus Status)
        {
            this.Status = Status;
        }
    }

    #endregion

    #region CommitBatchSummary Class

    /// <summary>
    /// DTO class which mostly mimics the SQL storage, but does not include batch data/log bloat.
    /// </summary>
    /// <remarks>
    /// Intended for showing listings for commit history. The batched data is not needed unless
    /// reviewing/committing a single specific batch.
    /// </remarks>
    public class CommitBatchSummary
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Store { get; set; }
        public int VendorId { get; set; }
        public CommitBatchType BatchType { get; set; }
        public int QtySubmitted { get; set; }
        public int? QtyCommitted { get; set; }
        public DateTime? DateCommitted { get; set; }
        public CommitBatchStatus SessionStatus { get; set; }
        public bool IsProcessing { get; set; }
        // not schema
        // not log
        // not commit data
        // not sessionID
    } 
    #endregion

    public interface IPlatformDatabase
    {
        Task<VendorData> GetVendorDataAsync(int vendorId);
        Task AddVendorData(VendorData vendorData);

        Task<List<DetailUrl>> GetDetailUrls(List<int> variantIds, int vendorId, string store);
        Task SaveVendorProductDetailPageUrl(int variantId, int vendorId, string detailUrl, string store);
        Task<List<AppSetting>> GetConfigSettingsAsync();
        Task<ScannerCheckpoint> GetScannerCheckpointAsync(Vendor vendor);

        // this doesn't populate the actual commit data - intended to be used to determine which vendors have checkpoints
        Task<List<ScannerCheckpoint>> GetScannerCheckpointsAsync(Store store);
        Task SaveScannerCheckpointAsync(Vendor vendor, CheckpointData scanData, DateTime startTime, List<EventLogRecord> log, TimeSpan duration, int errorCount, int maxScanningErrorCount, int delayBetweenRequests, ScanOptions options);

        Task SaveCommitAsync<T>(Vendor vendor, DateTime timestamp, CommitBatchType batchType, Guid sessionId, List<T> data);
        Task SaveScanLogAsync(Guid sessionId, List<EventLogRecord> list, Vendor vendor);
        Task RemoveScannerCheckpointAsync(Vendor vendor);

        Task<int> GetPendingCommitBatchCountAsync(int vendorID);
        Task<List<CommitBatchSummary>> GetPendingCommitBatchSummariesAsync(int vendorID);
        Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(int vendorId, int? skip = null, int? take = null);
        Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(StoreType storeKey, int? skip = null, int? take = null);
        Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(int? skip = null, int? take = null);
        Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(IEnumerable<int> batchNumbers);


        Task<ScannerCommit> GetCommitBatch(int batchId);

        Task SetCommitBatchProcessingStatus(int batchId, bool isProcessing);
        Task UpdateSuccessfullyProcessedCommitBatch(int batchId, int quantityCommitted, string log);
        Task<CommitBatchResult> DeleteBatchAsync(int batchId, int vendorID);
        Task<CommitBatchResult> DiscardBatchAsync(int batchId, int vendorID);

        Task<IVendorProperties> GetVendorPropertiesAsync(int vendorID);
        Task<bool> SaveVendorPropertiesAsync(int vendorID, IVendorProperties vendorProperties);

        Task<bool> DoesVendorExist(int vendorId);
    }
}