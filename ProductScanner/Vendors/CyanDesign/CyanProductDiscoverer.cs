using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace CyanDesign
{
    public class CyanProductDiscoverer : IProductDiscoverer<CyanDesignVendor>
    {
        private readonly CyanFileLoader _fileLoader;

        public CyanProductDiscoverer(CyanFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = _fileLoader.LoadStockData("CyanOnlineData.xlsx");
            return Task.FromResult(products.Select(x => new DiscoveredProduct(x)).ToList());
        }
    }
}