using System;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace PhillipJeffries
{
    public class PhillipJeffriesProductBuilder : ProductBuilder<PhillipJeffriesVendor>
    {
        public PhillipJeffriesProductBuilder(IPriceCalculator<PhillipJeffriesVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new PhillipJeffriesVendor();
            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(data[ScanField.StockCount] == "STOCKED"), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var color = FormatColor(data[ScanField.Color]);
            var pattern = FormatPattern(data[ScanField.ProductName]).Split('-').First().Trim();

            vendorProduct.PublicProperties[ProductPropertyType.AverageBolt] = GetAverageBolt(data[ScanField.AverageBolt]);
            vendorProduct.PublicProperties[ProductPropertyType.Book] = data[ScanField.Book];
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = data[ScanField.Cleaning].Replace("washable", "Washable");
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection];
            vendorProduct.PublicProperties[ProductPropertyType.CollectionId] = data[ScanField.CollectionId];
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = color;
            vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = data[ScanField.ColorGroup];
            vendorProduct.PublicProperties[ProductPropertyType.FireCode] = data[ScanField.FireCode];
            vendorProduct.PublicProperties[ProductPropertyType.IsLimitedAvailability] = data[ScanField.IsLimitedAvailability];
            vendorProduct.PublicProperties[ProductPropertyType.ItemNumber] = mpn;
            vendorProduct.PublicProperties[ProductPropertyType.Material] = data[ScanField.Material].Replace("&amp;", "&");
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = pattern;
            vendorProduct.PublicProperties[ProductPropertyType.Width] = FormatWidth(data[ScanField.Width]);

            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = GetRepeat(data[ScanField.HorizontalRepeat]);
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = GetRepeat(data[ScanField.VerticalRepeat]);

            vendorProduct.PublicProperties[ProductPropertyType.MadeToOrder] = data[ScanField.Book].ContainsIgnoreCase("Prints") ? "Yes" : null;

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            var orderIncrement = data[ScanField.OrderIncrement].Replace(" Yards", "").ToIntegerSafe();
            var minOrder = data[ScanField.MinimumQuantity].TakeOnlyFirstIntegerToken();

            vendorProduct.Correlator = data[ScanField.ManufacturerPartNumber];
            vendorProduct.IsDiscontinued = data.IsDiscontinued;
            vendorProduct.Name = new[] { mpn, pattern, color, "by", vendor.DisplayName }.BuildName();
            vendorProduct.MinimumQuantity = minOrder == 0 ? 1 : minOrder;
            vendorProduct.OrderIncrement = orderIncrement > 0 ? orderIncrement : 1;
            vendorProduct.OrderRequirementsNotice = GetOrderRequirements(data[ScanField.StockCount]);
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SetProductGroup(ProductGroup.Wallcovering);
            vendorProduct.SKU = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure].ContainsIgnoreCase("Panel") ? UnitOfMeasure.Panel : UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private string GetOrderRequirements(string stockCount)
        {
            if (stockCount.ContainsIgnoreCase("Lead Time:, Four Weeks"))
                return "This product is made to order. Please allow 4 weeks for delivery.";
            if (stockCount.ContainsIgnoreCase("Lead Time:, Two To Three Weeks"))
                return "This product is made to order. Please allow 2-3 weeks for delivery.";
            if (stockCount.ContainsIgnoreCase("Mill Stocked Item"))
                return "This product is made to order. Please contact us for lead time.";
            return string.Empty;
        }

        private string FormatWidth(string width)
        {
            width = width.TitleCase();
            width = width.Replace("\"", "");
            width = width.Replace("Tims", "Trims");
            width = width.Replace("Inches", "inches");
            width = width.Replace("Trims to", "Trims");
            width = width.Replace("inches Trims", "inches, trims to");
            width = width.Replace("Pretrimmed", "pretrimmed");
            width = width.Replace("Pre-Trimmed", "pretrimmed");
            width = width.Replace("Pre-trimmed", "pretrimmed");
            width = width.Replace("Untrimmed", "untrimmed");
            return width;
        }

        private string GetRepeat(string repeat)
        {
            if (repeat == "n/a" || repeat == string.Empty) return string.Empty;
            repeat = repeat.Replace(" Inches", "\"");
            var repeatInt = repeat.Split(new[] {"\""}, StringSplitOptions.RemoveEmptyEntries).First();
            if (repeatInt == string.Empty) return string.Empty;
            return repeatInt + " inches";
        }

        private string GetAverageBolt(string averageBolt)
        {
            if (averageBolt == "Not Applicable") return string.Empty;
            averageBolt = averageBolt.Replace(" Bolts", "").Replace(" Bolt", "");
            averageBolt = averageBolt.ReplaceWholeWord("Yard", "Yards");
            return averageBolt;
        }

        private string FormatColor(string color)
        {
            return color.TitleCase();
        }

        private string FormatPattern(string pattern)
        {
            pattern = pattern.Replace("LIMITED STOCK/", "");
            pattern = pattern.Replace("LIMITED STOCK: ", "");
            pattern = pattern.Replace("LIMITED STOCK /", "");
            pattern = pattern.Replace("LIMITED STOCK", "");
            pattern = pattern.Replace("Limited Stock/", "");
            pattern = pattern.Replace("Limited/", "");

            pattern = pattern.Replace("Discontinued/", "");
            pattern = pattern.Replace("DISCONTINUED/", "");

            pattern = pattern.Replace("Archive/", "");
            pattern = pattern.Replace("archive/", "");

            pattern = pattern.Replace("StriÃ©", "Stripe");

            pattern = pattern.Replace("Lighting Strikes", "Lightning Strikes");

            return pattern;
        }
    }
}