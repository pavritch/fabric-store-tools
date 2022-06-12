using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Dalyn
{
    public class DalynProductFileLoader : ProductFileLoader<DalynVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("SKU's", ScanField.ManufacturerPartNumber),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("MAP\n(1-23-17)", ScanField.MAP),
            new FileProperty("Description", ScanField.Ignore),
            new FileProperty("Design", ScanField.PatternNumber),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("Accent Colors", ScanField.ColorGroup),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("Rug Dims\n(inches)", ScanField.Ignore),
            new FileProperty("Rug Width\n(inches)", ScanField.Ignore),
            new FileProperty("Rug Length\n(inches)", ScanField.Ignore),
            new FileProperty("Rug Height\n(inches)", ScanField.Ignore),
            new FileProperty("Pile Height\n(mm)", ScanField.PileHeight),
            new FileProperty("Square Feet", ScanField.Ignore),
            new FileProperty("Square Yards", ScanField.Ignore),
            new FileProperty("Square Meters", ScanField.Ignore),
            new FileProperty("Rug Weight (lbs)", ScanField.Ignore),
            new FileProperty("Rug Weight (kg)", ScanField.Ignore),
            new FileProperty("Rug Weight (grams)", ScanField.Ignore),
            new FileProperty("GSM\ngrams/m2", ScanField.Ignore),
            new FileProperty("Shape", ScanField.Ignore),
            new FileProperty("Construction", ScanField.Construction),
            new FileProperty("Content", ScanField.Content),
            new FileProperty("Backing", ScanField.Backing),
            new FileProperty("Style1", ScanField.Style1),
            new FileProperty("Style2", ScanField.Style2),
            new FileProperty("Pattern", ScanField.Design),
            new FileProperty("Shipping Weight (lbs)", ScanField.Weight),
            new FileProperty("Shipping Dims (inches)", ScanField.Ignore),
            new FileProperty("Shipping Length (inches)", ScanField.PackageLength),
            new FileProperty("Shipping Height (inches)", ScanField.PackageHeight),
            new FileProperty("Shipping Width (inches)", ScanField.PackageWidth),
            new FileProperty("Shipping Cube", ScanField.Ignore),
            new FileProperty("Origin", ScanField.Country),
            new FileProperty("HTS", ScanField.Ignore),
            new FileProperty("NMFC#", ScanField.Ignore),
            new FileProperty("Freight Class", ScanField.Ignore),
            new FileProperty("Care", ScanField.Cleaning),
            new FileProperty("Collection Description", ScanField.Description),
            new FileProperty("Image 1", ScanField.Image1),
            new FileProperty("Image 2", ScanField.Image2),
            new FileProperty("Image 3", ScanField.Image3),
            new FileProperty("Image 4", ScanField.Image4),
            new FileProperty("Image 5", ScanField.Image5),
        };

        public DalynProductFileLoader(IStorageProvider<DalynVendor> storageProvider) 
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x => x.Cost = x[ScanField.MAP].ToDecimalSafe() / 2);

            // DelMar images are not listed, but we can build the image urls here
            var delMar = products.Where(x => x[ScanField.Collection] == "DelMar").ToList();
            delMar.ForEach(x =>
            {
                x[ScanField.Image1] = string.Format("{0}_{1}_{2}.jpg", x[ScanField.Collection], x[ScanField.PatternNumber], x[ScanField.Color]);
                x[ScanField.Image2] = string.Format("{0}_{1}_{2}_CU.jpg", x[ScanField.Collection], x[ScanField.PatternNumber], x[ScanField.Color]);
                x[ScanField.Image3] = string.Format("{0}_{1}_{2}_FL.jpg", x[ScanField.Collection], x[ScanField.PatternNumber], x[ScanField.Color]);
                x[ScanField.Image4] = string.Format("{0}_{1}_RS.JPG", x[ScanField.PatternNumber], x[ScanField.Color]);
            });

            return products;
        }
    }
}