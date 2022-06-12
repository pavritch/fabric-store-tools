using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace YourOtherWarehouse
{
    public class YourOtherWarehouseFileLoader : ProductFileLoader<YourOtherWarehouseVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("OMSID", ScanField.Ignore),
            new FileProperty("YOW ITEM NUMBER", ScanField.ManufacturerPartNumber),
            new FileProperty("NEW ITEM?", ScanField.Ignore),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("BRAND", ScanField.Brand),
            new FileProperty("PRODUCT NAME", ScanField.ProductName),
            new FileProperty("MODEL NUMBER", ScanField.Ignore),
            new FileProperty("FINISH / COLOR", ScanField.Color),
            new FileProperty("CLASS", ScanField.Classification),
            new FileProperty("SUBCLASS", ScanField.ProductType),
            new FileProperty("CATEGORY", ScanField.Category),
            new FileProperty("LIST PRICE", ScanField.RetailPrice),
            new FileProperty("PRODUCT IMAGE", ScanField.Image1),
            new FileProperty("MARKETING COPY", ScanField.Description),
            new FileProperty("BULLET 1", ScanField.Bullet1),
            new FileProperty("BULLET 2", ScanField.Bullet2),
            new FileProperty("BULLET 3", ScanField.Bullet3),
            new FileProperty("BULLET 4", ScanField.Bullet4),
            new FileProperty("BULLET 5", ScanField.Bullet5),
            new FileProperty("BULLET 6", ScanField.Bullet6),
            new FileProperty("BULLET 7", ScanField.Bullet7),
            new FileProperty("BULLET 8", ScanField.Bullet8),
            new FileProperty("BULLET 9", ScanField.Bullet9),
            new FileProperty("BULLET 10", ScanField.Bullet10),
            new FileProperty("PRODUCT WEIGHT (LBS.)", ScanField.Weight),
            new FileProperty("PRODUCT LENGTH (IN)", ScanField.Length),
            new FileProperty("PRODUCT HEIGHT (IN)", ScanField.Height),
            new FileProperty("PRODUCT WIDTH (IN)", ScanField.Width),
            new FileProperty("ITEM BOXED WEIGHT, EACH (LBS.)", ScanField.Ignore),
            new FileProperty("ITEM BOXED LENGTH, EACH (IN)", ScanField.Ignore),
            new FileProperty("ITEM BOXED HEIGHT, EACH (IN)", ScanField.Ignore),
            new FileProperty("ITEM BOXED WIDTH, EACH (IN)", ScanField.Ignore),
            new FileProperty("SHIPPING METHOD", ScanField.ShippingMethod),
            new FileProperty("COUNTRY OF ORIGIN CODE", ScanField.Country),
            new FileProperty("WATERSENSE QUALIFIED?", ScanField.Ignore),
        };

        public YourOtherWarehouseFileLoader(IStorageProvider<YourOtherWarehouseVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { }
    }
}