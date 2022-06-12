using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Jaipur
{
    public class JaipurProductFileLoader : ProductFileLoader<JaipurVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Product Category", ScanField.ProductType),
            new FileProperty("Item sub type", ScanField.Ignore),
            new FileProperty("UPC Code", ScanField.UPC),
            new FileProperty("Item Number", ScanField.ManufacturerPartNumber),
            new FileProperty("ITEM NUMBER Poly Fill/Down Fill", ScanField.Ignore),
            new FileProperty("Catalog Code", ScanField.Code),
            new FileProperty("Category", ScanField.Category),
            new FileProperty("NEW OR CONTINUITY", ScanField.Ignore),
            new FileProperty("Collection", ScanField.Ignore),
            new FileProperty("Design(US)", ScanField.Ignore),
            new FileProperty("Ground Color", ScanField.Ignore),
            new FileProperty("Border Color", ScanField.Ignore),
            new FileProperty("Color Family1", ScanField.Ignore),
            new FileProperty("Color Family2", ScanField.Ignore),
            new FileProperty("Color Family", ScanField.Ignore),
            new FileProperty("Product Size (in Feet)", ScanField.Ignore),
            new FileProperty("Product Size-2 (in Feet)", ScanField.Size),
            new FileProperty("Size Group", ScanField.Ignore),
            new FileProperty("Size Width ( in Feet)", ScanField.Ignore),
            new FileProperty("Size Length ( in Feet)", ScanField.Ignore),
            new FileProperty("Size Height/Product thickness", ScanField.Ignore),
            new FileProperty("Shape", ScanField.Ignore),
            new FileProperty("Sq.ft", ScanField.Ignore),
            new FileProperty("Design Description", ScanField.Ignore),
            new FileProperty("Option Description", ScanField.Ignore),
            new FileProperty("SKU Description", ScanField.Ignore),
            new FileProperty("Cost", ScanField.Ignore),
            new FileProperty("MSRP", ScanField.Ignore),
            new FileProperty("MAP", ScanField.MAP),
            new FileProperty("Weight(Lbs)", ScanField.Ignore),
            new FileProperty("Length (FT)", ScanField.Ignore),
            new FileProperty("Width (FT)", ScanField.Ignore),
            new FileProperty("Height (FT)", ScanField.Ignore),
            new FileProperty("Length (Inch)", ScanField.Ignore),
            new FileProperty("Width (Inch)", ScanField.Ignore),
            new FileProperty("Height (Inch)", ScanField.Ignore),
            new FileProperty("Construction", ScanField.Ignore),
            new FileProperty("Origin", ScanField.Ignore),
            new FileProperty("Material-1", ScanField.Ignore),
            new FileProperty("Material-2", ScanField.Ignore),
            new FileProperty("PILE HEIGHT IN INCHES", ScanField.Ignore),
            new FileProperty("INTRODUCTION COPY", ScanField.Ignore),
            new FileProperty("Key Feature 1", ScanField.Ignore),
            new FileProperty("Key Feature 2", ScanField.Ignore),
            new FileProperty("Key Feature 3", ScanField.Ignore),
            new FileProperty("Primary Style", ScanField.Ignore),
            new FileProperty("Secondary Style", ScanField.Ignore),
            new FileProperty("Primary Pattern", ScanField.Ignore),
            new FileProperty("Secondary Pattern/Trend", ScanField.Ignore),
            new FileProperty("Care Instructions", ScanField.Ignore),
            new FileProperty("Backing", ScanField.Ignore),
            new FileProperty("Pillow Closer", ScanField.Ignore),
            new FileProperty("Is the Back Same as Front?", ScanField.Ignore),
            new FileProperty("Color of Back- Pantone Number", ScanField.Ignore),
            new FileProperty("Color of Back- Pantone Name", ScanField.Ignore),
            new FileProperty("Material of Back", ScanField.Ignore),
            new FileProperty("Custom(Yes/No)", ScanField.Ignore),
            new FileProperty("Max length (Feet)", ScanField.Ignore),
            new FileProperty("Max width (Feet)", ScanField.Ignore),
            new FileProperty("Quick Ship", ScanField.Ignore),
            new FileProperty("Rug Pad", ScanField.Ignore),
            new FileProperty("Pantone", ScanField.Ignore),
            new FileProperty("Color (Pantone TPX)", ScanField.Ignore),
            new FileProperty("Headshot Image", ScanField.Ignore),
            new FileProperty("Floor Shot Images", ScanField.Ignore),
            new FileProperty("Room Shot Images", ScanField.Ignore),
            new FileProperty("Corner Shot Images", ScanField.Ignore),
            new FileProperty("Close Up Images", ScanField.Ignore),
        };

        public JaipurProductFileLoader(IStorageProvider<JaipurVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xlsx, 6, 7) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x => x[ScanField.ItemNumber] = GetItemNumber(x));
            return products.Where(x => x[ScanField.ProductType] == "Rug").ToList();
        }

        private string GetItemNumber(ScanData data)
        {
            var parsed = RugParser.ParseDimensions(data[ScanField.Size]);
            if (parsed == null) return string.Empty;
            return data[ScanField.Code] + parsed.GetSkuSuffix();
        }
    }
}