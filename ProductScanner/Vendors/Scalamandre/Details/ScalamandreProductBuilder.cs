using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Scalamandre.Details
{
    public class ScalamandreProductBuilder : ProductBuilder<ScalamandreVendor>
    {
        public ScalamandreProductBuilder(IPriceCalculator<ScalamandreVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            mpn = mpn.Replace("SC", "");

            var cost = data[ScanField.Cost].ToDecimalSafe();
            data.Cost = cost;

            var pattern = data[ScanField.Pattern].TitleCase();
            var color = FormatColorName(data[ScanField.Color]);

            var vendor = new ScalamandreVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, new StockData(data[ScanField.StockCount].ToDoubleSafe()), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            //vendorProduct.PublicProperties[ProductPropertyType.AdditionalInfo] = data[ScanField.AdditionalInfo].Replace(".,", ",").Replace("Panel Effect is,", "").Replace("uph.", "upholstery").RemovePattern(@"\.$");
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = data[ScanField.Cleaning].Replace("\"S\" no water solvents", "No water solvents").Replace(".,", ",").RemovePattern(@"\.$").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection].Replace("ollections", "ollection").Replace(" Collection", "").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Color] = color;
            //vendorProduct.PublicProperties[ProductPropertyType.Coordinates] = data[ScanField.Coordinates].RemoveLabel();
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content].Replace(",,", ",").Replace("\a", " ");
            //vendorProduct.PublicProperties[ProductPropertyType.CordSpread] = ExtensionMethods.MeasurementFromFraction(data[ScanField.CordSpread].RemoveLabel()).ToInchesMeasurement();
            //vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability].TitleCase().Replace("D.R.", "Double Rubs").RemovePattern(@"\.$").TrimToNull();
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = data[ScanField.Finish].Replace(".,", ",").RemovePattern(@"\.$").TitleCase();
            //vendorProduct.PublicProperties[ProductPropertyType.FlameRetardant] = data[ScanField.FlameRetardant].Replace(".,", ",").RemovePattern(@"\.$");
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = FormatHorizontalRepeat(data[ScanField.Repeat]);
            //vendorProduct.PublicProperties[ProductPropertyType.Layout] = data[ScanField.Layout].RemoveLabel();
            vendorProduct.PublicProperties[ProductPropertyType.Length] = ExtensionMethods.MeasurementFromFraction(data[ScanField.Length].RemoveLabel()).ToInchesMeasurement();
            //vendorProduct.PublicProperties[ProductPropertyType.Note] = FormatNote(data[ScanField.Note]);
            //vendorProduct.PublicProperties[ProductPropertyType.OrderInfo] = data[ScanField.OrderInfo].RemovePattern(@"\.$");
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = pattern;
            //vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternNumber].ToUpper();
            //vendorProduct.PublicProperties[ProductPropertyType.Prepasted] = data[ScanField.Prepasted] == string.Empty ? null : "Yes";
            //vendorProduct.PublicProperties[ProductPropertyType.Screens] = data[ScanField.Screens].RemoveLabel();
            vendorProduct.PublicProperties[ProductPropertyType.Use] = data[ScanField.ProductUse].TitleCase().RemovePattern(@"\.$").RemovePattern(@"\sAnd$").RemovePattern(@"\sAnd,");
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatVerticalRepeat(data[ScanField.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = ExtensionMethods.MeasurementFromFraction(data[ScanField.Width].RemoveLabel()).ToInchesMeasurement();

            //vendorProduct.PublicProperties[ProductPropertyType.Backing] = data[ScanField.Backing];
            vendorProduct.PublicProperties[ProductPropertyType.Brand] = data[ScanField.Brand];
            //vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.Category];
            //vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = data[ScanField.ColorNumber];
            //vendorProduct.PublicProperties[ProductPropertyType.Group] = data[ScanField.Group];
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ScanField.Style];

            vendorProduct.PublicProperties[ProductPropertyType.MadeToOrder] = data[ScanField.StockInventory] == "N" ? "Yes" : null;

            var unit = GetUnitOfMeasure(data[ScanField.UnitOfMeasure], data[ScanField.ProductGroup]);
            var orderInc = GetOrderIncrement(data, unit);
            var dimensions = GetDimensions(data[ScanField.OrderInfo], vendorProduct.PublicProperties[ProductPropertyType.Width], orderInc);

            vendorProduct.MinimumQuantity = GetMinimumQuantity(data);
            vendorProduct.OrderIncrement = orderInc;

            if (dimensions != null && unit == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthInYardsFormatted();

                vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                vendorProduct.NormalizeWallpaperPricing(vendorProduct.OrderIncrement);
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(vendorProduct.OrderIncrement);

                vendorProduct.OrderIncrement = 1;
                vendorProduct.MinimumQuantity = 1;
            }

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);
            vendorProduct.AddPublicProp(ProductPropertyType.MinimumOrder, GetMinimumOrder(data[ScanField.MinimumOrder]));

            vendorProduct.RemoveWhen(ProductPropertyType.Finish, s => s.ContainsIgnoreCase("in yard"));

            vendorProduct.Correlator = GetCorrelator(data[ScanField.PatternNumber], sku, vendor.SkuPrefix);
            vendorProduct.Name = new[] { mpn, pattern, color, "by", vendor.DisplayName }.BuildName();
            vendorProduct.ScannedImages = new List<ScannedImage> {new ScannedImage(ImageVariantType.Primary, data[ScanField.ImageUrl])};
            vendorProduct.SetProductGroup(GetProductGroup(data[ScanField.ProductGroup]));
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = unit;

            return vendorProduct;
        }

        private ProductGroup GetProductGroup(string group)
        {
            if (group == "FABR") return ProductGroup.Fabric;
            if (group == "TRIM") return ProductGroup.Trim;
            return ProductGroup.Wallcovering;
        }

        private UnitOfMeasure GetUnitOfMeasure(string stockCount, string productGroup)
        {
            if (stockCount.ContainsIgnoreCase("YD")) return UnitOfMeasure.Yard;
            // what is LY?? Looks like these are just by yard
            if (stockCount.ContainsIgnoreCase("LY")) return UnitOfMeasure.Yard;
            if (stockCount.ContainsIgnoreCase("EA")) return UnitOfMeasure.Each;
            if (stockCount.ContainsIgnoreCase("RL")) return UnitOfMeasure.Roll;
            if (stockCount.ContainsIgnoreCase("PL")) return UnitOfMeasure.Panel;
            if (stockCount.ContainsIgnoreCase("RP")) return UnitOfMeasure.Yard;
            // bolt
            if (stockCount.ContainsIgnoreCase("BT")) return UnitOfMeasure.Roll;
            return UnitOfMeasure.Yard;
        }

        private RollDimensions GetDimensions(string orderInfo, string width, int increment)
        {
            // make this more generic to future proof
            int length;
            if (orderInfo.ContainsIgnoreCase("yards to a roll")) length = orderInfo.TakeOnlyFirstIntegerToken();
            else if (orderInfo.ContainsIgnoreCase("Packed & Sold in")) length = 8;
            else return null;

            var widthNum = width.Replace("inches", "").Replace("inch", "").ToDoubleSafe();
            return new RollDimensions(widthNum, length * 12 * 3 * increment);
        }

        private string GetMinimumOrder(string minimumOrder)
        {
            if (minimumOrder.ContainsIgnoreCase("2 Yard Minimum") || minimumOrder.ContainsIgnoreCase("Two yard Minimum"))
                minimumOrder = "Minimum 2 Yards";

            return minimumOrder;
        }

        private int GetMinimumQuantity(ScanData data)
        {
            if (data[ScanField.OrderInfo].ContainsIgnoreCase("bolts")) return 2;
            if (data[ScanField.OrderInfo].ContainsIgnoreCase("8 yard increments")) return 8;
            if (data[ScanField.MinimumQuantity].Trim() == string.Empty) return 1;


            var qty = data[ScanField.MinimumQuantity]
                .Split('|').First()
                .Replace("YD MINIMUM (MUST ORDER IN 2 YD INCREMENTS)", "")
                .Replace("1ROLL", "1")
                .Replace("ROLL (13.12 YDS)", "")
                .Replace("ROLL CONSISTS OF 2 PANELS", "")
                .Replace("YARD INCREMENTS", "")
                .Replace("YDS (1 ROLL)", "")
                .Replace("YDS  (1 ROLL)", "")
                .Replace("YDS/ 1 ROLL", "")
                .Replace("YDS/1 ROLL", "")
                .Replace("YDS/1ROLL", "")
                .Replace("YDS./ 1 ROLL", "")
                .Replace("ROLL (11 YDS)", "")
                .Replace("YDS MIN", "")
                .Replace("YARD MINIMUM", "")
                .Replace("PC.", "")
                .ReplaceWholeWord("PIECES", "")
                .ReplaceWholeWord("PANEL", "")
                .ReplaceWholeWord("ROLL", "")
                .ReplaceWholeWord("ROLLS", "")
                .ReplaceWholeWord("YARDS", "")
                .ReplaceWholeWord("YARD", "")
                .ReplaceWholeWord("YD", "")
                .ReplaceWholeWord("YDS", "")
                .ReplaceWholeWord("TILES", "")
                .ReplaceWholeWord("METERS", "")
                .Trim();

            if (data[ScanField.MinimumQuantity] == "CUT LENGTH") return 1;
            if (data[ScanField.MinimumQuantity] == "CUT ORDER") return 1;

            // SQUARE METERS
            // SOLD BY REPEAT
            // DOUBLE ROLL

            double min;
            bool isDouble = Double.TryParse(qty, out min);
            if (!isDouble)
            {
                return data[ScanField.MinimumQuantity].TakeOnlyFirstIntegerToken();
            }
            return (int) min;
        }

        private int GetOrderIncrement(ScanData data, UnitOfMeasure unit)
        {
            if (data[ScanField.OrderInfo].ContainsIgnoreCase("bolts")) return 1;
            if (data[ScanField.OrderInfo].ContainsIgnoreCase("2 US Rolls")) return 2;
            if (unit == UnitOfMeasure.Roll) return 2;

            if (data[ScanField.OrderInfo].ContainsIgnoreCase("8 yard increments")) return 8;

            var value = data[ScanField.OrderIncrement];
            if (value == null)
                return 1;

            int increment = 0;
            if (int.TryParse(value, out increment) && increment > 1)
                return increment;
            return 1;
        }

        private string GetCorrelator(string patternNumber, string sku, string prefix)
        {
            if (patternNumber != string.Empty)
                return string.Format("{0}-{1}", prefix, patternNumber).ToUpper();

            return sku; 
        }

        private int GetStock(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.ContainsIgnoreCase("out of stock") || value.ContainsIgnoreCase("discontinued"))
                return 0;

            if (value.ContainsIgnoreCase("quick ship") || value.ContainsIgnoreCase("prompt delivery") || value.ContainsIgnoreCase("stock check") || value.ContainsIgnoreCase("please call"))
                return 1;

            var qtyStr = value.Replace("Stock Quantity:", "").Trim().Replace(" ", "");
            var qty = qtyStr.CaptureWithinMatchedPattern(@"(?<capture>\d+)(YD|RP|HD|PL|SF|BT|RL|EA|ST)");
            return qty.ToIntegerSafe();
        }

        private string FormatColorName(string value)
        {
            return value.Replace("Color:", "")
                .Replace("Color ", "")
                .Replace(" e ", " ")
                .Replace("(Documentary Color)", "")
                .Replace("(Documentary Colorway)", "")
                .Replace("(Gothic)", "")
                .Replace("*", "")
                .ReplaceWholeWord("Dk", "Dark")
                .ReplaceWholeWord("Lt", "Light")
                .Trim()
                .TitleCase();
        }

        private string FormatNote(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && (value.ContainsIgnoreCase("please call") || value.ContainsIgnoreCase("stock check")))
                return "Please contact us to check stock.";
            return null;
        }

        private string FormatVerticalRepeat(string repeat)
        {
            if (repeat.ContainsIgnoreCase("Vert Rpt:"))
            {
                // Approx Vert Rpt: 33
                // Approx Vert Rpt: 3/8"
                repeat = repeat.RemoveLabel();
            }
            else
            {
                // Approx Rpt: V. 26 5/8", H. 28 5/8"
                repeat = repeat.CaptureWithinMatchedPattern(@"V\.\s(?<capture>(.*)),");
            }

            repeat = ExtensionMethods.MeasurementFromFraction(repeat);
            return repeat.ToInchesMeasurement();
        }

        private string FormatHorizontalRepeat(string repeat)
        {
            if (repeat.ContainsIgnoreCase("Horz Rpt:"))
            {
                // Approx Vert Rpt: 33
                // Approx Vert Rpt: 3/8"
                repeat = repeat.RemoveLabel();
            }
            else
            {
                // Approx Rpt: V. 26 5/8", H. 28 5/8"
                repeat = repeat.CaptureWithinMatchedPattern(@"H\.\s(?<capture>(.*))(,)?");
            }

            repeat = ExtensionMethods.MeasurementFromFraction(repeat);
            return repeat.ToInchesMeasurement();
        }
    }
}