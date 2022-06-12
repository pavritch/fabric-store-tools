using System.Threading.Tasks;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace ProductScanner.Core.StockChecks.Checkers
{
    public interface IStockChecker
    {
        Task<ProductStockInfo> CheckStockAsync(StockCheck stockCheck, IWebClientEx webClient);
        bool CheckPreflight { get; }
    }

    public interface IStockChecker<T> : IStockChecker { }
}