using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace InnovationsUsa
{
  public class InnovationsUsaVendor : Vendor
  {
    public InnovationsUsaVendor() : base(107, "Innovations", StoreType.InsideFabric, "IN", StockCapabilities.None, 1.45M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Wallcovering };
      DiscoveryNotes = "Discovery uses list page in account area. Details come from public site and back end queries.";

      UsesStaticFiles = true;
      StaticFileVersion = 3;

      Username = "dddd";
      Password = "ddddd";
      LoginUrl = "https://order-track.com/index.php?option=com_comprofiler&task=login";
      PublicUrl = "http://www.innovationsusa.com/";
    }
  }
}