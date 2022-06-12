using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace WildWoodLamps
{
    public class WildWoodProductDiscoverer : IProductDiscoverer<WildWoodLampsVendor>
    {
        private readonly WildWoodLampsFileLoader _fileLoader;

        public WildWoodProductDiscoverer(WildWoodLampsFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = _fileLoader.LoadInventoryData();
            return Task.FromResult(products.Select(x => new DiscoveredProduct(x)).ToList());
        }
    }
}