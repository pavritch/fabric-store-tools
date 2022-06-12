using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Chandra
{
    public class ChandraVendor : RugVendor
    {
        // MAP is 2.25 * wholesale (from spreadsheet)
        public ChandraVendor() : base(105, "Chandra", "CD")
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
            UsesStaticFiles = true;
            UsesIMAP = true;
            DiscoveryNotes = "Uses a static file for product data, and downloads spreadsheet from google docs for inventory data.";
            PublicUrl = "http://www.shopchandra.com/";
        }
    }
}