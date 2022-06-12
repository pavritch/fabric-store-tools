using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Website
{
    /// <summary>
    /// Disposition of a given variant returned by the web service stock check API.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StockCheckStatus
    {
        AuthenticationFailed,
        Discontinued,
        InStock,
        InvalidProduct,
        NotSupported,
        OutOfStock,
        PartialStock,
        Unavailable,
    }
}