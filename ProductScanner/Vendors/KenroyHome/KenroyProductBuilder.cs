using System;
using System.Collections.Generic;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace KenroyHome
{
    public class KenroyProductBuilder : ProductBuilder<KenroyHomeVendor>
    {
        public KenroyProductBuilder(IPriceCalculator<KenroyHomeVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var name = FormatName(data[ScanField.ProductName]);

            var vendor = new KenroyHomeVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount].ToDoubleSafe()), mpn, BuildFeatures(data));
            homewareProduct.HomewareCategory = AssignCategory(name);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = name;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private string FormatName(string productName)
        {
            productName = productName.ReplaceWholeWord("Ftn", "Fountain");
            productName = productName.ReplaceWholeWord("Flr", "Floor");
            productName = productName.ReplaceWholeWord("Lnamp", "Lamp");
            productName = productName.ReplaceWholeWord("Lt", "Light");
            productName = productName.ReplaceWholeWord("Tale", "Table");
            return productName.TitleCase();
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            features.Depth = data[ScanField.Length].ToDoubleSafe();
            features.Description = new List<string> {data[ScanField.Description]};
            features.Features = BuildSpecs(data);
            features.Height = data[ScanField.Height].ToDoubleSafe();
            features.ShippingWeight = data[ScanField.ShippingWeight].ToDoubleSafe();
            features.UPC = data[ScanField.UPC];
            features.Width = data[ScanField.Width].ToDoubleSafe();
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                {ScanField.Bulbs, data[ScanField.Bulbs]},
                {ScanField.Category, data[ScanField.Category]},
                {ScanField.PatternNumber, data[ScanField.PatternNumber]},
            });
        }

        private HomewareCategory AssignCategory(string productName)
        {
            foreach (var category in _categories)
                if (category.Value(productName))
                    return category.Key;

            foreach (var category in _secondPass)
                if (category.Value(productName))
                    return category.Key;
            return HomewareCategory.Unknown;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Tables, name => name.Contains("Accent Table")},
            {HomewareCategory.Chandeliers, name => name.Contains("Chandelier")},
            {HomewareCategory.Curtains, name => name.Contains("Finial")},
            {HomewareCategory.Floor_Lamps, name => name.Contains("Floor Lamp")},
            {HomewareCategory.Garden_Decorations, name => name.ContainsAnyOf("Bird", "Fountain", "Urn", "Post Lantern", "Garden")},
            {HomewareCategory.Mirrors, name => name.Contains("Mirror")},
            {HomewareCategory.Mounts, name => name.ContainsIgnoreCase("Flush Mount") || name.ContainsAnyOf("Flushmount", "Flush")},
            {HomewareCategory.Pendants, name => name.ContainsAnyOf("Pendant", "Island Light", "Light Island", "Hanging Lantern")},
            {HomewareCategory.Sconces, name => name.ContainsAnyOf("Sconce", "Wall Lantern", "Swing Ar", "Wallchiere")},
            {HomewareCategory.Table_Lamps, name => name.ContainsAnyOf("Accent Lamp", "Buffet Lamp", "Table Lamp", "Desk Lamp", "Arm Lamp", "Torch", "Torchiere", "Banker Lamp")},
            {HomewareCategory.Vanities, name => name.Contains("Vanity")},
        };

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _secondPass = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Pendants, name => name.Contains("Lante")},
        };
    }
}