using ProductScanner.Core;

namespace DistinctiveDesigns
{
  public class DistinctiveDesignsVendor : HomewareVendor
  {
    public DistinctiveDesignsVendor() : base(154, "Distinctive Designs", "DD")
    {
      LoginUrl = "https://distinctivedesigns.com/customer/account/login/";
      LoginUrl2 = "https://distinctivedesigns.com/customer/account/loginPost/";
      PublicUrl = "https://www.distinctivedesigns.com";
      Username = "tessa@example";
      Password = "password
      DiscoveryNotes = "All data comes from static file.";

      RunDiscontinuedPercentageCheck = false;

      UsesStaticFiles = true;
      StaticFileVersion = 4;
    }
  }
}
