using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Brewster.Discovery
{
    public class BrewsterVendorAuthenticator : IVendorAuthenticator<BrewsterVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var brewsterVendor = new BrewsterVendor();

            var values = new NameValueCollection();
            values.Add("CustomerNumber", brewsterVendor.Username);
            values.Add("Password", brewsterVendor.Password);
            values.Add("Login", "Sign In");

            var webClient = new WebClientExtended();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var strResponse = await webClient.DownloadPageAsync(brewsterVendor.LoginUrl, values);
            return new AuthenticationResult(strResponse.InnerText.Contains("6005540"), 
                webClient.Cookies.GetCookies(new Uri(brewsterVendor.LoginUrl)));
        }
    }
}