using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Arteriors
{
    public class ArteriorsPriceCalculator : DefaultPriceCalculator<ArteriorsVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var retail = data[ScanField.RetailPrice].ToDecimalSafe();
            var map = data[ScanField.MAP].ToDecimalSafe();
            if (map == 0)
                return base.CalculatePrice(data);

            // boost their retail for our retail
            return new ProductPriceData(map, retail * 1.5m);
        }
    }
}