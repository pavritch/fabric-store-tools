using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace SilverState.Details
{
    public class SilverStatePriceCalculator : DefaultPriceCalculator<SilverStateVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var cost = data.Cost;
            // we're setting all 'Clearance' products to not have swatches
            if (data.IsClearance)
            {
                return CalculateClearancePrice(cost);
            }
            return base.CalculatePrice(data);
        }

        private ProductPriceData CalculateClearancePrice(decimal cost)
        {
            if (cost < 15M)
            {
                var ourPrice = cost * 1.6m;
                var msrp = cost * 5;
                return new ProductPriceData(ourPrice, msrp);

            }
            else
            {
                var ourPrice = cost * 1.4m;
                var msrp = cost * 4.3m;
                return new ProductPriceData(ourPrice, msrp);
            }
        }
    }
}