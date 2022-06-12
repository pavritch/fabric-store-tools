using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.FieldTypes;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Stout.Details
{
    public class StoutProductValidator : DefaultProductValidator<StoutVendor>
    {
        public override ProductValidationResult ValidateProduct(VendorProduct product)
        {
            var validation = base.ValidateProduct(product);
            var collection = product.PublicProperties[ProductPropertyType.Collection];
            if (collection.ContainsIgnoreCase("Pillow") || collection.ContainsIgnoreCase("Hardware"))
            {
                validation.ExcludedReasons.Add(ExcludedReason.UnapprovedProduct);
            }
            return validation;
        }
    }

    public class StoutProductBuilder : ProductBuilder<StoutVendor>
    {
        public StoutProductBuilder(IPriceCalculator<StoutVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var name = data[ScanField.ProductName];

            var splitName = name.Split(new[] {' '}).ToList();
            var pattern = splitName.First();
            var color = splitName.Last();

            var vendor = new StoutVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, data.Cost, new StockData(data[ScanField.StockCount].TakeOnlyFirstIntegerToken() > 0), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            var collection = FormatCollection(data[ScanField.Collection], data[ScanField.Book]);

            vendorProduct.PublicProperties[ProductPropertyType.Book] = data[ScanField.Collection].CaptureWithinMatchedPattern(@"^?(?<capture>(\d{4,4}))-\w+");
            vendorProduct.PublicProperties[ProductPropertyType.Cleaning] = FormatCleaning(data[ScanField.Cleaning]);
            vendorProduct.PublicProperties[ProductPropertyType.CleaningCode] = FormatCleaningCode(data[ScanField.Cleaning]);
            vendorProduct.PublicProperties[ProductPropertyType.Collection] = collection;
            vendorProduct.PublicProperties[ProductPropertyType.ColorGroup] = data[ScanField.ColorGroup].Replace("Lt.", "Light ").Replace("-", ", ").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = color;
            vendorProduct.PublicProperties[ProductPropertyType.Construction] = data[ScanField.Construction].Replace("/", ", ").TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.CountryOfOrigin] = new Country(data[ScanField.Country]).Format();
            vendorProduct.PublicProperties[ProductPropertyType.Durability] = FormatDurability(data[ScanField.Bullet2]);
            vendorProduct.PublicProperties[ProductPropertyType.Finish] = data[ScanField.Finish].TitleCase().Replace("(u.V.) ", "");
            vendorProduct.PublicProperties[ProductPropertyType.FlameRetardant] = FormatFlameRetardant(data[ScanField.Bullet2]);
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.HorizontalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.LeadTime] = FormatLeadTime(data[ScanField.LeadTime]);
            vendorProduct.PublicProperties[ProductPropertyType.Pattern] = pattern;
            vendorProduct.PublicProperties[ProductPropertyType.ProductUse] = data[ScanField.ProductUse].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ScanField.Style].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.Railroaded] = data[ScanField.Railroaded].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.VerticalRepeat].ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content];

            vendorProduct.PrivateProperties[ProductPropertyType.Coordinates] = data[ScanField.Coordinates];
            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.RemoveWhen(ProductPropertyType.ColorName, s => s.ContainsIgnoreCase("Miscellaneous") || s.ContainsIgnoreCase("Misc."));

            vendorProduct.IsDiscontinued = data.IsDiscontinued;
            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, pattern).ToUpper();
            vendorProduct.MinimumQuantity = collection.ContainsIgnoreCase("Marcus William") ? 2 : 1;
            vendorProduct.Name = new[] { mpn, pattern, color, "by", vendor.DisplayName }.BuildName();
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SetProductGroup(GetProductGroup(data[ScanField.ProductUse], data[ScanField.Description]));
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = UnitOfMeasure.Yard;
            return vendorProduct;
        }

        private ProductGroup GetProductGroup(string productUse, string description)
        {
            if (productUse == "Trimming" || (description != null && description.ContainsIgnoreCase(" is a Trimming")))
                return ProductGroup.Trim;
            return ProductGroup.Fabric;
        }

        private string FormatLeadTime(string status)
        {
            var countWeeks = status.CaptureWithinMatchedPattern(@"^(?<capture>(\d+)) week");

            if (countWeeks == null)
                return null;

            int weeks = 0;
            if (!int.TryParse(countWeeks, out weeks) || weeks <= 3)
                return null;

            return string.Format("{0} weeks", weeks);
        }

        private string FormatFlameRetardant(string tempContent2)
        {
            tempContent2 = tempContent2.CaptureMatchedPattern(@"FLAME.*");

            if (tempContent2 == null)
                return null;

            return tempContent2.RemovePattern(@"(\s|\()(WYZENBEEK|MARTINDALE).*").Replace("FLAME RETARDANT-", "").Trim();
        }

        private string FormatDurability(string tempContent2)
        {
            tempContent2 = tempContent2.CaptureMatchedPattern(@"(WYZENBEEK|MARTINDALE).*");

            if (tempContent2 == null)
                return null;

            tempContent2 = tempContent2.RemovePattern(@"\sFLAME.*").RemovePattern(@"\sAATCC.*");
            return tempContent2.TitleCase().Replace(" 000 ", "000 ").Replace("Duty)", "duty)").Trim();
        }

        private string FormatCleaningCode(string cleaningCode)
        {
            if (string.IsNullOrWhiteSpace(cleaningCode))
                return null;

            var code = cleaningCode.CaptureWithinMatchedPattern(@"^?(?<capture>(.+))-\w+");

            if (code == null)
                return null;

            if (code.Length < 6)
                return code;
            return null;
        }

        private string FormatCollection(string collection, string book)
        {
            // there out some non-collection phrases mixed in - this weeds them out

            if (book == null)
                return null;

            if (collection == string.Empty) return string.Empty;

            // remove number and hyphen
            return collection.Substring(book.Length + 1).RomanNumeralCase();
        }

        private string FormatCleaning(string cleaningCode)
        {
            cleaningCode = cleaningCode.Replace("ONLYS", "ONLY"); // happens 6 times
            cleaningCode = cleaningCode.Replace("SOAPW/S", "SOAP"); // 3 times
            return CleaningPhrases.Contains(cleaningCode) ? cleaningCode.TitleCase() : null;
        }
        private static readonly string[] CleaningPhrases = { "DRY CLEAN OR HAND WASH", "HAND WASHABLE", "DRY CLEAN ONLY", "WARM WATER AND MILD SOAP" };
    }
}