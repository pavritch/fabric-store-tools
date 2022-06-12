using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Sphinx
{
    public class SphinxProductBuilder : ProductBuilder<SphinxVendor>
    {
        public SphinxProductBuilder(IPriceCalculator<SphinxVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new SphinxVendor();
            var rugProduct = new RugProduct(vendor);
            var allImages = data.Variants.SelectMany(x => x.GetScannedImages()).DistinctBy(x => x.Url).ToList();
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, i, rugProduct, allImages)).ToList();
            rugProduct.AddVariants(vendorVariants);

            var firstVariant = data.Variants.First();
            var collection = firstVariant[ScanField.Collection].TitleCase();

            rugProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = firstVariant.GetDetailUrl();

            rugProduct.Correlator = firstVariant[ScanField.Pattern];
            rugProduct.Name = new[] {collection, firstVariant[ScanField.Pattern], "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = allImages;

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);
            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var rugDimensions = RugParser.ParseDimensions(variant[ScanField.Description]);
            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            features.UPC = variant[ScanField.UPC];

            var match = images.FirstOrDefault(x => rugDimensions.Shape.ToImageVariantType() == x.ImageVariantType);
            features.ImageFilename = match != null ? match.Id + ".jpg" :
                images.Any() ? images.First().Id + ".jpg" : string.Empty;

            return new RugVendorVariant(
                variant[ScanField.ManufacturerPartNumber],
                rugDimensions.GetSkuSuffix(),
                variant[ScanField.Cost].ToDecimalSafe(),
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount].ToDoubleSafe() > 0,
                new SphinxVendor(),
                rugProduct,
                index,
                features);
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var firstVariant = data.Variants.First();
            var collection = firstVariant[ScanField.Collection].TitleCase();

            var features = new RugProductFeatures();

            // contains codes like ST, SQ, but we're not sure what they represent
            //features.Backing = firstVariant[ScanField.Backing];
            features.Collection = collection;
            features.Color = firstVariant[ScanField.Color];
            features.Colors = new List<string> { features.Color };
            features.ColorGroup = firstVariant[ScanField.ColorGroup];
            // only contains countries for a few products that are marked as "Made in USA"
            features.CountryOfOrigin = firstVariant[ScanField.Country];
            features.Material = new RugMaterial(firstVariant[ScanField.Content]).GetFormattedMaterial();
            features.PatternNumber = firstVariant[ScanField.Pattern];
            features.Weave = new RugWeave(firstVariant[ScanField.Construction]).GetFormattedWeave();

            features.Tags = new TagList(new List<string>
            {
                firstVariant[ScanField.Style],
                firstVariant[ScanField.Design],
            }).GetFormattedTags();

            return features;
        }
    }
}