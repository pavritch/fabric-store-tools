using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace Sunbrella.Details
{
    public class SunbrellaProductBuilder : ProductBuilder<SunbrellaVendor>
    {
        public SunbrellaProductBuilder(IPriceCalculator<SunbrellaVendor> priceCalculator) : base(priceCalculator) { }

        public override VendorProduct Build(ScanData data)
        {
            var mpn = data[ScanField.ManufacturerPartNumber];
            var cost = data[ScanField.Cost].ToDecimalSafe();
            data.Cost = cost;

            var patternName = data[ScanField.PatternName].TitleCase();
            var colorName = data[ScanField.ColorName].TitleCase();

            var vendor = new SunbrellaVendor();
            var sku = string.Format("{0}-{1}", vendor.SkuPrefix, mpn).SkuTweaks();
            var vendorProduct = new FabricProduct(mpn, cost, new StockData(true), vendor);
            vendorProduct.ProductPriceData = PriceCalculator.CalculatePrice(data);

            vendorProduct.PrivateProperties[ProductPropertyType.ProductDetailUrl] = data.GetDetailUrl();

            //vendorProduct.PublicProperties[ProductPropertyType.Color] = FormatColor(data);
            vendorProduct.PublicProperties[ProductPropertyType.ColorName] = colorName;
            vendorProduct.PublicProperties[ProductPropertyType.Content] = data[ScanField.Content].Replace("®", "").TitleCase().Replace(" / ", ", ");
            vendorProduct.PublicProperties[ProductPropertyType.HorizontalRepeat] = data[ScanField.Repeat].CaptureWithinMatchedPattern(@"^(?<capture>(\d*\.{0,1}\d*))" + "\\\"").ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.PatternName] = patternName;
            vendorProduct.PublicProperties[ProductPropertyType.Style] = data[ScanField.Style].TitleCase();
            vendorProduct.PublicProperties[ProductPropertyType.VerticalRepeat] = data[ScanField.Repeat].CaptureWithinMatchedPattern(@"\sx\s(?<capture>(\d*\.{0,1}\d*))" + "\\\"").ToInchesMeasurement();
            vendorProduct.PublicProperties[ProductPropertyType.Width] = data[ScanField.Width].CaptureWithinMatchedPattern(@"^(?<capture>(\d+))" + "\\\"").ToInchesMeasurement();

            vendorProduct.PublicProperties[ProductPropertyType.Collection] = data[ScanField.Collection];
            vendorProduct.PublicProperties[ProductPropertyType.Direction] = data[ScanField.Direction];
            vendorProduct.PublicProperties[ProductPropertyType.Use] = data[ScanField.Use];

            //vendorProduct.AddPublicProp(ProductPropertyType.CountryOfOrigin, "USA");
            //vendorProduct.AddPublicProp(ProductPropertyType.ItemNumber, mpn);

            vendorProduct.Correlator = string.Format("{0}-{1}", vendor.SkuPrefix, patternName);
            vendorProduct.Name = new[] { mpn, patternName, colorName, "by", vendor.DisplayName }.BuildName();
            vendorProduct.ScannedImages = data.GetScannedImages();
            vendorProduct.SetProductGroup(ProductGroup.Fabric);
            vendorProduct.SKU = sku;
            vendorProduct.UnitOfMeasure = UnitOfMeasure.Yard;
            return vendorProduct;
        }
    }
}