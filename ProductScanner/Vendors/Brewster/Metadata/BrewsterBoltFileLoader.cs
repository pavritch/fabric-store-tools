using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Brewster.Metadata
{
    public class BrewsterBoltFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Book #", ScanField.Ignore),
            new FileProperty("Book Name", ScanField.Ignore),
            new FileProperty("Brand", ScanField.Ignore),
            new FileProperty("Pattern", ScanField.ManufacturerPartNumber),
            new FileProperty("Name", ScanField.Ignore),
            new FileProperty("Product Type", ScanField.Ignore),
            new FileProperty("Description", ScanField.Ignore),
            new FileProperty("MSRP", ScanField.Ignore),
            new FileProperty("Barcode", ScanField.Ignore),
            new FileProperty("Width (in)", ScanField.Ignore),
            new FileProperty("Length (ft)", ScanField.Ignore),
            new FileProperty("Coverage", ScanField.Ignore),
            new FileProperty("Unit Weight", ScanField.Ignore),
            new FileProperty("Repeat (in)", ScanField.Ignore),
            new FileProperty("Match", ScanField.Ignore),
            new FileProperty("Paste", ScanField.Ignore),
            new FileProperty("Material", ScanField.Ignore),
            new FileProperty("Washability", ScanField.Ignore),
            new FileProperty("Removability", ScanField.Ignore),
            new FileProperty("Colorway", ScanField.Ignore),
            new FileProperty("Color Family", ScanField.Ignore),
            new FileProperty("Style", ScanField.Ignore),
            new FileProperty("Theme", ScanField.Ignore),
        };

        private readonly IStorageProvider<BrewsterVendor> _storageProvider;
        public BrewsterBoltFileLoader(IStorageProvider<BrewsterVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "BoltItemData.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}