using System;
using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace FSchumacher
{
  public class FSchumacherVendor : Vendor
  {
    public FSchumacherVendor() : base(30, "Schumacher", StoreType.InsideFabric, "FS", StockCapabilities.ReportOnHand, 1.4M, 1.4M * 2M)
    {
      DiscoveryNotes = "Using static excel file that combines the three sheets in the excel doc on FSchumacher's website";
      ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim, ProductGroup.Wallcovering };

      Username = "ddddddda@ddddd.com";
      Password = "dddddd";
      LoginUrl = "https://www.fschumacher.com/account/login";
      PublicUrl = "http://www.fschumacher.com/";
    }
  }
}