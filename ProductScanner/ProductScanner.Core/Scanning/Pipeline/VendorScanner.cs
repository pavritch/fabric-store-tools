using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Checkpoints;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.VendorTesting;
using ProductScanner.Core.VendorTesting.TestTypes;
using ProductScanner.Core.WebClient;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Pipeline
{
    public class VendorScanner<T> : IVendorScanner<T> where T : Vendor, new()
    {
        private readonly IProductDiscoverer<T> _productDiscoverer;
        private readonly IProductScraper<T> _productScraper;
        private readonly IMetadataCollector<T> _metadataCollector;

        private readonly IVendorScanSessionManager<T> _vendorSessionManager;
        private readonly ICheckpointService<T> _checkpointService;
        private readonly IVendorTestRunner<T> _testRunner;

        private DateTime _lastSave = DateTime.MinValue;
        private bool _suspend;

        public VendorScanner(IProductDiscoverer<T> productDiscoverer, IProductScraper<T> productScraper, IMetadataCollector<T> metadataCollector, IVendorScanSessionManager<T> vendorSessionManager, ICheckpointService<T> checkpointService, IVendorTestRunner<T> testRunner)
        {
            _productDiscoverer = productDiscoverer;
            _productScraper = productScraper;
            _metadataCollector = metadataCollector;
            _vendorSessionManager = vendorSessionManager;
            _checkpointService = checkpointService;
            _testRunner = testRunner;
        }

        public async Task<List<ScanData>> Resume()
        {
            var checkpoint = await _checkpointService.GetLatestAsync();
            if (checkpoint == null) throw new InvalidOperationException("Cannot resume when no checkpoint exists");

            // we've already discovered all products
            _vendorSessionManager.ResumeScan(checkpoint);

            await _vendorSessionManager.AuthAsync();

            var checkpointData = checkpoint.CheckpointData.UnGZipMemoryToString().FromJSON() as CheckpointData;
            var products = await ProcessProductsAsync(checkpointData, checkpoint.ScanOptions);
            _vendorSessionManager.Log(new EventLogRecord("Scanned details for {0:N0} products.", products.Count));

            products = await _metadataCollector.PopulateMetadata(products);
            await _checkpointService.RemoveAsync();
            return products;
        }

        public void Suspend()
        {
            if (_vendorSessionManager.CanSuspend) _suspend = true;
            else _vendorSessionManager.Cancel();
        }

        public async Task Cancel()
        {
            await _checkpointService.RemoveAsync();
            _vendorSessionManager.Cancel();
        }

        public async Task<List<ScanData>> Scan(ScanOptions options)
        {
            await _checkpointService.RemoveAsync();
            _vendorSessionManager.StartScan(options);

            _vendorSessionManager.VendorSessionStats.CurrentTask = "Running tests";
            var testResults = await _testRunner.RunAllTests();
            var failedTests = testResults.Where(x => x.Code == TestResultCode.Failed).ToList();
            if (failedTests.Any())
            {
                failedTests.ForEach(x => _vendorSessionManager.Log(EventLogRecord.Error("Test failed: {0}", x.Message)));
                throw new Exception("Vendor testing failed");
            }

            _vendorSessionManager.Log(new EventLogRecord("Tests completed successfully"));

            _vendorSessionManager.CanSuspend = false;
            _vendorSessionManager.Log(new EventLogRecord("Vendor: {0}", new T().DisplayName));
            _vendorSessionManager.Log(new EventLogRecord("Started: {0}", DateTime.Now.ToString()));
            var result = await _vendorSessionManager.AuthAsync(options);
            _vendorSessionManager.VendorSessionStats.CurrentTask = string.Format("Authentication {0}", result ? "successful" : "failed");

            if (!result) throw new AuthenticationException("Authentication failed");

            _vendorSessionManager.VendorSessionStats.CurrentTask = "Discovering products";
            var discoveredProducts = await _productDiscoverer.DiscoverProductsAsync();
            _vendorSessionManager.Log(new EventLogRecord("Discovered {0:N0} products.", discoveredProducts.Count));

            var checkpointData = new CheckpointData(discoveredProducts);

            _vendorSessionManager.VendorSessionStats.CurrentTask = "Scanning product details";
            var products = await ProcessProductsAsync(checkpointData, options);
            _vendorSessionManager.Log(new EventLogRecord("Scanned details for {0:N0} products.", products.Count));
            var totalScanned = products.Count;

            _vendorSessionManager.VendorSessionStats.CurrentTask = "Populating Metadata";
            products = await _metadataCollector.PopulateMetadata(products);

            await _checkpointService.RemoveAsync();
            _vendorSessionManager.Log(new EventLogRecord("Found {0}/{1} valid products...", products.Count, totalScanned));
            return products;
        }

        private bool ShouldSave()
        {
            return ((DateTime.UtcNow - _lastSave).TotalMinutes >= 10);
        }

        private async Task<List<ScanData>> ProcessProductsAsync(CheckpointData checkpointData, ScanOptions options)
        {
            _vendorSessionManager.VendorSessionStats.CurrentTask = "Scraping products";
            _vendorSessionManager.VendorSessionStats.TotalItems = checkpointData.TotalScans;
            _vendorSessionManager.CanSuspend = true;

            while (checkpointData.IsNotDone())
            {
                if (_suspend)
                {
                    await _checkpointService.SaveAsync(checkpointData, options);
                    _vendorSessionManager.NotifyCheckpointSaved(checkpointData);
                    _suspend = false;
                    _vendorSessionManager.NotifyScanSuspended();
                    throw new OperationCanceledException();
                }

                _vendorSessionManager.ThrowIfCancellationRequested();
                if (ShouldSave())
                {
                    await _checkpointService.SaveAsync(checkpointData, options);
                    _vendorSessionManager.NotifyCheckpointSaved(checkpointData);
                    _lastSave = DateTime.UtcNow;
                }

                var authFailed = false;
                var nextProduct = checkpointData.GetNextProduct();
                try
                {
                    var result = await _productScraper.ScrapeAsync(nextProduct);
                    checkpointData.SaveResult(result);
                }
                catch (AuthenticationException)
                {
                    authFailed = true;
                }
                catch (Exception e)
                {
                    _vendorSessionManager.Log(EventLogRecord.Error("Error on {0}: {1}", nextProduct.MPN, e.Message));
                    e.WriteEventLog();
                    checkpointData.SaveResult(new List<ScanData>());
                    _vendorSessionManager.BumpErrorCount();
                    Thread.Sleep(2000);
                }

                if (_vendorSessionManager.HasErrored)
                    await _checkpointService.RemoveAsync();

                if (authFailed)
                {
                    _vendorSessionManager.Log(EventLogRecord.Warn("Authentication Failed. Reauthenticating."));
                    await _vendorSessionManager.Reauthenticate();
                }
                _vendorSessionManager.VendorSessionStats.CompletedItems = checkpointData.GetCountCompleted();
            }
            _vendorSessionManager.CanSuspend = false;
            _vendorSessionManager.VendorSessionStats.CurrentTask = string.Empty;
            return checkpointData.GetResults();
        }

    }
}