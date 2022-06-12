using System;
using System.Net;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace ProductScanner.Core.Sessions
{
    public interface IVendorSessionManager
    {
        DateTime WentUnavailable { get; }
        StockCapabilities GetCapabilities();
        void RefreshStatus();
    }

    public interface IVendorSessionManager<in T> : IVendorSessionManager where T : Vendor
    {
        VendorSession GetVendorSession();
        void SaveVendorSession(CookieCollection cookies);
        void CheckForTimeout();
        void SetLastRequest();
        void InvalidateSession();

        IWebClientEx CreateWebClient();

        void SetVendorUnavailable();
    }
}