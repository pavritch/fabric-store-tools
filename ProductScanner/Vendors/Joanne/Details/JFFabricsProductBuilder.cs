using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace JFFabrics.Details
{
    public class JFFabricsProductBuilder : ProductBuilder<JFFabricsVendor>
    {
        public JFFabricsProductBuilder(IPriceCalculator<JFFabricsVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var cost = data[ScanField.Cost].ToDecimalSafe();
            data.Cost = cost;

            var stock = data[ScanField.StockCount];
            var pattern = data[ScanField.PatternName];
            var patternName = data[ScanField.PatternName].ContainsDigit() ? "" : data[ScanField.PatternName].TitleCase();
            var vendor = new JFFabricsVendor();

            var vendorProduct = new FabricProduct(mpn, cost, new StockData(Convert.ToBoolean(stock)), vendor);

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var hr = data[ScanField.HorizontalRepeat];
            var vr = data[ScanField.VerticalRepeat];
            var width = data[ScanField.Width].ToDecimalSafe();

            vendorProduct.PublicProperties[ProductPropertyType.Book] = data[ScanField.Book].Replace("&amp;", "&").TitleCase().RomanNumeralCase();
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content];
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(data[ScanField.Durability]);
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = FormatFinish(data[ScanField.Finish]);
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = hr == "N/A" ? string.Empty : hr + " inches";
            vendorProduct.PublicProperties[ProductPropertyType.Match] = data[ScanField.Match].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternName].ContainsDigit() ? data[ScanField.PatternName] : "";
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.ProductUse].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = data[ScanField.Railroaded] == "Y" ? "Yes" : "No";
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = vr == "N/A" ? string.Empty : (vr + " inches");
            vendorProduct.PublicProperties[ProductPropertyType.Width] = width + " inches";


            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.Category].TitleCase().Replace(",", ", ").Trim(new[] {',', ' '});
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = FormatCleaning(data[ScanField.Cleaning]);
            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = data[ScanField.Color];
            vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = FormatColorGroup(data[ScanField.ColorGroup]);
            vendorProduct.PublicProperties[ProductPropertyType.Design] = FormatDesign(data[ScanField.Design]);
            vendorProduct.PublicProperties[ProductPropertyType.Flammability] = data[ScanField.Flammability];

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            var productGroup = GetProductGroup(data[ScanField.ProductGroup]);
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).Replace("_", "-").SkuTweaks();
            vendorProduct.Correlator = pattern;
            vendorProduct.Name = new[] {pattern, data[ScanField.Color], "by", vendor.DisplayName}.BuildName();
            vendorProduct.SetProductGroup(productGroup);
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = GetUnitOfMeasure(vendorProduct.ProductGroup);

            if (productGroup == ProductGroup.Wallcovering)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Length] = "10.94 yards";

                var coverage = GetCoverage(data[ScanField.Coverage]);
                var dimensions = new RollDimensions((double)width, 394);
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = coverage == 0 ? string.Empty : coverage + " square feet";
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
            }

            // everything is shown/sold as double rolls
            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(2);
                vendorProduct.NormalizeWallpaperPricing(2);
            }
            return vendorProduct;
        }

        private decimal GetCoverage(string coverage)
        {
            return coverage.TakeOnlyFirstDecimalToken(2);
        }

        private string FormatCleaning(string cleaning)
        {
            if (cleaning.ContainsIgnoreCase("Dry Clean")) return "Dry Clean";
            if (cleaning.ContainsIgnoreCase("Handwash")) return "Handwash";
            if (cleaning.ContainsIgnoreCase("Handwash/Dry Clean")) return "Handwash/Dry Clean";
            if (cleaning.ContainsIgnoreCase("Washable")) return "Washable";
            return string.Empty;
        }

        private string FormatDesign(string design)
        {
            return design.TitleCase().Replace(",", ", ").Trim(new[] {',', ' '});
        }

        private string FormatColorGroup(string colorGroup)
        {
            return colorGroup.TitleCase().Replace(",", ", ").Trim(new[] {',', ' '});
        }

        private string FormatFinish(string finish)
        {
            if (finish == "N/A") return string.Empty;
            return finish.TitleCase();
        }

        private ProductGroup GetProductGroup(string group)
        {
            if (group == "WALLPAPER") return ProductGroup.Wallcovering;
            if (group == "WALLCOVERING") return ProductGroup.Wallcovering;
            return ProductGroup.Fabric;
        }

        private UnitOfMeasure GetUnitOfMeasure(ProductGroup group)
        {
            return group == ProductGroup.Wallcovering ? UnitOfMeasure.Roll : UnitOfMeasure.Yard;
        }

        private string FormatDurability(string durability)
        {
            if (durability == "N/A") return string.Empty;

            durability = durability.Replace(" DR Wyzen. Cotton Duck", " Wyzenbeek Double Rubs");
            durability = durability.Replace(" DR Martin. Cotton Duck", " Martinique Double Rubs");
            return durability;
        }
    }
}
