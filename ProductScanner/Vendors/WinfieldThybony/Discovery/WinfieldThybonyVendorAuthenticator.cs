using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace WinfieldThybony.Discovery
{
  public class WinfieldThybonyVendorAuthenticator : IVendorAuthenticator<WinfieldThybonyVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var values = new NameValueCollection();
      values.Add("usr", "insideavenue");
      values.Add("pwd", "passwordhereK");
      values.Add("submit", "Login");

      var webClient = new WebClientExtended();
      var vendor = new WinfieldThybonyVendor();
      await webClient.DownloadPageAsync(vendor.LoginUrl);

      var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl2, values);
      if (loginPage.InnerText.Contains("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}