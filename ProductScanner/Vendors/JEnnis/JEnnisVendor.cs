using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace JEnnis
{
  public class JEnnisVendor : Vendor
  {
    public JEnnisVendor() : base(118, "J.Ennis", StoreType.InsideFabric, "JE")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };
      //DiscoveryNotes = "Discovery uses list page in account area. Details come from public site and back end queries.";

      Username = "sdddddd";
      Password = "dddd";
      LoginUrl = "https://www.jennisfabrics.com/jennis-web-core/goToSignIn.jef";
      LoginUrl2 = "https://www.jennisfabrics.com/jennis-web-core/signIn.jef";
      PublicUrl = "https://www.jennisfabrics.com/";

      RunDiscontinuedPercentageCheck = false;

      SwatchesEnabled = false;
    }
  }
}