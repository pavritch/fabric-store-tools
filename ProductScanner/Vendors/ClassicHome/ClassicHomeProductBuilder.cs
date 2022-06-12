using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ClassicHome
{
    public class ClassicHomeDimensions
    {
        public double Depth { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
    }

    public class ClassicHomeProductBuilder : ProductBuilder<ClassicHomeVendor>
    {
        public ClassicHomeProductBuilder(IPriceCalculator<ClassicHomeVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var name = FormatName(data[ScanField.ProductName]);

            var vendor = new ClassicHomeVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(GetStock(data[ScanField.StockCount])), mpn, BuildFeatures(data));

            var bullets = ParseBullets(data[ScanField.Bullets]);
            data.AddFields(bullets);

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Category], name);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = name;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            {HomewareCategory.Accessories, (cat, name) => name.ContainsAnyOf("Bottle Rack", "Oil Lamps", "Wheel")},
            {HomewareCategory.Accent_Chairs, (cat, name) => name.ContainsAnyOf("Club", "Chair", "Square Cube") && !name.Contains("Dining") && !name.Contains("Side Chair")},
            {HomewareCategory.Accent_Shelves, (cat, name) => name.ContainsAnyOf("Freestanding Display", "Wall Unit")},
            {HomewareCategory.Accent_Tables, (cat, name) => name.ContainsAnyOf("Accent Table", "Cube Table", "Box Table", "Box Ofk", "Table Ofk", "Box Sfk", "Wd Table", "Wd Box", "2 Drawer Table")},
            {HomewareCategory.Bar_Stools, (cat, name) => name.ContainsAnyOf("Barstool", "Stool")},
            {HomewareCategory.Bar_Tables, (cat, name) => name.ContainsAnyOf("Bar Table", "Pub Table", "Bar Counter", "Counter Bar", "Shutter Bar")},
            {HomewareCategory.Bedding, (cat, name) => cat.Contains("Bedding")},
            {HomewareCategory.Bedroom_Sets, (cat, name) => name.Contains("Bedroom Set")},
            {HomewareCategory.Beds, (cat, name) => name.Contains(" Bed")},
            {HomewareCategory.Benches, (cat, name) => name.Contains("Bench")},
            {HomewareCategory.Bookcases, (cat, name) => name.ContainsAnyOf("Bookcase", "Bookshelf", "Book Shelf")},
            {HomewareCategory.Cabinets, (cat, name) => name.ContainsAnyOf("Cabinet", "Almirah", "Cube", "Storage Box", "W/C Box", "WD Box", "Balcony Window", "Wd Glass", "Iron Box") ||
                (name.Contains("Chest") && !name.Contains("Chestnut"))},
            {HomewareCategory.Chandeliers, (cat, name) => name.Contains("Chandelier")},
            {HomewareCategory.Coffee_Tables, (cat, name) => name.ContainsAnyOf("Coffee Table", "Blanket Box")},
            {HomewareCategory.Console_Tables, (cat, name) => name.Contains("Console")},
            {HomewareCategory.Curtains, (cat, name) => cat.Contains("Curtains")},
            {HomewareCategory.Decorative_Pillows, (cat, name) => cat.Contains("Pillows")},
            {HomewareCategory.Desks, (cat, name) => name.Contains("Desk")},
            {HomewareCategory.Dining_Chairs, (cat, name) => name.Contains("Dining Chair")},
            {HomewareCategory.Dining_Sets, (cat, name) => name.ContainsAnyOf("Dining Set", "Occasional Set")},
            {HomewareCategory.Dining_Tables, (cat, name) => name.ContainsAnyOf("Dining Table", "Breakfast Table", "Round Table", "Octagonal Table", "Square Table")},
            {HomewareCategory.Doors, (cat, name) => name.Contains("Door")},
            {HomewareCategory.Dressers, (cat, name) => name.Contains("Dresser") || name.Contains("Tallboy")},
            {HomewareCategory.End_Tables, (cat, name) => name.ContainsAnyOf("End Table", "Crank Table", "Crank Pub Table", "Lamp Table")},
            {HomewareCategory.Gathering_Table, (cat, name) => name.Contains("Gathering Table")},
            {HomewareCategory.Island, (cat, name) => name.Contains("Kitchen Island")},
            {HomewareCategory.Mirrors, (cat, name) => name.Contains("Mirror")},
            {HomewareCategory.Nightstands, (cat, name) => name.Contains("Nightstand")},
            {HomewareCategory.Pendants, (cat, name) => name.Contains("Pendant")},
            {HomewareCategory.Railings, (cat, name) => name.Contains("Railing")},
            {HomewareCategory.Sideboards, (cat, name) => name.ContainsAnyOf("Sideboard", "Side Board", "Buffet", "Server")},
            {HomewareCategory.Side_Chairs, (cat, name) => name.Contains("Side Chair")},
            {HomewareCategory.Side_Tables, (cat, name) => name.ContainsAnyOf("Side Table", "Bedside")},
            {HomewareCategory.Sofas, (cat, name) => name.Contains("Sofa")},
            {HomewareCategory.TV_Stands, (cat, name) => name.ContainsAnyOf("Tv Stand", "Tv Chest", "Plasma", "Entertainment Stand")},
            {HomewareCategory.Vanities, (cat, name) => name.Contains("Vanity")},
            {HomewareCategory.Wall_Art, (cat, name) => name.Contains("Panel")},
        };

        private HomewareCategory AssignCategory(string categoryName, string name)
        {
            foreach (var category in _categories)
                if (category.Value(categoryName, name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private ScanData ParseBullets(string bullets)
        {
            var data = new ScanData();
            var fields = bullets.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var field in fields)
            {
                if (field.ContainsIgnoreCase(" x ")) data[ScanField.Dimensions] = field;
                if (field.ContainsIgnoreCase("Bulb")) data[ScanField.Bulbs] = field;
                if (field.ContainsIgnoreCase("%")) data[ScanField.Material] = field;
                if (field.ContainsIgnoreCase("Seat Height")) data[ScanField.SeatHeight] = field;
                if (field.ContainsIgnoreCase("Chain")) data[ScanField.ChainLength] = field;
                if (field.ContainsIgnoreCase("Adjustable height")) data[ScanField.AdjustableHeight] = field;
                if (field.ContainsIgnoreCase("Finish")) data[ScanField.Finish] = field;
                if (field.ContainsIgnoreCase("Arm Height")) data[ScanField.ArmHeight] = field;
                if (field.ContainsIgnoreCase("Wood")) data[ScanField.WoodType] = field;
                if (field.ContainsIgnoreCase("Drawers")) data[ScanField.Drawers] = field;
            }
            return data;
        }

        private string FormatName(string name)
        {
            return name
                .ReplaceWholeWord("Blk", "Black")
                .ReplaceWholeWord("Dbl", "Double")
                .ReplaceWholeWord("RndCoffee", "Round Coffee")
                .ReplaceWholeWord("60Tbl", "Table")
                .Replace("SpiralStairChandelier", "Spiral Stair Chandelier")
                .Replace("NikaCrankTbl", "Nika Crank Table")
                .Replace("PierreCaptChairMossStripe", "Pierre Capt Chair Moss Stripe")
                .Replace("TripleArchBookshelf", "Triple Arch Bookshelf")
                .ReplaceWholeWord("Dr", "Door")
                .ReplaceWholeWord("1Dr", "1 Door")
                .ReplaceWholeWord("2Dr", "2 Door")
                .ReplaceWholeWord("3Dr", "3 Door")
                .ReplaceWholeWord("4Dr", "4 Door")
                .ReplaceWholeWord("6Dr", "6 Door")
                .ReplaceWholeWord("3Pc", "3 Piece")
                .ReplaceWholeWord("3pc", "3 Piece")
                .ReplaceWholeWord("6pc", "6 Piece")
                .ReplaceWholeWord("Rnd", "Round")
                .ReplaceWholeWord("Dwr", "Drawer")
                .ReplaceWholeWord("Drw", "Drawer")
                .ReplaceWholeWord("Chr", "Chair")
                .ReplaceWholeWord("Brn", "Brown")
                .ReplaceWholeWord("Pc", "Piece")
                .ReplaceWholeWord("Dk", "Dark")
                .ReplaceWholeWord("Sqr", "Square")
                .ReplaceWholeWord("Crvd", "Carved")
                .ReplaceWholeWord("Choc", "Chocolate")
                .ReplaceWholeWord("Lavndr", "Lavender")
                .ReplaceWholeWord("Rect", "Rectangle")
                .ReplaceWholeWord("Tab Top", "Tabletop")
                .ReplaceWholeWord("Turq", "Turquoise")
                .ReplaceWholeWord("1Dwr", "1 Drawer")
                .ReplaceWholeWord("2Dwr", "2 Drawer")
                .ReplaceWholeWord("3Dwr", "3 Drawer")
                .ReplaceWholeWord("4Dwr", "4 Drawer")
                .ReplaceWholeWord("5Dwr", "5 Drawer")
                .ReplaceWholeWord("6Dwr", "6 Drawer")
                .ReplaceWholeWord("7Dwr", "7 Drawer")
                .ReplaceWholeWord("9Dwr", "9 Drawer")
                .ReplaceWholeWord("Cal", "California")
                .ReplaceWholeWord("Cab", "Cabinet")
                .ReplaceWholeWord("Coffe", "Coffee")
                .ReplaceWholeWord("Wd", "Wood")
                .ReplaceWholeWord("Conosle", "Console")
                .ReplaceWholeWord("CounterStool", "Counter Stool")
                .ReplaceWholeWord("Counterstool", "Counter Stool")
                .Replace("Media Cnsl", "Media Console")
                .ReplaceWholeWord("BacklessCntrstool", "Backless Counter Stool")
                .ReplaceWholeWord("KitchenIsland", "Kitchen Island")
                .ReplaceWholeWord("Tbl", "Table")
                .TitleCase();
        }

        private int GetStock(string stock)
        {
            if (stock.ContainsIgnoreCase("Out of Stock")) return 0;
            return stock.Replace("Available: ", "").ToIntegerSafe();
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var dimensions = GetDimensions(data[ScanField.Bullets]);

            var features = new HomewareProductFeatures();
            features.Depth = dimensions.Depth;
            features.Features = BuildSpecs(data);
            features.Height = dimensions.Height;
            features.ShippingWeight = GetWeight(data[ScanField.AdditionalInfo]).ToDoubleSafe();
            features.Width = dimensions.Width;
            return features;
        }

        private string GetWeight(string additionalInfo)
        {
            var weight = additionalInfo.Split(new[] {','}).SingleOrDefault(x => x.Contains("Package Weight"));
            if (weight != null)
                return weight.Replace("Product Weight: ", "");
            return string.Empty;
        }

        private ClassicHomeDimensions GetDimensions(string bullets)
        {
            if (bullets == string.Empty) return new ClassicHomeDimensions();

            // first bullet is always dimensions
            var dimensions = bullets.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries).First();
            var parts = dimensions.Split(new[] {'x'}).Select(x => x.Trim());
            var dimObj = new ClassicHomeDimensions();
            foreach (var part in parts)
            {
                if (part.ContainsIgnoreCase("D")) dimObj.Depth = part.Replace("D", "").ToDoubleSafe();
                if (part.ContainsIgnoreCase("W")) dimObj.Width = part.Replace("W", "").ToDoubleSafe();
                if (part.ContainsIgnoreCase("H")) dimObj.Height = part.Replace("H", "").ToDoubleSafe();
            }
            return dimObj;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                { ScanField.Material, GetMaterial(data[ScanField.Bullets])},
                { ScanField.RoomType, data[ScanField.RoomType]},
                { ScanField.WoodType, data[ScanField.WoodType].TitleCase().Replace("&quot;", "\"")},
            };
            if (!data[ScanField.Style].ContainsDigit())
                specs[ScanField.Style] = data[ScanField.Style];
            return ToSpecs(specs);
        }

        private string GetMaterial(string bullets)
        {
            var parts = bullets.Split(new[] {','}).ToList();
            foreach (var part in parts)
            {
                if (part.ContainsAnyOf(_materialKeys.ToArray()))
                    return part.Trim().TitleCase().Replace("&quot;", "\"").Replace("Appliqu?", "Applique");
            }
            return string.Empty;
        }

        private readonly List<string> _materialKeys = new List<string>
        {
            "Cotton",
            "Leather",
            "Linen",
            "Nylon",
            "Poly",
            "Rayon",
        }; 
    }
}