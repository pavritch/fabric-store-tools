using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ClarenceHouse.Metadata
{
    public class ClarenceHousePriceCalculator : DefaultPriceCalculator<ClarenceHouseVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var pricing = base.CalculatePrice(data);
            var map = data[ScanField.MAP].ToDecimalSafe();
            if (map != 0)
            {
                var msrp = Math.Round(map * 1.8M, 2);
                pricing = new ProductPriceData(map, msrp);
            }

            // Is there any way we can double the price on all CH items that the cost is $20 or less
            if (data.Cost <= 20)
            {
                pricing.OurPrice *= 2;
                pricing.RetailPrice *= 2;
            }
            return pricing;
        }
    }
}