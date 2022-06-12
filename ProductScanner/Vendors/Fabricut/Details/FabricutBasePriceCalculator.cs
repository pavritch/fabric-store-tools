using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Fabricut.Details
{
    public class FabricutPriceCalculator : FabricutBasePriceCalculator<FabricutVendor> { }
    public class StroheimPriceCalculator : FabricutBasePriceCalculator<StroheimVendor> { }
    public class SHarrisPriceCalculator : FabricutBasePriceCalculator<SHarrisVendor> { }
    public class TrendPriceCalculator : FabricutBasePriceCalculator<TrendVendor> { }
    public class VervainPriceCalculator : FabricutBasePriceCalculator<VervainVendor> { }

    public class FabricutBasePriceCalculator<T> : IPriceCalculator<T> where T : Vendor
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var cost = CalculatePricePerRoll(data[ScanField.UnitOfMeasure], data[ScanField.Cost].ToDecimalSafe());
            var retail = CalculatePricePerRoll(data[ScanField.UnitOfMeasure], data[ScanField.RetailPrice].ToDecimalSafe());

            var priceData = CalculatePrice(retail);
            if (data.IsClearance)
            {
                priceData = CalculateClearancePrice(cost, retail);
            }
            return priceData;
        }

        private decimal CalculatePricePerRoll(string unit, decimal wholesale)
        {
            if (unit.Equals("dblrl", StringComparison.OrdinalIgnoreCase))
                return Math.Round(wholesale/2, 2);
            if (unit.Equals("tplrl", StringComparison.OrdinalIgnoreCase))
                return Math.Round(wholesale/3, 2);
            return wholesale;
        }

        private ProductPriceData CalculatePrice(decimal retailPrice)
        {
            return new ProductPriceData(Math.Round(retailPrice * .8m, 2), Math.Round(retailPrice * 1.65M, 2));
        }

        private ProductPriceData CalculateClearancePrice(decimal cost, decimal retailPrice)
        {
            var retailMarkup = 1.35M;
            var calculatedPrice = cost <= 20M ? cost * 2M : cost * 1.6M;
            decimal ourPrice = Math.Max(calculatedPrice, 16.98M); // make sure not too low
            if (ourPrice < 20M)
                retailMarkup = 1.5M;

            return new ProductPriceData(Math.Round(ourPrice, 2), Math.Round(retailPrice * retailMarkup, 2));
        }
    }
}