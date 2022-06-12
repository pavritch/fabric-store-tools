using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace WildWoodLamps
{
  public class WildWoodVendorAuthenticator : IVendorAuthenticator<WildWoodLampsVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new WildWoodLampsVendor();
      var webClient = new WebClientExtended();

      var resp = await webClient.DownloadPageAsync(vendor.LoginUrl);
      var token = resp.QuerySelector("meta[name='csrf-token']").Attributes["content"].Value;

      var nvCol = new NameValueCollection();
      nvCol.Add("utf8", "✓");
      nvCol.Add("authenticity_token", token);
      nvCol.Add("return_url", "/wwjc/e/wwfc-2/products");
      nvCol.Add("username", vendor.Username);
      nvCol.Add("password", vendor.Password);

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
      var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
      if (dashboard.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}