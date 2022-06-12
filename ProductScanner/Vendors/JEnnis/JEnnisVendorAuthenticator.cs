using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace JEnnis
{
    /*
    public class JEnnisVendorAuthenticator : IVendorAuthenticator<JEnnisVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var webClient = new WebClientExtended();
            var vendor = new JEnnisVendor();

            await webClient.DownloadPageAsync("https://www.jennisfabrics.com/jennis-web-core/Home.jef");

            var values = new NameValueCollection();
            values.Add("completeurl", "https://www.jennisfabrics.com/jennis-web-core/Home.jef");
            values.Add("url", "Home.jef");
            var firstPage = await webClient.DownloadPageAsync(vendor.LoginUrl, values);

            var nvCol = new NameValueCollection();
            nvCol.Add("txt_mail", vendor.Username);
            nvCol.Add("txt_pwd", vendor.Password);

            var strResponse = await webClient.PostValuesTaskAsync(vendor.LoginUrl2, nvCol);
            if (strResponse.Contains("Sign Out"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
    */
}