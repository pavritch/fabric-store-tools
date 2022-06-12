using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProductScanner.Core;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Checkpoints;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.Data
{
    public class PlatformDatabase : IPlatformDatabase
    {
        public async Task SaveScannerCheckpointAsync(Vendor vendor, CheckpointData scanData, DateTime startTime, List<EventLogRecord> log, TimeSpan duration, int errorCount, int maxErrorCount, int requestDelay, ScanOptions options)
        {
            using (var db = new PlatformContext())
            {
                // Adding a new checkpoint requires that any existing checkpoint for the same manufacturer be deleted first.
                var checkpoint = await db.ScannerCheckpoints.SingleOrDefaultAsync(x => x.VendorId == vendor.Id && x.Store == vendor.Store.ToString()).ConfigureAwait(false);
                if (checkpoint != null) db.ScannerCheckpoints.Remove(checkpoint);

                var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
                var json = scanData.ToJSON(settings);
                var zipped = json.GZipMemory();

                var logJson = log.ToJSON(settings);

                db.ScannerCheckpoints.Add(new ScannerCheckpoint
                {
                    CheckpointData = zipped,
                    Created = DateTime.Now,
                    Store = vendor.Store.ToString(),
                    VendorId = vendor.Id,
                    Log = logJson,
                    ScanStarted = startTime,
                    DurationInSeconds = Convert.ToInt32(duration.TotalSeconds),
                    ScanOptions = options,
                    DelayMSBetweenVendorWebsiteRequests = requestDelay,
                    ErrorCount = errorCount,
                    MaximumScanningErrorCount = maxErrorCount
                });
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveScannerCheckpointAsync(Vendor vendor)
        {
            using (var db = new PlatformContext())
            {
                var checkpoint = await db.ScannerCheckpoints.SingleOrDefaultAsync(x => x.VendorId == vendor.Id && x.Store == vendor.Store.ToString()).ConfigureAwait(false);
                if (checkpoint != null) db.ScannerCheckpoints.Remove(checkpoint);
                await db.SaveChangesAsync();
            }
        }

        public async Task<ScannerCheckpoint> GetScannerCheckpointAsync(Vendor vendor)
        {
            using (var db = new PlatformContext())
            {
                return await db.ScannerCheckpoints.SingleOrDefaultAsync(x => x.VendorId == vendor.Id && x.Store == vendor.Store.ToString());
            }
        }

        public async Task<List<ScannerCheckpoint>> GetScannerCheckpointsAsync(Store store)
        {
            using (var db = new PlatformContext())
            {
                var checkpoints = await db.ScannerCheckpoints.Where(x => x.Store == store.ToString()).Select(x => new ScannerCheckpoint
                {
                    Created = x.Created,
                    Id = x.Id,
                    Schema = x.Schema,
                    SessionId = x.SessionId,
                    Store = x.Store,
                    VendorId = x.VendorId
                }).ToListAsync();

                // TODO: Check schema
                return checkpoints;
            }
        }

        public async Task<List<AppSetting>> GetConfigSettingsAsync()
        {
            using (var db = new PlatformContext())
            {
                return await db.AppSettings.ToListAsync();
            }
        }

        public async Task SaveVendorProductDetailPageUrl(int variantId, int vendorId, string detailUrl, string store)
        {
            using (var db = new PlatformContext())
            {
                var existingUrl = db.DetailUrls.FirstOrDefault(x => x.VariantId == variantId && x.VendorId == vendorId);
                if (existingUrl != null) return;

                db.DetailUrls.Add(new DetailUrl
                {
                    VariantId = variantId,
                    Url = detailUrl,
                    Store = store,
                    VendorId = vendorId
                });
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<DetailUrl>> GetDetailUrls(List<int> variantIds, int vendorId, string store)
        {
            using (var db = new PlatformContext())
            {
                return await db.DetailUrls.Where(x => variantIds.Contains(x.VariantId) && x.VendorId == vendorId && x.Store == store).ToListAsync();
            }
        }

        public async Task AddVendorData(VendorData vendorData)
        {
            using (var db = new PlatformContext())
            {
                db.VendorDatas.Add(vendorData);
                await db.SaveChangesAsync();
            }
        }

        public async Task<VendorData> GetVendorDataAsync(int vendorId)
        {
            using (var db = new PlatformContext())
            {
                return await db.VendorDatas.FirstOrDefaultAsync(x => x.Id == vendorId);
            }
        }

        public VendorData GetVendorData(int vendorId)
        {
            using (var db = new PlatformContext())
            {
                return db.VendorDatas.FirstOrDefault(x => x.Id == vendorId);
            }
        }

        public async Task SaveCommitAsync<T>(Vendor vendor, DateTime timestamp, CommitBatchType batchType, Guid sessionId, List<T> data)
        {
            if (data.Count == 0) return;
            using (var db = new PlatformContext())
            {
                var commit = new ScannerCommit
                {
                    BatchType = batchType,
                    CommitData = data.ToCompressedJSON(),
                    Created = timestamp,
                    DateCommitted = null,
                    //IsCommitted = false,
                    IsProcessing = false,
                    QtySubmitted = data.Count,
                    SessionStatus = CommitBatchStatus.Pending,
                    Store = vendor.Store.ToString(),
                    VendorId = vendor.Id,
                    SessionId = sessionId
                };
                db.ScannerCommits.Add(commit);
                await db.SaveChangesAsync();
            }
        }

        public async Task SaveScanLogAsync(Guid sessionId, List<EventLogRecord> list, Vendor vendor)
        {
            using (var db = new PlatformContext())
            {
                var commitLog = new ScanLog
                {
                    SessionId = sessionId,
                    Log = list.Select(x => x.Text).Aggregate((a, b) => a + Environment.NewLine + b),
                    Created = DateTime.UtcNow,
                    Store = vendor.Store.ToString(),
                    VendorId = vendor.Id
                };
                db.ScanLogs.Add(commitLog);
                await db.SaveChangesAsync();
            }
        }

        //public async Task<Dictionary<int, List<BatchType>>> GetExistingCommitBatches()
        //{
        //    using (var db = new PlatformContext())
        //    {
        //        var commits = await db.ScannerCommits.Select(x => new {x.VendorId, x.BatchType}).ToListAsync();
        //        return commits.GroupBy(x => x.VendorId).ToDictionary(g => g.Key, g => g.Select(p => p.BatchType).ToList());
        //    }
        //}

        //public async Task<List<ScannerCommit>> GetCommitBatchesAsync(int vendorId)
        //{
        //    using (var db = new PlatformContext())
        //    {
        //        return await db.ScannerCommits.Where(x => x.VendorId == vendorId).ToListAsync();
        //    }
        //}

        public async Task<int> GetPendingCommitBatchCountAsync(int vendorID)
        {
            try
            {
                using (var db = new PlatformContext())
                {
                    return await db.ScannerCommits.Where(x => x.VendorId == vendorID && x.SessionStatus == CommitBatchStatus.Pending).CountAsync();
                }
            }
            catch
            {
                return 0;   
            }
        }


        public async Task<List<CommitBatchSummary>> GetPendingCommitBatchSummariesAsync(int vendorID)
        {
            using (var db = new PlatformContext())
            {
                var list = await (from sc in db.ScannerCommits
                            where sc.VendorId == vendorID && sc.SessionStatus == CommitBatchStatus.Pending
                            orderby sc.Id descending
                            select new CommitBatchSummary()
                            {
                                Id = sc.Id,
                                Created = sc.Created,
                                Store = sc.Store,
                                VendorId = sc.VendorId,
                                BatchType = sc.BatchType,
                                QtySubmitted = sc.QtySubmitted,
                                QtyCommitted = sc.QtyCommitted,
                                DateCommitted = sc.DateCommitted,
                                SessionStatus = sc.SessionStatus,
                                IsProcessing = sc.IsProcessing,
                            }).ToListAsync();
                return list;
            }
        }

        public async Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(StoreType storeKey, int? skip = null, int? take = null)
        {
            using (var db = new PlatformContext())
            {
                var query = from sc in db.ScannerCommits
                                 where sc.Store == storeKey.ToString()
                                 orderby sc.Id descending
                                 select new CommitBatchSummary()
                                 {
                                     Id = sc.Id,
                                     Created = sc.Created,
                                     Store = sc.Store,
                                     VendorId = sc.VendorId,
                                     BatchType = sc.BatchType,
                                     QtySubmitted = sc.QtySubmitted,
                                     QtyCommitted = sc.QtyCommitted,
                                     DateCommitted = sc.DateCommitted,
                                     SessionStatus = sc.SessionStatus,
                                     IsProcessing = sc.IsProcessing,
                                 };

                if (skip.HasValue)
                    query = query.Skip(skip.Value);

                if (take.HasValue)
                    query = query.Take(take.Value);

                var list = await query.ToListAsync();

                return list;
            }
        }

        public async Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(int vendorId, int? skip = null, int? take = null)
        {
            using (var db = new PlatformContext())
            {
                var query = from sc in db.ScannerCommits
                                 where sc.VendorId == vendorId
                                 orderby sc.Id descending
                                 select new CommitBatchSummary()
                                 {
                                     Id = sc.Id,
                                     Created = sc.Created,
                                     Store = sc.Store,
                                     VendorId = sc.VendorId,
                                     BatchType = sc.BatchType,
                                     QtySubmitted = sc.QtySubmitted,
                                     QtyCommitted = sc.QtyCommitted,
                                     DateCommitted = sc.DateCommitted,
                                     SessionStatus = sc.SessionStatus,
                                     IsProcessing = sc.IsProcessing,
                                 };

                if (skip.HasValue)
                    query = query.Skip(skip.Value);

                if (take.HasValue)
                    query = query.Take(take.Value);

                var list = await query.ToListAsync();

                return list;
            }
        }


        public async Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(int? skip = null, int? take = null)
        {
            using (var db = new PlatformContext())
            {
                var query = from sc in db.ScannerCommits
                                 orderby sc.Id descending
                                 select new CommitBatchSummary()
                                 {
                                     Id = sc.Id,
                                     Created = sc.Created,
                                     Store = sc.Store,
                                     VendorId = sc.VendorId,
                                     BatchType = sc.BatchType,
                                     QtySubmitted = sc.QtySubmitted,
                                     QtyCommitted = sc.QtyCommitted,
                                     DateCommitted = sc.DateCommitted,
                                     SessionStatus = sc.SessionStatus,
                                     IsProcessing = sc.IsProcessing,
                                 };

                if (skip.HasValue)
                    query = query.Skip(skip.Value);

                if (take.HasValue)
                    query = query.Take(take.Value);

                var list = await query.ToListAsync();

                return list;
            }
        }

        public async Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(IEnumerable<int> batchNumbers)
        {
            using (var db = new PlatformContext())
            {
                var list = await (from sc in db.ScannerCommits
                                  where batchNumbers.Contains(sc.Id)
                                  orderby sc.Id descending
                                  select new CommitBatchSummary()
                                  {
                                      Id = sc.Id,
                                      Created = sc.Created,
                                      Store = sc.Store,
                                      VendorId = sc.VendorId,
                                      BatchType = sc.BatchType,
                                      QtySubmitted = sc.QtySubmitted,
                                      QtyCommitted = sc.QtyCommitted,
                                      DateCommitted = sc.DateCommitted,
                                      SessionStatus = sc.SessionStatus,
                                      IsProcessing = sc.IsProcessing,
                                  }).ToListAsync();
                return list;
            }
        }

        public async Task<ScannerCommit> GetCommitBatch(int batchId)
        {
            using (var db = new PlatformContext())
            {
                return await db.ScannerCommits.SingleOrDefaultAsync(x => x.Id == batchId);
            }
        }

        /// <summary>
        /// Mark the specified commit batch as currently being processed - locking it from other access.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        public async Task SetCommitBatchProcessingStatus(int batchId, bool isProcessing)
        {
            try
            {
                using (var db = new PlatformContext())
                {
                    var batch = await db.ScannerCommits.SingleOrDefaultAsync(x => x.Id == batchId);
                    if (batch != null)
                    {
                        batch.IsProcessing = isProcessing;
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Finalize the SQL record for a batch that has been successfully processed to store SQL.
        /// </summary>
        /// <param name="batchId"></param>
        /// <param name="quantityCommitted"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task UpdateSuccessfullyProcessedCommitBatch(int batchId, int quantityCommitted, string log)
        {
            using (var db = new PlatformContext())
            {
                var batch = await db.ScannerCommits.SingleOrDefaultAsync(x => x.Id == batchId);
                if (batch != null && batch.IsProcessing)
                {
                    batch.IsProcessing = false;
                    batch.QtyCommitted = quantityCommitted;
                    batch.DateCommitted = DateTime.Now;
                    batch.Log = log;
                    batch.SessionStatus = CommitBatchStatus.Committed;
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<CommitBatchResult> DeleteBatchAsync(int batchId, int vendorID)
        {
            // if cannot hit SQL (store or scanner) right off, then return DatabaseError.

            // if batch id no longer in scanner SQL, return NotFound

            // if batch status is InProgress, return AccessDenied

            try
            {

                using (var db = new PlatformContext())
                {
                    var batch = await db.ScannerCommits.SingleOrDefaultAsync(x => x.Id == batchId);

                    if (batch == null)
                        return CommitBatchResult.NotFound;

                    if (batch.VendorId != vendorID)
                        return CommitBatchResult.IncorrectVendor;

                    if (batch.IsProcessing)
                        return CommitBatchResult.AccessDenied;

                    db.ScannerCommits.Remove(batch);
                    await db.SaveChangesAsync();

                    return CommitBatchResult.Successful;
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return CommitBatchResult.Failed;
            }
        }

        public async Task<CommitBatchResult> DiscardBatchAsync(int batchId, int vendorID)
        {
            // if cannot hit SQL (store or scanner) right off, then return DatabaseError.

            // if batch id no longer in scanner SQL, return NotFound

            // if batch status is InProgress, return AccessDenied

            // if batch is not currently pending, return NotPending

            try
            {

                using (var db = new PlatformContext())
                {
                    var batch = await db.ScannerCommits.SingleOrDefaultAsync(x => x.Id == batchId);

                    if (batch == null)
                        return CommitBatchResult.NotFound;

                    if (batch.VendorId != vendorID)
                        return CommitBatchResult.IncorrectVendor;

                    if (batch.IsProcessing)
                        return CommitBatchResult.AccessDenied;

                    if (batch.SessionStatus != CommitBatchStatus.Pending)
                        return CommitBatchResult.NotPending;

                    batch.SessionStatus = CommitBatchStatus.Discarded;

                    await db.SaveChangesAsync();

                    return CommitBatchResult.Successful;
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return CommitBatchResult.Failed;
            }
        }

        public async Task<IVendorProperties> GetVendorPropertiesAsync(int vendorID)
        {
            try
            {
                using (var db = new PlatformContext())
                {
                    var data = await db.VendorDatas.FirstOrDefaultAsync(x => x.Id == vendorID);

                    if (data == null)
                        return new VendorProperties(VendorStatus.Disabled);

                    var vp = new VendorProperties()
                    {
                        Status = data.Status,
                        Name = data.Name
                    };

                    return vp;
                }
            }
            catch
            {
                return new VendorProperties(VendorStatus.Disabled);
            }
        }

        public async Task<bool> SaveVendorPropertiesAsync(int vendorID, IVendorProperties vendorProperties)
        {
            try
            {
                using (var db = new PlatformContext())
                {
                    var data = await db.VendorDatas.FirstOrDefaultAsync(x => x.Id == vendorID);

                    if (data == null)
                        return false;

                    data.Status = vendorProperties.Status;
                    data.Name = vendorProperties.Name;

                    await db.SaveChangesAsync();
                    return true;
                }
            }
            catch
            {
                return false;                
            }           
        }

        public async Task<bool> DoesVendorExist(int vendorId)
        {
            using (var db = new PlatformContext())
            {
                return await db.VendorDatas.CountAsync(x => x.Id == vendorId) > 0;
            }
        }
    }
}