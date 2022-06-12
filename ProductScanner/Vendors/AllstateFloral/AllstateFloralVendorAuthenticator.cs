using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace AllstateFloral
{
    public class AllstateFloralVendorAuthenticator : IVendorAuthenticator<AllstateFloralVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new AllstateFloralVendor();
            var webClient = new WebClientExtended();

            var nvCol = new NameValueCollection();
            nvCol.Add("UserCode", vendor.Username);
            nvCol.Add("password", vendor.Password);
            nvCol.Add("Submit", "Submit");

            var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
            if (postPage.InnerText.ContainsIgnoreCase("Logout"))
            {
                var cookies = webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl));
                var sessionId = cookies["JSESSIONID"];
                // for some reason the JSESSIONID cookie is not handled properly, so we have to add it here manually
                cookies.Add(new Cookie("JSESSIONID", sessionId.Value, "/", "www.allstatefloral.com"));
                return new AuthenticationResult(true, cookies);
            }
            return new AuthenticationResult(false);
        }
    }
}