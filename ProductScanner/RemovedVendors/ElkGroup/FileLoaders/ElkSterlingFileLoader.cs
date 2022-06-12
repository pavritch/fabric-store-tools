using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace ElkGroup.FileLoaders
{
    public class ElkSterlingFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Item Number", ScanField.ManufacturerPartNumber),
            new FileProperty("Unit Price", ScanField.Cost),
            new FileProperty("MAP 2.0", ScanField.MAP),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("Description", ScanField.Description),
            new FileProperty("Long Description", ScanField.LongDescription),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("New Item", ScanField.Ignore),
            new FileProperty("Min Order Qty", ScanField.Ignore),
            new FileProperty("Sales UoM", ScanField.Ignore),
            new FileProperty("Truck Only", ScanField.Ignore),
            new FileProperty("Kitted Item", ScanField.Ignore),
            new FileProperty("Catalog", ScanField.Ignore),
            new FileProperty("Page", ScanField.Ignore),
            new FileProperty("Image Filename", ScanField.Ignore),
            new FileProperty("Image URL", ScanField.ImageUrl),
            // none here
            new FileProperty("Alternate Images", ScanField.Ignore),
            new FileProperty("Room Setting Images", ScanField.Ignore),
            new FileProperty("Catalog Page", ScanField.Ignore),
            new FileProperty("Drawing on File", ScanField.Ignore),
            new FileProperty("Item Height", ScanField.Height),
            new FileProperty("Item Width", ScanField.Width),
            new FileProperty("Item Length", ScanField.Length),
            new FileProperty("Item Extension", ScanField.Ignore),
            new FileProperty("Item Weight", ScanField.Weight),
            new FileProperty("Item Height 2", ScanField.Ignore),
            new FileProperty("Item Width 2", ScanField.Ignore),
            new FileProperty("Item Length 2", ScanField.Ignore),
            new FileProperty("Item Height 3", ScanField.Ignore),
            new FileProperty("Item Width 3", ScanField.Ignore),
            new FileProperty("Item Length 3", ScanField.Ignore),
            new FileProperty("Bulb Number", ScanField.Bulbs),
            new FileProperty("Bulb Wattage", ScanField.Ignore),
            new FileProperty("Bulb Type", ScanField.Ignore),
            new FileProperty("Bulb Included", ScanField.Ignore),
            new FileProperty("Bulb 2 Number", ScanField.Ignore),
            new FileProperty("Bulb 2 Wattage", ScanField.Ignore),
            new FileProperty("Bulb 2 Type", ScanField.Ignore),
            new FileProperty("Bulb 2 Included", ScanField.Ignore),
            new FileProperty("Voltage", ScanField.Ignore),
            new FileProperty("Lumens", ScanField.Ignore),
            new FileProperty("Color Temperature (K)", ScanField.Ignore),
            new FileProperty("CRI", ScanField.Ignore),
            new FileProperty("Switch Type", ScanField.Ignore),
            new FileProperty("Item Class", ScanField.Classification),
            new FileProperty("Item Type", ScanField.Category),
            new FileProperty("Item Style", ScanField.Ignore),
            new FileProperty("Item Substyle", ScanField.Ignore),
            new FileProperty("Item Collection", ScanField.Collection),
            new FileProperty("Related Items", ScanField.Ignore),
            new FileProperty("Item Finish", ScanField.Finish),
            new FileProperty("Item Materials", ScanField.Material),
            new FileProperty("Outdoor", ScanField.Ignore),
            new FileProperty("Safety Rating", ScanField.Ignore),
            new FileProperty("Mirror Length", ScanField.Ignore),
            new FileProperty("Mirror Height", ScanField.Ignore),
            new FileProperty("Number of Drawers", ScanField.Ignore),
            new FileProperty("License Name", ScanField.Ignore),
            new FileProperty("Licensor Name", ScanField.Ignore),
            new FileProperty("Selling Point 1", ScanField.Bullet1),
            new FileProperty("Selling Point 2", ScanField.Bullet2),
            new FileProperty("Selling Point 3", ScanField.Bullet3),
            new FileProperty("Selling Point 4", ScanField.Bullet4),
            new FileProperty("Selling Point 5", ScanField.Bullet5),
            new FileProperty("Number of Cartons", ScanField.Ignore),
            new FileProperty("Carton 1 Height", ScanField.Ignore),
            new FileProperty("Carton 1 Width", ScanField.Ignore),
            new FileProperty("Carton 1 Length", ScanField.Ignore),
            new FileProperty("Carton 1 Volume", ScanField.Ignore),
            new FileProperty("Carton 1 Weight", ScanField.Ignore),
            new FileProperty("Carton 2 Height", ScanField.Ignore),
            new FileProperty("Carton 2 Width", ScanField.Ignore),
            new FileProperty("Carton 2 Length", ScanField.Ignore),
            new FileProperty("Carton 2 Volume", ScanField.Ignore),
            new FileProperty("Carton 2 Weight", ScanField.Ignore),
            new FileProperty("Ship Notes", ScanField.Ignore),
            new FileProperty("Country of Origin", ScanField.Country),
        };

        private readonly IStorageProvider<ElkGroupVendor> _storageProvider;
        public ElkSterlingFileLoader(IStorageProvider<ElkGroupVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "Sterling_Data.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}