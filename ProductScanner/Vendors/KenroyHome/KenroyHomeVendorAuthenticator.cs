using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace KenroyHome
{
  public class KenroyHomeVendorAuthenticator : IVendorAuthenticator<KenroyHomeVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new KenroyHomeVendor();
      var webClient = new WebClientExtended();

      webClient.Headers["Accept-Encoding"] = "gzip, deflate, sdch, br";

      var root = await GetGZippedDocument(webClient, vendor.LoginUrl);
      var token = root.QuerySelector("input[name='authenticity_token']").Attributes["value"].Value;

      var nvCol = new NameValueCollection();
      nvCol.Add("utf8", "✓");
      nvCol.Add("authenticity_token", token);
      nvCol.Add("username", vendor.Username);
      nvCol.Add("password", vendor.Password);
      nvCol.Add("return_url", "/khl/e/2/products");

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
      webClient.Headers["Accept-Encoding"] = "";
      var main = await webClient.DownloadPageAsync(vendor.LoginUrl2);

      if (main.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }

    private async Task<HtmlNode> GetGZippedDocument(WebClientExtended webClient, string url)
    {
      var firstPage = await webClient.DownloadDataTaskAsync(url);
      var str = Encoding.UTF8.GetString(firstPage.Decompress());
      var page = new HtmlDocument();
      page.LoadHtml(str);
      return page.DocumentNode.Clone();
    }
  }
}