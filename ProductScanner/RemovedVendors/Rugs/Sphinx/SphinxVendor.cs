using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Sphinx
{
  public class SphinxVendor : RugVendor
  {
    public SphinxVendor() : base(103, "Sphinx", "SP", StockCapabilities.None)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
      Username = "1452354523";
      Password = "2345434545";
      LoginUrl = "http://www.owrugs.net/cgi-bin2/login.mbr/start";
      PublicUrl = "http://www.owrugs.com/";

      DiscoveryNotes = "Pricing is pulled from manually created static file. Discovery comes from a table on the back end. Product details are pulled from the public website.";

      UsesStaticFiles = true;
      UsesIMAP = true;
    }
  }
}
