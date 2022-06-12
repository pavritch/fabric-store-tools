using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Momeni
{
    public class MomeniVendorAuthenticator : IVendorAuthenticator<MomeniVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new MomeniVendor();
            var nvCol = new NameValueCollection();
            nvCol.Add("txtUserID", vendor.Username);
            nvCol.Add("txtPassword", vendor.Password);

            var webClient = new WebClientExtended();
            var strResponse = (await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol)).InnerText;

            if (strResponse.Contains("Welcome to Momeni"))
            {
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            }
            return new AuthenticationResult(false);
        }
    }
}