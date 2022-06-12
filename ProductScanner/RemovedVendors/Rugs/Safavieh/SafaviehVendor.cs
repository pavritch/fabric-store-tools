using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Safavieh
{
    public class SafaviehVendor : RugVendor
    {
        public SafaviehVendor() : base(100, "Safavieh", "SV", StockCapabilities.None, 2.15M, 4.0M)
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
            UsesStaticFiles = true;

            DiscoveryNotes = "All product data comes from static file. Image discovery is done via FTP server.";

            PublicUrl = "http://safavieh.com/";
        }
    }
}