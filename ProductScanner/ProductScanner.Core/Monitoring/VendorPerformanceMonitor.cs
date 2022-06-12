using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.StockChecks.DTOs;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.Core.Monitoring
{
    public class VendorPerformanceMonitor<T> : IVendorPerformanceMonitor<T> where T : Vendor
    {
        public MultiTimeline ApiRequests { get; private set; }
        public MultiTimeline CacheHits { get; private set; }
        public DateTime LastSuccessfulQuery { get; set; }

        public Dictionary<StockCheckStatus, MultiTimeline> RequestsByStatus { get; private set; }
        private readonly ConcurrentQueue<StockCheckNotification> _notifications;

        // just used to pull current vendor performance data out (last login, etc...)
        private readonly IVendorSessionManager<T> _sessionManager; 

        public VendorPerformanceMonitor(IVendorSessionManager<T> sessionManager)
        {
            ApiRequests = new MultiTimeline();
            CacheHits = new MultiTimeline();
            _sessionManager = sessionManager;

            RequestsByStatus = new Dictionary<StockCheckStatus, MultiTimeline>();
            Enum.GetValues(typeof(StockCheckStatus)).Cast<StockCheckStatus>().ForEach(x => RequestsByStatus.Add(x, new MultiTimeline()));
            _notifications = new ConcurrentQueue<StockCheckNotification>();
        }

        public void SaveNotification(int variantId, string mpn, StockCheckStatus status)
        {
            if (status.IsSuccessful()) LastSuccessfulQuery = DateTime.UtcNow;
            RequestsByStatus[status].Bump();
            _notifications.Enqueue(new StockCheckNotification
            {
                StockCheckStatus = status,
                VariantId = variantId,
                MPN = mpn,
                Vendor = Vendor.GetDisplayName<T>(),
                DateTime = DateTime.UtcNow
            });
        }

        public VendorSessionInfo GetVendorSessionInfo()
        {
            return new VendorSessionInfo()
            {
                LastLogin = _sessionManager.GetVendorSession().InitiatedDateTime
            };
        }

        public List<StockCheckNotification> GetNotifications()
        {
            return _notifications.ToList();
        }

        public void BumpCacheHit()
        {
            CacheHits.Bump();
        }

        public void BumpApiRequest()
        {
            ApiRequests.Bump();
        }
    }
}