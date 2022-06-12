using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ProductScanner.Core;
using ProductScanner.Core.Sessions;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.StockChecks.StockCheckManagers;

namespace ProductScanner.Website.Controllers
{
    [RoutePrefix("api/1.0/InsideWallpaper")]
    public class InsideWallpaperController : ApiController
    {
        private readonly IStockCheckManager<InsideFabricStore> _stockCheckManager; 
        private readonly IVendorStatusManager<InsideFabricStore> _statusManager;

        public InsideWallpaperController(IStockCheckManager<InsideFabricStore> stockCheckManager, IVendorStatusManager<InsideFabricStore> statusManager)
        {
            _stockCheckManager = stockCheckManager;
            _statusManager = statusManager;
        }

        [Route("stockcheck")]
        public async Task<List<StockCheckResult>> Post([FromBody]List<StockCheck> stockChecks)
        {
            return await _stockCheckManager.CheckStockAsync(stockChecks);
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
    }
}