using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Loloi
{
    public class LoloiProductDiscoverer : IProductDiscoverer<LoloiVendor>
    {
        private readonly LoloiDetailsFileLoader _fileLoader;

        public LoloiProductDiscoverer(LoloiDetailsFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            return Task.FromResult(_fileLoader.LoadInventoryData().Select(x => new DiscoveredProduct(x)).ToList());
        }
    }
}