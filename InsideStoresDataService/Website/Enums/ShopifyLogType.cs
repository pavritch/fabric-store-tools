using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Matches the values in the Shopify.Log table.
    /// </summary>
    /// <remarks>
    /// Do not change values.
    /// </remarks>
    public enum ShopifyLogType
    {
        /// <summary>
        /// Log entry is for information only. Not an error.
        /// </summary>
        Information = 0,

        /// <summary>
        /// Log entry describes some kind of error
        /// </summary>
        Error = 1
    }
}