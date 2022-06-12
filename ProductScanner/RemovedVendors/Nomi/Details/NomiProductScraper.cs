using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.Pipeline.Details;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Nomi.Details
{
    public class NomiProductScraper : ProductScraper<NomiVendor>
    {
        public NomiProductScraper(IPageFetcher<NomiVendor> pageFetcher) : base(pageFetcher) { }
        public override Task<List<ScanData>> ScrapeAsync(DiscoveredProduct product)
        {
            return Task.FromResult(new List<ScanData> {product.ScanData});
        }
    }
}