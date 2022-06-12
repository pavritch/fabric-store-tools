using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ElkGroup
{
    public class ElkGroupProductBuilder : ProductBuilder<ElkGroupVendor>
    {
        public ElkGroupProductBuilder(IPriceCalculator<ElkGroupVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            data.Cost = data[ScanField.Cost].ToDecimalSafe();

            var vendor = new ElkGroupVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount].ToDoubleSafe()), mpn, BuildFeatures(data));
            var patternName = data[ScanField.Description].Replace("Chandeier", "Chandelier").Trim('\"').TitleCase().MoveSetInfo();

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Collection], patternName);
            homewareProduct.ManufacturerDescription = data[ScanField.LongDescription];
            homewareProduct.Name = patternName;
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = new List<ScannedImage> {new ScannedImage(ImageVariantType.Primary, data[ScanField.ImageUrl])};
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accessories, name => name.Contains("Medallion")},
            {HomewareCategory.Accent_Stools, name => name.Contains("Stool")},
            {HomewareCategory.Accent_Tables, name => name.Contains("Accent Table")},
            {HomewareCategory.Bar_Carts, name => name.Contains("Bar Cart")},
            {HomewareCategory.Bar_Tables, name => name.Contains("Bar Table")},
            {HomewareCategory.Baskets, name => name.Contains("Basket")},
            {HomewareCategory.Bookcases, name => name.ContainsAnyOf("Bookcase", "Bookshelf")},
            {HomewareCategory.Bowls, name => name.ContainsAnyOf("Bowl", "Dish")},
            {HomewareCategory.Cabinets, name => name.ContainsAnyOf("Cabinet", "Armoire", "Chest")},
            {HomewareCategory.Candleholders, name => name.ContainsAnyOf("Hurricane", "Candleholder", "Candlestick", "Candle Holder", 
                "Pillar Holder", "Candle Stick")},
            {HomewareCategory.Centerpieces, name => name.ContainsAnyOf("Centerpiece", "Centrepiece")},
            {HomewareCategory.Chandeliers, name => name.Contains("Chandelier")},
            {HomewareCategory.Coffee_Tables, name => name.Contains("Coffee Table")},
            {HomewareCategory.Console_Tables, name => name.Contains("Console Table")},
            {HomewareCategory.Decorative_Trays, name => name.Contains("Tray")},
            {HomewareCategory.Dining_Chairs, name => name.Contains("Side Chair")},
            {HomewareCategory.Dining_Tables, name => name.Contains("Dining Table")},
            {HomewareCategory.End_Tables, name => name.Contains("End Table")},
            {HomewareCategory.Floor_Lamps, name => name.Contains("Floor Lamp")},
            {HomewareCategory.Garden_Decorations, name => name.ContainsAnyOf("Post Lantern", "Post Lamp", "Post Light")},
            {HomewareCategory.Garden_Planters, name => name.Contains("Planter")},
            {HomewareCategory.Jars, name => name.ContainsAnyOf("Jar", "Urn")},
            {HomewareCategory.Lighting, name => name.ContainsAnyOf("1 Light", "Led Light")},
            {HomewareCategory.Mirrors, name => name.EndsWith("Mirror") || name.Contains("Wall Mirror")},
            {HomewareCategory.Ottomans, name => name.Contains("Ottoman")},
            {HomewareCategory.Patio_Furniture, name => name.Contains("Bench")},
            {HomewareCategory.Pendants, name => name.ContainsAnyOf("Pendant", "Convertible", "Hanging Lamp", "Lantern", "Island", "Billiard")},
            {HomewareCategory.Sconces, name => name.ContainsAnyOf("Wall Sconce", "Candle Lamp", "Light Sconce", "Wall Mount", "Candle Sconce", 
                "Vanity", "Light Bath Bar", "Light Bathbar")},
            {HomewareCategory.Shelves, name => name.Contains("Shelving Unit")},
            {HomewareCategory.Statues_and_Sculptures, name => name.ContainsAnyOf("Sculpture", "Statue")},
            {HomewareCategory.Sideboards, name => name.ContainsAnyOf("Credenza", "Sideboard")},
            {HomewareCategory.Side_Tables, name => name.Contains("Side Table")},
            {HomewareCategory.Table_Lamps, name => name.ContainsAnyOf("Table Lamp", "Buffet Lamp", "Portable Lamp", 
                "Portable Led Lamp", "Jug Lamp", "Candlestick Lamp")},
            {HomewareCategory.Vanity_Lights, name => name.ContainsAnyOf("Vanity", "Light Bath Bar", "Light Bathbar")},
            {HomewareCategory.Vases, name => name.Contains("Vase")},
            {HomewareCategory.Wall_Art, name => name.ContainsAnyOf("Shadow Box", "Frame", "Clock", "Wall Decor", "Original Art", "Wall Art", "Print")},
            {HomewareCategory.Wall_Lamps, name => name.ContainsAnyOf("Swingarm", "Cabinet Light", "Light Kit", "Flush", 
                "Zeeled", "Flushmount", "Recessed", "Wall Light", "Fixture")},

            {HomewareCategory.Ignored, name => name.ContainsAnyOf("Rug", "Transformer")},
        };

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _secondPass = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Chairs, name => name.Contains("Chair")},
            {HomewareCategory.Accent_Tables, name => name.Contains("Table")},
            {HomewareCategory.Decorative_Boxes, name => name.Contains("Box")},
            {HomewareCategory.Sconces, name => name.Contains("Sconce")},
            {HomewareCategory.Table_Lamps, name => name.Contains("Lamp")},
        };

        private HomewareCategory AssignCategory(string collection, string name)
        {
            foreach (var category in _categories)
                if (category.Value(collection + " " + name))
                    return category.Key;

            foreach (var category in _secondPass)
                if (category.Value(name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Bullets = new List<string>
            {
                data[ScanField.Bullet1], data[ScanField.Bullet2],
                data[ScanField.Bullet3], data[ScanField.Bullet4]
            }.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            features.Depth = data[ScanField.Length].ToDoubleSafe();
            features.Height = data[ScanField.Height].ToDoubleSafe();
            features.ShippingWeight = data[ScanField.Weight].ToDoubleSafe();
            features.UPC = data[ScanField.UPC];
            features.Width = data[ScanField.Width].ToDoubleSafe();

            features.Features = BuildSpecs(data);
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.CordLength, data[ScanField.CordLength]},
                {ScanField.Country, data[ScanField.Country]},
                {ScanField.Finish, data[ScanField.Finish].TitleCase()},
                {ScanField.Lumens, data[ScanField.Lumens]},
                {ScanField.Material, string.Join(", ", data[ScanField.Material].Split(new []{','}).Select(x => x.Trim().TitleCase()))},
                {ScanField.Shade, data[ScanField.Shade]},
                {ScanField.SwitchType, data[ScanField.SwitchType]},
            };

            if (data[ScanField.Bulbs] != "0") specs.Add(ScanField.Bulbs, data[ScanField.Bulbs]);
            if (data[ScanField.ChainLength] != "0") specs.Add(ScanField.ChainLength, data[ScanField.ChainLength].ToInchesMeasurement());
            if (data[ScanField.Collection] != "No Collection") specs.Add(ScanField.Collection, data[ScanField.Collection]);
            if (data[ScanField.Wattage] != string.Empty) specs.Add(ScanField.Wattage, data[ScanField.Wattage] + "W");
            return ToSpecs(specs);
        }
    }
}