using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace DistinctiveDesigns
{
    public class DistinctiveDesignsProductBuilder : ProductBuilder<DistinctiveDesignsVendor>
    {
        public DistinctiveDesignsProductBuilder(IPriceCalculator<DistinctiveDesignsVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            data.Cost = data[ScanField.Cost].ToDecimalSafe();

            var vendor = new DistinctiveDesignsVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true), mpn, BuildFeatures(data));

            homewareProduct.IsDiscontinued = data.IsDiscontinued;
            homewareProduct.HomewareCategory = AssignCategory(data);
            homewareProduct.ManufacturerDescription = FormatDescription(data[ScanField.Description]);
            homewareProduct.Name = data[ScanField.ProductName].TitleCase().MoveSetInfo();
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = new List<ScannedImage> { new ScannedImage(ImageVariantType.Primary, 
                "ftp://photos%40distinctivedesigns.com:ddiphotos2016@192.169.217.174/" + data[ScanField.Image1]) };
            return homewareProduct;
        }

        private string FormatDescription(string description)
        {
            return description.Replace("dÃ©cor", "decor").Replace("décor", "decor").Replace("”", "\"").Replace("â€œ", "").Replace("â€", "").Replace("Â®", "");
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Faux_Plants, cat => cat.ContainsAnyOf("Floral", "Plants", "Trees")}
        };

        private HomewareCategory AssignCategory(ScanData data)
        {
            foreach (var category in _categories)
                if (category.Value(data[ScanField.Breadcrumbs]))
                    return category.Key;
            return HomewareCategory.Faux_Flowers;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();

            var dims = data[ScanField.Dimensions].Split(new[] {'x'}).Select(x => x.Trim()).ToList();
            if (dims.Count == 3)
            {
                features.Depth = dims[2].ToIntegerSafe();
                features.Height = dims[0].ToIntegerSafe();
                features.Width = dims[1].ToIntegerSafe();
            }

            features.Color = data[ScanField.Color];
            features.Description = new List<string> {FormatDescription(data[ScanField.Description])};
            features.Features = BuildSpecs(data);
            features.ShippingWeight = data[ScanField.Weight].ToDoubleSafe();
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                {ScanField.Material, data[ScanField.Material]},
                {ScanField.Style, FormatCategory(data[ScanField.Category])},
                {ScanField.Type, data[ScanField.Type]}
            });
        }

        private string FormatCategory(string category)
        {
            var cats = category.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Regex.Replace(x.Replace("(", "").Replace(")", ""), @"[\d-]", string.Empty).Trim()).ToList();
            return string.Join(", ", cats);
        }
    }
}