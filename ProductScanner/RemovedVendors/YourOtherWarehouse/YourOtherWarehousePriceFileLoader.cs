using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace YourOtherWarehouse
{
    public class YourOtherWarehousePriceFileLoader
    {
        private readonly IStorageProvider<YourOtherWarehouseVendor> _storageProvider;

        public YourOtherWarehousePriceFileLoader(IStorageProvider<YourOtherWarehouseVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Key", ScanField.Code),
            new FileProperty("Brand", ScanField.Brand),
            new FileProperty("Multiplier", ScanField.Multiplier),
            new FileProperty("Type", ScanField.Type),
        };

        public List<ScanData> LoadStockData()
        {
            var stockFilePath = _storageProvider.GetProductsFileStaticPath(ProductFileType.Xlsx);
            var multiplierFilePath = stockFilePath.Replace("_Price", "_Multipliers");
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(multiplierFilePath, Properties, ScanField.Code, 1, 2);
        }
    }
}