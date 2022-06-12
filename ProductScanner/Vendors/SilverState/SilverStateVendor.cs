using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace SilverState
{
  public class SilverStateVendor : Vendor
  {
    public SilverStateVendor() : base(77, "Silver State", StoreType.InsideFabric, "SS", StockCapabilities.ReportOnHand)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
      IsStockCheckerFunctional = false;

      Username = "2323232";
      Password = "dfdfdfdfdfrd";
      LoginUrl = "https://www.silverstatetextiles.com/storefrontCommerce/login.do";
      LoginUrl2 = "https://www.silverstatetextiles.com/storefrontCommerce/home.do";
      PublicUrl = "http://www.silverstatetextiles.com/";

      DiscoveryNotes = "Discovery comes from public site after login. Additional data and pricing is pulled from product pages.";

      IsClearanceSupported = true;
    }
  }
}