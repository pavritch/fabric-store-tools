using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace InnovationsUsa.Discovery
{
    public class InnovationsUsaVendorAuthenticator : IVendorAuthenticator<InnovationsUsaVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new InnovationsUsaVendor();
            var values = new NameValueCollection();
            values.Add("username", vendor.Username);
            values.Add("passwd", vendor.Password);
            values.Add("op2", "login");
            values.Add("lang", "english");
            values.Add("force_session", "1");
            values.Add("return", "B:aHR0cHM6Ly9vcmRlci10cmFjay5jb20vaW5kZXgucGhwP29wdGlvbj1jb21fanVtaSZhbXA7dmlldz1hcHBsaWNhdGlvbiZhbXA7ZmlsZWlkPTM=");
            values.Add("message", "0");
            values.Add("loginfrom", "loginform");
            values.Add("remember", "yes");
            values.Add("Submit", "Login");

            var webClient = new WebClientExtended();
            var firstPage = await webClient.DownloadPageAsync(vendor.LoginUrl);
            var cbsecurity = firstPage.QuerySelector("input[name='cbsecuritym3']").Attributes["value"].Value;
            values.Add("cbsecuritym3", cbsecurity);

            var login = await webClient.DownloadPageAsync(vendor.LoginUrl, values);
            return new AuthenticationResult(login.InnerText.ContainsIgnoreCase("Your Profile"),
                webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
        }
    }
}