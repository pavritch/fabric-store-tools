using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Safavieh
{
    public class SafaviehFileLoader : ProductFileLoader<SafaviehVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("#", ScanField.Ignore),
            new FileProperty("Design Type", ScanField.Design),
            new FileProperty("Rug Collection", ScanField.Collection),
            new FileProperty("Product Name", ScanField.ProductName),
            new FileProperty("Manufacturer SKU", ScanField.ManufacturerPartNumber),
            new FileProperty("Discontinued?", ScanField.Status),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("Rug Size", ScanField.Size),

            // all come from size
            new FileProperty("width feet", ScanField.Ignore),
            new FileProperty("width inches", ScanField.Ignore),
            new FileProperty("length feet", ScanField.Ignore),
            new FileProperty("length inches", ScanField.Ignore),

            new FileProperty("Cost", ScanField.Cost),
            new FileProperty("PSF", ScanField.Ignore),
            new FileProperty("IMAP", ScanField.Ignore),
            new FileProperty("Max Discount Allowed 15% Advertised Price", ScanField.Ignore),
            new FileProperty("Origin", ScanField.Country),
            new FileProperty("Shape", ScanField.Shape),
            new FileProperty("PackingWeight (lbs.)", ScanField.Weight),
            new FileProperty("Packing Length (in.)", ScanField.PackageLength),
            new FileProperty("Packing Width (in.)", ScanField.PackageWidth),
            new FileProperty("Packing Height (in.)", ScanField.PackageHeight),
            new FileProperty("Pile Height (in.)", ScanField.PileHeight),
            new FileProperty("Material", ScanField.Material),
            new FileProperty("Weave", ScanField.Construction),
            new FileProperty("Rug Description", ScanField.Description),
            // these keywords are just the material and weave
            new FileProperty("Keywords", ScanField.Ignore),
            new FileProperty("", ScanField.Ignore),
            new FileProperty("", ScanField.Ignore),
            new FileProperty("Item ID", ScanField.AlternateItemNumber),
            new FileProperty("Area", ScanField.Ignore),
            new FileProperty("", ScanField.Ignore),
            new FileProperty("", ScanField.Ignore),
            new FileProperty("Image 1", ScanField.Image1),
            new FileProperty("Image 2", ScanField.Image2),
            new FileProperty("Image 3", ScanField.Image3),
            new FileProperty("Image 4", ScanField.Image4),
            new FileProperty("Image 5", ScanField.Image5),
            new FileProperty("*", ScanField.Ignore),
            new FileProperty("Cost with Freight", ScanField.Ignore),
            new FileProperty("Oversized", ScanField.Ignore),
        };

        public SafaviehFileLoader(IStorageProvider<SafaviehVendor> storageProvider)
            : base(storageProvider, ScanField.ProductName, Properties) { }

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var product in products)
            {
                product[ScanField.PatternNumber] = GetPatternNumber(product[ScanField.ManufacturerPartNumber]);
                product[ScanField.SKU] = GetProductSKU(product[ScanField.ManufacturerPartNumber]);
                product[ScanField.Image1] = product[ScanField.Image1].Replace(".jpg", "") + ".jpg";
                product[ScanField.Image2] = product[ScanField.Image2].Replace(".jpg", "") + ".jpg";
                product[ScanField.Image3] = product[ScanField.Image3].Replace(".jpg", "") + ".jpg";
                product[ScanField.Image4] = product[ScanField.Image4].Replace(".jpg", "") + ".jpg";
                product[ScanField.Image5] = product[ScanField.Image5].Replace(".jpg", "") + ".jpg";
                product[ScanField.Cost] = product[ScanField.Cost].Replace("$", "").Trim();
            }
            products = products.Where(x => !x[ScanField.Status].ContainsIgnoreCase("Yes")).ToList();
            products = products.Where(x => x[ScanField.Design] != "Padding").ToList();
            products = products.Where(x => !x[ScanField.Size].ContainsIgnoreCase("Set")).ToList();
            return products;
        }

        // so we can tie variants together
        private string GetProductSKU(string mpn)
        {
            var firstPart = mpn.Substring(0, mpn.LastIndexOf("-"));
            return firstPart;
        }

        private string GetPatternNumber(string mpn)
        {
            // MDA622B-7SQ
            // NWB8705-4020-5
            //var firstPart = mpn.Substring(0, mpn.IndexOf("-") - 1);
            //if (char.IsLetter(firstPart.Last())) return firstPart.Substring(0, firstPart.Length - 1);
            var firstPart = mpn.Substring(0, mpn.IndexOf("-"));
            if (char.IsLetter(firstPart.Last())) return firstPart.Substring(0, firstPart.Length);
            return firstPart;
        }
    }
}