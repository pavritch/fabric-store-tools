using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace RobertAllen
{
  public class RobertAllenVendor : Vendor
  {
    public RobertAllenVendor() : base(6, "Robert Allen", StoreType.InsideFabric, "RA", StockCapabilities.ReportOnHand, 1.4M, 1.4M * 1.9M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };

      Username = "sdddddd";
      Password = "dddddd";
      LoginUrl = "https://www.robertallendesign.com/customer/account/loginPost/";
      LoginUrl2 = "https://www.robertallendesign.com/";

      DiscoveryNotes = "Discovery comes from scanning public site and outlet site. Stock and additional details are pulled from individual product pages.";

      IsClearanceSupported = true;
      UsesIMAP = true;
    }
  }

  public class BeaconHillVendor : Vendor
  {
    public BeaconHillVendor() : base(9, "Beacon Hill", StoreType.InsideFabric, "BH", StockCapabilities.ReportOnHand, 1.4M, 1.4M * 1.7M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };

      Username = "sdddddd";
      Password = "ddddd";
      LoginUrl = "https://www.robertallendesign.com/customer/account/loginPost/";
      LoginUrl2 = "https://www.robertallendesign.com/";

      DiscoveryNotes = "Discovery comes from scanning public site and outlet site. Stock and additional details are pulled from individual product pages.";

      IsClearanceSupported = true;
      // Peter says this should be true, even though I'm just using default pricing
      // I think the 2.0 markup represents MAP
      UsesIMAP = true;
    }

    public override string GetOurPriceMarkupDescription()
    {
      return string.Format("{0} minus $1.", OurPriceMarkup);
    }
  }
}