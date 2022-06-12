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

namespace ProductScanner.App
{
  /// <summary>
  /// Implements IScannerDatabaseConnector to virtualize access to SQL.
  /// </summary>
  public class FakeScannerDatabaseConnector : IScannerDatabaseConnector
  {

    // SHANE - this concrete class must be fleshed out by you

    // fake - would not be here for real
    private VendorProperties vendorProperties = new VendorProperties()
    {
      Name = "My Vendor Name",
      Username = "tffffff",
      Password = "Myffffffsword",
      Status = VendorStatus.AutoPilot
    };

    /// <summary>
    ///  Fake. Track new batches per vendor.
    /// </summary>
    private Dictionary<int, int> dicPendingBatches = new Dictionary<int, int>();

    public FakeScannerDatabaseConnector()
    {
      if (!ViewModelBase.IsInDesignModeStatic)
      {
        HookMessages();
      }
    }

    private void HookMessages()
    {

      // just faking out the UX - nothing here relates to what we'd have for live code

      Messenger.Default.Register<AnnouncementMessage>(this, (msg) =>
      {
        switch (msg.Kind)
        {
          default:
            break;
        }
      });

      Messenger.Default.Register<VendorChangedNotification>(this, (msg) =>
      {
      });

      Messenger.Default.Register<ScanningOperationNotification>(this, (msg) =>
      {
        var vendorID = msg.Vendor.Vendor.Id;
        int count = 0;

        switch (msg.ScanningEvent)
        {
          case ScanningOperationEvent.SubmittedCommitBatch:
            if (dicPendingBatches.TryGetValue(vendorID, out count))
              dicPendingBatches[vendorID] = count + 1;
            else
              dicPendingBatches[vendorID] = 1;
            break;

          case ScanningOperationEvent.DiscardedOrDeletedCommitBatch:
            dicPendingBatches.TryGetValue(vendorID, out count);
            if (count > 0)
              dicPendingBatches[vendorID] = count - 1;
            break;

          case ScanningOperationEvent.Started:
            dicPendingBatches[vendorID] = 0;
            break;

          default:
            break;
        }
      });

    }

    private IAppModel AppModel
    {
      get
      {
        // purposely not held internally since that would create a circular reference.
        // Cannot inject in ctor since AppModel concrete class already depends on this being injected.
        return App.GetInstance<IAppModel>();
      }
    }


    /// <summary>
    /// Determine if this vendor has a record in the VendorData table.
    /// </summary>
    /// <param name="storeKey"></param>
    /// <param name="vendorID"></param>
    /// <returns></returns>
    public Task<bool> IsVendorInDatabaseAsync(int vendorID)
    {
      // fake showing one as not there
      var result = vendorID == 19 ? false : true;
      return Task.FromResult(result);
    }

    /// <summary>
    /// Return editable properties for the specified vendor.
    /// </summary>
    /// <param name="vendorID"></param>
    /// <returns></returns>
    public Task<VendorProperties> GetVendorPropertiesAsync(int vendorID)
    {
      // as coded, assumes vendorID is the ASPDNSF manufacturerID - but if some other ID
      // is more suitable, change things as needed...

      // fake - and all vendors use the same data
      return Task.FromResult(vendorProperties);
    }

    /// <summary>
    /// Persist editable vendor properties back to SQL.
    /// </summary>
    /// <param name="vendorID"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public Task<bool> SaveVendorPropertiesAsync(int vendorID, IVendorProperties properties)
    {
      // TODO: Implement this method
      throw new NotImplementedException();
    }

    /// <summary>
    /// Persist editable vendor properties back to SQL.
    /// </summary>
    /// <param name="vendorID"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public Task<bool> SaveVendorPropertiesAsync(int vendorID, VendorProperties properties)
    {
      // as coded, assumes vendorID is the ASPDNSF manufacturerID - but if some other ID
      // is more suitable, change things as needed...

      // fake - and all vendors use the same data

      vendorProperties = properties;
      return Task.FromResult(true);
    }

