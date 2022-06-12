using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Fabricut
{
  public class FabricutBaseVendor : Vendor
  {
    public FabricutBaseVendor() { StockCapabilities = StockCapabilities.ReportOnHand; }

    protected FabricutBaseVendor(int vendorId, string displayName, StoreType storeType, string skuPrefix)
        : base(vendorId, displayName, storeType, skuPrefix, StockCapabilities.ReportOnHand, 1.6M)
    {
      SwatchCost = 3M;
      IsClearanceSupported = true;
      UsesIMAP = true;
      DiscoveryNotes = "Data comes from excel spreadsheet downloaded from Fabricut's FTP (updated daily).";

      Username = "dddddd";
      Password = "ddddddd";
    }
  }

  public class FabricutVendor : FabricutBaseVendor
  {
    public FabricutVendor() : base(67, "Fabricut", StoreType.InsideFabric, "FC")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim, ProductGroup.Wallcovering };

      LoginUrl = "https://www.fabricut.com/login.php?status=logged_out";
      LoginUrl2 = "https://www.fabricut.com/login.php?returnurl=";
      PublicUrl = "https://www.fabricut.com/";
    }
  }

  public class SHarrisVendor : FabricutBaseVendor
  {
    public SHarrisVendor() : base(88, "S. Harris", StoreType.InsideFabric, "HA")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };

      LoginUrl = "https://www.sharris.com/login.php?status=logged_out";
      LoginUrl2 = "https://www.fabricut.com/login.php?returnurl=";
      PublicUrl = "https://www.sharris.com/";
    }
  }

  public class StroheimVendor : FabricutBaseVendor
  {
    public StroheimVendor() : base(69, "Stroheim", StoreType.InsideFabric, "SH")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim, ProductGroup.Wallcovering };

      LoginUrl = "https://www.stroheim.com/login.php?status=logged_out";
      LoginUrl2 = "https://www.fabricut.com/login.php?returnurl=";
      PublicUrl = "https://www.stroheim.com/";
    }
  }

  public class TrendVendor : FabricutBaseVendor
  {
    public TrendVendor() : base(70, "Trend", StoreType.InsideFabric, "TR")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };

      LoginUrl = "https://www.trend-fabrics.com/login.php?status=logged_out";
      LoginUrl2 = "https://www.fabricut.com/login.php?returnurl=";
      PublicUrl = "https://www.trend-fabrics.com/";
    }
  }

  public class VervainVendor : FabricutBaseVendor
  {
    public VervainVendor() : base(68, "Vervain", StoreType.InsideFabric, "VV")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };

      LoginUrl = "https://www.vervain.com/login.php?status=logged_out";
      LoginUrl2 = "https://www.fabricut.com/login.php?returnurl=";
      PublicUrl = "https://www.vervain.com/";
    }
  }
}