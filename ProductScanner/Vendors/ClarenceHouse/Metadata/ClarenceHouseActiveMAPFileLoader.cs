using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace ClarenceHouse.Metadata
{
    public class ClarenceHouseActiveMAPFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("NUM", ScanField.ManufacturerPartNumber),
            new FileProperty("NAME", ScanField.PatternName),
            new FileProperty("COLOR", ScanField.Color),
            new FileProperty("Cost", ScanField.Cost),
            new FileProperty("MAP Price", ScanField.MAP),
        };

        private readonly IStorageProvider<ClarenceHouseVendor> _storageProvider;
        public ClarenceHouseActiveMAPFileLoader(IStorageProvider<ClarenceHouseVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadMAPData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var filePath = Path.Combine(staticFileFolder, "CH Complete Limited Stock List 5.9.17_MAP.xlsx");
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(filePath, Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}