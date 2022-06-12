using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Pindler.Discovery
{
    public class PindlerFileLoader : ProductFileLoader<PindlerVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("Inventory Number", ScanField.ManufacturerPartNumber),
            new FileProperty("Book Name", ScanField.Book),
            new FileProperty("Cleaning Code", ScanField.Cleaning),
            new FileProperty("Collection Name", ScanField.Collection),
            new FileProperty("Content", ScanField.Content),
            new FileProperty("Country of Origin", ScanField.Country),
            new FileProperty("Durability", ScanField.Durability),
            new FileProperty("Finish Description", ScanField.Finish),
            new FileProperty("Flammability", ScanField.Flammability),
            new FileProperty("Horizontal Repeat", ScanField.HorizontalRepeat),
            new FileProperty("Image URL", ScanField.ImageUrl),
            new FileProperty("In Stock", ScanField.StockCount),
            new FileProperty("Keyword 1", ScanField.Bullet1),
            new FileProperty("Keyword 2", ScanField.Bullet2),
            new FileProperty("Keyword 3", ScanField.Bullet3),
            new FileProperty("Keyword 4", ScanField.Bullet4),
            new FileProperty("Keyword 5", ScanField.Bullet5),
            new FileProperty("Keyword 6", ScanField.Bullet6),
            new FileProperty("Pattern Color", ScanField.ColorName),
            new FileProperty("Pattern Name", ScanField.PatternName),
            new FileProperty("Pattern Number", ScanField.PatternNumber),
            new FileProperty("Red Label Header", ScanField.Ignore),
            new FileProperty("Status", ScanField.Status),
            new FileProperty("Unit of Measure", ScanField.UnitOfMeasure),
            new FileProperty("Vertical Repeat", ScanField.VerticalRepeat),
            new FileProperty("Wholesale Price", ScanField.Cost),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("Prop65", ScanField.Ignore),
            new FileProperty("9X9", ScanField.HasSwatch),
        };

        public PindlerFileLoader(IStorageProvider<PindlerVendor> storageProvider)
            : base(storageProvider, ScanField.ManufacturerPartNumber, Properties, ProductFileType.Xls, 1, 3) { }

        public async override Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x => x[ScanField.ImageUrl] = x[ScanField.ImageUrl].Replace(" ", "%20"));
            return products.Where(x => string.Equals(x[ScanField.Status], "CURRENT", StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}