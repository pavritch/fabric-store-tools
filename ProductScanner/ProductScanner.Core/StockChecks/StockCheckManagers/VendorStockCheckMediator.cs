using ProductScanner.Core.Monitoring;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.StockChecks.Aggregators;
using SimpleInjector;

namespace ProductScanner.Core.StockChecks.StockCheckManagers
{
    public interface IVendorSessionMediator
    {
        IVendorSessionManager GetSessionManager(Vendor vendor);
        IVendorPerformanceMonitor GetPerformanceMonitor(Vendor vendor);
    }

    public sealed class VendorSessionMediator : IVendorSessionMediator
    {
        private readonly Container _container;
        public VendorSessionMediator(Container container)
        { 
            _container = container;
        }

        public IVendorSessionManager GetSessionManager(Vendor vendor)
        {
            var checkerType = typeof(IVendorSessionManager<>).MakeGenericType(vendor.GetType());
            return (IVendorSessionManager)_container.GetInstance(checkerType);
        }

        public IVendorPerformanceMonitor GetPerformanceMonitor(Vendor vendor)
        {
            var monitorType = typeof(IVendorPerformanceMonitor<>).MakeGenericType(vendor.GetType());
            return (IVendorPerformanceMonitor)_container.GetInstance(monitorType);
        }
    }

    public sealed class VendorStockCheckMediator : IVendorStockCheckMediator
    {
        private readonly Container _container;
        public VendorStockCheckMediator(Container container)
        {
            _container = container;
        }

        public IStockCheckAggregator GetStockChecker(Vendor vendor)
        {
            var checkerType = typeof(IStockCheckAggregator<>).MakeGenericType(vendor.GetType());
            return (IStockCheckAggregator)_container.GetInstance(checkerType);
        }
    }
}