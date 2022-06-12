using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JFFabrics.Metadata;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace JFFabrics.Discovery
{
    public class JFFabricsProductDiscoverer : IProductDiscoverer<JFFabricsVendor>
    {
        private readonly JFFabricsDetailsFileLoader _fileLoader;

        public JFFabricsProductDiscoverer(JFFabricsDetailsFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var products = _fileLoader.LoadData();
            return Task.FromResult(products.Select(x => new DiscoveredProduct(x)).ToList());
        }
    }
}