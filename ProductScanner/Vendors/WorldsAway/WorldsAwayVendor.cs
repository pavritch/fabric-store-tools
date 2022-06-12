using ProductScanner.Core;

namespace WorldsAway
{
  public class WorldsAwayVendor : HomewareVendor
  {
    public WorldsAwayVendor() : base(171, "Worlds Away", "WA")
    {
      PublicUrl = "http://www.worlds-away.com";

      LoginUrl = "https://www.worlds-away.com/login.php";
      LoginUrl2 = "https://www.worlds-away.com/login.php?action=check_login";
      Username = "sddddddd";
      Password = "ddddddd";

      RunDiscontinuedPercentageCheck = false;

      DiscoveryNotes = "All data comes from public site after login.";
      //DeveloperComments = "I think the only thing left here is to clean up the name/description";
    }
  }
}
