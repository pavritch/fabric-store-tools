using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace TheRugMarket
{
    public class TheRugMarketProductDiscoverer : IProductDiscoverer<TheRugMarketVendor>
    {
        private readonly IProductFileLoader<TheRugMarketVendor> _fileLoader;

        public TheRugMarketProductDiscoverer(IProductFileLoader<TheRugMarketVendor> fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var fileProducts = await _fileLoader.LoadProductsAsync();
            return fileProducts.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}