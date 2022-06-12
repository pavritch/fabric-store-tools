using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Sphinx
{
    public class SphinxPriceCalculator : IPriceCalculator<SphinxVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            if (map != 0)
                return new ProductPriceData(map, Math.Round(map*1.7M, 2));

            var cost = data[ScanField.Cost].ToDecimalSafe();
            return new ProductPriceData(Math.Round(cost*2.2M, 2), Math.Round(cost*2.2M*1.7M, 2));
        }
    }
}