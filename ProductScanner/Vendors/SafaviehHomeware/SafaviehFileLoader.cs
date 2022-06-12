using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace SafaviehHomeware
{
    public class SafaviehFileLoader : ProductFileLoader<SafaviehHomewareVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("#", ScanField.Ignore),
            new FileProperty("Manufacturer Model Number", ScanField.ManufacturerPartNumber),
            new FileProperty("Discontinued", ScanField.IsDiscontinued),
            new FileProperty("UPC Code", ScanField.UPC),
            new FileProperty("Product Name", ScanField.ProductName),
            new FileProperty("Wholesale Price", ScanField.Cost),
            // these are pretty much all empty
            new FileProperty("MAP Price (Sale Price)", ScanField.Ignore),
            new FileProperty("Full Retail Price (MSRP)", ScanField.Ignore),
            new FileProperty("Minimum Order Quantity (Per Part #)", ScanField.Ignore),
            new FileProperty("Actual Product Weight", ScanField.Weight),
            new FileProperty("Product Min Height", ScanField.Height),
            // only 6 that are different from min height
            new FileProperty("Product Max Height", ScanField.Ignore),
            new FileProperty("Product Min Width", ScanField.Width),
            // only 3 that are different from min width
            new FileProperty("Product Max Width", ScanField.Ignore),
            new FileProperty("Product Min Depth", ScanField.Depth),
            // always the same as min depth
            new FileProperty("Product Max Depth", ScanField.Ignore),
            // never filled in
            new FileProperty("Square Feet Per Carton", ScanField.Ignore),

            new FileProperty("Option 2 Value", ScanField.Ignore),
            new FileProperty("Option 3 Content", ScanField.Material),
            new FileProperty("Option 3 Value Color", ScanField.Color),
            new FileProperty("Insert", ScanField.Ignore),
            // Most columns are 'Safavieh', but lots have material stuff in here
            new FileProperty("Collection Name", ScanField.Ignore),
            new FileProperty("Collection Ad Copy", ScanField.Description),
            new FileProperty("Paragraph description (if exist)", ScanField.Ignore),
            new FileProperty("Feature 1", ScanField.Ignore),
            new FileProperty("Feature 2", ScanField.Ignore),
            new FileProperty("Feature 3", ScanField.Ignore),
            new FileProperty("Feature 4", ScanField.Ignore),
            new FileProperty("Feature 5", ScanField.Ignore),
            new FileProperty("Feature 6", ScanField.Ignore),
            new FileProperty("Feature 7", ScanField.Ignore),
            new FileProperty("Feature 8", ScanField.Ignore),
            new FileProperty("Feature 9", ScanField.Ignore),
            new FileProperty("Specification 1", ScanField.Ignore),
            new FileProperty("Specification 2", ScanField.Ignore),
            new FileProperty("Specification 3", ScanField.Ignore),
            new FileProperty("Specification 4", ScanField.Ignore),
            new FileProperty("Specification 5", ScanField.Ignore),
            new FileProperty("Specification 6", ScanField.Ignore),
            new FileProperty("Assembly Required? (Y/N)", ScanField.AssemblyInstruction),
            new FileProperty("Distressed Finish? (Y/N)", ScanField.Finish),
            new FileProperty("Country of Manufacture", ScanField.Country),
            new FileProperty("Harmonized Code", ScanField.Ignore),
            new FileProperty("Canada Code (for Import)", ScanField.Ignore),
            new FileProperty("Ship Type (small parcel, LTL)", ScanField.Ignore),
            new FileProperty("Supplier Lead Time in Business Day Hours", ScanField.Ignore),
            new FileProperty("Supplier Lead Time in Business Day Hours for Replacement Parts", ScanField.Ignore),
            new FileProperty("Freight Class", ScanField.Ignore),
            new FileProperty("Number of Boxes", ScanField.Ignore),
            new FileProperty("Shipping Weight (Box 1)", ScanField.ShippingWeight),
            new FileProperty("Carton Height (Box 1)", ScanField.PackageHeight),
            new FileProperty("Carton Width (Box 1)", ScanField.PackageWidth),
            new FileProperty("Carton Depth (Box 1)", ScanField.PackageLength),
            new FileProperty("Shipping Weight (Box 2)", ScanField.Ignore),
            new FileProperty("Carton Height (Box 2)", ScanField.Ignore),
            new FileProperty("Carton Width (Box 2)", ScanField.Ignore),
            new FileProperty("Carton Depth (Box 2)", ScanField.Ignore),
            new FileProperty("Shipping Weight (Box 3)", ScanField.Ignore),
            new FileProperty("Carton Height (Box 3)", ScanField.Ignore),
            new FileProperty("Carton Width (Box 3)", ScanField.Ignore),
            new FileProperty("Carton Depth (Box 3)", ScanField.Ignore),
            new FileProperty("Image 1 File Name", ScanField.Ignore),
            new FileProperty("Image 2 File Name", ScanField.Ignore),
            new FileProperty("Image 3 File Name", ScanField.Ignore),
            new FileProperty("PDF 1 File Name", ScanField.Ignore),
            new FileProperty("PDF 2 File Name", ScanField.Ignore),
            new FileProperty("Warranty Length", ScanField.Ignore),
            new FileProperty("Warranty Term", ScanField.Ignore),
            new FileProperty("Additional Warning or Disclaimers", ScanField.Ignore),
            new FileProperty("General Conformity Certificate", ScanField.Ignore),
            new FileProperty("ISTA 3A Certified (Y/N)", ScanField.Ignore),
            new FileProperty("CPSIA - Small Parts Warning Code (0-6)", ScanField.Ignore),
            new FileProperty("Composite Wood (CARB) Code (0-5)", ScanField.Ignore),
            new FileProperty("UL Certification #", ScanField.Ignore),
            new FileProperty("ETL Certification #", ScanField.Ignore),
            new FileProperty("Image 1", ScanField.Image1),
            new FileProperty("Image 2", ScanField.Image2),
            new FileProperty("Image 3", ScanField.Image3),
            new FileProperty("Image 4", ScanField.Image4),
            new FileProperty("Image 5", ScanField.Image5),
            new FileProperty("*", ScanField.Ignore),
            new FileProperty("Column", ScanField.Ignore),
            new FileProperty("*2", ScanField.Ignore),
            new FileProperty("Column 84", ScanField.Ignore),
            new FileProperty("Column 85", ScanField.Ignore),
            new FileProperty("Column 86", ScanField.Ignore),
            new FileProperty("Column 87", ScanField.Ignore),
            new FileProperty("Column 88", ScanField.Ignore),
        };

        public SafaviehFileLoader(IStorageProvider<SafaviehHomewareVendor> storageProvider)
            : base(storageProvider, ScanField.ProductName, Properties) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x => x.Cost = x[ScanField.Cost].Replace("$", "").ToDecimalSafe());
            return products;
        }
    }
}