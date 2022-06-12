using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace WorldsAway
{
    public class WorldsAwayProductBuilder : ProductBuilder<WorldsAwayVendor>
    {
        public WorldsAwayProductBuilder(IPriceCalculator<WorldsAwayVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var stock = data[ScanField.StockCount].ToIntegerSafe();
            var vendor = new WorldsAwayVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(stock), mpn, BuildFeatures(data));

            // the last one is the SKU
            var breadCrumbs = data[ScanField.Breadcrumbs].Split(new[] {','}).TakeAllButLast();
            breadCrumbs = breadCrumbs.Where(x => x != "Home" && x != "Collection").Select(x => x.Trim()).ToList();

            var description = data[ScanField.Description].Replace("Décor", "Decor");

            homewareProduct.HomewareCategory = AssignCategory(string.Join(" ", breadCrumbs), description.TitleCase());
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = data[ScanField.ProductName].TitleCase();
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private HomewareCategory AssignCategory(string categoryName, string name)
        {
            foreach (var category in _firstPassCategories)
                if (category.Value(categoryName, name))
                    return category.Key;

            foreach (var category in _secondPassCategories)
                if (category.Value(categoryName))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _firstPassCategories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            {HomewareCategory.Accent_Stools, (cat, name) => name.ContainsAnyOf("Side Stool", "Stool")},
            {HomewareCategory.Accent_Tables, (cat, name) => name.Contains("Cigar Table")},
            {HomewareCategory.Accessories, (cat, name) => name.Contains("Kleenex Box")},
            {HomewareCategory.Bar_Carts, (cat, name) => name.Contains("Bar Cart")},
            {HomewareCategory.Bar_Stools, (cat, name) => name.ContainsAnyOf("Round Stool", "Counterstool", "Bar Stool", "Counter Stool")},
            {HomewareCategory.Benches, (cat, name) => name.Contains("Bench")},
            {HomewareCategory.Bookends, (cat, name) => name.ContainsAnyOf("Bookends", "Book Ends")},
            {HomewareCategory.Cabinets, (cat, name) => name.Contains("Cabinet")},
            {HomewareCategory.Chandeliers, (cat, name) => name.Contains("Chandelier")},
            {HomewareCategory.Coffee_Tables, (cat, name) => name.Contains("Coffee Table")},
            {HomewareCategory.Console_Tables, (cat, name) => name.Contains("Console") && !name.Contains("Media")},
            {HomewareCategory.Desks, (cat, name) => name.Contains("Desk")},
            {HomewareCategory.Decorative_Bowls, (cat, name) => name.Contains("Bowl")},
            {HomewareCategory.Decorative_Boxes, (cat, name) => name.Contains("Box")},
            {HomewareCategory.Decorative_Containers, (cat, name) => name.Contains("Container")},
            {HomewareCategory.Decorative_Trays, (cat, name) => name.Contains("Tray")},
            {HomewareCategory.Dining_Chairs, (cat, name) => name.Contains("Dining Chair")},
            {HomewareCategory.Dining_Tables, (cat, name) => name.Contains("Dining Table")},
            {HomewareCategory.Dressers, (cat, name) => name.ContainsAnyOf("Etagere", "Dresser")},
            {HomewareCategory.Garden_Decorations, (cat, name) => name.Contains("Obelisk")},
            {HomewareCategory.Garden_Planters, (cat, name) => name.Contains("Planter")},
            {HomewareCategory.Mirrors, (cat, name) => name.ContainsAnyOf("Mirror", "Miror")},
            {HomewareCategory.Pendants, (cat, name) => name.Contains("Pendant")},
            {HomewareCategory.Side_Chairs, (cat, name) => name.Contains("Arm Chair")},
            {HomewareCategory.Side_Tables, (cat, name) => cat.Contains("Side Table") || name.Contains("Side Table")},
            {HomewareCategory.Statues_and_Sculptures, (cat, name) => name.ContainsAnyOf("Sculpture", "Sphere", "Figure", "Style Star")},
            {HomewareCategory.Table_Lamps, (cat, name) => name.Contains("Table Lamp")},
            {HomewareCategory.TV_Stands, (cat, name) => name.Contains("Media Console")},
            {HomewareCategory.Wall_Art, (cat, name) => name.ContainsAnyOf("Wall Decor", "Wall Décor", "Iron Asterisk", "Tortoise Shell")},
            {HomewareCategory.Wastebaskets, (cat, name) => name.ContainsAnyOf("Wastebasket", "Wastbasket")},
        };

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _secondPassCategories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accessories, cat => cat.Contains("Decorative Objects")},
            {HomewareCategory.Accent_Chairs, cat => cat.Contains("Chairs")},
            {HomewareCategory.Benches, cat => cat.Contains("Benches")},
            {HomewareCategory.Cabinets, cat => cat.Contains("Cabinets")},
            {HomewareCategory.Chandeliers, cat => cat.Contains("Chandeliers")},
            {HomewareCategory.Coffee_Tables, cat => cat.Contains("Coffee Tables")},
            {HomewareCategory.Console_Tables, cat => cat.Contains("Console Tables")},
            {HomewareCategory.Decorative_Containers, cat => cat.Contains("Containers & Accessories")},
            {HomewareCategory.Decorative_Trays, cat => cat.Contains("Trays")},
            {HomewareCategory.Desks, cat => cat.Contains("Desks")},
            {HomewareCategory.Floor_Lamps, cat => cat.Contains("Floor Lamps")},
            {HomewareCategory.Sconces, cat => cat.Contains("Sconces")},
            {HomewareCategory.Shades, cat => cat.Contains("lamp shades")},
            {HomewareCategory.Side_Tables, cat => cat.ContainsAnyOf("Side Tables", "Occasional Tables")},
            {HomewareCategory.Table_Lamps, cat => cat.Contains("Table Lamps")},
            {HomewareCategory.TV_Stands, cat => cat.Contains("Media Consoles")},
            {HomewareCategory.Vanities, cat => cat.Contains("Vanities")},
            {HomewareCategory.Wastebaskets, cat => cat.Contains("Wastebaskets")},
        }; 

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Color = FormatColor(data[ScanField.Color]);
            features.Depth = GetDimension(data[ScanField.Dimensions], "D").ToDoubleSafe();
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            features.Height = GetDimension(data[ScanField.Dimensions], "H").ToDoubleSafe();
            features.Width = GetDimension(data[ScanField.Dimensions], "W").ToDoubleSafe();
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specDict = new Dictionary<ScanField, string>
            {
                { ScanField.Diameter, GetDimension(data[ScanField.Dimensions], "DIA").ToInchesMeasurement() }
            };
            var specs = data[ScanField.Description].Split(new[] {"::"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var spec in specs)
            {
                // TODO: Not sure how to handle this
            }
            return ToSpecs(specDict);
        }


        private string FormatColor(string color)
        {
            return string.Join(", ", color.Split(new[] {','}).Select(x => new ItemColor(x).GetFormattedColor()));
        }

        private string GetDimension(string dimensions, string field)
        {
            var dims = dimensions.Split(new[] {'X'}).ToList();
            var match = dims.SingleOrDefault(x => x.Trim().EndsWith(field));
            if (match != null) return match.Replace("\"", "").Replace(field, "");
            return string.Empty;
        }
    }
}