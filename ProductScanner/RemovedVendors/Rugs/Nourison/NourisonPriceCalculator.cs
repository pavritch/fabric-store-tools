using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Nourison
{
    public class NourisonPriceCalculator : IPriceCalculator<NourisonVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            if (map == 0)
            {
                var cost = data[ScanField.Cost].ToDecimalSafe();
                return new ProductPriceData(Math.Round(cost*1.6M, 2), Math.Round(cost*1.6M*1.7M, 2));
            }
            var msrp = Math.Round(map*1.7M, 2);
            return new ProductPriceData(map, msrp);
        }
    }
}