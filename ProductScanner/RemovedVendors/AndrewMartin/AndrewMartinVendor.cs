using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace AndrewMartin
{
    public class AndrewMartinVendor : Vendor
    {
        public AndrewMartinVendor() : base(93, "Andrew Martin", StoreType.InsideFabric, "AM")
        {
            UsesStaticFiles = true;
            ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Wallcovering };
            //DeveloperComments = "Pricing comes from static spreadsheet.";
            DiscoveryNotes = "Discovery and most details come from public site without login. Pricing comes from static spreadsheet. No stock data.";

            PublicUrl = "http://www.andrewmartin.co.uk/";

            PassesVendorQuantities = true;
        }
    }
}