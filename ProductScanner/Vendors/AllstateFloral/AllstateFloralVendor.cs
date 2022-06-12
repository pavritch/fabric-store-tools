using ProductScanner.Core;

namespace AllstateFloral
{
  public class AllstateFloralVendor : HomewareVendor
  {
    public AllstateFloralVendor() : base(150, "Allstate Floral", "AF")
    {
      PublicUrl = "http://www.allstatefloral.com/";
      LoginUrl = "https://www.allstatefloral.com/loginverify.cfm";
      DiscoveryNotes = "All data comes from the back-end server";

      Username = "2342424234234";
      Password = "sdfasdfsfdsfsd";

      MinimumCost = 50;
    }
  }
}
