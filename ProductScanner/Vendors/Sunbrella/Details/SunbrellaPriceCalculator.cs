using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Sunbrella.Details
{
    public class SunbrellaPriceCalculator : DefaultPriceCalculator<SunbrellaVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            if (map == 0)
                return new ProductPriceData(data.Cost * 1.6M, data.Cost * 1.6M * 2.5M);
            var msrp = Math.Round(map*1.6M, 2);
            return new ProductPriceData(map, msrp);
        }
    }
}