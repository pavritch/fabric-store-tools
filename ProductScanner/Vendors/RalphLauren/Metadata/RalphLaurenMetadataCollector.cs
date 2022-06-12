using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using RalphLauren.Details;

namespace RalphLauren.Metadata
{
    public class RalphLaurenMetadataCollector : IMetadataCollector<RalphLaurenVendor>
    {
        private readonly RalphLaurenPriceFileLoader _fileLoader;

        public RalphLaurenMetadataCollector(RalphLaurenPriceFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var prices = _fileLoader.LoadInventoryData();

            foreach (var product in products)
            {
                var match = prices.SingleOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ManufacturerPartNumber]);
                if (match != null) product[ScanField.MAP] = match[ScanField.MAP];
            }

            // attempt to fill in any missing unit of measure with ones from same pattern number
            var dicPatternNumberLookup = products
                .GroupBy(e => e[ScanField.PatternNumber])
                .ToDictionary(k => k.Key, v => v.ToList());

            foreach (var product in products.Where(e => !e.ContainsKey(ScanField.UnitOfMeasure)))
            {
                // search list of products with same pattern number to see if one might have a unit of measure filled in
                if (!product.ContainsKey(ScanField.ProductGroup))
                    continue;

                var list = dicPatternNumberLookup[product[ScanField.PatternNumber]];

                var otherProduct = list.FirstOrDefault(e => e.ContainsKey(ScanField.UnitOfMeasure) && 
                    e[ScanField.ProductGroup] == product[ScanField.ProductGroup]);
                if (otherProduct != null)
                    product[ScanField.UnitOfMeasure] = otherProduct[ScanField.UnitOfMeasure];
                else
                {
                    // unit of measure wasn't set
                    product[ScanField.UnitOfMeasure] = product[ScanField.ProductGroup] == "Wallpaper" ? "Roll" : "Yard";
                }
            }
            return Task.FromResult(products);
        }
    }
}