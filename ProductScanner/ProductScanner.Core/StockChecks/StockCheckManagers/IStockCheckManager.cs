using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.StockChecks.StockCheckManagers
{
    public interface IStockCheckManager<T> where T : Store
    {
        Task<List<StockCheckResult>> CheckStockAsync(List<StockCheck> stockChecks);
    }
}