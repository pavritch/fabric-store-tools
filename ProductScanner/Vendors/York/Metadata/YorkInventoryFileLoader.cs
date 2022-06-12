using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace York.Metadata
{
    public class YorkInventoryFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item", ScanField.ManufacturerPartNumber),
            new FileProperty("Qty Available", ScanField.StockCount)
        };

        private readonly IStorageProvider<YorkVendor> _storageProvider;
        public YorkInventoryFileLoader(IStorageProvider<YorkVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadInventoryData()
        {
            var csvFile = Path.Combine(_storageProvider.GetCacheFilesFolder(), "York Inventory.CSV");
            return CsvFileLoader.ReadProductsCsvFile(csvFile, Properties);
        }
    }
}