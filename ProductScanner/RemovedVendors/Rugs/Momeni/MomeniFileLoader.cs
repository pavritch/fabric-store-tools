using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Momeni
{
    public class MomeniFileLoader : ProductFileLoader<MomeniVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Rug ID", ScanField.ManufacturerPartNumber),
            new FileProperty("Discontinued", ScanField.IsDiscontinued),
            new FileProperty("Image ID", ScanField.SKU),
            new FileProperty("UPC", ScanField.UPC),
            new FileProperty("UPC 5 Digit ", ScanField.AlternateItemNumber),
            new FileProperty("Collection", ScanField.Collection),
            new FileProperty("Design", ScanField.PatternNumber),
            new FileProperty("Construction", ScanField.Construction),
            new FileProperty("Country", ScanField.Country),
            new FileProperty("Material", ScanField.Content),
            new FileProperty("Backing", ScanField.Backing),
            new FileProperty("Color", ScanField.Color1),
            new FileProperty("Color Family", ScanField.ColorGroup),
            new FileProperty("Size", ScanField.Size),
            new FileProperty("Stanard Size", ScanField.Ignore),
            new FileProperty("Pile Height", ScanField.PileHeight),
            new FileProperty("Shape", ScanField.Ignore),
            new FileProperty("Actual Length (Inch)", ScanField.Length),
            new FileProperty("Actual Width (Inch)", ScanField.Width),
            new FileProperty("Cost", ScanField.Cost),
            new FileProperty("MAP", ScanField.MAP),
            new FileProperty("MSRP", ScanField.RetailPrice),
            new FileProperty("Area", ScanField.Ignore),
            new FileProperty("Weight(Lb)", ScanField.Weight),
            new FileProperty("Package Length", ScanField.PackageLength),
            new FileProperty("Package Width", ScanField.PackageWidth),
            new FileProperty("Package Height", ScanField.PackageHeight),
            new FileProperty("Vol", ScanField.Ignore),
            new FileProperty("Style", ScanField.Category),
            new FileProperty("Pattern", ScanField.Style),
            new FileProperty("Keyword", ScanField.Ignore),
            new FileProperty("Web Description", ScanField.Description),
        };

        public MomeniFileLoader(IStorageProvider<MomeniVendor> storageProvider)
            : base(storageProvider, ScanField.UPC, Properties) { }
    }
}