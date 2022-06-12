using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Commits
{
    public class CommitSubmitter<T> : ICommitSubmitter<T> where T : Vendor, new()
    {
        private readonly IPlatformDatabase _database;
        private readonly IVendorScanSessionManager<T> _sessionManager; 

        public CommitSubmitter(IPlatformDatabase database, IVendorScanSessionManager<T> sessionManager)
        {
            _database = database;
            _sessionManager = sessionManager;
        }

        public async Task<bool> SubmitAsync(CommitData commitData)
        {
            if (_sessionManager.HasFlag(ScanOptions.DoNotSaveResults)) return true;

            var sessionId = _sessionManager.GetSessionId();
            var vendor = new T();
            var timestamp = DateTime.Now;

            var filledBatches = commitData.GetFilledBatches();
            var committedBatches = new List<CommitBatchType>();
            if (_sessionManager.HasFlag(ScanOptions.FullProductUpdate))
            {
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.Discontinued, sessionId, commitData.Discontinued);
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.NewProducts, sessionId, commitData.NewProducts);

                committedBatches.AddRange(new List<CommitBatchType>{ CommitBatchType.FullUpdate, CommitBatchType.Discontinued, CommitBatchType.NewProducts});
            }
            else
            {
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.Discontinued, sessionId, commitData.Discontinued);
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.NewProducts, sessionId, commitData.NewProducts);
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.InStock, sessionId, commitData.InStock);
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.OutOfStock, sessionId, commitData.OutOfStock);
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.PriceUpdate, sessionId, commitData.PriceChanges);
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.RemovedVariants, sessionId, commitData.RemovedVariants);
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.NewVariants, sessionId, commitData.NewVariantsExistingProducts);

                committedBatches.AddRange(new List<CommitBatchType>{ CommitBatchType.Discontinued, CommitBatchType.NewProducts, CommitBatchType.InStock, 
                    CommitBatchType.OutOfStock, CommitBatchType.PriceUpdate, CommitBatchType.RemovedVariants, CommitBatchType.NewVariants});
            }
            // we want to submit this batch regardless of the settings, provided that there are items in it
            // I'm checking the setting in the builder to determine what to put in the full update batch
            await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.FullUpdate, sessionId, commitData.UpdateProducts);

            if (_sessionManager.HasFlag(ScanOptions.SearchForMissingImages) &&
                !_sessionManager.HasFlag(ScanOptions.FullProductUpdate))
            {
                await _database.SaveCommitAsync(vendor, timestamp, CommitBatchType.Images, sessionId, commitData.UpdateImages);

                committedBatches.Add(CommitBatchType.Images);
            }

            var toLog = filledBatches.Intersect(committedBatches).ToList();
            toLog.ForEach(x => _sessionManager.Log(new EventLogRecord("Submitted commit batch: {0}", x.DescriptionAttr())));
            if (!toLog.Any())
            {
                _sessionManager.Log(new EventLogRecord("No commit batches submitted"));
            }
            _sessionManager.NotifyCommitSubmitted();

            await _database.SaveScanLogAsync(sessionId, _sessionManager.GetFullLog().ToList(), vendor);
            return true;
        }
    }
}