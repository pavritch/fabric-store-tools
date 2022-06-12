using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Emissary
{
    public class EmissaryProductBuilder : ProductBuilder<EmissaryVendor>
    {
        public EmissaryProductBuilder(IPriceCalculator<EmissaryVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new EmissaryVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true), mpn, BuildFeatures(data));

            homewareProduct.PublicProperties[ProductPropertyType.Description] = "";
            homewareProduct.PrivateProperties[ProductPropertyType.ShippingMethod] = data[ScanField.ShippingMethod];

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.ProductName]);
            homewareProduct.Name = FormatName(data[ScanField.ProductName]);

            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _firstPass = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Table_Lamps, name => name.Contains("Fashioned into a Lamp") || name.Contains("Fashioned into a Lantern")},
        };

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _secondPass = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Tables, name => name.Contains("Pedestal")},
            {HomewareCategory.Bottles, name => name.Contains("Bottle")},
            {HomewareCategory.Bowls, name => name.Contains("Bowl")},
            {HomewareCategory.Candleholders, name => name.Contains("Candle Holder")},
            {HomewareCategory.Floor_Lamps, name => name.Contains("Stool Lamp")},
            {HomewareCategory.Garden_Decorations, name => name.Contains("Pot")},
            {HomewareCategory.Garden_Planters, name => name.ContainsAnyOf("Planter", "Urn")},
            {HomewareCategory.Garden_Stools, name => name.ContainsAnyOf("Garden Stool", "Garden Seat", "Stool/Table")},
            {HomewareCategory.Jars, name => name.Contains("Jar")},
            {HomewareCategory.Pendants, name => name.Contains("Pendant")},
            {HomewareCategory.Side_Tables, name => name.ContainsAnyOf("Side Table", "End Table")},
            {HomewareCategory.Statues_and_Sculptures, name => name.Contains("Statue")},
            {HomewareCategory.Table_Lamps, name => name.Contains("Ornament Lamp")},
            {HomewareCategory.Vases, name => name.ContainsAnyOf("Vase", "Urn")},
        };

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _thirdPass = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Tables, name => name.Contains("Table")},
            {HomewareCategory.Garden_Stools, name => name.Contains("Stool")},
            {HomewareCategory.Table_Lamps, name => name.Contains("Lamp")},
        };

        private HomewareCategory AssignCategory(string name)
        {
            foreach (var category in _firstPass)
                if (category.Value(name))
                    return category.Key;

            foreach (var category in _secondPass)
                if (category.Value(name))
                    return category.Key;

            foreach (var category in _thirdPass)
                if (category.Value(name))
                    return category.Key;
            return HomewareCategory.Garden_Decorations;
        }

        private string FormatName(string name)
        {
            if (name.ContainsIgnoreCase("Bottle-neck Jar with Bright Orange Glaze (Small)")) return "Bottle-neck Jar with Bright Orange Glaze (Small)";
            name = name.Split(',').First().TitleCase();
            name = name.Replace("<br/>", "");
            name = name.Replace("\\", "");
            name = name.Replace("Each are hand glazed resulting in unique finish.", "");
            name = name.ReplaceWholeWord("Sm", "Small");
            name = name.ReplaceWholeWord("Lg", "Large");
            name = name.ReplaceWholeWord("Md", "Medium");
            name = name.ReplaceWholeWord("Yllw", "Yellow");
            name = name.ReplaceWholeWord("Tbl", "Table");
            name = name.ReplaceWholeWord("Sq", "Square");
            name = name.ReplaceWholeWord("Sqr", "Square");
            name = name.ReplaceWholeWord("Plntrs", "Planters");
            name = name.Trim('.');
            return name;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var dims = data[ScanField.Dimensions].Split('x').ToList();
            var features = new HomewareProductFeatures();
            features.Features = BuildSpecs(data);
            features.Color = ParseColor(data[ScanField.ProductName], data[ScanField.Description]);

            foreach (var dim in dims)
            {
                if (dim.Contains("H")) features.Height = dim.Replace("\" H", "").ToIntegerSafe();
                if (dim.Contains("Dia")) features.Features.Add("Diameter", dim.Replace("\" Dia", "").ToInchesMeasurement());
                if (dim.Contains("L")) features.Width = dim.Replace("\" L", "").ToIntegerSafe();
                if (dim.Contains("D")) features.Depth = dim.Replace("\" D", "").ToIntegerSafe();
            }
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.Finish, data[ScanField.Finish]}
            };
            return ToSpecs(specs);
        }

        // copied from http://www.emissaryusa.com/products.html left side
        private readonly List<string> _colors = new List<string>
        {
            "AMAZON GREEN", 
            "AMBER", 
            "ANTIQUE COPPER", 
            "ANTIQUE GREEN", 
            "ANTIQUE WHITE", 
            "APPLE GREEN/BLACK", 
            "APPLE GREEN/GOLD", 
            "AQUA", 
            "AQUA MARINE", 
            "AQUA/CHARCOAL", 
            "BAY GREEN", 
            "BAYSIDE GREEN", 
            "BLACK", 
            "BLACK/BLACK", 
            "BLACK/BLUE", 
            "BLACK/BRIGHT ORANGE", 
            "BLACK/BROWN", 
            "BLACK/CREAM", 
            "BLACK/GOLD", 
            "BLACK/WHITE", 
            "BLACK/WHITE/GOLD", 
            "BLUE", 
            "BLUE BAYOU", 
            "BLUE CASCADE", 
            "BLUE VIOLET", 
            "BLUE/GOLD", 
            "BLUE/WHITE", 
            "BONE WHITE", 
            "BRIGHT ORANGE", 
            "BRONZE", 
            "BROWN", 
            "BROWN GOLD", 
            "BROWN/COPPER", 
            "BUFF", 
            "BURGANDY", 
            "BURNT ORANGE", 
            "CANYON", 
            "CAPER GRAY", 
            "CAPPUCCINO", 
            "CASCADE BLUE/BROWN", 
            "CELADON", 
            "CELADON CRACKLE", 
            "CELADON ICE", 
            "CELERY GREEN", 
            "CHAMPAGNE", 
            "CHARTREUSE", 
            "CINNABAR", 
            "CITRON", 
            "COASTAL SPLASH", 
            "COPPER", 
            "COPPER GREEN", 
            "CORAL", 
            "CORAL RED", 
            "CORAL/BLACK", 
            "CORAL/GOLD", 
            "COSMOS", 
            "CRACKLE", 
            "CREAM", 
            "CREAM/JAVA", 
            "CRYSTAL BRONZE", 
            "CRYSTAL OYSTER", 
            "CRYSTAL TAUPE", 
            "DARK BLUE", 
            "DARK METALLIC", 
            "DARK REEF", 
            "DARK TURQUOISE", 
            "DEEP GREEN", 
            "DEEP TURQUOISE", 
            "DESERT", 
            "DESIGN ENGRAVE", 
            "DIJON", 
            "DISTRESS PURPLE", 
            "DISTRESSED BEIGE", 
            "DISTRESSED COPPER", 
            "DISTRESSED CRACKLE", 
            "DISTRESSED GREEN", 
            "DISTRESSED IVORY", 
            "DISTRESSED TURQUOISE", 
            "DISTRESSED WHITE", 
            "DUO AVOCADO", 
            "EARTH", 
            "EARTH BROWN", 
            "EGGPLANT", 
            "EMERALD GREEN", 
            "EMPEROR BLUE", 
            "FERN GREEN", 
            "FLAMBE RED", 
            "FOAM BLUE", 
            "FRENCH TURQUOISE", 
            "GLASS/BLACK", 
            "GLASS/GOLD", 
            "GOLD", 
            "GOLD/BLACK", 
            "GOLD/BLUE", 
            "GOLD/BRIGHT ORANGE", 
            "GOLD/GOLD", 
            "GOLD/WHITE", 
            "GRAY", 
            "GREEN", 
            "GREEN APPLE", 
            "GREEN CELADON ENGRAVE", 
            "GREEN KELP", 
            "GREEN OLIVE", 
            "GREEN/ORANGE", 
            "GUN METAL", 
            "GUNMETAL", 
            "GUNMETAL/BLACK", 
            "GUNMETAL/GOLD", 
            "HI-GLOSS BLACK", 
            "HONEY SPLASH", 
            "JADE CRACKLE", 
            "JADE FUSION", 
            "JADE SWIRL", 
            "JADE WALNUT", 
            "JAVA", 
            "JEWEL GREEN", 
            "KELLY GREEN", 
            "KHAKI", 
            "LAGOON", 
            "LAGOON SPECKLE", 
            "LAKE TEAL", 
            "LATTE/CHARCOAL", 
            "LEMON", 
            "LEMON GREEN", 
            "LIGHT GRAY", 
            "LIGHT TEAL", 
            "LIGHT TURQUOISE", 
            "LIME", 
            "LUSH TEAL", 
            "MAHOGANY", 
            "MARSALA", 
            "MARSALA/WHITE", 
            "MAUVE", 
            "MEADOW GREEN", 
            "MELON GREEN", 
            "MERLOT", 
            "METAL PAN", 
            "METALLIC", 
            "METALLIC BROWN", 
            "METALLIC MINT", 
            "MIDNIGHT STAR", 
            "MINT CRYSTAL", 
            "MISTY BLUE", 
            "MIXED", 
            "MOCHA", 
            "MOSS", 
            "MOSS CRYSTAL", 
            "MOSS GREEN", 
            "MOSS STREAK", 
            "MULTI-COLOR", 
            "MUSTARD YELLOW", 
            "NATURAL", 
            "NUTSHELL BROWN", 
            "PALE ROSE", 
            "PALMETTO GREEN", 
            "PAPRIKA", 
            "PEACOCK GREEN", 
            "PEARL AZURE", 
            "PERSIMMON", 
            "PINE GREEN", 
            "PLUM", 
            "QUIN BLUE", 
            "RED", 
            "RED SPECKLE", 
            "RED/BLACK", 
            "RED/WHITE", 
            "REEF", 
            "ROCK MOSS", 
            "ROYAL BLUE/BLACK", 
            "ROYAL BLUE/GOLD", 
            "ROYAL YELLOW", 
            "RUSSET", 
            "RUST", 
            "RUSTIC", 
            "SAFARI EARTH", 
            "SAGE/BLACK", 
            "SAGE/GOLD", 
            "SALMON", 
            "SAND", 
            "SAND DISTRESSED", 
            "SAND DUNE", 
            "SAPPHIRE BLUE", 
            "SEA SHORE", 
            "SEAWEED", 
            "SIERRA GREEN", 
            "SILVER", 
            "SMOKE", 
            "SMOKE GRAY", 
            "SMOKEY MATTE", 
            "SPA BLUE", 
            "SPRUCE", 
            "STONE GRAY", 
            "STORM BLUE", 
            "STORM GRAY", 
            "SUN YELLOW", 
            "TANGERINE", 
            "TAUPE", 
            "TEA GREEN", 
            "TEAL", 
            "TROPICAL SAND", 
            "TURQUOISE", 
            "TURQUOISE/LIME", 
            "VARIEGATED", 
            "VINTAGE ROSE", 
            "VIOLET FUSION", 
            "WASABI", 
            "WHITE", 
            "WHITE CELADON", 
            "WHITE CRACKLE", 
            "WHITE SPECKLE", 
            "WHITE/BLACK", 
            "WHITE/BLACK", 
            "WHITE/GOLD", 
            "WHITE/GOLD", 
            "WILLOW GREEN", 
            "YELLOW", 
            "YELLOW CRACKLE", 
            "YELLOW GREEN", 
            "YELLOW TWO-TONE", 
            "YELLOW/GREEN", 
            "YELLOW/WHITE", 
        }; 

        private string ParseColor(string name, string description)
        {
            // sort by length so we don't choose 'green' when 'willow green' is the color
            var sortedColors = _colors.OrderByDescending(x => x.Length);
            foreach (var color in sortedColors)
            {
                if (name.ContainsIgnoreCase(color) || description.ContainsIgnoreCase(color))
                    return color.TitleCase();
            }
            return string.Empty;
        }
    }
}