using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace TheRugMarket
{
    public class TheRugMarketVendor : RugVendor
    {
        public TheRugMarketVendor() : base(99, "TheRugMarket", "TM")
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
            UsesStaticFiles = true;
            UsesIMAP = true;

            DiscoveryNotes = "Stock comes from their back end. Discovery and product data comes from the static file. Also scraping website for some additional metadata.";
            PublicUrl = "http://www.therugmarket.com/";

            StaticFileVersion = 5;
        }
    }
}