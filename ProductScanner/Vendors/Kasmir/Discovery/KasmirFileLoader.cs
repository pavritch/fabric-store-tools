using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;

namespace Kasmir.Discovery
{
    public class KasmirFileLoader : ProductFileLoader<KasmirVendor>
    {
        private static readonly List<FileProperty> Properties = new List<FileProperty>
        {
            new FileProperty("BookPattern", ScanField.PatternName),
            new FileProperty("BookColor", ScanField.ColorName),
            new FileProperty("Retail price", ScanField.RetailPrice),
            new FileProperty("Width", ScanField.Width),
            new FileProperty("ScheduleNumber", ScanField.Ignore),
            new FileProperty("Verticle Repeat", ScanField.VerticalRepeat),
            new FileProperty("Horizontal Repeat", ScanField.HorizontalRepeat),
            new FileProperty("Cleaning Code", ScanField.Cleaning),
            new FileProperty("CountryOfOrigin", ScanField.Country),
            new FileProperty("Flammability", ScanField.Flammability),
            new FileProperty("Construction", ScanField.Construction),
            new FileProperty("Abrasion", ScanField.Durability),
            new FileProperty("Finish", ScanField.Finish),
            new FileProperty("KasmirExclusive", ScanField.Ignore),
            new FileProperty("Patternmatch", ScanField.Ignore),
            new FileProperty("Usable Width", ScanField.Ignore),
            new FileProperty("Reversible", ScanField.Ignore),
            // completely blank
            new FileProperty("Railroaded", ScanField.Railroaded),
            new FileProperty("Green Certification", ScanField.Ignore),
            new FileProperty("HalfDrop", ScanField.HorizontalHalfDrop),
            new FileProperty("Handcrafted", ScanField.Ignore),
            // a few properties here
            new FileProperty("Lightfastness", ScanField.Ignore),
            new FileProperty("Mildew Resistant", ScanField.Ignore),
            new FileProperty("Black Background", ScanField.Ignore),
            new FileProperty("Weighted Hem", ScanField.Ignore),
            new FileProperty("BookNumber", ScanField.BookNumber),
            new FileProperty("Contents", ScanField.Content),
            new FileProperty("inventory", ScanField.StockCount),
            new FileProperty("NLAFlag", ScanField.Ignore),
            new FileProperty("Color Aqua", ScanField.Ignore),
            new FileProperty("Color Baby Blue", ScanField.Ignore),
            new FileProperty("Color Beige", ScanField.Ignore),
            new FileProperty("Color Black", ScanField.Ignore),
            new FileProperty("Color Blue", ScanField.Ignore),
            new FileProperty("Color Brown", ScanField.Ignore),
            new FileProperty("Color Burgendy", ScanField.Ignore),
            new FileProperty("Color Charcoal", ScanField.Ignore),
            new FileProperty("Color Cream", ScanField.Ignore),
            new FileProperty("Color Dark Green", ScanField.Ignore),
            new FileProperty("Color Gold", ScanField.Ignore),
            new FileProperty("Color Grey", ScanField.Ignore),
            new FileProperty("Color Light Green", ScanField.Ignore),
            new FileProperty("Color Metalic Gold", ScanField.Ignore),
            new FileProperty("Color Metalic Silver", ScanField.Ignore),
            new FileProperty("Color navy", ScanField.Ignore),
            new FileProperty("Color Olive", ScanField.Ignore),
            new FileProperty("Color Orange", ScanField.Ignore),
            new FileProperty("Color Pink", ScanField.Ignore),
            new FileProperty("Color Purple", ScanField.Ignore),
            new FileProperty("Color Red", ScanField.Ignore),
            new FileProperty("Color White", ScanField.Ignore),
            new FileProperty("Color Yellow", ScanField.Ignore),
            new FileProperty("Color Black", ScanField.Ignore),
            new FileProperty("Color Teal", ScanField.Ignore),
            new FileProperty("Design Motif", ScanField.Ignore),
            new FileProperty("Construction", ScanField.Ignore),
            new FileProperty("PatternUsage", ScanField.Use),
            new FileProperty("ImageName", ScanField.ImageUrl),
        };

        public KasmirFileLoader(IStorageProvider<KasmirVendor> storageProvider)
            : base(storageProvider, ScanField.ImageUrl, Properties) { }

        public override async Task<List<ScanData>> LoadProductsAsync()
        {
            var products = await base.LoadProductsAsync();
            products.ForEach(x => x[ScanField.ManufacturerPartNumber] = MakeMPN(x[ScanField.PatternName], x[ScanField.ColorName]));
            products.ForEach(x => x.Cost = Math.Round(x[ScanField.RetailPrice].ToDecimalSafe()/2.0m, 2));
            return products;
        }

        private string MakeMPN(string patternName, string colorName)
        {
            return string.Format("{0}-{1}", patternName.Replace(" ", "-"),
                colorName.Replace(" ", "-"));
        }
    }
}