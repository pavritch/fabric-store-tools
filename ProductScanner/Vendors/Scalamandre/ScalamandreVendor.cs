using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Scalamandre
{
  public class ScalamandreVendor : Vendor
  {
    public ScalamandreVendor() : base(59, "Scalamandre", StoreType.InsideFabric, "SC", StockCapabilities.ReportOnHand, 1.4M)
    {
      SwatchCost = 2M;
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Wallcovering, ProductGroup.Trim };
      DiscoveryNotes = "Discovery works by searching for all products";

      MinimumCost = 7.5M;

      RunDiscontinuedPercentageCheck = false;

      Username = "dddd";
      Password = "dddd";
      LoginUrl = "http://visualaccess.scalamandre.com/wwiz.asp?wwizmstr=OE.LOGIN2";
      PublicUrl = "http://scalamandre.com";
    }

    /*
    public override string GetOurPriceMarkupDescription()
    {
        return string.Format("{0} minus $1.", OurPriceMarkup);
    }
    */
  }
}