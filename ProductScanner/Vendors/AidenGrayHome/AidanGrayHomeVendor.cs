using System.Text;
using ProductScanner.Core;

namespace AidanGrayHome
{
  public class AidanGrayHomeVendor : HomewareVendor
  {
    public AidanGrayHomeVendor() : base(172, "Aidan Gray", "AG")
    {
      // no login needed 
      PublicUrl = "http://www.aidangrayhome.com/";
      LoginUrl = "https://www.aidangrayhome.com/wholesale/customer/account/loginPost/";
      LoginUrl2 = "https://www.aidangrayhome.com/wholesale/customer/account/";
      Username = "sales@example.com";
      Password = "passwordhere";

      DiscoveryNotes = "Everything (details, pricing, stock, images) comes from the public website";

      RunDiscontinuedPercentageCheck = false;

      // The retail prices shown on their site is 2.75 mark up, so if you divide it by 2.75 you’ll get our cost.
      // For pricing, use the published price – 20%.
    }
  }
}
