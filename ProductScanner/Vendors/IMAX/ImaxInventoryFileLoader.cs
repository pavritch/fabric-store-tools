using System.Collections.Generic;
using System.IO;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace IMAX
{
    public class ImaxInventoryFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item #", ScanField.ManufacturerPartNumber),
            new FileProperty("Item Description", ScanField.ProductName),
            new FileProperty("Price", ScanField.Cost),
            new FileProperty("Min Qty", ScanField.MinimumQuantity),
            new FileProperty("DX?", ScanField.Ignore),
            new FileProperty("UPC Code", ScanField.UPC),
            new FileProperty("Bar Code", ScanField.Ignore),
            new FileProperty("Weight", ScanField.Weight),
            new FileProperty("Dimensions", ScanField.Dimensions),
            new FileProperty("UPS-Able", ScanField.Ignore),
            new FileProperty("Item Material", ScanField.Material),
            new FileProperty("Country of Origin", ScanField.Country),
            new FileProperty("Food Safe?", ScanField.FoodSafe),
            new FileProperty("Outdoor Safe?", ScanField.Ignore),
            new FileProperty("KD?", ScanField.Ignore),
        };

        private readonly IStorageProvider<ImaxVendor> _storageProvider;
        public ImaxInventoryFileLoader(IStorageProvider<ImaxVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadInventoryData()
        {
            var cacheFilesFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            var products = fileLoader.Load(Path.Combine(cacheFilesFolder, "SSI 102317.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
            products.ForEach(x => x.Cost = x[ScanField.Cost].ToDecimalSafe());
            return products;
        }
    }
}