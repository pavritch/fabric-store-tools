using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Kasmir
{
    public class KasmirPriceCalculator : IPriceCalculator<KasmirVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var vendor = new KasmirVendor();
            return new ProductPriceData(data.Cost*vendor.OurPriceMarkup - 1, data.Cost*vendor.OurPriceMarkup*2.0M);
        }
    }
}