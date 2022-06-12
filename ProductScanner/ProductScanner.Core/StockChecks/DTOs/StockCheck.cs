using System.Collections.Generic;
using System.Security.RightsManagement;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products.Vendor;
using Utilities.Extensions;

namespace ProductScanner.Core.StockChecks.DTOs
{
    public class StockCheck
    {
        public int VariantId { get; set; }
        public float Quantity { get; set; }
        public bool ForceFetch { get; set; }

        // populated later
        public string MPN { get; set; }
        public Vendor Vendor { get; set; }
        public string DetailUrl { get; set; }

        public bool IsDiscontinued { get; set; }
        public int CurrentStock { get; set; }

        public int ProductId { get; set; }
        public ProductGroup ProductGroup { get; set; }
        public bool HasSwatch { get; set; }

        public Dictionary<string, string> PublicProperties { get; set; }

        public string GetPackaging()
        {
            var key = ProductPropertyType.Packaging.DescriptionAttr();
            if (!PublicProperties.ContainsKey(key)) return string.Empty;
            return PublicProperties[key];
        }
    }
}
