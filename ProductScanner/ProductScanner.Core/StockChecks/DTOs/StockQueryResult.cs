using System.Collections.Generic;
using System.Security.RightsManagement;
using ProductScanner.Core.Scanning.Products.Vendor;
namespace ProductScanner.Core.StockChecks.DTOs
{
    public class StockQueryResult
    {
        public int VendorId { get; set; }
        public int VariantId { get; set; }
        public string MPN { get; set; }
        public int CurrentStock { get; set; }
        public bool IsDiscontinued { get; set; }

        // used for some stock checks (Pindler)
        // come up with an easier way to do this - we shouldn't have this type of stuff in the stock checking
        public string ExtensionData4 { get; set; }

        public Dictionary<string, string> PublicProperties { get; set; }

        public int ProductID { get; set; }
        public bool HasSwatch { get; set; }
        public string ProductGroup { get; set; } 
    }
}