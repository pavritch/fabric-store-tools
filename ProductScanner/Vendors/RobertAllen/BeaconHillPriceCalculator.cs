using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace RobertAllen
{
    public class BeaconHillPriceCalculator : IPriceCalculator<BeaconHillVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var vendor = new BeaconHillVendor();
            return new ProductPriceData(data.Cost*vendor.OurPriceMarkup - 1M, data.Cost*vendor.OurPriceMarkup*1.7M);
        }
    }
}