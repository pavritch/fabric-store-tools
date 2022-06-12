using ProductScanner.Core;

namespace YourOtherWarehouse
{
    public class YourOtherWarehouseVendor : HomewareVendor
    {
        public YourOtherWarehouseVendor() : base(120, "Your Other Warehouse", "YW", 2.2M, 2.2M * 1.7M)
        {
            UsesStaticFiles = true;
        }
    }
}
