using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Sunbrella
{
    public class SunbrellaVendor : Vendor
    {
        public SunbrellaVendor() : base(72, "Sunbrella", StoreType.InsideFabric, "SU", StockCapabilities.None, 1.6M, 2.5M)
        {
            ProductGroups = new List<ProductGroup> { ProductGroup.Fabric };
            UsesStaticFiles = true;
            StaticFileVersion = 2;
            DiscoveryNotes = "Discovery and some details come from static spreadsheet. The rest of the data comes from product pages.";

            RunDiscontinuedPercentageCheck = false;
        }

        public override string GetOurPriceMarkupDescription()
        {
            return "Uses MAP";
        }
    }
}