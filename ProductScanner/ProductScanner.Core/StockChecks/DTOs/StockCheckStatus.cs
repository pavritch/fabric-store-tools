namespace ProductScanner.Core.StockChecks.DTOs
{
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

    public static class StockCheckStatusExtensions
    {
        public static bool IsCacheStatus(this StockCheckStatus status)
        {
            return (status == StockCheckStatus.InStock || 
                status == StockCheckStatus.OutOfStock || 
                status == StockCheckStatus.PartialStock);
        }

        public static string GetStockCount(this StockCheckStatus status)
        {
            if (status == StockCheckStatus.InStock ||
                status == StockCheckStatus.PartialStock) return "999999";
            return "0";
        }

        public static bool IsSuccessful(this StockCheckStatus status)
        {
            return (status == StockCheckStatus.Discontinued ||
                    status == StockCheckStatus.InStock ||
                    status == StockCheckStatus.OutOfStock ||
                    status == StockCheckStatus.PartialStock);
        }
    }
}