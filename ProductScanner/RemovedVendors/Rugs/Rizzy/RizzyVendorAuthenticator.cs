using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Rizzy
{
    public class RizzyVendorAuthenticator : IVendorAuthenticator<RizzyVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var webClient = new WebClientExtended();
            var vendor = new RizzyVendor();

            var nvCol = new NameValueCollection();
            nvCol.Add("txtUserID", vendor.Username);
            nvCol.Add("txtPassword", vendor.Password);
            nvCol.Add("submit", "Login");

            var strResponse = (await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol)).InnerHtml;
            if (strResponse.ContainsIgnoreCase("Tessa"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}