using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Dalyn
{
    public class DalynProductBuilder : ProductBuilder<DalynVendor>
    {
        public DalynProductBuilder(IPriceCalculator<DalynVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new DalynVendor();
            var rugProduct = new RugProduct(vendor);

            var images = data.Variants.SelectMany(x => x.GetScannedImages()).DistinctBy(x => x.Url).ToList();
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, i, rugProduct, images)).ToList();
            rugProduct.AddVariants(vendorVariants);

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);

            var patternNumber = rugProduct.RugProductFeatures.PatternNumber;
            var color = rugProduct.RugProductFeatures.Color;
            var collection = rugProduct.RugProductFeatures.Collection;

            rugProduct.Correlator = patternNumber + "-" + color;
            rugProduct.Name = new[] {collection, patternNumber, color, "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = images;
            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var rugDimensions = RugParser.ParseDimensions(variant[ScanField.Size]);

            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            if (images.Any()) features.ImageFilename = images.First().Url;
            features.PileHeight = variant[ScanField.PileHeight].Replace("\"", "").ToDoubleSafe();
            features.ShippingHeight = variant[ScanField.PackageHeight].ToDoubleSafe();
            features.ShippingLength = variant[ScanField.PackageLength].ToDoubleSafe();
            features.ShippingWidth = variant[ScanField.PackageWidth].ToDoubleSafe();
            features.ShippingWeight = variant[ScanField.Weight].ToDoubleSafe();
            features.UPC = variant[ScanField.UPC];

            return new RugVendorVariant(
                variant[ScanField.ManufacturerPartNumber],
                rugDimensions.GetSkuSuffix(),
                variant.Cost,
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount].ToIntegerSafe() > 0,
                new DalynVendor(), 
                rugProduct,
                index,
                features);
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var firstVariant = data.Variants.First();

            var features = new RugProductFeatures();
            features.Backing = firstVariant[ScanField.Backing];
            features.CareInstructions = firstVariant[ScanField.Cleaning];
            features.Collection = firstVariant[ScanField.Collection].TitleCase();
            features.CountryOfOrigin = new Country(firstVariant[ScanField.Country]).Format();
            features.Description = new List<string> {firstVariant[ScanField.Description]};
            features.Material = new RugMaterial(firstVariant[ScanField.Content]).GetFormattedMaterial();

            var colors = firstVariant[ScanField.ColorGroup].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new ItemColor(x).GetFormattedColor()).ToList();
            colors.Add(new ItemColor(firstVariant[ScanField.Color]).GetFormattedColor());
            features.Colors = colors;
            features.Color = new ItemColor(firstVariant[ScanField.Color]).GetFormattedColor();
            features.PatternNumber = firstVariant[ScanField.PatternNumber];
            features.Tags = new TagList(new List<string>
            {
                firstVariant[ScanField.Style1],
                firstVariant[ScanField.Style2],
                firstVariant[ScanField.Design],
            }).GetFormattedTags();
            features.Weave = new RugWeave(firstVariant[ScanField.Construction]).GetFormattedWeave();
            return features;
        }
    }
}