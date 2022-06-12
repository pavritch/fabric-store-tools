using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Metric kept for a single full text search operation.
    /// </summary>
    public class SqlSearchMetric
    {
        /// <summary>
        /// Unique identifier for this record.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Indicates it the search was using the advanced search form.
        /// </summary>
        public bool IsAdvancedSearch { get; set; }

        /// <summary>
        /// Phrase searched for.
        /// </summary>
        public string SearchPhrase { get; set; }

        /// <summary>
        /// Time when the search took place.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Time to perform the search operation.
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Number of results returned.
        /// </summary>
        public int ResultCount { get; set; }
    }
}