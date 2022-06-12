using System;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    public class StockData
    {
        public bool InStock { get; set; }
        public double ActualCount { get; set; }
        public DateTime Recorded { get; set; }

        public StockData(bool inStock, double actualCount = 1)
        {
            InStock = inStock;
            ActualCount = actualCount;
            Recorded = DateTime.UtcNow;
        }

        public StockData(double actualCount)
        {
            InStock = actualCount > 0;
            ActualCount = actualCount;
            Recorded = DateTime.UtcNow;
        }
    }
}