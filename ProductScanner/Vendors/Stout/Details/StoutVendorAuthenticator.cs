using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Stout.Details
{
  public class StoutVendorAuthenticator : IVendorAuthenticator<StoutVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      try
      {
        var vendor = new StoutVendor();
        var nvCol = new NameValueCollection();
        nvCol.Add("un", vendor.Username);
        nvCol.Add("pw", vendor.Password);
        var webClient = new WebClientExtended();
        webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

        var strResponse = await webClient.PostValuesTaskAsync(vendor.LoginUrl, nvCol);
        if (strResponse.IndexOf("Welcome") >= 0 && strResponse.IndexOf("MARYJOE") >= 0)
          return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      }
      catch (Exception)
      {
        return new AuthenticationResult(false);
      }
      return new AuthenticationResult(false);
    }
  }
}