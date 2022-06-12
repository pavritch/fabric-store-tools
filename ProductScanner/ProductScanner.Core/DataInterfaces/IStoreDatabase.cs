using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Commits;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.StockChecks.DTOs;

namespace ProductScanner.Core.DataInterfaces
{

    /// <summary>
    /// Used to notate status without our old way of 0 or 999999.
    /// </summary>
    public enum InventoryStatus
    {
        Unknown,
        InStock,
        OutOfStock
    }

    /// <summary>
    /// Used to report progress back when pulling all products for a vendor from store SQL.
    /// </summary>
    public class RetrieveStoreProductsProgress
    {
        public int CountTotal { get; set; }
        public int CountCompleted { get; set; }
        public int CountRemaining { get; set; }
        public double PercentCompleted { get; set; }

        public RetrieveStoreProductsProgress()
        {

        }

        public RetrieveStoreProductsProgress(int countTotal, int countCompleted, int countRemaining, double percentCompleted)
        {
            CountTotal = countTotal;
            CountCompleted = countCompleted;
            CountRemaining = countRemaining;
            PercentCompleted = percentCompleted;
        }

    }

    /// <summary>
    /// Extra fields to be associated with commit data to enhance views in UX.
    /// </summary>
    public class ProductSupplementalData
    {
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string ProductGroup { get; set; }
        public string ImageUrl { get; set; }
        public string StoreUrl { get; set; }
        public string VendorUrl { get; set; }
    }

    /// <summary>
    /// Extra fields to be associated with commit data to enhance views in UX.
    /// </summary>
    public class VariantSupplementalData
    {
        public int VariantID { get; set; }
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string UnitOfMeasure { get; set; }
        public string ProductGroup { get; set; }
        public string ImageUrl { get; set; }
        public string StoreUrl { get; set; }
        public string VendorUrl { get; set; }
    }

    public class ProductCountMetrics
    {
        public int ProductCount { get; set; }
        public int ProductVariantCount { get; set; }
        public int DiscontinuedProductCount { get; set; }
        public int ClearanceProductCount { get; set; }
        public int InStockProductCount { get; set; }
        public int OutOfStockProductCount { get; set; }
        public int InStockProductVariantCount { get; set; }
        public int OutOfStockProductVariantCount { get; set; }
    }

    /// <summary>
    /// Primarily used by commit logic when updating the store database.
    /// </summary>
    public enum StoreDatabaseUpdateResult
    {
        [Description("Success")]
        Success,

        [Description("Not Found")]
        NotFound,

        [Description("Duplicate")]
        Duplicate,

        [Description("Invalid Data")]
        InvalidData,

        [Description("Not Allowed")]
        NotAllowed,

        [Description("Access Denied")]
        AccessDenied,

        // for any other general error

        [Description("Error")]
        Error,
    }

    public interface IStoreDatabase
    {
        Task<List<int>> GetVariantIds(int vendorId);

        Task<List<StoreProduct>> GetProductsAsync(int vendorId, CancellationToken cancelToken = default(CancellationToken), IProgress<RetrieveStoreProductsProgress> progress = null);

        /// <summary>
        /// Return a list of SKUs from only the Product record - not taking any variant component of the true SKUs into account.
        /// </summary>
        /// <remarks>
        /// For fabric/wallpaper, this would be the non-swatch SKU, but for something like rugs, it would only be the 
        /// first half of the SKU, so would not be all that meaningful.
        /// </remarks>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        Task<List<string>> GetProductSKUsAsync(int vendorId);

        /// <summary>
        /// Returns information for the set of variants which is used by stock check API logic
        /// to determine the appropriate actions to take when a query comes in.
        /// </summary>
        /// <param name="variantIds"></param>
        /// <returns></returns>
        Task<List<StockQueryResult>> GetStockCheckInfoAsync(IEnumerable<int> variantIds);


        /// <summary>
        /// Set the count of items in stock for the given variant.
        /// </summary>
        /// <remarks>
        /// For products such as fabric/wallpaper where there is a swatch, separate calls to this
        /// method are required for both variants.
        /// </remarks>
        /// <param name="variantId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult UpdateProductVariantInventory(int variantId, InventoryStatus status);

        /// <summary>
        /// For the given product, update the count for both the default variant for the product and
        /// its companion swatch - determined by having a variant with Name=Swatch.
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult UpdateProductWithSwatchInventory(int productId, InventoryStatus status);

        /// <summary>
        /// Remove (delete) and existing product variant.
        /// </summary>
        /// The corresponding row will be deleted from SQL. Frequently used for clearance fabric/wallpaper
        /// when we no longer allow swatches to be purchased.
        /// <param name="variantId"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult RemoveProductVariant(int variantId);


        /// <summary>
        /// Add a new product to SQL.
        /// </summary>
        /// <remarks>
        /// Includes all related variants and images.
        /// </remarks>
        /// <param name="product"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult AddProduct(StoreProduct product);

        /// <summary>
        /// Full refresh of product, including variants.
        /// </summary>
        /// <remarks>
        /// ProductID and VariantIDs remain the same - but everything else (except typically SKU) is up for grabs.
        /// </remarks>
        /// <param name="product"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult UpdateProduct(StoreProduct product);

        
        /// <summary>
        /// Add another variant to an existing product.
        /// </summary>
        /// <remarks>
        /// Variant must use same unit of measure as parent.
        /// </remarks>
        /// <param name="productId"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult AddProductVariant(int productId, StoreProductVariant variant);

        /// <summary>
        /// Full refresh of images associated with an existing product.
        /// </summary>
        /// <remarks>
        /// The image collection here completely replaces the existing collection. No merge.
        /// </remarks>
        /// <param name="productID"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult UpdateProductImages(int productID, List<ProductImage> images);


        /// <summary>
        /// Update the cost/price for an existing variant.
        /// </summary>
        /// <remarks>
        /// Also adjusts clearance status where needed.
        /// </remarks>
        /// <param name="price"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult UpdateProductVariantPrice(VariantPriceChange pricing);

        /// <summary>
        /// Returns a set of extended product data for the specified set of productIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 products per request.
        /// Should return list in same order as original IDs.
        /// </remarks>
        Task<List<ProductSupplementalData>> GetProductSupplementalDataAsync(List<int> products);

        /// <summary>
        /// Returns a set of extended product variant data for the specified set of variantIDs.
        /// </summary>
        /// <remarks>
        /// It is intended that the implementation of this method will break up the hits to SQL into
        /// reasonable chunks of no more than 100 variants per request. 
        /// Should return list in same order as original IDs.
        /// </remarks>
        Task<List<VariantSupplementalData>> GetVariantSupplementalDataAsync(List<int> products);

        /// <summary>
        /// Determine if the given vendor has an entry in the Manufacturer table.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        Task<bool> DoesVendorExistAsync(int vendorId);

        /// <summary>
        /// Determine if the store database exists at all.
        /// </summary>
        Task<bool> DoesStoreExistAsync();

        /// <summary>
        /// Return a set of product/variant counts for the given vendor.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        Task<ProductCountMetrics> GetProductCountMetricsAsync(int vendorId);

        /// <summary>
        /// Perform all business logic associated with marking a productID as discontinued.
        /// </summary>
        /// <remarks>
        /// All variants associated with product are no longer for sale. Remove from clearance too.
        /// </remarks>
        /// <param name="productID"></param>
        /// <returns></returns>
        StoreDatabaseUpdateResult MarkProductDiscontinued(int productID, int vendorId);

    }

    public interface IStoreDatabase<T> : IStoreDatabase where T : Store
    {
    }
}