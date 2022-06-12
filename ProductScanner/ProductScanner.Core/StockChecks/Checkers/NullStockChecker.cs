using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace ProductScanner.Core.StockChecks.Checkers
{
    public class NullStockChecker<T> : IStockChecker<T>
    {
        public Task<ProductStockInfo> CheckStockAsync(StockCheck stockChecks, IWebClientEx webClient)
        {
            return Task.FromResult(new ProductStockInfo(StockCheckStatus.NotSupported));
        }
        public StockCapabilities StockCapabilities { get { return StockCapabilities.None; } }
        public bool CheckPreflight { get { return false; } }
    }
}