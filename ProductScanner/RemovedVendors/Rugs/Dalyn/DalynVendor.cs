using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Dalyn
{
    public class DalynVendor : RugVendor
    {
        public DalynVendor() : base(122, "Dalyn", "DA", StockCapabilities.None, 2.0M, 3.4M)
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
            DiscoveryNotes = "Uses one static spreadsheet for details, and another for pricing. Images on scanner.insidefabric";
            PublicUrl = "http://www.dalyn.com/";
            UsesStaticFiles = true;
            UsesIMAP = true;

            StaticFileVersion = 5;
            // box URL = https://www.dropbox.com/s/x4z19hcxaw9tc10/Dalyn%20Box.com%20Link.url?dl=0 

            // awesome - they have one giant spreadsheet with all details in their Box.com account
            // the price sheet came through email - is going to be a little annoying to match up
            // nothing on their public website at all
            // images are in their Box account, so we may need to put those on our own FTP like we do for some fabric vendors
        }
    }
}
