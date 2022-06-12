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

namespace Rizzy
{
    public class RizzyProductBuilder : ProductBuilder<RizzyVendor>
    {
        public RizzyProductBuilder(IPriceCalculator<RizzyVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new RizzyVendor();
            var rugProduct = new RugProduct(vendor);
            var images = data.Variants.SelectMany(x => x.GetScannedImages()).DistinctBy(x => x.Url).ToList();

            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, i, rugProduct, images)).ToList();
            rugProduct.AddVariants(vendorVariants);

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);

            var patternNumber = rugProduct.RugProductFeatures.PatternNumber;
            var patternName = rugProduct.RugProductFeatures.PatternName;
            var color = rugProduct.RugProductFeatures.Color;

            rugProduct.Correlator = patternNumber;
            rugProduct.Name = new[] {patternNumber, patternName, color, "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = images;

            rugProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var size = variant[ScanField.Size];
            var rugDimensions = RugParser.ParseDimensions(size);
            var variantMpn = variant[ScanField.SKU] + rugDimensions.GetSkuSuffix();

            var match = images.FirstOrDefault(x => x.Url.Replace("-", "").ContainsIgnoreCase(variantMpn));
            var filename = match != null ? match.Id + ".jpg" :
                images.Any() ? images.First().Id + ".jpg" : string.Empty;

            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            features.ImageFilename = filename;
            features.UPC = variant[ScanField.UPC];
            features.ShippingLength = variant[ScanField.PackageLength].ToDoubleSafe();
            features.ShippingHeight = variant[ScanField.PackageHeight].ToDoubleSafe();
            features.ShippingWeight = variant[ScanField.Weight].ToDoubleSafe();
            features.ShippingWidth = variant[ScanField.PackageWidth].ToDoubleSafe();
            // convert millimeters to inches
            features.PileHeight = FormatPileHeight(variant[ScanField.PileHeight]);

            return new RugVendorVariant(
                variantMpn,
                rugDimensions.GetSkuSuffix(),
                variant[ScanField.Cost].ToDecimalSafe(),
                PriceCalculator.CalculatePrice(variant),
                true,
                // not sure what's goin on with stock - it's showing most as OOS
                // there's also an inventory spreadsheet, so I should review that more
                //variant[ProductPropertyType.StockCount].ToDoubleSafe() > 0, 
                new RizzyVendor(),
                rugProduct,
                index,
                features);
        }

        private double FormatPileHeight(string pileHeight)
        {
            if (pileHeight == "N/A") return 0;
            if (pileHeight == "Zero Pile") return 0;
            return pileHeight.Replace("\"", "").ToDoubleSafe();
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var firstVariant = data.Variants.First();
            var material = new RugMaterial(FormatMaterial(firstVariant[ScanField.Material]));
            if (firstVariant[ScanField.SecondaryMaterials] != "None") material.AddMaterial(firstVariant[ScanField.SecondaryMaterials]);

            var colors = firstVariant[ScanField.Color].Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new ItemColor(x.Trim()).GetFormattedColor())
                .ToList();

            var features = new RugProductFeatures();
            features.CareInstructions = firstVariant[ScanField.Cleaning];
            features.Collection = firstVariant[ScanField.PatternName].TitleCase();
            features.Color = string.Join(" / ", colors);
            features.Colors = colors;
            features.CountryOfOrigin = firstVariant[ScanField.Country];
            features.Description = new List<string> { firstVariant[ScanField.Description].Replace("’", "'").Replace("é", "e") };
            features.Material = material.GetFormattedMaterial();
            features.PatternName = firstVariant[ScanField.PatternName];
            features.PatternNumber = firstVariant[ScanField.PatternNumber];
            features.Weave = new RugWeave(firstVariant[ScanField.Construction]).GetFormattedWeave();

            features.Tags = new TagList(new List<string> {firstVariant[ScanField.Style]}).GetFormattedTags();
            return features;
        }

        // clean up material so normal processing works correctly
        private string FormatMaterial(string material)
        {
            material = material.Replace(",", " ");
            return material;
        }
    }
}