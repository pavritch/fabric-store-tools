using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Jaipur
{
  public class JaipurVendor : RugVendor
  {
    public JaipurVendor() : base(96, "Jaipur", "JP", StockCapabilities.None, 1.6M, 1.6M * 1.7M)
    {
      ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
      Username = "sales@example.com";
      Password = "password";
      DiscoveryNotes = "Uses public website for discovery and product details. Uses FTP server for inventory feed and images.";
      LoginUrl = "https://www.jaipurliving.com/Login.aspx";
      PublicUrl = "https://www.jaipurliving.com/";
    }
  }
}
