using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ElkGroup
{
    public class ElkGroupPriceCalculator : IPriceCalculator<ElkGroupVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var map = data[ScanField.MAP].ToDecimalSafe();
            var ourRetail = map*2;
            return new ProductPriceData(map, ourRetail);
        }
    }
}