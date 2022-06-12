using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InsideFabric.Data;
using ProductScanner.Core.DataInterfaces;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Correlators;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Reports;
using ProductScanner.Core.Scanning.Storage;
using ProductScanner.Core.Sessions;
using Utilities;
using Utilities.Extensions;
using Container = SimpleInjector.Container;

namespace ProductScanner.Core.Scanning
{


    /// <summary>Provides a task scheduler that dedicates a thread per task.</summary>
    public class ThreadPerTaskScheduler : TaskScheduler
    {
        private static object lockObj = new object();

        private static ThreadPerTaskScheduler _default = null;
        public static TaskScheduler ScannerDefault
        {
            get
            {
                lock (lockObj)
                {
                    if (_default == null)
                        _default = new ThreadPerTaskScheduler();

                    return _default;
                }
            }
        }

        /// <summary>Gets the tasks currently scheduled to this scheduler.</summary>
        /// <remarks>This will always return an empty enumerable, as tasks are launched as soon as they're queued.</remarks>
        protected override IEnumerable<Task> GetScheduledTasks() { return Enumerable.Empty<Task>(); }

        /// <summary>Starts a new thread to process the provided task.</summary>
        /// <param name="task">The task to be executed.</param>
        protected override void QueueTask(Task task)
        {
            new Thread(() => TryExecuteTask(task))
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal,
            }
            .Start();
        }

