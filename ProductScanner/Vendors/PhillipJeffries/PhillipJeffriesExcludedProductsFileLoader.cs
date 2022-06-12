using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace PhillipJeffries
{
    public class PhillipJeffriesExcludedProductsFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item", ScanField.ItemNumber),
            new FileProperty("Collection", ScanField.Ignore),
            new FileProperty("Color", ScanField.Ignore),
        };

        private readonly IStorageProvider<PhillipJeffriesVendor> _storageProvider;
        public PhillipJeffriesExcludedProductsFileLoader(IStorageProvider<PhillipJeffriesVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadData()
        {
            var staticFilesFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFilesFolder, "banned phillip jeffries items.xlsx"), Properties, ScanField.ItemNumber, 1, 2);
        }
    }
}