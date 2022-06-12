using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Website
{
    /// <summary>
    /// The JSON object sent to the stock check data service API. 
    /// </summary>
    /// <remarks>
    /// An array of these is sent.
    /// </remarks>
    public class StockCheckQuery
    {
        [JsonProperty(PropertyName = "variantId")]
        public int VariantId { get; set; } // variantID for true product, not swatch variant.

        [JsonProperty(PropertyName = "quantity")]
        public float Quantity { get; set; }

        [JsonProperty(PropertyName = "forceFetch")]
        public bool ForceFetch { get; set; }
    }

}