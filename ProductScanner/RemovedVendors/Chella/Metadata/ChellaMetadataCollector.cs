using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Chella.Metadata
{
    public class ChellaMetadataCollector : IMetadataCollector<ChellaVendor>
    {
        private readonly IProductFileLoader<ChellaVendor> _fileLoader;
        public ChellaMetadataCollector(IProductFileLoader<ChellaVendor> fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var fileProducts = await _fileLoader.LoadProductsAsync();
            UpdateProductsFromSpreadsheet(products, fileProducts);
            return products;
        }

        private void UpdateProductsFromSpreadsheet(IEnumerable<ScanData> products, List<ScanData> fileProducts)
        {
            foreach (var product in products)
            {
                // find associated data from the excel patterns
                var patternNumber = product[ProductPropertyType.PatternNumber];
                var associatedData = fileProducts.SingleOrDefault(x => x[ProductPropertyType.ItemNumber] == patternNumber);
                if (associatedData != null)
                {
                    var patternName = associatedData[ProductPropertyType.PatternName];
                    product[ProductPropertyType.Content] = associatedData[ProductPropertyType.Content];
                    product[ProductPropertyType.PatternName] = patternName;
                    product[ProductPropertyType.WholesalePrice] = associatedData[ProductPropertyType.WholesalePrice];
                    product[ProductPropertyType.Repeat] = associatedData[ProductPropertyType.Repeat];
                    product[ProductPropertyType.Width] = associatedData[ProductPropertyType.Width];
                }
            }
        }
    }
}