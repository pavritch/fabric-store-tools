using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace JFFabrics.Metadata
{
    public class JFFabricsFileLoader : ProductFileLoader<JFFabricsVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("PatternName", ScanField.PatternName),
            new FileProperty("Cost", ScanField.Cost),
            new FileProperty("Group", ScanField.ProductGroup),
        };

        public JFFabricsFileLoader(IStorageProvider<JFFabricsVendor> storageProvider)
            : base(storageProvider, ScanField.PatternName, Properties) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x => x[ScanField.Cost] = x[ScanField.Cost].Replace(" EACH", ""));
            return products;
        }
    }
}