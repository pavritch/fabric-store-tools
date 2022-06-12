using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductScanner.Core.Caching;
using ProductScanner.Core.Config;
using ProductScanner.Core.StockChecks.Caching;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.StockChecks.StockCheckManagers
{
    public class CachedStockCheckManager<T> : IStockCheckManager<T> where T : Store
    {
        private readonly IAppSettings _appSettings;
        private readonly IMemoryCacheService _cache;
        private readonly IStockCheckManager<T> _stockCheckManager;
        public CachedStockCheckManager(IAppSettings appSettings, IMemoryCacheService cache, 
            IStockCheckManager<T> stockCheckManager)
        {
            _appSettings = appSettings;
            _cache = cache;
            _stockCheckManager = stockCheckManager;
        }

        public async Task<List<StockCheckResult>> CheckStockAsync(List<StockCheck> stockChecks)
        {
            var cachedResults = GetCachedResults(stockChecks);
            var cachedIds = cachedResults.Select(x => x.VariantId);
            var remainingChecks = stockChecks.Where(x => !cachedIds.Contains(x.VariantId)).ToList();

            var results = await _stockCheckManager.CheckStockAsync(remainingChecks);
            var expiryTime = DateTimeOffset.UtcNow.AddSeconds(_appSettings.CacheDecayTimeInSeconds);
            foreach (var result in results)
            {
                var key = string.Format("{0}-{1}", typeof (T).Name, result.VariantId);
                if (result.StockCheckStatus.IsCacheStatus()) _cache.MemoryCache.Set(key, result, expiryTime);
            }
            return cachedResults.Concat(results).ToList();
        }

        private List<StockCheckResult> GetCachedResults(List<StockCheck> stockChecks)
        {
            var cachedResults = new List<StockCheckResult>();
            foreach (var check in stockChecks)
            {
                if (check.ForceFetch) continue;

                var productKey = string.Format("{0}-{1}", typeof (T).Name, check.VariantId);
                var cachedResult = _cache.MemoryCache.Get(productKey) as StockCheckResult;
                if (cachedResult != null)
                {
                    cachedResult.FromCache = true;
                    cachedResults.Add(cachedResult);
                }
            }
            return cachedResults;
        }
    }
}