using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Loloi.Previous
{
    public class LoloiProductFileLoader : ProductFileLoader<LoloiVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("ID", ScanField.ItemNumber),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("Disc", ScanField.IsDiscontinued),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("MSRP", ScanField.RetailPrice),
        };

        public LoloiProductFileLoader(IStorageProvider<LoloiVendor> storageProvider)
            : base(storageProvider, ScanField.ItemNumber, Properties, ProductFileType.Xlsx) { }

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products = products.Where(x => x[ScanField.Collection] != string.Empty).ToList();
            return products;
        }
    }
}