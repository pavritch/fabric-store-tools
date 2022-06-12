using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Chandra
{
    public class ChandraPriceCalculator : IPriceCalculator<ChandraVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            var msrp = Math.Round(map*1.7M, 2);
            return new ProductPriceData(map, msrp);
        }
    }
}