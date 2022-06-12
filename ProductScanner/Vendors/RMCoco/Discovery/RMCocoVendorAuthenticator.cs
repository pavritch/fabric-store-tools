using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace RMCoco.Discovery
{
  /*
  public class RMCocoVendorAuthenticator : IVendorAuthenticator<RMCocoVendor>
  {
      public async Task<AuthenticationResult> LoginAsync()
      {
          var webClient = new WebClientExtended();
          var vendor = new RMCocoVendor();

          var formData = new NameValueCollection();
          formData["useremail"] = vendor.Username;
          formData["password"] = vendor.Password;
          formData["remember"] = "off";

          var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, formData);

          var dashboard = await webClient.DownloadPageAsync("https://rmcoco.com/account.php");
          var success = dashboard.InnerHtml.Contains("Tessa Luu");
          return new AuthenticationResult(success, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      }
  }
  */
}