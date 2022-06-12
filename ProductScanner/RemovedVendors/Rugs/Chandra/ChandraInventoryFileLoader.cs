using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Chandra
{
    public class ChandraInventoryFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item Code", ScanField.ManufacturerPartNumber),
            new FileProperty("Item Code Description", ScanField.Description),
            new FileProperty("Actual Qty.", ScanField.StockCount),
            new FileProperty("Qty On PO", ScanField.Ignore),
        };

        private readonly IStorageProvider<ChandraVendor> _storageProvider;
        public ChandraInventoryFileLoader(IStorageProvider<ChandraVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadInventoryData()
        {
            var cacheFilesFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(cacheFilesFolder, "Inventory.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}