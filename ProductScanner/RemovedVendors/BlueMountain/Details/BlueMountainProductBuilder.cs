using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace BlueMountain.Details
{
    public class BlueMountainProductBuilder : IProductBuilder<BlueMountainVendor>
    {
        public VendorProduct Build(ScanData data)
        {
            var mpn = data[ProductPropertyType.ManufacturerPartNumber];
            var cost = data[ProductPropertyType.WholesalePrice].ToDecimalSafe();
            var stock = data[ProductPropertyType.StockCount];
            var pattern = data[ProductPropertyType.PatternName];
            var vendor = new BlueMountainVendor();

            var vendorProduct = new FabricProduct(mpn, cost, InStock(stock), vendor);

            vendorProduct.PublicProperties[ProductPropertyType.AlternateItemNumber] = data[ProductPropertyType.AlternateItemNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ProductPropertyType.Category].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = data[ProductPropertyType.Cleaning];
            vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = data[ProductPropertyType.ColorGroup].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = data[ProductPropertyType.CountryOfOrigin].ToFormattedCountry();
            vendorProduct.PublicProperties[ProductPropertyType.Coverage] = data[ProductPropertyType.Coverage].Replace(" per double", "").Replace("Sq f", "square feet").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Height] = data[ProductPropertyType.Height].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Length] = data[ProductPropertyType.Length].Replace("FT", "feet");
            vendorProduct.PublicProperties[ProductPropertyType.Material] = data[ProductPropertyType.Material].Replace(" - Pasted", "").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Match] = data[ProductPropertyType.Match];
            vendorProduct.PublicProperties[ProductPropertyType.Paste] = data[ProductPropertyType.Paste];
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = FormatPatternName(data[ProductPropertyType.PatternName]);
            vendorProduct.PublicProperties[ProductPropertyType.Prepasted] = data[ProductPropertyType.Prepasted];
            vendorProduct.PublicProperties[ProductPropertyType.ProductType] = data[ProductPropertyType.ProductType];
            vendorProduct.PublicProperties[ProductPropertyType.UPC] = data[ProductPropertyType.UPC];
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ProductPropertyType.Width].ToInchesMeasurement();

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data[ProductPropertyType.ProductDetailUrl];

            vendorProduct.AddPublicProp(ProductPropertyType.AdditionalInfo, data[ProductPropertyType.Removal]);
            vendorProduct.AddProp(ProductPropertyType.Height, data, "0");
            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);
            vendorProduct.AddProp(ProductPropertyType.Repeat, data, "0");

            vendorProduct.RemoveWhen(ProductPropertyType.Category, s => _colorGroups.Any(s.Contains));
            vendorProduct.RemoveWhen(ProductPropertyType.ColorGroup, s => !_colorGroups.Contains(s, true));
            vendorProduct.RemoveWhen(ProductPropertyType.Coverage, s => s.ContainsIgnoreCase("See Product Info"));
            vendorProduct.RemoveWhen(ProductPropertyType.PatternName, s => string.Equals(pattern, "Design by Color", StringComparison.OrdinalIgnoreCase));

            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn);
            vendorProduct.Correlator = string.IsNullOrWhiteSpace(pattern) ? sku : pattern;
            vendorProduct.Name = new[] {mpn, pattern, "by", vendor.DisplayName}.BuildName();
            vendorProduct.ProductClass = data[ProductPropertyType.ProductType].ToWallcoveringProductClassSafe();
            vendorProduct.ProductGroup = ProductGroup.Wallcovering;
            vendorProduct.SetImages(data.ProductImages);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = GetUnitOfMeasure(data[ProductPropertyType.UnitOfMeasure]);
            return vendorProduct;
        }

        private string FormatPatternName(string patternName)
        {
            patternName = patternName.RemovePattern(@" - BT\d+");
            patternName = patternName.RemovePattern("Self-Adhesive Border - ");

            patternName = patternName.RemovePattern(" 1 Sheet.*");
            patternName = patternName.RemovePattern(" 2 Sheets.*");
            patternName = patternName.RemovePattern(" 4 Sheets.*");

            patternName = patternName.RemovePattern(" - P.*");
            patternName = patternName.RemovePattern("\\d+\" W X");

            patternName = patternName.RemovePattern(" 12'.*");
            patternName = patternName.Replace("BDR.", "");
            patternName = patternName.Replace("BDR", "");

            patternName = patternName.RemovePattern("\\s\\d+(\"|').*"); // remove dimensions
            patternName = patternName.RemovePattern("\\s\\(.*\\)"); // remove anything in parenthesis

            return patternName.TitleCase();
        }

        private UnitOfMeasure GetUnitOfMeasure(string unit)
        {
            var unitOfMeasure = UnitOfMeasure.Roll;
            // Spool?
            if (unit == "SP") unitOfMeasure = UnitOfMeasure.Roll;
            else if (unit == "SR") unitOfMeasure = UnitOfMeasure.Roll;
            else if (unit == "Ea") unitOfMeasure = UnitOfMeasure.Each;
            return unitOfMeasure;
        }

        private bool InStock(string stock)
        {
            if (string.IsNullOrWhiteSpace(stock))
                return false;

            var count = Convert.ToDouble(stock);
            return count > 0;
        }

        private readonly List<string> _colorGroups = new List<string>
        {
            "Black",
            "Blue",
            "White",
            "Brown",
            "Green",
            "Neutral",
            "Orange",
            "Plum",
            "Red",
            "Yellow",
            "Metallic",
            "Purple",
            "Pastel",
            "Neutral",
            "Aqua",
        };
    }
}