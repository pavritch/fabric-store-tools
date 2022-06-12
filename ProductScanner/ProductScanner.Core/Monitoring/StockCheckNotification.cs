using System;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.Monitoring
{
    public class StockCheckNotification
    {
        public int VariantId { get; set; }
        public string MPN { get; set; }
        public StockCheckStatus StockCheckStatus { get; set; }
        public DateTime DateTime { get; set; }
        public string Vendor { get; set; }
    }
}