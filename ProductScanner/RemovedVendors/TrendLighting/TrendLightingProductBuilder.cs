using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace TrendLighting
{
    public class TrendLightingProductBuilder : ProductBuilder<TrendLightingVendor>
    {
        public TrendLightingProductBuilder(IPriceCalculator<TrendLightingVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new TrendLightingVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(IsInStock(data[ScanField.StockCount])), mpn, BuildFeatures(data));

            var bulletData = ParseBullets(data[ScanField.Bullet1]);
            data.AddFields(bulletData);

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Category], data[ScanField.ProductName]);
            homewareProduct.Name = data[ScanField.ProductName];
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private HomewareCategory AssignCategory(string categoryName, string name)
        {
            foreach (var category in _categories)
                if (category.Value(categoryName, name))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private readonly Dictionary<HomewareCategory, Func<string, string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, string, bool>>
        {
            {HomewareCategory.Ceiling_Lights, (cat, name) => cat.Contains("Pendants") || cat.Contains("Flushmount") || cat.Contains("Island") || name.Contains("Pendant")},
            {HomewareCategory.Chandeliers, (cat, name) => cat.Contains("Chandeliers") || name.Contains("Chandelier")},
            {HomewareCategory.Floor_Lamps, (cat, name) => cat.Contains("Floor Lamps") || name.Contains("Floor Lamp")},
            {HomewareCategory.Table_Lamps, (cat, name) => cat.Contains("Table Lamps") || cat.Contains("Task Lamp") || name.Contains("Table Lamp") || name.Contains("Accent Lamp") || name.Contains("Task Lamp")},
            {HomewareCategory.Wall_Lamps, (cat, name) => cat.Contains("Wall Lamps")}
        };

        private readonly List<TrendBullet> _trendBullets = new List<TrendBullet>
        {
            new TrendBullet(new List<string>{ "Shade Size" }, ScanField.ShadeSize, s => s),
            new TrendBullet(new List<string>{ "Shade" }, ScanField.Shade, s => s),
            new TrendBullet(new List<string>{ "or CFL", "or LED", "or Dimmable"}, ScanField.BulbReplacement, 
                s => s.Replace("or ", "").Replace("/LED", "/ LED").Replace("CFL/", "CFL /")),
            new TrendBullet(new List<string>{ "Watt" }, ScanField.Bulbs, s => s),
            new TrendBullet(new List<string>{ " x " }, ScanField.Dimensions, s => s),
        }; 

        private ScanData ParseBullets(string bulletText)
        {
            bulletText = bulletText.Replace("�", "\"");
            var data = new ScanData();
            var bullets = bulletText.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Where(x => x != string.Empty).ToList();
            foreach (var bullet in bullets)
            {
                var match = _trendBullets.FirstOrDefault(x => x.Matches(bullet));
                if (match != null)
                    data[match.ScanField] = bullet;
            }
            return data;
        }

        private bool IsInStock(string stock)
        {
            if (stock == "In stock") return true;
            // I think this is true also
            if (stock == "Contact Trend For Availability") return true;
            return false;
        }

        private string GetDimension(string dimensions, params string[] fields)
        {
            var dims = dimensions.Split(new[] {" x "}, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var field in fields)
            {
                var match = dims.SingleOrDefault(x => x.Trim().EndsWith(field));
                if (match != null) return match.Replace("\"", "").Replace(field, "");
            }
            return string.Empty;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Height = GetDimension(data[ScanField.Dimensions], "H", "Ht", "Height").ToIntegerSafe();
            features.Features = BuildSpecs(data);
            features.Finish = data[ScanField.Finish];
            features.Width = GetDimension(data[ScanField.Dimensions], "W", "Width").ToIntegerSafe();
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.BulbReplacement, data[ScanField.BulbReplacement]},
                {ScanField.ProductType, data[ScanField.ProductType]},
                {ScanField.ProductUse, data[ScanField.ProductUse]},
                {ScanField.Shade, data[ScanField.Shade]},
            };

            if (data[ScanField.Bulbs] != "0") specs.Add(ScanField.Bulbs, data[ScanField.Bulbs]);
            return specs.ToDictionary(x => x.Key.ToString(), x => x.Value);
        }
    }
}