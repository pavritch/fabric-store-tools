using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class ManufacturerInformation
    {
        public int ManufacturerID { get; set; }
        public string Name { get; set; }
        public string WebsiteUrl { get; set; }
        public string Url;
    }

    public interface IProductDataCache
    {
        string Identity { get; }

        bool Populate(StoreKeys storeKey, string connectionString);

        // metrics

        DateTime? TimeWhenPopulationStarted { get; }
        DateTime? TimeWhenPopulationCompleted { get; }
        TimeSpan? TimeToPopulate { get; }

        // core data collections

        Dictionary<int, CacheProduct> Products { get; }
        Dictionary<int, List<int>> Categories { get; }
        Dictionary<int, List<int>> Manufacturers { get; }
        Dictionary<string, List<int>> SortedCategories { get; }
        Dictionary<string, List<int>> SortedManufacturers { get; }
        Dictionary<int, List<int>> ChildCategories { get; }
        Dictionary<int, string> AutoSuggestPhrases { get; }
        Dictionary<int, ManufacturerInformation> ManufacturerInfo { get;}

        List<int> DiscontinuedProducts { get; }
        List<int> MissingImagesProducts { get; }

        /// <summary>
        /// Caches which category has been identified as the related one for the given category root.
        /// </summary>
        /// <remarks>
        /// Dictionary["ParentCategoryID:ProductID", CategoryID]
        /// </remarks>
        Dictionary<string, int> RelatedCategories { get; }

        /// <summary>
        /// ProductIDs for products which this store is offering into the pool of 
        /// cross marketed products.
        /// </summary>
        List<int> FeaturedProducts { get; }

        /// <summary>
        /// Actual products available for sale.
        /// </summary>
        /// <remarks>
        /// For fabric, does not include swatches. For other stores, includes
        /// variants since they are normal products.
        /// </remarks>
        int ProductsForSaleCount { get; }

        /// <summary>
        /// Gets a reference to the respective store. 
        /// </summary>
        /// <remarks>
        /// The cache does not keep a hard ptr to the store. It keeps a key, 
        /// which can create the reference to the store on demand. Do not want
        /// to have a circular reference bacl to the store.
        /// </remarks>
        IWebStore Store { get; }

        /// <summary>
        /// ReOrder list of productID to have an appropriate spread across all vendors
        /// based on weighting factors.
        /// </summary>
        /// <remarks>
        /// Only intended to be used for sorting lists which are comprised of multiple manufacturers.
        /// </remarks>
        /// <param name="products"></param>
        /// <returns></returns>
        List<int> MakeOrderedProductListByManufacturerWeight(List<int> products);

        /// <summary>
        /// Find the productID to match up with a SKU.
        /// </summary>
        /// <param name="SKU"></param>
        /// <returns></returns>
        int? LookupProductIDFromSKU(string SKU);

        /// <summary>
        /// Return the product matching the given sku. Null if not found.
        /// </summary>
        /// <param name="SKU"></param>
        /// <returns></returns>
        CacheProduct LookupProduct(string SKU);

        /// <summary>
        /// Return the product matching the given productID. Null if not found.
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        CacheProduct LookupProduct(int productID);



    }
}