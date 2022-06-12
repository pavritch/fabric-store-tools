using System;
using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Brewster.Details
{
    public class BrewsterProductBuilder : ProductBuilder<BrewsterVendor>
    {
        private readonly IDesignerFileLoader _designerFileLoader;

        public BrewsterProductBuilder(IPriceCalculator<BrewsterVendor> priceCalculator, IDesignerFileLoader designerFileLoader) : base(priceCalculator)
        {
            _designerFileLoader = designerFileLoader;
        }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var stock = data[ScanField.StockCount];
            var vendor = new BrewsterVendor();

            //if (data.IsBolt)
            {
                //data.Cost /= 2;
            }
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(InStock(stock)), vendor);

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var brand = FormatBrand(data[ScanField.Brand]);
            vendorProduct.PublicProperties[ProductPropertyType.AdditionalInfo] = FormatAdditionalInfo(data);
            vendorProduct.PublicProperties[ProductPropertyType.Book] = data[ScanField.Book];
            vendorProduct.PublicProperties[ProductPropertyType.BookNumber] = data[ScanField.BookNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Brand] = brand;
            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.Category].Replace("-", " ").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = FormatCleaning(data);
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection];
            vendorProduct.PublicProperties[ProductPropertyType.Color] = data[ScanField.Color].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = FormatColorGroup(data[ScanField.ColorGroup]);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Depth] = data[ScanField.Depth].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Designer] = GetDesigner(brand);
            vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = FormatDimensions(data);
            vendorProduct.PublicProperties[ProductPropertyType.EdgeFeature] = data[ScanField.EdgeFeature].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Height] = MakeMeasurement(data[ScanField.Height]);
            vendorProduct.PublicProperties[ProductPropertyType.League] = data[ScanField.League];
            vendorProduct.PublicProperties[ProductPropertyType.Length] = MakeMeasurement(data[ScanField.Length]);
            vendorProduct.PublicProperties[ProductPropertyType.Match] = FormatMatch(data[ScanField.Match]);
            vendorProduct.PublicProperties[ProductPropertyType.Material] = FormatMaterial(data);
            vendorProduct.PublicProperties[ProductPropertyType.NumberOfPanels] = FormatNumberOfPanels(data);
            vendorProduct.PublicProperties[ProductPropertyType.NumberOfPieces] = FormatNumberOfPieces(data);
            vendorProduct.PublicProperties[ProductPropertyType.NumberOfSheets] = FormatNumberOfSheets(data);
            vendorProduct.PublicProperties[ProductPropertyType.PackageHeight] = data[ScanField.PackageHeight];
            vendorProduct.PublicProperties[ProductPropertyType.PackageLength] = data[ScanField.PackageLength];
            vendorProduct.PublicProperties[ProductPropertyType.PackageWidth] = data[ScanField.PackageWidth];
            vendorProduct.PublicProperties[ProductPropertyType.Prepasted] = FormatPrepasted(data);
            vendorProduct.PublicProperties[ProductPropertyType.ProductName] = data[ScanField.ProductName];
            vendorProduct.PublicProperties[ProductPropertyType.ProductType] = FormatProductType(data);
            vendorProduct.PublicProperties[ProductPropertyType.Repeat] = FormatRepeat(data);
            vendorProduct.PublicProperties[ProductPropertyType.Strippable] = FormatStrippable(data[ScanField.Strippable]);
            vendorProduct.PublicProperties[ProductPropertyType.Style] = FormatStyle(data);
            vendorProduct.PublicProperties[ProductPropertyType.Theme] = data[ScanField.Theme];
            vendorProduct.PublicProperties[ProductPropertyType.UPC] = data[ScanField.UPC];
            vendorProduct.PublicProperties[ProductPropertyType.Width] = MakeMeasurement(data[ScanField.Width]);

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            if (!string.IsNullOrWhiteSpace(data[ScanField.ImageUrl]))
                vendorProduct.ScannedImages = new List<ScannedImage> {new ScannedImage(ImageVariantType.Primary, data[ScanField.ImageUrl])};
            vendorProduct.ScannedImages.Add(new ScannedImage(ImageVariantType.Primary, string.Format("http://www.brewsterwallcovering.com/data/default/images/catalog/original/{0}.jpg", mpn)));

            var dimensions = GetDimensions(vendorProduct.PublicProperties[ProductPropertyType.Length], vendorProduct.PublicProperties[ProductPropertyType.Width]);
            //vendorProduct.Correlator = data[ScanField.PatternCorrelator];
            vendorProduct.Correlator = mpn;
            vendorProduct.IsDiscontinued = data.IsDiscontinued;
            vendorProduct.ManufacturerDescription = data[ScanField.Description];
            vendorProduct.Name = BuildName(data, vendor);
            vendorProduct.ProductClass = GetProductClass(data[ScanField.ProductType]);
            vendorProduct.SetProductGroup(ProductGroup.Wallcovering);
            vendorProduct.SKU = string.Format("{0}-{1}", vendor.SkuPrefix, mpn.SkuTweaks());
            vendorProduct.UnitOfMeasure = GetUnitOfMeasure(data);

            var orderIncrement = GetOrderIncrement(data[ScanField.UnitOfMeasure], data[ScanField.OrderInfo]);

            vendorProduct.MinimumQuantity = orderIncrement;
            vendorProduct.OrderIncrement = orderIncrement;

            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Roll)
            {
                if (dimensions != null)
                {
                    vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                    vendorProduct.PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthInYardsFormatted();
                }
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = FormatCoverage(data[ScanField.Coverage]);
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(orderIncrement);

                if (data.IsBolt)
                {
                    vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(1);
                }

                vendorProduct.NormalizeWallpaperPricing(orderIncrement);
                vendorProduct.MinimumQuantity = 1;
                vendorProduct.OrderIncrement = 1;
            }

            return vendorProduct;
        }

        private string FormatBrand(string brand)
        {
            return brand.Replace("Eiffinger", "Eijffinger").Replace("Boråstapeter", "Borastapeter");
        }

        private string GetDesigner(string brand)
        {
            var validDesigners = _designerFileLoader.GetDesigners().Select(x => x.ToLower());
            return validDesigners.Contains(brand.ToLower()) ? brand : string.Empty;
        }

        private int GetOrderIncrement(string unit, string orderInfo)
        {
            if (unit.ContainsIgnoreCase("Single Roll")) return 2;
            if (orderInfo.ContainsIgnoreCase("Single Roll")) return 2;
            return 1;

            /*
            if (orderInfo.ContainsIgnoreCase("Unit")) return 1;
            if (isBolt) return 1;

            // The Wall Vision and Brewster Essential lines are sold online only and are sold and shipped as 1 unit ea.
            if (mpn.StartsWith("WV")) return 1;
            if (mpn.StartsWith("NU")) return 1;
            if (mpn.StartsWith("FD") && !mpn.StartsWith("FDB")) return 1;

            if (productClass == ProductClass.WallMurals) return 1;
            if (productClass == ProductClass.Border) return 1;
            if (productClass == ProductClass.Applique) return 1;
            if (productClass == ProductClass.WallDecals) return 1;
            if (productClass == ProductClass.AdhesiveFilm) return 1;
            return 2;
            */
        }

        private UnitOfMeasure GetUnitOfMeasure(ScanData data)
        {
            if (data[ScanField.UnitOfMeasure].ContainsIgnoreCase("Roll")) return UnitOfMeasure.Roll;
            if (data[ScanField.ProductName].ContainsIgnoreCase("wallpaper")) return UnitOfMeasure.Roll;

            if (!data.ContainsKey(ScanField.ProductType)) return UnitOfMeasure.Each;

            var type = data[ScanField.ProductType];
            if (type == "W" || type == "B" || type == "Border" || type == "Sidewall" || type == "Sidewalls" || type == "Liner") return UnitOfMeasure.Roll;
            return UnitOfMeasure.Each;
        }

        private RollDimensions GetDimensions(string length, string width)
        {
            if (length == null || width == null) return null;

            var lengthInInches = 0d;
            if (length.Contains("inches")) lengthInInches = length.Replace(" inches", "").ToDoubleSafe();
            else lengthInInches = length.Replace(" ft.", "").ToDoubleSafe() * 12;

            var widthInInches = 0d;
            if (width.Contains("inches")) widthInInches = width.Replace(" inches", "").ToDoubleSafe();
            else widthInInches = width.Replace(" ft.", "").ToDoubleSafe() * 12;

            return new RollDimensions(widthInInches, lengthInInches);
        }

        private ProductClass GetProductClass(string productType)
        {
            if (productType == "M") return ProductClass.WallMurals;
            if (productType == "W") return ProductClass.Wallpaper;
            if (productType == "B") return ProductClass.Border;

            if (productType.ContainsIgnoreCase("Mural")) return ProductClass.WallMurals;
            if (productType.ContainsIgnoreCase("Appliqu")) return ProductClass.Applique;
            if (productType.ContainsIgnoreCase("Border")) return ProductClass.Border;
            if (productType.ContainsIgnoreCase("Decal")) return ProductClass.WallDecals;
            if (productType.ContainsIgnoreCase("Adhesive Film")) return ProductClass.AdhesiveFilm;
            return ProductClass.Wallpaper;
        }

        private string BuildName(ScanData data, Vendor vendor)
        {
            var nameParts = new List<string>();
            nameParts.Add(data[ScanField.ManufacturerPartNumber]);

            var productName = data[ScanField.ProductName];
            if (!string.IsNullOrWhiteSpace(productName))
            {
                //productName = productName.Replace("Border", "");
                productName = productName.Replace("Wallpaper", "");
                //productName = productName.Replace("Wall Mural", "");
                nameParts.Add(productName);
            }
            else if (data[ScanField.Theme] != string.Empty)
            {
                nameParts.Add(data[ScanField.Theme]);
            }
            else
            {
                nameParts.Add(data[ScanField.Color]);
                nameParts.Add(data[ScanField.Collection]);
            }

            nameParts.Add("by");
            nameParts.Add(vendor.DisplayName);
            return nameParts.ToArray().BuildName();
        }

        private bool InStock(string stock)
        {
            if (string.IsNullOrWhiteSpace(stock))
                return false;

            var count = stock.ToDoubleSafe();
            return count > 0;
        }

        private string FormatAdditionalInfo(ScanData data)
        {
            var list = new List<string>();

            var strippable = data[ScanField.Strippable];
            if (string.IsNullOrWhiteSpace(strippable))
            {
                list.Add(GetBulletPointMatch(data, _additionalInfo));
            }

            if (!string.IsNullOrWhiteSpace(strippable) && strippable.Contains("Peelable"))  
                list.Add("Peelable");

            if (list.Count == 0)
                return null;

            return list.ToCommaDelimitedList();
        }

        private string FormatCleaning(ScanData data)
        {
            var washable = data[ScanField.Cleaning];
            if (string.IsNullOrWhiteSpace(washable))
            {
                var strippable = data[ScanField.Strippable];
                if (!string.IsNullOrWhiteSpace(strippable) && strippable.ContainsIgnoreCase("Washable"))
                    return "Washable";

                return null;
            }

            if (washable.Contains("Washable")) return "Washable";
            if (washable.Contains("Scrubbable")) return "Scrubbable";

            if (washable.ContainsIgnoreCase("cloth")) return washable;
            return null;
        }

        private string FormatColorGroup(string colorGroup)
        {
            if (colorGroup == "whites-off-whites") return "White/Off-White";

            if (colorGroup.EndsWith("s"))
                colorGroup = colorGroup.Remove(colorGroup.Length - 1);
            colorGroup = colorGroup.Replace("-wallpaper", "");

            return colorGroup.TitleCase();
        }

        private string FormatCoverage(string coverage)
        {
            if (!coverage.ContainsIgnoreCase("feet")) return null;
            if (coverage.ContainsIgnoreCase("wide")) return null;
            if (coverage.ContainsIgnoreCase("high")) return null;

            coverage = coverage.Replace("sq ", "square ");
            return coverage;
        }

        private string FormatDimensions(ScanData data)
        {
            var tempContent = data[ScanField.Bullet1];

            if (string.IsNullOrWhiteSpace(tempContent))
            {
                var coverage = data[ScanField.Coverage];
                if (!string.IsNullOrWhiteSpace(coverage) && coverage.ContainsIgnoreCase(" x "))
                    return coverage;

                return null;
            }

            if (tempContent.Contains("Measures to"))
            {
                return tempContent.Replace("Measures to ", "");
            }
            if (tempContent.Contains("Measures"))
            {
                return tempContent.Replace("Measures ", "");
            }
            if (tempContent.Contains("Assembles to"))
            {
                return tempContent.Replace("Assembles to ", "");
            }
            if (tempContent.Contains("Assembles"))
            {
                return tempContent.Replace("Assembles ", "");
            }
            return null;
        }

        private string FormatMatch(string match)
        {
            if (match.ContainsIgnoreCase("Panels")) return null;
            return match.TitleCase();
        }

        private string FormatMaterial(ScanData data)
        {
            var material = data[ScanField.Material];
            if (string.IsNullOrWhiteSpace(material))
            {
                return GetBulletPointMatch(data, _material);
            }

            return material
                .Replace("Heavyweight", "Heavy Weight")
                .Replace("-", " ").TitleCase();
        }

        private string FormatNumberOfPanels(ScanData data)
        {
            var numPanels = data[ScanField.NumberOfPanels];
            if (string.IsNullOrWhiteSpace(numPanels))
            {
                // sometimes the panel info is in the Match field
                var match = data[ScanField.Match];
                if (!string.IsNullOrWhiteSpace(match) && match.ContainsIgnoreCase("Panels"))
                {
                    return match.Split(new[] {' '}).First();
                }

                // sometimes it's in the bullet points
                var bulletPointMatch = GetBulletPointMatch(data, _numPanels);
                return bulletPointMatch;
            }
            return numPanels.TitleCase();
        }

        private string FormatNumberOfSheets(ScanData data)
        {
            var numSheets = data[ScanField.NumberOfSheets];
            if (string.IsNullOrWhiteSpace(numSheets))
            {
                var bulletPointMatch = GetBulletPointMatch(data, _numSheets);
                return bulletPointMatch;
            }
            return numSheets;
        }

        private string FormatNumberOfPieces(ScanData data)
        {
            var numPieces = data[ScanField.NumberOfPieces];
            if (string.IsNullOrWhiteSpace(numPieces))
            {
                return GetBulletPointMatch(data, _numPieces);
            }
            return numPieces;
        }

        private string FormatPrepasted(ScanData data)
        {
            var value = data[ScanField.Prepasted];
            if (value == null) return null;

            value = value.Replace("Prespasted", "Prepasted");
            value = value.Replace("Un-pasted", "Unpasted");

            if (value.ContainsIgnoreCase("Unpasted"))
                return "No";

            if (value.ContainsIgnoreCase("Paste Included"))
                return "No, Paste Included";

            if (value.ContainsIgnoreCase("Prepasted"))
                return "Yes";

            return null;
        }

        private string FormatProductType(ScanData data)
        {
            var badProductTypes = new[] { "Dots, Blox, Stripes", "G", "GC" };
            var type = data[ScanField.ProductType];
            if (string.IsNullOrWhiteSpace(type))
            {
                return GetBulletPointMatch(data, _productTypes);
            }

            if (type.Contains("Mural")) return "Murals";
            if (type.Contains("Border")) return "Borders";

            if (type == "M") return "Murals";
            if (type == "B") return "Borders";
            if (type == "W") return "Wallpaper";

            if (badProductTypes.Contains(type))
                return null;

            return type;
        }

        private string FormatRepeat(ScanData data)
        {
            var repeat = data[ScanField.Repeat];
            if (repeat.ContainsIgnoreCase("x")) return null;

            if (repeat.ContainsIgnoreCase("Random"))
                return "Random";

            return MakeMeasurement(repeat);
        }

        private string FormatStrippable(string strippable)
        {
            if (!string.IsNullOrWhiteSpace(strippable) && strippable.Contains("Strippable"))
                return "Yes";
            return "No";
        }

        private string FormatStyle(ScanData data)
        {
            var style1 = data[ScanField.Style1];
            if (style1 != string.Empty) style1 = style1.Replace("-", " ");

            var style2 = data[ScanField.Style2];
            if (style2 != string.Empty)
            {
                style2 = style2.Replace("-wallpaper", "");
                style2 = style2.Replace("-", " ");
            }

            return ExtensionMethods.CombineProperty(style1, style2);
        }

        private readonly Dictionary<string, string> _productTypes = new Dictionary<string, string>
        {
            {"Peel & Stick Appliqués", "Appliqués"},
            {"Peel & Stick Appliqué", "Appliqués"},
            {"Peel and stick", "Appliqués"},
            {"Peel and Stick", "Appliqués"},
            {"Peel & Stick Border Decal", "Appliqués"},
            {"Easy to Install - Just Peel and Stick!", "Appliqués"},
            {"Glow in the Dark", "Glow in the Dark"},
            {"Replicates Look & Feel of Authentic Stained Glass", "Stained Glass"},
            {"Dry-Erasable", "Dry-Erase"}
        };

        private readonly Dictionary<string, string> _numPanels = new Dictionary<string, string>
        {
            { "Comes with 8 panels", "8"},
            { "Comes with 4 panels", "4"},
            { "Comes with 2 panels", "2"},
            { "Comes with 1 panel", "1"},
        };

        private readonly List<string> _numPieces = new List<string>
        {
            @"Contains (?<pieces>\d+) total pieces",
            @"There are (?<pieces>\d+) pieces total",
            @"There are (?<pieces>\d+) Total Stickers",
            @"Comes with (?<pieces>\d+) Total Pieces",
            @"Comes with (?<pieces>\d+) total piece",
            @"Comes with (?<pieces>\d+) total pieces",
            @"Comes with (?<pieces>\d+) mirror pieces",
            @"^(?<pieces>\d+) pieces",
            @"^(?<pieces>\d+) Pieces",
            @"^(?<pieces>\d+) Dots",
        };

        private readonly Dictionary<string, string> _additionalInfo = new Dictionary<string, string>
        {
            { "Paste Included", "Paste Included"},
            { "Paste Not Included", "Paste Not Included"},
            { "Removable and Reusable", "Removable and Reusable"},
            { "Reusable and Removable Decals", "Removable and Reusable"},
            { "Repositionable, and Always Removable", "Removable and Repositionable"},
            { "Repositionable and Removable", "Removable and Repositionable"},
            { "Easy to Remove", "Removable"},
            { "Removable", "Removable"},
            { "Peel", "Peel & Stick"},
            { "Safe for Walls", "Safe for Walls"},
            { "Won't Stick to Itself", "Won't Stick to Itself"},
            { "Dry-erasable surface", "Dry-erasable surface"},
            { "Easy to Install - Just Peel and Stick!", "Just Peel and Stick"},
        }; 

        private readonly Dictionary<string, string> _numSheets = new Dictionary<string, string>
        {
            { "Comes on one sheet", "1"}
        }; 

        private readonly Dictionary<string, string> _material = new Dictionary<string, string>
        {
            { "Printed on Vinyl Coated Paper", "Vinyl Coated Paper"},
            { "Printed on Non-Woven", "Non Woven"},
            { "Printed on high quality eco&amp;#45;friendly paper", "Eco Friendly Paper"},
        };

        // takes a list of regex patterns
        private string GetBulletPointMatch(ScanData data, List<string> regexes)
        {
            var bulletPoints = GetBulletPoints(data);
            foreach (var regex in regexes)
            {
                var captures = bulletPoints.Select(x => x.CaptureWithinMatchedPattern(regex)).ToList();
                var found = captures.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (found != null) return found;
            }
            return null;
        }

        private string GetBulletPointMatch(ScanData data, Dictionary<string, string> collection)
        {
            var bulletPoints = GetBulletPoints(data);
            var foundValue = bulletPoints.FirstOrDefault(collection.ContainsKey);
            if (string.IsNullOrWhiteSpace(foundValue)) return null;
            return collection[foundValue];
        }

        private List<string> GetBulletPoints(ScanData data)
        {
            return new List<string>
            {
                data[ScanField.Bullet1],
                data[ScanField.Bullet2],
                data[ScanField.Bullet3],
                data[ScanField.Bullet4],
                data[ScanField.Bullet5],
                data[ScanField.Bullet6],
                data[ScanField.Bullet7],
                data[ScanField.Bullet8],
            }.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }

        private string MakeMeasurement(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !Char.IsDigit(value[0]))
                return null;

            value = value.Replace("'", " ft.");
            value = value.Replace("\"", " inches");

            if (value.ContainsIgnoreCase("ft.") || value.ContainsIgnoreCase("feet"))
                return value;

            if (value.Contains("/"))
                value = ExtensionMethods.MeasurementFromFraction(value);

            return value.ToInchesMeasurement();
        }
    }
}