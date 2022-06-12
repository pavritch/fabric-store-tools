using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Fabricut.Details
{
    public class FabricutProductBuilder : FabricutBaseProductBuilder<FabricutVendor>
    {
        public FabricutProductBuilder(IPriceCalculator<FabricutVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class SHarrisProductBuilder : FabricutBaseProductBuilder<SHarrisVendor>
    {
        public SHarrisProductBuilder(IPriceCalculator<SHarrisVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class StroheimProductBuilder : FabricutBaseProductBuilder<StroheimVendor>
    {
        public StroheimProductBuilder(IPriceCalculator<StroheimVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class TrendProductBuilder : FabricutBaseProductBuilder<TrendVendor>
    {
        public TrendProductBuilder(IPriceCalculator<TrendVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class VervainProductBuilder : FabricutBaseProductBuilder<VervainVendor>
    {
        public VervainProductBuilder(IPriceCalculator<VervainVendor> priceCalculator, IDesignerFileLoader designerFileLoader)
            : base(priceCalculator, designerFileLoader) { }
    }

    public class FabricutBaseProductBuilder<T> : ProductBuilder<T> where T:Vendor, new()
    {
        private const string ImageBaseUrl = "http://scanner.insidefabric.com/vendors/Fabricut/{0}.jpg";
        private readonly IDesignerFileLoader _designerFileLoader;

        public FabricutBaseProductBuilder(IPriceCalculator<T> priceCalculator, IDesignerFileLoader designerFileLoader) : base(priceCalculator)
        {
            _designerFileLoader = designerFileLoader;
        }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var cost = CalculatePricePerRoll(data[ScanField.UnitOfMeasure], data[ScanField.Cost].ToDecimalSafe());
            var stock = data[ScanField.StockCount];
            var patternName = data[ScanField.PatternName].TitleCase().RomanNumeralCase();
            var colorName = FormatColorName(data[ScanField.ColorName]);
            var itemNumber = data[ScanField.ItemNumber];

            var vendor = new T();

            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, new StockData(InStock(stock)), vendor);
            vendorProduct.MinimumQuantity = GetMinQuantity(data[ScanField.UnitOfMeasure]);
            vendorProduct.OrderIncrement = GetOrderIncrement(data[ScanField.UnitOfMeasure]);

            if (data.IsClearance)
            {
                vendorProduct.MinimumQuantity = 5;
                vendorProduct.HasSwatch = false;
            }

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);
            var colorNumber = mpn.Substring(mpn.Length - 2);
            var patternNumber = mpn.Substring(0, mpn.Length - 2);
            var productGroup = GetProductGroup(data);

            var book = FormatBook(data[ScanField.Book]);

            vendorProduct.PublicProperties[ProductPropertyType.Book] = book;
            vendorProduct.PublicProperties[ProductPropertyType.BookNumber] = data[ScanField.BookNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Category] = FormatCategory(data);
            vendorProduct.PublicProperties[ProductPropertyType.CleaningCode] = FormatCleaningCode(data[ScanField.Cleaning]);
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = FormatCollection(data[ScanField.Collection], data[ScanField.Book]);
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = colorNumber;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content].ToFormattedFabricContent();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country].TitleCase()).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Designer] = GetDesigner(book);
            vendorProduct.PublicProperties[ProductPropertyType.Drop] = data[ScanField.Drop].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(data[ScanField.Durability]);
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = FormatFinish(data[ScanField.Finish]);
            vendorProduct.PublicProperties[ProductPropertyType.FlameRetardant] = data[ScanField.FlameRetardant].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.InnerDiameter] = data[ScanField.InnerDiameter].ToInchesMeasurement();
            //vendorProduct.PublicProperties[ProductPropertyType.Note] = FormatNote(data[ScanField.TempContent6]);
            vendorProduct.PublicProperties[ProductPropertyType.OuterDiameter] = data[ScanField.OuterDiameter].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = patternNumber;
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = GetProductUse(data[ScanField.Use]);
            vendorProduct.PublicProperties[ProductPropertyType.ProductType] = data[ScanField.ProductType];
            //vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = FormatRailroaded(data[ScanField.TempContent6]);
            vendorProduct.PublicProperties[ProductPropertyType.Size] = data[ScanField.Size].Replace("Size: ", "").ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Style] = FormatStyle(data);
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].ToInchesMeasurement();

            var unit = GetUnitOfMeasure(data[ScanField.UnitOfMeasure]);

            // for Fabric, AverageBolt = AverageBolt - for Wallcovering, AverageBolt contains the Roll Length
            if (productGroup == ProductGroup.Fabric)
                vendorProduct.PublicProperties[ProductPropertyType.AverageBolt] = data[ScanField.AverageBolt];
            else if (productGroup == ProductGroup.Wallcovering)
            {
                if (unit == UnitOfMeasure.Roll)
                {
                    var numRolls = GetNumRolls(data[ScanField.UnitOfMeasure]);
                    var widthInInches = data[ScanField.Width].ToDoubleSafe();
                    var lengthInYards = data[ScanField.AverageBolt].ToDoubleSafe();
                    vendorProduct.PublicProperties[ProductPropertyType.Length] = data[ScanField.AverageBolt] + " yards";

                    var lengthInInches = lengthInYards*12*3;
                    var dimensions = new RollDimensions(widthInInches, lengthInInches, numRolls);
                    vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                    vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                    vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();
                    vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(numRolls);

                    vendorProduct.NormalizeWallpaperPricing(numRolls);
                    vendorProduct.MinimumQuantity = 1;
                    vendorProduct.OrderIncrement = 1;
                }
                else if (unit == UnitOfMeasure.Yard)
                {
                    // in this case, the AverageBolt value is actually the order increment
                    var orderIncrement = data[ScanField.AverageBolt].ToIntegerSafe();
                    vendorProduct.MinimumQuantity = orderIncrement;
                    vendorProduct.OrderIncrement = orderIncrement;
                }
            }

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = string.Format("{0}product_search.php?action=details&product_id={1}&application_id=F",
                vendor.PublicUrl, mpn);
            vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = data[ScanField.IsLimitedAvailability];

            vendorProduct.IsDiscontinued = data[ScanField.IsDiscontinued] == "Yes";
            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            var primaryImageUrlOurServer = string.Format(ImageBaseUrl, itemNumber);
            var primaryImageUrl = string.Format("{0}product_images/{1}.jpg", vendor.PublicUrl.Replace("https", "http"), itemNumber);
            var primaryImageUrlAlt = string.Format("https://img3.fabricut.com/product_images/{0}.jpg", itemNumber);
            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName);
            vendorProduct.IsClearance = data.IsClearance;
            vendorProduct.Name = new[] { mpn, patternName, colorName, "by", vendor.DisplayName }.BuildName();
            vendorProduct.SetProductGroup(productGroup);
            vendorProduct.ScannedImages = new List<ScannedImage> {new ScannedImage(ImageVariantType.Primary, primaryImageUrlOurServer), 
                new ScannedImage(ImageVariantType.Primary, primaryImageUrl),
            new ScannedImage(ImageVariantType.Primary, primaryImageUrlAlt)};
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = unit;

            return vendorProduct;
        }

        private string GetDesigner(string book)
        {
            var validDesigners = _designerFileLoader.GetDesigners();
            var match = validDesigners.FirstOrDefault(x => book.ToLower().Contains(x.ToLower()));
            return match ?? string.Empty;
        }

        private string FormatColorName(string color)
        {
            color = Regex.Replace(color, @"^[\d]*", "");
            return color.TitleCase();
        }

        private decimal CalculatePricePerRoll(string unit, decimal wholesale)
        {
            if (unit.Equals("dblrl", StringComparison.OrdinalIgnoreCase))
                return Math.Round(wholesale/2, 2);
            if (unit.Equals("tplrl", StringComparison.OrdinalIgnoreCase))
                return Math.Round(wholesale/3, 2);
            return wholesale;
        }

        private int GetNumRolls(string unit)
        {
            if (unit == "TPLRL") return 3;
            return 2;
        }

        private string FormatFinish(string finish)
        {
            finish = finish.Replace("@", "");
            return finish.TitleCase();
        }

        private string GetProductUse(string use)
        {
            var uses = new List<string>();
            if (use.Contains("B")) uses.Add("Bedding");
            if (use.Contains("D")) uses.Add("Drapery");
            if (use.Contains("M")) uses.Add("Multipurpose");
            if (use.Contains("U")) uses.Add("Upholstery");
            return string.Join(", ", uses);
        }

        private string FormatDurability(string durability)
        {
            if (string.IsNullOrWhiteSpace(durability)) return string.Empty;
            return durability.Replace("EXCEEDS ", "").Replace("XCEEDS", "").TitleCase();
        }

        private int GetMinQuantity(string unitOfMeasure)
        {
            if (unitOfMeasure == "TPLRL") return 3;
            if (unitOfMeasure == "DBLRL") return 2;
            return 1;
        }

        private int GetOrderIncrement(string unitOfMeasure)
        {
            if (unitOfMeasure == "TPLRL") return 3;
            if (unitOfMeasure == "DBLRL") return 2;
            return 1;
        }

        private UnitOfMeasure GetUnitOfMeasure(string value)
        {
            value = value.Replace("YARDS", "YARD");
            if (value == string.Empty) return UnitOfMeasure.None;
            if (value.Equals("dblrl", StringComparison.OrdinalIgnoreCase) || value.Equals("tplrl", StringComparison.OrdinalIgnoreCase))
                value = "Roll";
            return (UnitOfMeasure)Enum.Parse(typeof (UnitOfMeasure), value, true);
        }

        private int InStock(string stock)
        {
            // not really optiona, but have observed (Stroheim) that some do records do not include this field
            // so we fake out having something

            //if (stock == null)
                //stock = "1 yard(s)";

            stock = stock.Replace("yards", "").Trim();

            if (string.Equals(stock, "no longer available", StringComparison.OrdinalIgnoreCase))
                return 0;

            if (string.Equals(stock, "not in stock", StringComparison.OrdinalIgnoreCase))
                return 0;

            var count = stock.TakeOnlyFirstIntegerToken();
            return count;
        }

        private ProductGroup GetProductGroup(ScanData data)
        {
            var prodGroup = data[ScanField.ProductGroup];
            if (prodGroup == "FAB") return ProductGroup.Fabric;
            if (prodGroup == "TRM") return ProductGroup.Trim;
            if (prodGroup == "WC") return ProductGroup.Wallcovering;
            //if (prodGroup == "HARDWARE") return ProductGroup.Hardware;
            return ProductGroup.None;
        }

        private string FormatBook(string book)
        {
            if (book.Contains("NO SAMPLE BOOK")) return string.Empty;
            return book.TitleCase().RomanNumeralCase().Replace(" Collection", "");
        }

        private string FormatCategory(ScanData data)
        {
            var categoryValues = new List<string>
            {
                data[ScanField.Category1],
                data[ScanField.Category2],
                data[ScanField.Category3],
                data[ScanField.Category4],
            }.Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(", ", categoryValues).Replace(" / ", ", ")
                .Replace("NFPA 701 FR, ", "")
                .Replace(", NFPA 701 FR", "").TitleCase();
        }

        private string FormatStyle(ScanData data)
        {
            var styleValues = new List<string>
            {
                data[ScanField.Style1],
                data[ScanField.Style2],
                data[ScanField.Style3],
                data[ScanField.Style4],
            }.Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(", ", styleValues).Replace(" / ", ", ").TitleCase();
        }

        private string FormatCleaningCode(string other)
        {
            var cleaningCodeMatch = Regex.Match(other, @"(((^)|[,\s])CLEANING CODE-)(?<tag>[A-Z]{1,2})($|[,\s)])");

            if (cleaningCodeMatch.Success)
                return cleaningCodeMatch.Groups["tag"].Value.Trim();

            return null;
        }

        private string FormatCollection(string collection, string book)
        {
            if (string.IsNullOrWhiteSpace(collection))
                return null;

            // remove anything in parenthesis and possbile trailing space

            var regEx = new Regex(@"\([\w\s]+?\)\s*");

            collection = regEx.Replace(collection, string.Empty);

            if (collection == "None")
                return null;

            // if result is exactly same as Book, keep book, skip collection.

            if (collection == book)
                return null;

            collection = collection.TitleCase().RomanNumeralCase();
            collection = collection.MakeTheseTokensUpperCase(new[] {"Xix", "Xx", "Xxi"});

            return collection.Replace(" Collection", "");
        }

        private string FormatNote(string other)
        {
            return Regex.IsMatch(other, @"((^)|[,\s])SHOWN RAILROADED($|[,\s)])") ? "Shown Railroaded" : null;
        }

        private string FormatRailroaded(string other)
        {
            if (Regex.IsMatch(other, @"((^)|[,\s])SHOWN RAILROADED($|[,\s)])"))
                return "Yes";

            return null;
        }
    }
}