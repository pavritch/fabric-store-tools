using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace PhillipJeffries
{
  public class PhillipJeffriesVendor : Vendor
  {
    public PhillipJeffriesVendor() : base(117, "Phillip Jeffries", StoreType.InsideFabric, "PJ", StockCapabilities.InOrOutOfStock)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Wallcovering };
      DiscoveryNotes = "Discovery comes from scanning search page behind login page";

      Username = "ddddd";
      Password = "ddddd";
      LoginUrl = "https://www.phillipjeffries.com/api/session";
      LoginUrl2 = "https://www.phillipjeffries.com/api/cart/samples";
      PublicUrl = "https://www.phillipjeffries.com/";

      RunDiscontinuedPercentageCheck = false;

      UsesStaticFiles = true;
      StaticFileVersion = 4;
    }
  }
}
