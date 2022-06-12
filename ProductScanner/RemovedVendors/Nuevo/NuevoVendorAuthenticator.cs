using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Nuevo
{
  public class NuevoVendorAuthenticator : IVendorAuthenticator<NuevoVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new NuevoVendor();
      var webClient = new WebClientExtended();

      var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl);

      var nvCol = new NameValueCollection();
      nvCol.Add("xAction", "Login");
      nvCol.Add("Nurl", "CFID=19674909&CFTOKEN=14889280");
      nvCol.Add("xEmail", vendor.Username);
      nvCol.Add("xPassword", vendor.Password);

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
      if (postPage.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}