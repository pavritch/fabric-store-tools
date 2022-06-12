using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using InsideStores.Imaging;
using InsideFabric.Data;

namespace Website.Controllers
{
    [AuthorizeAPI]
    [AsyncTimeout(30 * 1000)]
    public class ProductSearchController : AsyncController
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

        #region Search

        [HttpGet]
        public void SearchAsync(StoreKeys storeKey, string query, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default, string recent=null)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);

            // the recent list is an optional list of productID which has been recently viewed by the user in case
            // some logic wishes to use it

            List<int> listRecentlyViewed = null;
            if (!string.IsNullOrWhiteSpace(recent))
                listRecentlyViewed = recent.Split(',').Select(e => int.Parse(e.Trim())).ToList();

            AsyncManager.OutstandingOperations.Increment();
            var productQuery = new SearchProductQuery(query, listRecentlyViewed)
            {
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(productQuery);
        }

        public ActionResult SearchCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion

        [HttpGet]
        public ActionResult ResizeUploadedPhotoAsSquare(StoreKeys storeKey, string guid, int size)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            var imageBytes = store.ResizeUploadedPhotoAsSquare(guid, size);
            // resized images are always jpg
            return File(imageBytes, "image/jpeg");
        }

        #region Facet Search

        /// <summary>
        /// Facet search introduced in Sept 2017 in a form that is common to all stores.
        /// </summary>
        /// <remarks>
        /// Made a decision to leave Advanced Search "as is" since old and not store agnostic. Better to start fresh.
        /// </remarks>
        /// <param name="storeKey"></param>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        [HttpPost]
        public void FacetSearchAsync(StoreKeys storeKey, FacetSearchCriteria criteria, int page = 1, int pageSize = 30, string visitor=null)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            // keep a log - writes async so won't delay search
            store.LogFacetSearch(criteria, page, visitor);

            criteria.SerialNumber = 1; // force, so caching of results will work better

            var productQuery = new FacetSearchProductQuery(criteria)
            {
                PageNo = page,
                PageSize = pageSize,
                OrderBy = ProductSortOrder.Default,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(productQuery);
        }

        public ActionResult FacetSearchCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion


        #region Search Gallery

        [HttpGet]
        public void SearchGalleryListAsync(StoreKeys storeKey, int galleryID, int page = 1, int pageSize = 30)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var criteria = new FacetSearchCriteria();

            using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
            {
                var json = dc.SearchGalleries.Where(e => e.SearchGalleryID == galleryID && e.Published == 1).Select(e => e.Query).FirstOrDefault();

#if false
                if (string.IsNullOrEmpty(json))
                    json = "{ \"SearchPhrase\": null, \"RecentlyViewed\": [], \"Facets\": [ { \"FacetKey\": \"Manufacturer\", \"Members\": [11] }, { \"FacetKey\": \"Color\", \"Members\": [39]}], \"SerialNumber\": 2}";
#endif                
                if (!string.IsNullOrEmpty(json))
                    criteria = json.FromJSON<FacetSearchCriteria>();
                    
            }

            criteria.SerialNumber = 1; // force, so caching of results will work better

            var productQuery = new FacetSearchProductQuery(criteria)
            {
                PageNo = page,
                PageSize = pageSize,
                OrderBy = ProductSortOrder.Default,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(productQuery);
        }

        public ActionResult SearchGalleryListCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion 


        #region Advanced Search

        [HttpGet]
        public void AdvSearchAsync(StoreKeys storeKey, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var criteria = new SearchCriteria(HttpContext);

            var productQuery = new AdvSearchProductQuery(criteria)
            {
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(productQuery);
        }

        public ActionResult AdvSearchCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion


        #region Image Search

        [HttpGet]
        public void ImageSearchAsync(StoreKeys storeKey, int productID, string mode, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            QueryRequestMethods method;

            switch((mode ?? string.Empty).ToLower())
            {
                case "color":
                    method = QueryRequestMethods.FindSimilarProductsByColor;
                    break;

                case "texture":
                    method = QueryRequestMethods.FindSimilarProductsByTexture;
                    break;

                case "colortexture":
                default:
                    method = QueryRequestMethods.FindSimilarProducts;
                    break;
            }

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var productQuery = new FindSimilarProductsQuery(productID, method)
            {
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(productQuery);
        }

        public ActionResult ImageSearchCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion


        #region Color Search

        [HttpGet]
        public void ColorSearchAsync(StoreKeys storeKey, string color, string mode, int page = 1, int pageSize = 20, ProductSortOrder orderBy = ProductSortOrder.Default)
        {
            QueryRequestMethods method;
            System.Windows.Media.Color mediaColor = color.ToColor();

            switch ((mode ?? string.Empty).ToLower())
            {
                case "any":
                    method = QueryRequestMethods.FindByAnyDominantColor;
                    break;

                case "top":
                default:
                    method = QueryRequestMethods.FindByTopDominantColor;
                    break;
            }

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();
            var productQuery = new FindProductsByDominantColorQuery(mediaColor, method)
            {
                PageNo = page,
                PageSize = pageSize,
                OrderBy = orderBy,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(productQuery);
        }

        public ActionResult ColorSearchCompleted(List<CacheProduct> productList, int pageCount)
        {
            return new ProductListActionResult(productList, pageCount);
        }

        #endregion




        #region Auto Suggest

        [HttpGet]
        public void AutoSuggestQueryAsync(StoreKeys storeKey, int listID, AutoSuggestMode mode, string query, int take=100)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var queryWorkItem = new AutoSuggestQuery()
            {
                Query = query,
                Mode = mode,
                ListID = listID,
                Take = take,
                CompletedAction = OperationCompleted2
            };

            store.SubmitAutoSuggestQuery(queryWorkItem);
        }

        private void OperationCompleted2(string query, List<string> autoSuggestPhrases)
        {
            AsyncManager.Sync(() =>
            {
                AsyncManager.Parameters.Add("autoSuggestPhrases", autoSuggestPhrases);
                AsyncManager.Parameters.Add("query", query);
                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        public JsonResult AutoSuggestQueryCompleted(string query, List<string> autoSuggestPhrases)
        {
            var jsonObj = new
            {
                query = query,
                suggestions = autoSuggestPhrases.ToArray(),
            };

            return Json(jsonObj, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region ProductCollection Auto Suggest

        [HttpGet]
        public void ProductCollectionAutoSuggestQueryAsync(StoreKeys storeKey, int collectionID, AutoSuggestMode mode, string query, int take = 100)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var queryWorkItem = new ProductCollectionAutoSuggestQuery()
            {
                Query = query,
                Mode = mode,
                CollectionID = collectionID,
                Take = take,
                CompletedAction = OperationCompleted2
            };

            store.SubmitAutoSuggestQuery(queryWorkItem);
        }

        public JsonResult ProductCollectionAutoSuggestQueryCompleted(string query, List<string> autoSuggestPhrases)
        {
            var jsonObj = new
            {
                query = query,
                suggestions = autoSuggestPhrases.ToArray(),
            };

            return Json(jsonObj, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region New Products Auto Suggest

        [HttpGet]
        public void NewProductsByManufacturerAutoSuggestQueryAsync(StoreKeys storeKey, int? manufacturerID, int days=120, AutoSuggestMode mode = AutoSuggestMode.Contains, string query =  null, int take = 100)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var queryWorkItem = new NewProductsByManufacturerAutoSuggestQuery()
            {
                Query = query,
                Mode = mode,
                ManufacturerID = manufacturerID,
                Days= days,
                Take = take,
                CompletedAction = OperationCompleted2
            };

            store.SubmitAutoSuggestQuery(queryWorkItem);
        }

        public JsonResult NewProductsByManufacturerAutoSuggestQueryCompleted(string query, List<string> autoSuggestPhrases)
        {
            var jsonObj = new
            {
                query = query,
                suggestions = autoSuggestPhrases.ToArray(),
            };

            return Json(jsonObj, JsonRequestBehavior.AllowGet);
        }

        #endregion


    }
}
