using ProductScanner.Core;

namespace GoHomeLTD
{
  public class GoHomeVendor : HomewareVendor
  {
    public GoHomeVendor() : base(177, "Go Home", "GH")
    {
      LoginUrl = "http://www.gohomeltd.com/Store/Default.aspx";
      Username = "example";
      Password = "ddddd";

      MinimumCost = 150;
      RunDiscontinuedPercentageCheck = false;
    }
  }
}