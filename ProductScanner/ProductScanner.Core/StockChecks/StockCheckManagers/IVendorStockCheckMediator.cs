using ProductScanner.Core.StockChecks.Aggregators;

namespace ProductScanner.Core.StockChecks.StockCheckManagers
{
    public interface IVendorStockCheckMediator
    {
        IStockCheckAggregator GetStockChecker(Vendor productType);
    }
}