using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Fabricut.Discovery
{
    public class FabricutStockFileLoader<T> where T : Vendor
    {
        private readonly IStorageProvider<T> _storageProvider;

        public FabricutStockFileLoader(IStorageProvider<T> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("FABRIC NO", ScanField.ManufacturerPartNumber),
            new FileProperty("STATUS", ScanField.Status),
            new FileProperty("BOLT/CASE NO", ScanField.Ignore),
            new FileProperty("BOLT/CASE B/C", ScanField.Ignore),
            new FileProperty("BOLT/CASE QOH", ScanField.StockCount),
            new FileProperty("STORE PRICE", ScanField.Cost),
            new FileProperty("RETAIL PRICE", ScanField.RetailPrice),
        };

        public List<ScanData> LoadStockData()
        {
            var stockFilePath = _storageProvider.GetStockFileCachePath(ProductFileType.Xlsx);
            var stockFileLoader = new ExcelFileLoader();
            var stockData = stockFileLoader.Load(stockFilePath, Properties, ScanField.ManufacturerPartNumber, 1, 2);
            var groupedStockRecords = stockData.GroupBy(x => x[ScanField.ManufacturerPartNumber]).Select(x => new ScanData
            {
                {ScanField.ManufacturerPartNumber, x.Key.PadLeft(7, '0')},
                {ScanField.Status, x.First()[ScanField.Status]},
                {ScanField.StockCount, x.Sum(a => a[ScanField.StockCount].ToDoubleSafe()).ToString()},
                // they should all be the same per MPN, so we can just take the first for the prices
                {ScanField.Cost, x.First()[ScanField.Cost]},
                {ScanField.RetailPrice, x.First()[ScanField.RetailPrice]}
            });
            return groupedStockRecords.ToList();
        }
    }
}