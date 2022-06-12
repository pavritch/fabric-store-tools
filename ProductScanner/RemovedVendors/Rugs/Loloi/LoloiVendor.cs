using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Loloi
{
  public class LoloiVendor : RugVendor
  {
    public LoloiVendor() : base(125, "Loloi", "LO", StockCapabilities.None, 1.6M, 1.6M * 1.7M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
      Username = "david@example.com";
      Password = "passwordhere";
      PublicUrl = "http://www.loloirugs.com";
      LoginUrl = "http://www.loloirugs.com/Dealer/Login";
      UsesStaticFiles = true;
      DiscoveryNotes = "Discovery comes from public website. Details come from product pages. Pricing comes from static spreadsheet received via email.";

      // Details - public site
      // Stock - public site (just in/out I think)
      // Images - public site

      // Pricing - static spreadsheet
      // I had to rebuild this because the format they give to us is not great
    }
  }
}
