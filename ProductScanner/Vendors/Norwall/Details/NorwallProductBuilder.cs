using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Norwall.Details
{
    public class NorwallProductBuilder : ProductBuilder<NorwallVendor>
    {
        public NorwallProductBuilder(IPriceCalculator<NorwallVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var stock = Convert.ToInt32(data[ScanField.StockCount]);

            var vendor = new NorwallVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn.Replace("/", "-")).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(stock), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = FormatCleaning(data[ScanField.AdditionalInfo]);
            vendorProduct.PublicProperties[ProductPropertyType.Color] = data[ScanField.Color];
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection].Split(new[] { ',' }).ToCommaDelimitedList();
            vendorProduct.PublicProperties[ProductPropertyType.Design] = data[ScanField.Design];
            vendorProduct.PublicProperties[ProductPropertyType.Match] = data[ScanField.Match];
            vendorProduct.PublicProperties[ProductPropertyType.Material] = data[ScanField.AdditionalInfo].Contains("Solid Vinyl") ? "Solid Vinyl" : null;
            vendorProduct.PublicProperties[ProductPropertyType.Prepasted] = FormatPrepasted(data[ScanField.AdditionalInfo]);
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ScanField.Style];
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.Repeat].ToInchesMeasurement();

            var dimensions = GetDimensions(data[ScanField.Coverage]);
            vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
            vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
            vendorProduct.PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthInYardsFormatted();

            vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();
            vendorProduct.PrivateProperties[ProductPropertyType.Coordinates] = data[ScanField.Coordinates];
            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.Correlator = data[ScanField.PatternCorrelator];
            vendorProduct.Name = new[] { mpn, "by", vendor.DisplayName }.BuildName();
            vendorProduct.ScannedImages = data.GetScannedImages();
            // looks like all of them are wallpaper
            vendorProduct.ProductClass = ProductClass.Wallpaper;
            vendorProduct.SetProductGroup(ProductGroup.Wallcovering);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();

            // everything is shown/sold as double rolls
            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(2);
                vendorProduct.NormalizeWallpaperPricing(2);
            }

            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Each)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = data[ScanField.Coverage];
            }
            return vendorProduct;
        }

        private RollDimensions GetDimensions(string coverage)
        {
            var values = Regex.Match(coverage, @"(?<length>\d+([.]\d+)?)\s*yds.*x\s*(?<width>\d+([.]\d+)?)");
            var length = values.Groups["length"].Value.ToDoubleSafe();
            var width = values.Groups["width"].Value.ToDoubleSafe();
            return new RollDimensions(width, length * 3 * 12, 2);
        }

        private string FormatCleaning(string additionalInfo)
        {
            var values = new List<string>();
            if (additionalInfo.Contains("Scrubbable")) values.Add("Scrubbable");
            if (additionalInfo.Contains("Peelable")) values.Add("Peelable");
            return values.ToCommaDelimitedList();
        }

        private string FormatPrepasted(string additionalInfo)
        {
            if (additionalInfo == null) return "No";
            return additionalInfo.Contains("Prepasted") ? "Yes" : "No";
        }
    }
}