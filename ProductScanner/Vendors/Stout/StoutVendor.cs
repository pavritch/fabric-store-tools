using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Stout
{
  public class StoutVendor : Vendor
  {
    public StoutVendor() : base(55, "Stout", StoreType.InsideFabric, "ST", StockCapabilities.ReportOnHand)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };

      Username = "343434";
      Password = "passwordhere";
      LoginUrl = "https://www.estout.com/checklogin?ref=";
      PublicUrl = "http://www.estout.com";

      DiscoveryNotes = "Discovery and some details come from downloaded spreadsheet. Product pages are scraped for extra data.";

      RunDiscontinuedPercentageCheck = false;
    }

    public override string GetOurPriceMarkupDescription()
    {
      return "Uses MAP, defined as cost * 1.5";
    }
  }
}