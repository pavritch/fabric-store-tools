using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace RMCoco
{
  public class RMCocoVendor : Vendor
  {
    public RMCocoVendor() : base(57, "RM Coco", StoreType.InsideFabric, "RM", StockCapabilities.ReportOnHand)
    {
      DiscoveryNotes = "Discovery is done by searching after logging in";
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };

      Username = "ddddd";
      Password = "ddddd";
      LoginUrl = "https://rmcoco.com/action/register.php?action=login";
      PublicUrl = "https://rmcoco.com/";

      RunDiscontinuedPercentageCheck = false;
      UsesStaticFiles = true;
      StaticFileVersion = 1;
    }
  }
}