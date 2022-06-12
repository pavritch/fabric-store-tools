using ProductScanner.Core;

namespace SuryaHomeware
{
  public class SuryaHomewareVendor : HomewareVendor
  {
    public SuryaHomewareVendor() : base(169, "Surya", "SR")
    {
      Username = "tddddddm";
      Password = "ddddd";
      LoginUrl = "https://www.surya.com/sign-in.aspx";
      PublicUrl = "https://www.surya.com";

      DiscoveryNotes = "Discovery and details are done using the public website. This also includes pricing and stock information.";
      UsesStaticFiles = true;
      //RunDiscontinuedPercentageCheck = false;
      // pillows, poufs and throws only
    }
  }
}
