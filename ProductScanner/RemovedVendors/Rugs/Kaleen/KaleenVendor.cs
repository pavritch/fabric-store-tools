using System.Collections.Generic;
using System.Text;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Kaleen
{
    public class KaleenVendor : RugVendor
    {
        public KaleenVendor() : base(123, "Kaleen", "KL", StockCapabilities.None, 2.25M, 2.25M * 1.7M)
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
            PublicUrl = "http://kaleen.com";

            DiscoveryNotes = "FTP server for images. Discovery + prices via static file.";
            // not sure I even need to scrape the website?

            UsesStaticFiles = true;
            UsesIMAP = true;

            StaticFileVersion = 2;

            // Kaleen: Kaleen Price Increase 9.1.15
            // - this email has all info
            // - info on images and pricing

        }
    }
}
