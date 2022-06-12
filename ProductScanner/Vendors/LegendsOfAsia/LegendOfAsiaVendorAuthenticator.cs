using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace LegendOfAsia
{
    public class LegendOfAsiaVendorAuthenticator : IVendorAuthenticator<LegendOfAsiaVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new LegendOfAsiaVendor();
            var webClient = new WebClientExtended();

            var nvCol = new NameValueCollection();
            nvCol.Add("login_email", vendor.Username);
            nvCol.Add("login_pass", vendor.Password);

            var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
            var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
            if (dashboard.InnerText.ContainsIgnoreCase("Log Out"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}