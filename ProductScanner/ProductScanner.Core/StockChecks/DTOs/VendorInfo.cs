namespace ProductScanner.Core.StockChecks.DTOs
{
    public class VendorInfo
    {
        public StockCapabilities StockCapabilities { get; set; }
        public string DisplayName { get; set; }
        public int VendorId { get; set; }

        public VendorInfo(StockCapabilities stockCapabilities, string displayName, int vendorId)
        {
            StockCapabilities = stockCapabilities;
            DisplayName = displayName;
            VendorId = vendorId;
        }
    }
}