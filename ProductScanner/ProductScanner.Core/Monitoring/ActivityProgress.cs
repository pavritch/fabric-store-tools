using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.Core
{

    /// <summary>
    /// Standard progress indicator class to be used by a variety of operations.
    /// </summary>
    public class ActivityProgress
    {
        public int CountTotal { get; set; }
        public int CountCompleted { get; set; }
        public double PercentCompleted { get; set; }

        /// <summary>
        /// Optional status message. Leave null when not needed. 
        /// </summary>
        /// <remarks>
        /// Not all operations support getting messages this way. The field is provided for 
        /// when useful to some specific scenario.
        /// </remarks>
        public string Message { get; set; }

        public ActivityProgress()
        {

        }

        public ActivityProgress(int countTotal, int countCompleted, double percentCompleted)
        {
            CountTotal = countTotal;
            CountCompleted = countCompleted;
            PercentCompleted = percentCompleted;
        }
    }
}
