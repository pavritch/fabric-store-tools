using System;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Kravet.Details
{
    public class KravetPriceCalculator : KravetBasePriceCalculator<KravetVendor> { }
    public class LeeJofaPriceCalculator : KravetBasePriceCalculator<LeeJofaVendor> { }
    public class BakerLifestylePriceCalculator : KravetBasePriceCalculator<BakerLifestyleVendor> { }
    public class ColeAndSonPriceCalculator : KravetBasePriceCalculator<ColeAndSonVendor> { }
    public class GPJBakerPriceCalculator : KravetBasePriceCalculator<GPJBakerVendor> { }
    public class GroundworksPriceCalculator : KravetBasePriceCalculator<GroundworksVendor> { }
    public class MulberryHomePriceCalculator : KravetBasePriceCalculator<MulberryHomeVendor> { }
    public class ParkertexPriceCalculator : KravetBasePriceCalculator<ParkertexVendor> { }
    public class ThreadsPriceCalculator : KravetBasePriceCalculator<ThreadsVendor> { }
    public class LauraAshleyPriceCalculator : KravetBasePriceCalculator<LauraAshleyVendor> { }
    public class AndrewMartinPriceCalculator : KravetBasePriceCalculator<AndrewMartinVendor> { }
    public class BrunschwigAndFilsPriceCalculator : KravetBasePriceCalculator<BrunschwigAndFilsVendor> { }

    public class KravetBasePriceCalculator<T> : IPriceCalculator<T> where T : Vendor, new()
    {
        public ProductPriceData CalculatePrice(ScanData data)
        {
            var cost = data[ScanField.Cost].ToDecimalSafe();
            var priceData = CalculatePrice(cost);
            var isOutlet = IsOutlet(data[ScanField.Status]);
            if (isOutlet)
            {
                priceData = CalculateClearancePrice(cost);
            }
            return priceData;
        }

        private bool IsOutlet(string status)
        {
            if (status == "Outlet") return true;
            return false;
        }

        private ProductPriceData CalculatePrice(decimal cost)
        {
            var ourPrice = cost.ComputeOurPrice(new T().DisplayName == "Cole & Son");
            var msrp = Math.Round(ourPrice * 2.25M, 2);

            return new ProductPriceData(ourPrice - 1, msrp);
        }

        private ProductPriceData CalculateClearancePrice(decimal cost)
        {
            if (cost >= 80M)
            {
                // figure out what might have been the old MSRP
                var adjustedCost3 = cost * 2M;
                var adjustedOurPrice3 = adjustedCost3.ComputeOurPrice();
                var msrp = Math.Round(adjustedOurPrice3 * 2.25M, 2);

                var ourPrice3 = cost * 1.4M;

                return new ProductPriceData(ourPrice3, msrp);
            }

            if (cost >= 29M)
            {
                var adjustedCost = cost * 1.5M;

                var adjustedOurPrice = adjustedCost.ComputeOurPrice();
                var msrp = Math.Round(adjustedOurPrice * 2.25M, 2);

                var ourPrice = adjustedCost;

                return new ProductPriceData(ourPrice, msrp);
            }

            var adjustedCost2 = Math.Max(cost, 5) * 3M;

            var adjustedOurPrice2 = adjustedCost2.ComputeOurPrice();
            var msrp2 = Math.Round(adjustedOurPrice2 * 2.25M, 2);

            var ourPrice2 = adjustedCost2;

            return new ProductPriceData(ourPrice2, msrp2);
        }

    }
}