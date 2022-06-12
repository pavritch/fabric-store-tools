using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Website
{
    public class RequestsMetric
    {
        /// <summary>
        /// Longest response time in milliseconds for a page view within the metric period.
        /// </summary>
        public int ResponseTimeHigh { get; set; }

        /// <summary>
        /// Shortest response time in milliseconds for a page view within the metric period.
        /// </summary>
        public int ResponseTimeLow { get; set; }

        /// <summary>
        /// Average response time in milliseconds for a page view within the metric period.
        /// </summary>
        public int ResponseTimeAvg { get; set; }

        /// <summary>
        /// Median response time in milliseconds for a page view within the metric period.
        /// </summary>
        public int ResponseTimeMedian { get; set; }

        /// <summary>
        /// Requests per second, minute, etc.
        /// </summary>
        public double RequestCount { get; set; }

        public RequestsMetric()
        {

        }

        public RequestsMetric(double requestCount)
        {
            this.RequestCount = requestCount;
        }
    }
}