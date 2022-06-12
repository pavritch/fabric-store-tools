using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace SuryaHomeware
{
    public class SuryaThrowFileLoader
    {
        private readonly List<FileProperty> _properties = new List<FileProperty>
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
            new FileProperty("Brand", ScanField.Ignore),
            new FileProperty("Brand Copy", ScanField.Ignore),
            new FileProperty("Romance Copy", ScanField.Ignore),
            new FileProperty("MSRP", ScanField.Ignore),
            new FileProperty("EMAP", ScanField.Ignore),
            new FileProperty("Ecom Cost", ScanField.Cost),
            new FileProperty("Number of Items Included", ScanField.Ignore),
            new FileProperty("Pieces Included", ScanField.Ignore),
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
            new FileProperty("Navigational Color", ScanField.Ignore),
            new FileProperty("Colors", ScanField.Ignore),
            new FileProperty("Pantone #", ScanField.Ignore),
            new FileProperty("Style", ScanField.Ignore),
            new FileProperty("Pattern", ScanField.Ignore),
            new FileProperty("Shape", ScanField.Ignore),
            new FileProperty("Heated", ScanField.Ignore),
            new FileProperty("Electric", ScanField.Ignore),
            new FileProperty("Power Source", ScanField.Ignore),
            new FileProperty("Control Type", ScanField.Ignore),
            new FileProperty("Dual Controllers", ScanField.Ignore),
            new FileProperty("Auto Shut Off Time(Hours)", ScanField.Ignore),
            new FileProperty("Wattage(Watts)", ScanField.Ignore),
            new FileProperty("Voltage(Volts)", ScanField.Ignore),
            new FileProperty("Embroidered", ScanField.Ignore),
            new FileProperty("Fill Type", ScanField.Ignore),
            new FileProperty("Reversible", ScanField.Ignore),
            new FileProperty("Reverse Side Color", ScanField.Ignore),
            new FileProperty("Reverse Side Material", ScanField.Ignore),
            new FileProperty("Reverse Side Pattern", ScanField.Ignore),
            new FileProperty("Gender", ScanField.Ignore),
            new FileProperty("Licensed Product", ScanField.Ignore),
            new FileProperty("Thread Count", ScanField.Ignore),
            new FileProperty("Fringe", ScanField.Ignore),
            new FileProperty("Fringe Type", ScanField.Ignore),
            new FileProperty("Hand Sewn", ScanField.Ignore),
            new FileProperty("Drying Method", ScanField.Ignore),
            new FileProperty("Pre-Shrunk", ScanField.Ignore),
            new FileProperty("Converts into Bag", ScanField.Ignore),
            new FileProperty("Swatch Available", ScanField.Ignore),
            new FileProperty("Outdoor Safe?", ScanField.Ignore),
            new FileProperty("Product Care", ScanField.Ignore),
            new FileProperty("Product Warranty", ScanField.Ignore),
            new FileProperty("Assembly Required?", ScanField.Ignore),
            new FileProperty("Product Weight - Lbs", ScanField.Ignore),
            new FileProperty("Product Weight - G", ScanField.Ignore),
            new FileProperty("Size - In", ScanField.Ignore),
            new FileProperty("Size - Cm", ScanField.Ignore),
            new FileProperty("Width - In", ScanField.Ignore),
            new FileProperty("Length - In", ScanField.Ignore),
            new FileProperty("Width - Cm", ScanField.Ignore),
            new FileProperty("Length - Cm", ScanField.Ignore),
            new FileProperty("Custom Size Available?", ScanField.Ignore),
            new FileProperty("Main Image", ScanField.Ignore),
            new FileProperty("Additional Image 1", ScanField.Ignore),
            new FileProperty("Additional Image 2", ScanField.Ignore),
            new FileProperty("Additional Image 3", ScanField.Ignore),
            new FileProperty("Additional Image 4", ScanField.Ignore),
            new FileProperty("Additional Products Shown in Images", ScanField.Ignore),
            new FileProperty("Video Link", ScanField.Ignore),
            new FileProperty("Lead Time", ScanField.Ignore),
            new FileProperty("Ship Method", ScanField.Ignore),
            new FileProperty("NMFC Code", ScanField.Ignore),
            new FileProperty("Freight Class", ScanField.Ignore),
            new FileProperty("Carton Weight - Lbs", ScanField.Ignore),
            new FileProperty("Carton Weight - G", ScanField.Ignore),
            new FileProperty("Shipping Dimensions - In", ScanField.Ignore),
            new FileProperty("Shipping Dimensions - Cm", ScanField.Ignore),
            new FileProperty("Carton Height - In", ScanField.Ignore),
            new FileProperty("Carton Width - In", ScanField.Ignore),
            new FileProperty("Carton Depth - In", ScanField.Ignore),
            new FileProperty("Carton Height - Cm", ScanField.Ignore),
            new FileProperty("Carton Width - Cm", ScanField.Ignore),
            new FileProperty("Carton Depth - Cm", ScanField.Ignore),
            new FileProperty("Eco-Friendly", ScanField.Ignore),
            new FileProperty("Fade Resistant", ScanField.Ignore),
            new FileProperty("Odor Resistant", ScanField.Ignore),
            new FileProperty("Mildew Resistant", ScanField.Ignore),
            new FileProperty("Bed Bug Resistant", ScanField.Ignore),
            new FileProperty("Dust Mite Resistant", ScanField.Ignore),
            new FileProperty("Moisture Wicking", ScanField.Ignore),
            new FileProperty("Antimicrobial", ScanField.Ignore),
            new FileProperty("Safe for Infants", ScanField.Ignore),
            new FileProperty("Water Resistant", ScanField.Ignore),
            new FileProperty("Water Resistant Details", ScanField.Ignore),
            new FileProperty("Stain Resistant", ScanField.Ignore),
            new FileProperty("Hypoallergenic", ScanField.Ignore),
            new FileProperty("Organic", ScanField.Ignore),
            new FileProperty("Recycled Content", ScanField.Ignore),
            new FileProperty("Total Recycled Content (Percentage)", ScanField.Ignore),
            new FileProperty("Post-Consumer Content (Percentage)", ScanField.Ignore),
            new FileProperty("Remanufactured/Refurbished", ScanField.Ignore),
            new FileProperty("UL Listed", ScanField.Ignore),
            new FileProperty("cUL Listed", ScanField.Ignore),
            new FileProperty("Greenguard Certified", ScanField.Ignore),
            new FileProperty("Certified Organic", ScanField.Ignore),
            new FileProperty("Öeko-Tex Standard Compliant", ScanField.Ignore),
            new FileProperty("DIN EN 12935 Certified", ScanField.Ignore),
            new FileProperty("NOMITE Certified", ScanField.Ignore),
            new FileProperty("AZO Free", ScanField.Ignore),
            new FileProperty("GOTS Certified", ScanField.Ignore),
        };
        
        private readonly IStorageProvider<SuryaHomewareVendor> _storageProvider;
        public SuryaThrowFileLoader(IStorageProvider<SuryaHomewareVendor> storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public List<ScanData> LoadProducts()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            var products = fileLoader.Load(Path.Combine(staticFileFolder, "SuryaThrows.xlsx"), _properties, ScanField.ManufacturerPartNumber, 1, 2);
            products.ForEach(x => x[ScanField.Category5] = "throws");
            return products;
        }
    }
}