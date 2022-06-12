using System.Runtime.Caching;

namespace ProductScanner.Core.StockChecks.Caching
{
    public interface IMemoryCacheService
    {
        MemoryCache MemoryCache
        {
            get;
            set;
        }

        void Flush();
    }
}