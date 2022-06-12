using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Brewster
{
  public class BrewsterVendor : Vendor
  {
    public BrewsterVendor() : base(76, "Brewster", StoreType.InsideFabric, "BR", StockCapabilities.ReportOnHand)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Wallcovering };
      DiscoveryNotes = "Data comes from FTP site that contains spreadsheets and images";
      UsesStaticFiles = true;
      UsesIMAP = true;

      Username = "123423413244";
      Password = "124324324234";
      LoginUrl = "https://dealer.brewsterwallcovering.com/Login/SignIn";
      PublicUrl = "http://www.brewsterwallcovering.com/";

      StaticFileVersion = 2;
    }

    public override string GetOurPriceMarkupDescription()
    {
      return "If we have a MAP price, we use the higher of it or our standard formula. If not, we use our standard markup minus $1.";
    }
  }
}