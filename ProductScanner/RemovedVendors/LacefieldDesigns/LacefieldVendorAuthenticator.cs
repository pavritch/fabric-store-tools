using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace LacefieldDesigns
{
  /*public class LacefieldVendorAuthenticator : IVendorAuthenticator<LacefieldDesignsVendor>
  {
      public async Task<AuthenticationResult> LoginAsync()
      {
          var vendor = new LacefieldDesignsVendor();
          var webClient = new WebClientExtended();

          var loginPage = await webClient.DownloadPageAsync("http://www.lacefielddesigns.com/profile/login/");
          var captchaAnswer = loginPage.QuerySelector("div.userpro-input:contains('Answer') input[type='hidden']").Attributes["value"].Value;
          var uniqueId = loginPage.QuerySelector("#unique_id").Attributes["value"].Value;
          var nonce = loginPage.QuerySelector("#_myuserpro_nonce").Attributes["value"].Value;

          var nvCol = new NameValueCollection();
          nvCol.Add(string.Format("force_redirect_uri-{0}", uniqueId), "");
          nvCol.Add(string.Format("redirect_uri-{0}", uniqueId), "http://www.lacefielddesigns.com/profile");
          nvCol.Add("_myuserpro_nonce", nonce);
          nvCol.Add("_wp_http_referer", "/profile/login");
          nvCol.Add("unique_id", uniqueId);
          nvCol.Add(string.Format("username_or_email-{0}", uniqueId), vendor.Username);
          nvCol.Add(string.Format("user_pass-{0}", uniqueId), vendor.Password);
          nvCol.Add(string.Format("antispam-{0}", uniqueId), captchaAnswer);
          nvCol.Add(string.Format("answer-{0}", uniqueId), captchaAnswer);
          nvCol.Add(string.Format("rememberme-{0}", uniqueId), "true");
          nvCol.Add("action", "userpro_process_form");
          nvCol.Add("template", "login");
          nvCol.Add("group", "default");
          nvCol.Add("shortcode", "");
          nvCol.Add("up_username", "0");

          var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
          var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
          if (dashboard.InnerText.ContainsIgnoreCase("Tessa Luu"))
          {
              return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
          }
          return new AuthenticationResult(false);
      }
  }*/
}