using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace LegendOfAsia
{
    public class LegendOfAsiaProductBuilder : ProductBuilder<LegendOfAsiaVendor>
    {
        public LegendOfAsiaProductBuilder(IPriceCalculator<LegendOfAsiaVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new LegendOfAsiaVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount].ToDoubleSafe()), 
                data[ScanField.ManufacturerPartNumber], BuildFeatures(data));
            //var categories = data[ScanField.Category].Split(',').Distinct();
            var name = data[ScanField.ProductName].Replace("&amp;", "&").Replace("??", "").TitleCase();

            homewareProduct.HomewareCategory = AssignCategory(name);
            homewareProduct.ManufacturerDescription = name;
            homewareProduct.Name = name;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            var match = Regex.Match(data[ScanField.Description], @"\D*(?<width>\d+\.?\d*) x (?<length>\d+\.?\d*) x (?<height>\d+\.?\d*).*");
            if (match.Success)
            {
                features.Width = match.Groups["width"].Value.Trim().ToDoubleSafe();
                features.Depth = match.Groups["length"].Value.Trim().ToDoubleSafe();
                features.Height = match.Groups["height"].Value.Trim().ToDoubleSafe();
                return features;
            }

            match = Regex.Match(data[ScanField.Description], @"\D*(?<width>\d+\.?\d*)W x (?<length>\d+\.?\d*)D x (?<height>\d+\.?\d*)H.*");
            if (match.Success)
            {
                features.Width = match.Groups["width"].Value.Trim().ToDoubleSafe();
                features.Depth = match.Groups["length"].Value.Trim().ToDoubleSafe();
                features.Height = match.Groups["height"].Value.Trim().ToDoubleSafe();
                return features;
            }

            match = Regex.Match(data[ScanField.Description], @"\D*(?<width>\d+\.?\d*)W x (?<length>\d+\.?\d*)H x (?<height>\d+\.?\d*)D.*");
            if (match.Success)
            {
                features.Width = match.Groups["width"].Value.Trim().ToDoubleSafe();
                features.Depth = match.Groups["length"].Value.Trim().ToDoubleSafe();
                features.Height = match.Groups["height"].Value.Trim().ToDoubleSafe();
                return features;
            }
            return features;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Tables, name => name.Contains("Tray Table")},
            {HomewareCategory.Accent_Stools, name => name.Contains("Stool") && !name.Contains("Bar")},
            {HomewareCategory.Accessories, name => name.ContainsAnyOf("Brush", "Wine Rack", "Chess", "Umbrella", 
                "Horse", "Elephant", "Hippo", "Rhino", "Dragon", "Lion", "Fish", "Dog", "Stallion", "Parrot") || 
                (name.Contains("Coral") && !name.Contains("Vase"))},
            {HomewareCategory.Bar_Stools, name => name.Contains("Bar Stool")},
            {HomewareCategory.Benches, name => name.Contains("Bench")},
            {HomewareCategory.Bookcases, name => name.Contains("Bookcase")},
            {HomewareCategory.Bowls, name => name.Contains("Bowl")},
            {HomewareCategory.Cabinets, name => name.ContainsAnyOf("Cabinet", "Display Case", "Buffet")},
            {HomewareCategory.Candleholders, name => name.ContainsAnyOf("Candleholder", "Candle Holder")},
            {HomewareCategory.Coffee_Tables, name => name.Contains("Coffee")},
            {HomewareCategory.Console_Tables, name => name.Contains("Console")},
            {HomewareCategory.Decorative_Boxes, name => name.Contains("Decorative Box")},
            {HomewareCategory.Decorative_Pillows, name => name.Contains("Pillow")},
            {HomewareCategory.Decorative_Trays, name => name.Contains("Tray")},
            {HomewareCategory.Dining_Chairs, name => name.Contains("Dining") && name.Contains("Chair")},
            {HomewareCategory.Dining_Tables, name => name.Contains("Dining") && name.Contains("Table")},
            {HomewareCategory.Dressers, name => name.ContainsAnyOf("Chest", "Armoire", "Dresser", "Trunk")},
            {HomewareCategory.Faux_Flowers, name => name.Contains("Flower Arrangement")},
            {HomewareCategory.Garden_Decorations, name => name.ContainsAnyOf("Buddha", "Pot")},
            {HomewareCategory.Garden_Planters, name => name.ContainsAnyOf("Plant Stand", "Planter")},
            {HomewareCategory.Jars, name => name.Contains("Jar")},
            {HomewareCategory.Mirrors, name => name.Contains("Mirror")},
            {HomewareCategory.Plates, name => name.Contains("Plate")},
            {HomewareCategory.Shelves, name => name.Contains("Shelf")},
            {HomewareCategory.Sideboards, name => name.ContainsAnyOf("Sideboard", "Buffet")},
            {HomewareCategory.Side_Chairs, name => name.Contains("Arm Chair")},
            {HomewareCategory.Side_Tables, name => name.Contains("Side Table")},
            {HomewareCategory.Statues_and_Sculptures, name => name.Contains("Statue")},
            {HomewareCategory.Sofas, name => name.Contains("Sofa")},
            {HomewareCategory.Wall_Lamps, name => name.Contains("Lamp")},
            {HomewareCategory.Vases, name => name.Contains("Vase")},
            {HomewareCategory.Wall_Art, name => name.ContainsAnyOf("Wall Panel", "Wall Art")},
        };

        private HomewareCategory AssignCategory(string productName)
        {
            foreach (var category in _categories)
                if (category.Value(productName))
                    return category.Key;
            return HomewareCategory.Unknown;
        }
    }
}