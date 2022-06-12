using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Dynamic
{
    public class DynamicProductBuilder : ProductBuilder<DynamicVendor>
    {
        private const string ImageBaseUrl = "http://scanner.insidefabric.com/vendors/dynamic/";
        public DynamicProductBuilder(IPriceCalculator<DynamicVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new DynamicVendor();
            var rugProduct = new RugProduct(vendor);

            var imageUrls = data.Variants.Select(x => string.Format("{0}{1}/{2}", ImageBaseUrl, x[ScanField.Collection], x[ScanField.Image1]));
            var images = imageUrls.Distinct().Select(x => new ScannedImage(ImageVariantType.Rectangular, x)).ToList();

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
            features.CareInstructions = firstVariant[ScanField.Cleaning];
            features.Collection = new ItemCollection(firstVariant[ScanField.Collection]).GetFormatted();
            features.CountryOfOrigin = new Country(firstVariant[ScanField.Country]).Format();
            features.Description = new List<string> { firstVariant[ScanField.Description] };
            features.Material = new RugMaterial(firstVariant[ScanField.Material]).GetFormattedMaterial();
            features.PatternNumber = firstVariant[ScanField.Design];
            features.Warranty = firstVariant[ScanField.Warranty].TitleCase();
            features.Weave = new RugWeave(firstVariant[ScanField.Construction]).GetFormattedWeave();

            var colors = firstVariant[ScanField.ColorName].Split(new[] {'/', '-'}).Select(x => new ItemColor(x).GetFormattedColor())
                .Distinct().ToList();
            features.Colors = colors;
            features.Color = string.Join(", ", colors);

            var tags = new List<string> {firstVariant[ScanField.Style].Trim()};
            if (firstVariant[ScanField.Style2] != "n/a") tags.Add(firstVariant[ScanField.Style2]);
            tags.Add(firstVariant[ScanField.Bullet1]);
            tags.Add(firstVariant[ScanField.Bullet2]);
            tags.Add(firstVariant[ScanField.Bullet3]);
            tags = tags.Where(x => !x.Contains("Wool") && !x.Contains("Backing") && !x.Contains("Polyprop")).ToList();
            features.Tags = new TagList(tags).GetFormattedTags();
            return features;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var size = variant[ScanField.Size];
            var rugDimensions = RugParser.ParseDimensions(size);

            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            features.ImageFilename = images.First().Url;
            features.PileHeight = Math.Round(variant[ScanField.PileHeight].ToDoubleSafe(), 2);
            features.ShippingHeight = variant[ScanField.PackageLength].ToDoubleSafe();
            features.ShippingLength = variant[ScanField.PackageLength].ToDoubleSafe();
            features.ShippingWidth = variant[ScanField.PackageWidth].ToDoubleSafe();
            features.ShippingWeight = Math.Round(variant[ScanField.Weight].ToDoubleSafe(), 2);
            features.UPC = variant[ScanField.UPC];

            return new RugVendorVariant(
                variant[ScanField.SKU],
                rugDimensions.GetSkuSuffix(),
                variant[ScanField.Cost].ToDecimalSafe(),
                PriceCalculator.CalculatePrice(variant),
                true, // I don't see anything about stock anywhere - I think everything in the sheet is in stock
                new DynamicVendor(), 
                rugProduct,
                index,
                features);
        }
    }
}