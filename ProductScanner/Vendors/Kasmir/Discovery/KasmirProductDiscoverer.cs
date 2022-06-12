using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Kasmir.Discovery
{
    public class KasmirProductDiscoverer : IProductDiscoverer<KasmirVendor>
    {
        private readonly IProductFileLoader<KasmirVendor> _fileLoader;

        public KasmirProductDiscoverer(IProductFileLoader<KasmirVendor> fileLoader)
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