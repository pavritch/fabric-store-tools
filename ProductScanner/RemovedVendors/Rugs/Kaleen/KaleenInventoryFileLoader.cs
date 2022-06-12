using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Kaleen
{
    public class KaleenInventoryFileLoader
    {
        private readonly IStorageProvider<KaleenVendor> _storageProvider;

        public KaleenInventoryFileLoader(IStorageProvider<KaleenVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Kaleen UPC Code", ScanField.UPC),
            new FileProperty("Kaleen Product SKU Number", ScanField.Ignore),
            new FileProperty("Available Inventory", ScanField.StockCount),
            new FileProperty("COLLECTION", ScanField.Ignore),
            new FileProperty("Design ID", ScanField.Ignore),
            new FileProperty("Color", ScanField.Ignore),
            new FileProperty("Rug Size", ScanField.Ignore),
            new FileProperty("MAP Prices", ScanField.Ignore),
            new FileProperty("MSRP", ScanField.Ignore),
            new FileProperty("ETA", ScanField.Ignore),
        };

        public List<ScanData> LoadData()
        {
            var stockFilePath = Path.Combine(_storageProvider.GetStaticFolder(), "Kaleen Programmed Inventory As On 07-13-2017.xlsx");
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(stockFilePath, Properties, ScanField.UPC, 1, 2);
        }
    }
}