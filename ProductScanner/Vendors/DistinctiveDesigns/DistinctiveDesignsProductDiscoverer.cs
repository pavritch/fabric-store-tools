using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Discovery;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace DistinctiveDesigns
{
    public class DistinctiveDesignsProductDiscoverer : IProductDiscoverer<DistinctiveDesignsVendor>
    {
        private readonly IProductFileLoader<DistinctiveDesignsVendor> _fileLoader;
        private readonly DistinctiveDesignsDiscontinuedFileLoader _discontinuedFileLoader;

        public DistinctiveDesignsProductDiscoverer(IProductFileLoader<DistinctiveDesignsVendor> fileLoader, 
            DistinctiveDesignsDiscontinuedFileLoader discontinuedFileLoader)
        {
            _fileLoader = fileLoader;
            _discontinuedFileLoader = discontinuedFileLoader;
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync()
        {
            var fileProducts = await _fileLoader.LoadProductsAsync();

            var discontinued = _discontinuedFileLoader.LoadDiscontinued();
            foreach (var product in fileProducts)
            {
                var match = discontinued.Any(x => x[ScanField.ItemNumber] == product[ScanField.ItemNumber]);
                if (match)
                {
                    product.IsDiscontinued = true;
                }
            }

            return fileProducts.Select(x => new DiscoveredProduct(x)).ToList();
        }
    }
}