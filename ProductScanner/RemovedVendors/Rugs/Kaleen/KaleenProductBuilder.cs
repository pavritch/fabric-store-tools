using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Kaleen
{
    public class KaleenProductBuilder : ProductBuilder<KaleenVendor>
    {
        public KaleenProductBuilder(IPriceCalculator<KaleenVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new KaleenVendor();
            var rugProduct = new RugProduct(vendor);

            var images = data.Variants.SelectMany(x => x.GetScannedImages()).DistinctBy(x => x.Url).ToList();
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, i, rugProduct, images)).ToList();
            rugProduct.AddVariants(vendorVariants);

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);

            rugProduct.Correlator = rugProduct.RugProductFeatures.PatternNumber;

            rugProduct.Name = new[] {rugProduct.RugProductFeatures.PatternNumber, 
                rugProduct.RugProductFeatures.Collection, 
                rugProduct.RugProductFeatures.Color, "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = images;
            return rugProduct;
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var firstVariant = data.Variants.First();

            var features = new RugProductFeatures();
            features.Backing = FindBacking(firstVariant);
            features.Collection = new ItemCollection(firstVariant[ScanField.Collection]).GetFormatted();
            features.Color = new ItemColor(firstVariant[ScanField.ColorName]).GetFormattedColor();

            var allColors = firstVariant[ScanField.Color1].Split(new[] {",", "and", ";"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new ItemColor(x).GetFormattedColor()).ToList();
            features.Colors = allColors;
            features.CountryOfOrigin = new Country(firstVariant[ScanField.Country]).Format();
            features.Description = new List<string>
            {
                firstVariant[ScanField.Description].Replace("â€™", "'").Replace("â€œ", "\"").Replace("â€�", "\"")
            };
            features.Material = new RugMaterial(firstVariant[ScanField.Content]).GetFormattedMaterial();
            features.PatternNumber = firstVariant[ScanField.PatternNumber].Replace(" - ", "-");
            features.Weave = new RugWeave(firstVariant[ScanField.Construction]).GetFormattedWeave();

            var tags = firstVariant[ScanField.Design].Split(new []{';', ','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            features.Tags = new TagList(tags).GetFormattedTags();
            return features;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var size = variant[ScanField.Size];
            var rugDimensions = RugParser.ParseDimensions(size);

            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            var match = images.FirstOrDefault(x => rugDimensions.Shape.ToImageVariantType() == x.ImageVariantType);
            features.ImageFilename = match != null ? match.Id + ".jpg" :
                images.Any() ? images.First().Id + ".jpg" : string.Empty;
            features.PileHeight = Math.Round(variant[ScanField.PileHeight].ToDoubleSafe(), 2);
            features.ShippingHeight = variant[ScanField.PackageHeight].ToDoubleSafe();
            features.ShippingLength = variant[ScanField.PackageLength].ToDoubleSafe();
            features.ShippingWidth = variant[ScanField.PackageWidth].ToDoubleSafe();
            features.ShippingWeight = Math.Round(variant[ScanField.Weight].ToDoubleSafe(), 2);
            features.UPC = variant[ScanField.UPC];

            return new RugVendorVariant(
                variant[ScanField.ManufacturerPartNumber],
                rugDimensions.GetSkuSuffix(),
                variant[ScanField.Cost].ToDecimalSafe(),
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount].ToIntegerSafe() > 0,
                new KaleenVendor(), 
                rugProduct,
                index,
                features);
        }

        private string FindBacking(ScanData data)
        {
            var key = "Backing";
            if (data[ScanField.Bullet1].Contains(key))
                return data[ScanField.Bullet1];
            if (data[ScanField.Bullet2].Contains(key))
                return data[ScanField.Bullet2];
            if (data[ScanField.Bullet3].Contains(key))
                return data[ScanField.Bullet3];
            return string.Empty;
        }
    }
}