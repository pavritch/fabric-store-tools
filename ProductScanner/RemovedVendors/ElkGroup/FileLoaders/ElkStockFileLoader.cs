using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace ElkGroup.FileLoaders
{
    public class ElkStockFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item Number", ScanField.ManufacturerPartNumber),
            new FileProperty("Item Class Code", ScanField.Ignore),
            new FileProperty("Actual Inventory", ScanField.StockCount),
            new FileProperty("Expected Qty", ScanField.Ignore),
            new FileProperty("Expected Ship Date to End Consumer", ScanField.Ignore),
            new FileProperty("Container Number", ScanField.Ignore),
            new FileProperty("2nd Expected qty", ScanField.Ignore),
            new FileProperty("2nd Expected date in New York", ScanField.Ignore),
            new FileProperty("2nd Container Number", ScanField.Ignore),
            new FileProperty("Discontinued", ScanField.IsDiscontinued),
            new FileProperty("UPC#", ScanField.UPC),
            new FileProperty("Item Desc Complete", ScanField.Ignore),
        };

        private readonly IStorageProvider<ElkGroupVendor> _storageProvider;
        public ElkStockFileLoader(IStorageProvider<ElkGroupVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "ElkGroup_Stock.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}