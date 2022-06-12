using System;
using System.Collections.Generic;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Duralee.Details
{
    public class BBergerProductBuilder : DuraleeBaseProductBuilder<BBergerVendor> 
    {
        public BBergerProductBuilder(IPriceCalculator<BBergerVendor> priceCalculator) : base(priceCalculator) { }
    }

    public class ClarkeAndClarkeProductBuilder : DuraleeBaseProductBuilder<ClarkeAndClarkeVendor>
    {
        public ClarkeAndClarkeProductBuilder(IPriceCalculator<ClarkeAndClarkeVendor> priceCalculator) : base(priceCalculator) { }
    }

    public class DuraleeProductBuilder : DuraleeBaseProductBuilder<DuraleeVendor>
    {
        public DuraleeProductBuilder(IPriceCalculator<DuraleeVendor> priceCalculator) : base(priceCalculator) { }
    }

    public class HighlandCourtProductBuilder : DuraleeBaseProductBuilder<HighlandCourtVendor>
    {
        public HighlandCourtProductBuilder(IPriceCalculator<HighlandCourtVendor> priceCalculator) : base(priceCalculator) { }
    }

    public class DuraleeBaseProductBuilder<T> : ProductBuilder<T> where T : Vendor, new()
    {
        public DuraleeBaseProductBuilder(IPriceCalculator<T> priceCalculator) : base(priceCalculator) { }
        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var stock = data[ScanField.StockCount].ToDoubleSafe();
            var patternName = GetPatternName(data);
            var colorName = FormatColorName(data[ScanField.ColorName]);

            var vendor = new T();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(stock), vendor);
            var width = data[ScanField.Width].Replace("\"", "").ToDoubleSafe();

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var collection = data[ScanField.Collection].Replace(" Collection", "");
            vendorProduct.PublicProperties[ProductPropertyType.BookNumber] = data[ScanField.Book].Replace("Book #", "");
            vendorProduct.PublicProperties[ProductPropertyType.Brand] = data[ScanField.Brand] != vendor.DisplayName ? data[ScanField.Brand] : null;
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = collection;
            vendorProduct.PublicProperties[ProductPropertyType.Color] = data[ScanField.Color];
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = data[ScanField.ColorNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content], collection);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Designer] = data[ScanField.Designer].Replace("+", " ");
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability];
            vendorProduct.PublicProperties[ProductPropertyType.Finishes] = data[ScanField.Finish];
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].Replace("\"", "").ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.OrderInfo] = data[ScanField.OrderInfo];
            vendorProduct.PublicProperties[ProductPropertyType.Other] = data[ScanField.Other].Replace("Indoor-Outdoor", "");
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternNumber];
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = FormatProductUse(data[ScanField.ProductUse], data[ScanField.Other]);
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = data[ScanField.Railroaded].Contains("Railroaded") ? "Railroaded" : "";
            vendorProduct.PublicProperties[ProductPropertyType.Style] = FormatStyle(data[ScanField.Style]);
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].Replace("\"", "").ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = width.ToString().ToInchesMeasurement();

            var group = data[ScanField.ProductGroup].ToProductGroup();
            var unit = GetUnitOfMeasure(data[ScanField.Content], group);
            if (group == ProductGroup.Wallcovering && unit == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Length] = "11 yards";

                var dimensions = new RollDimensions(width, 11 * 36);
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(1);

                vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();
            }

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);
            vendorProduct.RemoveWhen(ProductPropertyType.HorizontalRepeat, s => s.IsZeroMeasurement());
            vendorProduct.RemoveWhen(ProductPropertyType.VerticalRepeat, s => s.IsZeroMeasurement());
            vendorProduct.RemoveWhen(ProductPropertyType.Width, s => s.IsZeroMeasurement());


            var brand = data[ScanField.Brand];
            var vendorName = string.IsNullOrWhiteSpace(brand) ? vendor.DisplayName : brand;
            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, data[ScanField.PatternNumber]);
            vendorProduct.MinimumQuantity = GetMinimumQuantity(group, unit);
            vendorProduct.Name = new[] {mpn, patternName, colorName, "by", vendorName}.BuildName();
            vendorProduct.OrderIncrement = 1;
            vendorProduct.SetProductGroup(group);
            vendorProduct.ScannedImages = GetImages(data[ScanField.ImageUrl]);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = unit;
            return vendorProduct;
        }

        private string GetPatternName(ScanData data)
        {
            var patternName = data[ScanField.PatternName];

            if (data[ScanField.PatternName] == string.Empty) return string.Empty;
            if (data[ScanField.PatternNumber] != string.Empty) patternName = patternName.Replace(data[ScanField.PatternNumber], "");
            
            return patternName.Replace("20 1/2\"", "").TitleCase().RomanNumeralCase();
        }

        private int GetMinimumQuantity(ProductGroup group, UnitOfMeasure unit)
        {
            // 3 yards for any TRIM product sold by the Yard
            if (group == ProductGroup.Trim && unit == UnitOfMeasure.Yard) return 3;
            if (group == ProductGroup.Wallcovering) return 2;
            return 1;
        }

        private List<ScannedImage> GetImages(string imageUrl)
        {
            if (imageUrl.ContainsIgnoreCase("photoneeded.jpg") || imageUrl.ContainsIgnoreCase("new-item.jpg"))
                return new List<ScannedImage>();

            return new List<ScannedImage>
            {
                new ScannedImage(ImageVariantType.Primary, imageUrl.Replace("lo.jpg", "hi.jpg")),
                new ScannedImage(ImageVariantType.Primary, imageUrl),
            };
        }

        private UnitOfMeasure GetUnitOfMeasure(string content, ProductGroup group)
        {
            if (group == ProductGroup.Wallcovering) return UnitOfMeasure.Roll;
            var unitOfMeasure = UnitOfMeasure.Yard;
            if (content != null && content.Equals("100% Leather", StringComparison.OrdinalIgnoreCase))
                unitOfMeasure = UnitOfMeasure.SquareFoot;
            return unitOfMeasure;
        }

        private string FormatContent(string content, string collection)
        {
            if (collection.ContainsIgnoreCase("Leather") && !collection.ContainsIgnoreCase("Faux")) 
                return "100% Leather";

            content = content.ReplaceWholeWord("Cons", "Consumer");
            content = content.ReplaceWholeWord("Rcld", "Recycled");
            content = content.ReplaceWholeWord("Ind", "Industrial");
            return content;
        }

        private string FormatStyle(string style)
        {
            style = style.Replace("+%2f+", "/");
            style = style.Replace("%2f", "/");
            style = style.Replace("+-+", " ");
            style = style.Replace("+", " ");
            return style;
        }

        private string FormatColorName(string colorName)
        {
            colorName = colorName.Replace("-", " ");
            colorName = colorName.TitleCase();
            colorName = colorName.ReplaceWholeWord(" Cott", " Cotton");
            colorName = colorName.ReplaceWholeWord(" Diamon", " Diamond");
            colorName = colorName.ReplaceWholeWord(" Diamo", " Diamond");
            colorName = colorName.ReplaceWholeWord(" Diam", " Diamond");
            colorName = colorName.ReplaceWholeWord(" Dia", " Diamond");
            colorName = colorName.ReplaceWholeWord(" Di", " Diamond");
            colorName = colorName.ReplaceWholeWord(" Mouss", " Mousse");
            string correction;
            if (DicColorNameSpellingCorrections.TryGetValue(colorName, out correction))
                colorName = correction;
            return colorName;
        }

        private string FormatProductUse(string productUse, string other)
        {
            // if other has Indoor-Outdoor, we want to append to product use
            if (string.IsNullOrWhiteSpace(other) || other != "Indoor-Outdoor")
                return productUse;
            return ExtensionMethods.CombineProperty(productUse, "Indoor/Outdoor"); 
        }

        private static readonly Dictionary<string, string> DicColorNameSpellingCorrections = new Dictionary<string, string>
        {
            { "Grand CANY0N", "Grand Canyon"},
            { "Bronze/Waterfal", "Bronze/Waterfall"},
            { "Cocoa/Crimson/C", "Cocoa/Crimson/Celery"},
            { "Black Eyed Susa", "Black Eyed Susan"},
            { "Camel/Peppercor", "Camel/Peppercorn"},
            { "Ivory/Light Blu", "Ivory/Light Blue" },
            { "Light Blue/Ivor", "Light Blue/Ivory" },
            { "Light Green/Cre", "Light Green/Cream" },
            { "Light Blue/Brow", "Light Blue/Brown" },
            { "Spring Green W/", "Spring Green" },
        };
    }
}