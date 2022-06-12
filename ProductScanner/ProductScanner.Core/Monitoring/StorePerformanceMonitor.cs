using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace ProductScanner.Core.Monitoring
{
    public class StorePerformanceMonitor<T> : IStorePerformanceMonitor<T> where T : Store
    {
        private readonly List<AuthenticationFailure> _authFailures;
        private readonly IVendorPerformanceMediator _vendorPerformanceMediator;
        public StorePerformanceMonitor(IVendorPerformanceMediator vendorPerformanceMediator)
        {
            _authFailures = new List<AuthenticationFailure>();
            _vendorPerformanceMediator = vendorPerformanceMediator;
        }

        public void BumpCacheHit(Vendor vendor)
        {
            GetMonitor(vendor).BumpCacheHit();
        }

        public List<StockCheckNotification> GetCheckNotificationsInLast(string vendorName)
        {
            if (vendorName == "All") return GetNotificationsForAll();

            var vendor = Vendor.GetByDisplayName(vendorName);
            return GetMonitor(vendor).GetNotifications()
                .OrderByDescending(x => x.DateTime)
                .Take(30)
                .ToList();
        }

        private List<StockCheckNotification> GetNotificationsForAll()
        {
            var vendorsByStore = Vendor.GetByStore<T>();
            return vendorsByStore.SelectMany(x => GetMonitor(x).GetNotifications())
                .OrderByDescending(p => p.DateTime)
                .Take(30)
                .ToList();
        }

        public VendorSessionInfo GetVendorSessionInfo(string vendorName)
        {
            if (vendorName == "All") return new VendorSessionInfo();

            var vendor = Vendor.GetByDisplayName(vendorName);
            return GetMonitor(vendor).GetVendorSessionInfo();
        }

        private IVendorPerformanceMonitor GetMonitor(Vendor vendor)
        {
            return _vendorPerformanceMediator.GetVendorPerformanceMonitor(vendor);
        }

        public ApiRequestData GetApiRequests(string vendorName)
        {
            if (vendorName == "All") return GetAggregatedApiRequests();

            var vendor = Vendor.GetByDisplayName(vendorName);
            var monitor = GetMonitor(vendor);
            return new ApiRequestData(monitor.CacheHits.ToDto(), monitor.ApiRequests.ToDto(), 
                monitor.RequestsByStatus.ToDictionary(x => x.Key, x => x.Value.ToDto()));
        }

        private ApiRequestData GetAggregatedApiRequests()
        {
            var vendorsByStore = Vendor.GetByStore<T>();
            var monitors = vendorsByStore.Select(GetMonitor).ToList();

            var cacheHitTimelines = monitors.Select(x => x.CacheHits.ToDto()).Aggregate(MultiTimelineExtensions.Combine);
            var apiRequests = monitors.Select(x => x.ApiRequests.ToDto()).Aggregate(MultiTimelineExtensions.Combine);
            var byStatus = monitors.Select(x => x.RequestsByStatus.ToDictionary(p => p.Key, p => p.Value.ToDto()))
                .Aggregate((a, b) => a.Keys.ToDictionary(status => status, status => MultiTimelineExtensions.Combine(a[status], b[status])));
            return new ApiRequestData(cacheHitTimelines, apiRequests, byStatus);
        }

        public void RecordAuthenticationFailure(Vendor vendor)
        {
            _authFailures.Add(new AuthenticationFailure(vendor));
        }

        public List<AuthenticationFailureDTO> GetAuthenticationFailures()
        {
            return _authFailures.GroupBy(x => x.Vendor.Id)
                .Select(x => x.OrderByDescending(a => a.DateTime).First()).OrderByDescending(x => x.DateTime)
                .Select(x => new AuthenticationFailureDTO(x.Vendor.DisplayName, x.DateTime)).ToList();
        }
    }
}