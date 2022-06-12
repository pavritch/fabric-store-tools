using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Astek
{
    public class AstekVendor : Vendor
    {
        public AstekVendor() : base(95, "Astek", StoreType.InsideFabric, "AS")
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Wallcovering };
            DeveloperComments = "Needs a little more review - data is still somewhat ugly";

            PublicUrl = "http://www.designyourwall.com/";
        }
    }
}
