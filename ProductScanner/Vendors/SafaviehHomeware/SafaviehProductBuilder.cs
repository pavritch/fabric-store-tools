using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace SafaviehHomeware
{
    public class SafaviehProductBuilder : ProductBuilder<SafaviehHomewareVendor>
    {
        public SafaviehProductBuilder(IPriceCalculator<SafaviehHomewareVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            data.Cost = data[ScanField.Cost].ToDecimalSafe();

            var vendor = new SafaviehHomewareVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true), mpn, BuildFeatures(data));

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Category].Replace("DÃ©cor", "Decor"), data[ScanField.ProductName]);
            homewareProduct.Name = data[ScanField.ProductName].TitleCase();
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();

            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            return homewareProduct;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Color = data[ScanField.Color].Replace("Color: ", "");
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            features.ShippingWeight = data[ScanField.ShippingWeight].ToDoubleSafe();
            features.UPC = data[ScanField.UPC];

            var dims = data[ScanField.Dimensions].Split(new[] {" x ", " X "}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().Replace("\"", "")).ToList();
            if (dims.Count == 3)
            {
                features.Depth = dims[0].ToDoubleSafe();
                features.Width = dims[1].ToDoubleSafe();
                features.Height = dims[2].ToDoubleSafe();
            }
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.Content, data[ScanField.Content]},
                {ScanField.Country, data[ScanField.Country].TitleCase()},
                {ScanField.Material, data[ScanField.Material].TitleCase()},
            };
            return ToSpecs(specs);
        }

        // check by name for first pass
        private readonly Dictionary<HomewareCategory, Func<string, bool>> _firstPass = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Stools, name => name.Contains("Stool")},
            {HomewareCategory.Benches, name => name.Contains("Bench")},
            {HomewareCategory.Cabinets, name => name.ContainsAnyOf("Cabinet", "Storage Unit")},
            {HomewareCategory.Coffee_Tables, name => name.Contains("Cocktail Table")},
            {HomewareCategory.Decorative_Pillows, name => name.Contains("Decorative Pillow")},
            {HomewareCategory.Decorative_Trunks, name => name.Contains("Rolling Chest")},
            {HomewareCategory.Dining_Chairs, name => name.Contains("Dining Chair")},
            {HomewareCategory.End_Tables, name => name.Contains("End Table")},
            {HomewareCategory.Garden_Planters, name => name.Contains("Planter")},
            {HomewareCategory.Nightstands, name => name.ContainsAnyOf("Night Table", "Night Stand")},
            {HomewareCategory.Jars, name => name.Contains("Jar")},
            {HomewareCategory.Ottomans, name => name.Contains("Ottoman")},
            {HomewareCategory.Shelves, name => name.ContainsAnyOf("Etegere", "Etagere")},
            {HomewareCategory.Sideboards, name => name.Contains("Sideboard")},
            {HomewareCategory.Side_Chairs, name => name.Contains("Side Chair")},
            {HomewareCategory.Side_Tables, name => name.Contains("Side Table")},
            {HomewareCategory.Vases, name => name.Contains("Vase")},
        };

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            {HomewareCategory.Accent_Cabinets, (cat, name) => cat.Contains("cabinet")},
            {HomewareCategory.Accent_Chairs, (cat, name) => cat.Contains("Accent Chairs")},
            {HomewareCategory.Accent_Tables, (cat, name) => cat.Contains("Accent Tables")},
            {HomewareCategory.Bar_Stools, (cat, name) => cat.Contains("Barstools") || cat == "Stools" || cat.Contains("Counter Stool")},
            {HomewareCategory.Beds, (cat, name) => name.Contains("Headboard")},
            {HomewareCategory.Benches, (cat, name) => cat.Contains("Benches")},
            {HomewareCategory.Bookcases, (cat, name) => cat.Contains("Bookcases")},
            {HomewareCategory.Coffee_Tables, (cat, name) => cat.Contains("Coffee Tables")},
            {HomewareCategory.Console_Tables, (cat, name) => cat.Contains("Consoles")},
            {HomewareCategory.Desk_Chairs, (cat, name) => cat.Contains("Desk Chairs")},
            {HomewareCategory.Desks, (cat, name) => cat.Contains("Desks")},
            {HomewareCategory.Dining_Chairs, (cat, name) => cat.Contains("Dining Chairs")},
            {HomewareCategory.Dining_Tables, (cat, name) => cat.Contains("Dining Tables")},
            {HomewareCategory.Floor_Lamps, (cat, name) => cat.Contains("Floor Lamp")},
            {HomewareCategory.Garden_Stools, (cat, name) => cat.Contains("Garden Stools")},
            {HomewareCategory.Mirrors, (cat, name) => cat.Contains("Mirrors")},
            {HomewareCategory.Ottomans, (cat, name) => cat.Contains("Ottomans")},
            {HomewareCategory.Patio_Furniture, (cat, name) => cat.Contains("Outdoor")},
            {HomewareCategory.Poufs, (cat, name) => cat.Contains("Pouf")},
            {HomewareCategory.Room_Screens, (cat, name) => cat.Contains("Screen")},
            {HomewareCategory.Side_Tables, (cat, name) => cat.Contains("Happimess")},
            {HomewareCategory.Table_Lamps, (cat, name) => cat.Contains("Table Lamp")},
            {HomewareCategory.TV_Stands, (cat, name) => cat.Contains("TV Cabinet")},
            {HomewareCategory.Vanities, (cat, name) => cat.Contains("Vanity")},
            {HomewareCategory.Wall_Art, (cat, name) => cat.ContainsAnyOf("Art", "Decor")},
        };

        private HomewareCategory AssignCategory(string categoryName, string name)
        {
            foreach (var category in _firstPass)
                if (category.Value(name))
                    return category.Key;

            foreach (var category in _categories)
                if (category.Value(categoryName, name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }
    }
}