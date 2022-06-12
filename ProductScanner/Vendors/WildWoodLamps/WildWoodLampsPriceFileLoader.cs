using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace WildWoodLamps
{
    public class WildWoodLampsPriceFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item #", ScanField.ManufacturerPartNumber),
            new FileProperty("Catalog Page", ScanField.Ignore),
            new FileProperty("IMAP", ScanField.MAP),
            new FileProperty("Description", ScanField.Ignore),
        };

        private readonly IStorageProvider<WildWoodLampsVendor> _storageProvider;
        public WildWoodLampsPriceFileLoader(IStorageProvider<WildWoodLampsVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "Wildwood Price List December 2018 - IMAP.XLSX"), Properties, ScanField.ManufacturerPartNumber, 2, 3);
        }
    }
}