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

namespace Jaipur
{
    public class JaipurProductValidator : DefaultProductValidator<JaipurVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            if ((product as RugProduct).RugProductFeatures.Designer.ContainsIgnoreCase("Kate Spade"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }
            return validation;
        }
    }

    public class JaipurPriceCalculator : IPriceCalculator<JaipurVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            if (map == 0) return GetDefaultPrice(data.Cost);
            var msrp = Math.Round(map*1.7M, 2);
            return new ProductPriceData(map, msrp);
        }

        private ProductPriceData GetDefaultPrice(decimal cost)
        {
            var vendor = new JaipurVendor();
            var roundedCost = Math.Round(cost, 2);
            return new ProductPriceData(roundedCost * vendor.OurPriceMarkup, roundedCost * vendor.RetailPriceMarkup);
        }
    }


    public class JaipurProductBuilder : ProductBuilder<JaipurVendor>
    {
        public JaipurProductBuilder(IPriceCalculator<JaipurVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new JaipurVendor();
            var rugProduct = new RugProduct(vendor);

            var images = data.GetScannedImages();
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, i, rugProduct, images)).ToList();
            rugProduct.AddVariants(vendorVariants);

            rugProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);
            rugProduct.Correlator = data[ScanField.SKU];
            rugProduct.Name = new[] {data[ScanField.SKU], rugProduct.RugProductFeatures.Collection, 
                rugProduct.RugProductFeatures.Color, "by", vendor.DisplayName}.BuildName();
            rugProduct.ScannedImages = data.GetScannedImages();

            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var size = variant[ScanField.Size];
            var rugDimensions = RugParser.ParseDimensions(size, variant[ScanField.Shape].GetShape());

            var match = images.FirstOrDefault(x => x.Url.Replace("-", "").ContainsIgnoreCase(variant[ScanField.ManufacturerPartNumber].Replace("-", "")));
            var filename = match != null ? match.Id + ".jpg" :
                images.Any() ? images.First().Id + ".jpg" : string.Empty;

            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            features.ImageFilename = filename;
            features.PileHeight = FormatPileHeight(variant[ScanField.PileHeight]);

            return new RugVendorVariant(
                variant[ScanField.ManufacturerPartNumber],
                rugDimensions.GetSkuSuffix(),
                variant.Cost,
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount].ToIntegerSafe() > 0,
                new JaipurVendor(),
                rugProduct,
                index,
                features);
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var features = new RugProductFeatures();

            var color = FixMisspellings(data[ScanField.Color]);
            var colors = color
                .Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new ItemColor(x))
                .ToList();

            var collectionAndDesigner = ParseProductName(data[ScanField.ProductName], data[ScanField.SKU]);

            features.CareInstructions = data[ScanField.Cleaning]
                .Replace("Ã¢â‚¬â€œ", "");
            features.Collection = collectionAndDesigner.Item1;
            features.Color = color;
            features.Colors = colors.Select(x => x.GetFormattedColor()).ToList();
            features.ColorGroup = data[ScanField.ColorGroup].Replace(",", ", ").Replace("Ble", "Blue");
            features.CountryOfOrigin = new Country(data[ScanField.Country]).Format();
            features.Description = FormatDescription(data[ScanField.Description]);
            features.Designer = data[ScanField.Designer] == string.Empty ? collectionAndDesigner.Item2 : data[ScanField.Designer];
            features.PatternNumber = data[ScanField.SKU];
            features.PatternName = data[ScanField.Design];
            features.Material = new RugMaterial(data[ScanField.Content].ReplaceWholeWord("Tw", "Twisted").Replace("Wool13", "Wool 13")).GetFormattedMaterial();
            features.Weave = new RugWeave(data[ScanField.Construction]).GetFormattedWeave();

            var styleTags = data[ScanField.Style].Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var tags = new List<string> { data[ScanField.Pattern].Replace("NA", "") };
            tags.AddRange(styleTags);
            var additionalTags = data[ScanField.AdditionalInfo];
            if (additionalTags.Contains("durable.png")) tags.Add("Durable");
            if (additionalTags.Contains("easy-care.png")) tags.Add("Easy Care");
            if (additionalTags.Contains("plush-pile.png")) tags.Add("Plush Pile");
            if (additionalTags.Contains("textured.png")) tags.Add("Textured");
            if (additionalTags.Contains("reversible.png")) tags.Add("Reversible");
            if (additionalTags.Contains("natural-fiber.png")) tags.Add("Natural Fiber");
            if (additionalTags.Contains("eco-friendly.png")) tags.Add("Eco Friendly");

            var construction = data[ScanField.Construction];
            if (construction.Contains("Naturals")) tags.Add("Naturals");
            if (construction.Contains("Solids")) tags.Add("Solids");
            if (construction.Contains("Textured")) tags.Add("Textured");

            features.Tags = new TagList(tags).GetFormattedTags();
            return features;
        }

        private Tuple<string, string> ParseProductName(string productName, string sku)
        {
            var collectionPart = productName.Replace(sku, "").Trim(new[] {'-', ' '});
            var collection = collectionPart;
            var designer = string.Empty;
            if (collectionPart.Contains("By"))
            {
                var parts = collectionPart.Split(new[] {"By"}, StringSplitOptions.RemoveEmptyEntries);
                collection = parts.First();
                designer = parts.Last();
            }
            return new Tuple<string, string>(collection.Trim(), designer.Trim());
        }

        private List<string> FormatDescription(string description)
        {
            description = description.Replace(" â€™", "'");
            description = description.Replace("ÃƒÂ©", "e");
            description = description.Replace("Ã¢â‚¬Å“", "");
            description = description.Replace("Ã¢â‚¬â€œ", "");
            description = description.Replace("Ã¢â‚¬Â�", "");
            return new List<string> { description };
        }

        private double FormatPileHeight(string pileHeight)
        {
            pileHeight = pileHeight.Replace("\"", "");
            if (pileHeight.IsDouble()) return Math.Round(ExtensionMethods.MeasurementFromFraction(pileHeight).ToDoubleSafe(), 2);
            return 0;
        }

        private string FixMisspellings(string color)
        {
            return color.TitleCase()
                .Replace("Atmoshpere", "Atmosphere")
                .Replace("Atmospere", "Atmosphere")
                .Replace("Aparagus", "Asparagus")
                .Replace("Billard", "Billiard")
                .Replace("Blue Atool", "Blue Atoll")
                .Replace("Bombaay", "Bombay")
                .Replace("Bosa Nova", "Bossa Nova")
                .Replace("Boss Nova", "Bossa Nova")
                .Replace("Captains Blue", "Captain's Blue")
                .Replace("Celestal", "Celestial")
                .Replace("Charocoal", "Charcoal")
                .Replace("Crãƒã¨me", "Creme")
                .Replace("Crã¨me", "Creme")
                .Replace("Plam", "Palm")
                .Replace("Enisign", "Ensign")
                .Replace("Etuscan", "Etruscan")
                .Replace("Excaliber", "Excalibur")
                .Replace("Fuchia", "Fuschia")
                .Replace("Fuchsia", "Fuschia")
                .Replace("High-rise", "Highrise")
                .Replace("Hightrise", "Highrise")
                .Replace("Lilly", "Lily")
                .Replace("Liquorice", "Liquiorice")
                .Replace("Jeffa", "Jaffa")
                .Replace("Kahki", "Khaki")
                .Replace("Katydad", "Katydid")
                .Replace("Malard", "Mallard")
                .Replace("Medievl", "Medieval")
                .Replace("Mosiac", "Mosaic")
                .Replace("Niagra", "Niagara")
                .Replace("Ocrchre", "Ochre")
                .Replace("Ochra", "Ochre")
                .Replace("Orian", "Orion")
                .Replace("Pagonda", "Pagoda")
                .Replace("Praire", "Prairie")
                .Replace("Provinical", "Provincial")
                .Replace("Russett", "Russet")
                .Replace("Saxoy", "Saxony")
                .Replace("Shitake", "Shiitake")
                .Replace("Shitaki", "Shiitake")
                .Replace("Shittake", "Shiitake")
                .Replace("Simple Taupe", "Simply Taupe")
                .Replace("Yoke Yellow", "Yolk Yellow")
                .Replace("York Yellow", "Yolk Yellow");
        }
    }
}