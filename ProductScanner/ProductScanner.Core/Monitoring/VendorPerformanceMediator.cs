using SimpleInjector;

namespace ProductScanner.Core.Monitoring
{
    // used to retrieve a performance monitor for the given vendor
    public sealed class VendorPerformanceMediator : IVendorPerformanceMediator
    {
        private readonly Container _container;
        public VendorPerformanceMediator(Container container)
        { 
            _container = container;
        }

        public IVendorPerformanceMonitor GetVendorPerformanceMonitor(Vendor vendor)
        {
            var monitorType = typeof (IVendorPerformanceMonitor<>).MakeGenericType(vendor.GetType());
            return (IVendorPerformanceMonitor)_container.GetInstance(monitorType);
        }
    }
}