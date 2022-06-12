using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace AndrewMartin
{
    public class AndrewMartinProductBuilder : ProductBuilder<AndrewMartinVendor>
    {
        public AndrewMartinProductBuilder(IPriceCalculator<AndrewMartinVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new AndrewMartinVendor();

            // no stock info, so always in stock
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(true), vendor);
            var width = data[ScanField.Width].ToDoubleSafe();
            var widthInInches = Math.Round(width/2.54, 2);

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.Category];
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = data[ScanField.Cleaning].Replace("Â", "");
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection];
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = data[ScanField.ColorName].Replace("-", " ");
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content]);
            vendorProduct.PublicProperties[ProductPropertyType.Design] = data[ScanField.Design];
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(data[ScanField.Durability]);
            vendorProduct.PublicProperties[ProductPropertyType.FlameRetardant] = data[ScanField.FlameRetardant];
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = data[ScanField.PatternName].Replace("-", " ").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.ProductUse].Replace("-", "").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatVerticalRepeat(data[ScanField.VerticalRepeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = widthInInches <= 1 ? "" : widthInInches + " inches";

            vendorProduct.PublicProperties[ProductPropertyType.Coverage] = FindCoverage(data[ScanField.UnitOfMeasure]);

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.AddPublicProp(ProductPropertyType.Railroaded, IsRailroaded(data[ScanField.VerticalRepeat]));
            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.AddPublicProp(ProductPropertyType.ColorNumber, data[ScanField.ColorName]);
            vendorProduct.RemoveWhen(ProductPropertyType.ColorNumber, s => !s.ContainsDigit());

            vendorProduct.RemoveWhen(ProductPropertyType.ColorName, s => s.ContainsDigit());

            var unit = GetUnitOfMeasure(data[ScanField.UnitOfMeasure]);
            if (unit == UnitOfMeasure.Roll)
            {
                // all of them are 11 meters
                vendorProduct.PublicProperties[ProductPropertyType.Length] = "11 yards";

                // total square inches / 1296 sq inches in sq yd
                var coverage = Math.Round((widthInInches*11*36)/1296, 2);
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = coverage + " square yards per single roll";
            }

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, data[ScanField.PatternName]).ToUpper();
            vendorProduct.ManufacturerDescription = data[ScanField.Description];
            vendorProduct.MinimumQuantity = GetMinQty(data[ScanField.UnitOfMeasure], unit);
            vendorProduct.Name = new[] {mpn.Replace("-", " ").TitleCase(), "by", vendor.DisplayName}.BuildName();
            vendorProduct.OrderIncrementQuantity = vendorProduct.MinimumQuantity;
            vendorProduct.ProductGroup = unit == UnitOfMeasure.Roll ? ProductGroup.Wallcovering : ProductGroup.Fabric;
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = string.Format("{0}-{1}", vendor.SkuPrefix, mpn.Replace("/", "-"));
            vendorProduct.UnitOfMeasure = unit;
            return vendorProduct;
        }

        private string GetWidth(double width)
        {
            var inchesWidth = Math.Round(width/2.54, 2);
            return inchesWidth + " inches";
        }

        private int GetMinQty(string unitString, UnitOfMeasure unit)
        {
            if (unit == UnitOfMeasure.Yard) return 2;
            if (unit == UnitOfMeasure.Roll)
            {
                if (unitString.ContainsIgnoreCase("1 x")) return 1;
                if (unitString.ContainsIgnoreCase("2 x")) return 2;
                if (unitString.ContainsIgnoreCase("3 x")) return 3;
            }
            return 1;
        }

        private string FindCoverage(string unitOfMeasure)
        {
            if (unitOfMeasure.ContainsIgnoreCase("sq ft"))
            {
                unitOfMeasure = unitOfMeasure.Replace(" (Full Hide)", "").Replace(" (Half Hide)", "");
                return unitOfMeasure.Replace("sq yd", "square yards");
            }
            return string.Empty;
        }

        private UnitOfMeasure GetUnitOfMeasure(string unit)
        {
            if (unit.ContainsIgnoreCase("roll")) return UnitOfMeasure.Roll;
            // even though the product pages say metre, in the US they're sold by yard
            if (unit.ContainsIgnoreCase("metre")) return UnitOfMeasure.Yard;
            return UnitOfMeasure.Each;
        }

        private string IsRailroaded(string verticalRepeat)
        {
            if (string.IsNullOrWhiteSpace(verticalRepeat)) return "No";
            return verticalRepeat.Contains("Railroaded") ? "Yes" : "No";
        }

        private string FormatDurability(string durability)
        {
            if (durability == "0" || durability == "N/A" || durability == "No") return "";
            return durability.Replace(",", "") + " Martindale Double Rubs";
        }

        private readonly Dictionary<string, string> _contentAbbreviations = new Dictionary<string, string>
        {
            { "A", "Acrylic" },
            { "AC", "Acrylic" },
            { "BAM", "Backing" },
            { "BA", "Backing" },
            { "B", "Backing" },
            { "CA", "Acetate" },
            { "Co", "Cotton" },
            { "CO", "Cotton" },
            { "C", "Cotton" },
            { "H", "Cotton" },
            { "J", "Jute" },
            { "JU", "Jute" },
            { "K", "Kenaf" },
            { "Lr", "Leather" },
            { "LI", "Linen" },
            { "L", "Linen" },
            { "MA", "Modacrylic" },
            { "MAS", "Modal" },
            { "MT", "Metal" },
            { "N", "Nylon" },
            { "NY", "Nylon" },
            { "PAC", "Polyacrylic" },
            { "PAN", "Poly" },
            { "PES", "Polyester" },
            { "PVC", "PVC" },
            { "PA", "Polyamide" },
            { "PC", "Acrylic" },
            { "PL", "Polyester" },
            { "PP", "Polypropylene" },
            { "PU", "Polyurethane" },
            { "P", "Polyester" },
            { "RM", "RM" },
            { "R", "Rayon" },
            { "Spun", "Spun" },
            { "S", "Silk" },
            { "T", "Triacetate" },
            { "V", "Velvet" },
            { "VI", "Viscose" },
            { "WA", "Angora" },
            { "WL", "Wool" },
            { "WV", "Fleece Wool" },
            { "W", "Wool" },
            { "WO", "Wool" },
            { "WP", "Alpaca Wool" },
        };

        private string FormatContent(string content)
        {
            if (content.ContainsIgnoreCase("Coated")) return content;

            // exceptions:
            content = content.Replace("100% Cotton Chenille & 100% Cotton Applique", "100% Cotton");
            content = content.Replace("100% Cotton Chenille", "100% Cotton");
            content = content.Replace("100% L plus paper applique", "100% L");
            content = content.Replace("100% L Ground 100% P Embossed", "100% Lambswool");
            content = content.Replace("Paper applique", "Paper");
            content = content.Replace("100% Merino Lambswool", "100% Lambswool");
            content = content.Replace("Ground", "");
            content = content.Replace("Pile", "");
            content = content.Replace("Embossed", "");
            content = content.Replace("Spun P", "SP");

            content = content.Replace(" %", "%");
            content = content.Replace("% ", "%");
            content = content.Replace("%", "% ");

            // content field looks like: 34% C 30% V 7% BAM 5% PA
            var values = content.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var contentTypes = new Dictionary<string, int>();
            for (var i = 0; i < values.Length; i+=2)
            {
                var contentType = values[i + 1];
                if (!values[i].IsInteger()) continue;

                var percentage = Convert.ToInt32(values[i].Replace("%", ""));
                if (contentTypes.ContainsKey(contentType))
                {
                    contentTypes[contentType] += percentage;
                    continue;
                }
                contentTypes.Add(values[i + 1], Convert.ToInt32(values[i].Replace("%", "")));
            }
            content = contentTypes.FormatContentTypes();
            foreach (var kvp in _contentAbbreviations)
            {
                content = Regex.Replace(content, string.Format(@"% {0}\b", kvp.Key), string.Format(@"% {0}", kvp.Value));
            }
            return content;
        }

        private string FormatVerticalRepeat(string repeat)
        {
            repeat = repeat.RemovePattern(" half drop.*");

            repeat = repeat.Remove(" (Railroaded)");
            repeat = repeat.Remove(" (Half Drop)");
            repeat = repeat.Remove(" (Third Drop)");
            repeat = repeat.Remove(" (Straight Match)");
            repeat = repeat.Remove(" (Straight Match Reverse Hang)");
            repeat = repeat.Remove(" (Any Match)");
            repeat = repeat.Remove("Railroaded Ombre Stripe");
            repeat = repeat.Remove("Railroaded Strie");
            repeat = repeat.Remove("Random Match");
            repeat = repeat.Remove("Railroaded");

            repeat = repeat.Trim();

            if (repeat == "") return string.Empty;
            if (repeat == "0") return string.Empty;
            if (repeat == "N/A") return string.Empty;
            if (repeat.Contains("sq ft")) return string.Empty;

            return repeat + " centimeters";
        }
    }
}
