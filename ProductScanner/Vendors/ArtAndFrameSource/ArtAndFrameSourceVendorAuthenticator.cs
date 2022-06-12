using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace ArtAndFrameSource
{
  public class ArtAndFrameSourceVendorAuthenticator : IVendorAuthenticator<ArtAndFrameSourceVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new ArtAndFrameSourceVendor();
      var webClient = new WebClientExtended();

      var resp = await webClient.DownloadPageAsync(vendor.LoginUrl);
      var formKey = resp.QuerySelector("input[name='form_key']").Attributes["value"].Value;

      var nvCol = new NameValueCollection();
      nvCol.Add("form_key", formKey);
      nvCol.Add("login[username]", vendor.Username);
      nvCol.Add("login[password]", vendor.Password);
      nvCol.Add("send", "");

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl2, nvCol);
      if (postPage.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}