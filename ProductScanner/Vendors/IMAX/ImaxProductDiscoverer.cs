using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace IMAX
{
    public class ImaxProductDiscoverer : IProductDiscoverer<ImaxVendor>
    {
        private readonly ImaxInventoryFileLoader _fileLoader;

        public ImaxProductDiscoverer(ImaxInventoryFileLoader fileLoader)
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