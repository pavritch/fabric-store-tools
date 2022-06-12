using System;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Stout.Details
{
    public class StoutPriceCalculator : DefaultPriceCalculator<StoutVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            if (data[ScanField.Status] == "Discontinued")
            {
                return base.CalculatePrice(data);
            }

            var suggestedRetail = data.Cost*2;
            var map = Math.Round(suggestedRetail*.75m, 2);

            return new ProductPriceData(map, suggestedRetail);
        }
    }
}