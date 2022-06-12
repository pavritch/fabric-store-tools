using System;
using System.Net;

namespace ProductScanner.Core.Sessions
{
    public class VendorSession
    {
        public DateTime InitiatedDateTime { get; set; }
        public DateTime LastRequestDateTime { get; set; }
        public bool IsValid { get; set; }
        public CookieCollection CookieCollection { get; set; }

        public static VendorSession Invalid() { return new VendorSession {IsValid = false}; }
        public static VendorSession New(CookieCollection cookies)
        {
            return new VendorSession
            {
                InitiatedDateTime = DateTime.UtcNow,
                LastRequestDateTime = DateTime.UtcNow,
                CookieCollection = cookies,
                IsValid = true,
            };
        }

        public bool HitMaxSessionTimeout(int maxSessionTimeInSeconds)
        {
            var span = DateTime.UtcNow.Subtract(InitiatedDateTime);
            return span.TotalSeconds >= maxSessionTimeInSeconds;
        }

        public bool HitKeepAliveTimeout(int keepAliveTimeoutInSeconds)
        {
            // see how long it's been since our last request
            var span = DateTime.UtcNow.Subtract(LastRequestDateTime);
            return span.TotalSeconds >= keepAliveTimeoutInSeconds;
        }
    }
}