using System.Collections.Generic;
using System.Linq;
using InsideFabric.Data;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;

namespace ProductScanner.Core.Scanning.Commits
{
    public class CommitData
    {
        // List of ProductIDs for those that are newly discontinued
        // Discontinued comes from either products that we know of that are no longer found on the vendor's site OR
        // products that the vendor has explicitly tagged as discontinued
        public List<int> Discontinued { get; set; }

        // List of VariantIDs for variants that are discontinued (but the product is still valid)
        public List<int> RemovedVariants { get; set; }

        // List of Variant IDs
        public List<int> InStock { get; set; }

        // List of Variant IDs
        public List<int> OutOfStock { get; set; }

        // List of new variants associated with existing products
        public List<NewVariant> NewVariantsExistingProducts { get; set; }
        public List<StoreProduct> NewProducts { get; set; }
        public List<VariantPriceChange> PriceChanges { get; set; }
        public List<ProductImageSet> UpdateImages { get; set; }
        public int NewlyFoundImages { get; set; }

        //public HashSet<string> DuplicateSKUs { get; set; }

        public List<StoreProduct> UpdateProducts { get; set; }

        // not used as a commit batch - used for report generation
        public List<VendorVariant> NewVariantsForReport { get; set; }

        public CommitData()
        {
            Discontinued = new List<int>();
            RemovedVariants = new List<int>();
            InStock = new List<int>();
            OutOfStock = new List<int>();
            NewVariantsExistingProducts = new List<NewVariant>();
            NewProducts = new List<StoreProduct>();
            PriceChanges = new List<VariantPriceChange>();
            UpdateImages = new List<ProductImageSet>();
            UpdateProducts = new List<StoreProduct>();
            //DuplicateSKUs = new HashSet<string>();

            NewVariantsForReport = new List<VendorVariant>();
        }

        public List<CommitBatchType> GetFilledBatches()
        {
            var filledBatches = new List<CommitBatchType>();
            if (Discontinued.Any()) filledBatches.Add(CommitBatchType.Discontinued);
            if (NewProducts.Any()) filledBatches.Add(CommitBatchType.NewProducts);
            if (InStock.Any()) filledBatches.Add(CommitBatchType.InStock);
            if (OutOfStock.Any()) filledBatches.Add(CommitBatchType.OutOfStock);
            if (UpdateImages.Any()) filledBatches.Add(CommitBatchType.Images);
            if (PriceChanges.Any()) filledBatches.Add(CommitBatchType.PriceUpdate);
            if (RemovedVariants.Any()) filledBatches.Add(CommitBatchType.RemovedVariants);
            if (NewVariantsExistingProducts.Any()) filledBatches.Add(CommitBatchType.NewVariants);
            return filledBatches;
        }
    }

    public class NewVariant
    {
        public NewVariant() { }
        public NewVariant(int productId, StoreProductVariant variant)
        {
            ProductId = productId;
            StoreProductVariant = variant;
        }

        public StoreProductVariant StoreProductVariant { get; set; }
        public int ProductId { get; set; }
    }

    public class ProductImageSet
    {
        public int ProductId { get; set; }
        public List<ProductImage> ProductImages { get; set; }

        public ProductImageSet() { }
        public ProductImageSet(int productId, List<ProductImage> productImages)
        {
            ProductId = productId;
            ProductImages = productImages;
        }
    }
}