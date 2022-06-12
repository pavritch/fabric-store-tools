using ProductScanner.Core;
using System.Linq;
using System.Text;

namespace WildWoodLamps
{
  public class WildWoodLampsVendor : HomewareVendor
  {
    public WildWoodLampsVendor() : base(180, "Wildwood Lamps", "WW")
    {
      PublicUrl = "http://wildwoodlamps.com/";

      LoginUrl = "https://supercat.supercatsolutions.com/wwjc/e/wwfc-2/login";
      LoginUrl2 = "https://supercat.supercatsolutions.com/wwjc/e/wwfc-2/products";

      Username = "ddddd";
      Password = "ddddd";

      DiscoveryNotes = "All static data comes from spread sheet. Stock comes from site scrape";
      UsesStaticFiles = true;
      StaticFileVersion = 4;
      //DeveloperComments = "I think the only thing left here is to clean up the name/description";
    }
  }
}
