using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Dynamic
{
    public class DynamicFileLoader : ProductFileLoader<DynamicVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Product Name", ScanField.Ignore),
            new FileProperty("Item Code (SKU)", ScanField.SKU),
            new FileProperty("UPC Code", ScanField.UPC),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("New (Yes/No)", ScanField.Ignore),
            new FileProperty("Collection Description", ScanField.Description),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("Design", ScanField.Design),
            new FileProperty("Color #", ScanField.Color),
            new FileProperty("Color Name", ScanField.ColorName),
            new FileProperty("Technique", ScanField.Construction),
            new FileProperty("Material", ScanField.Material),
            new FileProperty("Shape", ScanField.Ignore),
            new FileProperty("Country of Origin", ScanField.Country),
            new FileProperty("Style", ScanField.Style),
            new FileProperty("Style Subcategory", ScanField.Style2),
            new FileProperty("Cleaning", ScanField.Cleaning),
            new FileProperty("Pile Description", ScanField.Pile),
            new FileProperty("Bullet 1", ScanField.Bullet1),
            new FileProperty("Bullet 2", ScanField.Bullet2),
            new FileProperty("Bullet 3", ScanField.Bullet3),
            new FileProperty("Wholesale Price", ScanField.Cost),
            new FileProperty("MAP", ScanField.MAP),
            new FileProperty("CAD MAP", ScanField.Ignore),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("Lead Time", ScanField.LeadTime),
            new FileProperty("Warranty Length", ScanField.Warranty),
            new FileProperty("Ship Type", ScanField.Ignore),
            new FileProperty("Cubes (ft3)", ScanField.Ignore),
            new FileProperty("Product Weight", ScanField.Weight),
            new FileProperty("Product Max Width", ScanField.Ignore),
            new FileProperty("Product Max Height", ScanField.Ignore),
            new FileProperty("Pile Height", ScanField.PileHeight),
            new FileProperty("Rolled Shipping Length/Height", ScanField.PackageLength),
            new FileProperty("Rolled Shipping Width", ScanField.PackageWidth),
            new FileProperty("Image File", ScanField.Image1),
        };

        public DynamicFileLoader(IStorageProvider<DynamicVendor> storageProvider)
            : base(storageProvider, ScanField.UPC, Properties, ProductFileType.Xlsx) { }

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x =>
            {
                x[ScanField.Size] = x[ScanField.Size].Replace("R ", "ROUND");
                x[ScanField.Country] = x[ScanField.Country].TitleCase();
                x[ScanField.Construction] = x[ScanField.Construction].Replace("Handmade", "Hand Made");
            });
            products = products.Where(x => !x[ScanField.Size].Contains("FL")).ToList();
            return products;
        }
    }
}