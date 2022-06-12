using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Safavieh
{
    public class SafaviehProductDiscoverer : IProductDiscoverer<SafaviehVendor>
    {
        private readonly IProductFileLoader<SafaviehVendor> _fileLoader;

        public SafaviehProductDiscoverer(IProductFileLoader<SafaviehVendor> fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var variants = await _fileLoader.LoadProductsAsync();
            return variants.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}