using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace ClarenceHouse.Discovery
{
    public class ClarenceHouseVendorAuthenticator : IVendorAuthenticator<ClarenceHouseVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new ClarenceHouseVendor();
            var nvCol = new NameValueCollection();
            nvCol.Add("ID", vendor.Username);
            nvCol.Add("PASS", vendor.Password);

            var webClient = new WebClientExtended();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var strResponse = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);

            if (strResponse.InnerText.ContainsIgnoreCase("INSIDE STORES") && strResponse.InnerText.ContainsIgnoreCase("Logout"))
            {
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            }
            if (strResponse.InnerText.ContainsIgnoreCase("currently under maintenance") ||
                strResponse.InnerText.ContainsIgnoreCase("database is currently unavailable"))
            {
                return new AuthenticationResult(false);
            }
            return new AuthenticationResult(false);
        }
    }
}