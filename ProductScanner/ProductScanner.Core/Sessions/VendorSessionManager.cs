using System;
using System.Net;
using ProductScanner.Core.Config;
using ProductScanner.Core.StockChecks.DTOs;
using ProductScanner.Core.WebClient;

namespace ProductScanner.Core.Sessions
{
    // thread safety is enforced higher up
    // used in stock checker
    public class VendorSessionManager<T> : IVendorSessionManager<T> where T : Vendor, new()
    {
        private VendorSession _session;
        private readonly IAppSettings _appSettings;

        // as part of managing our current session for each vendor, we keep track of any information
        // related to the current status of the vendor's website
        private bool _vendorUnavailable = false;
        public DateTime WentUnavailable { get; private set; }

        public VendorSessionManager(IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _session = VendorSession.Invalid();
        }

        public void CheckForTimeout()
        {
            var maxSessionTimeoutInSeconds = _appSettings.MaxSessionTimeInSeconds;
            var keepAliveSessionTimeoutInSeconds = _appSettings.KeepAliveSessionTimeoutInSeconds;

            if (_session.HitKeepAliveTimeout(keepAliveSessionTimeoutInSeconds) ||
                _session.HitMaxSessionTimeout(maxSessionTimeoutInSeconds))
                _session.IsValid = false;
        }

        public VendorSession GetVendorSession() { return _session; }

        public void SaveVendorSession(CookieCollection cookies)
        {
            _session = VendorSession.New(cookies);
        }

        public void SetLastRequest()
        {
            _session.LastRequestDateTime = DateTime.UtcNow;
        }

        public IWebClientEx CreateWebClient()
        {
            var webClient = new WebClientLoggingDecorator(_appSettings, new WebClientExtended());
            if (_session.CookieCollection != null)
                webClient.Cookies.Add(_session.CookieCollection);
            return webClient;
        }

        public void InvalidateSession()
        {
            _session = VendorSession.Invalid();
        }

        public StockCapabilities GetCapabilities()
        {
            if (_vendorUnavailable) return StockCapabilities.Unavailable;
            return new T().StockCapabilities;
        }

        public void RefreshStatus()
        {
            if (DateTime.Now - WentUnavailable > new TimeSpan(0, 15, 0))
            {
                WentUnavailable = DateTime.MinValue;
                _vendorUnavailable = false;
            }
        }

        public void SetVendorUnavailable()
        {
            WentUnavailable = DateTime.Now;
            _vendorUnavailable = true;
        }
    }
}