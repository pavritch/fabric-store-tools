using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using MoreLinq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Chandra
{
    public class ChandraProductBuilder : ProductBuilder<ChandraVendor>
    {
        public ChandraProductBuilder(IPriceCalculator<ChandraVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new ChandraVendor();
            var rugProduct = new RugProduct(vendor);

            var allProductImages = GetImages(data.Variants);
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, i, rugProduct, allProductImages)).ToList();
            rugProduct.AddVariants(vendorVariants);

            var variant = data.Variants.First();
            var patternNumber = variant[ScanField.PatternNumber];

            rugProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = string.Format("http://www.shopchandra.com/product/details/{0}-576", patternNumber.Replace("-", ""));

            rugProduct.Correlator = patternNumber;
            rugProduct.Name = new[] {patternNumber, "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = allProductImages;

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);
            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> allProductImages)
        {
            var dimensions = RugParser.ParseDimensions(variant[ScanField.Dimensions]);
            var match = allProductImages.FirstOrDefault(x => x.Url.Contains(variant[ScanField.PatternNumber] + "_Flat.jpg"));
            var filename = match != null ? match.Id + ".jpg" :
                allProductImages.Any() ? allProductImages.First().Id + ".jpg" : string.Empty;

            var features = RugProductVariantFeaturesBuilder.Build(dimensions);
            features.ImageFilename = filename;
            features.PileHeight = variant[ScanField.PileHeight].TakeOnlyNumericPart();
            features.ShippingHeight = variant[ScanField.Height].ToDoubleSafe();
            features.ShippingLength = variant[ScanField.Length].ToDoubleSafe();
            features.ShippingWidth = variant[ScanField.Width].ToDoubleSafe();
            features.ShippingWeight = variant[ScanField.Weight].ToDoubleSafe();
            features.UPC = variant[ScanField.UPC];

            return new RugVendorVariant(
                variant[ScanField.ManufacturerPartNumber],
                dimensions.GetSkuSuffix(),
                variant[ScanField.Cost].ToDecimalSafe(),
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount].ToDoubleSafe() > 0,
                new ChandraVendor(),
                rugProduct,
                index,
                features);
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var firstVariant = data.Variants.First();

            var colors = firstVariant[ScanField.Color]
                .Replace(",", "/")
                .Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new ItemColor(x))
                .ToList();

            var features = new RugProductFeatures();
            features.Backing = firstVariant[ScanField.Backing];
            features.Collection = firstVariant[ScanField.Collection];
            // Why?
            features.Color = string.Join(" / ", colors.Select(x => x.GetFormattedColor()));
            features.Colors = colors.Select(x => x.GetFormattedColor()).ToList();
            features.CountryOfOrigin = firstVariant[ScanField.Country];
            features.Material = new RugMaterial(firstVariant[ScanField.Material]).GetFormattedMaterial();
            features.PatternNumber = firstVariant[ScanField.PatternNumber];
            features.Weave = new RugWeave(firstVariant[ScanField.Construction]).GetFormattedWeave();

            var tagList = FindTags(firstVariant[ScanField.Description]);
            tagList.Add(firstVariant[ScanField.Type]);
            tagList.Add(firstVariant[ScanField.AdditionalInfo]);
            // interior/exterior
            features.Tags = new TagList(tagList).GetFormattedTags();
            return features;
        }

        private List<string> FindTags(string description)
        {
            description = description.Replace("Trasitional", "Traditional");
            // I don't see any better way to do this than just checking for known values
            var options = new List<string>
            {
                "Contemporary", "Designer", "Shag", "Reversible", "Sisal", "Traditional", "Solid", "Braided", "Thick", "Natural"
            };
            return options.Where(description.ContainsIgnoreCase).ToList();
        }

        private List<ScannedImage> GetImages(List<ScanData> variants)
        {
            var productImages = new List<ScannedImage>();

            // should be only one of each but no guarantee
            var flatImages = variants.Select(x => x[ScanField.Image2]).Where(x => x != string.Empty).Distinct();
            var cornerImages = variants.Select(x => x[ScanField.Image3]).Where(x => x != string.Empty).Distinct();
            var closeupImages = variants.Select(x => x[ScanField.Image4]).Where(x => x != string.Empty).Distinct();
            var styleshotImages = variants.Select(x => x[ScanField.Image5]).Where(x => x != string.Empty).Distinct();
            var roomsceneImages = variants.Select(x => x[ScanField.Image6]).Where(x => x != string.Empty).Distinct();

            MoreEnumerable.ForEach(flatImages, x => productImages.Add(new ScannedImage(GetImageVariantForFlatImage(x), x)));
            MoreEnumerable.ForEach(cornerImages, x => productImages.Add(new ScannedImage(ImageVariantType.Scene, x)));
            MoreEnumerable.ForEach(closeupImages, x => productImages.Add(new ScannedImage(ImageVariantType.Scene, x)));
            MoreEnumerable.ForEach(styleshotImages, x => productImages.Add(new ScannedImage(ImageVariantType.Scene, x)));
            MoreEnumerable.ForEach(roomsceneImages, x => productImages.Add(new ScannedImage(ImageVariantType.Scene, x)));
            return LinqExtensions.DistinctBy(productImages, image => image.Url).ToList();
        }

        private ImageVariantType GetImageVariantForFlatImage(string flatImage)
        {
            if (flatImage.Contains("Flat")) return ImageVariantType.Rectangular;
            // the Round images are actually scenes and not top-down
            if (flatImage.Contains("Round")) return ImageVariantType.Round;
            if (flatImage.Contains("Runner")) return ImageVariantType.Runner;
            if (flatImage.Contains("Closeup")) return ImageVariantType.Scene;

            return ImageVariantType.Rectangular;
        }
    }
}