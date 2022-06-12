using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Rizzy
{
    public class RizzyPriceCalculator : IPriceCalculator<RizzyVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            if (map == 0) return GetDefaultPrice(data[ScanField.Cost].ToDecimalSafe());
            var msrp = Math.Round(map*1.7M, 2);
            return new ProductPriceData(map, msrp);
        }

        private ProductPriceData GetDefaultPrice(decimal cost)
        {
            var vendor = new RizzyVendor();
            var roundedCost = Math.Round(cost, 2);
            return new ProductPriceData(roundedCost * vendor.OurPriceMarkup, roundedCost * vendor.RetailPriceMarkup);
        }
    }
}