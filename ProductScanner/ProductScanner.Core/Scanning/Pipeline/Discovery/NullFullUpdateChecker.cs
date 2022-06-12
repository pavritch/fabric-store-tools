using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Pipeline.Discovery
{
    public class NullFullUpdateChecker<T> : IFullUpdateChecker<T> where T : Vendor
    {
        public bool RequiresFullUpdate(VendorProduct vendorProduct, StoreProduct sqlProduct)
        {
            return false;
        }
    }
}