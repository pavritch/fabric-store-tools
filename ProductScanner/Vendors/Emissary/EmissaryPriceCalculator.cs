using System;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Emissary
{
    public class EmissaryPriceCalculator : IPriceCalculator<EmissaryVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var cost = data.Cost;
            var ourPrice = Math.Round(cost*2.5M, 2);
            if (data[ScanField.ShippingMethod] == "T") ourPrice += 150;

            var msrp = Math.Round(ourPrice*1.7M, 2);
            return new ProductPriceData(ourPrice, msrp);
        }
    }
}