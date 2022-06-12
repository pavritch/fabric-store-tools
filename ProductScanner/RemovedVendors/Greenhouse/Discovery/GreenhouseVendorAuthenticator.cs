using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Greenhouse.Discovery
{
    public class GreenhouseVendorAuthenticator : IVendorAuthenticator<GreenhouseVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new GreenhouseVendor();
            var values = new NameValueCollection();
            values.Add("name", vendor.Username);
            values.Add("pass", vendor.Password);
            values.Add("form_build_id", "form-HT7X1GPf81kNtU98qfbVKQ8as823tnCNkBrdujY1QB4");
            values.Add("form_id", "user_login");
            values.Add("op", "Log in");

            var webClient = new WebClientExtended();
            webClient.DisableAutoRedirect();
            var login = await webClient.DownloadPageAsync(vendor.LoginUrl, values);
            var success = login.InnerText == string.Empty;
            return new AuthenticationResult(success, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
        }
    }
}