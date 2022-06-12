using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace JaipurHomeware
{
    public class JaipurHomewareVendorAuthenticator : IVendorAuthenticator<JaipurHomewareVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var webClient = new WebClientExtended();
            var vendor = new JaipurHomewareVendor();
            var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl);

            var values = new NameValueCollection();
            values.Add("ReturnURL", "");
            values.Add("UserName", vendor.Username);
            values.Add("Password", vendor.Password);

            var login = await webClient.DownloadPageAsync(vendor.LoginUrl, values);
            var rugs = await webClient.DownloadPageAsync("https://www.jaipurliving.com/pillows");

            var success = rugs.InnerText.Contains("1003761");
            return new AuthenticationResult(success, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
        }
    }
}