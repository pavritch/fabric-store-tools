using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Scalamandre.Details
{
    public class ScalamandreFileLoader : ProductFileLoader<ScalamandreVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("CATEGORY", ScanField.ProductGroup),
            new FileProperty("PATTERN NUMBER", ScanField.ItemNumber),
            new FileProperty("PATTERN_NAME", ScanField.Pattern),
            new FileProperty("ORIG_SKU", ScanField.Ignore),
            new FileProperty("DESCRIPTION", ScanField.Description),
            new FileProperty("NETPRICE", ScanField.Cost),
            new FileProperty("WIDTH", ScanField.Width),
            new FileProperty("MINIMUMS", ScanField.MinimumQuantity),
            new FileProperty("INCREMENTS", ScanField.OrderIncrement),
            new FileProperty("BRAND", ScanField.Brand),
            new FileProperty("COLOR", ScanField.Color),
            new FileProperty("UNIT OF MEASURE", ScanField.UnitOfMeasure),
            new FileProperty("COUNTRY OF ORIGIN", ScanField.Country),
            new FileProperty("STYLE", ScanField.Style),
            new FileProperty("FINISH", ScanField.Finish),
            new FileProperty("COLLECTION", ScanField.Collection),
            new FileProperty("CONTENT", ScanField.Content),
            new FileProperty("VERTICAL REPEAT", ScanField.VerticalRepeat),
            new FileProperty("HORIZONTAL REPEAT", ScanField.HorizontalRepeat),
            new FileProperty("MATCH", ScanField.Match),
            new FileProperty("USE", ScanField.Use),
            new FileProperty("CLEANING INSTRUCTIONS", ScanField.Cleaning),
            new FileProperty("DIRECTION", ScanField.Direction),
            new FileProperty("ROLL SIZE", ScanField.Length),
            new FileProperty("WHERE STOCKED", ScanField.Ignore),
            new FileProperty("IMAGEVALID", ScanField.Ignore),
            new FileProperty("IMAGEPATH", ScanField.ImageUrl),
            new FileProperty("DISCONTINUED", ScanField.IsDiscontinued),
            new FileProperty("TOBEDROPPED", ScanField.Ignore),
            new FileProperty("STOCKINVENTORY", ScanField.StockInventory),
            new FileProperty("INVENTORY_LEVEL", ScanField.StockCount),
            new FileProperty("TRIMCLASS", ScanField.Ignore),
            new FileProperty("WASHABILITY", ScanField.Ignore),
            new FileProperty("DESIGNED BY", ScanField.Ignore),
            new FileProperty("DESIGN COMPANY", ScanField.Ignore),
            new FileProperty("DESIGN", ScanField.Design),
            new FileProperty("INDOOR OUT DOOR PAINT", ScanField.Ignore),
            new FileProperty("MATERIAL TYPE", ScanField.Material),
            new FileProperty("DESIGN TYPE", ScanField.Ignore),
            new FileProperty("SCALE", ScanField.Ignore),
            new FileProperty("END-USE QUALITY", ScanField.Ignore),
            new FileProperty("CFA REQUIRED", ScanField.Ignore),
            new FileProperty("WIDE WIDTH", ScanField.Ignore),
            new FileProperty("INDOOR / OUTDOOR", ScanField.Ignore),
            new FileProperty("CONTRACT / PERFORMANCE", ScanField.Ignore),
            new FileProperty("COLOR ATTRIBUTE", ScanField.Ignore),
            new FileProperty("DISCONTINUED1", ScanField.Ignore),
            new FileProperty("DISCONTINUEDON", ScanField.Ignore),
            new FileProperty("WEB SOLD BY", ScanField.Ignore),
            new FileProperty("WEB PACKAGED BY", ScanField.Ignore),
            new FileProperty("PRODUCT WEIGHT", ScanField.Ignore),
            new FileProperty("WEB CLIENT MIN ORDER", ScanField.Ignore),
            new FileProperty("LEAD TIME", ScanField.Ignore),
            new FileProperty("SOI", ScanField.Ignore),
            new FileProperty("OLDSKU", ScanField.ManufacturerPartNumber),
        };


        public ScalamandreFileLoader(IStorageProvider<ScalamandreVendor> storageProvider) 
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xls)
        {
        }
    }
}