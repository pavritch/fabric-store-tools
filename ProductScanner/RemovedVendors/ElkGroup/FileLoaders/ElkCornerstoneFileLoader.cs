using System.Collections.Generic;
using System.IO;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace ElkGroup.FileLoaders
{
    public class ElkCornerstoneFileLoader
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Part Number", ScanField.ManufacturerPartNumber),
            new FileProperty("Distributor Net", ScanField.Cost),
            new FileProperty("MAP", ScanField.MAP),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("Item Description", ScanField.Description),
            new FileProperty("Long Description", ScanField.LongDescription),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("New Item", ScanField.Ignore),
            new FileProperty("Truck Only", ScanField.Ignore),
            new FileProperty("Catalog", ScanField.Ignore),
            new FileProperty("Page", ScanField.Ignore),
            new FileProperty("Image", ScanField.Ignore),
            new FileProperty("URL Link to Image", ScanField.ImageUrl),
            new FileProperty("Alternate Image", ScanField.Ignore),
            new FileProperty("Room setting Image", ScanField.Ignore),
            new FileProperty("Length", ScanField.Length),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("Height", ScanField.Height),
            new FileProperty("Extension", ScanField.Ignore),
            new FileProperty("Weight (LBS)", ScanField.Weight),
            new FileProperty("Backplate/Canopy Size", ScanField.Ignore),
            new FileProperty("Bulb #", ScanField.Bulbs),
            new FileProperty("Bulb watt", ScanField.Wattage),
            new FileProperty("Bulb Type", ScanField.Ignore),
            new FileProperty("Bulb Included", ScanField.Ignore),
            new FileProperty("Item Collection", ScanField.Collection),
            new FileProperty("Item Class", ScanField.Classification),
            new FileProperty("item type", ScanField.ProductType),
            new FileProperty("Outdoor", ScanField.Ignore),
            new FileProperty("Material", ScanField.Material),
            new FileProperty("item finish", ScanField.Finish),
            new FileProperty("Glass finish", ScanField.Ignore),
            new FileProperty("Replacement Glass #", ScanField.Ignore),
            new FileProperty("Additional Chain SKU Number", ScanField.Ignore),
            new FileProperty("Available with LED", ScanField.Ignore),
            new FileProperty("Available with EEF", ScanField.Ignore),
            new FileProperty("Bullet Point 1", ScanField.Bullet1),
            new FileProperty("Bullet Point 2", ScanField.Bullet2),
            new FileProperty("Number of Cartons", ScanField.Ignore),
            new FileProperty("Carton Length", ScanField.Ignore),
            new FileProperty("Carton Width", ScanField.Ignore),
            new FileProperty("Carton Height", ScanField.Ignore),
            new FileProperty("Carton Weight", ScanField.Ignore),
            new FileProperty("Country of Origin", ScanField.Country),
        };

        private readonly IStorageProvider<ElkGroupVendor> _storageProvider;
        public ElkCornerstoneFileLoader(IStorageProvider<ElkGroupVendor> storageProvider) { _storageProvider = storageProvider; }

        public List<ScanData> LoadInventoryData()
        {
            var staticFileFolder = _storageProvider.GetStaticFolder();
            var fileLoader = new ExcelFileLoader();
            return fileLoader.Load(Path.Combine(staticFileFolder, "Cornerstone_Data.xlsx"), Properties, ScanField.ManufacturerPartNumber, 1, 2);
        }
    }
}