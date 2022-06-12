using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace CurreyCo
{
    public class CurreyVendorAuthenticator : IVendorAuthenticator<CurreyVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new CurreyVendor();
            var webClient = new WebClientExtended();

            var resp = await webClient.DownloadPageAsync(vendor.LoginUrl);

            var viewstate = resp.QuerySelector("#__VIEWSTATE").GetAttributeValue("value", "FALSE");

            var nvCol = new NameValueCollection();
            nvCol.Add("__EVENTTARGET", "");
            nvCol.Add("__EVENTARGUMENT", "");
            nvCol.Add("__VIEWSTATE", viewstate);
            nvCol.Add("SearchTerm", "Product Search");
            nvCol.Add("ctl00$PageContent$EMail", vendor.Username);
            nvCol.Add("ctl00$PageContent$txtPassword", vendor.Password);
            nvCol.Add("ctl00$PageContent$PersistLogin", "on");
            nvCol.Add("ctl00$PageContent$LoginButton", "Login");

            var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
            var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
            if (dashboard.InnerText.ContainsIgnoreCase("Welcome, INSIDE STORES"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}