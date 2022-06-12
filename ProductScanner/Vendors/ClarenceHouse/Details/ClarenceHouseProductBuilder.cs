using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ClarenceHouse.Details
{
    public class ClarenceHouseProductBuilder : ProductBuilder<ClarenceHouseVendor>
    {
        public ClarenceHouseProductBuilder(IPriceCalculator<ClarenceHouseVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            data.Cost = data[ScanField.Cost].Replace("$", "").ToDecimalSafe();
            var stockedAtMill = data[ScanField.StockCount].ContainsIgnoreCase("Stocked at mill");
            var stock = data[ScanField.StockCount].TakeOnlyFirstIntegerToken();
            var patternName = data[ScanField.PatternName].TitleCase().RomanNumeralCase();
            var colorName = FormatColorName(data[ScanField.ColorName]).TitleCase();
            var colorNumber = FormatColorNumber(data[ScanField.ColorName], data[ScanField.ColorNumber]);
            var width = data[ScanField.Width].Replace("&nbsp;", "").Replace("\"", "").ToDoubleSafe();

            var vendor = new ClarenceHouseVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();

            var stockData = new StockData(stock);
            if (stockedAtMill) stockData.InStock = true;
            var vendorProduct = new FabricProduct(mpn, data.Cost, stockData, vendor);

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var comment = data[ScanField.Note];
            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.Category].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = colorNumber;
            vendorProduct.PublicProperties[ProductPropertyType.Comment] = FormatComment(comment);
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content].ToFormattedFabricContent().TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(comment);
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.OrderInfo] = FormatOrderInfo(comment);
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternNumber];
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = width.ToString().ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.AlternateItemNumber] = data[ScanField.AlternateItemNumber];
            vendorProduct.PublicProperties[ProductPropertyType.WebItemNumber] = data[ScanField.WebItemNumber];

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = IsLimitedAvailability(stock, data[ScanField.Status]).ToString();
            vendorProduct.PrivateProperties[ProductPropertyType.StockedAtMill] = stockedAtMill.ToString();

            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.IsDiscontinued = IsDiscontinued(stock, data[ScanField.Status]);
            vendorProduct.Name = BuildName(mpn, patternName, CleanColorName(colorName), colorNumber);
            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, data[ScanField.PatternNumber]);
            vendorProduct.SetProductGroup(GetProductGroup(data[ScanField.ProductGroup], width));
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = GetUnitOfMeasure(data[ScanField.UnitOfMeasure], vendorProduct.ProductGroup);

            var increment = GetOrderIncrement(vendorProduct.UnitOfMeasure, vendorProduct.PublicProperties[ProductPropertyType.OrderInfo]);
            vendorProduct.MinimumQuantity = GetMinimumQuantity(data.Cost, vendorProduct.UnitOfMeasure, vendorProduct.ProductGroup);
            vendorProduct.OrderIncrement = increment;

            // everything is shown/sold as double rolls
            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(increment);

                var length = GetLength(comment);
                var dimensions = new RollDimensions(width, length * 3 * 12 * increment);
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Length] = length == 0 ? "" : length * increment + " yards";

                vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();

                vendorProduct.NormalizeWallpaperPricing(increment);

                vendorProduct.MinimumQuantity = 1;
                vendorProduct.OrderIncrement = 1;

                if (increment == 1) vendorProduct.MinimumQuantity = 2;
            }

            return vendorProduct;
        }

        public ProductGroup GetProductGroup(string productGroup, double width)
        {
            if (productGroup.Contains("FABRIC")) return ProductGroup.Fabric;
            if (productGroup.Contains("WALLPAPER")) return ProductGroup.Wallcovering;
            if (productGroup.Contains("TRIM")) return ProductGroup.Trim;

            if (width <= 12)
                return ProductGroup.Trim;
            if (width >= 40)
                return ProductGroup.Fabric;
            return ProductGroup.Wallcovering;
        }

        private bool IsLimitedAvailability(int stock, string status)
        {
            return stock != 0 && status.ContainsIgnoreCase("discontinued");
        }

        private bool IsDiscontinued(int stock, string status)
        {
            return stock == 0 && status.ContainsIgnoreCase("discontinued");
        }

        private int GetMinimumQuantity(decimal cost, UnitOfMeasure unit, ProductGroup productGroup)
        {
            // for fabric/trim product groups, if our cost is under $150 for a product sold by the yard, then min qty is 2 yards.
            if (cost < 150 && unit == UnitOfMeasure.Yard && productGroup == ProductGroup.Fabric)
            {
                return 2;
            }

            // anything by roll is minQty = 2
            if (unit == UnitOfMeasure.Roll) return 2;
            return 1;
        }

        private int GetOrderIncrement(UnitOfMeasure unit, string orderInfo)
        {
            if (orderInfo.ContainsIgnoreCase("Euro Roll")) return 1;
            if (orderInfo.ContainsIgnoreCase("Packaged in Doubles")) return 2;
            return unit == UnitOfMeasure.Roll ? 2 : 1;
        }

        private int GetLength(string orderInfo)
        {
            var length = 5;
            if (orderInfo.ContainsIgnoreCase("Euro Roll")) length = 11;
            if (orderInfo.ContainsIgnoreCase("4 Yd Roll")) length = 4;
            return length;
        }

        private UnitOfMeasure GetUnitOfMeasure(string unit, ProductGroup productGroup)
        {
            if (unit.IsValidUnitOfMeasure())
                return (UnitOfMeasure)Enum.Parse(typeof(UnitOfMeasure), unit, true);

            // if not explicitly set, then wallpaper is roll and everything else is yard
            return productGroup == ProductGroup.Wallcovering ? UnitOfMeasure.Roll : UnitOfMeasure.Yard;
        }

        private string BuildName(string mpn, string patternName, string colorName, string colorNumber)
        {
            // this is a combined pattern name and color extracted from the image url.
            // when present, we'll use this to exactly match their naming - although in 
            // reality it likely ends up to be the same.
            var nameParts = new List<string>();
            nameParts.Add(mpn);
            nameParts.Add(patternName);
            if (colorName != string.Empty)
                nameParts.Add(colorName);
            else if (colorNumber != string.Empty)
                nameParts.Add(colorNumber);
            nameParts.Add("by");
            nameParts.Add(new ClarenceHouseVendor().DisplayName);
            return nameParts.ToArray().BuildName();
        }

        /// A version of color name without some of the special codes they tend to use.
        private string CleanColorName(string colorName)
        {
            var capture = colorName.CaptureWithinMatchedPattern(@"^(?<capture>(.+))/\w+\d+");
            return capture ?? colorName;
        }

        private string FormatColorNumber(string colorName, string colorNumber)
        {
            if (!string.IsNullOrWhiteSpace(colorNumber)) return colorNumber;
            if (colorName.IsInteger()) return colorNumber;
            return string.Empty;
        }

        private static readonly string[] DurabilityWords = { "WYZENBEEK", "EXCEEDS ", " RUBS ", "UFAC1", "MARTINDALE ", " CYCLES ", " D/R ", "PASSES ", "WEARABILITY", "BULLETIN 117", "LIGHTFASTNESS", "COLORFASTNESS ", "TESTING " };
        private static readonly string[] OrderInfoWords = { "SOLD BY", "DOUBLE WIDTH", "COUBLE WIDTH", "PRICED "};
        private static readonly string[] CommentWords = { "HALF DROP", "PATTERN WIDTH", "IRREG", " INHERENT ", "ROLL SIZES", "SLUBS INHERENT", "Coord. fabric"};
        private string FormatComment(string comment)
        {
            foreach (var word in CommentWords)
            {
                if (!comment.ContainsIgnoreCase(word)) continue;

                comment = comment.TitleCase();
                if (!comment.ContainsIgnoreCase("Pattern Width")) continue;

                comment = comment.Replace("Mm", "mm").Replace("MM", "mm").Replace("Cm", "cm").Replace("CM", "cm");
                return comment;
            }
            return null;
        }
        private string FormatOrderInfo(string comment)
        {
            return OrderInfoWords.Any(comment.ContainsIgnoreCase) ? comment.TitleCase() : null;
        }
        private string FormatDurability(string comment)
        {
            var durability = DurabilityWords.Any(comment.ContainsIgnoreCase) ? comment.TitleCase() : null;
            if (durability == null) return string.Empty;

            durability = durability.Replace("Testing: ", "");
            durability = durability.Replace("Abrasion: ", "");
            return durability;
        }
        private string FormatColorName(string value)
        {
            value = value.Replace("*", "").Replace(" W/", " w/");

            // remove anything in paren
            var capture1 = value.CaptureWithinMatchedPattern(@"(?<capture>(\(.+\)))");
            if (capture1 != null)
                value = value.Replace(capture1, "");

            // add space when we see 9999AAAAA, like 4001Tieback
            var capture2 = value.CaptureWithinMatchedPattern(@"^(?<capture>(\d{4,4})).+");
            if (capture2 != null)
                value = value.Replace(capture2, capture2 + " ");

            if (value.IsInteger()) return null;

            value = value.Replace("MAR0ON", "Maroon");
            value = value.ReplaceWholeWord("Ros.", "Rosette");

            value = value.Replace("LMSTK14", "");
            value = value.Replace("LMSTK15", "");
            value = value.Replace("LMSTK16", "");
            value = value.Replace("LMSTK17", "");
            return value; 
        }
    }
}