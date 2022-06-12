using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ProductScanner.Core;
using ProductScanner.Core.Caching;
using ProductScanner.Core.LoadTesting;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.StockChecks.Caching;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.StockChecks.StockCheckManagers;

namespace ProductScanner.Website.Controllers
{
    [RoutePrefix("api/1.0/InsideFabric")]
    public class InsideFabricController : ApiController
    {
        private readonly IStockCheckManager<InsideFabricStore> _fabricStockCheckManager;
        private readonly IMemoryCacheService _cache;
        private readonly IRandomRequestGenerator _requestGenerator;
        private readonly IVendorStatusManager<InsideFabricStore> _statusManager;

        public InsideFabricController(IStockCheckManager<InsideFabricStore> fabricStockCheckManager, IMemoryCacheService cache, IRandomRequestGenerator requestGenerator, IVendorStatusManager<InsideFabricStore> statusManager)
        {
            _fabricStockCheckManager = fabricStockCheckManager;
            _cache = cache;
            _requestGenerator = requestGenerator;
            _statusManager = statusManager;
        }

        [Route("stockcheck")]
        public async Task<List<StockCheckResult>> Post([FromBody]List<StockCheck> stockChecks)
        {
            return await _fabricStockCheckManager.CheckStockAsync(stockChecks);
        }

        [Route("refresh")]
        [HttpGet]
        public async Task RefreshVendors()
        {
            // basically create a list of stock checks with every vendor represented
            // then call CheckStockAsync
            var stockChecks = await _requestGenerator.GenerateForVendors(Vendor.GetByStore(StoreType.InsideFabric), 1);
            await _fabricStockCheckManager.CheckStockAsync(stockChecks);
        }

        [Route("vendors")]
        public List<VendorInfo> GetAllCapabilities()
        {
            return _statusManager.GetAllCapabilities();
        }

        [Route("vendors/{vendorId:vendor}")]
        public VendorInfo GetCapabilitiesByVendor(int vendorId)
        {
            return _statusManager.GetCapabilityByVendor(vendorId);
        }

        [Route("cacheflush/{vendorId:vendor}")]
        public void PostFlushCache(int vendorId)
        {
        }

        [Route("cacheflush")]
        public void PostFlushCache()
        {
            // Retrieving an enumerator for a MemoryCache instance is a resource-intensive and
            // blocking operation. Shouldn't do this unless we absolutely need to.
            _cache.Flush();
        }
    }
}
