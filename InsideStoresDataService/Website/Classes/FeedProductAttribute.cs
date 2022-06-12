using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FeedProductAttribute : System.Attribute
    {
        public ProductFeedKeys FeedKey { get; set; }
        public string TrackingCode { get; set; }

        public string AnalyticsOrganicTrackingCode { get; set; }
        public string AnalyticsPaidTrackingCode { get; set; }

        //public FeedProductAttribute(ProductFeedKeys FeedKey, string TrackingCode, string AnalyticsOrganicTrackingCode=null, string AnalyticsPaidTrackingCode=null)
        public FeedProductAttribute(ProductFeedKeys FeedKey, string TrackingCode=null)
        {
            this.FeedKey = FeedKey;
            this.TrackingCode = TrackingCode;
            //this.AnalyticsOrganicTrackingCode = AnalyticsOrganicTrackingCode;
            //this.AnalyticsPaidTrackingCode = AnalyticsPaidTrackingCode;

        }

    }
}