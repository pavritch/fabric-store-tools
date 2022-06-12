using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace NewGrowthDesigns
{
  public class NewGrowthDesignsVendorAuthenticator : IVendorAuthenticator<NewGrowthDesignsVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var vendor = new NewGrowthDesignsVendor();
      var webClient = new WebClientExtended();

      var nvCol = new NameValueCollection();
      nvCol.Add("form_type", "customer_login");
      nvCol.Add("utf8", "✓");
      nvCol.Add("customer[email]", vendor.Username);
      nvCol.Add("customer[password]", vendor.Password);

      var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
      var dashboard = await webClient.DownloadPageAsync(vendor.LoginUrl2);
      if (dashboard.InnerText.ContainsIgnoreCase("mary joe"))
        return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
      return new AuthenticationResult(false);
    }
  }
}