using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Nourison
{
  public class NourisonVendor : RugVendor
  {
    public NourisonVendor() : base(121, "Nourison", "NO")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
      Username = "2323232";
      Password = "123456";
      PublicUrl = "http://www.nourison.com/";

      DiscoveryNotes = "Uses public website for product details. Uses manually created excel file (from PDF given by Nourison) for pricing.";

      UsesStaticFiles = true;
      StaticFileVersion = 2;
      UsesIMAP = true;

      RunDiscontinuedPercentageCheck = false;
    }
  }
}
