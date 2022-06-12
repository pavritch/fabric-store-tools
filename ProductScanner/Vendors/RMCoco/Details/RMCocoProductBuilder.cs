using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace RMCoco.Details
{
            //new FileProperty("Item Group ID", ScanField.Group),
            //new FileProperty("Lowest MAP Price", ScanField.MAP),
            //new FileProperty("Wholesale", ScanField.Cost),
    public class RMCocoPriceCalculator : DefaultPriceCalculator<RMCocoVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            var retail = data[ScanField.RetailPrice].ToDecimalSafe();
            return new ProductPriceData(map, retail * 2);
        }
    }

    public class RMCocoProductBuilder : ProductBuilder<RMCocoVendor>
    {
        public RMCocoProductBuilder(IPriceCalculator<RMCocoVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var productName = data[ScanField.ProductName].TitleCase();
            var split = productName.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var colorName = FormatColorName(data[ScanField.ColorName]);

            var style = data[ScanField.Style].Replace("solid-w-pattern", "").Replace("-", " ").TitleCase();

            var vendor = new RMCocoVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(true), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.Book] = data[ScanField.Book].Replace(" Book", string.Empty);
            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.Category].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.CleaningCode] = data[ScanField.Cleaning].Replace("w/ care", "with care");
            vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = data[ScanField.ColorGroup].Replace("-", " ").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content]);
            //vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability].Replace("Passes ", " ");
            vendorProduct.PublicProperties[ProductPropertyType.FlameRetardant] = data[ScanField.FlameRetardant];
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.OrderInfo] = data[ScanField.ColorName].ContainsIgnoreCase(" - CFA") ? "Cut for approval recommended." : "";
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = data[ScanField.PatternName];
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternNumber].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.ProductUse].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = data[ScanField.Railroaded];
            vendorProduct.PublicProperties[ProductPropertyType.Style] = style;
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = data[ScanField.ColorNumber];
            vendorProduct.PublicProperties[ProductPropertyType.FlameRetardant] = data[ScanField.FlameRetardant];
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = data[ScanField.Railroaded];

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = false.ToString();
            var status = data[ScanField.Status];
            if (status.ContainsIgnoreCase("Limited Stock"))
            {
                vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = true.ToString();
            }

            if (status.ContainsIgnoreCase("Discontinued"))
            {
                vendorProduct.IsDiscontinued = true;
            }

            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.RemoveWhen(ProductPropertyType.Book, s => s.ContainsIgnoreCase("n/a"));
            vendorProduct.RemoveWhen(ProductPropertyType.ColorNumber, s => !s.ContainsDigit());
            vendorProduct.RemoveWhen(ProductPropertyType.Style, s => excludedStyles.Contains(s));

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, data[ScanField.PatternNumber]);
            vendorProduct.Name = new[] { data[ScanField.PatternName], colorName, vendorProduct.PublicProperties[ProductPropertyType.Style], "by", vendor.DisplayName }.BuildName();
            vendorProduct.ScannedImages = new List<ScannedImage> { new ScannedImage(ImageVariantType.Primary, data[ScanField.ImageUrl])};
            vendorProduct.SetProductGroup(data[ScanField.Group].StartsWith("T") ? ProductGroup.Trim : ProductGroup.Fabric);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private double GetStockCount(string count)
        {
            if (string.IsNullOrWhiteSpace(count)) return 0;
            return count.Replace("YD", "").Replace("yd", "").Replace("Yd", "").Replace("ea", "").ToDoubleSafe();
        }

        private static readonly HashSet<string> excludedStyles = new HashSet<string>()
        {
            "cushiontas",
            "furn-fring",
            "beaded-fri",
            "beaded-tri",
            "beadtasfri",
            "brfri-rouc",
            "lipcord", 
            "ropetassfr", 
            "scallop-fr",
            "tassel-fri",
            "tietassel", 
            "4-bullion", 
            "5-bullion", 
            "6-bullion", 
            "button",
            "chair-tie",
            "key-tassel",
            "gimp",
        };

        private string FormatColorName(string colorName)
        {
            colorName = colorName.Replace(" - CFA", string.Empty);
            colorName = colorName.Replace(" SFA", string.Empty);

            // remove trim categories
            colorName = colorName.Replace("DEC.CORD WITH LIP", string.Empty);
            colorName = colorName.Replace("ROPE", string.Empty);
            colorName = colorName.ReplaceWholeWord("CORD", string.Empty);
            colorName = colorName.Replace("TASSEL", string.Empty);
            colorName = colorName.Replace("TSL", string.Empty);
            colorName = colorName.Replace("TSSL", string.Empty);
            colorName = colorName.Replace("FRINGE", string.Empty);

            // expand abbreviations
            colorName = colorName.Replace("DK", "Dark");
            colorName = colorName.Replace("BLK", "Black");

            colorName = colorName.Trim();

            // if contains "not use" return null
            if (colorName.ToLower().Contains("not use")) return null;

            // if all digits, then it's a number
            if (colorName.IsOnlyDigits()) return null;

            // if it starts with S and then has numbers, it's a number
            if (colorName.ContainsDigit() && colorName[0] == 'S') return null;

            // remove any words that contain numbers
            var split = colorName.Split(new[] { ' ' });
            var wordSplits = split.Where(x => !x.ContainsDigit()).ToList();

            if (!wordSplits.Any()) return null;
            if (string.IsNullOrWhiteSpace(colorName)) return null;

            colorName = string.Join(" ", wordSplits);
            return colorName.TitleCase();
        }

        private string FormatContent(string fabricContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fabricContent)) return null;

                if (fabricContent.ContainsIgnoreCase("100% Cotton")) return "100% Cotton";
                if (fabricContent.ContainsIgnoreCase("100% PVC")) return "100% PVC";
                if (fabricContent.ContainsIgnoreCase("100% Silk")) return "100% Silk";

                fabricContent = fabricContent.Replace("Base Cloth", "");
                fabricContent = fabricContent.Replace("Half Drop", "");
                fabricContent = fabricContent.Replace("Drop", "");
                fabricContent = fabricContent.Replace("Face", "");
                fabricContent = fabricContent.Replace("Back", "");
                fabricContent = fabricContent.Replace(":", "");

                fabricContent = fabricContent.Replace("$", "%").Replace("%", "% ");

                if (!fabricContent.Contains(","))
                {
                    // split on spaces to see if we can make sense of this
                    var split = fabricContent.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length >= 4)
                    {
                        if (split.Length % 2 == 1) split = split.Take(split.Length - 1).ToArray();

                        fabricContent = BuildCommaSeparatedString(split);
                    }
                }

                fabricContent = fabricContent.Trim().Trim(new[] { ',' }).Trim();

                var parts = fabricContent.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                var contentTypes = parts
                    .Select(x => x.Replace("%", ""))
                    .Select(x => x.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                var dictionary = new Dictionary<string, int>();
                foreach (var contentType in contentTypes)
                {
                    // we only have a % value so just assume it's 'Other'
                    if (contentType.Length == 1)
                    {
                        dictionary.Add("Other", ProcessContentValue(contentType.First()));
                    }
                    else
                    {
                        var content = contentType.Skip(1).Aggregate((a, b) => a + " " + b);
                        var expandedContentType = ExpandContentType(content);
                        if (!dictionary.ContainsKey(expandedContentType))
                        {
                            dictionary.Add(expandedContentType, ProcessContentValue(contentType.First()));
                        }
                    }
                }
                return dictionary.FormatContentTypes();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> _fullContentTypes = new List<string>
        {
            "Polyamide",
            "Polyacrylic",
            "Spun Viscose",
            "Spun Rayon",
            "Polyester Vinyl",
            "Viscose Rayon",
            "Viscose Spun",
            "Polyester Filament",
            "Egymer Cotton"
        };

        private string ExpandContentType(string content)
        {
            content = content.TitleCase();
            if (_fullContentTypes.Contains(content)) return content;

            if (content.StartsWith("Sp")) return "Spun Viscose";

            if (content.StartsWith("O")) return "Organic Cotton";
            if (content.StartsWith("C")) return "Cotton";
            if (content.StartsWith("N")) return "Nylon";
            if (content.StartsWith("S")) return "Silk";
            if (content.StartsWith("P")) return "Polyester";
            if (content.StartsWith("A")) return "Acrylic";
            if (content.StartsWith("V")) return "Viscose";
            if (content.StartsWith("R")) return "Rayon";
            if (content.StartsWith("L")) return "Linen";
            return content;
        }

        private int ProcessContentValue(string value)
        {
            double outValue;
            if (Double.TryParse(value, out outValue)) return (int) Math.Round(outValue);
            return 0;
        }

        private string BuildCommaSeparatedString(string[] split)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < split.Length; i += 2)
            {
                builder.AppendFormat("{0} {1}, ", split[i], split[i + 1]);
            }
            return builder.ToString();
        }
    }
}