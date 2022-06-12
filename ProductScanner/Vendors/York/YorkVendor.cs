using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace York
{
  public class YorkVendor : Vendor
  {
    public YorkVendor() : base(74, "York", StoreType.InsideFabric, "YK", StockCapabilities.ReportOnHand)
    {
      SwatchCost = 2M;
      UsesStaticFiles = true;
      StaticFileVersion = 7;
      UsesIMAP = true;
      ProductGroups = new List<ProductGroup> { ProductGroup.Wallcovering };

      DiscoveryNotes = "Uses public website to find collections, and then each product in each collection. Static spreadsheets used only for metadata.";

      Username = "dddddd";
      Password = "Lddddddd";
      LoginUrl = "https://www.yorkwall.com/login";
      PublicUrl = "https://www.yorkwall.com/";

      // yorkinventoryfiles@gmail.com
      // passwordhereK
    }

    public override string GetOurPriceMarkupDescription()
    {
      return "For books that require MAP pricing, we use the York MSRP * .85. Otherwise, the markup is 1.4";
    }
  }
}