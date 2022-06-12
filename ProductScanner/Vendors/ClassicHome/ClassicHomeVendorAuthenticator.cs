using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace ClassicHome
{
  public class ClassicHomeVendorAuthenticator : IVendorAuthenticator<ClassicHomeVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new ClassicHomeVendor();
      var webClient = new WebClientExtended();

      var firstPage = await webClient.DownloadPageAsync(vendor.LoginUrl);

      var nvCol = new NameValueCollection();
      nvCol.Add("form_key", firstPage.QuerySelector("input[name='form_key']").Attributes["value"].Value);
      nvCol.Add("login[username]", vendor.Username);
      nvCol.Add("login[password]", vendor.Password);
      nvCol.Add("referer", "aHR0cDovL3d3dy5jbGFzc2ljaG9tZS5jb20v");

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl2, nvCol);
      if (postPage.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}