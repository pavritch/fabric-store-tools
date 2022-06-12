using System.Collections.Generic;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.Sessions
{
    public interface IVendorStatusManager<T> where T : Store
    {
        List<VendorInfo> GetAllCapabilities();
        VendorInfo GetCapabilityByVendor(int vendorId);
        List<VendorStatusData> GetAllStatuses();
    }
}