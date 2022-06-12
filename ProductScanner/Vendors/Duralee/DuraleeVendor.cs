using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Duralee
{
  public class DuraleeVendor : Vendor
  {
    public DuraleeVendor() : base(11, "Duralee", StoreType.InsideFabric, "DL", StockCapabilities.ReportOnHand, 1.4M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
      DiscoveryNotes = "Discovery and most details come from JSON endpoints. Product pages are scraped for stock.";

      Username = "tessa@example.com";
      Password = "passwordhe";
      LoginUrl = "https://www.duralee.com/admin/code/iframe/login.aspx?r=1&p=https%3A%2F%2Fwww.duralee.com%2FMyDuraleeSignIn.htm%3Fs%3Dlogout";
      LoginUrl2 = "https://www.duralee.com/MyDuraleeSignIn.htm";
      PublicUrl = "https://www.duralee.com/";
    }

    public override string GetOurPriceMarkupDescription()
    {
      return string.Format("{0} minus $1.", OurPriceMarkup);
    }
  }

  public class BBergerVendor : Vendor
  {
    public BBergerVendor() : base(73, "BBerger", StoreType.InsideFabric, "BB", StockCapabilities.ReportOnHand)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
      DiscoveryNotes = "Discovery and most details come from JSON endpoints. Product pages are scraped for stock.";

      Username = "tessa@example.com";
      Password = "passwordhe";
      LoginUrl = "https://www.duralee.com/admin/code/iframe/login.aspx?r=1&p=https%3A%2F%2Fwww.duralee.com%2FMyDuraleeSignIn.htm%3Fs%3Dlogout";
      LoginUrl2 = "https://www.duralee.com/MyDuraleeSignIn.htm";
      PublicUrl = "https://www.duralee.com/";
    }
  }

  public class ClarkeAndClarkeVendor : Vendor
  {
    public ClarkeAndClarkeVendor() : base(80, "Clarke and Clarke", StoreType.InsideFabric, "CC", StockCapabilities.ReportOnHand, 1.4M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
      DiscoveryNotes = "Scrapes website for stock data because the stock endpoint for Duralee doesn't return data on C&C.";

      Username = "tessa@example.com";
      Password = "passwordhe";
      LoginUrl = "https://www.duralee.com/admin/code/iframe/login.aspx?r=1&p=https%3A%2F%2Fwww.duralee.com%2FMyDuraleeSignIn.htm%3Fs%3Dlogout";
      LoginUrl2 = "https://www.duralee.com/MyDuraleeSignIn.htm";
      PublicUrl = "https://www.duralee.com/";
    }

    public override string GetOurPriceMarkupDescription()
    {
      return string.Format("{0} minus $1.", OurPriceMarkup);
    }
  }

  public class HighlandCourtVendor : Vendor
  {
    public HighlandCourtVendor() : base(19, "Highland Court", StoreType.InsideFabric, "HC", StockCapabilities.ReportOnHand)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
      DiscoveryNotes = "Discovery and most details come from JSON endpoints. Product pages are scraped for stock.";

      Username = "tessa@example.com";
      Password = "passwordhe";
      LoginUrl = "https://www.duralee.com/admin/code/iframe/login.aspx?r=1&p=https%3A%2F%2Fwww.duralee.com%2FMyDuraleeSignIn.htm%3Fs%3Dlogout";
      LoginUrl2 = "https://www.duralee.com/MyDuraleeSignIn.htm";
      PublicUrl = "https://www.duralee.com/";
    }
  }
}