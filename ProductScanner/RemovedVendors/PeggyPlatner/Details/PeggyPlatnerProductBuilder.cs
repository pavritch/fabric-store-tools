using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace PeggyPlatner.Details
{
    public class PeggyPlatnerProductBuilder : IProductBuilder<PeggyPlatnerVendor>
    {
        public VendorProduct Build(ScanData data)
        {
            var mpn = data[ProductPropertyType.ManufacturerPartNumber];
            var cost = data[ProductPropertyType.WholesalePrice].ToDecimalSafe();
            var patternName = data[ProductPropertyType.PatternName];

            var vendor = new PeggyPlatnerVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn.Replace("/", "-")).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, true, vendor);

            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ProductPropertyType.Content]);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = data[ProductPropertyType.CountryOfOrigin].ToFormattedCountry();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(data[ProductPropertyType.Durability].Replace(",", ""));
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ProductPropertyType.Width].ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.Material] = data[ProductPropertyType.Material];
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = data[ProductPropertyType.PatternName];
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ProductPropertyType.PatternNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ProductPropertyType.Style];
            vendorProduct.PublicProperties[ProductPropertyType.Use] = data[ProductPropertyType.Use];

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data[ProductPropertyType.ProductDetailUrl];

            vendorProduct.AddProp(ProductPropertyType.Durability, data, "N/A");
            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName);
            vendorProduct.Name = new[] { mpn, patternName, "by", vendor.DisplayName }.BuildName();
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.ProductGroup = ProductGroup.Fabric;
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private string FormatDurability(string durability)
        {
            var value = durability.TakeOnlyFirstIntegerToken();
            if (value == 0) return string.Empty;
            return value + " Martindale Double Rubs";
        }

        private string FormatContent(string content)
        {
            content = content.ToFormattedFabricContent();
            if (content == null) return string.Empty;
            foreach (var kvp in _contentAbbreviations)
            {
                content = Regex.Replace(content, string.Format(@"% {0}\b", kvp.Key), string.Format(@"% {0}", kvp.Value));
            }
            return content.TitleCase();
        }

        private readonly Dictionary<string, string> _contentAbbreviations = new Dictionary<string, string>
        {
            { "ACR", "Acrylic" },
            { "AF", "Cotton" },
            { "CO", "Cotton" },
            { "LI", "Linen" },
            { "PC", "Acrylic" },
            { "PL", "Polyester" },
            { "S", "Silk" },
            { "VI", "Viscose" },
        };
    }
}