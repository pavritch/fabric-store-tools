using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace AidanGrayHome
{
    public class AidenGrayHomeProductBuilder : ProductBuilder<AidanGrayHomeVendor>
    {
        public AidenGrayHomeProductBuilder(IPriceCalculator<AidanGrayHomeVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendor = new AidanGrayHomeVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(IsInStock(data[ScanField.StockCount])), mpn, BuildFeatures(data));

            homewareProduct.Correlator = GetCorrelator(mpn);
            homewareProduct.HomewareCategory = AssignCategory(data);
            homewareProduct.ManufacturerDescription = FormatDescription(data[ScanField.Description]);
            homewareProduct.Name = data[ScanField.ProductName].Replace("&amp;", "&");
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private string FormatDescription(string description)
        {
            if (description == ".") return string.Empty;
            return description;
        }

        // we can add more general catches toward the bottom, since these are processed in order
        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            { HomewareCategory.Accent_Chairs, (cat, name) => name.ContainsAnyOf("Salon Chair", "Occasional Chair")},
            { HomewareCategory.Accessories, (cat, name) => cat.Contains("Accessories") || name.ContainsAnyOf("Holder", "Finial", "Ornament")},
            { HomewareCategory.Bar_Stools, (cat, name) => name.ContainsAnyOf("Barstool", "Bar Stool", "Counter Stool")},
            { HomewareCategory.Baskets, (cat, name) => name.Contains("Basket")},
            { HomewareCategory.Benches, (cat, name) => name.Contains("Bench")},
            { HomewareCategory.Bookcases, (cat, name) => name.Contains("Book Shelf")},
            { HomewareCategory.Cabinets, (cat, name) => name.Contains("Cabinet")},
            { HomewareCategory.Candles, (cat, name) => name.EndsWith("Candle")},
            { HomewareCategory.Candleholders, (cat, name) => name.ContainsAnyOf("Candlestick", "Candle Stick", "Candle Drip", "Candelabra", "Candleholder", "Candle Holder", "Rustic Wood", "Rust Wood") || 
                cat.Contains("Candle Sconces")},
            { HomewareCategory.Chandeliers, (cat, name) => cat.Contains("Lighting, Chandeliers") || name.ContainsAnyOf("Chandelier", "Pumpkin") || (name.Contains("Globe") && !name.Contains("Lamp"))},
            { HomewareCategory.Console_Tables, (cat, name) => name.Contains("Console")},
            { HomewareCategory.Desks, (cat, name) => name.Contains("Desk")},
            { HomewareCategory.Decorative_Trays, (cat, name) => name.Contains("Tray")},
            { HomewareCategory.Dining_Chairs, (cat, name) => name.ContainsAnyOf("Dining Chair", "Dining Arm Chair")},
            { HomewareCategory.Doors, (cat, name) => name.Contains("Door")},
            { HomewareCategory.Dressers, (cat, name) => name.ContainsAnyOf("Chest", "Dresser")},
            { HomewareCategory.Floor_Lamps, (cat, name) => name.Contains("Floor Lamp")},
            { HomewareCategory.Garden_Decorations, (cat, name) => cat.Contains("Garden, Decorative") || name.ContainsAnyOf("Urn", "Garden")},
            { HomewareCategory.Garden_Planters, (cat, name) => cat.Contains("Garden, Planters") || name.Contains("Planter")},
            { HomewareCategory.Lighting, (cat, name) => name.Contains("Light Bulb")},
            { HomewareCategory.Love_Seats, (cat, name) => name.Contains("Love Seat")},
            { HomewareCategory.Mirrors, (cat, name) => cat.Contains("Mirrors") || name.Contains("Mirror")},
            { HomewareCategory.Pendants, (cat, name) => name.ContainsAnyOf("Lantern", "Pendant")},
            { HomewareCategory.Sconces, (cat, name) => cat.Contains("Lighting, Sconces") || name.ContainsAnyOf("Sconce")},
            { HomewareCategory.Sideboards, (cat, name) => name.ContainsAnyOf("Buffet", "Credenza", "Cradenza", "Sideboard")},
            { HomewareCategory.Side_Chairs, (cat, name) => name.ContainsAnyOf("Side Chair", "Arm Chair")},
            { HomewareCategory.Side_Tables, (cat, name) => name.ContainsAnyOf("Side Table", "Occasional Table")},
            { HomewareCategory.Sofas, (cat, name) => name.ContainsAnyOf("Chaise", "Sofa")},
            { HomewareCategory.Storage, (cat, name) => name.ContainsAnyOf("Storage")},
            { HomewareCategory.Table_Lamps, (cat, name) => cat.Contains("Table Lamps") || name.Contains("Table Lamp") || (name.Contains("Lamp") && !name.Contains("Table"))},
            { HomewareCategory.Vases, (cat, name) => name.ContainsAnyOf("Floral Element", "Vase")},
            { HomewareCategory.Wall_Art, (cat, name) => cat.ContainsAnyOf("Art", "Decor") || name.Contains("Decor Piece")},
            { HomewareCategory.Wall_Lamps, (cat, name) => name.Contains("Ceiling Mount")},

            //{ HomewareCategory.DiningChairs, (cat, name) => name.Contains("Chair")},
            { HomewareCategory.Dining_Tables, (cat, name) => name.Contains("Table")},
            { HomewareCategory.Ignored, (cat, name) => name.ContainsAnyOf("Rug", "Window")},
        };

        private HomewareCategory AssignCategory(ScanData data)
        {
            foreach (var category in _categories)
                if (category.Value(data[ScanField.Category], data[ScanField.ProductName]))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private bool IsInStock(string stock)
        {
            // Also has Date Expected: on many products
            return stock == "In Stock";
        }

        private string GetCorrelator(string mpn)
        {
            // CH600-Leather, CH600-Linen
            // F347 STEEL, F347 AB
            // F525WHT, F525GRY
            // ML13 CRYSTAL, ML13 GRAY
            // WL294 Double, WL294 Single
            // L504 BROWN CHAN, L504 GRAY CHAN
            var split = mpn.Split(new[] {' ', '-'}).ToList();
            return split.First();
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Color = data[ScanField.Color].Replace("&amp;", "&");
            features.Depth = data[ScanField.Depth].Replace("&quot;", "").ToDoubleSafe();
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            features.Height = data[ScanField.Height].Replace("&quot;", "").ToDoubleSafe();
            features.ShippingWeight = data[ScanField.ShippingWeight].ToDoubleSafe();
            features.Width = data[ScanField.Width].Replace("&quot;", "").ToDoubleSafe();
            return features;
        }

        private string FormatMaterial(string material)
        {
            var materials = material.Split(new[] {','}).Select(x => x.Trim().TitleCase());
            return string.Join(", ", materials);
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.AssemblyInstruction, data[ScanField.AssemblyInstruction]},
                {ScanField.BaseWidth, data[ScanField.BaseWidth].Replace(" out from wall", "").ToInchesMeasurement()},
                {ScanField.Collection, data[ScanField.Collection]},
                {ScanField.ColorGroup, data[ScanField.ColorGroup]},
                {ScanField.CordLength, data[ScanField.CordLength]},
                {ScanField.Material, FormatMaterial(data[ScanField.Material])},
                {ScanField.Wattage, data[ScanField.Wattage]},
                {ScanField.Weight, data[ScanField.Weight]},
            };
            if (data[ScanField.Bulbs] != "0") specs.Add(ScanField.Bulbs, data[ScanField.Bulbs].Replace("&quot;", ""));
            return ToSpecs(specs);
        }
    }
}