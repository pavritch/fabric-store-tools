using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace LuxArtSilks
{
    public class LuxArtSilksProductValidator : DefaultProductValidator<LuxArtSilksVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            if ((product as HomewareProduct).MPN.StartsWith("F-14"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }
            return validation;
        }
    }

    public class LuxArtProductBuilder : ProductBuilder<LuxArtSilksVendor>
    {
        public LuxArtProductBuilder(IPriceCalculator<LuxArtSilksVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var vendor = new LuxArtSilksVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true),
                data[ScanField.ManufacturerPartNumber], BuildFeatures(data));

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.ProductName]);
            homewareProduct.Name = data[ScanField.ProductName].TitleCase();
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            var description = GetDimensions(data[ScanField.Description], data[ScanField.AdditionalInfo]).Replace("â€³", "\"");
            if (description.Contains("X"))
            {
                var splitDimensions = description.Split(new [] {"X"}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                features.Height = splitDimensions[0].Replace(":", "").Replace("H", "").Replace("\"", "").ToDoubleSafe();
                features.Width = splitDimensions[1].Replace(":", "").Replace("W", "").Replace("\"", "").ToDoubleSafe();
                if (splitDimensions.Count > 2) features.Depth = splitDimensions[2].Replace("D", "").Replace("\"", "").ToDoubleSafe();
            }
            else
            {
                features.Height = GetHeight(description).ToDoubleSafe();
                features.Width = GetWidth(description).ToDoubleSafe();
            }
            features.Brand = "Lux Art Silks";
            features.Features = BuildSpecs(data);
            return features;
        }

        private string GetDimensions(string description, string additionalInfo)
        {
            var dimensions = description == "" ?  additionalInfo.Split(',').FirstOrDefault(x => x.ContainsIgnoreCase(" x ")) : description;
            if (dimensions != null)
                return dimensions.Replace("&#8243;", "\"");
            return string.Empty;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.Category, data[ScanField.Category].Replace("Categories: ", "")}
            };
            return ToSpecs(specs);
        }

        private string GetWidth(string description)
        {
            var width = description.CaptureWithinMatchedPattern(@"Width: (?<capture>(\d+)) in.");
            if (width == null) width = description.CaptureWithinMatchedPattern("Width: (?<capture>(\\d+))\"");
            if (width == null) width = description.CaptureWithinMatchedPattern("Width: (?<capture>((\\d*\\.)?\\d+)) ft");
            if (width == null) width = description.CaptureWithinMatchedPattern("(?<capture>((\\d*\\.)?\\d+))\"W");
            if (width == null) width = description.CaptureWithinMatchedPattern("(?<capture>((\\d*\\.)?\\d+))\" W");
            return width;
        }

        private string GetHeight(string description)
        {
            var height = description.CaptureWithinMatchedPattern(@"Height: (?<capture>(\d+)) in.");
            if (height == null) height = description.CaptureWithinMatchedPattern("Height: (?<capture>(\\d+))\"");
            if (height == null) height = description.CaptureWithinMatchedPattern("Height: (?<capture>((\\d*\\.)?\\d+)) ft");
            if (height == null) height = description.CaptureWithinMatchedPattern("(?<capture>((\\d*\\.)?\\d+))\"H");
            if (height == null) height = description.CaptureWithinMatchedPattern("(?<capture>((\\d*\\.)?\\d+))\" H");
            return height;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Tables, name => name.Contains("Tray Table")}
        };

        private HomewareCategory AssignCategory(string productName)
        {
            foreach (var category in _categories)
                if (category.Value(productName))
                    return category.Key;
            return HomewareCategory.Faux_Flowers;
        }
    }
}