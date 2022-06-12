using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace York.Discovery
{
  public class YorkVendorAuthenticator : IVendorAuthenticator<YorkVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var webClient = new WebClientExtended();
      var vendor = new YorkVendor();
      var htmlLoginPage = await webClient.DownloadPageAsync(vendor.LoginUrl);
      var requiredElements = new[]
      {
                // not totally complete - but enough to get a feel if structure is about what's expected
                "input[name='LW3WEBUSR']",
                "input[name='LW3USRPAS']",
                "input[name='_SESSIONKEY']",
                "input[name='_SERVICENAME']",
                "input[name='LWBOP01ST']",
                "input[name='_PARTITION']",
                "input[name='_WEBAPP']",
            };

      if (!htmlLoginPage.InnerHtml.HasAllRequiredElements(requiredElements))
        throw new Exception("Login failed. Html on login page has changed.");

      var nvCol = new NameValueCollection();
      nvCol.Add("ddddd", vendor.Username);
      nvCol.Add("ddddd", vendor.Password);

      var namedElements = new[] { "STDWEBUSR", "STDSESSID", "WW3SUBSIT", "LWBOP01ST", "_SERVICENAME", "_WEBAPP", "_WEBROUTINE", "_PARTITION", "_LANGUAGE", "_SESSIONKEY", "_LW3TRCID", "STDRENTRY", "STDROWNUM" };

      foreach (var item in htmlLoginPage.InnerHtml.GetFormPostValuesByName(namedElements))
        nvCol.Add(item.Key, item.Value);

      webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

      var strResponse = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
      if (strResponse.InnerText.ContainsIgnoreCase("Welcome, TESSA"))
      {
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      }
      return new AuthenticationResult(false);
    }
  }
}