using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Website
{
    /// <summary>
    /// This data service result for checking one variant. 
    /// </summary>
    /// <remarks>
    /// This is what gets returned to AspDotNetStorefront.
    /// </remarks>
    public class StockCheckResult
    {
        public int VariantId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StockCheckStatus StockCheckStatus { get; set; }
    }
}