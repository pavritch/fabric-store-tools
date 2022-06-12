using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Details
{
    public class NullProductScraper<T> : IProductScraper<T> where T : Vendor
    {
        public Task<List<ScanData>> ScrapeAsync(DiscoveredProduct context)
        {
            return Task.FromResult(new List<ScanData> { context.ScanData });
        }

        public void DisableCaching() { }
    }
}