using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Details
{


    public interface IProductScraper<T> : IProductScraper where T : Vendor
    {
    }

    public interface IProductScraper
    {
        // not sure what data we'll need to pass in - will be different from vendor to vendor...
        // also, even though this is for scanning a single URL, we could return multiple products
        // different colorways, variants, etc...
        Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product);
        void DisableCaching();
    }
}