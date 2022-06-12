using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Interlude
{
  public class InterludeVendorAuthenticator : IVendorAuthenticator<InterludeVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new InterludeVendor();
      var webClient = new WebClientExtended();

      var resp = await webClient.DownloadPageAsync(vendor.LoginUrl);
      var eventValidation = resp.QuerySelector("#__EVENTVALIDATION").Attributes["value"].Value;
      var viewstate = resp.QuerySelector("#__VIEWSTATE").Attributes["value"].Value;

      var nvCol = new NameValueCollection();
      nvCol.Add("__LASTFOCUS", "");
      nvCol.Add("__EVENTTARGET", "");
      nvCol.Add("__EVENTARGUMENT", "");
      nvCol.Add("__VIEWSTATE", viewstate);
      nvCol.Add("__VIEWSTATEGENERATOR", "2F2B7CAE");
      nvCol.Add("__SCROLLPOSITIONX", "");
      nvCol.Add("__SCROLLPOSITIONY", "");
      nvCol.Add("__EVENTVALIDATION", eventValidation);
      nvCol.Add("EMail", vendor.Username);
      nvCol.Add("txtPassword", vendor.Password);
      nvCol.Add("LoginButton", "Login");
      nvCol.Add("validateuser$txtAccountNo", "");
      nvCol.Add("ForgotEMail", "");

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
      var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
      if (dashboard.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}