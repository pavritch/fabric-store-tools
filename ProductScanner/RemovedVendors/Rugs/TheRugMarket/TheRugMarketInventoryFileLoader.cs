using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace TheRugMarket
{
    public class TheRugMarketInventoryFileLoader
    {
        private readonly IStorageProvider<TheRugMarketVendor> _storageProvider;

        public TheRugMarketInventoryFileLoader(IStorageProvider<TheRugMarketVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item", ScanField.ManufacturerPartNumber),
            new FileProperty("qty on hand", ScanField.StockCount),
            new FileProperty("qty backordered", ScanField.Ignore),
            new FileProperty("qty on order", ScanField.Ignore),
            new FileProperty("closeout", ScanField.IsClearance),
            new FileProperty("item discontinued", ScanField.IsDiscontinued),
            new FileProperty("Description", ScanField.ProductName),
        };

        public List<ScanData> LoadStockData()
        {
            var stockFilePath = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(stockFilePath, "TheRugMarket_Inventory.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}