using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Maxwell
{
  public class MaxwellVendor : Vendor
  {
    public MaxwellVendor() : base(56, "Maxwell", StoreType.InsideFabric, "MX", StockCapabilities.ReportOnHand, 1.4M)
    {
      DiscoveryNotes = "Discovery via dynamically downloaded excel file";
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };

      Username = "dddd";
      Password = "dddd";
      LoginUrl = "https://www.maxwellfabrics.com/maxwell_user/logout?destination=user/login";
      LoginUrl2 = "https://www.maxwellfabrics.com/user/login";
      PublicUrl = "https://www.maxwellfabrics.com";
    }

    public override string GetOurPriceMarkupDescription()
    {
      return string.Format("{0}. Uses 2.5 X cost for products marked as clearance.", OurPriceMarkup);
    }
  }
}