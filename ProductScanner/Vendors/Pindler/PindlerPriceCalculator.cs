using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Pindler
{
    public class PindlerPriceCalculator : IPriceCalculator<PindlerVendor>
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var vendor = new PindlerVendor();
            var cost = data[ScanField.Cost].ToDecimalSafe();
            if (cost < 20)
                return new ProductPriceData(Math.Round(cost*1.8M, 2), Math.Round(cost*1.8M*1.7M));
            return new ProductPriceData(Math.Round(cost*vendor.OurPriceMarkup, 2), Math.Round(cost*vendor.OurPriceMarkup*1.7M));
        }
    }
}