using ProductScanner.Core;

namespace LacefieldDesigns
{
  public class LacefieldDesignsVendor : HomewareVendor
  {
    public LacefieldDesignsVendor() : base(159, "Lacefield Designs", "LD", 2.0M, 2.0M * 1.7M)
    {
      LoginUrl = "http://www.lacefielddesigns.com/wp-admin/admin-ajax.php";
      LoginUrl2 = "http://www.lacefielddesigns.com/profile/";
      PublicUrl = "http://www.lacefielddesigns.com/";
      Username = "tessa@example";
      Password = "passworfsdfsdfd";
      DiscoveryNotes = "All data comes from public site after login";

      // Made to order:  2-3 weeks ship
    }
  }
}
