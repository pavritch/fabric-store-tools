using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace CurreyCo
{
    public class CurreyDiscontinuedFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item#", ScanField.ItemNumber),
            new FileProperty("Description", ScanField.Ignore),
        };

        private readonly IStorageProvider<CurreyVendor> _storageProvider;
        public CurreyDiscontinuedFileLoader(IStorageProvider<CurreyVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadPriceData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "Currey Disco 2017 by SKU.xlsx"), Properties, ScanField.ItemNumber, 2, 3);
        }
    }
}