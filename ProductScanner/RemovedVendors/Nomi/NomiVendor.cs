using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Nomi
{
    public class NomiVendor : Vendor
    {
        public NomiVendor() : base(87, "Nomi", StoreType.InsideFabric, "NM")
        {
            UsesStaticFiles = true;
            ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };

            PublicUrl = "http://nomiinc.com/";
        }
    }
}