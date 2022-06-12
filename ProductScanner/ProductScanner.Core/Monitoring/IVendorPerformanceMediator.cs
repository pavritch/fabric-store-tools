namespace ProductScanner.Core.Monitoring
{
    public interface IVendorPerformanceMediator
    {
        IVendorPerformanceMonitor GetVendorPerformanceMonitor(Vendor productType);
    }
}