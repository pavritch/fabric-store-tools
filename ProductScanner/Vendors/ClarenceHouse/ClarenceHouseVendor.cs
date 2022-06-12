using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace ClarenceHouse
{
  public class ClarenceHouseVendor : Vendor
  {
    public ClarenceHouseVendor() : base(63, "Clarence House", StoreType.InsideFabric, "CH", StockCapabilities.ReportOnHand)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Wallcovering, ProductGroup.Trim };
      DiscoveryNotes = "Data comes from PDF on Clarence House website that is converted to a CSV.";
      UsesStaticFiles = true;
      StaticFileVersion = 2;

      Username = "3423434234324324";
      Password = "passwordhere";
      LoginUrl = "http://customers.clarencehouse.com/login.asp?action=read";
      PublicUrl = "http://clarencehouse.com";
    }

    public override string GetOurPriceMarkupDescription()
    {
      return "When we have MAP, we use the MAP price. If not, we use our standard markup. Price is doubled on items with cost under $20.";
    }
  }
}