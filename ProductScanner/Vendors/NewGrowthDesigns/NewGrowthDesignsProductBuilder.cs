using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace NewGrowthDesigns
{
    public class NewGrowthDesignsProductBuilder : ProductBuilder<NewGrowthDesignsVendor>
    {
        public NewGrowthDesignsProductBuilder(IPriceCalculator<NewGrowthDesignsVendor> priceCalculator) : base(priceCalculator) { }

        // almost no information on these products
        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];

            var vendor = new NewGrowthDesignsVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true), mpn, BuildFeatures(data));

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.ProductName]);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = data[ScanField.ProductName].Replace("&quot;", "\"");
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Description = new List<string> {data[ScanField.Description]};
            return features;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Centerpieces, name => name.Contains("Centerpiece")},
            {HomewareCategory.Faux_Flowers, name => name.ContainsAnyOf("Faux", "Bouquet", "Arrangement", "Orchid")},
            {HomewareCategory.Garden_Planters, name => name.ContainsAnyOf("Planter", "Pot")},
            {HomewareCategory.Vases, name => name.Contains("Vase")},
            {HomewareCategory.Wreaths, name => name.Contains("Wreath")},
        };

        private HomewareCategory AssignCategory(string name)
        {
            foreach (var category in _categories)
                if (category.Value(name))
                    return category.Key;
            return HomewareCategory.Faux_Flowers;
        }
    }
}