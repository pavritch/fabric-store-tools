using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Safavieh
{
    public class SafaviehInventoryFileLoader
    {
        private readonly IStorageProvider<SafaviehVendor> _storageProvider;

        public SafaviehInventoryFileLoader(IStorageProvider<SafaviehVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item ID", ScanField.ManufacturerPartNumber),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("Discontinued?", ScanField.IsDiscontinued),
            new FileProperty("Total", ScanField.StockCount),
            new FileProperty("ETA for Inbound Shipment 1", ScanField.Ignore),
            new FileProperty("ETA for Inbound Shipment 2", ScanField.Ignore),
            new FileProperty("ETA Date from Factory", ScanField.Ignore),
        };

        public List<ScanData> LoadStockData()
        {
            var stockFilePath = _storageProvider.GetStockFileCachePath(ProductFileType.Xlsx);
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(stockFilePath, Properties, ScanField.ManufacturerPartNumber, 3, 4);
        }
    }
}