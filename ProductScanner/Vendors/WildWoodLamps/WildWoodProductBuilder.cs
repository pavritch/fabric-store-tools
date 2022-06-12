using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace WildWoodLamps
{
    public class WildWoodProductBuilder : ProductBuilder<WildWoodLampsVendor>
    {
        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Tables, cat => cat.Contains("Tables")},
            {HomewareCategory.Bar_Tables, cat => cat.Contains("Bars")},
            {HomewareCategory.Benches, cat => cat.Contains("Bench")},
            {HomewareCategory.Bookends, cat => cat.Contains("Bookends")},
            {HomewareCategory.Bookcases, cat => cat.Contains("Bookcases")},
            {HomewareCategory.Bowls, cat => cat.Contains("Bowls")},
            {HomewareCategory.Candleholders, cat => cat.Contains("Candleholders")},
            {HomewareCategory.Candlesticks, cat => cat.Contains("Candlesticks")},
            {HomewareCategory.Cabinets, cat => cat.Contains("Cabinets")},
            {HomewareCategory.Chandeliers, cat => cat.Contains("Chandeliers")},
            {HomewareCategory.Console_Tables, cat => cat.Contains("Console")},
            {HomewareCategory.Coffee_Tables, cat => cat.Contains("Coffee")},
            {HomewareCategory.Dressers, cat => cat.Contains("Chest") || cat.Contains("Etagere")},
            {HomewareCategory.Decor, cat => cat.Contains("Decorative Objects") || cat.Contains("Figurines") || cat.Contains("Decoration")},
            {HomewareCategory.Decorative_Boxes, cat => cat.Contains("Boxes")},
            {HomewareCategory.Decorative_Trays, cat => cat.Contains("Tray")},
            {HomewareCategory.Desks, cat => cat.Contains("Desks")},
            {HomewareCategory.Dining_Tables, cat => cat.Contains("Dining")},
            {HomewareCategory.End_Tables, cat => cat.Contains("End/Side")},
            {HomewareCategory.Floor_Lamps, cat => cat.Contains("Floor Lamp")},
            {HomewareCategory.Garden_Planters, cat => cat.Contains("Planters")},
            {HomewareCategory.Pillows_Throws_and_Poufs, cat => cat.Contains("Pillows")},
            {HomewareCategory.Sconces, cat => cat.Contains("Sconces")},
            {HomewareCategory.Shades, cat => cat.Contains("Shades")},
            {HomewareCategory.Sideboards, cat => cat.Contains("Sideboard")},
            {HomewareCategory.Sofas, cat => cat.Contains("Sofa")},
            {HomewareCategory.Table_Lamps, cat => cat.Contains("Table Lamp")},
            {HomewareCategory.Vases, cat => cat.Contains("Vases")},
            {HomewareCategory.Wall_Art, cat => cat.ContainsAnyOf("Wall", "Oil Paintings", "Photo Frames", "Prints", "Engravings", "Watercolors", "Hanging Wall")},
            {HomewareCategory.Wastebaskets, cat => cat.Contains("Wastepaper Baskets")},
        };

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _secondPass = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Chairs, name => name.ContainsIgnoreCase("Armchair") || name.ContainsIgnoreCase("Sidechair") || name.ContainsIgnoreCase("Side Chair")},
            {HomewareCategory.Beds, name => name.ContainsIgnoreCase("Bed") || name.Contains("Headboard")},
            {HomewareCategory.Benches, name => name.ContainsIgnoreCase("Bench")},
            {HomewareCategory.Candleholders, name => name.ContainsIgnoreCase("Lantern") || name.ContainsIgnoreCase("Candlestick Lamp") || name.ContainsIgnoreCase("Hurricane")},
            {HomewareCategory.Chandeliers, name => name.ContainsIgnoreCase("Chandelier")},
            {HomewareCategory.Coffee_Tables, name => name.ContainsIgnoreCase("Cocktail Table") || name.ContainsIgnoreCase("Coffee Table") || name.Contains("Gilded Iron Base")},
            {HomewareCategory.Console_Tables, name => name.ContainsIgnoreCase("Console Table")},
            {HomewareCategory.Decorative_Trays, name => name.ContainsIgnoreCase("Tray")},
            {HomewareCategory.Dining_Tables, name => name.ContainsIgnoreCase("Dining Table")},
            {HomewareCategory.Dressers, name => name.ContainsIgnoreCase("Dresser") || name.ContainsIgnoreCase("Cabinet") || name.ContainsIgnoreCase("Chest")},
            {HomewareCategory.End_Tables, name => name.ContainsIgnoreCase("End Table")},
            {HomewareCategory.Floor_Lamps, name => name.ContainsIgnoreCase("Floor Lamp")},
            {HomewareCategory.Mirrors, name => name.ContainsIgnoreCase("Mirror")},
            {HomewareCategory.Pendants, name => name.ContainsIgnoreCase("Pendant")},
            {HomewareCategory.Sofas, name => name.ContainsIgnoreCase("Sofa")},
            {HomewareCategory.Side_Tables, name => name.ContainsIgnoreCase("Side Table") || name.ContainsIgnoreCase("Round Table")},
            {HomewareCategory.Table_Lamps, name => name.ContainsIgnoreCase("Desk Lamp")},
            {HomewareCategory.Wall_Lamps, name => name.ContainsIgnoreCase("Wall Light")},
        };

        public WildWoodProductBuilder(IPriceCalculator<WildWoodLampsVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new WildWoodLampsVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount].Replace(" available", "").ToIntegerSafe()), 
                mpn, BuildFeatures(data));

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Category], data[ScanField.ProductName]);
            homewareProduct.ManufacturerDescription = data[ScanField.LongDescription].TitleCase();
            homewareProduct.MinimumQuantity = 1;
            homewareProduct.Name = data[ScanField.ProductName].TitleCase();
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = GetImages(data[ScanField.ImageUrl]);
            return homewareProduct;
        }

        private List<ScannedImage> GetImages(string filename)
        {
            return new List<ScannedImage>
            {
                new ScannedImage(ImageVariantType.Primary, "http://scanner.insidefabric.com/vendors/Willdwood/" + filename),
            };
        }

        private HomewareCategory AssignCategory(string categoryName, string name)
        {
            foreach (var category in _categories)
                if (category.Value(categoryName))
                    return category.Key;

            foreach (var category in _secondPass)
                if (category.Value(name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Brand = ExpandBrand(data[ScanField.Brand]);
            features.Color = data[ScanField.ShadeColor];
            features.Height = data[ScanField.Height].ToDoubleSafe();
            features.Depth = data[ScanField.Depth].ToDoubleSafe();
            features.Width = data[ScanField.Width].ToDoubleSafe();

            features.Features = BuildSpecs(data);
            return features;
        }

        private string ExpandBrand(string brandCode)
        {
            if (brandCode == "WW") return "Wildwood";
            if (brandCode == "FC") return "Frederick Cooper";
            if (brandCode == "JC") return "Jonathan Charles";
            if (brandCode == "CH") return "Chelsea House";
            if (brandCode == "JM") return "Jamie Merida";
            if (brandCode == "LD") return "Lauren Deloach";

            if (brandCode == "JE") return "Jonathan Charles";
            if (brandCode == "JO") return "Jonathan Charles";

            return string.Empty;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specDict = new Dictionary<ScanField, string>
            {
                { ScanField.Bulb, data[ScanField.Bulb] },
                { ScanField.Bulbs, data[ScanField.Bulbs] },
                { ScanField.Collection, data[ScanField.Collection] },
                { ScanField.Finish, data[ScanField.Finish].TitleCase() },
                { ScanField.Material, data[ScanField.Material].TitleCase() },
                { ScanField.SwitchType, data[ScanField.SwitchType] },
                { ScanField.Wattage, data[ScanField.Wattage] },
            };
            return ToSpecs(specDict);
        }
    }
}