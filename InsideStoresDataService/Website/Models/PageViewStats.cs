using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Website
{
    public class PageViewStats
    {
        private List<int> responseTimes = null;
        private int responseTimeMedian;

        public int Home { get; set; }
        public int Manufacturer { get; set; }
        public int Category { get; set; }
        public int Product { get; set; }
        public int Other { get; set; }
        public int Bot { get; set; }

        public int ResponseTimeHigh { get; set; }
        public int ResponseTimeLow { get; set; }
        public int ResponseTimeAvg { get; set; }

        public int ResponseTimeMedian
        {
            get
            {
                return responseTimeMedian;
            }
        }

        public List<int> ResponseTimes
        {
            get
            {
                if (!allowMedian || responseTimes == null)
                    return new List<int>();

                return responseTimes;
            }
        }


        private int bumpCount;
        private int accumulatedAvg;
        private readonly bool allowMedian;

        public PageViewStats()
        {
            bumpCount = 0;
            allowMedian = false;
        }

        public PageViewStats(bool allowMedian=false)
        {
            bumpCount = 0;
            this.allowMedian = allowMedian;
            if (allowMedian)
                responseTimes = new List<int>();
        }

        public PageViewStats(IEnumerable<int> responseTimes) : this(true)
        {
            this.responseTimes.AddRange(responseTimes);
        }


        public int Total
        {
            get
            {
                return Home + Manufacturer + Category + Product + Other + Bot;
            }
        }

        /// <summary>
        /// Called just before put into ring buffer.
        /// </summary>
        /// <remarks>
        /// Primarily used to lock down median calc and dispose of the list since no longer needed.
        /// </remarks>
        public void PrepareForArchiving()
        {
            // calc median and avg

            if (responseTimes != null && responseTimes.Count() > 0)
            {
                var m = responseTimes.Select(e => (decimal)e).ToList().Median();
                responseTimeMedian = (int)Math.Round(m, 0);

                // more accurate way to calc average
                var avg = responseTimes.Average();
                ResponseTimeAvg = (int)Math.Round(avg, 0);
            }                

            responseTimes = null;
        }

        public void Bump(PageViewStats addStats)
        {
            Home += addStats.Home;
            Manufacturer += addStats.Manufacturer;
            Category += addStats.Category;
            Product += addStats.Product;
            Other += addStats.Other;
            Bot += addStats.Bot;

            if (Total > 0)
            {
                if (addStats.ResponseTimeHigh > this.ResponseTimeHigh)
                    this.ResponseTimeHigh = addStats.ResponseTimeHigh;

                if (this.ResponseTimeLow == 0)
                {
                    if (addStats.ResponseTimeLow > 0)
                        this.ResponseTimeLow = addStats.ResponseTimeLow;
                }
                else
                {
                    if (addStats.ResponseTimeLow > 0 && addStats.ResponseTimeLow < this.ResponseTimeLow)
                        this.ResponseTimeLow = addStats.ResponseTimeLow;
                }

                // it should actually always have a non-zero value here since already tested for total 

                if (addStats.ResponseTimeAvg > 0)
                {
                    bumpCount++;
                    accumulatedAvg += addStats.ResponseTimeAvg;
                    ResponseTimeAvg = (int)Math.Round((decimal)accumulatedAvg / (decimal)bumpCount);
                }

                if (allowMedian)
                    responseTimes.AddRange(addStats.responseTimes);
            }
        }

    }
}
