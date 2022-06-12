using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace DistinctiveDesigns
{
  public class DistinctiveDesignsVendorAuthenticator : IVendorAuthenticator<DistinctiveDesignsVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new DistinctiveDesignsVendor();
      var webClient = new WebClientExtended();

      var home = await webClient.DownloadPageAsync(vendor.PublicUrl);
      var authLink = home.QuerySelector(".authorization-link a").Attributes["href"].Value;

      var resp = await webClient.DownloadPageAsync(authLink);

      var key = resp.QuerySelector("input[name='form_key']").Attributes["value"].Value;
      var nvCol = new NameValueCollection();
      nvCol.Add("form_key", key);
      nvCol.Add("login[username]", vendor.Username);
      nvCol.Add("login[password]", vendor.Password);
      nvCol.Add("persistent_remember_me", "on");
      nvCol.Add("send", "");

      //webClient.Headers["Referer"] = vendor.LoginUrl;
      //webClient.Headers["Accept-Encoding"] = "gzip, deflate, br";

      var postLink = resp.QuerySelector("#login-form").Attributes["action"].Value;

      var postPage = await webClient.DownloadPageAsync(postLink, nvCol);
      var dashboard = await webClient.DownloadPageAsync("https://distinctivedesigns.com/customer/account/");
      //if (dashboard.InnerText.ContainsIgnoreCase("Tessa Luu"))
      return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      //return new AuthenticationResult(false);
    }
  }
}