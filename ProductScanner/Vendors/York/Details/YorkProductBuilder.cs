using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace York.Details
{
    public class YorkProductBuilder : ProductBuilder<YorkVendor>
    {
        private readonly IDesignerFileLoader _designerFileLoader;

        public YorkProductBuilder(IPriceCalculator<YorkVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator)
        {
            _designerFileLoader = designerFileLoader;
        }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            //var cost = Math.Round(GetActualYorkMSRP(data)*.45M, 2);
            var stock = data[ScanField.StockCount];
            var pattern = FormatPatternName(data[ScanField.PatternName]);
            var vendor = new YorkVendor();

            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(stock.Replace(",", "").TakeOnlyFirstIntegerToken() > 0), vendor);
            var borderHeight = FormatBorderHeight(data[ScanField.BorderHeight]);

            var book = data[ScanField.Collection].Split(new []{'-'}, StringSplitOptions.RemoveEmptyEntries).First();
            data[ScanField.Book] = book;

            var collection = FormatCollection(data[ScanField.Collection]);

            vendorProduct.PublicProperties[ProductPropertyType.AdditionalInfo] = FormatAdditionalInfo(data);
            vendorProduct.PublicProperties[ProductPropertyType.Backing] = FormatBacking(data);
            vendorProduct.PublicProperties[ProductPropertyType.Book] = book;
            vendorProduct.PublicProperties[ProductPropertyType.BorderHeight] = borderHeight.ToString().ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.Category].Replace("Borderser", "Borders");
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = FormatCleaning(data);
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = collection;
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = FormatCountry(data[ScanField.Country]);
            vendorProduct.PublicProperties[ProductPropertyType.Design] = FormatDesign(data);
            vendorProduct.PublicProperties[ProductPropertyType.Designer] = GetDesigner(collection);
            vendorProduct.PublicProperties[ProductPropertyType.Length] = FormatLength(data[ScanField.Dimensions]);
            vendorProduct.PublicProperties[ProductPropertyType.Match] = FormatMatch(data[ScanField.Match]);
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = FormatPatternName(data[ScanField.PatternName]);
            vendorProduct.PublicProperties[ProductPropertyType.Prepasted] = FormatPrepasted(data);
            vendorProduct.PublicProperties[ProductPropertyType.Repeat] = FormatRepeat(data[ScanField.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Strippable] = FormatStrippable(data);
            vendorProduct.PublicProperties[ProductPropertyType.UPC] = FormatUPC(data[ScanField.UPC]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = FormatWidth(data[ScanField.Category], data[ScanField.Dimensions], data[ScanField.Width]);

            vendorProduct.PublicProperties[ProductPropertyType.Color] = data[ScanField.Color];
            vendorProduct.PublicProperties[ProductPropertyType.Coordinates] = data[ScanField.Coordinates];
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ScanField.Style];

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var dimensions = GetDimensions(data[ScanField.Dimensions]);
            if (dimensions.GetCoverage() <= 0)
            {
                dimensions = new RollDimensions((double)borderHeight, GetLengthInInches(data[ScanField.Dimensions]));
            }
            vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
            vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();

            vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();
            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.AddPublicProp(ProductPropertyType.Construction, TempContent6Fix(data[ScanField.Bullet6]));
            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn);
            vendorProduct.Correlator = BuildCorrelator(sku, pattern, vendor.SkuPrefix);
            vendorProduct.Name = BuildName(pattern, data, vendor);
            vendorProduct.ProductClass = GetProductClass(data[ScanField.Category]);
            vendorProduct.SetProductGroup(ProductGroup.Wallcovering);
            vendorProduct.ScannedImages = new List<ScannedImage> { new ScannedImage(ImageVariantType.Primary, data[ScanField.ImageUrl])};
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();
            //vendorProduct.Packaging = GetPackaging(data[ScanField.Packaging], vendorProduct.UnitOfMeasure);

            vendorProduct.MinimumQuantity = GetMinimumQuantity(data);
            vendorProduct.OrderIncrement = vendorProduct.UnitOfMeasure == UnitOfMeasure.Each ? 1 : 2;

            // everything is shown/sold as double rolls
            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(2);
                vendorProduct.PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthInYardsFormatted();
                vendorProduct.NormalizeWallpaperPricing(2);

                vendorProduct.OrderIncrement = 1;
                vendorProduct.MinimumQuantity = 1;
            }

            return vendorProduct;
        }

            //if (isEach)
            //    addDetailProperty("MSRP (Each)", ProductPropertyType.RetailPrice, @"(\$){0,1}(?<capture>((.*)))");


        private string GetDesigner(string collection)
        {
            var validDesigners = _designerFileLoader.GetDesigners();
            var match = validDesigners.FirstOrDefault(x => collection.ToLower().Contains(x.ToLower()));
            return match ?? string.Empty;
        }

        private string FormatCollection(string collection)
        {
            var formatted = collection.CaptureWithinMatchedPattern(@"(\d+-){0,1}(?<capture>((.*)))");
            return formatted.TitleCase().RomanNumeralCase();
        }

        private string TempContent6Fix(string input)
        {
            var sb = new StringBuilder(100);
            var lastCharWasLowercase = false;

            foreach (var c in input)
            {
                if (Char.IsUpper(c) && lastCharWasLowercase)
                    sb.Append(" ");

                sb.Append(c);

                lastCharWasLowercase = Char.IsLower(c);
            }

            return sb.ToString();
        }

        private double GetLengthInInches(string dimensions)
        {
            if (dimensions.ContainsIgnoreCase("5 yard spool")) return 5*12*3;
            if (dimensions.ContainsIgnoreCase("3 yard spool")) return 3*12*3;
            return 0;
        }

        private string FormatLength(string dimensions)
        {
            var lengthInYards = dimensions.CaptureWithinMatchedPattern(@"(?<capture>(\d+)) yard spool");
            if (!string.IsNullOrEmpty(lengthInYards)) return lengthInYards + " yards";
            return string.Empty;
        }

        // pull this stuff into core - this could be general enough to use for several vendors
        private readonly List<DimensionRegex> _regexes = new List<DimensionRegex>
        {
            new DimensionRegex(@"(?<width>\d+([.]\d+)?)\s*in.*x\s*(?<length>\d+([.]\d+)?)\s*f", w => w, l => l * 12),
            new DimensionRegex(@"(?<width>\d+([.]\d+)?)\s*in.*X\s*(?<length>\d+([.]\d+)?)\s*f", w => w, l => l * 12),
            new DimensionRegex(@"(?<width>\d+([.]\d+)?)\s*Inches.*X\s*(?<length>\d+([.]\d+)?)\s*Feet", w => w, l => l * 12),
            new DimensionRegex(@"(?<length>\d+([.]\d+)?)\s*f.*x\s*(?<width>\d+([.]\d+)?)\s*in", w => w, l => l * 12),
            new DimensionRegex(@"(?<width>\d+([.]\d+)?)'w x (?<length>\d+([.]\d+)?)'h", w => w * 12, l => l * 12),
            new DimensionRegex(@"(?<length>\d+([.]\d+)?)'h x (?<width>\d+([.]\d+)?)'w", w => w * 12, l => l * 12),
            new DimensionRegex(@"(?<width>\d+([.]\d+)?)""w x (?<length>\d+([.]\d+)?)""h", w => w, l => l),
            new DimensionRegex(@"(?<width>\d+([.]\d+)?)"" x (?<length>\d+([.]\d+)?)""", w => w, l => l),
            new DimensionRegex(@"(?<width>\d+([.]\d+)?)""w x (?<length>\d+([.]\d+)?)'h", w => w, l => l * 12),
        };

        private RollDimensions GetDimensions(string dimensions)
        {
            dimensions = dimensions.Replace(" 1/2", ".5");

            foreach (var regex in _regexes)
            {
                var match = Regex.Match(dimensions, regex.Regex);
                if (match.Success)
                {
                    var width = match.Groups["width"].Value.Trim().ToDoubleSafe();
                    var length = match.Groups["length"].Value.Trim().ToDoubleSafe();
                    return new RollDimensions(regex.WidthFunc(width), regex.LengthFunc(length), 2);
                }
            }
            return new RollDimensions(0, 0);
        }

        private ProductClass GetProductClass(string category)
        {
            if (category == "Murals") return ProductClass.WallMurals;
            if (category == "Borders") return ProductClass.Border;
            if (category == "Applique") return ProductClass.Applique;
            return ProductClass.Wallpaper;
        }

        private int GetMinimumQuantity(ScanData data)
        {
            // some items have embedded requirements in strange fields, so go looking
            if (data[ScanField.Packaging].ContainsIgnoreCase("Minimum Order"))
            {
                // doen't seem to hit on this one
                var qty = data[ScanField.Packaging].CaptureWithinMatchedPattern(@"minimum order:\s*(?<capture>((\d{1,3})))");
                int intQty = 0;
                if (qty != null && int.TryParse(qty, out intQty) && intQty > 1)
                    return intQty;
            }

            if (data[ScanField.Dimensions].ContainsIgnoreCase("Minimum Order"))
            {
                // this one hits
                var qty = data[ScanField.Dimensions].CaptureWithinMatchedPattern(@"minimum order:\s*(?<capture>((\d{1,3})))");
                int intQty = 0;
                if (qty != null && int.TryParse(qty, out intQty) && intQty > 1)
                    return intQty;
            }


            // let default processing prevail for this scenario
            if (data[ScanField.MinimumQuantity] == "1")
                return 1;

            // let default processing prevail for this scenario
            if (data[ScanField.Category] == "Wallpaper" && data[ScanField.UnitOfMeasure] == "Roll" && data[ScanField.MinimumQuantity] == "2")
                return 2;

            return 1;
        }

        private string BuildCorrelator(string sku, string patternName, string skuPrefix)
        {
            if (string.IsNullOrWhiteSpace(patternName))
                return sku;

            return string.Format("{0}-{1}", skuPrefix, patternName.MakeSafeSEName()).ToUpper();
        }

        private string BuildName(string patternName, ScanData data, Vendor vendor)
        {
            var nameParts = new List<string>();
            nameParts.Add(data[ScanField.ManufacturerPartNumber]);
            var temp3 = data[ScanField.Bullet3];

            if (!string.IsNullOrWhiteSpace(temp3))
            {
                if (temp3.IsAllUpperCase())
                    temp3 = temp3.TitleCase();
                nameParts.Add(temp3);
            }
            else
                nameParts.Add(patternName);
            nameParts.Add("by");
            nameParts.Add(vendor.DisplayName);
            return nameParts.ToArray().BuildName();
        }

        private string FormatAdditionalInfo(ScanData data)
        {
            foreach (var s in new string[] {"Removable & Repositionable", "Peelable"})
            {
                if (HasFeatureKeyword(data, s))
                    return s;
            }
            return null;
        }

        private string FormatBacking(ScanData data)
        {
            foreach (var s in new[] {"Peel and Stick", "", "Peel & Stick", "Self-Adhesive", "Self Stick", "Self Applied"})
            {
                if (HasFeatureKeyword(data, s))
                    return s;
            }

            return null;
        }

        private decimal FormatBorderHeight(string value)
        {
            value = value.ToUpper().Replace("\"", "").Replace("IN.", " ").Trim();

            if (value.ContainsIgnoreCase("/"))
                return ExtensionMethods.MeasurementFromFraction(value).TakeOnlyFirstDecimalToken();
            return value.TakeOnlyFirstDecimalToken();
        }

        private string FormatCleaning(ScanData data)
        {
            foreach (var s in new[] {"Washable", "Scrubbable"})
            {
                if (HasFeatureKeyword(data, s))
                    return s;
            }

            return null;
        }

        private string FormatCountry(string country)
        {
            // some bad data is excel - skip when
            // if has a digit of any kind, or word wallpaper or wallcovering, texture, transitional, floral
            foreach (var c in country)
            {
                if (Char.IsDigit(c))
                    return null;
            }

            foreach (var word in CountryStopWords)
            {
                if (country.ContainsIgnoreCase(word))
                    return null;
            }

            return new Country(country).Format();
        }

        private string FormatDesign(ScanData data)
        {
            var value1 = data[ScanField.Bullet1];
            var value2 = data[ScanField.Bullet2];

            if (!string.IsNullOrWhiteSpace(value2))
                value1 += ", " + value2;

            if (string.IsNullOrWhiteSpace(value1))
                return null;

            var rawTokens = value1.Split(new char[] {',', ';', ':'});

            var phrases = new List<string>();

            foreach (var token in rawTokens)
            {
                var trimToken = token.TrimToNull();
                if (trimToken == null)
                    continue;

                // detect stop words

                bool fSkip = false;
                foreach (var re in DesignStopRegEx)
                {
                    if (Regex.IsMatch(trimToken, re, RegexOptions.IgnoreCase))
                    {
                        fSkip = true;
                        break;
                    }
                }

                if (!fSkip)
                {
                    if (trimToken.IsAllUpperCase())
                        phrases.Add(trimToken);

                    phrases.Add(trimToken.TitleCase());
                }

            }

            var value = phrases.Distinct().ToCommaDelimitedList();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }

        private string FormatDimensions(string category, string dimensions, string tempContent4)
        {
            switch (category)
            {
                case "Wallpaper":
                    // the dimensions from the website are best
                    return dimensions.AddSpacesAfterCommas();

                case "Borders":
                case "Murals":
                case "Wall Appliques":
                case "Embellishments":
                    // best is to get from spreadsheet when possible
                    if (!string.IsNullOrWhiteSpace(tempContent4))
                        return tempContent4.Replace("Package Dimensions: ", "").Trim().ToLower().AddSpacesAfterCommas();
                    break;
            }
            return dimensions.AddSpacesAfterCommas();
        }

        private string FormatMatch(string value)
        {
            value = value.Replace(".", "").Trim();
            foreach (var s in MatchValues)
            {
                if (s.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return s;
            }
            return null;
        }

        private string FormatPatternName(string value)
        {
            value = value.Replace("Mickey Mouse - ", "");
            value = value.Replace("Winnie the Pooh- ", "");
            value = value.Replace("Winnie the Pooh - ", "");
            value = value.Replace(" Appliques", "");
            value = value.Replace(" Wall Decals", "");
            value = value.ReplaceWholeWord("Sm", "Small");
            value = value.ReplaceWholeWord("Shlf", "Shelf");

            if (value.IsAllUpperCase())
                return value.TitleCase();

            return value;
        }

        private string FormatPrepasted(ScanData data)
        {
            if (HasFeatureKeyword(data, "Prepasted"))
                return "Yes";

            if (HasFeatureKeyword(data, "Unpasted"))
                return "No";

            return null;
        }

        private string FormatRepeat(string value)
        {
            value = value.ToUpper().Replace("\"", "").Replace("IN.", " ").Trim();

            if (value.ContainsIgnoreCase("None"))
                return "None";

            if (value.ContainsIgnoreCase("Random"))
                return "Random";

            // 5 yard non repeating
            // 9 ft. non repeating
            if (value.ContainsIgnoreCase("repeating"))
                return value.TitleCase();

            if (value.ContainsIgnoreCase("/"))
                return ExtensionMethods.MeasurementFromFraction(value).ToInchesMeasurement();
            return value.ToInchesMeasurement();
        }

        private string FormatStrippable(ScanData data)
        {
            if (HasFeatureKeyword(data, "Strippable"))
                return "Yes";

            return null;
        }

        private string FormatUPC(string upc)
        {
            if (string.IsNullOrWhiteSpace(upc) || upc.Length < 10)
                return null;
            return upc;
        }

        private string FormatWidth(string category, string dimensions, string widthValue)
        {
            // earlier logic now only sets width from file when is wallpaper - using the length from file

            // only width when is wallpaper, the value might not otherwise be accurate
            if (!category.Equals("Wallpaper", StringComparison.OrdinalIgnoreCase))
                return null;

            // use the width (actually Item Length) from spreadsheet when available
            if (!string.IsNullOrWhiteSpace(widthValue))
                return widthValue.ToInchesMeasurement();

            if (dimensions != null)
            {
                var firstNumber = dimensions.CaptureWithinMatchedPattern(@"(?<capture>((.*)))\sx\s\d");
                if (!string.IsNullOrWhiteSpace(firstNumber))
                    widthValue = firstNumber;
            }

            if (string.IsNullOrWhiteSpace(widthValue))
                return null;

            widthValue = widthValue.ToUpper().Replace("\"", "").Replace("IN.", "").Replace("W", "").Trim();

            if (widthValue.ContainsIgnoreCase("/"))
                return ExtensionMethods.MeasurementFromFraction(widthValue).ToInchesMeasurement();
            else
                return widthValue.ToInchesMeasurement();
        }

        private bool HasFeatureKeyword(ScanData data, string name)
        {
            var features = new List<string>()
            {
                data[ScanField.Bullet2],
                data[ScanField.Bullet5]
            };

            foreach (var s in features)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                if (s.ContainsIgnoreCase(name))
                    return true;
            }

            return false;
        }

        private static readonly string[] PackagingStopRegEx =
        {
            @"unpasted",
            @"prepasted",
            @"washable",
            @"scrubbable",
            @"Repositionable",
            @"Removable",
        };

        private static readonly string[] DesignStopRegEx =
        {
            @"perfect coordinate",
            @"wallpaper",
            @"wall paper",
            @"wall covering",
            @"wallcovering",
            @"border",
            @"unpasted",
            @"prepasted",
            @"washable",
            @"strippable",
            @"scrubbable",
            @"made in",
            @"Search Words",
            @"\w{2,4}\d{2,4}",
        };

        private static readonly string[] MatchValues =
        {
            "Straight",
            "Drop",
            "Random",
            "Reversible",
            "Non-Reverse",
            "Random Reversible",
            "Hang",
            "Reverse",
            "Non-Reversible",
            "Straight Non-Reversible",
            "Straight Non-Reversi", // for type
            "Straight Reversible",
            "Random Reverse",
            "Half Drop",
            "None",
        };

        private static readonly string[] CountryStopWords =
        { 
            "wallpaper",
            "wallcovering",
            "texture",
            "transitional",
            "floral",
        };
    }
}