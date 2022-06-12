using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Maxwell.Discovery
{
    public class MaxwellFileLoader : ProductFileLoader<MaxwellVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Pattern", ScanField.PatternName),
            new FileProperty("Book(s)", ScanField.Book),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("Repeat", ScanField.Repeat),
            new FileProperty("Content", ScanField.Content),
            new FileProperty("Price (US)", ScanField.Cost),
        };

        public MaxwellFileLoader(IStorageProvider<MaxwellVendor> storageProvider)
            : base(storageProvider, ScanField.PatternName, Properties) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var product in products)
            {
                product[ScanField.PatternName] = product[ScanField.PatternName].StripNonAlphanumericChararcters();
            }
            return products;
        }
    }
}