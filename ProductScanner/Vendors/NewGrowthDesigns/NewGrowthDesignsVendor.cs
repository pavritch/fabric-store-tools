using ProductScanner.Core;

namespace NewGrowthDesigns
{
  public class NewGrowthDesignsVendor : HomewareVendor
  {
    public NewGrowthDesignsVendor() : base(163, "New Growth Designs", "NG")
    {
      LoginUrl = "https://newgrowthdesigns.com/account/login";
      LoginUrl2 = "https://newgrowthdesigns.com/account";
      PublicUrl = "http://www.newgrowthdesigns.com";
      Username = "sales@example.com";
      Password = "passwordhere";
      DiscoveryNotes = "All data comes from public site after login";

      // Made to order:  2-3 weeks ship
    }
  }
}
