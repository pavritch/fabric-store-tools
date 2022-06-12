using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Tracks stock status on a cached product.
    /// </summary>
    /// <remarks>
    /// The order of these is important since is used for priority sorting. Lower value is better.
    /// </remarks>
    public enum InventoryStatus
    {
        [Description("In Stock")]
        InStock,

        [Description("Out of Stock")]
        OutOfStock,

        [Description("Unknown")]
        Unknown,

        [Description("Discontinued")]
        Discontinued,
    }
}