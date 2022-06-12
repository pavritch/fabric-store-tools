using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Chandra
{
    public class ChandraProductDiscoverer : IProductDiscoverer<ChandraVendor>
    {
        private readonly ChandraProductFileLoader _fileLoader;

        public ChandraProductDiscoverer(ChandraProductFileLoader fileLoader)
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