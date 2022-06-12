using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Commits
{
    public class AssociatedVariant
    {
        public VendorVariant VendorVariant { get; set; }
        public StoreProductVariant SqlVariant { get; set; }
        public StoreProduct SqlProduct { get; set; }
        public bool SimulateNotDiscontinued { get; set; }

        public AssociatedVariant(VendorVariant vendorVariant, StoreProductVariant sqlVariant)
        {
            VendorVariant = vendorVariant;
            SqlVariant = sqlVariant;
            if (sqlVariant != null) SqlProduct = sqlVariant.StoreProduct;
        }

        public AssociatedVariant(VendorVariant vendorVariant, StoreProductVariant sqlVariant, StoreProduct sqlProduct)
        {
            VendorVariant = vendorVariant;
            SqlVariant = sqlVariant;
            SqlProduct = sqlProduct;
            if (sqlVariant != null && sqlProduct == null) SqlProduct = sqlVariant.StoreProduct;
        }

        public int GetProductId()
        {
            if (SqlProduct != null) return SqlProduct.ProductID.Value;
            if (SqlVariant != null) return SqlVariant.ProductID.Value;
            return 0;
        }

        public bool IsProductMissingImage()
        {
            if (SqlProduct != null) return !IsDiscontinued() && SqlProduct.ImageFilename == null;
            return false;
        }

        public bool FoundMissingImage()
        {
            // if it was originally missing an image, but now has a valid image
            return IsProductMissingImage() && HasImageChange();
        }

        public int GetVariantId()
        {
            if (SqlVariant == null) return 0;
            return SqlVariant.VariantID.Value;
        }

        public bool IsRemoved()
        {
            // don't mark as removed if it's already part of a discontinued product
            // don't take SimulateNotDiscontinued flag into account for this
            // Innovations treats out of stock as discontinued
            if (SqlProduct != null && SqlProduct.VendorID == 107 && !SqlVariant.InStock) return false;
            if (SqlProduct != null && SqlProduct.IsDiscontinued) return false;
            return VendorVariant == null && SqlVariant != null;
        }

        public bool CurrentlyExists()
        {
            return SqlVariant != null;
        }

        // if Simulate Zero Discontinued is set, then we always return false here
        public bool IsDiscontinued()
        {
            if (SqlProduct != null)
            {
                if (SimulateNotDiscontinued) return false;
                if (SqlProduct.IsDiscontinued) return true;
            }
            if (VendorVariant != null && VendorVariant.VendorProduct != null)
            {
                return VendorVariant.VendorProduct.IsDiscontinued;
            }
            return false;
        }

        // the flag in the database for the associated sql product
        public bool IsSqlDiscontinued()
        {
            if (SqlProduct != null) return SqlProduct.IsDiscontinued;
            return false;
        }

        public string GetMPN()
        {
            if (VendorVariant != null) return VendorVariant.ManufacturerPartNumber;
            return SqlVariant.ManufacturerPartNumber;
        }

        public string GetSKU()
        {
            var product = VendorVariant.VendorProduct as FabricProduct;
            if (product != null)
                return product.SKU;
            return null;
        }

        public bool IsNew()
        {
            return VendorVariant != null && SqlVariant == null;
        }

        // don't want to issue variant change unless the variant exists in sql and from vendor data
        // also don't want to change variant if currently discontinued
        // also don't want to include in batch if RequiresFullUpdate
        private bool IssueVariantChange()
        {
            if (IsDiscontinued()) return false;
            //if (VendorVariant != null && VendorVariant.VendorProduct != null && VendorVariant.VendorProduct.RequiresFullUpdate) return false;
            return VendorVariant != null && SqlVariant != null;
        }

        public bool IsSkipped()
        {
            if (VendorVariant != null) return VendorVariant.IsSkipped;
            return false;
        }

        public bool HasPriceChange()
        {
            if (!IssueVariantChange()) return false;

            if (SqlVariant.Cost != VendorVariant.Cost)
                return true;

            if (SqlVariant.RetailPrice != VendorVariant.RetailPrice)
                return true;

            if (SqlVariant.OurPrice != VendorVariant.OurPrice)
                return true;
            return false;
        }

        public bool IsNowOutOfStock()
        {
            if (!IssueVariantChange()) return false;
            return SqlVariant.InStock && !VendorVariant.StockData.InStock;
        }

        public bool IsNowInStock()
        {
            if (!IssueVariantChange()) return false;
            return !SqlVariant.InStock && VendorVariant.StockData.InStock;
        }

        public bool HasImageChange()
        {
            if (!IssueVariantChange()) return false;
            var currentImageUrls = SqlProduct.ProductImages.Where(x => x.SourceUrl != null).Select(x => x.SourceUrl).ToList();
            if (!currentImageUrls.SequenceEqual(VendorVariant.VendorProduct.ScannedImages.Select(x => x.Url)))
            {
                return true;
            }
            return false;
        }

        public VariantPriceChange GetPriceChange()
        {
            return new VariantPriceChange(SqlVariant, VendorVariant, SqlProduct);
        }

        public ProductImageSet GetImageChange()
        {
            return new ProductImageSet(GetProductId(), VendorVariant.VendorProduct.GetProductImages(SqlProduct.SKU));
        }

        public bool IsVendorReportedDiscontinued()
        {
            if (VendorVariant != null && VendorVariant.VendorProduct != null) return VendorVariant.VendorProduct.IsDiscontinued;
            return false;
        }
    }
}