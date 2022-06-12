using System;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace RobertAllen.Details
{
    public class RobertAllenPriceCalculator : DefaultPriceCalculator<RobertAllenVendor>
    {
        private bool UsingMAP(string brand)
        {
            // every other product uses MAP
            return !brand.ContainsIgnoreCase("Contract") && !brand.ContainsIgnoreCase("@Home");
        }

        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var cost = data.Cost;
            if (UsingMAP(data[ScanField.Brand]))
            {
                return CalculateRobertAllenPrice(cost);
            }
            return base.CalculatePrice(data);
        }

        private ProductPriceData CalculateRobertAllenPrice(decimal cost)
        {
            var ourPrice = Math.Round(cost * 1.5M, 2);
            var msrp = Math.Round(ourPrice*1.9M, 2);
            return new ProductPriceData(ourPrice, msrp);
        }
    }

    public class RobertAllenProductBuilder : RobertAllenBaseProductBuilder<RobertAllenVendor>
    {
        public RobertAllenProductBuilder(IPriceCalculator<RobertAllenVendor> priceCalculator) : base(priceCalculator) { }
    }

    public class BeaconHillProductBuilder : RobertAllenBaseProductBuilder<BeaconHillVendor>
    {
        public BeaconHillProductBuilder(IPriceCalculator<BeaconHillVendor> priceCalculator) : base(priceCalculator) { }
    }

    public class RobertAllenBaseProductBuilder<T> : ProductBuilder<T> where T : Vendor, new()
    {
        public RobertAllenBaseProductBuilder(IPriceCalculator<T> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var patternName = FormatPatternName(data[ScanField.PatternName]);
            var colorName = FormatColorName(data[ScanField.PatternName], data[ScanField.ColorName]);
            var isLimitedAvailability = data.IsLimitedAvailability;

            var vendor = new T();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var stockCount = data[ScanField.StockCount].ToDoubleSafe();
            var stock = new StockData(stockCount);
            if (isLimitedAvailability) stock.InStock = true;
            var vendorProduct = new FabricProduct(mpn, data.Cost, stock, vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.Book] = FormatBook(data[ScanField.Book]);
            vendorProduct.PublicProperties[ProductPropertyType.BookNumber] = data[ScanField.BookNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Brand] = FormatBrand(data[ScanField.Brand]);
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = FormatCleaning(data[ScanField.Cleaning]);
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = FormatCollection(data[ScanField.Collection]);
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content]);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Direction] = data[ScanField.Direction];
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(data[ScanField.FabricPerformance]);
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = data[ScanField.Finish];
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.Use];
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = string.Equals(data[ScanField.Railroaded], "Yes") ? "Yes" : null;
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = FormatWidth(data[ScanField.Width]);

            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = data[ScanField.ColorNumber];
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternNumber];
            //vendorProduct.PublicProperties[ProductPropertyType.FurnitureGrade] = data[ScanField.FurnitureGrade];
            //vendorProduct.PublicProperties[ProductPropertyType.SoftHomeGrade] = data[ScanField.SoftHomeGrade];

            // right now this is never actually hitting, because the bolt program products do not have prices
            //vendorProduct.PublicProperties[ProductPropertyType.OrderRequirementsNotice] = data[ScanField.TempContent1].ContainsIgnoreCase("Bolt program") ? "Please contact us for availability" : "";

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = isLimitedAvailability.ToString();

            //vendorProduct.IsClearance = isLimitedAvailability;
            vendorProduct.HasSwatch = !isLimitedAvailability;
            if (data.RemoveSwatch) vendorProduct.HasSwatch = false;

            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName);
            vendorProduct.Name = new[] { mpn, patternName, colorName, "by", vendor.DisplayName }.BuildName();
            vendorProduct.IsDiscontinued = data.IsDiscontinued;
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SetProductGroup(data[ScanField.ProductGroup].ToProductGroup());
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private string FormatCollection(string collection)
        {
            collection = collection.TitleCase().Replace(" Collection", "");
            collection = collection.Replace("Uph", "Upholstery");
            collection = collection.RomanNumeralCase();
            return collection;
        }

        private string FormatCleaning(string cleaning)
        {
            cleaning = cleaning.Replace("WashablePure", "Washable Pure");
            cleaning = cleaning.Replace("CleanableWater", "Cleanable Water");
            return cleaning;
        }

        private string FormatBrand(string brand)
        {
            brand = brand.TitleCase();
            return brand.Replace("Robert Allen @home", "Robert Allen@Home");
        }

        private string FormatPatternName(string patternName)
        {
            if (patternName.Contains("|"))
            {
                patternName = patternName.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries).First();
            }

            patternName = patternName.TitleCase().RomanNumeralCase();
            patternName = patternName.ReplaceWholeWord("Lt", "Light");
            return patternName;
        }

        private string FormatContent(string content)
        {
            content = content.TitleCase();
            content = content.ReplaceWholeWord("Cot", "Cotton");
            content = content.ReplaceWholeWord("Ray", "Rayon");
            content = content.ReplaceWholeWord("Poly", "Polyester");
            content = content.ReplaceWholeWord("Acr", "Acrylic");
            content = content.ReplaceWholeWord("Vir", "Virgin");
            content = content.ReplaceWholeWord("Nyl", "Nylon");
            content = content.ReplaceWholeWord("Lin", "Linen");
            content = content.ReplaceWholeWord("Ln", "Linen");
            content = content.ReplaceWholeWord("Pp", "Polypropylene");
            content = content.ReplaceWholeWord("Vis", "Viscose");
            content = content.ReplaceBR(", ").Replace("  ", " ").TitleCase().ToFormattedFabricContent();
            return content;
        }

        private string FormatBook(string book)
        {
            book = book.TitleCase().RomanNumeralCase();
            book = book.ReplaceWholeWord("-", " ");
            book = book.ReplaceWholeWord("Dk", "Dark");
            book = book.ReplaceWholeWord("Lt", "Light");
            book = book.ReplaceWholeWord("Neut", "Neutral");
            book = book.ReplaceWholeWord("Plds", "Plaids");
            book = book.ReplaceWholeWord("Strps", "Stripes");
            book = book.ReplaceWholeWord("Blu", "Blue");
            book = book.ReplaceWholeWord("Brwn/Gld", "Brown/Gold");
            book = book.ReplaceWholeWord("Pl", "Plum");
            book = book.Replace(" Coll ", " ");
            return book;
        }

        private string FormatColorName(string patternName, string colorName)
        {
            if (patternName.Contains("|"))
            {
                colorName = patternName.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries).Last();
            }

            if (colorName == "P1423YELLOW") return "Yellow";
            if (colorName == "1018-BUTTER") return "Butter";
            if (colorName == "1243-TUSCAN") return "Tuscan";
            if (colorName == "1018-MIST") return "Mist";
            if (colorName == "1018-NAVY") return "Navy";
            if (colorName == "1018-CLARET") return "Claret";
            if (colorName == "492-GARDEN Grov") return "Garden Grove";
            if (colorName == "PORTOBELL0") return "Portobello";
            if (colorName == "Honeysuckl") return "Honeysuckle";

            colorName = colorName.Replace("9119-", " ");
            colorName = colorName.Replace("1243-", " ");
            colorName = colorName.Replace("924-", " ");
            colorName = colorName.Replace("502-", " ");
            colorName = colorName.Replace("501-", " ");
            colorName = colorName.Replace("492-", " ");
            colorName = colorName.Replace("126-", " ");
            colorName = colorName.Replace("-", " ");
            return colorName.ToFormattedColors().TitleCase().RomanNumeralCase();
        }

        private string FormatDurability(string d)
        {
            const string dblrubs = "Double Rubs";

            d = d.ReplaceBR(", ").Replace("<>", ", ");

            // they have way too much stuff for this property - just keep double rubs

            var index = d.ToLower().IndexOf(dblrubs);

            if (index == -1)
                return null;

            var d2 = d.Substring(0, index + dblrubs.Length);
            return d2.TitleCase();
        }

        private string FormatWidth(string width)
        {
            var w = width.Replace(" 1/2\"", ".5").Replace("\"", "");
            if (w.EndsWith(" 1/2"))
                w = w.Replace(" 1/2", ".5");
            return w == "1" ? w + " inch" : w + " inches"; 
        }
    }
}