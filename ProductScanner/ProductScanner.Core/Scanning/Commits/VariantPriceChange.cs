using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Commits
{
    public class VariantPriceChange
    {
        public int VariantId { get; set; }

        // Null means no change in clearance status/membership. Setting true or false instructs a specific change. 
        public bool? IsClearance { get; set; }

        public decimal Cost { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal OurPrice { get; set; }
        public decimal? SalePrice { get; set; }

        public decimal OldCost { get; set; }
        public decimal OldRetailPrice { get; set; }
        public decimal OldOurPrice { get; set; }
        public decimal? OldSalePrice { get; set; }

        // used by json deserialization
        public VariantPriceChange() { }

        public VariantPriceChange(StoreProductVariant matchingStoreVariant, VendorVariant vendorVariant, StoreProduct sqlProduct)
        {
            VariantId = matchingStoreVariant.VariantID.Value;

            if (vendorVariant.IsClearance != sqlProduct.IsClearance)
            {
                IsClearance = vendorVariant.IsClearance;
            }

            OldCost = matchingStoreVariant.Cost;
            OldRetailPrice = matchingStoreVariant.RetailPrice;
            OldOurPrice = matchingStoreVariant.OurPrice;
            OldSalePrice = matchingStoreVariant.SalePrice;

            Cost = vendorVariant.Cost;
            RetailPrice = vendorVariant.RetailPrice;
            OurPrice = vendorVariant.OurPrice;
            SalePrice = vendorVariant.SalePrice;
        }
    }
}