using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Kravet
{
  public class KravetBaseVendor : Vendor
  {
    public KravetBaseVendor(int id, string displayName, string skuPrefix)
        : base(id, displayName, StoreType.InsideFabric, skuPrefix, StockCapabilities.CheckForQuantity)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim, ProductGroup.Wallcovering };

      Username = "ddddd";
      Password = "dddd";
      PublicUrl = "http://www.kravet.com/products/fabrics/";

      HasStockCheckApi = true;
      IsClearanceSupported = true;
      DiscoveryNotes = "All data comes from downloaded file";

      MinimumCost = 4.94m;
    }
  }

  public class AndrewMartinVendor : KravetBaseVendor { public AndrewMartinVendor() : base(93, "Andrew Martin", "AM") { } }

  public class KravetVendor : KravetBaseVendor { public KravetVendor() : base(5, "Kravet", "KR") { } }

  public class LeeJofaVendor : KravetBaseVendor
  {
    public LeeJofaVendor() : base(8, "Lee Jofa", "LJ")
    {
      DeveloperComments = "Looks like some minor truncation issues with color name + pattern name, but not sure how we can address.";
    }
  }

  public class BakerLifestyleVendor : KravetBaseVendor { public BakerLifestyleVendor() : base(108, "Baker Lifestyle", "BL") { } }

  public class ColeAndSonVendor : KravetBaseVendor
  {
    public ColeAndSonVendor() : base(109, "Cole & Son", "CS")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Wallcovering };
    }
  }

  public class GPJBakerVendor : KravetBaseVendor { public GPJBakerVendor() : base(110, "G P & J Baker", "GP") { } }

  public class GroundworksVendor : KravetBaseVendor { public GroundworksVendor() : base(111, "Groundworks", "GW") { } }

  public class MulberryHomeVendor : KravetBaseVendor { public MulberryHomeVendor() : base(112, "Mulberry Home", "MB") { } }

  public class ParkertexVendor : KravetBaseVendor
  {
    public ParkertexVendor() : base(113, "Parkertex", "PT")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
    }
  }

  public class ThreadsVendor : KravetBaseVendor { public ThreadsVendor() : base(115, "Threads", "TH") { } }

  public class LauraAshleyVendor : KravetBaseVendor
  {
    public LauraAshleyVendor() : base(116, "Laura Ashley", "LA")
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
    }
  }

  public class BrunschwigAndFilsVendor : KravetBaseVendor { public BrunschwigAndFilsVendor() : base(119, "Brunschwig & Fils", "BF") { } }
}