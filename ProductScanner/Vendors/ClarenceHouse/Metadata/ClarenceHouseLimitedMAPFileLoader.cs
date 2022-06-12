using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace ClarenceHouse.Metadata
{
    public class ClarenceHouseLimitedMAPFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item No.", ScanField.ManufacturerPartNumber),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("Pattern Desc", ScanField.Pattern),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("Content", ScanField.Content),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("V-Rep", ScanField.VerticalRepeat),
            new FileProperty("H-Rep", ScanField.HorizontalRepeat),
            new FileProperty("Country", ScanField.Country),
            new FileProperty("Cost", ScanField.Cost),
            new FileProperty("MAP Price", ScanField.MAP),
            new FileProperty("Group", ScanField.Ignore),
        };

        private readonly IStorageProvider<ClarenceHouseVendor> _storageProvider;
        public ClarenceHouseLimitedMAPFileLoader(IStorageProvider<ClarenceHouseVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadMAPData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var filePath = Path.Combine(staticFileFolder, "CH2017 Active Product MAP Price List.xlsx");
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(filePath, Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}