using ProductScanner.Core;

namespace KenroyHome
{
  public class KenroyHomeVendor : HomewareVendor
  {
    public KenroyHomeVendor() : base(158, "Kenroy Home", "KH")
    {
      LoginUrl = "https://catalog.kenroyhome.com/khl/e/2/login";
      LoginUrl2 = "https://catalog.kenroyhome.com/khl/e/2/products";
      PublicUrl = "http://www.kenroyhome.com";
      Username = "tluu";
      Password = "ddddd";

      // Sign in under "partners", exlcude all fountains products
      // within the b2b site there's an inventory page, which seems to work with most product numbers
      // that are pulled from the public site during discovery

      RunDiscontinuedPercentageCheck = false;

      UsesStaticFiles = true;
      StaticFileVersion = 3;
    }
  }
}
