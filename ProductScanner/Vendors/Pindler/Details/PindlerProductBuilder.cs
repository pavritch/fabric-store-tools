using System;
using System.Linq;
using System.Text;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Pindler.Details
{
    public class PindlerProductBuilder : ProductBuilder<PindlerVendor>
    {
        public PindlerProductBuilder(IPriceCalculator<PindlerVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var cost = data[ScanField.Cost].ToDecimalSafe();
            data.Cost = cost;

            var patternName = data[ScanField.PatternName].TitleCase();
            var colorName = data[ScanField.ColorName].TitleCase();

            var vendor = new PindlerVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, new StockData(GetStock(data[ScanField.StockCount])), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.HasSwatch = data[ScanField.HasSwatch] == "Y";

            // if we make Book/Collection a more strongly typed object, we can easily implement these rules across the board
            vendorProduct.PublicProperties[ProductPropertyType.Book] = data[ScanField.Book].TitleCase()
                .Replace("&ndash;", "-").Replace("&amp;", "&").Replace("&#39;", "'").Replace(" Collection", "").RomanNumeralCase();
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection].TitleCase()
                .Replace("&ndash;", "-").Replace("&#39;", "'").Replace(" Collection", "").Replace(" Coll", "").RomanNumeralCase();
            //vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = data[ScanField.TempContent1].Replace("_", " ").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = tweakText(data[ScanField.Content].TitleCase().Replace(";", ",").ToFormattedFabricContent()).TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability].TitleCase()
                .Replace("&ndash;", "-");
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = tweakText(data[ScanField.Finish].Replace(";", ", ").TitleCase())
                .Replace("&ndash;", "-").Replace("&amp,", "&");
            vendorProduct.PublicProperties[ProductPropertyType.Flammability] = FormatFlammability(data[ScanField.Flammability]);
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Material] = MakeAttributePhrase(data, MaterialsKeywords);
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.Style] = MakeAttributePhrase(data, StylesKeywords);
            vendorProduct.PublicProperties[ProductPropertyType.Type] = MakeAttributePhrase(data, TypesKeywords);
            vendorProduct.PublicProperties[ProductPropertyType.Use] = MakeAttributePhrase(data, UsesKeywords);
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.CleaningCode] = data[ScanField.Cleaning];
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternNumber];

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = string.Format("http://www.pindler.com/cgi-bin/fccgi.exe?w3exec=public&cmd=cust.inv.detail&id={0}&pn={1}-{2}",
                mpn, data[ScanField.PatternNumber], data[ScanField.ColorName]);

            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.Correlator = string.Format("{0}-{1}", data[ScanField.PatternNumber], patternName);
            vendorProduct.Name = new[] { mpn, patternName, colorName, "by", vendor.DisplayName }.BuildName();
            vendorProduct.MinimumQuantity = 2;
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SetProductGroup(data[ScanField.PatternNumber].StartsWith("T") ? ProductGroup.Trim : ProductGroup.Fabric);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure] == "YDS" ? UnitOfMeasure.Yard : UnitOfMeasure.Each;
            return vendorProduct;
        }

        // this type of thing should be global for every field basically
        private string FormatFlammability(string flammability)
        {
            flammability = flammability.Replace("&ndash;", "-");
            flammability = flammability.Replace("&amp;", "&");
            return flammability;
        }

        private int GetStock(string value)
        {
            if (string.Equals(value, "IN STOCK", StringComparison.OrdinalIgnoreCase))
                return 1;

            int count = 0;
            int.TryParse(value, out count);
            return count;
        }

        private string MakeAttributePhrase(ScanData data, string[] knownKeywordsList)
        {
            // given the product keywords 1-6, return a comma-del list in title case
            // of any that are in the known list.

            // all propery keywords and known list already in upper case

            var keywords = new[]
            {
                data[ScanField.Bullet1],
                data[ScanField.Bullet2],
                data[ScanField.Bullet3],
                data[ScanField.Bullet4],
                data[ScanField.Bullet5],
                data[ScanField.Bullet6],
            };

            int count = 0;

            var sb = new StringBuilder(100);
            foreach (var key in keywords.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                if (knownKeywordsList.Contains(key))
                {
                    if (count > 0)
                        sb.Append(", ");

                    sb.Append(key.Replace("BULLION9", "BULLION").Replace("_", " ").TitleCase());
                    count++;
                }
            }

            var phrase = sb.ToString().TrimToNull();
            return phrase;
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

        private static readonly string[] UsesKeywords =
        {
            "DRAPERY",
            "MULTI",
            "UPHOLSTERY",
            "OUTDOOR"
        };

        private static readonly string[] MaterialsKeywords =
        {
            "VELVET",
            "VINYL",
            "EMBROIDERED",
            "CHENILLE",
            "LINEN",
            "SHEER",
            "SILK",
            "SUEDE"
        };

        private static readonly string[] TypesKeywords =
        {
            "GIMP", // not present Oct 2013
            "TRIM",
            "TRIM_OUTDOOR",
            "CASEMENT",
            "CORD",
            "FRINGE_BRUSH",
            "FRINGE_TASSEL",
            "TASSEL_BEAD",
            "TASSEL_KEY", // not present Oct 2013
            "TAPE_BRAID",
            "TIEBACKS", // not present Oct 2013
            "BULLION9" // remove the 9 for display,  not present Oct 2013
        };

        private static readonly string[] StylesKeywords =
        {
            "TAPESTRY",
            "DAMASK",
            "PRINT",
            "SOLID",
            "TRADITIONAL",
            "CONTEMPORARY",
            "CHECK",
            "ETHNIC",
            "MATELASSE",
            "STRIPE",
            "CREWEL",
            "PAISLEY"
        };
    }
}