using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace CurreyCo
{
    public class CurreyFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("SKU", ScanField.ItemNumber),
            new FileProperty("DESCRIPTION", ScanField.Ignore),
            new FileProperty("LIGHTS", ScanField.Ignore),
            new FileProperty("Length of Chain", ScanField.Ignore),
            new FileProperty("Addt'l Chain SKU", ScanField.Ignore),
            new FileProperty("SHADE/TOP DETAIL", ScanField.Ignore),
            new FileProperty("FINISH", ScanField.Ignore),
            new FileProperty("DIMENSIONS               (Inches unless otherwise stated) ", ScanField.Ignore),
            new FileProperty("RETAIL", ScanField.RetailPrice),
            new FileProperty("STOCKING DEALER", ScanField.Cost),
            new FileProperty("IMAP", ScanField.MAP),
            new FileProperty("SHIP", ScanField.Ignore),
            new FileProperty("PAGE", ScanField.Ignore),
        };

        private readonly IStorageProvider<CurreyVendor> _storageProvider;
        public CurreyFileLoader(IStorageProvider<CurreyVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadPriceData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "Currey_Price.xlsx"), Properties, ScanField.ItemNumber, 1, 4);
        }
    }
}