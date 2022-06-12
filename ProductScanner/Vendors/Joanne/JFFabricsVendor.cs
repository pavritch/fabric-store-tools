using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace JFFabrics
{
  public class JFFabricsVendor : Vendor
  {
    public JFFabricsVendor() : base(97, "JF Fabrics", StoreType.InsideFabric, "JF", StockCapabilities.InOrOutOfStock, 2M, 2.6M)
    {
      UsesStaticFiles = true;
      StaticFileVersion = 6;

      RunDiscontinuedPercentageCheck = false;

      UsesIMAP = true;
      DiscoveryNotes = "Product details from JFFabrics spreadsheet. Created spreadsheet manually based on Retail price list PDF. We get the costs (except for wallpaper) via the stock check, but we need retail pricing since \"This is the retail price we have to show as prices.\"";
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Wallcovering };

      Username = "dddd";
      Password = "ddd";
      LoginUrl = "http://67.211.122.138:450/rpgsp/JF_E_LOGON.pgm?USERID={0}&PASSWD={1}";
      PublicUrl = "https://www.jffabrics.com";
    }
  }
}