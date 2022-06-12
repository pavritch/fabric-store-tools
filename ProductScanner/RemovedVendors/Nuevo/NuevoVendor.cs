using ProductScanner.Core;

namespace Nuevo
{
  public class NuevoVendor : HomewareVendor
  {
    public NuevoVendor() : base(164, "Nuevo", "NV", 2.2M, 2.2M * 1.7M)
    {
      LoginUrl = "http://www.nuevoliving.com/NuevoCore.cfm";
      PublicUrl = "http://www.nuevoliving.com/";
      Username = "tessa@example";
      Password = "dfsdfsdfdsf";
      DiscoveryNotes = "Most data comes from static spreadsheet. Product pages are scraped for additional info.";
      UsesStaticFiles = true;
    }
  }
}
