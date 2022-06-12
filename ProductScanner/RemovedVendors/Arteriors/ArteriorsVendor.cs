using ProductScanner.Core;

namespace Arteriors
{
  public class ArteriorsVendor : HomewareVendor
  {
    // Should probably just use their Retail price
    public ArteriorsVendor() : base(151, "Arteriors", "AR")
    {
      LoginUrl = "https://www.arteriorshome.com/customer/account/loginPost/";
      LoginUrl2 = "https://www.arteriorshome.com/customer/account/";

      DiscoveryNotes = "All data comes from website after logging in.";
      UsesIMAP = true;

      Username = "sales@example.com";
      Password = "24123423142312";

      RunDiscontinuedPercentageCheck = false;
    }

    public override string GetOurPriceMarkupDescription()
    {
      return "Our Price is their required IMAP (20% less than their MSRP)";
    }
  }
}
