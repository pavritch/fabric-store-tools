using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace RalphLauren.Discovery
{
    public class RalphLaurenVendorAuthenticator : IVendorAuthenticator<RalphLaurenVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new RalphLaurenVendor();
            var nvCol = new NameValueCollection();
            nvCol.Add("ID", vendor.Username);
            nvCol.Add("PASS", vendor.Password);

            var webClient = new WebClientExtended();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
            if (loginPage.InnerText.ContainsIgnoreCase("INSIDE STORES") &&
                loginPage.InnerText.ContainsIgnoreCase("Logout"))
            {
                // then just go to this page
                // http://customers.folia-fabrics.com/readitem.asp?acct=01022149

                var host = new Uri(vendor.LoginUrl, UriKind.Absolute).Host;
                var url = string.Format("http://{0}/readitem.asp?acct={1}", host, "01022149");

                var html = await webClient.DownloadPageAsync(url);
                if (html.InnerText.ContainsIgnoreCase("Please enter your customer account number and password, then click Login")
                    || !html.InnerText.ContainsIgnoreCase("By Pattern Name"))
                {
                    return new AuthenticationResult(false);
                }

                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            }
            if (loginPage.InnerText.ContainsIgnoreCase("currently under maintenance") ||
                loginPage.InnerText.ContainsIgnoreCase("database is currently unavailable"))
            {
                return new AuthenticationResult(false);
            }
            return new AuthenticationResult(false);
        }
    }
}