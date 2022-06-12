using System;
using System.Collections.Generic;
using System.Text;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Capel
{
    public class CapelVendor : RugVendor
    {
        public CapelVendor() : base(101, "Capel", "CR")
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
            UsesStaticFiles = true;
            DiscoveryNotes = "Capel needs totally reworked to pull from the site primarily. We should be able to match with the spreadsheet on image urls if the spreadsheet has any data. May want to scrape the back end for stock instead.";

            PublicUrl = "http://www.capelrugs.com/";
        }
    }
}
