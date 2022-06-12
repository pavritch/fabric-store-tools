using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace RyanStudio
{
    public class RyanStudioVendorAuthenticator : IVendorAuthenticator<RyanStudioVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new RyanStudioVendor();
            var webClient = new WebClientExtended();
            webClient.DisableAutoRedirect();

            var loginPage = await webClient.DownloadPageAsync("http://www.ryanstudio.biz/register.asp?");

            var nvCol = new NameValueCollection();
            nvCol.Add("email", vendor.Username);
            nvCol.Add("password", vendor.Password);
            nvCol.Add("Login", "Login");
            nvCol.Add("CalledBy", "Register.asp");
            nvCol.Add("CustomerNewOld", "old");

            // Still issues with HTTPOnly cookie (I think) - the slt cookie is missing, which I'm assuming is causing the issues
            // it says if I reuse the cookiecontainer (which I'm doing) then it should pass the HttpOnly cookies back on subsequent requests
            // maybe the next thing to do is manually add a working cookie and see if it works

            var newCookie = new Cookie("slt", "68DE5DE9-23FC-4657-AA87-7A103F81EC21", "/", "www.ryanstudio.biz");
            webClient.Cookies.Add(newCookie);
            var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);

            var dashboardPage = await webClient.DownloadPageAsync(vendor.LoginUrl2);
            if (dashboardPage.InnerText.ContainsIgnoreCase("Tessa"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}