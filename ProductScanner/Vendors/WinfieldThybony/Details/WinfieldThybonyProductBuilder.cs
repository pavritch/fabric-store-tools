using System;
using System.Linq;
using System.Text.RegularExpressions;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace WinfieldThybony.Details
{
    public class WinfieldThybonyProductBuilder : ProductBuilder<WinfieldThybonyVendor>
    {
        public WinfieldThybonyProductBuilder(IPriceCalculator<WinfieldThybonyVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var cost = data[ScanField.Cost].ToDecimalSafe();
            data.Cost = cost;

            var vendor = new WinfieldThybonyVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, new StockData(true), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var color = data[ScanField.Color].Replace("| ", "");
            var pattern = data[ScanField.Pattern];
            var width = data[ScanField.Width].Replace("Width:", "").Replace("\"", "").Replace(" 1/2", ".5").Replace("(30 untrimmed)", "").Trim().ToDoubleSafe();
            var unit = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();

            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content]);
            vendorProduct.PublicProperties[ProductPropertyType.Repeat] = data[ScanField.Repeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = width.ToString().ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.Backing] = data[ScanField.Backing].Replace("Backing:", "");
            vendorProduct.PublicProperties[ProductPropertyType.Brand] = data[ScanField.Brand];
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection];
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = data[ScanField.Country];
            vendorProduct.PublicProperties[ProductPropertyType.Direction] = data[ScanField.Direction];
            vendorProduct.PublicProperties[ProductPropertyType.FireCode] = data[ScanField.FireCode].Replace("Fire Rating:", "");
            vendorProduct.PublicProperties[ProductPropertyType.ItemNumber] = mpn;
            vendorProduct.PublicProperties[ProductPropertyType.Match] = data[ScanField.Match].Replace("Pattern Match:", "");
            vendorProduct.PublicProperties[ProductPropertyType.Repeat] = data[ScanField.Repeat].Replace("Repeat:", "");
            vendorProduct.PublicProperties[ProductPropertyType.Pattern] = pattern;
            vendorProduct.PublicProperties[ProductPropertyType.Use] = data[ScanField.Use];

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            var packaging = data[ScanField.Packaging];
            if (unit == UnitOfMeasure.Roll && !string.IsNullOrWhiteSpace(packaging))
            {
                // apparently "3 roll" (coverage) minimum purchase means that it's sold by triple rolls
                var soldBy = packaging.ContainsIgnoreCase("30 roll") ? 30 :
                    packaging.ContainsIgnoreCase("3 roll") ? 3 :
                    packaging.ContainsIgnoreCase("double roll") ? 2 : 1;
                var coverage = Regex.Matches(packaging, @"\((.*?)\)").Cast<Match>().Last().Groups[1].Value.Replace(" sq ft", "").ToDoubleSafe();
                var coverageInInches = coverage*144;
                var lengthInInches = coverageInInches/width;

                var dimensions = new RollDimensions(width, lengthInInches, soldBy);
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Length] = Math.Round(lengthInInches/36, 2) + " yards";

                vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();

                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(soldBy);
                vendorProduct.NormalizeWallpaperPricing(soldBy);
            }

            if (unit == UnitOfMeasure.Yard)
            {
                // a few are min:6 - everything else is minimum 30
                vendorProduct.MinimumQuantity = 6;

                if (packaging.ContainsIgnoreCase("packaged and sold in 15 yard (202.5 sq ft) bolts"))
                {
                    vendorProduct.MinimumQuantity = 15;
                    vendorProduct.OrderIncrement = 15;
                }

                if (packaging.ContainsIgnoreCase("increments with a 24 yard (216 sq ft) minimum"))
                {
                    vendorProduct.MinimumQuantity = 24;
                    vendorProduct.OrderIncrement = 4;
                }

                if (packaging.ContainsIgnoreCase("30 yard minimum order")) vendorProduct.MinimumQuantity = 30;
                if (packaging.ContainsIgnoreCase("Minimum order 6 yards")) vendorProduct.MinimumQuantity = 6;

                if (packaging.ContainsIgnoreCase("sold by the yard and packaged in 4 yard increments"))
                {
                    vendorProduct.MinimumQuantity = 6;
                    vendorProduct.OrderIncrement = 4;
                }
            }

            vendorProduct.Correlator = data[ScanField.PatternCorrelator];
            vendorProduct.ManufacturerDescription = data[ScanField.Description];
            vendorProduct.Name = new[] {mpn, pattern, color, "by", vendor.DisplayName}.BuildName();
            vendorProduct.SetProductGroup(ProductGroup.Wallcovering);
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = unit;

            return vendorProduct;
        }

        private string FormatContent(string content)
        {
            if (content.Contains("N/A")) return string.Empty;
            return content;
        }
    }
}