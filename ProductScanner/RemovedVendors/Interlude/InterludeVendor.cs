using ProductScanner.Core;

namespace Interlude
{
  public class InterludeVendor : HomewareVendor
  {
    public InterludeVendor() : base(156, "Interlude", "IL")
    {
      LoginUrl = "https://www.interludehome.com/wholesale_signin.aspx?ReturnUrl=";
      LoginUrl2 = "https://www.interludehome.com/default.aspx?";
      Username = "tessa@example.com";
      Password = "passwordhere";
      DiscoveryNotes = "All data comes from public site after login";
    }
  }
}
