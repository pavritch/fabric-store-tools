using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace JEnnis
{
    public class JEnnisFileLoader : ProductFileLoader<JEnnisVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("MPN", ScanField.ManufacturerPartNumber),
            new FileProperty("Brand", ScanField.Brand),
            new FileProperty("Pattern Name", ScanField.PatternName),
            new FileProperty("Color Name/Number", ScanField.ColorName),
            new FileProperty("AbbeyShea Pattern Name", ScanField.Ignore),
            new FileProperty("AbbeyShea Color Name", ScanField.Ignore),
            new FileProperty("Discontinued", ScanField.IsDiscontinued),
            new FileProperty("Image Url", ScanField.ImageUrl),
            new FileProperty("UOM", ScanField.UnitOfMeasure),
            new FileProperty("Product Group", ScanField.ProductGroup),
            new FileProperty("Cost", ScanField.Cost),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("Min Qty", ScanField.MinimumQuantity),
            new FileProperty("Order Increment", ScanField.OrderIncrement),
            new FileProperty("Book", ScanField.Book),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("Product Use", ScanField.ProductUse),
            new FileProperty("Country", ScanField.Country),
            new FileProperty("Content", ScanField.Content),
            new FileProperty("Vertical Repeat", ScanField.VerticalRepeat),
            new FileProperty("Horizontal Repeat", ScanField.HorizontalRepeat),
            new FileProperty("Color", ScanField.Color),
            new FileProperty("Order Info", ScanField.OrderInfo),
            new FileProperty("Finish", ScanField.Finish),
            new FileProperty("Cleaning", ScanField.Ignore),
            new FileProperty("Cleaning Code", ScanField.Cleaning),
            new FileProperty("Durability", ScanField.Durability),
            new FileProperty("Match", ScanField.Match),
            new FileProperty("Prepasted", ScanField.Prepasted),
            new FileProperty("Strippable", ScanField.Strippable),
            new FileProperty("Packaging", ScanField.Packaging),
            new FileProperty("Railroaded", ScanField.Railroaded),
            new FileProperty("Style", ScanField.Style),
            new FileProperty("Lead Time", ScanField.LeadTime),
            new FileProperty("Material", ScanField.Material),
        };

        public JEnnisFileLoader(IStorageProvider<JEnnisVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties) { }
    }
}