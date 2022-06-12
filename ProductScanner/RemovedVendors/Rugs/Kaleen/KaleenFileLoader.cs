using System.Collections.Generic;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Kaleen
{
    public class KaleenFileLoader : ProductFileLoader<KaleenVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Kaleen UPC Code", ScanField.UPC),
            new FileProperty("Kaleen Product SKU Number", ScanField.ManufacturerPartNumber),
            new FileProperty("FTP Site Image ID", ScanField.Image1),
            new FileProperty("Rug Contruction", ScanField.Construction),
            new FileProperty("Content/Fiber", ScanField.Content),
            new FileProperty("Collection Name", ScanField.Collection),
            new FileProperty("Website Copy Description", ScanField.Description),
            new FileProperty("Design ID", ScanField.PatternNumber),
            new FileProperty("Color ", ScanField.ColorName),
            new FileProperty("Rug Size", ScanField.Size),
            new FileProperty("Dealer Cost", ScanField.Cost),
            new FileProperty("MAP Prices", ScanField.MAP),
            new FileProperty("MSRP", ScanField.RetailPrice),

            // captured in the Size property
            new FileProperty("Rug Shape", ScanField.Ignore),
            new FileProperty("Square Feet", ScanField.Ignore),

            new FileProperty("Rug Weight (LBS.)", ScanField.Weight),
            new FileProperty("Length Rolled (inches)", ScanField.PackageLength),
            new FileProperty("Width Rolled (inches)", ScanField.PackageWidth),
            new FileProperty("Height Rolled (inches)", ScanField.PackageHeight),
            new FileProperty("Pile Height (inches)", ScanField.PileHeight),
            new FileProperty("Origin of Rug", ScanField.Country),
            new FileProperty("Lifestyle", ScanField.Design),
            new FileProperty("Detailed Rug Colors", ScanField.Color1),
            new FileProperty("Bullet Point #1", ScanField.Bullet1),
            new FileProperty("Bullet Point #2", ScanField.Bullet2),
            new FileProperty("Bullet Point #3", ScanField.Bullet3),
            new FileProperty("Ships Via", ScanField.Ignore),
            new FileProperty("Harmonized Code", ScanField.Ignore),
            new FileProperty("Freight Class", ScanField.Ignore),
            new FileProperty("NMFC Class", ScanField.Ignore),
            new FileProperty("Rug Was Created for Running Line", ScanField.Ignore),

            new FileProperty("Column 32", ScanField.Ignore),
            new FileProperty("Column 33", ScanField.Ignore),
            new FileProperty("Column 34", ScanField.Ignore),
            new FileProperty("Column 35", ScanField.Ignore),
        };

        public KaleenFileLoader(IStorageProvider<KaleenVendor> storageProvider)
            : base(storageProvider, ScanField.UPC, Properties, ProductFileType.Xlsx, 3, 4) { }
    }
}