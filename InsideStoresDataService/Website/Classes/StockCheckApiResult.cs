using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Website
{
    /// <summary>
    /// The web service returns an array of these, one per variant in the original query.
    /// </summary>
    public class StockCheckApiResult
    {
        // MoreExpectedOn:  date when the vendor has indicated more should arrive… 
        // for now, will be null … but we’ll want to support this down the road where possible.

        [JsonProperty(PropertyName = "mPN")]
        public string MPN { get; set; }
        [JsonProperty(PropertyName = "variantId")]
        public int VariantId { get; set; }
        [JsonProperty(PropertyName = "stockCapabilities")]
        public StockCapabilities StockCapabilities { get; set; }
        [JsonProperty(PropertyName = "stockCheckTime")]
        public DateTime? StockCheckTime { get; set; }
        [JsonProperty(PropertyName = "vendorName")]
        public string VendorName { get; set; }
        [JsonProperty(PropertyName = "fromCache")]
        public bool FromCache { get; set; }
        [JsonProperty(PropertyName = "stockCheckStatus")]
        public StockCheckStatus StockCheckStatus { get; set; }
        [JsonProperty(PropertyName = "moreExpectedOn")]
        public DateTime? MoreExpectedOn { get; set; }
        [JsonProperty(PropertyName = "quantityOnHand")]
        public float? QuantityOnHand { get; set; }
    }
}