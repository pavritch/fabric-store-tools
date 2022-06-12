using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace IMAX
{
    public class ImaxDimensions
    {
        public string Depth { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }

        public bool None()
        {
            return Depth.ToDoubleSafe() <= 0 && Height.ToDoubleSafe() <= 0 && Width.ToDoubleSafe() <= 0;
        }
    }

    public class ImaxProductBuilder : ProductBuilder<ImaxVendor>
    {
        public ImaxProductBuilder(IPriceCalculator<ImaxVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendor = new ImaxVendor();
            var name = FormatName(data[ScanField.ProductName]);
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true), mpn, BuildFeatures(data));

            homewareProduct.HomewareCategory = AssignCategory(name);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = data[ScanField.ProductName];
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = new List<ScannedImage>
            {
                new ScannedImage(ImageVariantType.Primary, string.Format("https://repzioimages.global.ssl.fastly.net/productimages/121/{0}_lg.jpg", mpn))
            };
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accessories, name => name.ContainsAnyOf("Globe", "Metal Letter", "Swan", "Crains", "Sailboat", "Birds", "Glass Float", 
                "Crystal Sphere", "Turtle", "Decorative Dice", "Shell", "Decorative Balls", "Reclaimed Metal", "Rooster", "Rabbits", "Sphere", 
                "Medallion", "Container", "Pear", "Orbs", "Seahorse", "Seabirds", "Whale", "Horse", "Finial", "Frogs", "Elephant", "Crab",
                "Octopus", "Pineapple", "Dinosaur", "Chicken", "Hourglass", "Orchid", "Stallion", "Deco Balls", "Silver Balls", "Gold Balls",
                "Owls", "Cranes", "Rhino", "Reindeer", "Antlers", "Pedestal", "Starfish", "Angel", "Bell", "Pumpkin", "Gourd", "Shadowbox",
                "Handcarved", "Bird")},
            {HomewareCategory.Bar_Carts, name => name.Contains("Bar Cart")},
            {HomewareCategory.Bar_Stools, name => name.ContainsAnyOf("Barstool", "Stool", "Bar Chair") && !name.Contains("Garden")},
            {HomewareCategory.Bar_Tables, name => name.Contains("Bar Table")},
            {HomewareCategory.Baskets, name => name.Contains("Basket")},
            {HomewareCategory.Benches, name => name.Contains("Bench")},
            {HomewareCategory.Birdfeeders, name => name.Contains("Bird Feeder")},
            {HomewareCategory.Birdhouses, name => name.Contains("Bird House")},
            {HomewareCategory.Bookcases, name => name.Contains("Bookshelf")},
            {HomewareCategory.Bookends, name => name.Contains("Bookends")},
            {HomewareCategory.Bottles, name => name.Contains("Bottle")},
            {HomewareCategory.Bowls, name => name.Contains("Bowl")},
            {HomewareCategory.Cabinets, name => name.Contains("Cabinet")},
            {HomewareCategory.Candles, name => name.EndsWith("Candle")},
            {HomewareCategory.Candleholders, name => name.ContainsAnyOf("Hurricane", "Candelabra", "Wax Candle", "Lantern", 
                "Candle Stand", "Candle Holder", "Candleholder", "Candle Floor Cylinder", "Votive", "Tealight Holders", "Candlestick")},
            {HomewareCategory.Centerpieces, name => name.Contains("Centerpiece")},
            {HomewareCategory.Chandeliers, name => name.Contains("Chandelier")},
            {HomewareCategory.Clocks, name => name.Contains("Clock")},
            {HomewareCategory.Coasters, name => name.Contains("Coasters")},
            {HomewareCategory.Coffee_Tables, name => name.Contains("Coffee Table")},
            {HomewareCategory.Console_Tables, name => name.Contains("Console")},
            {HomewareCategory.Decanters, name => name.Contains("Decanter")},
            {HomewareCategory.Decorative_Boxes, name => name.ContainsAnyOf("Boxes", "Box")},
            {HomewareCategory.Decorative_Pillows, name => name.Contains("Pillow")},
            {HomewareCategory.Decorative_Trays, name => name.Contains("Tray")},
            {HomewareCategory.Decorative_Trunks, name => name.Contains("Trunk")},
            {HomewareCategory.Desks, name => name.Contains("Desk") && !name.Contains("Lamp")},
            {HomewareCategory.Dining_Chairs, name => name.ContainsAnyOf("Dining Chair", "Side Chair")},
            {HomewareCategory.Dining_Tables, name => name.Contains("Dining Table")},
            {HomewareCategory.Dressers, name => name.ContainsAnyOf("Etagere", "Armoire", "Chest", "Dresser")},
            {HomewareCategory.End_Tables, name => name.ContainsAnyOf("Nesting Tables", "Nested Table")},
            {HomewareCategory.Faux_Flowers, name => name.Contains("Floral")},
            {HomewareCategory.Filing, name => name.Contains("File Holder")},
            {HomewareCategory.Fire_Screens, name => name.ContainsAnyOf("Firescreen", "Fire Screen")},
            {HomewareCategory.Flowers_and_Plants, name => name.ContainsAnyOf("Bundle")},
            {HomewareCategory.Floor_Lamps, name => name.Contains("Floor Lamp")},
            {HomewareCategory.Food_Storage, name => name.ContainsAnyOf("Caddy", "Canister")},
            {HomewareCategory.Garden_Decorations, name => name.ContainsAnyOf("Obelisk", "Garden Stake", "Fountain")},
            {HomewareCategory.Garden_Planters, name => name.ContainsAnyOf("Urn", "Pot", "Planter", "Terrarium")},
            {HomewareCategory.Garden_Stools, name => name.Contains("Garden Stool")},
            {HomewareCategory.Ice_Buckets, name => name.Contains("Bucket")},
            {HomewareCategory.Jars, name => name.Contains("Jar")},
            {HomewareCategory.Kitchen_and_Dining, name => name.ContainsAnyOf("Glasses", "Pitcher", "Server", "Knives", "Cheese", "Cutting", "Bread", "Rolling Pin")},
            {HomewareCategory.Luggage, name => name.Contains("Suitcases")},
            {HomewareCategory.Mirrors, name => name.Contains("Mirror")},
            {HomewareCategory.Ottomans, name => name.Contains("Ottoman")},
            {HomewareCategory.Outdoor_Decor, name => name.Contains("Weather")},
            {HomewareCategory.Pendants, name => name.Contains("Pendant")},
            {HomewareCategory.Pillows_Throws_and_Poufs, name => name.Contains("Cushion")},
            {HomewareCategory.Plates, name => name.ContainsAnyOf("Charger", "Dish", "Plate")},
            {HomewareCategory.Sconces, name => name.ContainsAnyOf("Wall Sconce", "Sconce")},
            {HomewareCategory.Statues_and_Sculptures, name => name.ContainsAnyOf("Sculpture", "Statuary", "Statuaries", "Statue")},
            {HomewareCategory.Shelves, name => name.ContainsAnyOf("Shelf", "Shelves")},
            {HomewareCategory.Sideboards, name => name.ContainsAnyOf("Buffet", "Sideboard")},
            {HomewareCategory.Side_Chairs, name => name.Contains("Side Chair")},
            {HomewareCategory.Side_Tables, name => name.ContainsAnyOf("Wood Table", "Side Table")},
            {HomewareCategory.Sofas, name => name.Contains("Sofa")},
            {HomewareCategory.Stands, name => name.Contains("Stand")},
            {HomewareCategory.Storage, name => name.ContainsAnyOf("Cubby", "Crates", "Storage", "Bins")},
            {HomewareCategory.Supplies, name => name.Contains("Easel")},
            {HomewareCategory.Table_Lamps, name => name.ContainsAnyOf("Lamp", "Cloche") && (!name.Contains("Floor") && !name.Contains("Desk"))},
            {HomewareCategory.Throws, name => name.Contains("Throw")},
            {HomewareCategory.Utensils, name => name.Contains("Scoop")},
            {HomewareCategory.Vases, name => name.ContainsAnyOf("Jug", "Vase")},
            {HomewareCategory.Wall_Art, name => name.ContainsAnyOf("Wall Art", "Wall Flower", "Wood Panel", "Oil Painting", 
                "Wall Decor", "Gallery Art", "Wall Dice", "Oil on Canvas", "Frame", "Metal Art", "Wall Plaque", "Hanging Panel",
                "Wall Panel", "Plaque", "Wall Signs", "Sign", "Picture Holder", "Dimensional Art", "CKI", "Painting", "Oil Canvas",
                "Wall Letters", "Art Panel")},
            {HomewareCategory.Watering, name => name.Contains("Watering Can")},
            {HomewareCategory.Wall_Lamps, name => name.Contains("Wall Mount")},
            {HomewareCategory.Windchimes, name => name.Contains("Windchime")},
            {HomewareCategory.Wine_Racks, name => name.Contains("Wine")},

            {HomewareCategory.Ignored, name => name.ContainsAnyOf("Bracelet", "Necklace", "Ring", "Scarf", "Bag", "Jewelry", "Christmas", "Snowman", "Btq")},
        };

        private string FormatName(string name)
        {
            name = name.Replace("DÃƒÂ©cor", "Decor");
            name = name.Replace("Décor", "Decor");
            name = name.Replace("Stauary", "Statuary");
            return name.TitleCase();
        }

        private HomewareCategory AssignCategory(string productName)
        {
            foreach (var category in _categories)
                if (category.Value(productName))
                    return category.Key;
            if (productName.Contains("Table")) return HomewareCategory.Accent_Tables;
            if (productName.Contains("Chair")) return HomewareCategory.Accent_Chairs;
            //if (productName.Contains("BTQ")) return HomewareCategory.Jewelry;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Features = BuildSpecs(data);
            features.Description = new List<string> {data[ScanField.Description]};

            var dimensions = ParseDimensions(data[ScanField.Dimensions]);
            if (!dimensions.None())
            {
                features.Depth = dimensions.Depth.ToDoubleSafe();
                features.Height = dimensions.Height.ToDoubleSafe();
                features.Width = dimensions.Width.ToDoubleSafe();
            }
            else
            {
                features.Features.Add(ScanField.Dimensions.ToString(), data[ScanField.Dimensions].Replace("(", "").Replace(")", ""));
            }

            return features;
        }

        private ImaxDimensions ParseDimensions(string dimensions)
        {
            dimensions = dimensions.Replace("(", "").Replace(")", "").Replace("\"", "").Replace("'", "");
            var parts = dimensions.Split(new[] {" x "}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var imaxDimensions = new ImaxDimensions();
            foreach (var part in parts)
            {
                if (part.ContainsIgnoreCase("h")) imaxDimensions.Height = part.Replace("h", "", StringComparison.OrdinalIgnoreCase).Split('-').First();
                if (part.ContainsIgnoreCase("w")) imaxDimensions.Width = part.Replace("w", "", StringComparison.OrdinalIgnoreCase).Split('-').First();
                if (part.ContainsIgnoreCase("d")) imaxDimensions.Depth = part.Replace("d", "", StringComparison.OrdinalIgnoreCase).Split('-').First();
            }
            return imaxDimensions;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                {ScanField.Designer, data[ScanField.Designer]},
                {ScanField.FoodSafe, data[ScanField.FoodSafe]},
                {ScanField.ItemNumber, data[ScanField.ItemNumber]},
            });
        }

        private bool IsInStock(string stock)
        {
            if (stock == "Out of Stock") return false;
            if (stock == "Backorder") return false;

            if (stock == "In Stock") return true;
            if (stock == "In Transit") return true;
            if (stock == "Low Stock") return true;
            return true;
        }
    }
}
