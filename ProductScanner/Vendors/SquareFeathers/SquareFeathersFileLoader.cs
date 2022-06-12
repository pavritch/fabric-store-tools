using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace SquareFeathers
{
    public class SquareFeathersFileLoader : ProductFileLoader<SquareFeathersVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Product", ScanField.ProductName),
            new FileProperty("Price", ScanField.Cost)
        };

        public SquareFeathersFileLoader(IStorageProvider<SquareFeathersVendor> storageProvider)
            : base(storageProvider, ScanField.ProductName, Properties) { }
    }
}