using System;
using System.Linq;
using System.Text.RegularExpressions;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace FSchumacher.Details
{
    public class FSchumacherProductBuilder : ProductBuilder<FSchumacherVendor>
    {
        private readonly IDesignerFileLoader _designerFileLoader;

        public FSchumacherProductBuilder(IPriceCalculator<FSchumacherVendor> priceCalculator, IDesignerFileLoader designerFileLoader) : base(priceCalculator)
        {
            _designerFileLoader = designerFileLoader;
        }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var cost = FormatPrice(data[ScanField.Cost]);
            //data.Cost = cost * .9m;
            data.Cost = cost;

            var stock = data[ScanField.StockCount];
            var patternName = FormatPatternName(data[ScanField.ProductName]);
            var colorName = FormatColorName(data[ScanField.ColorName]);

            var vendor = new FSchumacherVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, new StockData(stock.ToDoubleSafe()), vendor);
            var width = FormatRepeat(data[ScanField.Width]);
            var vertRepeat = FormatRepeat(data[ScanField.VerticalRepeat]);
            var horizRepeat = FormatRepeat(data[ScanField.HorizontalRepeat]);
            var collection = FormatCollection(data[ScanField.Collection]);

            if (data.Cost > 3000) vendorProduct.HasSwatch = false;

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.Collection] = collection;
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content].TitleCase().ToFormattedFabricContent();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfFinish] = new Country(data[ScanField.Country].TitleCase()).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Designer] = GetDesigner(collection);
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(data[ScanField.Durability]);
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = FormatRepeat(data[ScanField.HorizontalRepeat]);
            vendorProduct.PublicProperties[ProductPropertyType.ItemNumber] = mpn;
            vendorProduct.PublicProperties[ProductPropertyType.Match] = FormatMatch(data[ScanField.Match]);
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatRepeat(data[ScanField.VerticalRepeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = width;


            var length = GetLength(data[ScanField.Length]);
            var increment = GetOrderIncrement(data[ScanField.OrderInfo]);
            var widthValue = width.Replace("\"", "")
                .Replace(" 1/2", ".5")
                .Replace(" 1/4", ".25")
                .Replace(" 3/8", ".375")
                .Replace(" 1/8", ".125")
                .Replace(" 6/8", ".75")
                .Replace(" 5/8", ".625")
                .Replace(" 2/8", ".25")
                .Replace(" 3/4", ".75")
                .Replace(" 7/8", ".875")
                .ToDoubleSafe();
            var dimensions = new RollDimensions(widthValue, length * 12 * 3 * increment);

            var group = data[ScanField.ProductGroup].ToProductGroup();
            var unit = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();

            if (group == ProductGroup.Wallcovering)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthInYardsFormatted();
                vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();
            }

            //vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = data[ProductPropertyType.IsLimitedAvailability];
            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();


            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName.StripNonAlphaSpaceChararcters()).ToUpper();
            vendorProduct.ManufacturerDescription = data[ScanField.Description];
            vendorProduct.MinimumQuantity = GetMinimumQuantity(data[ScanField.OrderInfo], data[ScanField.Bullets], data.MadeToOrder, group, unit);

            vendorProduct.Name = new[] {mpn, patternName, colorName, "by", vendor.DisplayName}.BuildName();
            vendorProduct.OrderIncrement = increment;
            vendorProduct.OrderRequirementsNotice = FindOrderRequirements(data[ScanField.Bullets], GetRepeatInYards(vertRepeat, horizRepeat), data.MadeToOrder);
            vendorProduct.SetProductGroup(group);
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SETitle = group == ProductGroup.Wallcovering ? vendorProduct.Name + " Wallpaper" : vendorProduct.Name + " Fabric";
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = unit;

            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(increment);
                vendorProduct.NormalizeWallpaperPricing(increment);

                vendorProduct.OrderIncrement = 1;
                vendorProduct.MinimumQuantity = 1;
            }

            if (data[ScanField.MinimumOrder] == "8")
                vendorProduct.MinimumQuantity = 8;
            if (data[ScanField.MinimumOrder] == "30")
                vendorProduct.MinimumQuantity = 30;

            if (data[ScanField.OrderIncrement] == "8")
                vendorProduct.OrderIncrement = 8;

            return vendorProduct;
        }

        private string GetDesigner(string collection)
        {
            var validDesigners = _designerFileLoader.GetDesigners();
            var match = validDesigners.FirstOrDefault(x => collection.ToLower().Contains(x.ToLower()));
            return match ?? string.Empty;
        }

        private string FindOrderRequirements(string tempContent1, double repeatInYards, bool madeToOrder)
        {
            if (madeToOrder)
            {
                return "Made to order. Allow 4-6 weeks for delivery";
            }
            if (tempContent1 == "REPEAT")
            {
                return string.Format("Sold only by the {0} yard repeat. Please contact us to order this product.", repeatInYards);
            }
            return string.Empty;
        }

        private double GetLength(string length)
        {
            double res;
            if (Double.TryParse(length, out res))
                return res;
            return 0;
        }

        private string FormatMatch(string match)
        {
            if (match == "N/A") return string.Empty;
            return match.TitleCase();
        }

        private string FormatRepeat(string repeat)
        {
            if (repeat == "NONE") return string.Empty;
            if (repeat == "N/A") return string.Empty;
            if (repeat == "LEGACY UDC ADD") return string.Empty;
            if (repeat == "ADD FOR CONVERSION") return string.Empty;

            repeat = repeat.Replace("&quot;", "\"");
            repeat = repeat.Replace(" WIDE", "");
            if (repeat.IndexOf("(") != -1)
                repeat = repeat.Substring(0, repeat.IndexOf("("));
            return repeat;
        }

        private string FormatColorName(string colorName)
        {
            colorName = colorName.Replace("&#39;", "'");
            colorName = colorName.Replace("&#200;", "e");
            colorName = colorName.ToFormattedColors().TitleCase();

            if (colorName == null) return string.Empty;
            colorName = colorName.Replace("Color: ", "");
            colorName = colorName.Replace("/", ", ");
            return colorName;
        }

        private string FormatPatternName(string patternName)
        {
            patternName = patternName.Replace("&#39;", "'");
            patternName = patternName.TitleCase().RomanNumeralCase();
            patternName = patternName.Replace("Frg", "Fringe");
            return patternName;
        }

        private string FormatCollection(string collection)
        {
            collection = collection.Replace(" COLLECTION", "");
            collection = collection.Replace("COLLECTION NAME TBA - ", "");
            collection = collection.Replace("D&#39;AZUR", "D'Azure");
            collection = collection.Replace("&amp;", "&");
            return collection.TitleCase().RomanNumeralCase().Replace("Mcdonald", "McDonald");
        }

        private string FormatDurability(string durability)
        {
            if (durability == "N/A") return string.Empty;
            return durability.TitleCase();
        }

        private double GetRepeatInYards(string verticalRepeat, string horizontalRepeat)
        {
            if (verticalRepeat != null)
            {
                var repeatInYards = ExtensionMethods.MeasurementFromFraction(verticalRepeat).ToDoubleSafe();
                return Math.Round(repeatInYards/36.0, 2);
            }
            if (horizontalRepeat != null)
            {
                var repeatInYards = ExtensionMethods.MeasurementFromFraction(horizontalRepeat).ToDoubleSafe();
                return Math.Round(repeatInYards/36.0, 2);
            }
            return 0;
        }

        private decimal FormatPrice(string price)
        {
            price = price.Replace("US $", "").Replace("&nbsp;", "");
            return price.ToDecimalSafe();
        }

        private int GetOrderIncrement(string orderInfo)
        {
            if (orderInfo == "3") return 3;
            if (orderInfo == "2") return 2;
            return 1;

            /*
            var inc = orderInfo.CaptureWithinMatchedPattern(@"yards in (?<capture>(.*)) yard increments.");
            if (inc.IsDouble()) return (int)Double.Parse(inc);

            if (soldBy == "DOUBLE ROLL") return 2;
            if (soldBy == "TRIPLE ROLL") return 3;

            return 1;
            */
        }

        private int GetMinimumQuantity(string orderInfo, string soldBy, bool madeToOrder, ProductGroup productGroup, UnitOfMeasure unit)
        {
            if (madeToOrder) return 16;

            // Minimum Order: 8 yards
            // Minimum Order: 16 yards
            var min = orderInfo.CaptureWithinMatchedPattern(@"This product has a minimum order of (?<capture>(.*))");
            if (min != null) min = min.Trim();
            if (min.IsDouble()) return (int)Convert.ToDouble(min);

            if (soldBy == "DOUBLE ROLL") return 2;
            if (soldBy == "TRIPLE ROLL") return 3;

            if (productGroup == ProductGroup.Fabric) return 2;
            if (productGroup == ProductGroup.Wallcovering && unit == UnitOfMeasure.Yard) return 2;

            return 1;
        }

        private UnitOfMeasure GetUnitOfMeasure(string unit)
        {
            if (unit == "YARD") return UnitOfMeasure.Yard;
            if (unit == "SINGLE ROLL") return UnitOfMeasure.Roll;
            if (unit == "PANEL") return UnitOfMeasure.Panel;
            if (unit == "N/A") return UnitOfMeasure.Yard;
            return UnitOfMeasure.Each;
        }
    }
}