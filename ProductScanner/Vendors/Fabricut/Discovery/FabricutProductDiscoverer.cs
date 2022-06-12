using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities;
using Utilities.Extensions;

namespace Fabricut.Discovery
{
    public class FabricutProductDiscoverer : FabricutBaseProductDiscoverer<FabricutVendor>
    {
        public FabricutProductDiscoverer(IProductFileLoader<FabricutVendor> productFileLoader, IStorageProvider<FabricutVendor> storageProvider, IPageFetcher<FabricutVendor> pageFetcher) : base(productFileLoader, storageProvider, pageFetcher) { }
    }

    public class SHarrisProductDiscoverer : FabricutBaseProductDiscoverer<SHarrisVendor>
    {
        public SHarrisProductDiscoverer(IProductFileLoader<SHarrisVendor> productFileLoader, IStorageProvider<SHarrisVendor> storageProvider, IPageFetcher<SHarrisVendor> pageFetcher) : base(productFileLoader, storageProvider, pageFetcher) { }
    }

    public class StroheimProductDiscoverer : FabricutBaseProductDiscoverer<StroheimVendor>
    {
        public StroheimProductDiscoverer(IProductFileLoader<StroheimVendor> productFileLoader, IStorageProvider<StroheimVendor> storageProvider, IPageFetcher<StroheimVendor> pageFetcher) : base(productFileLoader, storageProvider, pageFetcher) { }
    }

    public class TrendProductDiscoverer : FabricutBaseProductDiscoverer<TrendVendor>
    {
        public TrendProductDiscoverer(IProductFileLoader<TrendVendor> productFileLoader, IStorageProvider<TrendVendor> storageProvider, IPageFetcher<TrendVendor> pageFetcher) : base(productFileLoader, storageProvider, pageFetcher) { }
    }

    public class VervainProductDiscoverer : FabricutBaseProductDiscoverer<VervainVendor>
    {
        public VervainProductDiscoverer(IProductFileLoader<VervainVendor> productFileLoader, IStorageProvider<VervainVendor> storageProvider, IPageFetcher<VervainVendor> pageFetcher) : base(productFileLoader, storageProvider, pageFetcher) { }
    }

    public class FabricutBaseProductDiscoverer<T> : IProductDiscoverer<T> where T : Vendor, new()
    {
        private const string PriceFileUrl = "ftp://insidestores:fabricutftp@ftp.fabricut.com/pricesd3i.xlsx";
        private const string StockFileUrl = "ftp://insidestores:fabricutftp@ftp.fabricut.com/fabinvenwk.xlsx";
        private readonly IProductFileLoader<T> _productFileLoader;
        private readonly IStorageProvider<T> _storageProvider;
        private readonly IPageFetcher<T> _pageFetcher;

        public FabricutBaseProductDiscoverer(IProductFileLoader<T> productFileLoader, IStorageProvider<T> storageProvider, IPageFetcher<T> pageFetcher)
        {
            _productFileLoader = productFileLoader;
            _storageProvider = storageProvider;
            _pageFetcher = pageFetcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = await Retry.DoAsync(async () =>
            {
                var task1 = Task.Run(() => _pageFetcher.FetchBinaryAsync(PriceFileUrl));
                var task2 = Task.Run(() => _pageFetcher.FetchBinaryAsync(StockFileUrl));
                var tasks = new List<Task> {task1, task2};

                if (Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(60)))
                {
                    _storageProvider.SaveProductsFile(ProductFileType.Xlsx, task1.Result);
                    _storageProvider.SaveStockFile(ProductFileType.Xlsx, task2.Result);

                    var fileProducts = await _productFileLoader.LoadProductsAsync();
                    var productsForVendor = fileProducts.Where(x => x[ScanField.Brand].ContainsIgnoreCase(new T().DisplayName.Replace(" ", ""))).ToList();
                    return productsForVendor.Select(x => new DiscoveredProduct(x)).ToList();
                }
                throw new TimeoutException();
            }, TimeSpan.FromSeconds(60), 5);

            return products;
        }
    }
}