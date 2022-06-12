using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace AllstateFloral
{
    public class AllstateFloralProductBuilder : ProductBuilder<AllstateFloralVendor>
    {
        public AllstateFloralProductBuilder(IPriceCalculator<AllstateFloralVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new AllstateFloralVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(GetStock(data[ScanField.StockCount])), mpn, BuildFeatures(data));

            homewareProduct.HomewareCategory = AssignCategory(data);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.MinimumQuantity = data[ScanField.MinimumQuantity].ToIntegerSafe();
            homewareProduct.Name = data[ScanField.Description];
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accessories, cat => cat.Contains("Accessories")},
            {HomewareCategory.Ignored, cat => cat.Contains("Christmas")},
            {HomewareCategory.Florals, cat => cat.Contains("Living Items")},
        };

        private HomewareCategory AssignCategory(ScanData data)
        {
            foreach (var category in _categories)
                if (category.Value(data[ScanField.ProductType]))
                    return category.Key;
            return HomewareCategory.Faux_Flowers;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var dimensions = GetDimensions(data[ScanField.Dimensions]);
            if (dimensions.IsEmpty())
            {
                // dimensions are in the Description field for most products
                dimensions = Parse(data[ScanField.Description]);
            }

            var features = new HomewareProductFeatures();
            features.Color = data[ScanField.ColorName];
            features.Depth = GetDepth(dimensions);
            features.Features = BuildSpecs(data);
            features.Height = dimensions.Height;
            features.ShippingWeight = data[ScanField.Weight].Replace("OZ", "").ToDoubleSafe();
            features.UPC = data[ScanField.UPC];
            features.Width = dimensions.Width;
            return features;
        }

        private double GetDepth(AllstateDimensions dimensions)
        {
            return dimensions.Depth <= 0 ? dimensions.Length : dimensions.Depth;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                {ScanField.ColorGroup, _groups[data[ScanField.ColorGroup]]},
                {ScanField.Material, data[ScanField.Material]},
                {ScanField.ProductType, data[ScanField.ProductType]},
            });
        }

        private Dictionary<string, string> _groups = new Dictionary<string, string>
        {
            { "", "" },
            { "BT", "" },
            { "BU", "" },
            { "PU", "Purple" },
            { "IV", "Ivory" },
            { "GR", "Green" },
            { "WH", "White" },
            { "BK", "Black" },
            { "RE", "Red" },
            { "BR", "Brown" },
            { "GO", "Gold" },
            { "SI", "Silver" },
            { "OR", "Orange" },
            { "YE", "Yellow" },
            { "LV", "Lavender" },
            { "MX", "Mixed" },
            { "BL", "Blue" },
            { "AQ", "Aqua" },
            { "PK", "Pink" },
            { "AS", "Assorted" },
            { "PE", "Peach" },
        }; 

        private AllstateDimensions GetDimensions(string description)
        {
            var split = description.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            var dimensionsText = split.First();
            var splitDims = dimensionsText.Split(new[] {'x'}).ToList();
            return new AllstateDimensions(splitDims.FirstOrDefault(x => x.Contains("W")),
                splitDims.FirstOrDefault(x => x.Contains("H")),
                splitDims.FirstOrDefault(x => x.Contains("L")),
                splitDims.FirstOrDefault(x => x.Contains("D")));
        }

        public AllstateDimensions Parse(string description)
        {
            var lastQuote = Math.Max(description.LastIndexOf("\""), description.LastIndexOf("'"));
            if (lastQuote + 2 >= description.Length) return new AllstateDimensions(0, 0, 0, 0);
            var dimensions = description.Substring(0, lastQuote + 2);
            if (!dimensions.Contains("x") && !dimensions.Contains("D") && !dimensions.Contains("L")) 
                return new AllstateDimensions(string.Empty, dimensions.Trim(), string.Empty, string.Empty);
            return GetDimensions(dimensions);
        }

        private string GetProductName(string description)
        {
            var split = description.Split(new[] {' '}).ToList();
            return string.Join(" ", split.Skip(1));
        }

        private bool GetStock(string stock)
        {
            // 1-3 WEEKS, 2-3 MONTHS, NOW, OVER SOLD, TBD
            if (stock == "NOW") return true;
            return false;
        }

    }
}