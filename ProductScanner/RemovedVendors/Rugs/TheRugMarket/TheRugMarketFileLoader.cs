using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace TheRugMarket
{
    public class TheRugMarketFileLoader : ProductFileLoader<TheRugMarketVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item", ScanField.ManufacturerPartNumber),
            new FileProperty("Description", ScanField.PatternName),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("Shipping Dimension", ScanField.ShippingMethod),
            new FileProperty("Weight", ScanField.Weight),
            new FileProperty("Shipping via LTL", ScanField.Ignore),
            new FileProperty("Material/Fiber Content", ScanField.Material),
            new FileProperty("Quality", ScanField.Construction),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("WHOLESALE RATE", ScanField.Cost),
            new FileProperty("MAPP price", ScanField.MAP),
            // getting stock straight from the website
            new FileProperty("Ready Stock", ScanField.Ignore),
            new FileProperty("Incoming stocks 30-90 days", ScanField.Ignore),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("WEBSITE  URL", ScanField.ImageUrl),
        };

        public TheRugMarketFileLoader(IStorageProvider<TheRugMarketVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, headerRow:2, startRow:4) { }

        // to get a pattern number, take off any letters at the end
        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var product in products)
            {
                product[ScanField.PatternNumber] = GetPatternNumber(product[ScanField.ManufacturerPartNumber]);
                product[ScanField.Bullet1] = product[ScanField.ManufacturerPartNumber]
                    .Replace(product[ScanField.PatternNumber], "");
                product[ScanField.ShippingMethod] = product[ScanField.ShippingMethod].Replace("1010", "10x10");
            }
            return products.Where(x => 
                x[ScanField.ManufacturerPartNumber] != "ACTIVE RUGS" &&
                x[ScanField.ManufacturerPartNumber] != "ITEMS THAT ARE CLOSE OUT WITH STOCKS." &&
                x[ScanField.ManufacturerPartNumber] != "NEW INTRODUCTIONS"
                )
                .ToList();
        }

        private string GetPatternNumber(string mpn)
        {
            var suffixes = new[] {"A$", "B$", "C$", "D$", "E$", "F$", "H$", "Q$", "R$", 
                "S$", "W$", "BS$", "QX$", "RX$", "RL$", "WX$", "W*$"};
            return suffixes.Aggregate(mpn, (current, suffix) => current.RemovePattern(suffix));
        }
    }
}