using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace TheRugMarket
{
    public class TheRugMarketProductBuilder : ProductBuilder<TheRugMarketVendor>
    {
        public TheRugMarketProductBuilder(IPriceCalculator<TheRugMarketVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new TheRugMarketVendor();
            var rugProduct = new RugProduct(vendor);

            // every image is rectangular
            var images = data.Variants.DistinctBy(x => x[ScanField.ImageUrl])
                .Select(x => new ScannedImage(ImageVariantType.Rectangular, x[ScanField.ImageUrl])).ToList();

            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, i, images, rugProduct)).ToList();
            rugProduct.AddVariants(vendorVariants);

            var patternNumber = data[ScanField.PatternNumber];
            var patternName = FormatPatternName(data[ScanField.PatternName]);
            //rugProduct.Correlator = patternName;
            //if (patternName == string.Empty)
                rugProduct.Correlator = patternNumber;
            rugProduct.Name = new[] {patternName, patternNumber, "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = images.ToList();

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);
            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, List<ScannedImage> allProductImages, RugProduct rugProduct)
        {
            var rugDimensions = RugParser.ParseDimensions(variant[ScanField.Size]);
            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            var split = variant[ScanField.ShippingMethod].ToLower().Split(new[] {"x"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (allProductImages.Any())
            {
                features.ImageFilename = allProductImages.First().Id + ".jpg";
            }

            features.ShippingLength = split[0].ToDoubleSafe();
            features.ShippingWidth = split[1].ToDoubleSafe();
            features.ShippingHeight = split[2].ToDoubleSafe();
            features.ShippingWeight = variant[ScanField.Weight].ToDoubleSafe();
            features.UPC = variant[ScanField.UPC];
            return new RugVendorVariant(
                variant[ScanField.ManufacturerPartNumber],
                rugDimensions.GetSkuSuffix(),
                variant[ScanField.Cost].ToDecimalSafe(),
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount].ToIntegerSafe() > 0,
                new TheRugMarketVendor(),
                rugProduct,
                index,
                features);
        }

        public RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var patternName = FormatPatternName(data[ScanField.PatternName]);
            var colors = data[ScanField.Color].Split(new[] {',', '/'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new ItemColor(x).GetFormattedColor())
                .ToList();

            var features = new RugProductFeatures();
            features.Collection = data[ScanField.Collection].TitleCase();
            features.Color = string.Join("/", colors);
            features.Colors = colors;
            features.Description = new List<string> {data[ScanField.Description]};
            features.Material = new RugMaterial(FormatMaterial(data[ScanField.Material].TitleCase().Replace("/", ", "))).GetFormattedMaterial();
            features.PatternName = patternName;
            features.Weave = new RugWeave(FormatWeave(data[ScanField.Construction])).GetFormattedWeave();

            features.Tags = new TagList(data[ScanField.Category].Split(new []{'|'}, StringSplitOptions.RemoveEmptyEntries).ToList()).GetFormattedTags();

            return features;
        }

        private string FormatMaterial(string material)
        {
            material = material.TitleCase();
            material = material.Replace("(premium)", "");
            material = material.Replace("(Taba)", "");
            material = material.Replace("Symthetic", "Synthetic");
            material = material.Replace("Senthetic", "Synthetic");
            material = material.Replace("Nz Wool", "New Zealand Wool");
            material = material.Replace("Mod Acrylic", "Mod-Acrylic");
            material = material.Replace("Micro-", "Micro");

            return material.Trim().TitleCase().Replace("/", ", ").Replace(" & ", ", ").Replace(".", " ");
        }

        private string FormatWeave(string weave)
        {
            weave = weave.Replace(" & ", "/");
            weave = weave.Replace(" + ", "/");
            return weave;
        }

        private string FormatPatternName(string patternName)
        {
            patternName = patternName
                .Replace("1-", " ")
                .Replace("2-", " ")
                .Replace("3-", " ")
                .TitleCase()
                .Replace("Lt.", "Light ")
                .Replace("Lt ", "Light ");
            return patternName;
        }
    }
}
