using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace MoesHome
{
    public class MoesInventoryFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item No.", ScanField.ManufacturerPartNumber),
            new FileProperty("Description", ScanField.Description),
            new FileProperty("In stock", ScanField.StockCount),
            new FileProperty("Backorder Date", ScanField.LeadTime),
            new FileProperty("Disco. (Yes=1, No=0)", ScanField.IsDiscontinued),
        };

        private readonly IStorageProvider<MoesHomeVendor> _storageProvider;
        public MoesInventoryFileLoader(IStorageProvider<MoesHomeVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "Moes_Stock.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}