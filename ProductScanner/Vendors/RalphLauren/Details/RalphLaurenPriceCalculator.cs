using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace RalphLauren.Details
{
    public class RalphLaurenPriceCalculator : IPriceCalculator<RalphLaurenVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var vendor = new RalphLaurenVendor();
            var cost = data.Cost;
            var map = data[ScanField.MAP].ToDecimalSafe();
            if (map != 0) return new ProductPriceData(map, Math.Round(cost * vendor.RetailPriceMarkup, 2));
            return new ProductPriceData(Math.Round(cost * vendor.OurPriceMarkup - 1, 2), Math.Round(cost * vendor.RetailPriceMarkup, 2));
        }
    }
}