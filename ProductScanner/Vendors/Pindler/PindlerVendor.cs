using System.Collections.Generic;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.StockChecks.DTOs;

namespace Pindler
{
    public class PindlerVendor : Vendor
    {
        public PindlerVendor() : base(51, "Pindler", StoreType.InsideFabric, "PD", StockCapabilities.InOrOutOfStock, 1.4M)
        {
            SwatchesEnabled = false;
            ProductGroups = new List<ProductGroup> { ProductGroup.Fabric, ProductGroup.Trim };
            DiscoveryNotes = "Discovery uses dynamically downloaded excel spreadsheet. Everything comes from the file.";

            PublicUrl = "http://pindler.com/";

            RunDiscontinuedPercentageCheck = false;
        }

        public override string GetOurPriceMarkupDescription()
        {
            return string.Format("Uses {0} for products with cost >= 20. Uses cost * 1.8 for products with cost < 20.", OurPriceMarkup);
        }
    }
}