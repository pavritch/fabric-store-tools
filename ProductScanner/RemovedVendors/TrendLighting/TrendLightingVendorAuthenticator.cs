using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace TrendLighting
{
    public class TrendLightingVendorAuthenticator : IVendorAuthenticator<TrendLightingVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new TrendLightingVendor();
            var webClient = new WebClientExtended();

            var loginPage = await webClient.DownloadPageAsync("http://www.tlighting.com/customer/account/login/");

            var nvCol = new NameValueCollection();
            nvCol.Add("login[username]", vendor.Username);
            nvCol.Add("login[password]", vendor.Password);
            nvCol.Add("send", "");

            var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
            var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
            if (dashboard.InnerText.ContainsIgnoreCase("Log Out"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}