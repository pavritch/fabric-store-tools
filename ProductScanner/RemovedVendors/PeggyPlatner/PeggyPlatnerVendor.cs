using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace PeggyPlatner
{
    public class PeggyPlatnerVendor : Vendor
    {
        public PeggyPlatnerVendor() : base(94, "Peggy Platner", StoreType.InsideFabric, "PP")
        {
            UsesStaticFiles = true;
            ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };

            PublicUrl = "http://www.peggyplatnercollection.com/";
        }
    }
}