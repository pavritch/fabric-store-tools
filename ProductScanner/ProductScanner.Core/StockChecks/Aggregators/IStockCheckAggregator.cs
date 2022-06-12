using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.StockChecks.Aggregators
{
    public interface IStockCheckAggregator<T> : IStockCheckAggregator { }
    public interface IStockCheckAggregator
    {
        Task<List<ProductStockInfo>> CheckStockAsync(List<StockCheck> checks);
        bool CheckPreflight { get; }
    }
}