using System;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Maxwell.Details
{
    public class MaxwellPriceCalculator : DefaultPriceCalculator<MaxwellVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var cost = Math.Round(data.Cost, 2);
            if (data.IsClearance)
                return CalculateClearancePrice(cost);

            var price = base.CalculatePrice(data);
            //price.OurPrice -= 1;
            return price;
        }

        private ProductPriceData CalculateClearancePrice(decimal cost)
        {
            var ourPrice = cost*2.5m;
            var msrp = cost*6;
            return new ProductPriceData(ourPrice, msrp);
        }
    }
}