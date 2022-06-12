using System;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Greenhouse.Details
{
    public class GreenhousePriceCalculator : DefaultPriceCalculator<GreenhouseVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var cost = Math.Round(data.Cost, 2);
            var isClearance = data[ScanField.IsClearance] == "true";
            if (isClearance)
            {
                return CalculateClearancePrice(cost);
            }

            var defaultPrice = base.CalculatePrice(data);
            var ourPrice = Math.Max(defaultPrice.OurPrice, 11.99M);
            return new ProductPriceData(ourPrice, Math.Round(ourPrice * 1.85M, 2));
        }

        private ProductPriceData CalculateClearancePrice(decimal cost)
        {
            var ourPrice = cost*1.8m;
            var msrp = cost*6;
            return new ProductPriceData(ourPrice, msrp);
        }
    }
}