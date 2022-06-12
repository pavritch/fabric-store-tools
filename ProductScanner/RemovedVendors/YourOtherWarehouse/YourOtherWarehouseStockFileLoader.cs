using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace YourOtherWarehouse
{
    public class YourOtherWarehouseStockFileLoader
    {
        private readonly IStorageProvider<YourOtherWarehouseVendor> _storageProvider;

        public YourOtherWarehouseStockFileLoader(IStorageProvider<YourOtherWarehouseVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("ItemNumber", ScanField.ManufacturerPartNumber),
            new FileProperty("ProductNumber", ScanField.Ignore),
            new FileProperty("ItemDescription", ScanField.Ignore),
            new FileProperty("QuantityAvailable", ScanField.StockCount),
            new FileProperty("ListPrice", ScanField.RetailPrice),
            new FileProperty("Discount", ScanField.Ignore),
            new FileProperty("NetPrice", ScanField.Cost),
        };

        public List<ScanData> LoadStockData()
        {
            var stockFilePath = _storageProvider.GetProductsFileStaticPath(ProductFileType.Xlsx).Replace("_Price", "_Stock");
            var stockFileLoader = new ExcelFileLoader();
            return stockFileLoader.Load(stockFilePath, Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}