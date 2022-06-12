using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace PhillipJeffries
{
    public class PhillipJeffriesVendorAuthenticator : IVendorAuthenticator<PhillipJeffriesVendor>
    {
        public Task<AuthenticationResult> LoginAsync()
        {
            return Task.FromResult(new AuthenticationResult(true));
            /*
            var vendor = new PhillipJeffriesVendor();
            var webClient = new WebClientExtended();


            var values = new NameValueCollection();
            values.Add("remember_me", "false");
            values.Add("email", vendor.Username);
            values.Add("password", vendor.Password);

            // The Content-Type header cannot be changed from its default value for this request.

            webClient.Headers["Accept-Encoding"] = "gzip, deflate, br";
            webClient.Headers["Accept-Language"] = "en-US,en;q=0.8,ms;q=0.6,la;q=0.4";

            var signIn = await webClient.DownloadPageAsync("https://www.phillipjeffries.com/sign-in");

            webClient.Headers["Accept"] = "application/json, text/javascript";
            webClient.Headers["Content-Type"] = "application/json;charset=UTF-8";
            webClient.Headers["Referer"] = "https://www.phillipjeffries.com/sign-in";
            webClient.Headers["Origin"] = "https://www.phillipjeffries.com";
            //webClient.Headers.Add("DNT", "1");
            //webClient.Headers.Add("X-CSRF-Token", "SlJ0kxJg7mc/v4v5RUrGNKofpE9EOg1kTWH5R+tMXMKb+Rh2xAFu2hATlJplGf6/1mWMyuyKyZi2mp5wrWFYHg==");

            var bytes = Encoding.Default.GetBytes(string.Format("{{\"email\":\"{0}\",\"password\":\"{1}\",\"remember_me\":false}}", 
                vendor.Username, vendor.Password));
            webClient.UploadData(vendor.LoginUrl, "POST", bytes);

            var login = await webClient.DownloadPageAsync(vendor.LoginUrl, values);

            var accountPage = await webClient.DownloadPageAsync(vendor.LoginUrl2);
            if (accountPage.InnerText.ContainsIgnoreCase("My Account"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
            */
        }
    }
}