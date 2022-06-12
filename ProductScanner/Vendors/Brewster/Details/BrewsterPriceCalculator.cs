using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Brewster.Details
{
    public class BrewsterPriceCalculator : DefaultPriceCalculator<BrewsterVendor>
    {
        public override ProductPriceData CalculatePrice(ScanData data)
        {
            var vendor = new BrewsterVendor();
            var cost = Math.Round(data.Cost, 2);
            var ourPrice = CalculateOurPrice(cost, data[ScanField.MAP].ToDecimalSafe(), vendor.OurPriceMarkup);

            var retailPrice = CalculateRetailPrice(data[ScanField.RetailPrice], ourPrice);
            if (ourPrice > retailPrice)
            {
                retailPrice = Math.Round(Convert.ToDecimal(retailPrice) * 1.4M, 2);
            }
            return new ProductPriceData(ourPrice, retailPrice);
        }

        private decimal CalculateOurPrice(decimal cost, decimal map, decimal markup)
        {
            var ourPrice = cost*markup;
            if (cost < (20 / markup)) ourPrice = cost*2;
            if (ourPrice > 50) ourPrice -= 1;
            return Math.Max(map, ourPrice);
        }

        private decimal CalculateRetailPrice(string retailPrice, decimal ourPrice)
        {
            if (ourPrice < 20)
                return Math.Round(ourPrice * 2.0M, 2);

            if (retailPrice != string.Empty)
                return Math.Round(Convert.ToDecimal(retailPrice) * 1.3M, 2);

            return Math.Round(ourPrice*2.0M, 2);
        }
    }
}