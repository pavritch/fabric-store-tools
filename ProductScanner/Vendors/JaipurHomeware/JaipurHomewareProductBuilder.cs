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

namespace JaipurHomeware
{
    public class JaipurDimensions
    {
        public double Height { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
    }

    public class JaipurProductValidator : DefaultProductValidator<JaipurHomewareVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            if (product.Name.ContainsIgnoreCase("Kate Spade"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }
            return validation;
        }
    }

    public class JaipurHomewareProductBuilder : ProductBuilder<JaipurHomewareVendor>
    {
        public JaipurHomewareProductBuilder(IPriceCalculator<JaipurHomewareVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendor = new JaipurHomewareVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount].ToIntegerSafe()), mpn, BuildFeatures(data));

            homewareProduct.AddVariants(data.Variants.Select(x => CreateVendorVariant(x, homewareProduct)).ToList());
            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Construction]);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = data[ScanField.ProductName];
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, HomewareProduct product)
        {
            var vendorVariant = new HomewareVendorVariant(PriceCalculator.CalculatePrice(variant), variant[ScanField.SKU], new JaipurHomewareVendor());
            vendorVariant.Cost = variant.Cost;
            vendorVariant.IsDefault = false;
            vendorVariant.ManufacturerPartNumber = variant[ScanField.ManufacturerPartNumber];
            vendorVariant.Name = variant[ScanField.ProductName];
            vendorVariant.StockData = new StockData(variant[ScanField.StockCount].ToDoubleSafe());
            vendorVariant.VendorProduct = product;
            return vendorVariant;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Decorative_Pillows, con => con.Contains("Pillows")},
            {HomewareCategory.Poufs, con => con.Contains("Pouf")},
            {HomewareCategory.Throws, con => con.Contains("Throws")},
        };

        private HomewareCategory AssignCategory(string construction)
        {
            foreach (var category in _categories)
                if (category.Value(construction))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var dimensions = GetDimensions(data[ScanField.Dimensions]);

            var features = new HomewareProductFeatures();
            features.CareInstructions = data[ScanField.Cleaning];
            features.Color = FormatColor(data[ScanField.Color]);
            features.Depth = dimensions.Length;
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            features.Height = dimensions.Height;
            features.Width = dimensions.Width;
            return features;
        }

        private string FormatColor(string color)
        {
            color = color.Replace("Crãƒã¨me", "Creme");
            color = color.Replace("Crã¨me", "Creme");
            var colors = color.Split('&').Select(x => x.Trim().TitleCase()).ToList();
            return string.Join(", ", colors);
        }

        private JaipurDimensions GetDimensions(string dimStr)
        {
            var dimensions = new JaipurDimensions();
            var parts = dimStr.Replace("\"", "").Split(new[] {'x'}).ToList();
            if (parts.Count == 2)
            {
                dimensions.Length = parts[0].ToDoubleSafe();
                dimensions.Width = parts[1].ToDoubleSafe();
            }
            if (parts.Count == 3)
            {
                dimensions.Length = parts[0].ToDoubleSafe();
                dimensions.Width = parts[1].ToDoubleSafe();
                dimensions.Height = parts[2].ToDoubleSafe();
            }
            return dimensions;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                {ScanField.Backing, data[ScanField.Backing]},
                {ScanField.Content, data[ScanField.Content]},
                {ScanField.Country, data[ScanField.Country]},
                {ScanField.Pile, data[ScanField.Pile]},
                {ScanField.Shape, data[ScanField.Shape]},
            });
        }
    }
}