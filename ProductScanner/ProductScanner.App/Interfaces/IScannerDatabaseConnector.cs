using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.App
{


    #region CommitBatchDetails Class

    /// <summary>
    /// DTO class which mostly mimics the SQL storage.
    /// </summary>
    public class CommitBatchDetails
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Store { get; set; }
        public int VendorId { get; set; }
        public CommitBatchType BatchType { get; set; }
        public int QtySubmitted { get; set; }
        public int? QtyCommitted { get; set; }
        public bool IsCommitted { get; set; }
        public DateTime? DateCommitted { get; set; }
        public CommitBatchStatus Status { get; set; }
        public bool IsProcessing { get; set; }
        public string Log { get; set; } // TODO: change to byte[] and SQL to gzipped
        public byte[] CommitData { get; set; } // gzipped JSON data
        public Guid SessionId { get; set; } 
    }
    #endregion

    #region CheckpointDetails
    public class CheckpointDetails
    {
        public DateTime Created { get; set; }
        public DateTime StartTime {get; set;}
        public TimeSpan Duration  {get; set;}
        public List<EventLogRecord> LogEvents {get; set;}
        public List<ScanOptions> Options  {get; set;}
        public int ErrorCount  {get; set;}
        public int DelayMSBetweenVendorWebsiteRequests {get; set;}
        public int MaximumScanningErrorCount { get; set; }
    }
    #endregion

    #region VendorProperties Class



    /// <summary>
    /// Vendor properties to edit within the UI. 
    /// </summary>
    /// <remarks>
    /// Consider this to be a DTO subset for only what's needed by the UI editor. 
    /// </remarks>
    public class VendorProperties : ObservableObject, IVendorProperties
    {
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
        private string _userName = null;
        public string Username
        {
            get
            {
                return _userName;
            }
            set
            {
                Set(() => Username, ref _userName, value);
            }
        }

        private string _password = null;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                Set(() => Password, ref _password, value);
            }
        }


        private VendorStatus _status =  VendorStatus.Manual;
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


        public VendorProperties()
        {

        }

        public VendorProperties(IVendorProperties p)
        {
            if (p == null) return;

            this.Name = p.Name;
            this.Username = p.Username;
            this.Password = p.Password;
            this.Status = p.Status;
        }
        
    }

    #endregion

    /// <summary>
    /// Abstraction for database access to ProductScanner SQL.
    /// </summary>
    public interface IScannerDatabaseConnector
    {
        /// <summary>
        /// Get full set of vendor profile data. Read only.
        /// </summary>
        /// <remarks>
        /// Needed for login validation.
        /// </remarks>
        /// <param name="vendorID"></param>
        /// <returns></returns>
        Task<VendorData> GetVendorDataAsync(int vendorID);

        /// <summary>
        /// Return editable properties for the specified vendor.
        /// </summary>
        /// <param name="vendorID"></param>
        /// <returns></returns>
        Task<VendorProperties> GetVendorPropertiesAsync(int vendorID);

        /// <summary>
        /// Persist editable vendor properties back to SQL.
        /// </summary>
        /// <param name="vendorID"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        Task<bool> SaveVendorPropertiesAsync(int vendorID, IVendorProperties properties);

        /// <summary>
        /// Fetch recent commit batches in descending order for the specified vendor.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="skip">Optional number of records to skip.</param>
        /// <param name="take">Optional number of records to take.</param>
        /// <returns></returns>
        Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(int vendorID, int? skip = null, int? take = null);

        /// <summary>
        /// Fetch summaries for all pending batches for this vendor.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <returns></returns>
        Task<List<CommitBatchSummary>> GetPendingCommitBatchSummariesAsync(int vendorID);

        /// <summary>
        /// Fetch recent commit batches in descending order for the specified store.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="skip">Optional number of records to skip.</param>
        /// <param name="take">Optional number of records to take.</param>
        /// <returns></returns>
        Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(StoreType storeKey, int? skip = null, int? take = null);


        /// <summary>
        /// Get a specific set of batches.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="batchNumbers"></param>
        /// <returns></returns>
        Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(IEnumerable<int> batchNumbers);

        /// <summary>
        /// Return the number of pending commit batches for the specified vendor.
        /// </summary>
        /// <param name="vendorID"></param>
        /// <returns></returns>
        Task<int> GetPendingCommitBatchCountAsync(int vendorID);

        /// <summary>
        /// Retrieve the details for a single batch. Can be any status.
        /// </summary>
        /// <param name="batchID"></param>
        /// <returns>Null if batchID does not exist.</returns>
        Task<CommitBatchDetails> GetCommitBatchAsync(int batchID);


        /// <summary>
        /// Fetch the details of the single|none checkpoint for the vendor.
        /// </summary>
        /// <remarks>
        /// Return null if no checkpoint for this vendor.
        /// </remarks>
        /// <param name="vendorID"></param>
        /// <returns></returns>
        Task<CheckpointDetails> GetCheckpointDetailsAsync(Vendor vendor);

        /// <summary>
        /// Completed delete the specified batch from SQL.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<CommitBatchResult> DeleteBatchAsync(int batchId, int vendorID);

        /// <summary>
        /// Mark the batch in SQL as discarded. Just sets the status.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task<CommitBatchResult> DiscardBatchAsync(int batchId, int vendorID);

    }
}
