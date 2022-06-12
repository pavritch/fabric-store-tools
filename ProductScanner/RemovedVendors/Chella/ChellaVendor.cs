using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Chella
{
    public class ChellaVendor : Vendor
    {
        public ChellaVendor() : base(85, "Chella", StoreType.InsideFabric, "CL")
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Trim, ProductGroup.Fabric };
            DiscoveryNotes = "Data comes from scraping public website and static file for pricing";
            UsesStaticFiles = true;
            IsFullyImplemented = false;
            DeveloperComments = "Issues with discovery that need to be looked at";

            PublicUrl = "http://www.chellatextiles.com/";
        }
    }
}