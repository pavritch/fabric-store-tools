using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;

namespace Website.Controllers
{
    [AuthorizeAPI]
    [AsyncTimeout(30 * 1000)]
    public class ProductListController : AsyncController
    {

        /// <summary>
        /// Common completion method used by all entry points.
        /// </summary>
        /// <param name="productList"></param>
        /// <param name="pageCount"></param>
        private void OperationCompleted(List<CacheProduct> productList, int pageCount)
        {
            // need to sync since call to here could be made on a different thread

            AsyncManager.Sync(() =>
            {
                AsyncManager.Parameters.Add("productList", productList);
                AsyncManager.Parameters.Add("pageCount", pageCount);
                AsyncManager.OutstandingOperations.Decrement();
            });
        }


        #region Entity

        [HttpGet]
        public void ProductsByEntityAsync(StoreKeys storeKey, StoreEntityKeys entityName, int entityID, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            IProductQuery productQuery;

            if (entityName == StoreEntityKeys.Category)
            {
                productQuery = new CategoryProductQuery(entityID)
                {
                    PageNo = page,
                    PageSize = pageSize,
                    OrderBy = orderBy,
                    CompletedAction = OperationCompleted,
                };
            }
            else
            {
                productQuery = new ManufacturerProductQuery(entityID)
                {
                    PageNo = page,
                    PageSize = pageSize,
                    OrderBy = orderBy,
                    CompletedAction = OperationCompleted,
                };

            }

            store.SubmitProductQuery(productQuery);
        }

        public ActionResult ProductsByEntityCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }



        #endregion

        #region Category

