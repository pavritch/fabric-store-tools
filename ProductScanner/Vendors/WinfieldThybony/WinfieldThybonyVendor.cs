using System.Collections.Generic;
using System.Configuration;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace WinfieldThybony
{
  public class WinfieldThybonyVendor : Vendor
  {
    public WinfieldThybonyVendor() : base(90, "Winfield Thybony", StoreType.InsideFabric, "WT")
    {
      RunDiscontinuedPercentageCheck = false;
      UsesStaticFiles = true;
      StaticFileVersion = 3;

      ProductGroups = new List<ProductGroup> { ProductGroup.Wallcovering };

      Username = "dddddd";
      Password = "dddddddd";
      LoginUrl = "https://www.e-designtrade.com/login.asp";
      LoginUrl2 = "https://www.e-designtrade.com/logina.asp";
      DiscoveryNotes = "Discovery comes from searching public site after login. Details come from product pages. Pricing comes from static spreadsheet that is build on demand using Kravet's back-end.";
    }
  }
}