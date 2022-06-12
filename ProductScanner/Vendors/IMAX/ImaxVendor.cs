using ProductScanner.Core;

namespace IMAX
{
  public class ImaxVendor : HomewareVendor
  {
    public ImaxVendor() : base(174, "Imax", "IM")
    {
      LoginUrl = "https://imaxcorp.com/account/login?ReturnUrl=/";
      LoginUrl2 = "https://imaxcorp.com/account";
      PublicUrl = "http://www.imaxcorp.com/";
      Username = "sales@example.com";
      Password = "dddddd";
      DiscoveryNotes = "All data comes from static file";

      UsesStaticFiles = true;
      StaticFileVersion = 1;
    }
  }
}