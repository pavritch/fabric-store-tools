using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using FabricUpdater.Core;
using FabricUpdater.Core.Authentication;
using FabricUpdater.Core.WebsiteEntities;
using Fizzler.Systems.HtmlAgilityPack;

namespace FabricUpdater.Vendors.Seabrook
{
    public class SeabrookVendorAuthenticator : IVendorAuthenticator<SeabrookVendor>
    {
        public async Task<AuthenticationResult> LoginAsync(VendorCredential credentials)
        {
            var webClient = new WebClientEx();
            var page = await webClient.DownloadPageAsync(credentials.LoginUrl);

            var viewstate = page.QuerySelector("#__VIEWSTATE").Attributes["value"].Value;
            var eventvalidation = page.QuerySelector("#__EVENTVALIDATION").Attributes["value"].Value;

            var values = new NameValueCollection();
            values.Add("CEWFR_EVENTTARGET", "CEWFR_RNDR1_eaILGunORDun1");
            values.Add("__EVENTTARGET", "");
            values.Add("__EVENTARGUMENT", "");
            values.Add("__VIEWSTATE", viewstate);
            values.Add("__EVENTVALIDATION", eventvalidation);
            values.Add("CEWFR_RNDR1$eaILGunID", credentials.Username);
            values.Add("CEWFR_RNDR1$eaILGunPSW", credentials.Password);
            values.Add("CEWFR_RNDR1$eaILGunSTSun1", "Go to Order Entry Screen");
            values.Add("CEWFR_RNDR1$eaILGunPSWunN1", "");
            values.Add("CEWFR_RNDR1$eaILGunPSWunN2", "");
            return new AuthenticationResult(false);

            var _loginUrl = webClient.ResponseUri.AbsoluteUri;
            //var _lastPage = LoadDocument("Stock", "defaultAuth" + Guid.NewGuid(), WebClient.ResponseUri.AbsoluteUri, values);
            return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(credentials.LoginUrl)));
        }
    }
}