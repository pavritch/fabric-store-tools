using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.Pipeline;
using ProductScanner.Core.Scanning.Pipeline.Builder;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Commits
{
    public class CommitDataBuilder<T> : ICommitDataBuilder<T> where T : Vendor, new()
    {
        private readonly IVendorScanSessionManager<T> _sessionManager;
        private readonly IImageValidator<T> _imageValidator;
        private readonly IFullUpdateChecker<T> _fullUpdateChecker; 

        public CommitDataBuilder(IVendorScanSessionManager<T> sessionManager, IImageValidator<T> imageValidator, IFullUpdateChecker<T> fullUpdateChecker)
        {
            _sessionManager = sessionManager;
            _imageValidator = imageValidator;
            _fullUpdateChecker = fullUpdateChecker;
        }

        // duplicated in VendorVariant
        private StoreProductVariant FindMatchingSqlVariant(List<StoreProductVariant> sqlVariants, VendorVariant variant)
        {
            StoreProductVariant matchingSqlVariant;
            // checking for existing variant to do comparison
            if (variant.IsSwatch())
            {
                // we want to find the swatch variant
                matchingSqlVariant = sqlVariants.FirstOrDefault(x => x.ManufacturerPartNumber == variant.ManufacturerPartNumber && x.IsSwatch);
            }
            else
            {
                // we want to find the non-swatch variant
                matchingSqlVariant = sqlVariants.FirstOrDefault(x => x.ManufacturerPartNumber == variant.ManufacturerPartNumber && x.IsDefault);
                if (matchingSqlVariant == null)
                    matchingSqlVariant = sqlVariants.FirstOrDefault(x => x.ManufacturerPartNumber == variant.ManufacturerPartNumber);
            }
            return matchingSqlVariant;
        }

        private StoreProduct FindMatchingSqlProduct(List<StoreProduct> sqlProducts, VendorVariant variant)
        {
            // not sure of another way to do this, since there are differences in matching between rugs and fabric
            if (variant is FabricVendorVariant)
            {
                return sqlProducts.FirstOrDefault(x => x.Correlator == variant.ManufacturerPartNumber);
            }
            if (variant.VendorProduct.Correlator == null) return null;
            return sqlProducts.FirstOrDefault(x => x.Correlator == variant.VendorProduct.Correlator);
        }

        private List<AssociatedVariant> AssociateVariants(List<VendorVariant> vendorVariants, List<StoreProductVariant> sqlVariants)
        {
            // the key here needs to be something that uniquely identifies a variant and works across fabric/wallpaper + rugs
            var associatedVariants = new Dictionary<string, AssociatedVariant>();
            _sessionManager.ForEachNotify("Matching vendor variants to SQL variants", vendorVariants, vendorVariant =>
            {
                StoreProduct matchingSqlProduct = null;
                var matchingSqlVariant = FindMatchingSqlVariant(sqlVariants, vendorVariant);
                if (matchingSqlVariant == null)
                {
                    // if we didn't find a matching variant, see if we can find a matching product
                    matchingSqlProduct = FindMatchingSqlProduct(sqlVariants.Select(x => x.StoreProduct).ToList(), vendorVariant);
                }
                associatedVariants.Add(vendorVariant.GetUniqueKey(), new AssociatedVariant(vendorVariant, matchingSqlVariant, matchingSqlProduct));
            });

            _sessionManager.ForEachNotify("Matching SQL variants to vendor variants", sqlVariants, sqlVariant =>
            {
                var matchingVendorVariant = FindMatchingVendorVariant(vendorVariants, sqlVariant);
                if (!associatedVariants.ContainsKey(sqlVariant.GetUniqueKey()))
                    associatedVariants.Add(sqlVariant.GetUniqueKey(), new AssociatedVariant(matchingVendorVariant, sqlVariant));
            });
            return associatedVariants.Select(x => x.Value).ToList();
        }

        private List<int> FindDiscontinuedProducts(List<StoreProduct> sqlProducts, List<AssociatedVariant> removedVariants)
        {
            // check the actual IsDiscontinued flag from the database
            var nonDiscontinuedSqlProducts = sqlProducts.Where(x => !x.IsDiscontinued);

            var discontinuedProducts = new List<int>();
            // a product is considered discontinued if all of its variants are removed
            _sessionManager.ForEachNotify("Finding discontinued products", nonDiscontinuedSqlProducts, sqlProduct =>
            {
                var removedForThisProduct = removedVariants.Count(x => x.GetProductId() == sqlProduct.ProductID.Value);
                if (removedForThisProduct == sqlProduct.ProductVariants.Count) discontinuedProducts.Add(sqlProduct.ProductID.Value);
            });
            return discontinuedProducts;
        }

        private List<VendorProduct> FindNewProducts(List<AssociatedVariant> newVariants)
        {
            return newVariants.Where(x => x.SqlProduct == null)
                .Where(x => !x.IsDiscontinued())
                .DistinctBy(x => x.VendorVariant.VendorProduct)
                .Select(x => x.VendorVariant.VendorProduct).ToList();
        }

        private List<AssociatedVariant> FindNewVariantExistingProducts(List<AssociatedVariant> newVariants)
        {
            // where there's an associated SqlProduct, and it's not discontinued
            var variants = newVariants.Where(x => x.SqlProduct != null && !x.IsDiscontinued()).ToList();

            // no new variants should be default
            variants.ForEach(x => x.VendorVariant.IsDefault = false);
            return variants;
        }

        // if a specific variant is gone but product is still valid, issue a delete on the variant id
        private List<int> FindRemovedVariants(List<AssociatedVariant> removedVariants, List<int> discontinuedProducts)
        {
            var removedVariantIds = new List<int>();
            // check the actual IsDiscontinued flag from the database
            // if simulate is selected, I don't want to issue a bunch of remove variants because we think the product is still valid
            foreach (var removedVariant in removedVariants.Where(x => !x.IsSqlDiscontinued()))
            {
                if (!discontinuedProducts.Contains(removedVariant.GetProductId()))
                    removedVariantIds.Add(removedVariant.GetVariantId());
            }
            return removedVariantIds;
        }

        public CommitData Build(List<VendorVariant> vendorVariants, List<StoreProduct> sqlProducts)
        {
            // build up a data structure with vendor variants and their associated SQL Variant
            var sqlVariants = sqlProducts.SelectMany(x => x.ProductVariants).ToList();

            var associatedVariants = AssociateVariants(vendorVariants, sqlVariants);
            if (_sessionManager.HasFlag(ScanOptions.SimulateZeroDiscontinuedProducts))
                associatedVariants.ForEach(x => x.SimulateNotDiscontinued = true);

            var vendorReportedDiscontinued = associatedVariants.Where(x => x.IsVendorReportedDiscontinued() && x.CurrentlyExists()).ToList();
            var newVariants = associatedVariants.Where(x => x.IsNew() && !x.IsDiscontinued()).ToList();
            newVariants = RemoveDuplicates(newVariants, sqlProducts);

            var removedVariants = associatedVariants.Where(x => x.IsRemoved()).ToList();
            removedVariants.AddRange(vendorReportedDiscontinued);

            // if a product doesn't have variants (they were excluded), then we don't want it
            var newProducts = FindNewProducts(newVariants).Where(x => x.GetPopulatedVariants().Any());
            var newVariantsExistingProducts = FindNewVariantExistingProducts(newVariants);
            var newlyDiscontinuedProductIds = FindDiscontinuedProducts(sqlProducts, removedVariants);
            var promotedFullUpdates = associatedVariants.Where(x => x.SqlProduct != null && x.VendorVariant != null)
                .DistinctBy(x => x.VendorVariant.VendorProduct)
                .Where(x => !x.VendorVariant.VendorProduct.IsDiscontinued && _fullUpdateChecker.RequiresFullUpdate(x.VendorVariant.VendorProduct, x.SqlProduct))
                .ToList();

            var productsToImageValidate = new List<AssociatedVariant>();
            // always want to validate new products
            productsToImageValidate.AddRange(newVariants);
            productsToImageValidate.AddRange(promotedFullUpdates);

            if (_sessionManager.HasFlag(ScanOptions.SearchForMissingImages))
            {
                var productsMissingImage = associatedVariants.Where(x => x.IsProductMissingImage())
                    .Where(x => x.VendorVariant != null)
                    .DistinctBy(x => x.VendorVariant.VendorProduct);
                productsToImageValidate.AddRange(productsMissingImage);
            }
            else if (_sessionManager.HasFlag(ScanOptions.FullProductUpdate) && !_sessionManager.HasFlag(ScanOptions.SkipImagesOnFullUpdateRecords))
            {
                productsToImageValidate = associatedVariants.Where(x => x.VendorVariant != null).ToList();
            }
            _imageValidator.Validate(productsToImageValidate.Select(x => x.VendorVariant.VendorProduct).Distinct().ToList());

            var highestSku = 100;
            if (sqlProducts.Any())
                highestSku = sqlProducts.Select(x => x.SKU.Split(new[] {'-'}).Last().ToIntegerSafe()).Max();

            var commit = new CommitData();
            commit.Discontinued = newlyDiscontinuedProductIds;

            // take the skipped ones out here before we build the Stock batches
            associatedVariants = associatedVariants.Where(x => !x.IsSkipped()).ToList();

            commit.PriceChanges = associatedVariants.Where(x => x.HasPriceChange()).Where(x => !newlyDiscontinuedProductIds.Contains(x.GetProductId())).Select(x => x.GetPriceChange()).ToList();
            commit.OutOfStock = associatedVariants.Where(x => x.IsNowOutOfStock()).Where(x => !newlyDiscontinuedProductIds.Contains(x.GetProductId())).Select(x => x.SqlVariant.VariantID.Value).ToList();
            commit.InStock = associatedVariants.Where(x => x.IsNowInStock()).Where(x => !newlyDiscontinuedProductIds.Contains(x.GetProductId())).Select(x => x.SqlVariant.VariantID.Value).ToList();
            // only ever want to pass through image changes for ones that we've validated
            commit.UpdateImages = productsToImageValidate.Where(x => x.HasImageChange()).Where(x => !newlyDiscontinuedProductIds.Contains(x.GetProductId())).Select(x => x.GetImageChange()).ToList();
            commit.NewlyFoundImages = productsToImageValidate.Count(x => x.FoundMissingImage());
            commit.RemovedVariants = FindRemovedVariants(removedVariants, commit.Discontinued);
            commit.NewProducts = newProducts.Select(x => x.MakeNewStoreProduct(++highestSku)).Where(x => x != null).ToList();
            commit.UpdateProducts = GetFullUpdateBatch(associatedVariants, promotedFullUpdates);
            commit.NewVariantsExistingProducts = newVariantsExistingProducts.Select(x => new NewVariant(x.GetProductId(), x.VendorVariant.BuildNewStoreVariant())).ToList();
            commit.NewVariantsForReport = newProducts.SelectMany(x => x.GetPopulatedVariants()).ToList();

            return commit;
        }

        private List<StoreProduct> GetFullUpdateBatch(List<AssociatedVariant> associatedVariants, List<AssociatedVariant> promotedFullUpdates)
        {
            var skuChangeFlag = _sessionManager.HasFlag(ScanOptions.AllowSkuChangeOnUpdates);
            var skipImages = _sessionManager.HasFlag(ScanOptions.SkipImagesOnFullUpdateRecords);
            if (_sessionManager.HasFlag(ScanOptions.FullProductUpdate))
            {
                return associatedVariants.Where(x => x.SqlProduct != null && x.VendorVariant != null)
                    .DistinctBy(x => x.VendorVariant.VendorProduct)
                    .Where(x => !x.VendorVariant.VendorProduct.IsDiscontinued)
                    .Select(x => x.VendorVariant.VendorProduct.MakeUpdateStoreProduct(x.SqlProduct, skuChangeFlag, skipImages)).ToList();
            }
            // check for ones that should be promoted
            // always skip images for promoted full updates
            return promotedFullUpdates.Select(x => x.VendorVariant.VendorProduct.MakeUpdateStoreProduct(x.SqlProduct, skuChangeFlag, true)).ToList();
        }

        private List<AssociatedVariant> RemoveDuplicates(List<AssociatedVariant> associatedVariants, List<StoreProduct> sqlProducts)
        {
            var currentSqlSKUs = sqlProducts.Select(x => x.SKU).ToList();
            var newProductSKUs = associatedVariants.Select(x => x.GetSKU()).ToList();

            var uniqueVariants = new List<AssociatedVariant>();
            foreach (var newVariant in associatedVariants)
            {
                // if we already have this SKU in the db
                if (currentSqlSKUs.Contains(newVariant.GetSKU())) continue;

                // if we're trying to add two products with the same sku
                //if (newProductSKUs.Count(x => x == newVariant.GetSKU()) > 1) continue;
                uniqueVariants.Add(newVariant);
            }
            return uniqueVariants;
        }

        // these two functions won't work for rugs?
        // in the case of rugs, I just want to match on MPN right?
        private VendorVariant FindMatchingVendorVariant(List<VendorVariant> vendorVariants, StoreProductVariant sqlVariant)
        {
            VendorVariant matchingVendorVariant;
            // checking for existing variant to do comparison
            if (sqlVariant.IsSwatch)
            {
                // we want to find the swatch variant
                matchingVendorVariant = vendorVariants.SingleOrDefault(x => x.ManufacturerPartNumber == sqlVariant.ManufacturerPartNumber && x.IsSwatch());
            }
            else
            {
                // we want to find the non-swatch variant
                matchingVendorVariant = vendorVariants.SingleOrDefault(x => x.ManufacturerPartNumber == sqlVariant.ManufacturerPartNumber && x.IsDefault);
            }
            return matchingVendorVariant;
        }
    }
}