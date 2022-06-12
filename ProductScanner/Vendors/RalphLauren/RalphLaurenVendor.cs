using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace RalphLauren
{
  public class RalphLaurenVendor : Vendor
  {
    public RalphLaurenVendor() : base(52, "Ralph Lauren", StoreType.InsideFabric, "RL", StockCapabilities.ReportOnHand, 1.35M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Wallcovering, ProductGroup.Trim };
      DiscoveryNotes = "Discovery is done by doing a search for all products on the back end";

      Username = "dddd";
      Password = "ddddd";
      LoginUrl = "http://customers.folia-fabrics.com/login.asp?action=read";

      UsesStaticFiles = true;
      StaticFileVersion = 4;
    }

    public override string GetOurPriceMarkupDescription()
    {
      return string.Format("When MAP is available, uses MAP. Otherwise, uses {0} minus $1.", OurPriceMarkup);
    }
  }
}