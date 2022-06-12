using System;
using Newtonsoft.Json;

namespace ProductScanner.Core.StockChecks.DTOs
{
    [Serializable]
    public class StockCheckResult
    {
        public string MPN { get; set; }
        public int VariantId { get; set; }
        public StockCapabilities StockCapabilities { get; set; }
        public DateTime? StockCheckTime { get; set; }

        [JsonIgnore]
        public Vendor Vendor { get; set; }
        public string VendorName { get; set; }
        public bool FromCache { get; set; }

        public StockCheckStatus StockCheckStatus { get; set; }
        // date when the vendor has indicated more should arrive… for now, will be null … but we’ll want to support this down the road where possible.
        public DateTime? MoreExpectedOn { get; set; }
        // if happened to be available to us, then tell us… (which means, must be kept in the cache)
        
        public float? QuantityOnHand { get; set; }

        public StockCheckResult(ProductStockInfo stockInfo, Vendor vendor, string mpn, int variantId, StockCapabilities stockCapabilities)
        {
            StockCapabilities = stockCapabilities;
            StockCheckTime = DateTime.Now;
            VariantId = variantId;
            MPN = mpn;
            Vendor = vendor;
            VendorName = vendor.DisplayName;

            StockCheckStatus = stockInfo.StockCheckStatus;
            MoreExpectedOn = stockInfo.MoreExpectedOn;
            QuantityOnHand = stockInfo.QuantityOnHand;
        }

        public StockCheckResult() { }
    }
}