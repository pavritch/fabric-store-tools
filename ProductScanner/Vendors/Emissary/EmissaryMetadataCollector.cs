using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Emissary
{
    public class EmissaryMetadataCollector : IMetadataCollector<EmissaryVendor>
    {
        private readonly IProductFileLoader<EmissaryVendor> _productFileLoader;

        public EmissaryMetadataCollector(IProductFileLoader<EmissaryVendor> productFileLoader)
        {
            _productFileLoader = productFileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var fileProducts = await _productFileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                var match = fileProducts.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.SKU].Replace("SKU ", ""));
                if (match != null)
                {
                    product[ScanField.StockCount] = match[ScanField.StockCount];
                    product[ScanField.ShippingMethod] = match[ScanField.ShippingMethod];
                    product.Cost = match[ScanField.Cost].ToDecimalSafe();
                }
            }
            return products;
        }
    }
}