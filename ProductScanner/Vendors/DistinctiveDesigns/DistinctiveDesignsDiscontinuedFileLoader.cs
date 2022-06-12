using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace DistinctiveDesigns
{
    public class DistinctiveDesignsDiscontinuedFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("SKU", ScanField.ItemNumber),
            new FileProperty("Item_Name", ScanField.PatternName),
            new FileProperty("Status", ScanField.Ignore),
        };

        private readonly IStorageProvider<DistinctiveDesignsVendor> _storageProvider;
        public DistinctiveDesignsDiscontinuedFileLoader(IStorageProvider<DistinctiveDesignsVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadDiscontinued()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "eReseller Discontinued Items Update 101317.xlsx"), Properties, ScanField.ItemNumber, 1, 2);
        }
    }
}