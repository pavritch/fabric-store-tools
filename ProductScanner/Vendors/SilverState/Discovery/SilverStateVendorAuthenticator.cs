using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities;
using Utilities.Extensions;

namespace SilverState.Discovery
{
    public class SilverStateVendorAuthenticator : IVendorAuthenticator<SilverStateVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new SilverStateVendor();
            var userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36";
            var values = new Dictionary<string, object>
            {
                { "nextForward", "login.do" },
                { "usr_name", vendor.Username},
                { "usr_password", vendor.Password}

            };

            var webClient = new WebClientExtended();
            var response = FormUpload.MultipartFormDataPost(vendor.LoginUrl, userAgent, values);
            webClient.Cookies.Add(response.Cookies);

            var page = await webClient.DownloadPageAsync(vendor.LoginUrl2);
            return new AuthenticationResult(page.InnerText.ContainsIgnoreCase("Sign out"),
                webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
        }
    }
}