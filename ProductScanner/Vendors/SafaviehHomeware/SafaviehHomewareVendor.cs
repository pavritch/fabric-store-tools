using ProductScanner.Core;
using ProductScanner.Core.Scanning.ProductProperties;

namespace SafaviehHomeware
{
  public class SafaviehHomewareVendor : HomewareVendor
  {
    public SafaviehHomewareVendor() : base(166, "Safavieh", "SF", 2.2M, 2.2M * 1.7M)
    {
      Username = "example";
      Password = "dddddd";

      DiscoveryNotes = "Discovery and some details come from spreadsheet. Additional details come from public site.";

      UsesStaticFiles = true;

      // Since the prices only come from the spreadsheet, we'll use that as discovery, and then search for products on the site

      // Images on the FTP server
    }
  }
}
