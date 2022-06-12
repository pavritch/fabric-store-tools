using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Indicates a level of granularity for a requested timeline.
    /// </summary>
    public enum TimelineSeries
    {
        Seconds,
        Minutes,
        Hours,
        Days,
    }
}