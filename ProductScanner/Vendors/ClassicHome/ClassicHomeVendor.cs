using ProductScanner.Core;

namespace ClassicHome
{
  public class ClassicHomeVendor : HomewareVendor
  {
    public ClassicHomeVendor() : base(175, "Classic Home", "CL")
    {
      PublicUrl = "http://www.classichome.com/";
      LoginUrl = "https://www.classichome.com/";
      LoginUrl2 = "https://www.classichome.com/customer/account/loginPost/";
      Username = "sales@example.com";
      Password = "passwordhere";
      DiscoveryNotes = "All data comes from the website after login";

      RunDiscontinuedPercentageCheck = false;
    }
  }
}
