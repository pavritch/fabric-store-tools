using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Maxwell.Details
{
    public class MaxwellProductBuilder : ProductBuilder<MaxwellVendor>
    {
        public MaxwellProductBuilder(IPriceCalculator<MaxwellVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var stock = data[ScanField.StockCount];
            var patternName = data[ScanField.PatternName].Replace("&#039;", "'").TitleCase().Replace("(new)", "").Replace("(wp)", "").Trim();
            var colorName = FormatColorName(data[ScanField.ColorName]);

            var vendor = new MaxwellVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(IsInStock(stock)), vendor);

            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = FormatCleaning(data[ScanField.Cleaning]);
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Book].TitleCase().Split('.').First().Replace(" Collection", "").RomanNumeralCase();
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content].Replace("/", ", ").TitleCase().ToFormattedFabricContent().TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country].TitleCase()).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = data[ScanField.Durability].TitleCase().RemovePattern(@"\-.*").TrimToNull();
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.Repeat].CaptureWithinMatchedPattern(@"H-(?<capture>(\d+\.\d+))" + "\"").ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.ItemNumber] = mpn;
            vendorProduct.PublicProperties[ProductPropertyType.Other] = data[ScanField.AdditionalInfo].TitleCase().Replace("/", ",").Replace("-", ",").AddSpacesAfterCommas();
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.PatternNumber] = data[ScanField.PatternNumber].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = data[ScanField.Railroaded].ContainsIgnoreCase("railroaded") ? "Yes" : null;
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.Repeat].CaptureWithinMatchedPattern(@"V-(?<capture>(\d+\.\d+))" + "\"").ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = data[ScanField.ColorGroup];
            vendorProduct.PublicProperties[ProductPropertyType.ColorNumber] = data[ScanField.ColorNumber];
            vendorProduct.PublicProperties[ProductPropertyType.Construction] = data[ScanField.Construction];
            vendorProduct.PublicProperties[ProductPropertyType.Design] = data[ScanField.Design];
            vendorProduct.PublicProperties[ProductPropertyType.FlameRetardant] = data[ScanField.FlameRetardant];
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = FormatProductUse(data[ScanField.ProductUse]);
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ScanField.Style];
            vendorProduct.PublicProperties[ProductPropertyType.Match] = data[ScanField.Match].TitleCase();

            var isWallcovering = data[ScanField.Dimensions] != "";
            if (isWallcovering)
            {
                var dimensions = GetDimensions(data[ScanField.Dimensions]);
                vendorProduct.PublicProperties[ProductPropertyType.Dimensions] = dimensions.Format();
                vendorProduct.PublicProperties[ProductPropertyType.Coverage] = dimensions.GetCoverageFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Length] = dimensions.GetLengthInYardsFormatted();
                vendorProduct.PublicProperties[ProductPropertyType.Packaging] = GetRollType(1);
            }
            else
            {
                // All fabric patterns require 2 yard minimum
                vendorProduct.MinimumQuantity = 2;
            }

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();
            vendorProduct.PrivateProperties[ProductPropertyType.LargestPiece] = GetLargestPiece(stock).ToString();
            vendorProduct.PrivateProperties[ProductPropertyType.IsLimitedAvailability] = stock.ContainsIgnoreCase("Limited Stock").ToString();
            vendorProduct.PrivateProperties[ProductPropertyType.Note] = data[ScanField.Note];

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName);
            vendorProduct.IsSkipped = data.IsSkipped;
            vendorProduct.Name = new[] {mpn, patternName, colorName, "by", vendor.DisplayName}.BuildName();
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SetProductGroup(isWallcovering ? ProductGroup.Wallcovering : ProductGroup.Fabric);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = isWallcovering ? UnitOfMeasure.Roll : UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private RollDimensions GetDimensions(string coverage)
        {
            // all wallcovering is the same
            return new RollDimensions(20.5, 11*3*12);
        }

        private string FormatColorName(string colorNameField)
        {
            var colorName = colorNameField.CaptureWithinMatchedPattern(@"^#\s\d+\s(?<capture>(.*))");
            if (colorName != null) return colorName.TitleCase().Replace("&#039;", "'");
            return string.Empty;
        }

        private string FormatProductUse(string productUse)
        {
            // universe of values, can be in any combination
            //Upholstery
            //Light Upholstery
            //Multi-Purpose
            //Drapery
            //Indoor/Outdoor	
            //Boucle	

            var listUses = new List<string>();

            // find only one of the upholstery values
            if (productUse.ContainsIgnoreCase("Light Upholstery"))
                listUses.Add("Light Upholstery");
            else if (productUse.ContainsIgnoreCase("Upholstery"))
                listUses.Add("Upholstery");

            listUses.AddRange(new[] {"Multi-Purpose", "Drapery", "Indoor/Outdoor", "Boucle"}.Where(productUse.ContainsIgnoreCase));
            return listUses.ToCommaDelimitedList().TrimToNull();
        }

        private double GetLargestPiece(string stockField)
        {
            var piece = stockField.CaptureWithinMatchedPattern(@"Largest piece: (?<capture>(.*))yds");
            return piece.ToDoubleSafe();
        }

        private bool IsInStock(string stockField)
        {
            if (stockField.ContainsIgnoreCase("In stock")) return true;
            if (stockField.ContainsIgnoreCase("Low stock")) return true;
            if (stockField.ToDoubleSafe() > 0) return true;
            return false;
        }

        private string FormatCleaning(string cleaning)
        {
            var value = cleaning.TitleCase();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Replace("-", ",").AddSpacesAfterCommas()
                .TitleCase()
                .Replace("Wash Below 30 Gentle", "Wash Below 30C Gentle")
                .Replace("Wash Below 30 C", "Wash Below 30C")
                .Replace("Wash Below 30,", "Wash Below 30C,")
                .Replace("Wsh BLW30", "Wash Below 30C")
                .Replace("Wash Below 40 Gentle", "Wash Below 40C Gentle")
                .Replace("Wash Below 40 C", "Wash Below 40C")
                .Replace("Wash Below 40,", "Wash Below 40C,")
                .Replace("Wsh BLW40", "Wash Below 40C")
                .Replace("Dryclean", "Dry Clean")
                .Replace("Cleaning Code Ws", "Cleaning Code WS");

            return value;
        }
    }
}