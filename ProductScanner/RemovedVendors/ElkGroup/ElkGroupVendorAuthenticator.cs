using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace ElkGroup
{
  public class ElkGroupVendorAuthenticator : IVendorAuthenticator<ElkGroupVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new ElkGroupVendor();
      var webClient = new WebClientExtended();

      var resp = await webClient.DownloadPageAsync(vendor.LoginUrl);

      var viewstate = resp.QuerySelector("#__VIEWSTATE").GetAttributeValue("value", "FALSE");

      var nvCol = new NameValueCollection();
      nvCol.Add("__WPPS", "u");
      nvCol.Add("__LASTFOCUS", "");
      nvCol.Add("__EVENTTARGET", "ctl01$WebPartManager1$wp146025045$wp2042132081$btnLogOn");
      nvCol.Add("__EVENTARGUMENT", "");
      nvCol.Add("__VIEWSTATE", viewstate);
      nvCol.Add("__SCROLLPOSITIONX", "");
      nvCol.Add("__SCROLLPOSITIONY", "");
      nvCol.Add("ctl01$WebPartManager1$wp146025045$wp2042132081$txtUserid", vendor.Username);
      nvCol.Add("ctl01$WebPartManager1$wp146025045$wp2042132081$txtPassword", vendor.Password);

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
      var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
      if (dashboard.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}