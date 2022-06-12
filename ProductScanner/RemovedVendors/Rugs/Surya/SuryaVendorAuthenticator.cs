using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Surya
{
  public class SuryaVendorAuthenticator : IVendorAuthenticator<SuryaVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var webClient = new WebClientExtended();
      var vendor = new SuryaVendor();
      var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl);

      var values = new NameValueCollection();
      values.Add("manScript_HiddenField", ";;AjaxControlToolkit, Version=4.1.51116.0, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e:en-US:fd384f95-1b49-47cf-9b47-2fa2a921a36a:475a4ef5:effe2a26:7e63a579");
      values.Add("__EVENTTARGET", "");
      values.Add("__EVENTARGUMENT", "");
      values.Add("__SCROLLPOSITIONX", "");
      values.Add("__SCROLLPOSITIONY", "");
      values.Add("lng", "en-us");
      values.Add("p$lt$ctl05$SearchBox$txtWord_exWatermark_ClientState", "");
      values.Add("p$lt$ctl05$SearchBox$txtWord", "");
      values.Add("p$lt$ctl06$pageplaceholder$p$lt$ctl01$logonform$Login1$UserName", "tessa@example");
      values.Add("p$lt$ctl06$pageplaceholder$p$lt$ctl01$logonform$Login1$Password", "passwordher
      values.Add("p$lt$ctl06$pageplaceholder$p$lt$ctl01$logonform$Login1$chkRememberMe", "on");
      values.Add("p$lt$ctl06$pageplaceholder$p$lt$ctl01$logonform$Login1$LoginButton", "Log on");
      values.Add("p$lt$ctl06$pageplaceholder$p$lt$ctl01$logonform$txtPasswordRetrieval", "");
      values.Add("__VIEWSTATE", loginPage.QuerySelector("#__VIEWSTATE").Attributes["value"].Value);

      var login = await webClient.DownloadPageAsync(vendor.LoginUrl, values);
      var success = login.InnerText.ContainsIgnoreCase("Tessa  Luu");
      return new AuthenticationResult(success, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
    }
  }
}