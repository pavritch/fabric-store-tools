using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Duralee.Discovery
{
    public class BBergerVendorAuthenticator : DuraleeBaseVendorAuthenticator<BBergerVendor> { }
    public class ClarkeAndClarkeVendorAuthenticator : DuraleeBaseVendorAuthenticator<ClarkeAndClarkeVendor> { }
    public class HighlandCourtVendorAuthenticator : DuraleeBaseVendorAuthenticator<HighlandCourtVendor> { }
    public class DuraleeVendorAuthenticator : DuraleeBaseVendorAuthenticator<DuraleeVendor> { }

    public abstract class DuraleeBaseVendorAuthenticator<T> : IVendorAuthenticator<T> where T : Vendor, new()
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new T();
            var webClient = new WebClientExtended();
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var resp = await webClient.DownloadPageAsync(vendor.LoginUrl);
            var viewstate = resp.QuerySelector("#__VIEWSTATE").GetAttributeValue("value", "FALSE");

            var nvCol = new NameValueCollection();
            nvCol.Add("__VIEWSTATE", viewstate);
            nvCol.Add("__VIEWSTATEGENERATOR", "E8AAE766");
            nvCol.Add("user", vendor.Username);
            nvCol.Add("pass", vendor.Password);
            nvCol.Add("rememberMe", "1");
            nvCol.Add("cmdAction", "login");

            var postPage = (await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol));
            var afterLogin = await webClient.DownloadPageAsync(vendor.LoginUrl2);
            var text = afterLogin.InnerText;

            if (text.IndexOf(vendor.Username) >= 0 && text.IndexOf("Log Out") >= 0)
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}