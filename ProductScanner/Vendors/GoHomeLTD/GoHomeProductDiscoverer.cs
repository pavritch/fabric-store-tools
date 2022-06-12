using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace GoHomeLTD
{
    public class GoHomeProductDiscoverer : IProductDiscoverer<GoHomeVendor>
    {
        private readonly GoHomeSearcher _searcher;

        public GoHomeProductDiscoverer(GoHomeSearcher searcher)
        {
            _searcher = searcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var allProducts = await _searcher.SearchAll();
            return allProducts.Distinct().Select(x => new DiscoveredProduct(x.ToString())).ToList();
        }
    }
}