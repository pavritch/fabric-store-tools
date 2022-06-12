using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Products;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Commits
{
    public class BatchCommitter<TVendor> : IBatchCommitter where TVendor : Vendor, new()
    {
        #region BatchCommitterException Class
        private class BatchCommitterException : Exception
        {
            public CommitBatchResult CommitResult { get; set; }

            public BatchCommitterException(CommitBatchResult commitResult)
            {
                this.CommitResult = commitResult;
            }
        }

        #endregion

        #region ProcessingResult Class
        private class ProcessingResult
        {
            public CommitBatchResult Result { get; set; }
            public int QuantityCommitted { get; set; }
            public string Log { get; set; }

            public ProcessingResult()
            {

            }

            public ProcessingResult(CommitBatchResult result, int quantityCommitted = 0)
            {
                this.Result = result;
                this.QuantityCommitted = quantityCommitted;
            }
        }


        #endregion

        #region ProcessingContext Class

        /// <summary>
        /// Keeps track of some common runtime information when processing a collection of batch records.
        /// </summary>
        private class ProcessingContext<TData> 
        {
            private TVendor vendor;
            private StringBuilder log;
            private DateTime timeStarted = DateTime.Now;

            public List<TData> items;
            public CancellationToken cancelToken;
            public IProgress<ActivityProgress> progress;

            public int countSubmitted;
            public int countCommitted;
            public int countErrors;

            public ProcessingContext(TVendor vendor, List<TData> items, string description, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
            {
                log = new StringBuilder();

                this.vendor = vendor;
                this.items = items;

                this.cancelToken = cancelToken;
                this.progress = progress;

                this.countSubmitted = items.Count;
                countCommitted = 0;
                countErrors = 0;

                WriteLog(string.Format("Vendor: {0}", this.vendor.DisplayName));
                WriteLog(description);
                WriteLog(string.Format("Quantity submitted: {0:N0}", countSubmitted));
                WriteLog(string.Format("Time started: {0}", timeStarted));
            }

            public ProcessingResult FinishUp()
            {
                if (cancelToken != null && cancelToken.IsCancellationRequested)
                    return new ProcessingResult(CommitBatchResult.Cancelled);

                var timeCompleted = DateTime.Now;

                WriteLog(string.Format("Time completed: {0}", timeCompleted));
                WriteLog(string.Format("Elapsed time: {0}", timeCompleted - timeStarted));

                WriteLog(string.Format("Quantity submitted: {0:N0}", countSubmitted));
                WriteLog(string.Format("Quantity committed: {0:N0}", countCommitted));
                WriteLog(string.Format("Errors: {0:N0}", countErrors));

                var result = new ProcessingResult()
                {
                    Result = CommitBatchResult.Successful,
                    Log = GetLog(),
                    QuantityCommitted = countCommitted,
                };

                return result;
            }

            public void WriteLog(string s)
            {
                log.AppendLine(s);
            }

            public string GetLog()
            {
                return log.ToString();
            }
        }


        #endregion

        private readonly IPlatformDatabase _platformDatabase;

        // used to figure out which store to use (fabric, rugs, wallpaper) based on the vendor
        private readonly IStoreDatabaseFactory<TVendor> _storeDatabaseFactory;

        public BatchCommitter(IPlatformDatabase database, IStoreDatabaseFactory<TVendor> storeDatabaseFactory)
        {
            _platformDatabase = database;
            _storeDatabaseFactory = storeDatabaseFactory;
        }

        #region Commit Batch Entry Point
        public async Task<CommitBatchResult> CommitBatchAsync(int batchId, bool ignoreDuplicates, CancellationToken cancelToken = default(CancellationToken), IProgress<ActivityProgress> progress = null)
        {
            // if cannot hit SQL (store or scanner) right off, then return DatabaseError.
            // if batch id no longer in scanner SQL, return NotFound
            // if batch id is not for "this vendor", return IncorrectVendor (indicative of a proramming error)
            // if batch status is InProgress, return AccessDenied
            // if batch is not currently pending, return NotPending

            bool isProcessing = false;

            try
            {
                ScannerCommit batch = null;

                try
                {
                    batch = await _platformDatabase.GetCommitBatch(batchId);

                    if (batch == null)
                        return CommitBatchResult.NotFound;

                    if (batch.IsProcessing)
                        return CommitBatchResult.AccessDenied;

                    if (batch.SessionStatus != CommitBatchStatus.Pending)
                        return CommitBatchResult.NotPending;

                    var vendor = new TVendor();
                    if (batch.VendorId != vendor.Id)
                        return CommitBatchResult.IncorrectVendor;
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine(Ex.Message);
                    throw new BatchCommitterException(CommitBatchResult.DatabaseError);
                }

                await _platformDatabase.SetCommitBatchProcessingStatus(batchId, true);
                isProcessing = true;

                string commitJson = batch.CommitData.UnGZipMemoryToString();

                ProcessingResult processingResult = null;

                // handle based on the type of batch

                switch (batch.BatchType)
                {
                    case CommitBatchType.Discontinued:
                        processingResult = await CommitDiscontinued(commitJson.JSONtoList<List<int>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    case CommitBatchType.NewProducts:
                        processingResult = await CommitNewProducts(commitJson.JSONtoList<List<StoreProduct>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    case CommitBatchType.InStock:
                        processingResult = await CommitInStockVariants(commitJson.JSONtoList<List<int>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    case CommitBatchType.OutOfStock:
                        processingResult = await CommitOutOfStockVariants(commitJson.JSONtoList<List<int>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    case CommitBatchType.PriceUpdate:
                        processingResult = await CommitPriceUpdates(commitJson.JSONtoList<List<VariantPriceChange>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    case CommitBatchType.FullUpdate:
                        processingResult = await CommitFullUpdates(commitJson.JSONtoList<List<StoreProduct>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    case CommitBatchType.RemovedVariants:
                        processingResult = await CommitRemovedVariants(commitJson.JSONtoList<List<int>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    case CommitBatchType.NewVariants:
                        processingResult = await CommitNewVariants(commitJson.JSONtoList<List<NewVariant>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    case CommitBatchType.Images:
                        processingResult = await CommitImages(commitJson.JSONtoList<List<ProductImageSet>>(), ignoreDuplicates, cancelToken, progress);
                        break;

                    default:
                        throw new NotImplementedException("Unsupported batch type.");
                }

                // we only update SQL if successful. Otherwise, if cancelled or failed, we simply mark it as not being processed so that
                // the user can determine what to do next, or discard/delete.

                if (processingResult.Result == CommitBatchResult.Successful)
                {
                    // update SQL:
                    //
                    // QtyCommitted
                    // DateCommitted
                    // SessionStatus
                    // Log
                    // IsProcessing

                    await _platformDatabase.UpdateSuccessfullyProcessedCommitBatch(batchId, processingResult.QuantityCommitted, processingResult.Log);
                    isProcessing = false;
                    Debug.WriteLine(processingResult.Log);
                }

                return processingResult.Result;
            }
            catch (BatchCommitterException Ex)
            {
                return Ex.CommitResult;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return CommitBatchResult.Failed;
            }
            finally
            {
                // make sure no matter what we've cleared this flag

                if (isProcessing)
                    _platformDatabase.SetCommitBatchProcessingStatus(batchId, false).Wait();
            }
        } 
        #endregion

        #region ProcessCollection
        /// <summary>
        /// Process a collection of records. Call the provided func for each item, while tracking cancellation and progress.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="dataCollection"></param>
        /// <param name="proccessSingleItem"></param>
        /// <param name="cancelToken"></param>
        /// <param name="progress"></param>
        /// <returns>Number of items successfully committed.</returns>
        private ProcessingResult ProcessCollection<TData>(ProcessingContext<TData> context, Action<TData> proccessSingleItem)
        {
            int countTotal = 0;
            int countCompleted = 0;
            double percentCompleted = 0;
            double lastReportedPercentCompleted = -1;


            Action reportProgress = () =>
            {
                percentCompleted = countTotal == 0 ? 0 : (countCompleted * 100.0) / (double)countTotal;

                if (percentCompleted != lastReportedPercentCompleted && context.progress != null)
                {
                    context.progress.Report(new ActivityProgress(countTotal, countCompleted, percentCompleted));
                    lastReportedPercentCompleted = percentCompleted;
                }
            };

            countTotal = context.countSubmitted;

            // initial report with correct totals and zero progress
            reportProgress();

            foreach (TData item in context.items)
            {
                if (context.cancelToken != null && context.cancelToken.IsCancellationRequested)
                    break;

                // process the item here, the caller deals with pass/fail issues directly in the context, log too.

                // caller can throw exception, in which case the entire operation is failed since all callers
                // have an outer try block.

                proccessSingleItem(item);

                countCompleted++;
                reportProgress();
            }

            return context.FinishUp();
        }
        
        #endregion

        #region Discontinued Products
        private  Task<ProcessingResult> CommitDiscontinued(List<int> productIds, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<int>(vendor, productIds, "Discontinued Products", cancelToken, progress);

                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<int> processProduct = (productID) =>
                    {
                        var res = storeDatabase.MarkProductDiscontinued(productID, new TVendor().Id);

                        if (res == StoreDatabaseUpdateResult.Success || (res == StoreDatabaseUpdateResult.NotFound && ignoreDuplicates))
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on ProductID: {1}", res.DescriptionAttr(), productID));
                        }

                    };

                    var result = ProcessCollection(context, processProduct);
                    tcs.SetResult(result);
                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);
            return tcs.Task;
        } 

        #endregion

        #region New Products
        private Task<ProcessingResult> CommitNewProducts(List<StoreProduct> products, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<StoreProduct>(vendor, products, "New Products", cancelToken, progress);

                    var countProductsMissingImages = products.Where(e => e.ProductImages == null || e.ProductImages.Count() == 0).Count();
                    context.WriteLog(string.Format("New products missing images: {0:N0}", countProductsMissingImages));
                    //Debug.WriteLine(string.Format("Products missing images: {0:N0}", countProductsMissingImages));
                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<StoreProduct> processProduct = (product) =>
                    {
                        var res = storeDatabase.AddProduct(product);
                        if (res == StoreDatabaseUpdateResult.Success || (res == StoreDatabaseUpdateResult.Duplicate && ignoreDuplicates))
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on product: {1}", res.DescriptionAttr(), product.SKU));
                        }

                    };

                    var result = ProcessCollection(context, processProduct);
                    tcs.SetResult(result);

                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;

        }
        #endregion

        #region In Stock Variants
        private Task<ProcessingResult> CommitInStockVariants(List<int> variantIds, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<int>(vendor, variantIds, "In Stock Variants",  cancelToken, progress);

                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<int> processVariant = (variantID) =>
                    {
                        var res = storeDatabase.UpdateProductVariantInventory(variantID, InventoryStatus.InStock); 
                        
                        if (res == StoreDatabaseUpdateResult.Success)
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on VariantID: {1}", res.DescriptionAttr(), variantID));
                        }

                    };

                    var result = ProcessCollection(context, processVariant);
                    tcs.SetResult(result);


                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;
        }

        #endregion

        #region Out of Stock Variants
        private Task<ProcessingResult> CommitOutOfStockVariants(List<int> variantIds, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<int>(vendor, variantIds, "Out of Stock Variants", cancelToken, progress);

                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<int> processVariant = (variantID) =>
                    {
                        var res = storeDatabase.UpdateProductVariantInventory(variantID, InventoryStatus.OutOfStock); 
                        
                        if (res == StoreDatabaseUpdateResult.Success)
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on VariantID: {1}", res.DescriptionAttr(), variantID));
                        }

                    };

                    var result = ProcessCollection(context, processVariant);
                    tcs.SetResult(result);

                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;

        }
        
        #endregion

        #region Price Updates
        private Task<ProcessingResult> CommitPriceUpdates(List<VariantPriceChange> priceUpdates, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<VariantPriceChange>(vendor, priceUpdates, "Update Prices", cancelToken, progress);

                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<VariantPriceChange> processPriceUpdate = (priceUpdate) =>
                    {

                        var res = storeDatabase.UpdateProductVariantPrice(priceUpdate); 
                       
                        if (res == StoreDatabaseUpdateResult.Success)
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on variantID: {1}", res.DescriptionAttr(), priceUpdate.VariantId));
                        }

                    };

                    var result = ProcessCollection(context, processPriceUpdate);
                    tcs.SetResult(result);

                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;

        }

        #endregion

        #region Full Product Updates
        private Task<ProcessingResult> CommitFullUpdates(List<StoreProduct> products, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<StoreProduct>(vendor, products, "Full Product Updates", cancelToken, progress);

                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<StoreProduct> processProduct = (product) =>
                    {
                        var res = storeDatabase.UpdateProduct(product);
                       
                        if (res == StoreDatabaseUpdateResult.Success)
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on product: {1}", res.DescriptionAttr(), product.SKU));
                        }

                    };

                    var result = ProcessCollection(context, processProduct);
                    tcs.SetResult(result);

                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;

        }
        
        #endregion

        #region Removed Variants
        private Task<ProcessingResult> CommitRemovedVariants(List<int> variantIds, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<int>(vendor, variantIds, "Removed Variants", cancelToken, progress);

                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<int> processVariant = (variantID) =>
                    {
                        var res = storeDatabase.RemoveProductVariant(variantID);

                        if (res == StoreDatabaseUpdateResult.Success || (res == StoreDatabaseUpdateResult.NotFound && ignoreDuplicates))
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on VariantID: {1}", res.DescriptionAttr(), variantID));
                        }

                    };

                    var result = ProcessCollection(context, processVariant);
                    tcs.SetResult(result);

                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;

        }
        
        #endregion

        #region New Variants
        private Task<ProcessingResult> CommitNewVariants(List<NewVariant> variants, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<NewVariant>(vendor, variants, "New Variants", cancelToken, progress);

                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<NewVariant> processVariant = (variant) =>
                    {
                        var res = storeDatabase.AddProductVariant(variant.ProductId, variant.StoreProductVariant);

                        if (res == StoreDatabaseUpdateResult.Success || (res == StoreDatabaseUpdateResult.Duplicate && ignoreDuplicates))
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on productID: {1}", res.DescriptionAttr(), variant.ProductId));
                        }

                    };

                    var result = ProcessCollection(context, processVariant);
                    tcs.SetResult(result);

                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;

        }
        #endregion

        #region Images
        private Task<ProcessingResult> CommitImages(List<ProductImageSet> productImages, bool ignoreDuplicates, CancellationToken cancelToken, IProgress<ActivityProgress> progress)
        {
            var tcs = new TaskCompletionSource<ProcessingResult>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var vendor = new TVendor();
                    var context = new ProcessingContext<ProductImageSet>(vendor, productImages, "Update Product Images", cancelToken, progress);

                    var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

                    Action<ProductImageSet> processImages = (imageSet) =>
                    {

                        var res = storeDatabase.UpdateProductImages(imageSet.ProductId, imageSet.ProductImages); ;
                       
                        if (res == StoreDatabaseUpdateResult.Success)
                        {
                            context.countCommitted++;
                        }
                        else
                        {
                            context.countErrors++;
                            context.WriteLog(string.Format("{0} on productID: {1}", res.DescriptionAttr(), imageSet.ProductId));
                        }

                    };

                    var result = ProcessCollection(context, processImages);
                    tcs.SetResult(result);

                }
                catch
                {
                    tcs.SetResult(new ProcessingResult(CommitBatchResult.Failed));
                }

            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);

            return tcs.Task;

        }
        #endregion

    }
}