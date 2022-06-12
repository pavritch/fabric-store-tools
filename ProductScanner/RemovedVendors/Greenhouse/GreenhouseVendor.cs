using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Greenhouse
{
  public class GreenhouseVendor : Vendor
  {
    public GreenhouseVendor() : base(32, "Greenhouse", StoreType.InsideFabric, "GD", StockCapabilities.ReportOnHand)
    {
      DiscoveryNotes = "Uses JSON endpoint for discovery, and then scrapes the public website for details";
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };
      SwatchesEnabled = false;

      Username = "customerservice@example.com";
      Password = "24234324";
      LoginUrl = "https://www.greenhousefabrics.com/user/login";
      PublicUrl = "https://www.greenhousefabrics.com/";
    }

    public override string GetOurPriceMarkupDescription()
    {
      return "Uses the max of 11.99 and our standard markup minus $1. Uses 1.8 X cost for those marked as clearance.";
    }
  }
}