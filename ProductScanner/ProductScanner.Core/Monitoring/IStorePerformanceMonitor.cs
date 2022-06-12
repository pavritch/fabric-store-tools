using System;
using System.Collections.Generic;

namespace ProductScanner.Core.Monitoring
{
    public interface IStorePerformanceMonitor<T> where T : Store
    {
        void BumpCacheHit(Vendor vendor);
        void RecordAuthenticationFailure(Vendor vendor);
        List<StockCheckNotification> GetCheckNotificationsInLast(string vendor);
        ApiRequestData GetApiRequests(string vendor);
        VendorSessionInfo GetVendorSessionInfo(string vendor);
        List<AuthenticationFailureDTO> GetAuthenticationFailures();
    }
}