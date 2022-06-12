using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace MoesHome
{
    public class MoesProductFileLoader : ProductFileLoader<MoesHomeVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("APROX. QTY IN STOCK AS OF TODAY", ScanField.StockCount),
            new FileProperty("NEW ITEMS", ScanField.Ignore),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("Moe's Model Number", ScanField.ManufacturerPartNumber),
            new FileProperty("Product Name", ScanField.ProductName),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("Category", ScanField.Breadcrumbs),
            new FileProperty("Ad-Copy", ScanField.Description),
            new FileProperty("Wholesale Price", ScanField.Cost),
            new FileProperty("MAP Price (Sale Price)", ScanField.MAP),
            new FileProperty("Retail Price (MSRP)", ScanField.RetailPrice),
            new FileProperty("Minimum Order Quantity", ScanField.MinimumQuantity),
            new FileProperty("Width (inches)", ScanField.Width),
            new FileProperty("Depth  (inches)", ScanField.Depth),
            new FileProperty("Height  (inches)", ScanField.Height),
            new FileProperty("Product Weight  (lbs)", ScanField.Weight),
            new FileProperty("4th Free Text Dimension (inches)", ScanField.Ignore),
            new FileProperty("Material 1", ScanField.Material),
            new FileProperty("Material 2", ScanField.Material2),
            new FileProperty("Material 3", ScanField.Material3),
            new FileProperty("Material 4", ScanField.Material4),
            new FileProperty("Feature 1", ScanField.Ignore),
            new FileProperty("Feature 2", ScanField.Ignore),
            new FileProperty("Feature 3", ScanField.Ignore),
            new FileProperty("Feature 4", ScanField.Ignore),
            new FileProperty("POWER (WATT/VOLT)", ScanField.Wattage),
            new FileProperty("CORD LENGTH", ScanField.CordLength),
            new FileProperty("NUMBER OF BULBS", ScanField.Bulbs),
            new FileProperty("Assembly Required? (Y/N)", ScanField.AssemblyInstruction),
            new FileProperty("Distressed Finish (Y/N)", ScanField.Ignore),
            new FileProperty("Country of Manufacture", ScanField.Country),
            new FileProperty("Ship Type (small parcel, LTL)", ScanField.Ignore),
            new FileProperty("Lead Time (hrs)", ScanField.Ignore),
            new FileProperty("Freight Class", ScanField.Ignore),
            new FileProperty("Number of Cartons/Item", ScanField.Ignore),
            new FileProperty("Carton Width (Box 1)", ScanField.Ignore),
            new FileProperty("Carton Depth (Box 1)", ScanField.Ignore),
            new FileProperty("Carton Height (Box 1)", ScanField.Ignore),
            new FileProperty("Carton Weight (Box 1)", ScanField.Ignore),
            new FileProperty("Carton Width (Box 2)", ScanField.Ignore),
            new FileProperty("Carton Depth (Box 2)", ScanField.Ignore),
            new FileProperty("Carton Height (Box 2)", ScanField.Ignore),
            new FileProperty("Carton Weight (Box 2)", ScanField.Ignore),
            new FileProperty("Carton Width (Box 3)", ScanField.Ignore),
            new FileProperty("Carton Depth (Box 3)", ScanField.Ignore),
            new FileProperty("Carton Height (Box 3)", ScanField.Ignore),
            new FileProperty("Carton Weight (Box 3)", ScanField.Ignore),
            new FileProperty("Carton Width (Box 4)", ScanField.Ignore),
            new FileProperty("Carton Depth (Box 4)", ScanField.Ignore),
            new FileProperty("Carton Height (Box 4)", ScanField.Ignore),
            new FileProperty("Carton Weight (Box 4)", ScanField.Ignore),
            new FileProperty("Carton Width (Box 5)", ScanField.Ignore),
            new FileProperty("Carton Depth (Box 5)", ScanField.Ignore),
            new FileProperty("Carton Height (Box 5)", ScanField.Ignore),
            new FileProperty("Carton Weight (Box 5)", ScanField.Ignore),
            new FileProperty("Carton Width (Box 6)", ScanField.Ignore),
            new FileProperty("Carton Depth (Box 6)", ScanField.Ignore),
            new FileProperty("Carton Height (Box 6)", ScanField.Ignore),
            new FileProperty("Carton Weight (Box 6)", ScanField.Ignore),
            new FileProperty("Warranty Length (1 Year)", ScanField.Warranty),
            new FileProperty("Warranty Term (Covered on any manufacturer defect)", ScanField.Ignore),
            new FileProperty("Additional Warning or Disclaimers", ScanField.Ignore),
        };

        public MoesProductFileLoader(IStorageProvider<MoesHomeVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xlsx, 2, 4) { }
    }
}