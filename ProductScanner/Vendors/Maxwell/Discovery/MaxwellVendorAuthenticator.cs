using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Maxwell.Discovery
{
    public class MaxwellVendorAuthenticator : IVendorAuthenticator<MaxwellVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var webClient = new WebClientExtended();
            var vendor = new MaxwellVendor();
            var htmlLoginPage = await webClient.DownloadStringTaskAsync(vendor.LoginUrl);

            var nvCol = new NameValueCollection();
            nvCol.Add("getCust", vendor.Username);
            nvCol.Add("custPW", vendor.Password);
            nvCol.Add("op", "Log in to Trade Account");
            nvCol.Add("form_id", "maxwell_user_login_form");

            var docNode = htmlLoginPage.ParseHtmlPage().DocumentNode;
            var formBuildId = docNode.QuerySelector("input[name='form_build_id']");
            nvCol.Add("form_build_id", formBuildId.GetAttributeValue("value", ""));

            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var strResponse = await webClient.PostValuesTaskAsync(vendor.LoginUrl2, nvCol);
            if (strResponse.Contains("Welcome") && strResponse.Contains("INSIDE STORES"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl2)));
            return new AuthenticationResult(false);
        }
    }
}