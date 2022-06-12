using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ArtAndFrameSource
{
    public class ArtAndFrameSourceProductBuilder : ProductBuilder<ArtAndFrameSourceVendor>
    {
        public ArtAndFrameSourceProductBuilder(IPriceCalculator<ArtAndFrameSourceVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var vendor = new ArtAndFrameSourceVendor();
            var homewareProduct = new HomewareProduct(data.Cost, vendor, new StockData(true), mpn, BuildFeatures(data));

            homewareProduct.HomewareCategory = HomewareCategory.Wall_Art;
            homewareProduct.ManufacturerDescription = data[ScanField.Description];
            homewareProduct.Name = FormatName(data[ScanField.ProductName]);
            if (homewareProduct.Name == string.Empty) homewareProduct.Name = mpn;
            homewareProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            homewareProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            homewareProduct.ScannedImages = data.GetScannedImages();
            return homewareProduct;
        }

        private string FormatName(string name)
        {
            return name.RemovePattern(@"^\d+")
                .Trim('-', ' ')
                .RemovePattern(@"\d+x\d+$")
                .RemovePattern(@"\d+ x \d+$")
                .Trim('-', ' ')
                .TitleCase()
                .RomanNumeralCase();
        }

        private HomewareProductFeatures BuildFeatures(ScanData data)
        {
            var features = new HomewareProductFeatures();
            if (!data[ScanField.Size].ToLower().Contains("x"))
            {
                // sometimes dimensions are only in the product name
                data[ScanField.Size] = data[ScanField.ProductName].CaptureWithinMatchedPattern(@"(?<capture>(\d+x\d+))");
                if (data[ScanField.Size] == string.Empty)
                    data[ScanField.Size] = data[ScanField.ProductName].CaptureWithinMatchedPattern(@"(?<capture>(\d+ x \d+))");
            }

            if (data[ScanField.Size].ToLower().Contains("x"))
            {
                var size = data[ScanField.Size].Replace("¼", "1/4");
                var dims = size.ToLower().Split('x').Select(x => x.Trim()).ToList();
                features.Width = ExtensionMethods.MeasurementFromFraction(dims.First()).ToDoubleSafe();
                features.Height = ExtensionMethods.MeasurementFromFraction(dims.Last()).ToDoubleSafe();
            }
            features.Features = BuildSpecs(data);
            return features;
        }

        private Dictionary<string, string> BuildSpecs(ScanData data)
        {
            var specs = new Dictionary<ScanField, string>
            {
                {ScanField.Designer, data[ScanField.Designer].TitleCase()},
                {ScanField.Material, data[ScanField.Material].TitleCase().Trim('.')}
            };
            return ToSpecs(specs);
        }
    }
}