using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Surya
{
    public class SuryaProductBuilder : ProductBuilder<SuryaVendor>
    {
        public SuryaProductBuilder(IPriceCalculator<SuryaVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new SuryaVendor();
            var mpn = data[ScanField.ManufacturerPartNumber];

            var images = data.Variants.SelectMany(x => x.GetScannedImages()).ToList();
            images.AddRange(data.GetScannedImages());
            var rugProduct = new RugProduct(vendor);
            var vendorVariants = data.Variants.Select((x, i) => CreateVendorVariant(x, mpn, i, rugProduct, images)).ToList();
            rugProduct.AddVariants(vendorVariants);

            rugProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            rugProduct.Correlator = mpn;
            rugProduct.Name = new[] { mpn, "by", vendor.DisplayName }.BuildName();
            rugProduct.ScannedImages = images;

            rugProduct.RugProductFeatures = BuildRugProductFeatures(data);
            return rugProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, string mpn, int index, RugProduct rugProduct, List<ScannedImage> images)
        {
            var size = variant[ScanField.Size];
            var rugDimensions = RugParser.ParseDimensions(size);
            var features = RugProductVariantFeaturesBuilder.Build(rugDimensions);
            var variantMpn = mpn + GetSKUSuffix(variant[ScanField.SKU]);

            var match = images.FirstOrDefault(x => x.Url.Replace("-", "").ContainsIgnoreCase(variantMpn.Replace("-", "")));
            features.ImageFilename = match != null ? match.Id + ".jpg" :
                images.Any() ? images.First().Id + ".jpg" : string.Empty;

            variant.Cost = variant[ScanField.Cost].ToDecimalSafe();
            return new RugVendorVariant(
                variantMpn,
                GetSKUSuffix(variant[ScanField.SKU]),
                variant.Cost,
                PriceCalculator.CalculatePrice(variant),
                variant[ScanField.StockCount].ToDoubleSafe() > 0,
                new SuryaVendor(),
                rugProduct,
                index,
                features);
        }

        private RugProductFeatures BuildRugProductFeatures(ScanData data)
        {
            var features = new RugProductFeatures();
            var color = FormatColor(FindContainsValue(data, new List<string> { "Color" }));

            features.Backing = FindBacking(data);
            features.Color = color;
            features.Colors = color.Split(new[] {','}).Select(x => x.Trim()).ToList();
            features.ColorGroup = data[ScanField.ColorGroup].Replace(" (Purple)", "");
            features.CountryOfOrigin = new Country(FindValue(data, _countries)).Format();
            features.Material = new RugMaterial(data[ScanField.Content]).GetFormattedMaterial();
            features.Weave = FindValue(data, _constructions);

            var tags = data[ScanField.Style].Split(new[] {" and "}, StringSplitOptions.RemoveEmptyEntries).ToList();
            tags.Add(data[ScanField.Design]);
            tags.Add(FindValue(data, _additionalInfo));

            features.Tags = new TagList(tags).GetFormattedTags();
            return features;
        }

        private string GetSKUSuffix(string sku)
        {
            // AMD1003-268
            return sku.Substring(sku.IndexOf("-"));
        }

        private string FormatColor(string color)
        {
            color = color.Replace("Color (Pantone TPX): ", "");
            color = Regex.Replace(color, @"\([^()]+\)", "");
            var colors = color.Split(new[] {','}).Select(x => x.Trim()).Distinct();
            return colors.Aggregate((a, b) => a + ", " + b);
        }

        private string FindContainsValue(ScanData data, List<string> values)
        {
            foreach (var field in _searchFields)
            {
                if (values.Any(x => data[field].Contains(x))) return data[field];
            }
            return string.Empty;
        }

        private string FindValue(ScanData data, List<string> values)
        {
            foreach (var field in _searchFields)
                if (values.Contains(data[field])) 
                    return data[field];
            return string.Empty;
        }

        private string FindBacking(ScanData data)
        {
            // a field that has 'Backing:' in it
            foreach (var field in _searchFields)
                if (data[field].Contains("Backing"))
                {
                    var backing = data[field].Replace("Backing: ", "");
                    return backing == "N/A" ? string.Empty : backing;
                }
            return string.Empty;
        }

        private readonly List<ScanField> _searchFields = new List<ScanField>
        {
            ScanField.Bullet2,
            ScanField.Bullet3,
            ScanField.Bullet4,
            ScanField.Bullet5,
            ScanField.Bullet6,
            ScanField.Bullet7,
            ScanField.Bullet8,
        }; 

        private readonly List<string> _constructions = new List<string>
        {
            "Hand Crafted",
            "Machine Made",
            "Hand Knotted",
            "Hand Loomed",
            "Hand Tufted",
            "Hand Woven",
            "Hand Hooked",
        }; 

        private readonly List<string> _countries = new List<string>
        {
            "Made in Brazil",
            "Made in Turkey",
            "Made in China",
            "Made in India",
            "Made in Egypt",
            "Made in Israel",
            "Made in U.S.A.",
            "Made in Belgium",
            "Made in Argentina",
            "Made in K.S.A.",
        }; 

        private readonly List<string> _piles = new List<string>
        {
            "No Pile",
            "Low Pile",
            "High Pile",
            "Medium Pile / No Pile",
            "Medium Pile",
            "High Pile / Low Pile",
            "Plush Pile",
        }; 

        private readonly List<string> _additionalInfo = new List<string>
        {
            "Antique Wash",
            "Carved",
            "Chenille Accents",
            "Detail",
            "Hand Stitching",
            "Loop Accents",
            "Lustrous Sheen",
            "Printed",
            "Recycled Materials",
            "Reversible",
            "Shaped Edges",
            "Shedding",
            "Sheen",
            "Soft",
            "Super Soft",
            "Texture",
            "Tufted Accents",
            "Undyed",
            "Viscose Accents",
        }; 
    }
}