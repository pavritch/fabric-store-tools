using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Kaleen
{
    public class KaleenPriceCalculator : IPriceCalculator<KaleenVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            if (data.IsDiscontinued)
            {
                var cost = data[ScanField.Cost].ToDecimalSafe();
                return new ProductPriceData(Math.Round(cost * 2.25m, 2), Math.Round(cost * 2.25m * 1.7m, 2));
            }
            var map = data[ScanField.MAP].ToDecimalSafe();
            var msrp = Math.Round(map*1.7M, 2);
            return new ProductPriceData(map, msrp);
        }
    }
}