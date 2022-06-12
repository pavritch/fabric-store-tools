using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace CyanDesign
{
    public class CyanVendorAuthenticator : IVendorAuthenticator<CyanDesignVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new CyanDesignVendor();
            var webClient = new WebClientExtended();

            webClient.Headers["Accept-Encoding"] = "gzip, deflate";
            webClient.Headers["Referer"] = "http://cyan.design/CustomerSignin.asp";
            webClient.Headers["Origin"] = "http://cyan.design";

            var resp = await webClient.DownloadPageAsync(vendor.LoginUrl);

            var nvCol = new NameValueCollection();
            nvCol.Add("emailLd", vendor.Username);
            nvCol.Add("passLd", vendor.Password);
            nvCol.Add("emailLc", "");
            nvCol.Add("passLc", "");
            nvCol.Add("area2a", "");
            nvCol.Add("area2b", "");
            nvCol.Add("area2c", "");
            nvCol.Add("emailL", vendor.Username);
            nvCol.Add("passL", vendor.Password);

            var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
            if (postPage.InnerText.ContainsIgnoreCase("Inside Avenue"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}