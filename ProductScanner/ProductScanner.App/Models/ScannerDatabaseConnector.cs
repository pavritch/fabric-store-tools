using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.App
{
    /// <summary>
    /// Implements IScannerDatabaseConnector to virtualize access to SQL.
    /// Note: this class is not really necessarily - just passes through to IPlatformDatabase
    /// </summary>
    public class ScannerDatabaseConnector : IScannerDatabaseConnector
    {

        public ScannerDatabaseConnector()
        {

        }

        private IPlatformDatabase GetDatabase()
        {
            return App.GetInstance<IPlatformDatabase>();
        }

        /// <summary>
        /// Get full set of vendor profile data. Read only.
        /// </summary>
        /// <remarks>
        /// Needed for login validation.
        /// </remarks>
        /// <param name="vendorID"></param>
        /// <returns></returns>
        public async Task<VendorData> GetVendorDataAsync(int vendorID)
        {
            try
            {
                var database = GetDatabase();
                return await database.GetVendorDataAsync(vendorID);

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Return editable properties for the specified vendor.
        /// </summary>
        /// <param name="vendorID"></param>
        /// <returns></returns>
        public async Task<VendorProperties> GetVendorPropertiesAsync(int vendorID)
        {
            try
            {
                var database = GetDatabase();
                var vp = await database.GetVendorPropertiesAsync(vendorID);
                var vendorProperties = new VendorProperties(vp);
                return vendorProperties;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Persist editable vendor properties back to SQL.
        /// </summary>
        /// <param name="vendorID"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public async Task<bool> SaveVendorPropertiesAsync(int vendorID, IVendorProperties properties)
        {
            try
            {
                var database = GetDatabase();
                return await database.SaveVendorPropertiesAsync(vendorID, properties);
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// Fetch recent commit batches in descending order for the specified vendor.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="skip">Optional number of records to skip.</param>
        /// <param name="take">Optional number of records to take.</param>
        /// <returns></returns>
        public Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(int vendorId, int? skip = null, int? take = null)
        {
            var database = GetDatabase();
            return database.GetCommitBatchSummariesAsync(vendorId, skip, take);
        }

        /// <summary>
        /// Fetch recent commit batches in descending order for the specified store.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <param name="skip">Optional number of records to skip.</param>
        /// <param name="take">Optional number of records to take.</param>
        /// <returns></returns>
        public Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(StoreType storeKey, int? skip = null, int? take = null)
        {
            var database = GetDatabase();
            return database.GetCommitBatchSummariesAsync(storeKey, skip, take);
        }

        /// <summary>
        /// Get a specific set of batches.
        /// </summary>
        /// <remarks>
        /// Descending. It is okay if some batch numbers no longer exist. May have just deleted some and doing a refresh.
        /// </remarks>
        /// <param name="batchNumbers"></param>
        /// <returns></returns>
        public Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(IEnumerable<int> batchNumbers)
        {
            var database = GetDatabase();
            return database.GetCommitBatchSummariesAsync(batchNumbers);
        }

        /// <summary>
        /// Return the number of pending commit batches for the specified vendor.
        /// </summary>
        /// <param name="vendorID"></param>
        /// <returns></returns>
        public Task<int> GetPendingCommitBatchCountAsync(int vendorID)
        {
            var database = GetDatabase();
            return database.GetPendingCommitBatchCountAsync(vendorID);
        }

        /// <summary>
        /// Fetch the details of the single|none checkpoint for the vendor.
        /// </summary>
        /// <remarks>
        /// Return null if no checkpoint for this vendor.
        /// </remarks>
        /// <param name="vendorID"></param>
        /// <returns></returns>
        public async Task<CheckpointDetails> GetCheckpointDetailsAsync(Vendor vendor)
        {
            var database = GetDatabase();
            var checkpoint = await database.GetScannerCheckpointAsync(vendor);
            if (checkpoint == null) return null;

            var log = checkpoint.Log.FromJSON<List<EventLogRecord>>();

            return new CheckpointDetails
            {
                Created = checkpoint.Created,
                LogEvents = log,
                StartTime = checkpoint.ScanStarted,
                Duration = new TimeSpan(0, 0, checkpoint.DurationInSeconds),
                Options = checkpoint.ScanOptions.MaskToList<ScanOptions>().ToList(),
                DelayMSBetweenVendorWebsiteRequests = checkpoint.DelayMSBetweenVendorWebsiteRequests,
                ErrorCount = checkpoint.ErrorCount,
                MaximumScanningErrorCount = checkpoint.MaximumScanningErrorCount
            };
        }

        /// <summary>
        /// Fetch summaries for all pending batches for this vendor.
        /// </summary>
        /// <remarks>
        /// Descending.
        /// </remarks>
        /// <returns></returns>
        public Task<List<CommitBatchSummary>> GetPendingCommitBatchSummariesAsync(int vendorID)
        {
            var database = GetDatabase();
            return database.GetPendingCommitBatchSummariesAsync(vendorID);
        }


        /// <summary>
        /// Completed delete the specified batch from SQL.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public Task<CommitBatchResult> DeleteBatchAsync(int batchId, int vendorID)
        {
            var database = GetDatabase();
            return database.DeleteBatchAsync(batchId, vendorID);
        }

        /// <summary>
        /// Mark the batch in SQL as discarded. Just sets the status.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public Task<CommitBatchResult> DiscardBatchAsync(int batchId, int vendorID)
        {
            var database = GetDatabase();
            return database.DiscardBatchAsync(batchId, vendorID);
        }


        /// <summary>
        /// Retrieve the details for a single batch. Can be any status.
        /// </summary>
        /// <param name="batchID"></param>
        /// <returns>Null if batchID does not exist.</returns>
        public async Task<CommitBatchDetails> GetCommitBatchAsync(int batchID)
        {
            var database = GetDatabase();
            var commit = await database.GetCommitBatch(batchID);
            return new CommitBatchDetails
            {
                BatchType = commit.BatchType,
                CommitData = commit.CommitData,
                Created = commit.Created,
                DateCommitted = commit.DateCommitted,
                Id = commit.Id,
                IsCommitted = commit.SessionStatus == CommitBatchStatus.Committed,
                IsProcessing = commit.IsProcessing,
                Log = commit.Log,
                QtyCommitted = commit.QtyCommitted,
                QtySubmitted = commit.QtySubmitted,
                SessionId = commit.SessionId,
                Status = commit.SessionStatus,
                Store = commit.Store,
                VendorId = commit.VendorId
            };
        }
    }
}
