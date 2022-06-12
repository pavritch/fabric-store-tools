using System;
using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace JFFabrics.Discovery
{
    public class JFFabricsVendorAuthenticator : IVendorAuthenticator<JFFabricsVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var webClient = new WebClientExtended();
            var vendor = new JFFabricsVendor();
            var host = new Uri(vendor.LoginUrl).GetLeftPart(UriPartial.Authority);
            webClient.Cookies.Add(new Uri(host), new Cookie("RPGSPSESSIONID", "5"));
            var loginPage = await webClient.DownloadPageAsync(string.Format(vendor.LoginUrl, vendor.Username, vendor.Password));
            return new AuthenticationResult(loginPage.InnerText.ContainsIgnoreCase("Place An Order"), 
                webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
        }
    }
}