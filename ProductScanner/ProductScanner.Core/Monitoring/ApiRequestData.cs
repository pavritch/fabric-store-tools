using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using ProductScanner.Core.StockChecks.DTOs;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.Core.Monitoring
{
    // stores information about how many times the API was hit
    public class ApiRequestData
    {
        public MultiTimelineData CacheHits { get; set; }
        public MultiTimelineData VendorRequests { get; set; }
        public Dictionary<StockCheckStatus, MultiTimelineData> RequestsByStatus { get; set; }
        public int TotalRequests 
        {
            get { return RequestsByStatus.Sum(x => x.Value.DaysTimeline.Sum()) + CacheHits.DaysTimeline.Sum(); } 
        }

        public int RequestsLast5Minutes
        {
            get { return RequestsByStatus.Sum(x => x.Value.MinutesTimeline.TakeLast(5).Sum() + CacheHits.MinutesTimeline.TakeLast(5).Sum()); }
        }

        public string CachePercentage
        {
            get { return TotalRequests == 0 ? "0" : (CacheHits.DaysTimeline.Sum() / (float)TotalRequests).AsPercentage(); }
        }

        public ApiRequestData(MultiTimelineData cacheHits, MultiTimelineData apiRequests, Dictionary<StockCheckStatus, MultiTimelineData> requestsByStatus)
        {
            CacheHits = cacheHits;
            VendorRequests = apiRequests;
            RequestsByStatus = requestsByStatus;
        }
    }
}