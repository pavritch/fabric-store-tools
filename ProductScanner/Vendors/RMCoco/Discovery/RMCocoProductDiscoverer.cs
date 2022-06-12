using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using RMCoco.Metadata;

namespace RMCoco.Discovery
{
    public class RMCocoProductDiscoverer : IProductDiscoverer<RMCocoVendor>
    {
        private readonly RMCocoFileLoader _fileLoader;

        public RMCocoProductDiscoverer(RMCocoFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = _fileLoader.LoadPriceData();
            return Task.FromResult(products.Select(x => new DiscoveredProduct(x)).ToList());
        }
    }
}