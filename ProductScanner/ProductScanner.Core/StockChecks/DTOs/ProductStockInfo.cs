using System;

namespace ProductScanner.Core.StockChecks.DTOs
{
    [Serializable]
    public class ProductStockInfo
    {
        public StockCheckStatus StockCheckStatus { get; set; }
        // date when the vendor has indicated more should arrive… for now, will be null … but we’ll want to support this down the road where possible.
        public DateTime? MoreExpectedOn { get; set; }
        // if happened to be available to us, then tell us… (which means, must be kept in the cache)
        public float? QuantityOnHand { get; set; }

        public string ProductDetailUrl { get; set; }

        public ProductStockInfo(StockCheckStatus stockCheckStatus, float? quantityOnHand = 0f, DateTime? moreExpectedOn = null)
        {
            StockCheckStatus = stockCheckStatus;
            QuantityOnHand = quantityOnHand;
            MoreExpectedOn = moreExpectedOn;
        }

        public static ProductStockInfo Invalid()
        {
            return new ProductStockInfo(StockCheckStatus.Unavailable);
        }
    }
}