using System;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace JEnnis
{
    public class JEnnisProductBuilder : ProductBuilder<JEnnisVendor>
    {
        public JEnnisProductBuilder(IPriceCalculator<JEnnisVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new JEnnisVendor();
            var mpn = data[ScanField.ManufacturerPartNumber];

            data.Cost = data[ScanField.Cost].ToDecimalSafe();

            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(true), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var pattern = FormatPatternName(data[ScanField.PatternName]);
            var colorName = FormatColorName(data[ScanField.ColorName]);

            vendorProduct.PublicProperties[ProductPropertyType.Book] = data[ScanField.Book];
            vendorProduct.PublicProperties[ProductPropertyType.Brand] = data[ScanField.Brand];
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = data[ScanField.Cleaning];
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection];
            vendorProduct.PublicProperties[ProductPropertyType.Color] = data[ScanField.Color];
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = RemoveWhitespace(data[ScanField.Content].Replace(" ( PU)", "").Replace(" (PVC)", ""));
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = data[ScanField.Country];
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability];
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = data[ScanField.Finish];
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = FormatMeasurement(data[ScanField.HorizontalRepeat]);
            vendorProduct.PublicProperties[ProductPropertyType.LeadTime] = data[ScanField.LeadTime];
            vendorProduct.PublicProperties[ProductPropertyType.Material] = data[ScanField.Material];
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = pattern;
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = data[ScanField.Railroaded];
            vendorProduct.PublicProperties[ProductPropertyType.Style] = RemoveWhitespace(data[ScanField.Style]);
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatMeasurement(data[ScanField.VerticalRepeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = FormatMeasurement(data[ScanField.Width]);

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).Replace("_", "-").SkuTweaks();
            vendorProduct.Correlator = pattern;
            vendorProduct.Name = new[] {pattern, colorName, "by", vendor.DisplayName}.BuildName();
            vendorProduct.SetProductGroup(ProductGroup.Fabric);
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = sku;
            if (data[ScanField.Weight].ContainsIgnoreCase("per square foot")) vendorProduct.UnitOfMeasure = UnitOfMeasure.SquareFoot;
            else vendorProduct.UnitOfMeasure = UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private string FormatCleaning(string cleaning)
        {
            if (cleaning.Length < 800) return cleaning;
            return cleaning.Substring(0, 800);
        }

        private string RemoveWhitespace(string field)
        {
            return string.Join(" ", field.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().Replace(" \"", "")));
        }

        private string FormatPatternName(string pattern)
        {
            pattern = pattern.Replace(" (FULL HIDES ONLY)", "");
            pattern = pattern.Replace(" FULL HIDES ONLY", "");
            pattern = pattern.RemovePattern(@"\(See$");
            return pattern.TitleCase();
        }

        private string FormatColorName(string colorName)
        {
            // TODO: Still a lot of cut off colors names, etc...
            var parts = colorName.Split(' ');
            if (parts.First().IsOnlyDigits()) return string.Join(" ", parts.Skip(1));

            colorName = colorName.Replace(" (FULL HIDES ONLY)", "");
            colorName = colorName.RemovePattern(@"\(See$");
            return colorName;
        }

        private string FormatBolt(string bolt)
        {
            if (bolt == string.Empty) return string.Empty;
            return bolt.Substring(0, bolt.IndexOf("(")).Trim();
        }

        private string FormatMeasurement(string measurement)
        {
            if (measurement == string.Empty) return string.Empty;
            measurement = measurement.Replace("&#034;", "\"");
            return measurement.Substring(0, measurement.IndexOf("(")).ToInchesMeasurement();
        }
    }
}