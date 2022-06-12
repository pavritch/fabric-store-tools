using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using System.Linq;
using ProductScanner.Core;

namespace SuryaHomeware
{
    public class SuryaPillowFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Introduced", ScanField.Bullet1),
            new FileProperty("Sku", ScanField.ManufacturerPartNumber),
            new FileProperty("Design-Name", ScanField.PatternNumber),
            new FileProperty("DesignName", ScanField.Pattern),
            new FileProperty("Parent ID", ScanField.PatternCorrelator),
            new FileProperty("Master ID", ScanField.Ignore),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("Product Name", ScanField.ProductName),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("Designer", ScanField.Designer),
            new FileProperty("Ignore", ScanField.Ignore),
            new FileProperty("Ignore Copy", ScanField.Ignore),
            new FileProperty("Romance Copy", ScanField.Ignore),
            new FileProperty("MSRP", ScanField.Ignore),
            new FileProperty("EMAP", ScanField.Ignore),
            new FileProperty("Ecom Cost", ScanField.Cost),
            new FileProperty("Number of Items Included", ScanField.Ignore),
            new FileProperty("Pieces Included", ScanField.Type),
            new FileProperty("Top Seller", ScanField.Ignore),
            new FileProperty("Commercial Use", ScanField.Ignore),
            new FileProperty("Product Category", ScanField.Category),
            new FileProperty("Product Type", ScanField.Ignore),
            new FileProperty("Keywords", ScanField.Ignore),
            new FileProperty("Related Items", ScanField.Ignore),
            new FileProperty("Country of Origin", ScanField.Ignore),
            new FileProperty("Construction Group", ScanField.Ignore),
            new FileProperty("Construction", ScanField.Ignore),
            new FileProperty("Generic Materials", ScanField.Ignore),
            new FileProperty("Specific Materials", ScanField.Ignore),
            new FileProperty("Fill Material", ScanField.Ignore),
            new FileProperty("Navigational Color", ScanField.Ignore),
            new FileProperty("Colors", ScanField.Color),
            new FileProperty("Pantone #", ScanField.Ignore),
            new FileProperty("Style", ScanField.Ignore),
            new FileProperty("Pattern", ScanField.Ignore),
            new FileProperty("Shape", ScanField.Ignore),
            new FileProperty("Theme", ScanField.Ignore),
            new FileProperty("Embroidered", ScanField.Ignore),
            new FileProperty("Organic", ScanField.Ignore),
            new FileProperty("Gender", ScanField.Ignore),
            new FileProperty("Life Stage", ScanField.Ignore),
            new FileProperty("Tassels Included", ScanField.Ignore),
            new FileProperty("Tassel Type", ScanField.Ignore),
            new FileProperty("Tassel Color", ScanField.Ignore),
            new FileProperty("Piped Edges", ScanField.Ignore),
            new FileProperty("Piping Material", ScanField.Ignore),
            new FileProperty("Piping Color", ScanField.Ignore),
            new FileProperty("Fringed Edges", ScanField.Ignore),
            new FileProperty("Fringe Type", ScanField.Ignore),
            new FileProperty("Fringe Color", ScanField.Ignore),
            new FileProperty("Flanged Edges", ScanField.Ignore),
            new FileProperty("Flange Color", ScanField.Ignore),
            new FileProperty("Contrasting Border", ScanField.Ignore),
            new FileProperty("Contrasting Border Color", ScanField.Ignore),
            new FileProperty("Reversible", ScanField.Ignore),
            new FileProperty("Reverse Side Color", ScanField.Ignore),
            new FileProperty("Reverse Side Material", ScanField.Ignore),
            new FileProperty("Reverse Side Pattern", ScanField.Ignore),
            new FileProperty("Distressed", ScanField.Ignore),
            new FileProperty("Moisture Wicking", ScanField.Ignore),
            new FileProperty("Areas of Support", ScanField.Ignore),
            new FileProperty("Density", ScanField.Ignore),
            new FileProperty("Thread Count", ScanField.Ignore),
            new FileProperty("Handmade", ScanField.Ignore),
            new FileProperty("Double Stitched", ScanField.Ignore),
            new FileProperty("Ruffled", ScanField.Ignore),
            new FileProperty("Buttons", ScanField.Ignore),
            new FileProperty("Applique", ScanField.Ignore),
            new FileProperty("Sequined", ScanField.Ignore),
            new FileProperty("Gimp Accent", ScanField.Ignore),
            new FileProperty("Room Use", ScanField.Ignore),
            new FileProperty("Self-Backed", ScanField.Ignore),
            new FileProperty("Removable Cover", ScanField.Ignore),
            new FileProperty("Personalization", ScanField.Ignore),
            new FileProperty("Pre-shrunk", ScanField.Ignore),
            new FileProperty("Licensed Product", ScanField.Ignore),
            new FileProperty("Outdoor Safe?", ScanField.Ignore),
            new FileProperty("Product Care", ScanField.Ignore),
            new FileProperty("Washing Method", ScanField.Ignore),
            new FileProperty("Drying Method", ScanField.Ignore),
            new FileProperty("Iron Safe", ScanField.Ignore),
            new FileProperty("Product Warranty", ScanField.Ignore),
            new FileProperty("Assembly Required?", ScanField.Ignore),
            new FileProperty("Product Weight - Lbs", ScanField.Ignore),
            new FileProperty("Product Weight - G", ScanField.Ignore),
            new FileProperty("Diameter", ScanField.Ignore),
            new FileProperty("Size", ScanField.Ignore),
            new FileProperty("Size - In", ScanField.Ignore),
            new FileProperty("Size - Cm", ScanField.Ignore),
            new FileProperty("Width - In", ScanField.Ignore),
            new FileProperty("Length - In", ScanField.Ignore),
            new FileProperty("Depth - In", ScanField.Ignore),
            new FileProperty("Width - Cm", ScanField.Ignore),
            new FileProperty("Length - Cm", ScanField.Ignore),
            new FileProperty("Depth - Cm", ScanField.Ignore),
            new FileProperty("Custom Size Available?", ScanField.Ignore),
            new FileProperty("Main Image", ScanField.Ignore),
            new FileProperty("Additional Image 1", ScanField.Ignore),
            new FileProperty("Additional Image 2", ScanField.Ignore),
            new FileProperty("Additional Image 3", ScanField.Ignore),
            new FileProperty("Additional Image 4", ScanField.Ignore),
            new FileProperty("Additional Products Shown in Images", ScanField.Ignore),
            new FileProperty("Lead Time", ScanField.Ignore),
            new FileProperty("Ship Method", ScanField.Ignore),
            new FileProperty("NMFC Code", ScanField.Ignore),
            new FileProperty("Freight Class", ScanField.Ignore),
            new FileProperty("Carton Weight - Lbs", ScanField.Ignore),
            new FileProperty("Carton Weight - G", ScanField.Ignore),
            new FileProperty("Shipping Dimensions - In", ScanField.Ignore),
            new FileProperty("Shipping Dimensions - Cm", ScanField.Ignore),
            new FileProperty("Shipping Dimensions - Cubic Meters", ScanField.Ignore),
            new FileProperty("Carton Height - In", ScanField.Ignore),
            new FileProperty("Carton Width - In", ScanField.Ignore),
            new FileProperty("Carton Depth - In", ScanField.Ignore),
            new FileProperty("Carton Height - Cm", ScanField.Ignore),
            new FileProperty("Carton Width - Cm", ScanField.Ignore),
            new FileProperty("Carton Depth - Cm", ScanField.Ignore),
            new FileProperty("Eco-Friendly", ScanField.Ignore),
            new FileProperty("Weather Resistant", ScanField.Ignore),
            new FileProperty("Weather Resistant Details", ScanField.Ignore),
            new FileProperty("Water Resistant", ScanField.Ignore),
            new FileProperty("Water Resistant Details", ScanField.Ignore),
            new FileProperty("Non-Toxic", ScanField.Ignore),
            new FileProperty("Stain Resistant", ScanField.Ignore),
            new FileProperty("Mildew Resistant", ScanField.Ignore),
            new FileProperty("Fade Resistant", ScanField.Ignore),
            new FileProperty("Tear Resistant", ScanField.Ignore),
            new FileProperty("Odor Resistant", ScanField.Ignore),
            new FileProperty("Heat Resistant", ScanField.Ignore),
            new FileProperty("Wrinkle Resistant", ScanField.Ignore),
            new FileProperty("Dust Mite Resistant", ScanField.Ignore),
            new FileProperty("Hypoallergenic", ScanField.Ignore),
            new FileProperty("Antimirobial", ScanField.Ignore),
            new FileProperty("Anti-Bacterial", ScanField.Ignore),
            new FileProperty("Lint Free", ScanField.Ignore),
            new FileProperty("Non-Pilling", ScanField.Ignore),
            new FileProperty("Bed Bug Resistant", ScanField.Ignore),
            new FileProperty("Recycled Content", ScanField.Ignore),
            new FileProperty("Total Recycled Content (Percentage)", ScanField.Ignore),
            new FileProperty("Post-Consumer Content (Percentage)", ScanField.Ignore),
            new FileProperty("Remanufactured/Refurbished", ScanField.Ignore),
            new FileProperty("ISTA 3A Certified", ScanField.Ignore),
            new FileProperty("Greenguard Certified", ScanField.Ignore),
            new FileProperty("GOTS Certified", ScanField.Ignore),
            new FileProperty("�eko-Tex Standard Compliant", ScanField.Ignore),
            new FileProperty("Organic Certified", ScanField.Ignore),
            new FileProperty("AZO Free", ScanField.Ignore),
            new FileProperty("DIN EN 12935 Certified", ScanField.Ignore),
            new FileProperty("NOMITE Certified", ScanField.Ignore),
            new FileProperty("ISO 9001 Certified", ScanField.Ignore),
            new FileProperty("ISO 14001 Certified", ScanField.Ignore),
            new FileProperty("ASTM Compliant", ScanField.Ignore),
            new FileProperty("CPSIA or CPSC Compliant", ScanField.Ignore),
            new FileProperty("CSA Compliant", ScanField.Ignore),
            new FileProperty("General Conformity Certificate", ScanField.Ignore),
            new FileProperty("CE Approved", ScanField.Ignore),
            new FileProperty("CertiPUR-US Certified", ScanField.Ignore),
        };

        private readonly IStorageProvider<SuryaHomewareVendor> _storageProvider;
        public SuryaPillowFileLoader(IStorageProvider<SuryaHomewareVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadProducts()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            var products = fileLoader.Load(Path.Combine(staticFileFolder, "SuryaPillows.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
            products.ForEach(x => x[ScanField.Category5] = "pillows");
            products.ForEach(x => x[ScanField.ProductName] = x[ScanField.ProductName].Replace(" with Down Fill", "").Replace(" with Poly Fill", "").Split('-').First());
            return products.GroupBy(x => x[ScanField.Pattern]).Select(x => new ScanData(x.First()) {Variants = x.ToList()}).ToList();
        }
    }
}