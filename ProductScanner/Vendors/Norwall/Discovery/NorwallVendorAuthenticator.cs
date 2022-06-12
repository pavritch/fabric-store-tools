using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Norwall.Discovery
{
    public class NorwallVendorAuthenticator : IVendorAuthenticator<NorwallVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new NorwallVendor();
            var values = new NameValueCollection();
            values.Add("txt_username", vendor.Username);
            values.Add("txt_password", vendor.Password);
            values.Add("Submit", "Sign in");

            var webClient = new WebClientExtended();
            var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl, values);
            return new AuthenticationResult(!loginPage.InnerText.Contains("Sign In"), 
                webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
        }
    }
}