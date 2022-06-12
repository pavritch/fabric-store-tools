using ProductScanner.Core;

namespace TrendLighting
{
  public class TrendLightingVendor : HomewareVendor
  {
    public TrendLightingVendor() : base(170, "Trend Lighting", "TL")
    {
      PublicUrl = "http://www.tlighting.com";

      LoginUrl = "http://www.tlighting.com/customer/account/loginPost/";
      LoginUrl2 = "http://www.tlighting.com/retail/filter.html";
      Username = "tessa@example.com";
      Password = "passwordhere";

      DiscoveryNotes = "All data comes from public site after login.";
    }
  }
}
