using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Norwall
{
  public class NorwallVendor : Vendor
  {
    public NorwallVendor() : base(92, "Norwall", StoreType.InsideFabric, "NW", StockCapabilities.CheckForQuantity)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Wallcovering };
      DiscoveryNotes = "Discovery comes from scanning collection pages on website";

      Username = "sales@example.com";
      Password = "ddddd";
      LoginUrl = "http://www.pattonwallcoverings.net/index.php";
      PublicUrl = "http://norwall.net";
    }
  }
}