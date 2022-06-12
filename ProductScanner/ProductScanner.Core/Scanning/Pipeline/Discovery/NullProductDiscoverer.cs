using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Discovery
{
    public class NullProductDiscoverer<T> : IProductDiscoverer<T> where T : Vendor
    {
        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            return Task.FromResult(new List<DiscoveredProduct>());
        }
    }
}