using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace AidanGrayHome
{
    public class AidenGrayPriceCalculator : IPriceCalculator<AidanGrayHomeVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            return new ProductPriceData(data.Cost * 2.5m, data.Cost * 2.5m * 1.7m);
        }
    }
}