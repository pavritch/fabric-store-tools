using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace JamesHare
{
    public class JamesHareVendor : Vendor
    {
        public JamesHareVendor() : base(91, "James Hare", StoreType.InsideFabric, "JH")
        {
            IsFullyImplemented = false;
            DeveloperComments = "After website change the existing code stopped working and has not been revisited";
            ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };

            PublicUrl = "http://www.james-hare.com/";
        }
    }
}