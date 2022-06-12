using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.FileLoading;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Kravet.Details
{
    public class KravetBaseProductBuilder<T> : ProductBuilder<T> where T : Vendor, new()
    {
        private readonly IDesignerFileLoader _designerFileLoader;

        public KravetBaseProductBuilder(IPriceCalculator<T> priceCalculator, IDesignerFileLoader designerFileLoader) : base(priceCalculator)
        {
            _designerFileLoader = designerFileLoader;
        }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var cost = data[ScanField.Cost].ToDecimalSafe();
            data.Cost = cost;

            var stock = data[ScanField.StockCount];
            var patternName = FormatPatternName(data[ScanField.Pattern]);
            var colorName = FormatColorName(data[ScanField.ColorName]);
            var brand = data[ScanField.Brand].TitleCase();

            var vendor = new T();
            if (vendor.DisplayName == "Andrew Martin")
                brand = "Andrew Martin";
            var width = data[ScanField.Width].ToInchesMeasurement();
            var productGroup = GetProductGroup(data[ScanField.ProductUse]);
            var isOutlet = IsOutlet(data[ScanField.Status]);

            var vendorProduct = new FabricProduct(mpn, cost, new StockData(string.Equals(stock, "In Stock", StringComparison.OrdinalIgnoreCase)), vendor);
            // we're setting all 'Outlet' products to not have swatches
            vendorProduct.HasSwatch = !isOutlet;
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var collection = data[ScanField.Collection].TitleCase().Replace(" Collection", "").Replace("Andrew Martin ", "").RomanNumeralCase();

            var itemNumber = data[ScanField.ItemNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Brand] = brand;
            vendorProduct.PublicProperties[ProductPropertyType.CleaningCode] = data[ScanField.Cleaning].ToUpper();
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = collection;
            vendorProduct.PublicProperties[ProductPropertyType.Color] = FormatColor(data);
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = FormatColorNumber(data[ScanField.ManufacturerPartNumber], data[ScanField.ItemNumber]);
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content], data[ScanField.Type1], data[ScanField.Type2]);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Designer] = GetDesigner(collection);
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = tweakText(data[ScanField.Finish].Replace(";", ", ").TitleCase()).Replace("Finiish", "Finish");
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.ItemNumber] = itemNumber.EndsWith(".0") ? itemNumber.Left(itemNumber.Length - 2) : itemNumber;
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = FormatPatternNumber(data[ScanField.ManufacturerPartNumber], data[ScanField.ItemNumber]);
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.ProductUse].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Style] = ExtensionMethods.CombineProperty(data[ScanField.Style1], data[ScanField.Style2]);
            vendorProduct.PublicProperties[ProductPropertyType.Type] = ExtensionMethods.CombineProperty(data[ScanField.Type1], data[ScanField.Type2]);
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = width;

            vendorProduct.PublicProperties[ProductPropertyType.Direction] = data[ScanField.Direction];
            vendorProduct.PublicProperties[ProductPropertyType.Flammability] = FormatFlammability(data[ScanField.Flammability]);
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalHalfDrop] = data[ScanField.HorizontalHalfDrop];

            var increment = data[ScanField.OrderIncrement].ToIntegerSafe();
            var dimensions = GetDimensions(width, data[ScanField.Length], increment);

            vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();
            vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = GetLimitedAvailability(data[ScanField.Status]).ToString();

            vendorProduct.Correlator = FormatCorrelator(mpn, vendor);
            vendorProduct.IsClearance = isOutlet;
            vendorProduct.Name = BuildName(data[ScanField.ItemNumber], mpn, patternName, colorName, brand);
            vendorProduct.SetProductGroup(productGroup);
            vendorProduct.ScannedImages = GetImages(mpn.ToLower(), data[ScanField.ImageUrl], data[ScanField.AlternateImageUrl]);
            vendorProduct.SKU = BuildSKU(mpn, vendor.SkuPrefix);
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();

            vendorProduct.MinimumQuantity = data[ScanField.MinimumQuantity].ToIntegerSafe();
            vendorProduct.OrderIncrement = increment;

            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthInYardsFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(increment);

                vendorProduct.NormalizeWallpaperPricing(increment);
                vendorProduct.MinimumQuantity = 1;
                vendorProduct.OrderIncrement = 1;

                var euroBrands = new List<string> {"Andrew Martin", "Baker Lifestyle", "Cole & Son", "G P & J Baker", "Groundworks", "Kravet", "Lee Jofa", "Mulberry Home", "Threads"};
                if (increment == 1 && euroBrands.Contains(vendor.DisplayName))
                {
                    // Euro roll lines Cole & Son, Clarke & Clarke, Andrew Martin, GP &J Baker, Threads, Mulberry Home 
                    // are ordered by the single Euro roll with the vendor.
                    // These are the ones that I believe should all be two roll minimum due to cost.
                    vendorProduct.MinimumQuantity = 2;
                }
            }
            return vendorProduct;
        }

        private string GetDesigner(string collection)
        {
            var validDesigners = _designerFileLoader.GetDesigners();
            var match = validDesigners.FirstOrDefault(x => collection.ToLower().Contains(x.ToLower()));
            return match ?? string.Empty;
        }

        private string FormatContent(string content, string type1, string type2)
        {
            if (type1.ContainsIgnoreCase("Leather") || type2.ContainsIgnoreCase("Leather")) return "100% Leather";
            content = content.Replace(";", ", ").Replace(" - ", " ").TitleCase();

            if (content == "Leather 100%") return "100% Leather";
            return content;
        }

        private bool GetLimitedAvailability(string status)
        {
            if (status == "Limited Stock") return true;
            return false;
        }

        private bool IsOutlet(string status)
        {
            if (status == "Outlet") return true;
            return false;
        }

        private string FormatLength(string length)
        {
            if (string.IsNullOrWhiteSpace(length)) return string.Empty;
            return length + " yards";
        }

        private RollDimensions GetDimensions(string width, string length, int increment)
        {
            return new RollDimensions((double)width.TakeOnlyFirstDecimalToken(), length.ToDoubleSafe() * 36 * increment);
        }

        private List<ScannedImage> GetImages(string mpnLower, string imageUrl, string altImageUrl)
        {
            const string imageBaseUrl = "http://scanner.insidefabric.com/vendors/Kravet{0}";
            const string ftpImageBaseUrl = "ftp://insideave:inside00@file.kravet.com{0}";
            const string publicImageUrlOne = "http://s3.amazonaws.com/images2.eprevue.net/p4dbimg/400/image1024/{0}.jpg";
            const string publicImageUrlTwo = "http://s3.amazonaws.com/images2.eprevue.net/p4dbimg/788/image1024/{0}.jpg";
            const string publicImageUrlThree = "http://s3.amazonaws.com/images2.eprevue.net/p4dbimg/400/fabric1024/{0}.jpg";

            var imageKey = mpnLower;
            mpnLower = mpnLower.Replace(".", "_");
            if (mpnLower.Contains("_")) imageKey = mpnLower.Substring(0, mpnLower.LastIndexOf("_"));

            var hiResData = imageUrl;
            var lowResData = altImageUrl;

            var ourHiResUrl = string.Format(imageBaseUrl, hiResData);
            var ourLowResUrl = string.Format(imageBaseUrl, lowResData);

            var kravetHiResUrl = string.Format(ftpImageBaseUrl, hiResData);
            var kravetLowResUrl = string.Format(ftpImageBaseUrl, lowResData);

            var images = new List<ScannedImage>();
            if (!string.IsNullOrWhiteSpace(hiResData)) images.Add(new ScannedImage(ImageVariantType.Primary, ourHiResUrl));
            if (!string.IsNullOrWhiteSpace(hiResData)) images.Add(new ScannedImage(ImageVariantType.Primary, kravetHiResUrl));

            images.Add(new ScannedImage(ImageVariantType.Primary, string.Format(publicImageUrlOne, imageKey)));
            images.Add(new ScannedImage(ImageVariantType.Primary, string.Format(publicImageUrlTwo, imageKey)));
            images.Add(new ScannedImage(ImageVariantType.Primary, string.Format(publicImageUrlThree, imageKey)));

            if (!string.IsNullOrWhiteSpace(lowResData)) images.Add(new ScannedImage(ImageVariantType.Primary, ourLowResUrl));
            if (!string.IsNullOrWhiteSpace(lowResData)) images.Add(new ScannedImage(ImageVariantType.Primary, kravetLowResUrl));
            return images;
        }

        private string FormatFlammability(string flammability)
        {
            if (flammability == "N") return "Not Tested";
            return flammability;
        }

        private int GetMinimumQuantity(Vendor vendor, ScanData data)
        {
            var qty = data[ScanField.MinimumQuantity];
            int qtyInt;
            if (Int32.TryParse(qty, out qtyInt))
            {
                return qtyInt;
            }
            return 1;
        }

        private ProductGroup GetProductGroup(string use)
        {
            if (use.ContainsIgnoreCase("Wallcovering")) return ProductGroup.Wallcovering;
            if (use.ContainsIgnoreCase("Trim")) return ProductGroup.Trim;
            if (use.ContainsIgnoreCase("Upholstery")) return ProductGroup.Fabric;
            if (use.ContainsIgnoreCase("Drapery")) return ProductGroup.Fabric;
            if (use.ContainsIgnoreCase("Multipurpose")) return ProductGroup.Fabric;
            return ProductGroup.None;
        }

        private string FormatCorrelator(string mpn, Vendor vendor)
        {
            // 10/7/2013
            // In reworking lots of things for KR/LJ, didn't want to change the logic for
            // computing pattern correlator since that would have trickle down affects,
            // so reproduced the original logic here for adjusted pattern number.

            // eventually, would be best to change this and do a full sweep through all products

            Func<string> legacyAdjPatternNumber = () =>
            {
                var aryParts = mpn.Replace("*", ".").Replace("..", ".").Split('.');

                if (aryParts.Length > 0)
                    return aryParts[0].RemoveSpecificTrailingChars("/-");

                throw new Exception("Unable to determine pattern number.");
            };

            // the website logic does not filter specifically to products from this vendor - so
            // it's generally important to use a vendor-unique prefix to prevent products from
            // other vendors from being mixed in the results.

            // below is what was in the original code.
            return string.Format("{0}-{1}", vendor.SkuPrefix, legacyAdjPatternNumber());
        }

        private string BuildSKU(string currentMPN, string skuPrefix)
        {
            Func<string, string> tweakMPN = (mpn) =>
            {
                string adjustedMPN;
                if (mpn.EndsWith(".0"))
                    adjustedMPN = mpn.Left(mpn.Length - 2);
                else
                    adjustedMPN = mpn;

                adjustedMPN = adjustedMPN.Replace("'", "-").Replace(" ", "-").Replace("/", "-").Replace("*", ".").Replace("..", ".").Replace(",", "").Replace(".", "-").Replace("--", "-").Replace("--", "-").RemoveSpecificTrailingChars("-");

                return adjustedMPN;
            };

            return string.Format("{0}-{1}", skuPrefix, tweakMPN(currentMPN)).ToUpper().Replace("--", "-").RemoveSpecificTrailingChars("-").SkuTweaks();
        }

        private string BuildName(string itemNumber, string manufacturerPartNumber, string pattern, string color, string brand)
        {
            // note that KR has over 1,000 products with no pattern or color - just MPN.
            var nameParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(itemNumber))
            {
                nameParts.Add(itemNumber);
            }
            else
            {
                var mpn = manufacturerPartNumber;
                if (mpn.EndsWith(".0"))
                    mpn = mpn.Left(mpn.Length - 2);
                nameParts.Add(mpn);
            }
            nameParts.Add(pattern);
            nameParts.Add(color);

            nameParts.Add("by");
            nameParts.Add(brand);
            return nameParts.ToArray().BuildName();
        }

        private string FormatColor(ScanData data)
        {
            return ExtensionMethods.CombineProperty(data[ScanField.Color1], data[ScanField.Color2], data[ScanField.Color3]);
        }

        private string FormatColorName(string colorName)
        {
            if (!string.IsNullOrWhiteSpace(colorName) && colorName.Length > 1)
            {
                var value = colorName.TitleCase().RemoveInitialDash().RemoveSpecificTrailingChars("/-");
                // cannot have any digits
                if (!Regex.IsMatch(value, @"\d+"))
                {
                    value = value.Replace("DK", "Dark");
                    value = value.Replace("Dk", "Dark");
                    return value;
                }
            }
            return null;
        }

        private string FormatColorNumber(string mpn, string itemNumber)
        {
            // can return null if we don't find a good number to use

            string value;

            // item number is more reliable when exists

            if (itemNumber != null && itemNumber.Contains("."))
            {
                value = itemNumber.Split('.')[1];
            }
            else
            {
                var aryParts = mpn.Replace("*", ".").Replace("..", ".").Split('.');

                if (aryParts.Length < 2)
                    return null;

                value = aryParts[1];
            }

            // must have only digits

            if (!Regex.IsMatch(value, @"^\d+$"))
                return null;

            return value;
        }

        private string FormatPatternNumber(string mpn, string itemNumber)
        {
            string value;

            // item number is the most reliable source, try for that first
            if (itemNumber != null && itemNumber.Contains("."))
            {
                value = itemNumber.Split('.')[0];
            }
            else
            {
                var aryParts = mpn.Replace("*", ".").Replace("..", ".").Split('.');
                if (aryParts.Length == 0)
                    return null;
                value = aryParts[0].RemoveSpecificTrailingChars("/-");
            }

            // must have at least one digit, else we don't consider to be a pattern number for display

            if (!Regex.IsMatch(value, @"\d+"))
                return null;

            return value;
        }

        private string FormatPatternName(string pattern)
        {
            if (pattern.ContainsIgnoreCase("Use Correct Code")) return string.Empty;

            if (string.IsNullOrWhiteSpace(pattern) || pattern.Length <= 2)
                return null;

            var pat = pattern.TitleCase().Replace("?", "");
            pat = pat.Replace("::", " ");

            // with the exception of ones that start with KF seem to be funny values not to be title cased
            if (pat.StartsWith("KF ", StringComparison.OrdinalIgnoreCase))
                pat = pat.ToUpper();

            if (pat.StartsWith("."))
                pat = pat.Remove(0, 1);

            pat = pat.RemoveSpecificTrailingChars("/-");

            // cannot be all numbers

            if (!string.IsNullOrWhiteSpace(pat) && !Regex.IsMatch(pat, @"^\d+$"))
                return pat.Trim();

            // could not figure out
            return null;
        }

        private string tweakText(string input)
        {
            input = input.Trim();

            if (input.StartsWith(","))
                input = input.Remove(0, 1);

            if (input.StartsWith(";"))
                input = input.Remove(0, 1);

            if (input.EndsWith(";"))
                input = input.Substring(0, input.Length - 1);

            if (input.EndsWith(","))
                input = input.Substring(0, input.Length - 1);

            input = input.Replace(" ,", ",");

            return input;
        }
    }
}