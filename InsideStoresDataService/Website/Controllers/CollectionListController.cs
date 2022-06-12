using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using Website.Entities;

namespace Website.Controllers
{
    [AuthorizeAPI]
    [AsyncTimeout(30 * 1000)]
    public class CollectionListController : AsyncController
    {

        /// <summary>
        /// Common completion method used by all entry points.
        /// </summary>
        /// <param name="productList"></param>
        /// <param name="pageCount"></param>
        private void OperationCompleted(List<ProductCollection> collectionList, int pageCount)
        {
            // need to sync since call to here could be made on a different thread

            AsyncManager.Sync(() =>
            {
                AsyncManager.Parameters.Add("collectionList", collectionList);
                AsyncManager.Parameters.Add("pageCount", pageCount);
                AsyncManager.OutstandingOperations.Decrement();
            });
        }


        #region Books by Manufacturer

        [HttpGet]
        public void BooksByManufacturerAsync(StoreKeys storeKey, int manufacturerID, string filter =  null, int page = 1, int pageSize = 20)
        {
            // optional filter constrains results to ContainsIgnoreCase() on short name

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var collectionQuery = new BooksCollectionQuery(manufacturerID, filter)
            {
                PageNo = page,
                PageSize = pageSize,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(collectionQuery);
        }

        public ActionResult BooksByManufacturerCompleted(List<ProductCollection> collectionList, int pageCount)
        {
            return new CollectionListActionResult(collectionList, pageCount);
        }

        #endregion


        #region Collections by Manufacturer

        [HttpGet]
        public void CollectionsByManufacturerAsync(StoreKeys storeKey, int manufacturerID, string filter, int page = 1, int pageSize = 20)
        {
            // optional filter constrains results to ContainsIgnoreCase() on short name

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var collectionQuery = new CollectionsCollectionQuery(manufacturerID, filter)
            {
                PageNo = page,
                PageSize = pageSize,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(collectionQuery);
        }

        public ActionResult CollectionsByManufacturerCompleted(List<ProductCollection> collectionList, int pageCount)
        {
            return new CollectionListActionResult(collectionList, pageCount);
        }

        #endregion


        #region Patterns by Manufacturer

        [HttpGet]
        public void PatternsByManufacturerAsync(StoreKeys storeKey, int manufacturerID, string filter = null, int page = 1, int pageSize = 20)
        {
            // optional filter constrains results to ContainsIgnoreCase() on short name

            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var collectionQuery = new PatternsCollectionQuery(manufacturerID, filter)
            {
                PageNo = page,
                PageSize = pageSize,
                CompletedAction = OperationCompleted,
            };

            store.SubmitProductQuery(collectionQuery);
        }

        public ActionResult PatternsByManufacturerCompleted(List<ProductCollection> collectionList, int pageCount)
        {
            return new CollectionListActionResult(collectionList, pageCount);
        }

        #endregion


        #region Auto Suggest

        [HttpGet]
        public void AutoSuggestQueryAsync(StoreKeys storeKey, CollectionsKind collectionsKind, int manufacturerID, AutoSuggestMode mode, string query, int take = 100)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);
            AsyncManager.OutstandingOperations.Increment();

            var queryWorkItem = new CollectionsAutoSuggestQuery()
            {
                Query = query,
                Mode = mode,
                Kind = collectionsKind,
                manufacturerID = manufacturerID,
                Take = take,
                CompletedAction = OperationCompleted2
            };

            store.SubmitAutoSuggestQuery(queryWorkItem);
        }

        private void OperationCompleted2(CollectionsKind collectionsKind, int manufacturerID, string query, List<string> autoSuggestPhrases)
        {
            AsyncManager.Sync(() =>
            {
                AsyncManager.Parameters.Add("autoSuggestPhrases", autoSuggestPhrases);
                AsyncManager.Parameters.Add("query", query);
                AsyncManager.Parameters.Add("collectionsKind", collectionsKind);
                AsyncManager.Parameters.Add("manufacturerID", manufacturerID);
                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        public JsonResult AutoSuggestQueryCompleted(CollectionsKind collectionsKind, int manufacturerID, string query, List<string> autoSuggestPhrases)
        {
            var jsonObj = new
            {
                collectionsKind = collectionsKind.ToString(),
                manufacturerID = manufacturerID,
                query = query,
                suggestions = autoSuggestPhrases.ToArray(),
            };

            return Json(jsonObj, JsonRequestBehavior.AllowGet);
        }

        #endregion


    }
}
