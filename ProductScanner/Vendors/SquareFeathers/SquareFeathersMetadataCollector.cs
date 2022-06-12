using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace SquareFeathers
{
    public class SquareFeathersMetadataCollector : IMetadataCollector<SquareFeathersVendor>
    {
        private readonly IProductFileLoader<SquareFeathersVendor> _fileLoader;

        public SquareFeathersMetadataCollector(IProductFileLoader<SquareFeathersVendor> fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var priceData = await _fileLoader.LoadProductsAsync();
            foreach (var product in products)
            {
                foreach (var variant in product.Variants)
                {
                    var key = product[ScanField.ManufacturerPartNumber].Replace(" ", "").ToLower() + variant[ScanField.Size];
                    var match = priceData.SingleOrDefault(x => x[ScanField.ProductName] == key);
                    if (match != null)
                    {
                        variant.Cost = match[ScanField.Cost].Replace("$", "").ToDecimalSafe();
                    }
                    else
                    {
                        variant.Cost = variant[ScanField.RetailPrice].ToDecimalSafe() /2.25m;
                    }
                }
            }
            return products;
        }
    }
}