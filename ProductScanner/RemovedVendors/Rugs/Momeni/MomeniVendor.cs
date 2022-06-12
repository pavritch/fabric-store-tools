using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Momeni
{
  public class MomeniVendor : RugVendor
  {
    public MomeniVendor() : base(104, "Momeni", "MO")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
      Username = "082343242348090";
      Password = "2342342342";
      LoginUrl = "http://mom-web.momeni.com/b2b_asp/VerifyLogin.vbhtml";
      PublicUrl = "http://mom-web.momeni.com/";

      DiscoveryNotes = "Uses static file for product details, FTP server for images. Queries the website backend for stock info";

      UsesStaticFiles = true;
      UsesIMAP = true;
    }
  }
}