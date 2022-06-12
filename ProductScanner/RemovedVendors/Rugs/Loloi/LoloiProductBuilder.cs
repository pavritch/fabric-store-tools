using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Loloi
{
    public class LoloiProductValidator : DefaultProductValidator<LoloiVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            if ((product as RugProduct).RugProductFeatures.Weave.ContainsIgnoreCase("Hand Knotted") ||
                (product as RugProduct).RugProductFeatures.Collection.ContainsIgnoreCase("Majestic"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.HandKnotted);
            }
            return validation;
        }
    }

    public class LoloiPriceCalculator : IPriceCalculator<LoloiVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            var retail = data[ScanField.RetailPrice].ToDecimalSafe();
            return new ProductPriceData(map, Math.Round(map * 1.5m, 2));
        }
    }

    public class LoloiProductBuilder : ProductBuilder<LoloiVendor>
    {
        public LoloiProductBuilder(IPriceCalculator<LoloiVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new LoloiVendor();
            var firstVariantMpn = data.Variants.First()[ScanField.ManufacturerPartNumber];
            var mpn = firstVariantMpn.Substring(0, firstVariantMpn.Length - 4);
            var rugProduct = new RugProduct(vendor);

            var images = new List<ScannedImage> { new ScannedImage(ImageVariantType.Rectangular, data.Variants.First()[ScanField.ImageUrl])};
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(mpn, x, i, rugProduct, images)).ToList();
            rugProduct.AddVariants(vendorVariants);

            rugProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            rugProduct.RugProductFeatures = BuildRugProductFeatures(data.Variants.First());
            rugProduct.Correlator = rugProduct.RugProductFeatures.PatternNumber;
            rugProduct.Name = new[] {rugProduct.RugProductFeatures.PatternNumber, rugProduct.RugProductFeatures.Collection, 
                rugProduct.RugProductFeatures.Color, "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = images;

            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(string mpn, ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var size = variant[ScanField.Size];
            var rugDimensions = RugParser.ParseDimensions(size, variant[ScanField.Shape].GetShape());

            var match = images.FirstOrDefault(x => x.Url.Replace("-", "").ContainsIgnoreCase(variant[ScanField.ManufacturerPartNumber].Replace("-", "")));
            var filename = match != null ? match.Id + ".jpg" :
                images.Any() ? images.First().Id + ".jpg" : string.Empty;

            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            features.ImageFilename = filename;
            variant.Cost = variant[ScanField.RetailPrice].ToDecimalSafe()/3;

            return new RugVendorVariant(
                mpn + rugDimensions.GetSkuSuffix(),
                rugDimensions.GetSkuSuffix(),
                variant.Cost,
                PriceCalculator.CalculatePrice(variant),
                true,
                new LoloiVendor(), 
                rugProduct,
                index,
                features);
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var features = new RugProductFeatures();

            features.CareInstructions = data[ScanField.Cleaning];
            features.Collection = new ItemCollection(data[ScanField.Collection]).GetFormatted();
            features.Description = new List<string> {data[ScanField.Description]};

            var colors = data[ScanField.Color].Split(new[] {'/'}).ToList();
            features.Colors = colors.Select(x => new ItemColor(x).GetFormattedColor()).ToList();
            features.Color = string.Join(", ", features.Colors);
            features.PatternNumber = data[ScanField.PatternNumber];

            features.CountryOfOrigin = new Country(data[ScanField.Country]).Format();
            features.Material = new RugMaterial(data[ScanField.Material]).GetFormattedMaterial();
            features.Weave = new RugWeave(data[ScanField.Construction]).GetFormattedWeave();

            features.ColorGroup = data[ScanField.ColorGroup];

            features.Tags = new TagList(new List<string>{data[ScanField.Style]}).GetFormattedTags();
            return features;
        }
    }
}