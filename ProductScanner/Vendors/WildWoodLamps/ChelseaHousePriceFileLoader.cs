using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace WildWoodLamps
{
    public class ChelseaHousePriceFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item #", ScanField.ManufacturerPartNumber),
            new FileProperty("Cat. Pg. #", ScanField.Ignore),
            new FileProperty("IMAP", ScanField.MAP),
            new FileProperty("Description", ScanField.Ignore),
        };

        private readonly IStorageProvider<WildWoodLampsVendor> _storageProvider;
        public ChelseaHousePriceFileLoader(IStorageProvider<WildWoodLampsVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "Chelsea House Price List December 2018 - IMAP.xlsx"), Properties, ScanField.ManufacturerPartNumber, 2, 3);
        }
    }
}