using System.Linq;
using System.Runtime.Caching;
using ProductScanner.Core.StockChecks.Caching;

namespace ProductScanner.Core.Caching
{
    // just using a single cache with prefixes for each store
    // cache service to store results of stock checks in memory
    public class MemoryCacheService : IMemoryCacheService
    {
        public MemoryCacheService()
        {
            MemoryCache = new MemoryCache("stock");
        }

        public MemoryCache MemoryCache
        {
            get;
            set;
        }

        public void Flush()
        {
            var cacheKeys = MemoryCache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                MemoryCache.Remove(cacheKey);
            }
        }
    }
}