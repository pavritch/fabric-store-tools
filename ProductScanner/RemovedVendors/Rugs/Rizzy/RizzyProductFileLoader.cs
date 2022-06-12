using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Rizzy
{
    public class RizzyProductFileLoader : ProductFileLoader<RizzyVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("MFG", ScanField.Ignore),
            new FileProperty("COLLECTION NAME (if applicable)", ScanField.Collection),
            new FileProperty("STATUS", ScanField.Ignore),
            new FileProperty("LONG SKU - VPN", ScanField.SKU),
            new FileProperty("SHORT SKU - VPN SHORT", ScanField.ItemNumber),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("CONSTRUCTION INFO", ScanField.Construction),
            new FileProperty("APPROVED COST", ScanField.Cost),
            new FileProperty("MAPP", ScanField.MAP),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("SIZE", ScanField.Size),

            // Shape, which is also in Size
            new FileProperty("CATEGORY", ScanField.Ignore),
            new FileProperty("PATTERN", ScanField.Pattern),
            new FileProperty("PRIMARY COLOR", ScanField.ColorName),
            new FileProperty("COLORS - ALL", ScanField.Color),
            new FileProperty("MATERIAL", ScanField.Material),
            new FileProperty("MATERIAL DETAIL", ScanField.Content),
            new FileProperty("BACKING ON RUG", ScanField.Backing),
            new FileProperty("REVERSIBLE", ScanField.AdditionalInfo),
            new FileProperty("CARE & CLEANING METHOD", ScanField.Cleaning),
            new FileProperty("LICENSED", ScanField.Ignore),
            new FileProperty("FEATURE I", ScanField.Bullet1),
            new FileProperty("FEATURE II", ScanField.Bullet2),
            new FileProperty("FEATURE III", ScanField.Bullet3),
            new FileProperty("FEATURE IV", ScanField.Bullet4),
            new FileProperty("WARRANTY", ScanField.Ignore),
            new FileProperty("MADE IN", ScanField.Country),
            new FileProperty("SUPPLIER SHIP TIME", ScanField.Ignore),
            new FileProperty("PILE HEIGHT", ScanField.PileHeight),
            new FileProperty("PRODUCT WIDTH", ScanField.Ignore),
            new FileProperty("PRODUCT DEPTH (THICKNESS)", ScanField.Depth),
            new FileProperty("PRODUCT HEIGHT", ScanField.Ignore),
            new FileProperty("PRODUCT WEIGHT", ScanField.Weight),
            new FileProperty("UNITS PER CARTON", ScanField.Ignore),
            new FileProperty("HOW MANY CARTONS", ScanField.Ignore),
            new FileProperty("SHIPPING FROM", ScanField.Ignore),
            new FileProperty("SHIPPING - CTN 1 - WIDTH", ScanField.Ignore),
            new FileProperty("SHIPPING - CTN 1 - DEPTH", ScanField.Ignore),
            new FileProperty("SHIPPING - CTN 1 - HEIGHT", ScanField.Ignore),
            new FileProperty("SHIPPING - CTN 1 - WEIGHT", ScanField.Ignore),
            new FileProperty("LXWXD", ScanField.Ignore),
            new FileProperty("CTN 1 CUBES", ScanField.Ignore),
            new FileProperty("SHIPPING METHOD", ScanField.Ignore),
        };

        public RizzyProductFileLoader(IStorageProvider<RizzyVendor> storageProvider)
            : base(storageProvider, ScanField.SKU, Properties) { }

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var product in products)
            {
                var patternNumber = product[ScanField.ItemNumber].Split(new[] { '-' }).First();
                product[ScanField.PatternNumber] = patternNumber;
            }
            return products;
        }
    }
}