using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Momeni
{
    public class MomeniProductBuilder : ProductBuilder<MomeniVendor>
    {
        public MomeniProductBuilder(IPriceCalculator<MomeniVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new MomeniVendor();
            var rugProduct = new RugProduct(vendor);

            var imageIds = data.Variants.Select(x => x[ScanField.SKU]);
            var images = imageIds.SelectMany(BuildImages).DistinctBy(x => x.Url).ToList();
            //var images = data.Variants.SelectMany(x => x.GetScannedImages()).DistinctBy(x => x.Url).ToList();
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

        private List<ScannedImage> BuildImages(string imageId)
        {
            var images = new List<ScannedImage>();
            images.Add(new ScannedImage(ImageVariantType.Rectangular, string.Format("http://scanner.insidefabric.com/vendors/momeni/1A/{0}.jpg", imageId)));
            images.Add(new ScannedImage(ImageVariantType.Scene, string.Format("http://scanner.insidefabric.com/vendors/momeni/1A/{0}_1.jpg", imageId)));
            images.Add(new ScannedImage(ImageVariantType.Scene, string.Format("http://scanner.insidefabric.com/vendors/momeni/1A/{0}_2.jpg", imageId)));
            //images.Add(new ScannedImage(ImageVariantType.Alternate, string.Format("http://scanner.insidefabric.com/vendors/momeni/1A/{0}_3.jpg", imageId)));
            return images;
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var firstVariant = data.Variants.First();
            // missing properties: Backing, Designer, ColorGroup

            var colorList = firstVariant[ScanField.Color].Replace("][", ",").Replace("[", "").Replace("]", "");
            var colors = colorList.Split(new[] {','}).ToList();
            var allColors = colors.Where(x => !string.IsNullOrEmpty(x)).Select(x => new ItemColor(x)).ToList();
            allColors.Add(new ItemColor(firstVariant[ScanField.Color1].Replace("L.", "Light ").Replace("Midnight B", "Midnight Blue")));

            var features = new RugProductFeatures();
            features.Collection = new ItemCollection(firstVariant[ScanField.Collection]).GetFormatted();
            features.Color = new ItemColor(firstVariant[ScanField.Color1].Replace("L.", "Light ").Replace("Midnight B", "Midnight Blue")).GetFormattedColor();
            features.Colors = allColors.Select(x => x.GetFormattedColor()).ToList();
            features.CountryOfOrigin = new Country(firstVariant[ScanField.Country]).Format();
            features.Description = new List<string> {firstVariant[ScanField.Description]};
            features.Material = new RugMaterial(firstVariant[ScanField.Content]).GetFormattedMaterial();
            features.PatternNumber = firstVariant[ScanField.PatternNumber];

            var tags = firstVariant[ScanField.Category];
            features.Tags = tags.Replace("][", ",").Replace("[", "").Replace("]", "").Replace(" Rugs", "")
                .Split(new []{",", "/"}, StringSplitOptions.RemoveEmptyEntries).ToList();
            features.Tags = new TagList(features.Tags).GetFormattedTags();
            features.Weave = new RugWeave(RemoveCountries(firstVariant[ScanField.Construction])).GetFormattedWeave();
            return features;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var size = variant[ScanField.Size];
            var rugDimensions = RugParser.ParseDimensions(size);

            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            features.ImageFilename = FindFilename(rugDimensions.Shape, images);
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
                variant[ScanField.StockCount].ToDoubleSafe() > 0,
                new MomeniVendor(), 
                rugProduct,
                index,
                features);
        }

        private string FindFilename(ProductShapeType shape, List<ScannedImage> images)
        {
            // the images are per-product, so we need to try to match up based on shape
            if (!images.Any()) return string.Empty;

            ScannedImage match = null;
            var filename = images.First().Id + ".jpg";
            if (shape == ProductShapeType.Runner) match = images.FirstOrDefault(x => x.Url.ContainsIgnoreCase("Runner"));
            if (shape == ProductShapeType.Round) match = images.FirstOrDefault(x => x.Url.ContainsIgnoreCase("Round"));
            if (shape == ProductShapeType.Rectangular) match = images.FirstOrDefault(x => x.Url.ContainsIgnoreCase("Rectangular"));
            if (match != null) return match.Id + ".jpg";
            return filename;
        }

        private string RemoveCountries(string construction)
        {
            construction = construction.TitleCase();
            construction = construction.Replace("Belgium", "").Trim();
            construction = construction.Replace("Chinese", "").Trim();
            construction = construction.Replace("Egyptian", "").Trim();
            construction = construction.Replace("Indian", "").Trim();
            construction = construction.Replace("Turkish", "").Trim();
            return construction;
        }
    }
}