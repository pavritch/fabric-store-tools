using System;
using System.Collections.Generic;
using ProductScanner.Core.StockChecks.DTOs;
using Utilities;

namespace ProductScanner.Core.Monitoring
{
    public class VendorSessionInfo
    {
        public DateTime LastLogin { get; set; }
    }

    public interface IVendorPerformanceMonitor
    {
        void SaveNotification(int productId, string mpn, StockCheckStatus status);
        void BumpCacheHit();
        void BumpApiRequest();
        List<StockCheckNotification> GetNotifications();
        MultiTimeline ApiRequests { get; }
        MultiTimeline CacheHits { get; }
        DateTime LastSuccessfulQuery { get; }

        Dictionary<StockCheckStatus, MultiTimeline> RequestsByStatus { get; }
        VendorSessionInfo GetVendorSessionInfo();
    }

    public interface IVendorPerformanceMonitor<T> : IVendorPerformanceMonitor where T : Vendor
    {
    }
}