using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace SquareFeathers
{
    public class SquareFeathersProductBuilder : ProductBuilder<SquareFeathersVendor>
    {
        public SquareFeathersProductBuilder(IPriceCalculator<SquareFeathersVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new SquareFeathersVendor();
            var cost = data.Cost;
            var homewareProduct = new HomewareProduct(cost, vendor, new StockData(true), mpn, BuildFeatures(data));
            var category = data[ScanField.Category].Replace("DÃ©cor", "Decor");

            homewareProduct.AddVariants(data.Variants.Select(x => CreateVendorVariant(x, homewareProduct)).ToList());
            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Breadcrumbs], mpn);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = mpn;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private VendorVariant CreateVendorVariant(ScanData variant, HomewareProduct product)
        {
            var vendorVariant = new HomewareVendorVariant(PriceCalculator.CalculatePrice(variant), "-" + variant[ScanField.Size].Replace("x", ""), new SquareFeathersVendor());
            vendorVariant.Cost = variant.Cost;
            vendorVariant.IsDefault = false;
            vendorVariant.ManufacturerPartNumber = product.MPN + " " + variant[ScanField.Size];
            vendorVariant.Name = string.Join(" x ", variant[ScanField.Size].Split('x').Select(x => x + "\""));
            vendorVariant.StockData = new StockData(true);
            vendorVariant.VendorProduct = product;
            return vendorVariant;
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            // put name checks first
            {HomewareCategory.Decorative_Trays, (cat, name) => name.Contains("Tray")},
            {HomewareCategory.Pendants, (cat, name) => name.Contains("Hanging Light")},
            {HomewareCategory.Sconces, (cat, name) => name.Contains("Sconce")},
            {HomewareCategory.Table_Lamps, (cat, name) => name.Contains("Gourd Lamp")},
            {HomewareCategory.Throws, (cat, name) => name.Contains("Throw")},

            {HomewareCategory.Accent_Chairs, (cat, name) => cat.Contains("Chair")},
            {HomewareCategory.Benches, (cat, name) => cat.Contains("Bench")},
            {HomewareCategory.Decorative_Pillows, (cat, name) => cat.Contains("Pillow")},
            {HomewareCategory.Dining_Tables, (cat, name) => cat.Contains("Table")},
            {HomewareCategory.Decor, (cat, name) => cat.Contains("Home DÃ©cor")},
            {HomewareCategory.Ottomans, (cat, name) => cat.Contains("Ottoman")},
            {HomewareCategory.Sofas, (cat, name) => cat.Contains("Sofa")},
            {HomewareCategory.Wall_Art, (cat, name) => cat.Contains("Decor")},
        };

        private HomewareCategory AssignCategory(string categoryName, string name)
        {
            foreach (var category in _categories)
                if (category.Value(categoryName, name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var bullets = data[ScanField.Bullets].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

            var features = new HomewareProductFeatures();
            features.CareInstructions = FindCare(bullets);
            features.Features = BuildSpecs(bullets);
            return features;
        }

        private string FindCare(List<string> bullets)
        {
            return bullets.SingleOrDefault(x => x.Contains("Clean"));
        }

        private Dictionary<string, string> BuildSpecs(List<string> bullets)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                {ScanField.Insert, bullets.SingleOrDefault(x => x.Contains("Down Insert"))},
                {ScanField.Country, bullets.SingleOrDefault(x => x.Contains("Made in the"))},
            });
        }
    }
}