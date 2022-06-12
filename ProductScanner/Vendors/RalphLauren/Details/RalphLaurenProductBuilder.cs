using System;
using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace RalphLauren.Details
{
    public class RalphLaurenProductBuilder : ProductBuilder<RalphLaurenVendor>
    {
        public RalphLaurenProductBuilder(IPriceCalculator<RalphLaurenVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var patternName = FormatPatternName(data[ScanField.PatternName].TitleCase());
            var patternNumber = data[ScanField.PatternNumber];
            var colorName = data[ScanField.ColorName].Replace("*", "").Trim().TitleCase().Replace("Lt.", "Light ");

            var vendor = new RalphLaurenVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(data[ScanField.StockCount].TakeOnlyFirstIntegerToken()), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.Book] = data[ScanField.Book].TitleCase().RomanNumeralCase().Replace(" Wp", " WP");
            vendorProduct.PublicProperties[ProductPropertyType.Category] = data[ScanField.Category].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = FormatContent(data[ScanField.Content]);
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternNumber];
            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            var dimensions = GetDimensions(FormatOrderInfo(data[ScanField.Content]), data[ScanField.Width], 2);
            vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
            vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
            vendorProduct.PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthInYardsFormatted();

            vendorProduct.PrivateProperties[ProductPropertyType.Coverage] = dimensions.GetCoverage().ToString();
            vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = data.IsLimitedAvailability.ToString();
            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            var productGroup = data[ScanField.ProductGroup].ToProductGroup();
            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternNumber == string.Empty ? mpn : patternNumber);
            vendorProduct.IsDiscontinued = data.IsDiscontinued;
            vendorProduct.Name = new[] { mpn, patternName, colorName, "by", vendor.DisplayName }.BuildName();
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SetProductGroup(productGroup);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = data[ScanField.UnitOfMeasure].ToUnitOfMeasure();

            vendorProduct.OrderIncrement = 1;
            vendorProduct.MinimumQuantity = 2;

            // everything is shown/sold as double rolls
            if (vendorProduct.UnitOfMeasure == UnitOfMeasure.Roll)
            {
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(2);
                vendorProduct.NormalizeWallpaperPricing(2);

                vendorProduct.OrderIncrement = 1;
                vendorProduct.MinimumQuantity = 1;
            }
            return vendorProduct;
        }

        private RollDimensions GetDimensions(string orderInfo, string width, int increment)
        {
            var widthInInches = width.Replace("\"", "").ToDoubleSafe();
            var length = Convert.ToDouble(orderInfo.TakeOnlyFirstDecimalToken());
            return new RollDimensions(widthInInches, length * 12 * 3 * increment);
        }

        private string FormatPatternName(string patternName)
        {
            foreach (var rep in _patternReplacements) patternName = patternName.ReplaceWholeWord(rep.Key, rep.Value);
            return patternName;
        }

        private string FormatOrderInfo(string content)
        {
            // some important informatino actually appears within the content property,
            // so go look 

            if (string.IsNullOrWhiteSpace(content) || content.Contains("%"))
                return null;

            // these values appeared - need to parse out

            //5.5 YD SINGLE ROLL : 296
            //4.5 SINGLE ROLL : 16
            //4.5 YD SINGLE ROLL : 454
            //BY THE YARD : 50
            //4 YARD SINGLE ROLL : 30
            //5.5 YARD SINGLE ROLL : 49
            //5.5 YD ROLL :
            //5.5 YD SINGLE ROLL HALF DROP MATCH : 45
            //5 YD SINGLE ROLL : 46
            //4/5 YD SINGLE ROLL : 2
            //4.5 YD SINGLE ROLLS : 5


            var parts = content.Split(' ');
            foreach (var part in parts)
                foreach (var word in OrderInfoWords)
                    if (part.Equals(word, StringComparison.OrdinalIgnoreCase))
                    {
                        // we have a match

                        content = content.Replace("YD", "Yard").TitleCase();
                        return content;
                    }
            return null;
        }

        private string FormatContent(string content)
        {
            if (!content.Contains("%"))
            {
                var parts = content.Split(' ');
                foreach (var part in parts)
                    foreach (var stopword in StopWords)
                        if (part.Equals(stopword, StringComparison.OrdinalIgnoreCase))
                            return null;
            }

            return content.ToFormattedFabricContent().TitleCase();
        }

        private static readonly string[] OrderInfoWords = { "SINGLE", "ROLL", "YARD" };
        private static readonly string[] StopWords = { "SINGLE", "ROLL", "HANDPRINT", "ROTARY", "YARD", "YD", "Q", "¦", "`" };

        private readonly Dictionary<string, string> _patternReplacements = new Dictionary<string, string>
        {
            { "Blea", "Bleached" },

            { "Embro", "Embroidery" },
            { "Embroi", "Embroidery" },
            { "Embroid", "Embroidery" },
            { "Embroide", "Embroidery" },
            { "Embroider", "Embroidery" },

            { "Hounds", "Houndstooth" },
            { "Houndstoo", "Houndstooth" },

            { "Wea", "Weave" },
            { "Weav", "Weave" },

            { "Vel", "Velvet" },

            { "Novelt", "Novelty" },

            { "Mtallc", "Metallic" },

            { "Lin", "Linen" },

            { "Fring", "Fringe" },

            { "Dama", "Damask" },
            { "Damas", "Damask" },
            { "Damsk", "Damask" },

            { "Platinu", "Platinum" },

            { "Flora", "Floral" },

            { "Zebr", "Zebra" },

            { "Str", "Stripe" },
            { "Strp", "Stripe" },
            { "Stri", "Stripe" },
            { "Strip", "Stripe" },

            { "Ticki", "Ticking" },
            { "Tickin", "Ticking" },

            { "Textu", "Texture" },

            { "Blan", "Blanket" },

            { "Eyele", "Eyelet" },

            { "Herringb", "Herringbone" },
            { "Herringbo", "Herringbone" },
            { "Herringbn", "Herringbone" },

            { "Patchwo", "Patchwork" },

            { "Blkp", "Blockprint" },
            { "Blkpr", "Blockprint" },
            { "Blckprn", "Blockprint" },

            { "Psly", "Paisley" },
            { "Paisle", "Paisley" },

            { "Unfinis", "Unfinished" },

            { "Unbacke", "Unbacked" },

            { "Sati", "Satin" },

            { "Tarta", "Tartan" },

            { "Mate", "Matelasse" },
            { "Matelass", "Matelasse" },
        }; 
    }
}