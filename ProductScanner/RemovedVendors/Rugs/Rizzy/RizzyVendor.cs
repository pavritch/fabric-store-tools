using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Rizzy
{
  public class RizzyVendor : RugVendor
  {
    public RizzyVendor() : base(106, "Rizzy Home", "RH")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
      Username = "sales@example.com";
      Password = "Slaksd;asdlkjfK";
      LoginUrl = "http://www.rizzyhome.com/VerifyLoginAsk.vbhtml";
      PublicUrl = "http://rizzyhome.com/";

      DiscoveryNotes = "Pulls data from static spreadsheet. Scans public site for images and shipping info.";

      UsesStaticFiles = true;
      UsesIMAP = true;
      ThrottleInMs = 2500;
    }
  }
}
