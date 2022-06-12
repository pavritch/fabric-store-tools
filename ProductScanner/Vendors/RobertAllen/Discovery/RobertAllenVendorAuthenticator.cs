using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace RobertAllen.Discovery
{
    public class BeaconHillVendorAuthenticator : RobertAllenBaseVendorAuthenticator<BeaconHillVendor> { }
    public class RobertAllenVendorAuthenticator : RobertAllenBaseVendorAuthenticator<RobertAllenVendor> { }

    public class RobertAllenBaseVendorAuthenticator<T> : IVendorAuthenticator<T> where T : Vendor, new()
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new T();

            var webClient = new WebClientExtended();
            var initial = await webClient.DownloadPageAsync(vendor.LoginUrl);
            var formKey = initial.QuerySelector("input[name='form_key']").Attributes["value"].Value;

            var values = new NameValueCollection();
            values.Add("form_key", formKey);
            values.Add("login[username]", vendor.Username);
            values.Add("login[password]", vendor.Password);
            values.Add("login[referer_url]", "http://www.robertallendesign.com/");
            values.Add("send", "Sign In");

            var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl, values);
            var success = loginPage.InnerText.ContainsIgnoreCase("WELCOME INSIDE STORES");
            return new AuthenticationResult(success, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
        }
    }
}