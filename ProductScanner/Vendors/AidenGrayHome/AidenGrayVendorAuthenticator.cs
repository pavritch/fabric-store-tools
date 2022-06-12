using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace AidanGrayHome
{
  public class AidenGrayVendorAuthenticator : IVendorAuthenticator<AidanGrayHomeVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new AidanGrayHomeVendor();
      var webClient = new WebClientExtended();

      var resp = await webClient.DownloadPageAsync("https://www.aidangrayhome.com/wholesale/customer/account/login/");
      var key = resp.QuerySelector("input[name='form_key']").Attributes["value"].Value;

      var nvCol = new NameValueCollection();
      nvCol.Add("form_key", key);
      nvCol.Add("login[username]", vendor.Username);
      nvCol.Add("login[password]", vendor.Password);
      nvCol.Add("send", "");


      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);

      //webClient.Headers["Accept-Encoding"] = "gzip, deflate, sdch, br";
      webClient.Headers["Referer"] = "https://www.aidangrayhome.com/wholesale/customer/account/login/";

      var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
      if (dashboard.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}