        /// <summary>
        /// Returns all matches for the given category sorted as specified. No further filtering.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="categoryID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        [HttpGet]
        public void ProductsByCategoryAsync(StoreKeys storeKey, int categoryID, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new CategoryProductQuery(categoryID)
            {
                // non-filtered
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult ProductsByCategoryCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }


        /// <summary>
        /// Filters the matches for the category based on the specified filter lists.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="categoryID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        [HttpGet]
        public void ProductsByCategoryExAsync(StoreKeys storeKey, int categoryID, string filterByColor= null, string filterByPattern = null, string filterByType = null, string filterByBrand = null, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            // the categoryID represents the category of the page we are on...the other fields further filter the results.
            // the filters are optional, but if provided, behave exactly like advanced search

            // we do not know which kind of category page we're presently on (type, patter, color) and we do not want to waste time figuring it out here since
            // really doesn't matter provided that the category that we are in fact on appears in one of the provided lists when using filters

            if (filterByColor == null && filterByPattern == null && filterByType == null)
                throw new Exception("Missing filter criteria in call to ProductsByCategoryEx.");

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new CategoryProductQuery(categoryID)
            {
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,

                // extra fields for the Ex version

                IsFiltered = true,
                FilterByTypeList = filterByType.ParseIntList(),
                FilterByPatternList = filterByPattern.ParseIntList(),
                FilterByColorList = filterByColor.ParseIntList(),
                FilterByBrandList = filterByBrand.ParseIntList(),
            };

            // see if nothing passed in and should revert to simple non-filtered scenario
            // checking for LE 1 because no matter what, the current category must be represented in one of the filters, but if it's the
            // only criteria passed in, then it's a simple non-filtered scenario

            if (((query.FilterByTypeList.Count() + query.FilterByPatternList.Count() + query.FilterByColorList.Count()) <= 1) && query.FilterByBrandList.Count() == 0)
                query.IsFiltered = false;

            // safety check

            if (query.IsFiltered)
            {
                // one of the provided category filters must have a single item in it, which matches the base category for the current page

                // this way, when the query is converted into an advanced search, we are assured that the current category will be included
                // in the search no matter what other filter criteria are included.

                var assertList = new List<List<int>>()
                {
                    query.FilterByTypeList,
                    query.FilterByPatternList,
                    query.FilterByColorList,
                    // do not include brand list, brand not part of the test assertion
                };

                var count = assertList.Where(e => e.Count() == 1 && e[0] == query.CategoryID).Count();

                if (count != 1)
                    throw new Exception("Missing base category in filter criteria in call to ProductsByCategoryEx.");
            }

            store.SubmitProductQuery(query);
        }

        public ActionResult ProductsByCategoryExCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion


        #region Pattern

        /// <summary>
        /// Returns all products for a pattern using the pattern correlator.
        /// </summary>
        /// <remarks>
        /// Optionally excludes a productID - self.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <param name="categoryID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        [HttpGet]
        public void ProductsByPatternCorrelatorAsync(StoreKeys storeKey, string pattern, bool? SkipMissingImages, int? productID=null, int page = 1, int pageSize = 20)
        {
            // this feature intended to be used where we show a bunch of similar patterns (less self) under the big photos on the product details page

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new PatternCorrelatorProductQuery(pattern, SkipMissingImages ?? false, productID)
            {
                PageNo = page,
                PageSize = pageSize,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult ProductsByPatternCorrelatorCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }


        #endregion


        #region Product Collection

        /// <summary>
        /// Returns all products for collection by ID.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="collectionID"></param>
        [HttpGet]
        public void ProductsByCollectionAsync(StoreKeys storeKey, int collectionID, string filter=null, int page = 1, int pageSize = 20)
        {
            // optional filter constrains results to ContainsIgnoreCase() on short name

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new ProductCollectionProductQuery(collectionID, filter)
            {
                PageNo = page,
                PageSize = pageSize,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult ProductsByCollectionCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }


        #endregion


        #region New Products

        /// <summary>
        /// Returns all products for a manufacturer, or across all.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="collectionID"></param>
        [HttpGet]
        public void ListNewProductsByManufacturerAsync(StoreKeys storeKey, int? manufacturerID, int days = 120, string filter = null, int page = 1, int pageSize = 20)
        {
            // manufacturerID or 0 null for all.
            // filter does a ContainsIgnoreCase() on name

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new NewProductsByManufacturerProductQuery(manufacturerID, days, filter)
            {
                PageNo = page,
                PageSize = pageSize,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult ListNewProductsByManufacturerCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }


        #endregion


        #region Manufacturer

        /// <summary>
        /// Returns all matches for the given manufacturer - no additional filters.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        [HttpGet]
        public void ProductsByManufacturerAsync(StoreKeys storeKey, int manufacturerID, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new ManufacturerProductQuery(manufacturerID)
            {
                // non-filtered
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult ProductsByManufacturerCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }


        /// <summary>
        /// Returns matches for the given manufacturer, subject to the specified filter(s).
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        [HttpGet]
        public void ProductsByManufacturerExAsync(StoreKeys storeKey, int manufacturerID, string filterByColor = null, string filterByPattern = null, string filterByType = null, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            // the manufacturerID indicates the page we are on - and the other category lists further filter the results.
            // the filters are optional, but if provided, behave exactly like advanced search

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new ManufacturerProductQuery(manufacturerID)
            {
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,

                // extra fields for the Ex version

                IsFiltered = true,
                FilterByTypeList = filterByType.ParseIntList(),
                FilterByPatternList = filterByPattern.ParseIntList(),
                FilterByColorList = filterByColor.ParseIntList(),
            };

            // see if nothing passed in and should revert to simple non-filtered scenario
            if (query.FilterByTypeList.Count() == 0 && query.FilterByPatternList.Count() == 0 && query.FilterByColorList.Count() == 0)
                query.IsFiltered = false;

            store.SubmitProductQuery(query);
        }

        public ActionResult ProductsByManufacturerExCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }



        /// <summary>
        /// List all products for the manufacturer having the given value for the label.
        /// </summary>
        /// <remarks>
        /// Intended to find things like all Kravet products from Book 12345.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <param name="manufacturerID"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        [HttpGet]
        public void ProductsByManufacturerLabelValueAsync(StoreKeys storeKey, int manufacturerID, string label, string value, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new LabelProdutQuery(manufacturerID, label, value)
            {
                // non-filtered
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult ProductsByManufacturerLabelValueCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion


        #region Product Set

        [HttpGet]
        public void ProductSetAsync(StoreKeys storeKey, string products)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);

            // parse and create a safe list of ProductID int

            var productSetList = new List<int>();

            if (!string.IsNullOrEmpty(products))
            {
                foreach (var id in products.Split(','))
                {
                    var trimmedID = id.Trim();
                    if (string.IsNullOrEmpty(trimmedID))
                        continue;

                    int productID;
                    if (int.TryParse(trimmedID, out productID))
                        productSetList.Add(productID);
                }
            }

            AsyncManager.OutstandingOperations.Increment();

            var query = new ProductSetProductQuery(productSetList)
            {
                PageNo = 1,
                PageSize = Math.Max(1, productSetList.Count()),
                OrderBy = ProductSortOrder.Default,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult ProductSetCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion

        #region Related Products

        [HttpGet]
        public void RelatedProductsAsync(StoreKeys storeKey, int productID, int parentCategoryID, int count = 20)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new RelatedProductQuery(productID, parentCategoryID)
            {
                PageNo = 1,
                PageSize = count,
                OrderBy = ProductSortOrder.Default,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult RelatedProductsCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion

        #region Discontinued Products

        /// <summary>
        /// Returns all discontinued products for the store.
        /// </summary>
        [HttpGet]
        public void DiscontinuedProductsAsync(StoreKeys storeKey, int? manufacturerID=null,  int page = 1, int pageSize = 20)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new DiscontinuedProductQuery(null)
            {
                ManufacturerID = manufacturerID,
                PageNo = page,
                PageSize = pageSize,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult DiscontinuedProductsCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }
        #endregion


        #region Products Missing Images

        /// <summary>
        /// Returns all products missing images for the store.
        /// </summary>
        [HttpGet]
        public void MissingImagesProductsAsync(StoreKeys storeKey, int? manufacturerID = null, int page = 1, int pageSize = 20)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new MissingImagesProductQuery(null)
            {
                ManufacturerID = manufacturerID,
                PageNo = page,
                PageSize = pageSize,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult MissingImagesProductsCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }
        #endregion



        #region Cross Marketing Products

        [HttpGet]
        public void CrossMarketingProductsAsync(StoreKeys storeKey, string referenceIdentifier, int count = 10, bool allowResultsFromSelf=false)
        {
            // the reference identifier is simply some string/productID/Url/etc which is ultimately used to create
            // a digest and then into a seed for the random number generator. No fixed rules. On product pages could
            // be a productID. On category pages could be a catID, etc. Can even be a url or filename.

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var query = new CrossMarketingProductQuery(referenceIdentifier, allowResultsFromSelf)
            {
                PageNo = 1,
                PageSize = count,
                OrderBy = ProductSortOrder.Default,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(query);
        }

        public ActionResult CrossMarketingProductsCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion


    }
}
