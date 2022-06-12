using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Nourison
{
    public class NourisonProductBuilder : ProductBuilder<NourisonVendor>
    {
        public NourisonProductBuilder(IPriceCalculator<NourisonVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new NourisonVendor();
            var rugProduct = new RugProduct(vendor);

            var images = data.GetScannedImages();
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(data[ScanField.ManufacturerPartNumber], x, i, rugProduct, images)).ToList();
            rugProduct.AddVariants(vendorVariants);

            rugProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);

            rugProduct.Correlator = data[ScanField.ManufacturerPartNumber];
            rugProduct.Name = new[] {rugProduct.RugProductFeatures.Collection, rugProduct.RugProductFeatures.PatternNumber, 
                rugProduct.RugProductFeatures.Color, "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = images;
            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(string mpn, ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var rugDimensions = RugParser.ParseDimensions(variant[ScanField.Size],
                variant[ScanField.Shape].ToProductShape());

            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            if (images.Any()) features.ImageFilename = images.First().Url;

            return new RugVendorVariant(
                mpn + rugDimensions.GetSkuSuffix(),
                rugDimensions.GetSkuSuffix(),
                variant[ScanField.Cost].ToDecimalSafe(),
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount] == "Call" || variant[ScanField.StockCount].ToIntegerSafe() > 0,
                new NourisonVendor(), 
                rugProduct,
                index,
                features);
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var features = new RugProductFeatures();
            features.Collection = data[ScanField.Collection].TitleCase();
            features.Description = new List<string> {data[ScanField.Description]};
            features.Color = new ItemColor(ReplaceAbbreviations(data[ScanField.ColorName].TitleCase())).GetFormattedColor();
            features.ColorGroup = data[ScanField.ColorGroup].TitleCase();
            features.Material = new RugMaterial(data[ScanField.Content]).GetFormattedMaterial();
            features.PatternNumber = data[ScanField.PatternNumber];

            var tags = new List<string> {data[ScanField.Category].Replace("-", " ")};
            tags.Remove("New Rugs");
            features.Tags = new TagList(tags).GetFormattedTags();
            //features.Weave = new RugWeave(data[ScanField.TempContent1]).GetFormattedWeave();
            return features;
        }

        private string ReplaceAbbreviations(string color)
        {
            color = color.Replace("AQUBR", "Aqua/Brown");
            color = color.Replace("BBK", "Brown/Black");
            color = color.Replace("BGEBK", "Beige/Black");
            color = color.Replace("BKBGE", "Black/Beige");
            color = color.Replace("BKW", "Black/White");
            color = color.Replace("BGEBN", "Beige/Brown");
            color = color.Replace("BGEGD", "Beige/Gold");
            color = color.Replace("BGG", "Beige/Green");
            color = color.Replace("BIS", "Biscuit");
            color = color.Replace("BISQU", "Bisque");
            return color;
        }
    }
}