    /// <summary>
    /// Return editable properties for the specified vendor.
    /// </summary>
    /// <param name="vendorID"></param>
    /// <returns></returns>
    public Task<VendorData> GetVendorDataAsync(int vendorID)
    {
      return Task.FromResult(new VendorData());
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
    public Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(int vendorID, int? skip = null, int? take = null)
    {
      return Task.FromResult(MakeFakeCommits().OrderByDescending(e => e.Id).ToList());
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
      return Task.FromResult(MakeFakeCommits().OrderByDescending(e => e.Id).ToList());
    }


    /// <summary>
    /// Get a specific set of batches.
    /// </summary>
    /// <remarks>
    /// Descending. It is okay if some batch numbers no longer exist. May have just deleted some and doing a refresh.
    /// </remarks>
    /// <param name="batchNumbers"></param>
    /// <returns></returns>
    public async Task<List<CommitBatchSummary>> GetCommitBatchSummariesAsync(IEnumerable<int> batchNumbers)
    {
      await Task.Delay(150); // just to simulate some latency.
      return MakeFakeCommits();
    }

    /// <summary>
    /// Return the number of pending commit batches for the specified vendor.
    /// </summary>
    /// <param name="vendorID"></param>
    /// <returns></returns>
    public Task<int> GetPendingCommitBatchCountAsync(int vendorID)
    {
      int count = 0;
      dicPendingBatches.TryGetValue(vendorID, out count);
      return Task.FromResult(count);
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
      //var cp = new CheckpointDetails()
      //{
      //    Created = DateTime.Now,
      //    StartTime = DateTime.Now.AddMinutes(10),
      //    Duration = TimeSpan.FromSeconds(317),
      //    LogEvents = new List<ScanningLogEvent>(),
      //    Options  = new List<ScanOptions>(),
      //    ErrorCount = 0,
      //    DelayMSBetweenVendorWebsiteRequests = 2000,
      //    MaximumScanningErrorCount = 5
      //};

      CheckpointDetails cp = null;
      await Task.Delay(20); // latency
      return cp;
    }

    /// <summary>
    /// Fetch summaries for all pending batches for this vendor.
    /// </summary>
    /// <remarks>
    /// Descending.
    /// </remarks>
    /// <returns></returns>
    public async Task<List<CommitBatchSummary>> GetPendingCommitBatchSummariesAsync(int vendorID)
    {
      var list = new List<CommitBatchSummary>();

      int count = 0;
      dicPendingBatches.TryGetValue(vendorID, out count);

      for (int i = 0; i < count; i++)
      {
        var batch = new CommitBatchSummary
        {
          Id = 1243 + i,
          VendorId = vendorID,
          Store = "InsideFabric",
          Created = DateTime.Now.AddDays(-1),
          BatchType = CommitBatchType.InStock,
          QtySubmitted = 1020,
          SessionStatus = CommitBatchStatus.Pending,
          DateCommitted = null,
          QtyCommitted = null,
          IsProcessing = false,
        };

        list.Add(batch);
      }

      await Task.Delay(20); // latency
      return list;
    }

    /// <summary>
    /// Completed delete the specified batch from SQL.
    /// </summary>
    /// <param name="batchId"></param>
    /// <returns></returns>
    public Task<CommitBatchResult> DeleteBatchAsync(int batchId, int vendorID)
    {
      return Task.FromResult(CommitBatchResult.Successful);
    }

    /// <summary>
    /// Mark the batch in SQL as discarded. Just sets the status.
    /// </summary>
    /// <param name="batchId"></param>
    /// <returns></returns>
    public Task<CommitBatchResult> DiscardBatchAsync(int batchId, int vendorID)
    {
      return Task.FromResult(CommitBatchResult.Successful);
    }

    /// <summary>
    /// Retrieve the details for a single batch. Can be any status.
    /// </summary>
    /// <param name="batchID"></param>
    /// <returns>Null if batchID does not exist.</returns>
    public Task<CommitBatchDetails> GetCommitBatchAsync(int batchID)
    {
      var batch = new CommitBatchDetails
      {
        Id = 1243,
        VendorId = 5,
        Store = "InsideFabric",
        Created = DateTime.Now.AddDays(-1),
        BatchType = CommitBatchType.InStock,
        QtySubmitted = 1020,
        Status = CommitBatchStatus.Pending,
        DateCommitted = null,
        QtyCommitted = null,
        IsCommitted = false,
        IsProcessing = false,
        Log = "This is my log stuff",
        CommitData = null,
        SessionId = Guid.NewGuid(),
      };

      var fake = MakeFakeCommits().Where(e => e.Id == batchID).FirstOrDefault();
      if (fake != null)
        batch.BatchType = fake.BatchType;

      return Task.FromResult(batch);
    }

    #region Fake Data

    private List<CommitBatchSummary> MakeFakeCommits()
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
                    QtySubmitted = 321,
                    SessionStatus = CommitBatchStatus.Pending,
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
                    QtySubmitted = 652,
                    SessionStatus = CommitBatchStatus.Pending,
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
                    SessionStatus = CommitBatchStatus.Pending,
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
                    BatchType = CommitBatchType.PriceUpdate,
                    QtySubmitted = 1329,
                    SessionStatus = CommitBatchStatus.Discarded,
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
                    QtySubmitted = 238,
                    SessionStatus = CommitBatchStatus.Committed,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },


                // different vendor

                new CommitBatchSummary
                {
                    Id = 200,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-1),
                    BatchType = CommitBatchType.RemovedVariants,
                    QtySubmitted = 89,
                    SessionStatus = CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 201,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-2),
                    BatchType = CommitBatchType.FullUpdate,
                    QtySubmitted = 734,
                    SessionStatus = CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 202,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-3),
                    BatchType = CommitBatchType.Images,
                    QtySubmitted = 100,
                    SessionStatus = CommitBatchStatus.Pending,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },
                new CommitBatchSummary
                {
                    Id = 203,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-4),
                    BatchType = CommitBatchType.NewProducts,
                    QtySubmitted = 1097,
                    SessionStatus = CommitBatchStatus.Discarded,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },

                new CommitBatchSummary
                {
                    Id = 204,
                    VendorId = 5,
                    Store = "InsideFabric",
                    Created  = DateTime.Now.AddDays(-5),
                    BatchType = CommitBatchType.NewVariants,
                    QtySubmitted = 917,
                    SessionStatus = CommitBatchStatus.Committed,
                    DateCommitted = null,
                    QtyCommitted = null,
                    IsProcessing = false,
                },


            };

      return list;
    }

    #endregion
  }
}
