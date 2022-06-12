using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using InsideFabric.Data;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Kasmir.Details
{
    public class KasmirProductBuilder : ProductBuilder<KasmirVendor>
    {
        public KasmirProductBuilder(IPriceCalculator<KasmirVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var stock = data[ScanField.StockCount] == "In Stock";
            var colorName = data[ScanField.ColorName].Replace("SAGE-BK1315", "Sage").TitleCase();
            var patternName = data[ScanField.PatternName].TitleCase();

            var vendor = new KasmirVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(stock), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.BookNumber] = data[ScanField.BookNumber];
            vendorProduct.PublicProperties[ProductPropertyType.CleaningCode] = data[ScanField.Cleaning];
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Construction] = data[ScanField.Construction];
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content]);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(data[ScanField.Durability]);
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = data[ScanField.Finish];
            vendorProduct.PublicProperties[ProductPropertyType.Flammability] = data[ScanField.Flammability];
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalHalfDrop] = data[ScanField.HorizontalHalfDrop];
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.Use];
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = MakeMeasurementValue(data[ScanField.HorizontalRepeat]);
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = MakeMeasurementValue(data[ScanField.VerticalRepeat]);

            /*
            vendorProduct.PublicProperties[ProductPropertyType.Style] = FormatStyle(data[ScanField.Style]);
            
            vendorProduct.AddPublicProp(ProductPropertyType.ColorNumber, data[ScanField.ColorName]);
            vendorProduct.AddPublicProp(ProductPropertyType.PatternNumber, data[ScanField.PatternName]);

            vendorProduct.RemoveWhen(ProductPropertyType.ColorName, s => s.EndsWithDigit());
            vendorProduct.RemoveWhen(ProductPropertyType.ColorNumber, s => !s.EndsWithDigit());
            vendorProduct.RemoveWhen(ProductPropertyType.PatternName, s => s.ContainsDigit());
            vendorProduct.RemoveWhen(ProductPropertyType.PatternNumber, s => !s.ContainsDigit());*/

            //vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = string.Format(KasmirDetailUrl, patternName.ToUpper(), colorName.ToUpper());

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName.Replace(" ", "-")).ToUpper();
            vendorProduct.Name = new[] {patternName, string.IsNullOrWhiteSpace(colorName) ? data[ScanField.ColorNumber] : colorName, "by", vendor.DisplayName}.BuildName();
            if (!data[ScanField.ImageUrl].ContainsIgnoreCase("ImageComingSoon"))
            {
                vendorProduct.ScannedImages = new List<ScannedImage> { new ScannedImage(ImageVariantType.Primary, 
                    string.Format("ftp://216860:Inss242!@65.98.183.71/Images/Large/{0}", data[ScanField.ImageUrl]))};
            }
            vendorProduct.SetProductGroup(mpn.StartsWithDigit() ? ProductGroup.Trim : ProductGroup.Fabric);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private string FormatDurability(string durability)
        {
            if (durability == "Not Tested") return string.Empty;
            return durability;
        }

        private string MakeMeasurementValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "N/A")
                return null;

            value = value.Replace(" inches", string.Empty);

            if (value.ContainsIgnoreCase("/"))
                return ExtensionMethods.MeasurementFromFraction(value).ToInchesMeasurement();
            return value.ToInchesMeasurement();
        }

        private string FormatStyle(string style)
        {
            style = style.Replace("Strié", "Stripe");
            style = style.Replace("Matelass�", "Matelasse");
            style = style.Replace("Moir�", "Moire");
            style = style.Replace("Moir�", "Moire");

            style = style.Replace("Made In USA, ", "");
            style = style.Replace(", Made In USA", "");

            style = style.Replace("/", ", ");

            return style.TitleCase();
        }

        private string FormatBook(string book)
        {
            if (string.IsNullOrWhiteSpace(book)) return book;

            book = book.Replace("VOL1", "Vol 1");
            book = book.Replace("VOL2", "Vol 2");
            book = book.Replace("VOL3", "Vol 3");
            book = book.ReplaceWholeWord("COLL", "");
            book = book.ReplaceWholeWord("COL", "");
            book = book.ReplaceWholeWord("COLLECTION", "");
            book = book.RemovePattern("COLL$");
            book = book.RemovePattern("COLLECTION$");
            return book.TitleCase().RomanNumeralCase().Substring(2); 
        }

        private string FormatContent(string content)
        {
            var contentOrig = content;
            // VIS48.7 R42.1 P6 N3.2
            // R51.7 VIS43.4 N2.25 P2.4
            // R48 C42 P10
            // R100

            // sometimes it's 12O instead of O12...
            // C55 R20 P13 12O

            // sometimes there are no spaces
            // C45.4 VIS27.9 R15.8P10.9

            try
            {
                if (string.IsNullOrWhiteSpace(content)) return null;

                // from the details page they look like "Cotton 100" or "Nylon 74 Rayon 23 Polyester 3"
                if (!content.Contains("%")) return FormatContentWithoutPercentage(content);

                content = content.Replace("F:", "");
                content = content.Replace("B:", "");
                content = content.Replace("EMB:", "");
                content = content.Replace("Emb:", "");

                var contentPercents = content.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                var contentTypes = new Dictionary<string, int>();
                foreach (var contentValue in contentPercents)
                {
                    var alphaNum = new Regex("(?<Alpha>[a-zA-Z]*)(?<Numeric>[0-9]*)");
                    var match = alphaNum.Match(contentValue);

                    if (match.Groups["Alpha"].Value == string.Empty)
                    {
                        var numAlpha = new Regex("(?<Numeric>[0-9]*)(?<Alpha>[a-zA-Z]*)");
                        match = numAlpha.Match(contentValue);
                    }

                    var type = ExpandContentType(match.Groups["Alpha"].Value);
                    var value = match.Groups["Numeric"].Value;

                    if (!contentTypes.ContainsKey(type))
                    {
                        contentTypes.Add(type, Convert.ToInt32(value));
                    }
                }
                return contentTypes.FormatContentTypes();
            }
            catch (Exception)
            {
                Debug.WriteLine(contentOrig);
                return null;
            }
        }

        private string ExpandContentType(string type)
        {
            foreach (var kvp in contentTypes)
            {
                if (kvp.Key == type) return kvp.Value;
            }
            return "Other";
        }

        private string FormatContentWithoutPercentage(string content)
        {
            try
            {
                content = content.AddContentDelimiters();
                var splitContent = content.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                var contentValues = new Dictionary<string, int>();
                for (int i = 0; i < splitContent.Length; i += 2)
                {
                    var contentType = splitContent[i];
                    if (contentType.ContainsDigit()) continue;
                    contentValues.Add(splitContent[i], Convert.ToInt32(splitContent[i + 1]));
                }
                return contentValues.FormatContentTypes().Replace("-", " ");
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static readonly Dictionary<string, string> contentTypes = new Dictionary<string, string>
        {
            { "A", "Acetate" },
            { "AB", "Acrylic Bead" },
            { "AC", "Acrylic" },
            { "ACFB", "Acrylic Foam Back" },
            { "AP", "Avlin Polyester" },
            { "AR", "Avril Rayon" },
            { "AV", "Avora FR Polyester" },
            { "B", "Bamboo" },
            { "C", "Cotton" },
            { "C/R/F", "Crease Resistant Finish" },
            { "D", "Dralon" },
            { "DP", "Dacron Polyester" },
            { "E", "Enkrome Rayon" },
            { "F", "Flax" },
            { "F/R", "Flame Retardant" },
            { "FE", "Feathers" },
            { "FP", "Fortrel Polyester" },
            { "GB", "Glass Bead" },
            { "GF", "Glass Fiber" },
            { "H", "Hemp" },
            { "J", "Jute" },
            { "L", "Linen" },
            { "LB", "Linen Backing" },
            { "LU", "Lurex" },
            { "MF", "Mixed Fibers" },
            { "MH", "Mohair" },
            { "MOD", "Modacrylic" },
            { "MVIN", "Marine Vinyl" },
            { "N", "Nylon" },
            { "O", "Other" },
            { "OC", "Organic Cotton" },
            { "OL", "Olefin" },
            { "P", "Polyester" },
            { "PA", "PolyAcrylic" },
            { "PB", "Polyester Back" },
            { "PC", "Polyvinyl Chloride" },
            { "PET", "Polyethylene" },
            { "PLY", "Polymide" },
            { "PP", "Polypropylene" },
            { "PU", "Polyurethane" },
            { "R", "Rayon" },
            { "RA", "Ramie" },
            { "RB", "Rubber Back" },
            { "RNS", "Rain No Stain" },
            { "S", "Silk" },
            { "SB", "Suedeback AC80 C20" },
            { "SEF", "Monsanto's Modacrylic Fiber" },
            { "SFM", "Saran Flat Monofilament" },
            { "SLR", "Self Lined" },
            { "SP", "Spandex" },
            { "SR", "Spun Rayon" },
            { "TCS", "Trevira CS" },
            { "TP", "Trevira Polyester" },
            { "UVP", "High UV Polyester" },
            // not sure on this
            { "V", "Vinyl" },
            { "VB", "Vinyl Back" },
            { "VF", "Visa Finish" },
            { "VI", "Vinyon" },
            { "VIN", "Vinyl" },
            { "VIS", "Viscose" },
            { "VM", "Verel Modacrylic" },
            { "W", "Wool" },
            { "W/R", "Water Repellent Finish" },
            { "WL", "Wool" },
        };
    }
}
