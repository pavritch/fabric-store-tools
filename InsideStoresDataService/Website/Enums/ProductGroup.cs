using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// List of all product groups from p.ProductGroup SQL column.
    /// </summary>
    /// <remarks>
    /// Description value is the phrase used in SQL. 
    /// Product groups are required in SQL for all stores.
    /// </remarks>
    public enum ProductGroup
    {
        [Description("Fabric")]
        Fabric,

        [Description("Trim")]
        Trim,

        [Description("Wallcovering")]
        Wallcovering,

        [Description("Rug")]
        Rug,

        [Description("Misc")]
        Misc,

        [Description("Homeware")]
        Homeware

    }
}