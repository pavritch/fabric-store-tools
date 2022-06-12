using System.Collections.Generic;
using System.IO;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace RMCoco.Metadata
{
    public class RMCocoFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Pattern Name", ScanField.PatternName),
            new FileProperty("Color Name", ScanField.ColorName),
            new FileProperty("Book", ScanField.Book),
            new FileProperty("Item Group ID", ScanField.Group),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("Horizontal Repeat", ScanField.HorizontalRepeat),
            new FileProperty("Vertical Repeat", ScanField.VerticalRepeat),
            new FileProperty("Railroaded", ScanField.Railroaded),
            new FileProperty("Content", ScanField.Content),
            new FileProperty("Flame Retardant", ScanField.FlameRetardant),
            new FileProperty("Cleaning Code", ScanField.Cleaning),
            new FileProperty("Status", ScanField.Status),
            new FileProperty("Suggested Retail Pricing", ScanField.RetailPrice),
            new FileProperty("Lowest MAP Price", ScanField.MAP),
            new FileProperty("Wholesale", ScanField.Cost),
            new FileProperty("Image URL", ScanField.ImageUrl),
        };

        private readonly IStorageProvider<RMCocoVendor> _storageProvider;
        public RMCocoFileLoader(IStorageProvider<RMCocoVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadPriceData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            var products = fileLoader.Load(Path.Combine(staticFileFolder, "RMCOCO Products 04 22 19.xlsx"), Properties, ScanField.ImageUrl, 1, 2);
            products.ForEach(x => x.Cost = x[ScanField.Cost].ToDecimalSafe());
            products.ForEach(x => x[ScanField.ManufacturerPartNumber] = x[ScanField.PatternName] + "-" + x[ScanField.ColorName]);
            return products;
        }
    }
}