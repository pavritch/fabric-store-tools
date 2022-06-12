using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace KenroyHome
{
    public class KenroyHomeFileLoader : ProductFileLoader<KenroyHomeVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Upc", ScanField.UPC),
            new FileProperty("ItemNumber", ScanField.ManufacturerPartNumber),
            new FileProperty("ItemDescription", ScanField.Description),
            new FileProperty("ProductLine", ScanField.PatternNumber),
            new FileProperty("ProductLineDescription", ScanField.Category),
            new FileProperty("Unit Price", ScanField.Cost),
            new FileProperty("Weight", ScanField.Weight),
            new FileProperty("Mpc_Weight", ScanField.ShippingWeight),
            new FileProperty("Length", ScanField.Length),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("Height", ScanField.Height),
            new FileProperty("Cube", ScanField.Ignore),
            new FileProperty("Qty Available", ScanField.StockCount),
            new FileProperty("ProductType", ScanField.ProductType),
            new FileProperty("Available Date", ScanField.LeadTime),
        };

        public KenroyHomeFileLoader(IStorageProvider<KenroyHomeVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { }
    }
}