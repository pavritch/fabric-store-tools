using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Chandra
{
    public class ChandraProductFileLoader : ProductFileLoader<ChandraVendor>
    {
        // Analysis as of 8/21
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Sr.#", ScanField.Ignore),
            // "Chandra Rugs" for all rows
            new FileProperty("Manufacturer", ScanField.Ignore),

            // Area Rugs, Furniture, Pillows, Poufs, Throws - only interested in Area Rugs
            new FileProperty("Category", ScanField.Category),

            // 168 total collections
            new FileProperty("Collection", ScanField.Collection),

            // Style with the suffix appended
            new FileProperty("MAIN SKU", ScanField.ManufacturerPartNumber),

            // Just the style - something like ADO-901
            new FileProperty("Style#", ScanField.PatternNumber),

            new FileProperty("All Colors", ScanField.Color),

            // Always "Program"
            new FileProperty("2015-Status", ScanField.Ignore),

            // Furniture, Pattern, Texture, Throws
            new FileProperty("Category", ScanField.Type),

            new FileProperty("UPC", ScanField.UPC),

            // Either "Available in Catalogs" or "Not Available in Catalogs" - not relevant either way
            new FileProperty("Image in Catalog Status", ScanField.Ignore),

            new FileProperty("Short Description", ScanField.Description),

            new FileProperty("Construction ", ScanField.Construction),

            // Always "Handmade"
            new FileProperty("Manufacture", ScanField.Ignore),

            // Sometimes has %s, sometimes just a list of materials
            new FileProperty("Material ", ScanField.Material),

            new FileProperty(" Wholesale Cost ", ScanField.Cost),
            new FileProperty(" MAP ", ScanField.MAP),
            new FileProperty(" MSRP ", ScanField.RetailPrice),

            // Parsed to get the length, width, and shape
            new FileProperty("Dimensions", ScanField.Dimensions),
            new FileProperty("Weight (lbs.)", ScanField.Weight),

            // All calculated from parsed dimensions
            new FileProperty("Width (Feet) ", ScanField.Ignore),
            new FileProperty("Length (Feet) ", ScanField.Ignore),
            new FileProperty("Carrier", ScanField.Ignore),
            new FileProperty("Area (sq.ft)", ScanField.Ignore),

            new FileProperty("Box Length (inches)", ScanField.PackageLength),
            new FileProperty("Box Width (inches)", ScanField.PackageWidth),
            new FileProperty("Box Height (inches)", ScanField.PackageHeight),

            // Also determined based on dimensions
            new FileProperty("Shape", ScanField.Ignore),

            // Either "Interior" or "Interior/Exterior"
            new FileProperty("Interior / Exterior", ScanField.AdditionalInfo),

            new FileProperty("Pile Height / Thickness (inches)", ScanField.PileHeight),
            new FileProperty("Backing", ScanField.Backing),

            // Ignoring for now because we don't have enough info
            new FileProperty("Sample Type", ScanField.Ignore),
            // India, China, USA
            new FileProperty("Country of Origin", ScanField.Country),

            // Images - augmented by those found on the public site
            new FileProperty("Image Used", ScanField.Image1),
            new FileProperty("Flat Image Link Address", ScanField.Image2),
            new FileProperty("Corner Image Link Address", ScanField.Image3),
            new FileProperty("Closeup Image Link Address", ScanField.Image4),
            new FileProperty("Styleshot Image Link Address", ScanField.Image5),
            new FileProperty("Roomscene Image Link Address", ScanField.Image6),
            new FileProperty("Collection Image Link Address", ScanField.Image7),
            // Not selling keyrings at this time
            new FileProperty("KeyRing Image Link Address", ScanField.Ignore),
        };

        public ChandraProductFileLoader(IStorageProvider<ChandraVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { } 

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            foreach (var product in products)
            {
                product[ScanField.AdditionalInfo] = product[ScanField.AdditionalInfo].Replace("- N A-", "");
                product[ScanField.Backing] = product[ScanField.Backing]
                    .Replace("- N A-", "")
                    .Replace("No Backing", "");
                product[ScanField.PileHeight] = product[ScanField.PileHeight].Replace("- N A-", "");
                product[ScanField.Description] = product[ScanField.Description]
                    .Replace("C+E3833ontemporary", "Contemporary")
                    .Replace("Rug", "").Trim();
            }
            products = products.Where(x => x[ScanField.Category] == "Area Rugs").ToList();
            return products;
        }
    }
}