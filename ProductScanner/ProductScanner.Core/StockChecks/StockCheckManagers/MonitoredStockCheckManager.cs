using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Monitoring;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.StockChecks.StockCheckManagers
{
    public class MonitoredStockCheckManager<T> : IStockCheckManager<T> where T : Store
    {
        private readonly IStorePerformanceMonitor<T> _monitor;
        private readonly IStockCheckManager<T> _stockCheckManager;

        public MonitoredStockCheckManager(IStorePerformanceMonitor<T> monitor, IStockCheckManager<T> stockCheckManager)
        {
            _monitor = monitor;
            _stockCheckManager = stockCheckManager;
        }

        public async Task<List<StockCheckResult>> CheckStockAsync(List<StockCheck> stockChecks)
        {
            var results = await _stockCheckManager.CheckStockAsync(stockChecks);
            foreach (var result in results)
            {
                if (result.FromCache) _monitor.BumpCacheHit(result.Vendor);
                if (result.StockCheckStatus == StockCheckStatus.AuthenticationFailed) _monitor.RecordAuthenticationFailure(result.Vendor);
            }
            return results;
        }
    }
}