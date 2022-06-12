using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace FSchumacher.Discovery
{
    public class FSchumacherVendorAuthenticator : IVendorAuthenticator<FSchumacherVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var webClient = new WebClientExtended();
            var vendor = new FSchumacherVendor();

            var values = new NameValueCollection();
            values.Add("LoginToken", vendor.Username);
            values.Add("Password", vendor.Password);
            var htmlLoginPage = await webClient.DownloadPageAsync(vendor.LoginUrl, values);

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(htmlLoginPage.InnerText);
            var result = Convert.ToBoolean(jsonResponse.loginsuccess);

            if (result)
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}