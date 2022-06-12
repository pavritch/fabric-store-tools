using System;
using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Surya
{
  // On Surya pricing, we’ll have to adhere to MAP which is 2x cost.
  // So for regular price, use MSRP (or  cost x 2.75)
  public class SuryaVendor : RugVendor
  {
    public SuryaVendor() : base(98, "Surya", "SY", StockCapabilities.None, 2.0M, 2.75M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Rug };

      Username = "tessa@example.com";
      Password = "passwordhere";
      LoginUrl = "https://www.surya.com/sign-in.aspx";
      PublicUrl = "https://www.surya.com";

      DiscoveryNotes = "Discovery and details are done using the public website. This also includes pricing and stock information.";
    }
  }
}