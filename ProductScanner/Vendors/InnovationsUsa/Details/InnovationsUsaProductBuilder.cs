using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace InnovationsUsa.Details
{
    public class InnovationsUsaProductBuilder : ProductBuilder<InnovationsUsaVendor>
    {
        public InnovationsUsaProductBuilder(IPriceCalculator<InnovationsUsaVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var cost = data[ScanField.Cost].ToDecimalSafe();
            data.Cost = cost;

            var patternName = data[ScanField.PatternName].TitleCase();
            var colorName = data[ScanField.Color].TitleCase().Replace("/J2D4", "").Replace("-", " ");
            var vendor = new InnovationsUsaVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, new StockData(true), vendor);

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = FormatHorizontalRepeat(data[ScanField.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatVerticalRepeat(data[ScanField.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = FormatWidth(data[ScanField.Width]);

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName);
            vendorProduct.MinimumQuantity = GetMinimumQuantity(data[ScanField.MinimumOrder]);
            vendorProduct.Name = new[] {mpn, patternName, colorName, "by", vendor.DisplayName}.BuildName();
            vendorProduct.OrderIncrement = GetOrderIncrement(data[ScanField.OrderIncrement]);
            // the only brands we're allowed to sell are wallcovering
            vendorProduct.SetProductGroup(ProductGroup.Wallcovering);
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();
            return vendorProduct;
        }

        private int GetMinimumQuantity(string minOrder)
        {
            minOrder = minOrder.Replace(" roll", "").Replace(" tiles", "").Replace(" tile", "");

            if (minOrder.ToIntegerSafe() == 0) return 1;
            return minOrder.ToIntegerSafe();
        }

        private int GetOrderIncrement(string orderInc)
        {
            var intInc = orderInc.Replace(" roll", "").Replace(" tiles", "").Replace(" tile", "").ToIntegerSafe();
            return intInc == 0 ? 1 : intInc;
        }

        private string FormatHorizontalRepeat(string repeat)
        {
            var hRepeat = repeat.CaptureWithinMatchedPattern("H: (?<capture>(.*))\"");
            if (hRepeat.ContainsIgnoreCase("V"))
            {
                hRepeat = repeat.CaptureWithinMatchedPattern("(?<capture>(.*))\"");
            }
            if (string.IsNullOrWhiteSpace(hRepeat)) return string.Empty;
            return hRepeat.Replace("APPROX ", "").Replace("APPROX. ", "");
        }

        private string FormatVerticalRepeat(string repeat)
        {
            var vRepeat = repeat.CaptureWithinMatchedPattern("V: (?<capture>(.*))\"");
            if (vRepeat.ContainsIgnoreCase("H"))
            {
                vRepeat = repeat.CaptureWithinMatchedPattern("(?<capture>(.*))\"");
            }
            if (string.IsNullOrWhiteSpace(vRepeat)) return string.Empty;
            return vRepeat.Replace("APPROX ", "").Replace("APPROX. ", "");
        }

        private string FormatWidth(string width)
        {
            width = width.Replace(@"""", "");
            width = width.Replace("H", "");
            if (string.IsNullOrWhiteSpace(width)) return "";
            return width + " inches";
        }
    }
}
