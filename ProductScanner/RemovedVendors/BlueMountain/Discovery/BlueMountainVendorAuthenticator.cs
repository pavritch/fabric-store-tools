using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.WebClient;
using Utilities;
using Utilities.Extensions;

namespace BlueMountain.Discovery
{
    public class BlueMountainVendorAuthenticator : IVendorAuthenticator<BlueMountainVendor>
    {
        public async Task<AuthenticationResult> LoginAsync(VendorData vendorData)
        {
            if (vendorData == null) throw new AuthenticationException("Authentication ScanData not supplied");

            // the blue mountain page returns an invalid certification, so we have to ignore it so the request works
            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            var webClient = new WebClientExtended();
            var loginPage = await webClient.DownloadPageAsync(vendorData.LoginUrl1);

            webClient.Headers["Origin"] = vendorData.LoginUrl1;
            webClient.Headers["Referer"] = vendorData.LoginUrl2;

            var viewstate = loginPage.QuerySelector("#__VIEWSTATE").Attributes["value"].Value;
            var eventValidation = loginPage.QuerySelector("#__EVENTVALIDATION").Attributes["value"].Value;
            var values = new NameValueCollection();
            values.Add("__EVENTTARGET", "btnLogin");
            values.Add("__EVENTARGUMENT", "");
            values.Add("__VIEWSTATE", viewstate);
            values.Add("__EVENTVALIDATION", eventValidation);
            values.Add("txtusername", vendorData.Username);
            values.Add("txtpassword", vendorData.Password);

            var afterLoginPage = await webClient.DownloadPageAsync(vendorData.LoginUrl2, values);
            var success = afterLoginPage.InnerText.ContainsIgnoreCase("logout");
            ServicePointManager.ServerCertificateValidationCallback = null;
            return new AuthenticationResult(success, webClient.Cookies.GetCookies(new Uri(vendorData.LoginUrl1)));
        }
    }
}