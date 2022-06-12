using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Available sorting for product result sets.
    /// </summary>
    public enum ProductSortOrder
    {
        /// <summary>
        /// Default is the order determined by the system using whatever
        /// logic seems appropriate for the user experience.
        /// </summary>
        Default,
        
        // user-directed orders

        PriceAscend,
        PriceDescend,
    }
}