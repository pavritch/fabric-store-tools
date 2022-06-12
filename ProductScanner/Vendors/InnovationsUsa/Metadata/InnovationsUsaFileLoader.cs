using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace InnovationsUsa.Metadata
{
    public class InnovationsUsaFileLoader : ProductFileLoader<InnovationsUsaVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Product", ScanField.PatternName),
            new FileProperty("Net Price", ScanField.Cost),
            new FileProperty("UK Purchase Price $/meter", ScanField.Ignore),
            new FileProperty("UK Sale Price $/meter", ScanField.Ignore),
            new FileProperty("Stock Location", ScanField.Ignore),
            new FileProperty("Item Width", ScanField.Ignore),
            new FileProperty("Roll Size", ScanField.Length),
            new FileProperty("Wt/yd", ScanField.Weight),
            new FileProperty("Cut Fee", ScanField.CutFee),
            new FileProperty("Min. Order", ScanField.MinimumOrder),
            new FileProperty("Content", ScanField.Ignore),
            new FileProperty("Repeat", ScanField.Ignore),
        };

        public InnovationsUsaFileLoader(IStorageProvider<InnovationsUsaVendor> storageProvider)
            : base(storageProvider, ScanField.PatternName, Properties, headerRow:7, startRow:8) { } 

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var product in products)
            {
                var pattern = product[ScanField.PatternName].Replace("+", "").Replace("*", "").Trim();
                product[ScanField.UnitOfMeasure] = UnitOfMeasure.Yard.ToString();
                product[ScanField.Cost] = product[ScanField.Cost].Replace("$", "").Replace("/roll", "").Replace("/tile", "");

                var patternName = product[ScanField.PatternName];
                if (patternName.Contains("**"))
                {
                    product[ScanField.UnitOfMeasure] = UnitOfMeasure.Roll.ToString();
                }
                else if (patternName.Contains("*"))
                {
                    product[ScanField.OrderIncrement] = 4.ToString();
                }
                product[ScanField.PatternName] = pattern;
            }
            return products;
        }
    }
}