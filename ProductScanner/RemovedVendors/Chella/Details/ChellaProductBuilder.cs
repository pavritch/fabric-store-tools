using System;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Chella.Details
{
    public class ChellaProductBuilder : IProductBuilder<ChellaVendor>
    {
        public VendorProduct Build(ScanData data)
        {
            var mpn = data[ProductPropertyType.ManufacturerPartNumber];
            var cost = data[ProductPropertyType.WholesalePrice].ToDecimalSafe();
            var patternName = data[ProductPropertyType.PatternName].TitleCase();

            var vendor = new ChellaVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn);
            var vendorProduct = new FabricProduct(mpn, cost, true, vendor);

            vendorProduct.PublicProperties[ProductPropertyType.Category] = FormatCategory(data[ProductPropertyType.Category]);
            vendorProduct.PublicProperties[ProductPropertyType.Color] = FormatColor(data[ProductPropertyType.Color]);
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = FormatColorName(data[ProductPropertyType.ProductName].Split(new[] { ',' }).Last().TitleCase());
            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = data[ProductPropertyType.ColorNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ProductPropertyType.Content].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = data[ProductPropertyType.CountryOfOrigin].Replace("Country of Origin:", "").Trim().ToFormattedCountry();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ProductPropertyType.Durability].Replace("Durability: ", "");
            vendorProduct.PublicProperties[ProductPropertyType.FireCode] = data[ProductPropertyType.FireCode].Trim().Replace("FR:", "");
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = FormatRepeat(data[ProductPropertyType.Repeat], "H");
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ProductPropertyType.PatternNumber];
            vendorProduct.PublicProperties[ProductPropertyType.ProductType] = data[ProductPropertyType.ProductType].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatRepeat(data[ProductPropertyType.Repeat], "V");
            vendorProduct.PublicProperties[ProductPropertyType.Width] = FormatWidth(data[ProductPropertyType.Width]);

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data[ProductPropertyType.ProductDetailUrl];

            vendorProduct.AddPublicProp(ProductPropertyType.Style, data[ProductPropertyType.Category]);

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, data[ProductPropertyType.PatternNumber]);
            vendorProduct.Name = new[] {mpn, patternName, "by", vendor.DisplayName}.BuildName();
            vendorProduct.ProductGroup = mpn.ContainsIgnoreCase("t") ? ProductGroup.Trim : ProductGroup.Fabric;
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = patternName.ContainsIgnoreCase("tie back") ? UnitOfMeasure.Each : UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private string FormatCategory(string category)
        {
            category = category.Replace("&amp; ", "");
            return category;
        }

        private string FormatColorName(string color)
        {
            color = color.Replace("Cr�me", "Crame");
            color = color.Replace("&#038;", "&");
            color = color.Replace("&#8217;", "'");
            return color;
        }

        private string FormatColor(string colors)
        {
            var list = colors.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct()
                     .Where(x => !x.ContainsIgnoreCase("Multi")).ToList();
            if (!list.Any()) return null;
            return list.ToCommaDelimitedList().TitleCase(); 
        }

        private string FormatRepeat(string repeat, string key)
        {
            if (repeat.ContainsIgnoreCase("Varied")) return null;
            var horizontal = repeat.CaptureWithinMatchedPattern(string.Format("{0}(?<capture>(.*))\"", key));
            return horizontal == null ? null : horizontal.Trim(new[] {':'}).ToInchesMeasurement();
        }

        private string FormatWidth(string width)
        {
            width = width.Replace("approx.", "");
            if (width.Contains("-")) width = width.Substring(0, width.IndexOf("-") - 1);
            return width.ToInchesMeasurement();
        }
    }
}