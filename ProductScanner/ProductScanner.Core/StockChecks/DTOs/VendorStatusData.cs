using System;

namespace ProductScanner.Core.StockChecks.DTOs
{
    public class VendorStatusData
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public StockCapabilities StockCapabilities { get; set; }
        public bool Available { get; set; }
        public DateTime? UnavailableSince { get; set; }
        public int TotalQueries { get; set; }
        public DateTime? LastSuccessfulQuery { get; set; }
    }
}