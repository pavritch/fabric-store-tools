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
    [RoutePrefix("api/1.0/InsideRugs")]
    public class InsideRugsController : ApiController
    {
        private readonly IStockCheckManager<InsideRugsStore> _stockCheckManager;
        private readonly IVendorStatusManager<InsideRugsStore> _statusManager;

        public InsideRugsController(IStockCheckManager<InsideRugsStore> stockCheckManager, IVendorStatusManager<InsideRugsStore> statusManager)
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