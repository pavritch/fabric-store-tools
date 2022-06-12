using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace YourOtherWarehouse
{
    public class YourOtherWarehouseProductBuilder : ProductBuilder<YourOtherWarehouseVendor>
    {
        public YourOtherWarehouseProductBuilder(IPriceCalculator<YourOtherWarehouseVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var stock = data[ScanField.StockCount];
            var vendor = new YourOtherWarehouseVendor();
            var name = FormatName(data[ScanField.ProductName]);
            var isDiscontinued = IsDiscontinued(data[ScanField.ProductName]);

            var vendorProduct = new HomewareProduct(data.Cost, vendor, new StockData(stock.ToDoubleSafe()), mpn, BuildFeatures(data));

            vendorProduct.ScannedImages = new List<ScannedImage> { new ScannedImage(ImageVariantType.Primary, data[ScanField.Image1].Replace("econtent@", "econtent%40")) };

            vendorProduct.HomewareCategory = AssignCategory(data[ScanField.Classification], data[ScanField.ProductType]);
            vendorProduct.Correlator = mpn;
            vendorProduct.IsDiscontinued = isDiscontinued;
            vendorProduct.ManufacturerDescription = data[ScanField.Description];
            vendorProduct.Name = new[] {mpn, name}.BuildName();
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            vendorProduct.UnitOfMeasure = UnitOfMeasure.Each;
            return vendorProduct;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Brand = data[ScanField.Brand].TitleCase();
            features.Bullets = MakeBulletList(data);
            features.Color = FormatColor(data[ScanField.Color], data[ScanField.ProductName]);
            features.Depth = data[ScanField.Length].ToDoubleSafe();
            features.Features = BuildSpecs(data);
            features.Height = data[ScanField.Height].ToDoubleSafe();
            features.ShippingWeight = data[ScanField.Weight].ToDoubleSafe();
            features.UPC = data[ScanField.UPC];
            features.Width = data[ScanField.Width].ToDoubleSafe();
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                {ScanField.Country, string.Join(", ", data[ScanField.Country].TitleCase().Split(new[] {','}).Select(x => new Country(x).Format()))},
                {ScanField.ItemNumber, data[ScanField.ItemNumber]},
            });
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            {HomewareCategory.Bathroom_Fans, (@class, subclass) => subclass.Contains("BATH FANS")},
            {HomewareCategory.Bathroom_Hardware, (@class, subclass) => subclass.Contains("BATH HARDWARE")},
            {HomewareCategory.Bathroom_Storage, (@class, subclass) => subclass.Contains("BATH STORAGE")},
            {HomewareCategory.Bathwares, (@class, subclass) => subclass.Contains("BATHWARES")},
            {HomewareCategory.Cabinets, (@class, subclass) => subclass == "CABINET" || subclass.Contains("RTA FINISH CABINETS") || subclass.Contains("CABINET ORGANIZATION")},
            {HomewareCategory.Faucets, (@class, subclass) => subclass.Contains("S/O FAUCETS") || subclass.Contains("TWO HANDLE BATH FAUCETS") || subclass.Contains("TWO HANDLE KITCHEN FAU")},
            {HomewareCategory.Mirrors, (@class, subclass) => subclass.Contains("MIRRORS")},
            {HomewareCategory.Mounts, (@class, subclass) => subclass.Contains("CEILING MOUNT")},
            {HomewareCategory.Pendants, (@class, subclass) => subclass.Contains("PENDANT")},
            {HomewareCategory.Sinks, (@class, subclass) => subclass.Contains("CERAMIC SINKS") || subclass.Contains("LAVATORY SINKS")},
            {HomewareCategory.Side_Tables, (@class, subclass) => subclass.Contains("TABLES AND STANDS")},
            {HomewareCategory.Vanities, (@class, subclass) => subclass.Contains("VANITIES") || subclass.Contains("VANITY,WALL & SCONCE")},
            {HomewareCategory.Wall_Art, (@class, subclass) => subclass.Contains("NUMBERS, LETTERS")},
        };

        private HomewareCategory AssignCategory(string classification, string subclass)
        {
            foreach (var category in _categories)
                if (category.Value(classification, subclass))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private decimal FindWholesale(string multiplier, string retailPrice)
        {
            //var mult = multiplier.ToDecimalSafe();
            var retail = retailPrice.ToDecimalSafe();
            //return Math.Round(mult*retail, 2);
            // looks like difference between retail (in spreadsheet) and cost (on website) is about 1.75
            return Math.Round(retail/1.75m, 2);
        }

        private List<string> MakeBulletList(ScanData data)
        {
            return new List<string>
            {
                data[ScanField.Bullet1],
                data[ScanField.Bullet2],
                data[ScanField.Bullet3],
                data[ScanField.Bullet4],
                data[ScanField.Bullet5],
                data[ScanField.Bullet6],
                data[ScanField.Bullet7],
                data[ScanField.Bullet8],
                data[ScanField.Bullet9],
                data[ScanField.Bullet10],
            };
        }

        private List<string> _validColors = new List<string>
        {
            "StarLight Chrome",
            "a Polished Chrome",
            "Absinthe",
            "Acorn",
            "Aegean Mist",
            "Aged Bronze",
            "Aged Pewter",
            "Aged Walnut",
            "Almond",
            "Almond Galaxy",
            "Alpine White",
            "Aluminum",
            "Amber",
            "Americana Brown",
            "Anthracite",
            "Anthracite Mix",
            "Antique Brass",
            "Antique Brass and Polished Chrome",
            "Antique Bronze",
            "Antique Cherry",
            "Antique Copper",
            "Antique Copper Finish",
            "Antique Maple",
            "Antique Nickel",
            "Antique Parchment",
            "Antique Pewter",
            "Antique White",
            "Appliance Pull",
            "Arctic",
            "Arctic Granite",
            "Arctic White",
            "Ash",
            "Aspen Green",
            "Atmosphere",
            "Autumn Cherry",
            "Autumn Leaf",
            "Avocado",
            "Avocado Brown",
            "Axe Handle",
            "Aztec",
            "Azure",
            "Baby's Breath",
            "Balsa",
            "Barley",
            "Bayberry",
            "Beige",
            "Berlato",
            "Bermuda Coral",
            "Bermuda Sand",
            "Bianca",
            "Bianco",
            "Bin Pull",
            "Bisc",
            "Biscotti",
            "Biscotti Mix",
            "Biscuit",
            "Biscuit/Linen",
            "Bisque",
            "Black",
            "Black Ash",
            "Black Control",
            "Black Current",
            "Black Galaxy",
            "Black Granite",
            "Black Lima and Mahogany",
            "Black Lima White Gloss",
            "Black Limba White",
            "Black 'N Tan",
            "Black Nickel",
            "Blonde",
            "Blue",
            "Blue and Gold",
            "Blue Copper Storm",
            "Blue Copper Swirl",
            "Blue Mist",
            "Blue Pearl",
            "Blue Shoal",
            "Blue/White",
            "Blush",
            "Bobbing Along Frosted",
            "Bone",
            "Bone/Wheat",
            "Bourbon Cherry",
            "Brass",
            "Brass and White",
            "Brass with Polished Chrome Trim",
            "Bright Chrome",
            "Bright Euro Highlighted",
            "Bright Stainless Steel",
            "Brilliance Brass",
            "Brilliant Satin",
            "Bronze",
            "Bronze Glaze",
            "Bronze Red",
            "Bronze with Topaz Cut Glass",
            "Brookline Pacific White",
            "Brown",
            "Brushed",
            "Brushed Bronze",
            "Brushed Chrome",
            "Brushed Nickel",
            "Brushed Nickel Finish",
            "Brushed Nickel with Chrome Trim",
            "Brushed Nickel with Clear Glass",
            "Brushed Stainless",
            "Brushed Stainless Steel",
            "Brushed Stainless Steel Finish",
            "Brushed-Nickel and Hammered Glass",
            "Brushed-Nickel Finish with Bistro Glass",
            "Brushed-Nickel Finish with Clear Glass",
            "Brushed-Nickel Finish with Rain Glass",
            "Burnt Orange",
            "Cabernet",
            "Cafe Brown",
            "Caf� Brown",
            "Cafe Brown Mix",
            "Candlelight",
            "Canyon",
            "Caraway Seed",
            "Carlisle Toffee Glaze",
            "Carmello",
            "Carrara",
            "Carrara White",
            "Carrera White Marble",
            "Cashmere",
            "Cerulean Blue",
            "Chamois",
            "Champagne",
            "Champagne Bronze",
            "Champagne Gold",
            "Charcoal Gray",
            "Cherry",
            "Cherry Finish",
            "Cherry Wood Finish",
            "Chestnut",
            "Chocolate",
            "Chrome",
            "Chrome and Brushed Nickel",
            "Chrome and Clear Glass",
            "Chrome Finish",
            "Chrome Polished",
            "Chrome Trim Only",
            "Chrome/Foasted",
            "Chrome/Matte Black",
            "Chrome/Polished Brass",
            "Chrome/White",
            "Cinnamon",
            "Classic Cherry",
            "Classic Grey",
            "Classic Mink",
            "Classic Turquoise",
            "Clear",
            "Clear Frosted Glass",
            "Clear Glass",
            "Clear Glass Blue",
            "Clear Glass Charcoal",
            "Clear/Aqua",
            "Cloud Bone",
            "Cloud White",
            "Coated Polished Brass",
            "Cobalt Copper",
            "Cocoa",
            "Cognac",
            "Colonial Blue",
            "Composite",
            "Concord Pacific White",
            "Copper Swirl",
            "Cornflower",
            "Cotton",
            "Cotton White",
            "Country Grey",
            "Crackled Bronze with Silver",
            "Cradle",
            "Crane White",
            "Cream",
            "Creamy White",
            "Creamy Yellow",
            "Crema Marfil",
            "Creme",
            "Crystal",
            "Cylinder",
            "Dark Bamboo",
            "Dark Beige",
            "Dark Bronze",
            "Dark Brown Cherry",
            "Dark Cherry",
            "Dark Chocolate",
            "Dark Emperador",
            "Dark Espresso",
            "Dark Mahogany",
            "Dark Oak",
            "Dark Walnut",
            "Daydream",
            "Deco Bronze",
            "Deco Framed Door",
            "Decobronze",
            "DecoBronze",
            "Deep Bronze",
            "Deerfield Cinnamon",
            "Denim Blue",
            "Depth",
            "Desert Bloom",
            "Desert Gold",
            "Distressed Antique Nickel",
            "Distressed Bronze",
            "Distressed Grey",
            "Distressed Maple",
            "Distressed Parchment",
            "Dresden Blue",
            "Driftwood",
            "Durango",
            "Dusk Gray",
            "Dusty Rose",
            "E and MM",
            "Ebony",
            "Ebony Black Gloss",
            "Edge",
            "Elite Satin",
            "English",
            "English Bronze",
            "Espresso",
            "Espresso Brown",
            "Etched Bin Pull",
            "Euro White",
            "External Bubble Round Hand Level",
            "Fairhaven Manganite",
            "Fawn",
            "Fawn Beige",
            "Flat Black",
            "Flemish",
            "Forever Brass",
            "Forstner Bit",
            "French Gold",
            "Fresh Green",
            "Frost White",
            "Frosted",
            "Frosted Amber",
            "Frosted Clear",
            "Frosted Crystal",
            "Frosted Deep Bronze",
            "Frosted Glass",
            "Frosted Glass Cobalt",
            "Frosted Glass Crystal",
            "Frosted Glass Natural",
            "Frosted Glass Red",
            "Frosted Glass Transparent Natural",
            "Frosted Glass Violet",
            "Frosted Nickel",
            "Frosted White",
            "Frosty",
            "Galala Beige Marble",
            "Geo Mosaic Blue",
            "Glacier",
            "Glacier Blue",
            "Glass and Polished Chrome",
            "Gloss",
            "Gloss Black",
            "Glossy Black",
            "Glossy Grey",
            "Glossy Porcelain",
            "Glossy Red",
            "Glossy White",
            "Gold",
            "Gold Lava",
            "Gold Reflections",
            "Golden Hill",
            "Granite Gray",
            "Granito",
            "Graphite",
            "Gray",
            "Gray Copper Storm",
            "Gray Granite",
            "Gray Marble",
            "Green",
            "Green Gold Storm",
            "Green Pasture",
            "Green Reflections",
            "Green/Clear",
            "Green/White",
            "Grey",
            "Grey/Clear",
            "Hammered Bronze",
            "Hand Rubbed Black",
            "Hand-Rubbed Black",
            "Harrisburg Cinnamon",
            "Harvard Arctic White",
            "Harvest",
            "Harvest Gold",
            "Head System Bracket",
            "Head System O-ring",
            "Head System Wrench",
            "Heather",
            "Heritage Bronze",
            "Heron Blue",
            "High Gloss White",
            "High Polished Chrome",
            "High Quality Chrome Plated Brass",
            "Highlighted Satin",
            "Hightlighted Satin",
            "Honeycomb",
            "Hotel Silver",
            "Ice Gray",
            "Ice Grey",
            "Imperial Brown",
            "Brushed Nickel",
            "Indian Grass",
            "Infinity Brushed Nickel",
            "Infinity Brushed-Nickel",
            "Infinity Super Steel",
            "Innocent Blush",
            "Iron Black",
            "Island Sea",
            "Ivory",
            "Ivory Select",
            "Ivory/White",
            "Jade",
            "Jersey Cream",
            "Kettering Cabernet",
            "Lagoon",
            "Lancing Cabernet",
            "Lavatory Sink In White",
            "Left",
            "Lenox Toffee Glaze",
            "Lifetime Polished Brass",
            "Lifetime Polished Brass Finish",
            "Light Beige",
            "Light Brown",
            "Light Espresso",
            "Light Mahogany",
            "Light Marble",
            "Light Mink",
            "Light Turquoise",
            "Light Wood",
            "Lilac",
            "Lilac Grey",
            "Lime",
            "Lincoln Cinnamon",
            "Linen",
            "Loganberry",
            "Lustrous Chrome",
            "Lustrous Highlighted Satin",
            "Lustrous Steel",
            "Magma",
            "Mahogany",
            "Mahogany Bronze",
            "Manganite",
            "Maple",
            "Marble",
            "Matte Antique Nickel",
            "Matte Black",
            "Matte Brass and Black",
            "Matte Chrome",
            "Matte Silver",
            "Mediterranean Bronze",
            "Medium",
            "Medium Brown",
            "Medium Oak",
            "Medium Walnut",
            "Merlot",
            "Metallic Black",
            "Metallic Bronze",
            "Metallic Brown Copper",
            "Metallic Chocolate",
            "Metallic Copper",
            "Metallic Copper and Blue",
            "Metallic Gold",
            "Metallic Gray",
            "Metallic Gray Copper",
            "Metallic Pewter",
            "Metallic Silver",
            "Metallic Smoke",
            "Mexican Sand",
            "Midnight Black",
            "Midnight Chrome",
            "Ming Green",
            "Mirror",
            "Mist",
            "Mocha",
            "Montero",
            "Montesol",
            "Moon White",
            "Mosaic Blue",
            "Mosaic Silver",
            "Multi",
            "Napoli",
            "Nashville Pacific White",
            "Natural",
            "Natural Oak",
            "Navy",
            "Nero",
            "New Orleans Blue",
            "Nickel",
            "Nickel Finish",
            "Night Sky",
            "Noche Rustico",
            "Noche Rustico Travertine",
            "Noche Travertine",
            "Nutmeg",
            "Oak",
            "Oak Veneer",
            "Obsidian",
            "Obsin",
            "Oil Rub Bronze",
            "Oil Rubbed Brass",
            "Oil Rubbed Bronze",
            "Oil- Rubbed Bronze",
            "Oil RUbbed Bronze",
            "Oil Rubbed Bronze.",
            "Oil-Rubbed Bronze",
            "Old Bronze",
            "Old Copper",
            "Old World Bronze",
            "Onyx",
            "Orange",
            "Orchid",
            "Oval Knob",
            "Overlord Gray",
            "Oxford Blue",
            "Oxide Bronze",
            "Pacific White",
            "Painted Copper",
            "Painted Metallic Silver",
            "Palmer Cabernet",
            "Parchment",
            "Peach Bisque",
            "Peach Blossom",
            "Peach/Coral",
            "Pearl Black",
            "Pearl Nickel",
            "Pebble",
            "Pebble with Pebble Basin",
            "Pecan",
            "Peened Stainless Steel",
            "Petal Pink",
            "Pewter",
            "Pine",
            "Pink",
            "Pink Champagne",
            "Plain White",
            "Polar White",
            "Polishe Nickel",
            "Polished Brass",
            "Polished Brass Finish",
            "Polished Chrome",
            "Polished Chrome and Porcelain",
            "Polished Chrome and White",
            "Polished Copper",
            "Polished Nickel",
            "Polished Satin",
            "Polished Stainless",
            "Polished Stainless Steel",
            "Polished Stainless-Steel",
            "Porcelain and Chrome",
            "Porcelain/Chrome",
            "Powder Coat White",
            "Prairie",
            "Pull",
            "Pure and Clear",
            "Purple",
            "PVD Forever Brass",
            "PVD Polished Chrome",
            "Quadro",
            "Radiant Satin",
            "Rage",
            "Rain Forest",
            "Random Brown and Green",
            "Raspberry",
            "Red",
            "Red Bronze",
            "Red Bronze Finish",
            "Red Ebony",
            "Red Mahagony",
            "Red Permanent",
            "Red Spiral",
            "Reddish-Brown",
            "Regency Blue",
            "Rhapsody Blue",
            "Rich Cinnamon",
            "Rich Mahogany",
            "Rochester Manganite",
            "Rough Brass",
            "Rough Chrome",
            "Round",
            "Rubbed Bronze",
            "Rubbed Oil Bronze",
            "Ruby",
            "Rugged Satin",
            "Rust",
            "Rust Finish",
            "Rustic Bronze",
            "Rustic Pewter",
            "Sage",
            "Sahara Beige",
            "Sahara Stone",
            "Sainless Steel",
            "Sand",
            "Sand and Dark Bronze",
            "Sand Granite",
            "Sandbar",
            "Sandstone powder finish",
            "Sandy Blue",
            "Sapphire Blue",
            "Satin",
            "Satin Black",
            "Satin Brass & Black",
            "Satin Brass and Black",
            "Satin Chrome",
            "Satin Gold",
            "Satin Nickel",
            "Satin-Nickel",
            "Sea Green",
            "Sea Mist Green",
            "Seafoam",
            "Shadow",
            "Shell",
            "Sierra",
            "Silver",
            "Silver and Clear Glass",
            "Silver and Hammered Glass",
            "Silver Finish",
            "Silver Metallic",
            "Silver Pearl",
            "Sky Blue",
            "Skylight",
            "Slate",
            "Small Pull",
            "Smooth Bronze",
            "Smooth Finish",
            "Soft Silver",
            "Speckled Brass",
            "Spice Mocha",
            "Spray White",
            "Spring",
            "Spruce Green",
            "Stainless",
            "Stainless Nickel",
            "Stainless Steel",
            "Stainless Steel and Black",
            "Stainless Steel Polished",
            "Stainless-Steel",
            "Starlight Chrome",
            "StarLight Chrome",
            "Starscape Silver",
            "Steel Black",
            "Steel Optik",
            "Steel Optik/Black",
            "Sterling Brushed Nickel",
            "Suez Tan",
            "Sunlight",
            "Sunrise",
            "Super Steel",
            "Supersteel",
            "SuperSteel",
            "Surf Green",
            "Swansea Manganite",
            "Swiss Chocolate",
            "Tahiti Desert",
            "Tahiti Gray",
            "Tahiti Ivory",
            "Tahiti Matrix",
            "Tahiti Sand",
            "Tahiti Terra",
            "Tahiti White",
            "Tan",
            "Teal",
            "Teal Rings",
            "Tender Grey",
            "Terra Cotta",
            "Terrazzo Black",
            "Terrazzo White",
            "Thunder Grey",
            "Tight Spaces",
            "Timberline",
            "Titanium",
            "Tobacco",
            "Toffee Glaze",
            "Towel Bar",
            "Transparent",
            "Transparent Black",
            "Transparent Blue Bits",
            "Transparent Charcoal",
            "Transparent Crystal",
            "Transparent Golden Yellow",
            "Transparent Natural",
            "Tri-View Beveled Mirror",
            "Truffle",
            "Tumbled Bronze",
            "Tumbled Chrome",
            "Turquoise",
            "Tuscan Bronze",
            "Twilight Blue",
            "Undermount Sinks",
            "Un-Plated Finish",
            "Urban White",
            "Valve",
            "Vanilla",
            "Vanilla Cream",
            "Velvet Aged Bronze",
            "Velvet Black",
            "Venetian Bronze",
            "Venetian Bronze Finish",
            "Venetian Pink",
            "Venice Gold",
            "Verde Green",
            "Vibrant Brushed Nickel",
            "Vintage Oak",
            "Violet",
            "vitreous china lavatory Biscuit",
            "vitreous china lavatory white",
            "Walnut",
            "Walnut top",
            "Warm Cinnamon",
            "Warm Walnut",
            "Warm White",
            "Weatherd Copper",
            "Weathered Bronze",
            "Weathered Brown",
            "Weathered Copper",
            "Weathered Dune",
            "Weathered Pine",
            "Weathered White",
            "Wenge Satin",
            "White",
            "White ",
            "White and Chrome",
            "White Carrara Marble",
            "White Control",
            "White Gloss",
            "White Marble",
            "White ",
            "White Porcelain and Chrome",
            "White Porcelain and Polished Chrome",
            "White Swirl",
            "White Washed",
            "White Wood",
            "White/Chrome",
            "White/Clear",
            "White/Silver",
            "Wiggly Fish Frosted",
            "Wild Rose",
            "Winter Wheat",
            "World Map Blue",
            "Woven Stripe White",
            "Yellow",
            "Zebra Veneer"
        }; 

        private string FormatColor(string color, string productName)
        {
            if (!string.IsNullOrWhiteSpace(color)) return color.TitleCase();
            foreach (var validColor in _validColors)
            {
                if (productName.ContainsIgnoreCase(validColor)) return validColor;
            }
            return string.Empty;
        }

        private string FormatName(string productName)
        {
            productName = productName.Replace("-DISCONTINUED", "");

            // remove colors from product name
            foreach (var validColor in _validColors)
            {
                productName = productName.Replace(" in " + validColor, "");
            }
            return productName;
        }

        private bool IsDiscontinued(string productName)
        {
            return productName.Contains("Discontinued");
        }

        private string FormatCategory(string category)
        {
            category = category.Replace("/", ", ");
            return category;
        }
    }
}