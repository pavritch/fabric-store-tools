using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace SilverState.Details
{
    public class SilverStateProductBuilder : ProductBuilder<SilverStateVendor>
    {
        public SilverStateProductBuilder(IPriceCalculator<SilverStateVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var patternName = FormatPatternName(data[ScanField.PatternName]);
            var colorName = Regex.Replace(data[ScanField.Color], @"[0-9]", "").Trim();
            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendor = new SilverStateVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(data[ScanField.Status].ContainsIgnoreCase("Available")), vendor);

            // we're setting all 'Clearance' products to not have swatches
            if (data.IsClearance)
            {
                vendorProduct.HasSwatch = false;
                vendorProduct.MinimumQuantity = 5;
            }

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.Brand] = data[ScanField.Brand].Replace("™", "").Replace("SS Polyurethaneâ„¢", "SS Polyurethane").UnEscape();
            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.ProductUse].Trim(' ', ',');
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = data[ScanField.Cleaning].Replace("dry", "Dry").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection].UnEscape().Replace(" Collection", "");
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content]);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = FormatFinish(data[ScanField.Finish]);
            vendorProduct.PublicProperties[ProductPropertyType.FireCode] = data[ScanField.FireCode];
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = FormatHorizontalRepeat(data[ScanField.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternName].Split(new[] { ' ' }).Last().TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.Use].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = data[ScanField.Railroaded].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatVerticalRepeat(data[ScanField.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = FormatWidth(data[ScanField.Width]);

            vendorProduct.PublicProperties[ProductPropertyType.ManufacturerPartNumber] = mpn;
            vendorProduct.PublicProperties[ProductPropertyType.ItemNumber] = mpn;

            vendorProduct.PrivateProperties[ProductPropertyType.Coordinates] = data[ScanField.Coordinates];
            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.RemoveWhen(ProductPropertyType.Cleaning, s => s.Contains("See"));
            vendorProduct.RemoveWhen(ProductPropertyType.PatternNumber, s => !s.ContainsDigit());

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName);
            vendorProduct.IsClearance = data.IsClearance;
            vendorProduct.Name = new[] { patternName, data[ScanField.Color], "by", vendor.DisplayName }.BuildName();
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SetProductGroup(data[ScanField.ProductGroup].ToProductGroup());
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();
            return vendorProduct;
        }

        private string FormatHorizontalRepeat(string repeat)
        {
            repeat = repeat.Replace("&quot;", "\"");
            repeat = repeat.Replace("\\", "");

            // should be in the form: H 2.5", V 14"
            var horizontalRepeat = repeat.CaptureWithinMatchedPattern("H (?<capture>(.*))\",");
            return horizontalRepeat.ToInchesMeasurement();
        }

        private string FormatVerticalRepeat(string repeat)
        {
            repeat = repeat.Replace("&quot;", "\"");
            repeat = repeat.Replace("\\", "");

            // should be in the form: H 2.5", V 14"
            var verticalRepeat = repeat.CaptureWithinMatchedPattern(", V(?<capture>(.*))\"");
            return verticalRepeat.ToInchesMeasurement();
        }

        private string FormatWidth(string width)
        {
            width = width.Replace("\\\"", "").UnEscape();
            width = width.Replace("Width", "");
            width = width.Replace("Roll Size 30 yards", "").Trim();

            return width.ToInchesMeasurement();
        }

        private string FormatPatternName(string patternName)
        {
            if (!patternName.ContainsDigit()) return patternName.TitleCase();

            var numberAndName = patternName.Split(new[] { ' ' });
            return numberAndName.Last().TitleCase();
        }

        private string FormatContent(string content)
        {
            if (!content.ContainsDigit()) return null;

            content = content.Replace("Content: ", "");
            content = content.Replace("&reg", "");
            content = content.Replace("SunbrellaAcrylic", "Sunbrella Acrylic");

            var firstDigit = content.IndexOfAny("0123456789".ToCharArray());
            content = content.Substring(firstDigit);
            content = content.RemovePattern("Backing:.*");

            return content.ToFormattedFabricContent().TitleCase();
        }

        private string FormatFinish(string finish)
        {
            finish = finish.UnEscape();

            if (finish == "Soil and StainResistant") return "Soil and Stain Resistant";

            var found = _finishes.FirstOrDefault(x => finish.ContainsIgnoreCase(x));
            if (found != null)
                return found;

            return finish.TitleCase();
        }

        private readonly List<string> _finishes = new List<string>
        {
            "Soil and Stain Resistant",
            "Sulfide Stain Resistant",
            "Eco Wash",
            "Produratect Mildew Resistant",
            "Napa Finish",
            "Teflon/F1",
            "Soil and Stain Repellant",
            "Permablok3",
            "Permablok 3",
            "Permaguard",
            "Produratect",
            "Resilience",
            "Teflon",
            "Nanotex Permanent Soil and Stain Protection",
            "Backing/Biancalani",
            "Stain Repellent",
            "Teflon W/ Backing",
            "Bleach Resistant",
            "Teflin/BA1",
            "Airo",
            "BSA Backing",
            "Acrylic Backing",
            "Water and Stain Repellent",
            "UV Resistant"
        };
    }
}