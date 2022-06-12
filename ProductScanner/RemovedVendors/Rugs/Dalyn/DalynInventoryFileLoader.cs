using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Dalyn
{
    public class DalynInventoryFileLoader
    {
        private readonly IStorageProvider<DalynVendor> _storageProvider;

        public DalynInventoryFileLoader(IStorageProvider<DalynVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item Number", ScanField.ManufacturerPartNumber),
            new FileProperty("Description", ScanField.Description),
            new FileProperty("Quantity", ScanField.StockCount),
            new FileProperty("Design", ScanField.PatternName),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("Status", ScanField.Status),
        };

        public List<ScanData> LoadStockData()
        {
            var stockFilePath = Path.Combine(_storageProvider.GetStaticFolder(), "Dalyn_Inventory.xlsx");
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(stockFilePath, Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}