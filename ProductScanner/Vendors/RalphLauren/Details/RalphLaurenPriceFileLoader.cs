using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace RalphLauren.Details
{
    public class RalphLaurenPriceFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("COLL", ScanField.Ignore),
            new FileProperty("PATTERN NAME", ScanField.Ignore),
            new FileProperty("COLOR WAY", ScanField.Ignore),
            new FileProperty("NUMBER", ScanField.ManufacturerPartNumber),
            new FileProperty("UOM", ScanField.Ignore),
            new FileProperty("BOOK NAME", ScanField.Ignore),
            new FileProperty("STK", ScanField.Ignore),
            new FileProperty("MAP", ScanField.MAP),
        };

        private readonly IStorageProvider<RalphLaurenVendor> _storageProvider;
        public RalphLaurenPriceFileLoader(IStorageProvider<RalphLaurenVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "JAN 1 2019 - PRICE SHEET - - -   MAP PRICING.xlsx"), Properties, ScanField.ManufacturerPartNumber, 5, 6);
        }
    }
}