        /// <summary>Runs the provided task on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Ignored.</param>
        /// <returns>Whether the task could be executed on the current thread.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }
    }


    /// <summary>
    /// The results which can be expected back from calling one of the 4 primary scanning entry points (start, resume, suspend, cancel).
    /// </summary>
    /// <remarks>
    /// The description is the human readable text to be displayed in UX.
    /// </remarks>
    public enum ScanningActionResult
    {
        /// <summary>
        /// When no result just yet.
        /// </summary>
        [Description("None")]
        None,

        [Description("Successful")]
        Success,

        [Description("Failed")]
        Failed
    }

    public interface IVendorRunnerMediator
    {
        IVendorRunner GetVendorRunner(Vendor vendor);
    }

    public sealed class VendorRunnerMediator : IVendorRunnerMediator
    {
        private readonly Container _container;
        public VendorRunnerMediator(Container container)
        { 
            _container = container;
        }

        public IVendorRunner GetVendorRunner(Vendor vendor)
        {
            var monitorType = typeof (IVendorRunner<>).MakeGenericType(vendor.GetType());
            return (IVendorRunner)_container.GetInstance(monitorType);
        }
    }

    public interface IVendorRunner
    {
        void Start(ScanOptions options);
        void Suspend();
        Task Cancel();
        void Resume();
    }

    public interface IVendorRunner<T> : IVendorRunner where T : Vendor
    {
    }

    public class VendorRunner<T> : IVendorRunner<T> where T : Vendor, new()
    {
        private readonly IVendorScanner<T> _vendorScanner;
        private readonly ICommitDataBuilder<T> _commitDataBuilder;
        private readonly ICommitValidator<T> _commitValidator;
        private readonly IStoreDatabaseFactory<T> _storeDatabaseFactory;
        private readonly ICommitSubmitter<T> _commitSubmitter;
        private readonly IAuditFileCreator<T> _auditFileCreator;
        private readonly ICorrelatorSetter<T> _correlatorSetter;
        private readonly IVendorScanSessionManager<T> _sessionManager;
        private readonly IProductBuilder<T> _productBuilder;
        private readonly IVariantMerger<T> _variantMerger;
        private readonly IProductValidator<T> _productValidator;
        private readonly IVariantValidator<T> _variantValidator;
        private readonly IStorageProvider<T> _storageProvider; 

        public VendorRunner(IVendorScanner<T> vendorScanner, ICommitDataBuilder<T> commitDataBuilder, ICommitValidator<T> commitValidator,
            IStoreDatabaseFactory<T> storeDatabaseFactory, ICommitSubmitter<T> commitSubmitter, 
            IAuditFileCreator<T> auditFileCreator, ICorrelatorSetter<T> correlatorSetter, 
            IVendorScanSessionManager<T> sessionManager, IProductBuilder<T> productBuilder, IVariantMerger<T> variantMerger, 
            IProductValidator<T> productValidator, IVariantValidator<T> variantValidator, IStorageProvider<T> storageProvider)
        {
            _vendorScanner = vendorScanner;
            _commitDataBuilder = commitDataBuilder;
            _commitValidator = commitValidator;
            _storeDatabaseFactory = storeDatabaseFactory;
            _commitSubmitter = commitSubmitter;
            _auditFileCreator = auditFileCreator;
            _correlatorSetter = correlatorSetter;
            _sessionManager = sessionManager;
            _productBuilder = productBuilder;
            _variantMerger = variantMerger;
            _productValidator = productValidator;
            _variantValidator = variantValidator;
            _storageProvider = storageProvider;
        }

        private void CheckStaticDataVersion()
        {
            var vendor = new T();
            var fileVersion = _storageProvider.GetStaticDataVersion();
            var dllVersion = vendor.StaticFileVersion;
            if (vendor.UsesStaticFiles && fileVersion != dllVersion)
                throw new StaticVersionMismatchException(fileVersion, dllVersion);
        }

        public void Start(ScanOptions options)
        {
            Task.Factory.StartNew(async () =>
            {
                var failure = "";
                try
                {
                    CheckStaticDataVersion();
                    var vendorProducts = await _vendorScanner.Scan(options);
                    await ProcessScanResultsAsync(vendorProducts);
                    _sessionManager.NotifyScanFinished();
                }
                catch (OperationCanceledException)
                {
                }
                catch (StaticVersionMismatchException e)
                {
                    failure = e.Message;
                }
                catch (Exception e)
                {
                    failure = e.Message;
                    e.WriteEventLog();
                }
                if (!string.IsNullOrWhiteSpace(failure))
                {
                    _sessionManager.Log(EventLogRecord.Error(failure));
                    _sessionManager.NotifyScanFailed();
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);
        }

        public void Resume()
        {
            Task.Factory.StartNew(async () =>
            {
                var failure = "";
                try
                {
                    var vendorProducts = await _vendorScanner.Resume();
                    await ProcessScanResultsAsync(vendorProducts);

                    _sessionManager.Log(new EventLogRecord("Duration: {0}", _sessionManager.GetTotalDuration()));
                    _sessionManager.NotifyScanFinished();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    failure = e.Message;
                    e.WriteEventLog();
                }
                if (!string.IsNullOrWhiteSpace(failure))
                {
                    _sessionManager.Log(EventLogRecord.Error(failure));
                    _sessionManager.NotifyScanFailed();
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, ThreadPerTaskScheduler.ScannerDefault);
        }

        // need to rethink this - they've already been excluded from variants (look at GetPopulatedVariants)
        private List<VendorVariant> ReportWarnings(List<VendorVariant> variants)
        {
            //variants.Where(x => x.HasLowCost()).Take(100).ForEach(x => _sessionManager.Log(EventLogRecord.Warn("Excluded for low cost: {0} ({1})", x.ManufacturerPartNumber, x.Cost)));
            //variants.Where(x => x.HasRetailLowerThanOurPrice()).Take(100).ForEach(x => _sessionManager.Log(EventLogRecord.Warn("Has retail price lower than our price: {0} ({1} < {2})", x.ManufacturerPartNumber, x.RetailPrice, x.OurPrice)));
            //variants.Where(x => x.HasOurPriceLowerThanCost()).Take(100).ForEach(x => _sessionManager.Log(EventLogRecord.Warn("Has our price lower than cost: {0} ({1} < {2})", x.ManufacturerPartNumber, x.RetailPrice, x.OurPrice)));
            //variants.Where(x => !x.HasUnitOfMeasure()).Take(100).ForEach(x => _sessionManager.Log(EventLogRecord.Warn("Missing Unit of Measure: " + x.ManufacturerPartNumber)));
            //variants.Where(x => !x.HasProductGroup()).Take(100).ForEach(x => _sessionManager.Log(EventLogRecord.Warn("Missing Product Group: " + x.ManufacturerPartNumber)));

            var excludedReasons = variants.SelectMany(x => x.GetExcludedReasons()).ToList();
            var lowCostCount = excludedReasons.Count(x => x == ExcludedReason.HasLowCost);
            var lowRetailPrice = excludedReasons.Count(x => x == ExcludedReason.HasRetailLowerThanOurPrice);
            var lowOurPrice = excludedReasons.Count(x => x == ExcludedReason.HasOurPriceLowerThanCost);
            var noWholesalePrice = excludedReasons.Count(x => x == ExcludedReason.MissingCost);
            var noUnitOfMeasure = excludedReasons.Count(x => x == ExcludedReason.MissingUnitOfMeasure);
            var noProductGroup = excludedReasons.Count(x => x == ExcludedReason.MissingProductGroup);
            var missingImages = excludedReasons.Count(x => x == ExcludedReason.MissingImage);

            _sessionManager.Log(new EventLogRecord(lowCostCount > 0 ? EventType.Warning : EventType.General, "Total Excluded for low cost: {0:N0}", lowCostCount));
            _sessionManager.Log(new EventLogRecord(lowRetailPrice > 0 ? EventType.Warning : EventType.General, "Total Excluded for low retail price: {0:N0}", lowRetailPrice));
            _sessionManager.Log(new EventLogRecord(lowOurPrice > 0 ? EventType.Warning : EventType.General, "Total Excluded for low our price: {0:N0}", lowOurPrice));
            _sessionManager.Log(new EventLogRecord(noWholesalePrice > 0 ? EventType.Warning : EventType.General, "Total Missing Cost: {0:N0}", noWholesalePrice));
            _sessionManager.Log(new EventLogRecord(noUnitOfMeasure > 0 ? EventType.Warning : EventType.General, "Total Missing Unit of Measure: {0:N0}", noUnitOfMeasure));
            _sessionManager.Log(new EventLogRecord(noProductGroup > 0 ? EventType.Warning : EventType.General, "Total Missing Product Group: {0:N0}", noProductGroup));
            _sessionManager.Log(new EventLogRecord(missingImages > 0 ? EventType.Warning : EventType.General, "Total Missing Images: {0:N0}", missingImages));

            return variants;
        }

        private async Task ProcessScanResultsAsync(List<ScanData> scanData)
        {
            var vendor = new T();
            var storeDatabase = _storeDatabaseFactory.GetStoreDatabase();

            _sessionManager.VendorSessionStats.CurrentTask = "Loading SQL products";
            var progress = new Progress<RetrieveStoreProductsProgress>(p =>
            {
                _sessionManager.VendorSessionStats.TotalItems = p.CountTotal;
                _sessionManager.VendorSessionStats.CompletedItems = p.CountCompleted;
            });

            var sqlProducts = new List<StoreProduct>();
            if (!_sessionManager.HasFlag(ScanOptions.SimulateZeroStoreProducts))
            {
                sqlProducts = await storeDatabase.GetProductsAsync(vendor.Id, _sessionManager.GetCancellationToken(), progress);
            }

            _sessionManager.Log(new EventLogRecord("Total SQL Products: {0:N0}", sqlProducts.Count));

            if (scanData.Any(x => x.RelatedProducts.Any()))
            {
                _correlatorSetter.SetCorrelators(scanData, sqlProducts);
            }

            _auditFileCreator.BuildPropertyAuditFile(scanData);
            _auditFileCreator.BuildCSVRawAnalysisFile(scanData);

            var mergedProducts = _variantMerger.MergeVariants(scanData);

            var vendorProducts = new List<VendorProduct>();
            _sessionManager.ForEachNotify("Building products", mergedProducts, data => vendorProducts.Add(_productBuilder.Build(data)));

            var productValidationResults = vendorProducts.Select(x => _productValidator.ValidateProduct(x)).ToList();
            var validProducts = productValidationResults.Where(x => x.IsValid()).Select(x => x.Product).ToList();
            var invalidProducts = productValidationResults.Where(x => !x.IsValid()).ToList();

            var variants = validProducts.SelectMany(x => x.GetPopulatedVariants()).ToList();
            var variantValidationResults = variants.Select(x => _variantValidator.ValidateVariant(x)).ToList();
            var validVariants = variantValidationResults.Where(x => x.IsValid()).Select(x => x.Variant).ToList();
            var invalidVariants = variantValidationResults.Where(x => !x.IsValid()).ToList();

            // remove invalid variants from valid products
            foreach (var product in validProducts)
            {
                var variantsToRemove = invalidVariants.Where(x => x.Variant.VendorProduct == product).ToList();
                product.RemoveVariants(variantsToRemove.Select(x => x.Variant).ToList());
            }

            _sessionManager.Log(new EventLogRecord(EventType.Warning, "Total Excluded Products: {0:N0}", invalidProducts.Count));

            if (vendor.Store == StoreType.InsideAvenue)
            {
                _sessionManager.Log(new EventLogRecord(EventType.Warning, "Uncategorized Products: {0:N0}",
                    invalidProducts.Count(x => x.ExcludedReasons.Contains(ExcludedReason.HomewareCategoryUnknown))));
            }

            _auditFileCreator.BuildValidationResultsFile(invalidProducts);
            _auditFileCreator.BuildValidationResultsFile(invalidVariants);

            /*
            // remove products that only have samples (mainly in the context of rugs)
            vendorProducts = vendorProducts.Where(x => !x.HasOnlySample()).ToList();
            */

            _auditFileCreator.BuildCSVFinalAnalysisFile(validVariants);

            _sessionManager.VendorSessionStats.CurrentTask = "Building products audit file";
            _auditFileCreator.BuildProductAuditFile(validVariants);

            _sessionManager.VendorSessionStats.CurrentTask = "Saving commit data";
            var commit = _commitDataBuilder.Build(validVariants, sqlProducts);

            var validCommit = _commitValidator.Validate(sqlProducts, commit);

            _sessionManager.VendorSessionStats.CurrentTask = "Building new products audit file";
            _auditFileCreator.BuildNewProductsAuditFile(commit.NewVariantsForReport);

            _sessionManager.Log(new EventLogRecord("Total New Products: {0:N0}", commit.NewProducts.Count));
            _sessionManager.Log(new EventLogRecord("New Products Missing Images: {0:N0}", commit.NewProducts.Count(x => !x.ProductImages.Any())));
            _sessionManager.Log(new EventLogRecord("Previously Missing Images Found: {0:N0}", commit.NewlyFoundImages));
            _sessionManager.Log(new EventLogRecord("Total Discontinued Products: {0:N0}", commit.Discontinued.Count));
            _sessionManager.Log(new EventLogRecord("Total Now In Stock Variants: {0:N0}", commit.InStock.Count));
            _sessionManager.Log(new EventLogRecord("Total Now Out of Stock Variants: {0:N0}", commit.OutOfStock.Count));

            if (validCommit)
            {
                await _commitSubmitter.SubmitAsync(commit);
            }
            else
            {
                throw new Exception("Commit validation failed");
            }
            _sessionManager.VendorSessionStats.Clear();
        }

        public async Task Cancel()
        {
            await _vendorScanner.Cancel();
        }

        public void Suspend()
        {
            _vendorScanner.Suspend();
        }
    }
}