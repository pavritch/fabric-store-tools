using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;
using WinfieldThybony.Metadata;

namespace WinfieldThybony.Discovery
{
    public class WinfieldThybonyProductDiscoverer : IProductDiscoverer<WinfieldThybonyVendor>
    {
        private readonly WinfieldThybonyFileLoader _fileLoader;

        public WinfieldThybonyProductDiscoverer(WinfieldThybonyFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = await _fileLoader.LoadProductsAsync();
            return products.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}
