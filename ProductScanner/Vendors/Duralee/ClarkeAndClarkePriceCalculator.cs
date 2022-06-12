using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Duralee
{
    public class ClarkeAndClarkePriceCalculator : IPriceCalculator<ClarkeAndClarkeVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var vendor = new ClarkeAndClarkeVendor();
            return new ProductPriceData(data.Cost * vendor.OurPriceMarkup - 1M, data.Cost * vendor.OurPriceMarkup * 2M);
        }
    }
}