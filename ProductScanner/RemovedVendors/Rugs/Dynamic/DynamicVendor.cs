using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace Dynamic
{
    public class DynamicVendor : RugVendor
    {
        public DynamicVendor() : base(124, "Dynamic", "DY")
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Rug };
            DiscoveryNotes = "Uses a static spreadsheet for details. Images on scanner.insidefabric";
            PublicUrl = "http://www.dynamicrugs.com/";
            UsesStaticFiles = true;
            StaticFileVersion = 2;
            UsesIMAP = true;

            RunDiscontinuedPercentageCheck = false;

            // they have one spreadsheet with all details
            // images are on our own FTP 
        }
    }
}
