using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class SearchMetric
    {
        public int SimpleCount { get; set; }
        public int AdvancedCount { get; set; }
        public int Total { get; set; }

        public SearchMetric()
        {

        }

        public SearchMetric(int simpleCount, int advancedCount)
        {
            SimpleCount = simpleCount;
            AdvancedCount = advancedCount;
            Total = simpleCount + advancedCount;
        }
    }
}