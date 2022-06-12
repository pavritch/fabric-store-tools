using ProductScanner.Core;

namespace SquareFeathers
{
  public class SquareFeathersVendor : HomewareVendor
  {
    public SquareFeathersVendor() : base(167, "Square Feathers", "SQ", 2.0M, 2.0M * 1.7M)
    {
      Username = "example";
      Password = "passwordhere";
      LoginUrl = "https://www.surya.com/sign-in.aspx";
      PublicUrl = "http://www.squarefeathers.com";

      // Made to order:  2-3 weeks ship
      DiscoveryNotes = "All data comes from public site without login.";
      RunDiscontinuedPercentageCheck = false;

      UsesStaticFiles = true;
    }
  }
}
