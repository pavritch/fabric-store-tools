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

namespace Safavieh
{
    // Note with Safavieh we have to adhere to their MAP pricing, plus they only allow certain collections to be sold on line.
    public class SafaviehProductBuilder : ProductBuilder<SafaviehVendor>
    {
        public SafaviehProductBuilder(IPriceCalculator<SafaviehVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new SafaviehVendor();
            var rugProduct = new RugProduct(vendor);

            var allProductImages = data.Variants.SelectMany(x => x.GetScannedImages()).DistinctBy(x => x.Url).ToList();
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, i, allProductImages, rugProduct)).ToList();
            rugProduct.AddVariants(vendorVariants);

            rugProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            var firstVariant = data.Variants.First();
            var sku = firstVariant[ScanField.SKU].Split(new[] {'-'}).First();

            rugProduct.Correlator = firstVariant[ScanField.PatternNumber];
            rugProduct.ManufacturerDescription = data[ScanField.Description];
            rugProduct.Name = new[] {sku, firstVariant[ScanField.Color], "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = allProductImages;

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);
            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, List<ScannedImage> images, RugProduct vendorProduct)
        {
            var rugDimensions = RugParser.ParseDimensions(variant[ScanField.Size]);
            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            features.PileHeight = variant[ScanField.PileHeight].ToDoubleSafe();
            features.UPC = variant[ScanField.UPC];

            var match = images.FirstOrDefault(x => x.Url.Contains(variant[ScanField.ManufacturerPartNumber] + ".jpg"));
            var filename = match != null ? match.Id + ".jpg" :
                images.Any() ? images.First().Id + ".jpg" : string.Empty;
            features.ImageFilename = filename;

            features.ShippingHeight = Math.Round(variant[ScanField.PackageHeight].ToDoubleSafe(), 2);
            features.ShippingLength = Math.Round(variant[ScanField.PackageLength].ToDoubleSafe(), 2);
            features.ShippingWidth = Math.Round(variant[ScanField.PackageWidth].ToDoubleSafe(), 2);
            features.ShippingWeight = Math.Round(variant[ScanField.Weight].ToDoubleSafe(), 2);
            variant.Cost = variant[ScanField.Cost].ToDecimalSafe();

            return new RugVendorVariant(
                variant[ScanField.ManufacturerPartNumber],
                rugDimensions.GetSkuSuffix(),
                variant[ScanField.Cost].ToDecimalSafe(),
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount].ToIntegerSafe() > 0,
                new SafaviehVendor(),
                vendorProduct,
                index,
                features);
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var firstVariant = data.Variants.First();
            var color = CleanColor(firstVariant[ScanField.Color]);
            var colors = color.Split(new[] {'/'}).Select(x => new ItemColor(x).GetFormattedColor()).Distinct().ToList();

            var features = new RugProductFeatures();
            features.CareInstructions = firstVariant[ScanField.Cleaning];
            features.Collection = firstVariant[ScanField.Collection];
            features.Color = string.Join(" / ", colors);
            features.Colors = colors;
            features.CountryOfOrigin = new Country(firstVariant[ScanField.Country]).Format();
            features.Description = RemoveHtml(firstVariant[ScanField.Description]);
            features.PatternNumber = firstVariant[ScanField.PatternNumber];
            features.Weave = new RugWeave(firstVariant[ScanField.Construction]).GetFormattedWeave();

            features.Material = ParseMaterial(firstVariant[ScanField.Material], firstVariant[ScanField.Content]);

            var designTags = FormatDesign(firstVariant[ScanField.Design]);
            designTags.AddRange(firstVariant[ScanField.Style].Replace("&amp;", "&").Split(new []{'&'}));
            features.Tags = new TagList(designTags).GetFormattedTags();
            return features;
        }

        private List<string> FormatDesign(string design)
        {
            design = design.Replace("Bath Mats", "Bath Mat");
            design = design.Replace("Causal", "Casual");

            return design.Split(new[] {'&'}).ToList();
        }

        private List<string> RemoveHtml(string description)
        {
            if (description.Contains("<br>")) return new List<string> {description.Substring(0, description.IndexOf("<br>"))};
            return new List<string> { description };
        }

        private Dictionary<string, int?> ParseMaterial(string material, string content)
        {
            var rugMaterial = new RugMaterial(material);
            if (!string.IsNullOrWhiteSpace(content)) rugMaterial.AddMaterial(content);
            return rugMaterial.GetFormattedMaterial();
        }

        private string CleanColor(string color)
        {
            // Front/Back are sometimes labeled
            color = color.ReplaceWholeWord("B", "/");
            color = color.ReplaceWholeWord("F", "/");
            return color;
        }
    }
}