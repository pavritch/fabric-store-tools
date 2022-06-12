using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace ProductScanner.Core.Scanning.Pipeline.Details
{
    public abstract class ProductScraper<T> : IProductScraper<T> where T : Vendor
    {
        protected readonly IPageFetcher<T> PageFetcher;
        protected ProductScraper(IPageFetcher<T> pageFetcher) { PageFetcher = pageFetcher; }

        public abstract Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product);
        public void DisableCaching() { PageFetcher.DisableCaching(); }
    }
}