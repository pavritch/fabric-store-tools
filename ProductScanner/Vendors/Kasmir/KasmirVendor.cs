using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Kasmir
{
  public class KasmirVendor : Vendor
  {
    public KasmirVendor() : base(58, "Kasmir", StoreType.InsideFabric, "KM", StockCapabilities.None, 1.5M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
      DiscoveryNotes = "Discovery comes from querying search page.";

      Username = "dddd";
      Password = "ddddd";
      LoginUrl = "http://www.kasmirfabricsonline.com/kasmirweb/login.aspx";
      PublicUrl = "http://www.kasmirfabricsonline.com/";

      StaticFileVersion = 8;
      UsesStaticFiles = true;

      RunDiscontinuedPercentageCheck = false;
    }

    public override string GetOurPriceMarkupDescription()
    {
      return string.Format("{0} minus $1", OurPriceMarkup);
    }
  }
}