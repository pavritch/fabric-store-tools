using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Discovery
{
    public interface IProductDiscoverer<T> : IProductDiscoverer where T : Vendor
    {

    }
    public interface IProductDiscoverer
    {
        // contains any data that we found during discovery
        Task<List<DiscoveredProduct>> DiscoverProductsAsync();
    }
}