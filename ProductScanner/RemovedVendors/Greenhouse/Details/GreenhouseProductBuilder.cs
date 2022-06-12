using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Greenhouse.Details
{
    public class GreenhouseProductBuilder : ProductBuilder<GreenhouseVendor>
    {
        public GreenhouseProductBuilder(IPriceCalculator<GreenhouseVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var stock = GetStock(data[ScanField.StockCount]);
            var colorName = FormatColorName(mpn);

            var vendor = new GreenhouseVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(stock), vendor);
            var isClearance = data[ScanField.IsClearance] == "true";

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            if (isClearance)
            {
                vendorProduct.HasSwatch = false;
                vendorProduct.MinimumQuantity = 5;
            }

            var book = data[ScanField.Book];
            vendorProduct.PublicProperties[ProductPropertyType.AverageBolt] = GetAverageBolt(data);
            vendorProduct.PublicProperties[ProductPropertyType.Backing] = GetBacking(data);
            vendorProduct.PublicProperties[ProductPropertyType.Book] = GetBook(book);
            vendorProduct.PublicProperties[ProductPropertyType.BookNumber] = GetBookNumber(book);
            vendorProduct.PublicProperties[ProductPropertyType.Color] = data[ScanField.Color].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content], data[ScanField.Category]);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = FormatCountry(data[ScanField.Country]);
            vendorProduct.PublicProperties[ProductPropertyType.Direction] = data[ScanField.Direction].Replace("Sampled ", "");
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability].Replace("* ", "").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = GetFinish(data);
            vendorProduct.PublicProperties[ProductPropertyType.HideSize] = GetHideSize(data);
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = FormatHorizontalRepeat(data[ScanField.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Match] = GetMatch(data);
            vendorProduct.PublicProperties[ProductPropertyType.Material] = GetMaterial(data);
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = mpn.Split(new[] { '-' }).First().TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.ProductUse].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ScanField.Style].Replace("-", " ").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = FormatVerticalRepeat(data[ScanField.Repeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].UnEscape().ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.CleaningCode] = data[ScanField.Cleaning];
            vendorProduct.PublicProperties[ProductPropertyType.FireCode] = data[ScanField.FireCode];

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = isClearance.ToString();
            vendorProduct.IsClearance = isClearance;

            vendorProduct.RemoveWhen(ProductPropertyType.Durability, s => s.ContainsIgnoreCase("none"));
            vendorProduct.RemoveWhen(ProductPropertyType.Durability, s => s.ContainsIgnoreCase("drapery"));

            vendorProduct.RemoveWhen(ProductPropertyType.PatternNumber, s => !s.ContainsDigit());

            vendorProduct.RemoveWhen(ProductPropertyType.HorizontalRepeat, s => s.ContainsIgnoreCase("none"));
            vendorProduct.RemoveWhen(ProductPropertyType.HorizontalRepeat, s => s.ContainsIgnoreCase("No Definite Repeat"));
            vendorProduct.RemoveWhen(ProductPropertyType.HorizontalRepeat, s => s.ContainsIgnoreCase("NDR"));
            vendorProduct.RemoveWhen(ProductPropertyType.HorizontalRepeat, s => s.ContainsIgnoreCase("Plain"));

            vendorProduct.RemoveWhen(ProductPropertyType.VerticalRepeat, s => s.ContainsIgnoreCase("none"));
            vendorProduct.RemoveWhen(ProductPropertyType.VerticalRepeat, s => s.ContainsIgnoreCase("No Definite Repeat"));
            vendorProduct.RemoveWhen(ProductPropertyType.VerticalRepeat, s => s.ContainsIgnoreCase("NDR"));
            vendorProduct.RemoveWhen(ProductPropertyType.VerticalRepeat, s => s.ContainsIgnoreCase("Plain"));

            var patternNumber = "";
            if (vendorProduct.PublicProperties.ContainsKey(ProductPropertyType.PatternNumber))
                patternNumber = vendorProduct.PublicProperties[ProductPropertyType.PatternNumber];

            vendorProduct.Correlator = data[ScanField.PatternCorrelator];
            vendorProduct.IsDiscontinued = IsDiscontinued(data[ScanField.Status], stock);
            vendorProduct.Name = new[] {patternNumber, colorName, "by", vendor.DisplayName}.BuildName();
            vendorProduct.SetProductGroup(ProductGroup.Fabric);
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();
            return vendorProduct;
        }

        private double GetStock(string inventory)
        {
            if (inventory == null) inventory = "0";
            inventory = inventory.Replace(" yards", "");
            inventory = inventory.Replace(" yard", "");
            inventory = inventory.Replace(" in stock", "");
            inventory = inventory.Replace("Â", "");
            inventory = inventory.Replace("¼", "");
            inventory = inventory.Replace("½", "");
            return inventory.Replace("¾", "").ToDoubleSafe();
        }

        private bool IsDiscontinued(string status, double stockCount)
        {
            return stockCount < 1 && status.Contains("Discontinued");
        }

        private string GetBook(string book)
        {
            book = book.Replace("&amp;", "&");
            var parts = book.Replace("greenhouse", "Greenhouse").Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Any()) return parts.Last();
            return null;
        }

        private string GetBookNumber(string book)
        {
            var parts = book.Replace("greenhouse", "Greenhouse").Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Any()) return parts.First();
            return null;
        }

        private string FormatColorName(string mpn)
        {
            var colorName = mpn.Split(new[] { '-' }).Skip(1).Aggregate((a, b) => a + "-" + b);
            colorName = colorName.Replace("-", " ");

            colorName = colorName.Replace(" 30YD", "");
            colorName = colorName.Replace("WHT", "White");
            colorName = colorName.Replace(" D ", " Dark ");
            colorName = colorName.Replace(" ISL ", " Islander ");
            return colorName.TitleCase().ToFormattedColors();
        }

        private string FormatVerticalRepeat(string repeat)
        {
            repeat = repeat.Replace("\"H", " \"H").UnEscape();
            if (!repeat.ContainsIgnoreCase("V")) return null;

            repeat = repeat.CaptureWithinMatchedPattern(@"(?<capture>(\d+\.\d+))" + "\" V");
            return repeat.ToInchesMeasurement();
        }

        private string FormatHorizontalRepeat(string repeat)
        {
            repeat = repeat.Replace("\"H", " \"H").UnEscape();
            if (!repeat.ContainsIgnoreCase("H")) return null;

            repeat = repeat.CaptureWithinMatchedPattern(@"(?<capture>(\d+\.\d+))" + "\" H");
            return repeat.ToInchesMeasurement();
        }

        private string FormatCountry(string country)
        {
            if (string.Equals(country, "Use", StringComparison.OrdinalIgnoreCase))
                return null;

            country = country.Replace("Country of origin:&nbsp;", "").Trim();
            if (country.Equals("Use", StringComparison.OrdinalIgnoreCase))
                country = "USA";

            return new Country(country).Format();
        }

        private string FormatContent(string content, string material)
        {
            // if the product is category=leather, return "100% Leather"
            if (material == "leather") 
                return "100% Leather";

            // some of these fields don't contain anything content related
            if (!content.Contains("%")) return null;
            if (content.Contains("Backing") && !content.Contains("Face")) return null;

            content = content.RemovePattern("Backing:.*");
            content = content.RemovePattern("Back:.*");
            content = content.Replace("Face: ", "");
            content = content.Replace("Base: ", "");
            content = content.Replace("Surface Type: ", "");
            content = content.Trim(new[] { ',', ' ' });

            return content.TitleCase();
        }

        private string GetAverageBolt(ScanData data)
        {
            var length = GetContentMatchRegex(data, _lengths);
            if (length == null) return null;

            return length + " Yards";
        }

        private string GetBacking(ScanData data)
        {
            var backing = GetContentMatchRegex(data, _backing);
            if (backing == null) return null;
            if (backing.ContainsIgnoreCase("None")) return null;

            backing = backing.Replace("®", "");
            backing = backing.Replace("Â", "");
            if (backing == "Sbr Latex") return "SBR Latex";
            if (backing == "S.B.R Latex") return "SBR Latex";
            return backing;
        }

        private string GetFinish(ScanData data)
        {
            var finish = data[ScanField.Finish];
            if (finish == null)
            {
                return GetContentMatchWithReplace(data, _finishes);
            }
            if (finish.ContainsIgnoreCase("none")) return null;

            return finish.UnEscape().TitleCase();
        }

        private string GetHideSize(ScanData data)
        {
            var size = GetContentMatchRegex(data, _sizes);
            if (size == null) return null;

            return size + " square feet";
        }

        private string GetMatch(ScanData data)
        {
            var content = GetContentFields(data);
            if (content.Any(x => x.Contains("Half Drop"))) return "Half Drop";

            return null;
        }

        private string GetMaterial(ScanData data)
        {
            var category = data[ScanField.Category];
            if (category != null && category.Contains("Made in USA")) return null;
            var material = category ?? GetContentMatchContains(data, _materials);
            return material.Replace("-", " ").TitleCase();
        }

        private string GetContentMatchRegex(ScanData data, List<string> regexes)
        {
            var contentFields = GetContentFields(data);
            foreach (var regex in regexes)
            {
                var captures = contentFields.Select(x => x.CaptureWithinMatchedPattern(regex)).ToList();
                var found = captures.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (found != null) return found;
            }
            return null;
        }

        private List<string> GetContentFields(ScanData data)
        {
            return new List<string>
            {
                data[ScanField.Content],
                data[ScanField.Bullet1],
                data[ScanField.Bullet2],
                data[ScanField.Bullet3],
                data[ScanField.Bullet4],
                data[ScanField.Bullet5],
                data[ScanField.Bullet6],
            }.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }

        private string GetContentMatchWithReplace(ScanData data, Dictionary<string, string> values)
        {
            var contentFields = GetContentFields(data);
            foreach (var value in values)
            {
                var found = contentFields.FirstOrDefault(x => x.Contains(value.Key));
                if (found != null) return value.Value;
            }
            return null;
        }

        private string GetContentMatchContains(ScanData data, IEnumerable<string> values)
        {
            var contentFields = GetContentFields(data);
            foreach (var value in values)
            {
                var found = contentFields.FirstOrDefault(x => x.Contains(value));
                return found;
            }
            return null;
        }

        private readonly List<string> _lengths = new List<string>
        {
            @"Yards Per Roll: (?<yards>\d+) Yards",
            @"(?<yards>\d+) Yd Roll",
            @"(?<yards>\d+) Yds/Rl",
            @"Roll Size: (?<yards>\d+) Yards",
            @"Roll Size: (?<yards>\d+) Yds",
            @"Roll Size: (?<yards>\d+) Yd",
            @"Roll Size: (?<yards>\d+)Yds",
        };

        private readonly List<string> _sizes = new List<string>
        {
            @"Hide Size: (?<size>\d+) Sq Ft",
            @"Size: (?<size>\d+) Sq Ft",
            @"Hide Size (?<size>\d+) Sq Ft",
            @"Hide Size: (?<size>\d+) Sf",
            @"Hide Size: (?<size>\d+) Sf",
        };

        private readonly List<string> _backing = new List<string>
        {
            @"Backing: (?<backing>.*)$",
            @"Back: (?<backing>.*)$",
            @"Backing Contents: (?<backing>.*)$"
        };

        private readonly List<string> _materials = new List<string>
        {
            "Bovine Hide",
            "Vinyl / Urethane Topcoat",
            "European Premium Selected Leather Hides",
            "South American Cowhide",
            //"Raw Material: South American",
            "European Cowhide",
            "Raw Material: Select European Bovine",
            "Top Grain"
            //"Raw Material: European"
        };

        private readonly Dictionary<string, string> _finishes = new Dictionary<string, string>
        {
            { "Blockaide", "Blockaide Treatment" },
            { "Mildew Resistant", "Mildew Resistant" },
            { "Water & Stain Resistant", "Water & Stain Resistant" },
            { "Water & Stain Restant", "Water & Stain Resistant" },
            { "Stain Repellant", "Stain Repellant" },
            { "Polyurethane Top Coat", "Polyurethane Top Coat" },
            { "Seimi-Aniline", "Semi-Aniline" },
            { "Resilience", "Resilience" },
            { "Lustre", "Lustre" },
            { "Full Pigment", "Full Pigment" },
            { "Crease Resistant", "Crease Resistant" },
            { "Oil Resistant", "Oil Resistant" },
            { "Anti-Stain", "Anti-Stain Finish" },
            { "Pigmented", "Pigmented" },
            { "Aniline Dyed", "Aniline Dyed" },
            { "Drum-Dyed Through", "Drum-Dyed Through" },
            { "Scotchgard", "Scotchgard" },
            { "Polyester Binder", "Polyester Binder" },
        };
    }
}
