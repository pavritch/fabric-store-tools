using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Greenhouse.Discovery
{
    public class GreenhouseProductDiscoverer : IProductDiscoverer<GreenhouseVendor>
    {
        private readonly GreenhouseJSONSearcher _greenhouseSearcher;

        public GreenhouseProductDiscoverer(GreenhouseJSONSearcher greenhouseSearcher)
        {
            _greenhouseSearcher = greenhouseSearcher;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var mpns = await _greenhouseSearcher.Search();
            return mpns.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}