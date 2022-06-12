using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Nomi.Details
{
    public class NomiProductBuilder : IProductBuilder<NomiVendor>
    {
        public VendorProduct Build(ScanData data)
        {
            var mpn = data[ProductPropertyType.ManufacturerPartNumber];
            var cost = data[ProductPropertyType.WholesalePrice].ToDecimalSafe();
            var patternName = data[ProductPropertyType.PatternName].TitleCase();
            var colorName = data[ProductPropertyType.ColorName].Replace("BlackAndWhite", "Black and White").TitleCase();

            var vendor = new NomiVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, true, vendor);

            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = data[ProductPropertyType.ManufacturerPartNumber].Split(new[] { '-' }).Last();
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ProductPropertyType.Content].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ProductPropertyType.Durability].Replace("ABRASION: ", "").Trim() + " Double Rubs";
            vendorProduct.PublicProperties[ProductPropertyType.FireCode] = data[ProductPropertyType.FireCode];
            vendorProduct.PublicProperties[ProductPropertyType.FlameRetardant] = data[ProductPropertyType.FlameRetardant];
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = GetHorizontalRepeat(data[ProductPropertyType.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Material] = data[ProductPropertyType.Material].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ProductPropertyType.ManufacturerPartNumber].Split(new[] { '-' }).First();
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ProductPropertyType.Style];
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = GetVerticalRepeat(data[ProductPropertyType.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ProductPropertyType.Width].Replace("WIDTH: ", "").Trim().ToInchesMeasurement();

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, data[ProductPropertyType.PatternNumber]);
            vendorProduct.Name = new[] {mpn, patternName, colorName, "by", vendor.DisplayName}.BuildName();
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.ProductGroup = ProductGroup.Fabric;
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private string GetVerticalRepeat(string repeat)
        {
            if (repeat == null) return null;
            if (!repeat.ContainsIgnoreCase("v") || !repeat.ContainsDigit()) return null;

            repeat = repeat.Replace("REPEAT: ", "").Replace("”", "\"");
            var vert = repeat.CaptureWithinMatchedPattern("x (?<capture>(.*))\"v");
            if (!string.IsNullOrWhiteSpace(vert)) return vert.ToInchesMeasurement();

            vert = repeat.CaptureWithinMatchedPattern("(?<capture>(.*))\"v x");
            if (!string.IsNullOrWhiteSpace(vert)) return vert.ToInchesMeasurement();

            return vert;
        }

        private string GetHorizontalRepeat(string repeat)
        {
            if (repeat == null) return null;
            if (!repeat.ContainsIgnoreCase("h") || !repeat.ContainsDigit()) return null;

            repeat = repeat.Replace("REPEAT: ", "").Replace("”", "\"");
            var horz = repeat.CaptureWithinMatchedPattern("x (?<capture>(.*))\"h");
            if (!string.IsNullOrWhiteSpace(horz)) return horz.ToInchesMeasurement();

            horz = repeat.CaptureWithinMatchedPattern("(?<capture>(.*))\"h x");
            if (!string.IsNullOrWhiteSpace(horz)) return horz.ToInchesMeasurement();

            return horz; ;
        }
    }
}