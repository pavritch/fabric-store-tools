using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Variants;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Loloi
{
    public class LoloiVariantMerger : IVariantMerger<LoloiVendor>
    {
        public List<ScanData> MergeVariants(List<ScanData> variants)
        {
            return variants.GroupBy(x => x[ScanField.ManufacturerPartNumber].Substring(0, x[ScanField.ManufacturerPartNumber].Length - 4))
                .Select(x => new ScanData {Variants = x.ToList()}).ToList();
        }
    }

    public class LoloiDetailsFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("E-Com Cost", ScanField.Ignore),
            new FileProperty("MAP", ScanField.MAP),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("Main Category", ScanField.Style),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("Rug ID", ScanField.ManufacturerPartNumber),
            new FileProperty("Available Quantity", ScanField.StockCount),
            new FileProperty("Discontinued", ScanField.IsDiscontinued),
            new FileProperty("Creation Date", ScanField.Ignore),
            new FileProperty("Design", ScanField.PatternNumber),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("Weight", ScanField.Weight),
            new FileProperty("Shipping L", ScanField.Ignore),
            new FileProperty("Shipping W", ScanField.Ignore),
            new FileProperty("Shipping H", ScanField.Ignore),
            new FileProperty("Pile Height", ScanField.PileHeight),
            new FileProperty("Fiber", ScanField.Material),
            new FileProperty("Quality", ScanField.Construction),
            new FileProperty("Lifestyle/Running", ScanField.Ignore),
            new FileProperty("HTC", ScanField.Ignore),
            new FileProperty("Product Care", ScanField.Cleaning),
            new FileProperty("Description", ScanField.Description),
            new FileProperty("Country", ScanField.Country),
            new FileProperty("Shipping Method", ScanField.ShippingMethod),
            new FileProperty("Image Name", ScanField.Ignore),
            new FileProperty("URL Image", ScanField.ImageUrl),
        };

        private readonly IStorageProvider<LoloiVendor> _storageProvider;

        public LoloiDetailsFileLoader(IStorageProvider<LoloiVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            var data = fileLoader.Load(Path.Combine(staticFileFolder, "Loloi_Details.xlsx"), Properties, ScanField.UPC, 6, 7);
            return data.Where(x => x[ScanField.IsDiscontinued] == "No").ToList();
        }
    }
}