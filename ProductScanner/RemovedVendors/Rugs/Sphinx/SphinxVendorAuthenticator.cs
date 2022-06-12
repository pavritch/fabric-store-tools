using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Sphinx
{
    public class SphinxVendorAuthenticator : IVendorAuthenticator<SphinxVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new SphinxVendor();
            var nvCol = new NameValueCollection();
            nvCol.Add("UserID", vendor.Username);
            nvCol.Add("Password", vendor.Password);
            nvCol.Add("Action", "Login");

            var webClient = new WebClientExtended();
            var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
            var formUrl = loginPage.QuerySelector("form[name=login]").Attributes["action"].Value;

            var page = await webClient.DownloadPageAsync(formUrl, nvCol);
            return new AuthenticationResult(page.InnerText.Contains("Enter Orders"), webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)), formUrl);
        }
    }
}