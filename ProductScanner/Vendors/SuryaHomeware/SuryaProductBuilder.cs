using System;
using System.Collections.Generic;
using InsideFabric.Data;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace SuryaHomeware
{
    public class SuryaProductBuilder : ProductBuilder<SuryaHomewareVendor>
    {
        public SuryaProductBuilder(IPriceCalculator<SuryaHomewareVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendor = new SuryaHomewareVendor();
            var homewareProduct = new HomewareProduct(50, vendor, new StockData(true), mpn, BuildFeatures(data));

            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            homewareProduct.AddVariants(data.Variants.Select(x => CreateVendorVariant(x, homewareProduct)).ToList());
            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Category5]);
            homewareProduct.Name = data[ScanField.ProductName];
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, HomewareProduct product)
        {
            var name = variant[ScanField.Size].Trim().Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).First().Replace("&nbsp; ", "");

            var vendorVariant = new HomewareVendorVariant(PriceCalculator.CalculatePrice(variant), "-" + variant[ScanField.ManufacturerPartNumber].Split('-').Last(), new SuryaHomewareVendor());
            vendorVariant.Cost = variant[ScanField.Cost].ToDecimalSafe();
            vendorVariant.Name = name;
            vendorVariant.IsDefault = false;
            vendorVariant.ManufacturerPartNumber = variant[ScanField.ManufacturerPartNumber];
            vendorVariant.StockData = new StockData(variant[ScanField.StockCount].ToDoubleSafe());
            vendorVariant.VendorProduct = product;
            return vendorVariant;
        }

        private HomewareCategory AssignCategory(string categoryName)
        {
            foreach (var category in _categories)
                if (category.Value(categoryName))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Features = BuildSpecs(data);
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                { ScanField.Collection, data[ScanField.Collection]},
                { ScanField.Designer, data[ScanField.Designer]},
            });
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Decorative_Pillows, cat => cat.Contains("pillows")},
            {HomewareCategory.Poufs, cat => cat.Contains("poufs")},
            {HomewareCategory.Throws, cat => cat.Contains("throws")},
        };
    }
}