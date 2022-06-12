using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Rizzy
{
    public class RizzyProductDiscoverer : IProductDiscoverer<RizzyVendor>
    {
        private readonly IProductFileLoader<RizzyVendor> _fileLoader;

        public RizzyProductDiscoverer(IProductFileLoader<RizzyVendor> fileLoader)
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