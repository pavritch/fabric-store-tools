using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website
{

    /// <summary>
    /// The kinds of pages we track for page view statistics.
    /// </summary>
    /// <remarks>
    /// Do not change order - used for array indexes. Correlates with ReportStatistics module.
    /// </remarks>
    public enum PageViewType
    {
        Home = 0,
        Manufacturer,
        Category,
        Product,
        Other,
        Bot
    }
}
