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

namespace LacefieldDesigns
{
    public class LacefieldProductBuilder : ProductBuilder<LacefieldDesignsVendor>
    {
        public LacefieldProductBuilder(IPriceCalculator<LacefieldDesignsVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var name = data[ScanField.ProductName].Replace("Â", "").Replace("&#215;", "x").Replace("Ã—", "x").Replace(" X ", "x").Trim('.');

            var vendor = new LacefieldDesignsVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(data[ScanField.StockCount] != "Out"), mpn, BuildFeatures(data, name));

            homewareProduct.HomewareCategory = AssignCategory(data[ScanField.Construction]);
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = name;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private readonly Dictionary<HomewareCategory, Func<string, bool>> _categories = new Dictionary<HomewareCategory, Func<string, bool>>
        {
            {HomewareCategory.Accent_Stools, con => con.Contains("Stool")},
            {HomewareCategory.Decorative_Pillows, con => con.Contains("Pillows")},
            {HomewareCategory.Poufs, con => con.Contains("Poufs")},
            {HomewareCategory.Throws, con => con.Contains("Throws")},
        };

        private HomewareCategory AssignCategory(string construction)
        {
            //foreach (var category in _categories)
                //if (category.Value(construction))
                    //return category.Key;
            return HomewareCategory.Decorative_Pillows;
        }

        private HomewareProductFeatures BuildFeatures(ScanData data, string name)
        {
            var dimensions = ParseDimensions(name);
            if (dimensions == null)
            {
                dimensions = ParseDimensions(data[ScanField.Category]);
            }

            var features = new HomewareProductFeatures();
            features.Features = BuildSpecs(data);

            if (dimensions != null)
            {
                features.Width = dimensions.Width;
                features.Height = dimensions.Height;
                features.Depth = dimensions.Length;
            }
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            return ToSpecs(new Dictionary<ScanField, string>
            {
                { ScanField.Content, data[ScanField.Content]},
                { ScanField.Material, data[ScanField.Material]},
                { ScanField.Tags, data[ScanField.Tags]},
            });
        }

        private LacefieldDimensions ParseDimensions(string name)
        {
            // \D*(\d+)x(\d+)x(\d+).*
            // \D*(\d+) X (\d+).*

            var match = Regex.Match(name, @"\D*(?<width>\d+)x(?<length>\d+)x(?<height>\d+).*");
            if (match.Success)
            {
                var width = match.Groups["width"].Value.Trim().ToDoubleSafe();
                var length = match.Groups["length"].Value.Trim().ToDoubleSafe();
                var height = match.Groups["height"].Value.Trim().ToDoubleSafe();
                return new LacefieldDimensions() { Width = width, Length = length, Height = height};
            }

            match = Regex.Match(name, @"\D*(?<width>\d+)x(?<length>\d+).*");
            if (match.Success)
            {
                var width = match.Groups["width"].Value.Trim().ToDoubleSafe();
                var length = match.Groups["length"].Value.Trim().ToDoubleSafe();
                return new LacefieldDimensions() { Width = width, Length = length, Height = 0};
            }

            match = Regex.Match(name, @"\D*(?<width>\d+) X (?<length>\d+).*");
            if (match.Success)
            {
                var width = match.Groups["width"].Value.Trim().ToDoubleSafe();
                var length = match.Groups["length"].Value.Trim().ToDoubleSafe();
                return new LacefieldDimensions() { Width = width, Length = length, Height = 0};
            }
            return null;
        }
    }

    public class LacefieldDimensions
    {
        public double Length { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
    }
}