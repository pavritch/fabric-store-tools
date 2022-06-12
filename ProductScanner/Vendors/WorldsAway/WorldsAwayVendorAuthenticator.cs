using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace WorldsAway
{
    public class WorldsAwayVendorAuthenticator : IVendorAuthenticator<WorldsAwayVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new WorldsAwayVendor();
            var webClient = new WebClientExtended();

            var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl);

            var nvCol = new NameValueCollection();
            nvCol.Add("login_email", vendor.Username);
            nvCol.Add("login_pass", vendor.Password);

            var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl2, nvCol);
            var dashboard = await webClient.DownloadPageAsync("https://www.worlds-away.com/account.php");
            if (dashboard.InnerText.ContainsIgnoreCase("Sign Out"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}