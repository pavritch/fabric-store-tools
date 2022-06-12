using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Kravet.Details
{
    public class KravetBaseFullUpdateChecker<T> : IFullUpdateChecker<T> where T : Vendor, new()
    {
        public bool RequiresFullUpdate(VendorProduct vendorProduct, StoreProduct sqlProduct)
        {
            var vendorLimitedAvail = vendorProduct.GetPrivateProperty(ProductPropertyType.IsLimitedAvailability);
            var sqlLimitedAvail = sqlProduct.GetPrivateProperty(ProductPropertyType.IsLimitedAvailability);

            // no property exists now, so only update if we're actually setting to true
            if (sqlLimitedAvail == "" && vendorLimitedAvail == "True")
                return true;

            // the property currently exists, so update if it has changed
            if (sqlLimitedAvail != "" && sqlLimitedAvail != vendorLimitedAvail)
                return true;
            return false;
        }
    }

    public class KravetFullUpdateChecker : KravetBaseFullUpdateChecker<KravetVendor> { }
    public class LeeJofaFullUpdateChecker : KravetBaseFullUpdateChecker<LeeJofaVendor> { }
    public class BakerLifestyleFullUpdateChecker : KravetBaseFullUpdateChecker<BakerLifestyleVendor> { }
    public class ColeAndSonFullUpdateChecker : KravetBaseFullUpdateChecker<ColeAndSonVendor> { }
    public class GPJBakerFullUpdateChecker : KravetBaseFullUpdateChecker<GPJBakerVendor > { }
    public class GroundworksFullUpdateChecker : KravetBaseFullUpdateChecker<GroundworksVendor> { }
    public class MulberryHomeFullUpdateChecker : KravetBaseFullUpdateChecker<MulberryHomeVendor> { }
    public class ParkertexFullUpdateChecker : KravetBaseFullUpdateChecker<ParkertexVendor> { }
    public class ThreadsFullUpdateChecker : KravetBaseFullUpdateChecker<ThreadsVendor> { }
    public class LauraAshleyFullUpdateChecker : KravetBaseFullUpdateChecker<LauraAshleyVendor> { }
    public class AndrewMartinFullUpdateChecker : KravetBaseFullUpdateChecker<AndrewMartinVendor> { }
    public class BrunschwigAndFilsFullUpdateChecker : KravetBaseFullUpdateChecker<BrunschwigAndFilsVendor> { }
}