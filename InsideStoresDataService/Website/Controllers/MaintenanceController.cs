using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Website.Controllers
{
    public class MaintenanceController : Controller
    {
        //
        // GET: /Maintenance/

        public async Task<ActionResult> RepopulateProducts(StoreKeys storeKey)
        {
            Debug.WriteLine(string.Format("RepopulateProducts({0})", storeKey.ToString()));
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var bSuccess = await Task.Factory.StartNew<bool>(() =>
            {
                return store.RepopulateProducts();   
            });

            ViewBag.CompletedSuccessfully = bSuccess.ToString();
            ViewBag.TimeWhenPopulationCompleted = store.TimeWhenPopulationCompleted.ToString();
            ViewBag.TimeToPopulate = store.TimeToPopulate.ToString();

            return View();
        }


        public async Task<ActionResult> RebuildProductCategoryTable(StoreKeys storeKey)
        {
            Debug.WriteLine(string.Format("RebuildProductCategoryTable({0})", storeKey.ToString()));
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var bSuccess = await Task.Factory.StartNew<bool>(() =>
            {
                return store.RebuildProductCategoryTable();
            });

            ViewBag.StoreName = store.FriendlyName;
            ViewBag.CompletedSuccessfully = bSuccess.ToString();

            return View();
        }


        public async Task<ActionResult> RebuildProductSearchExtensionData(StoreKeys storeKey)
        {
            Debug.WriteLine(string.Format("RebuildProductSearchExtensionData({0})", storeKey.ToString()));
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var bSuccess = await Task.Factory.StartNew<bool>(() =>
            {
                return store.RebuildProductSearchExtensionData();
            });

            ViewBag.StoreName = store.FriendlyName;
            ViewBag.CompletedSuccessfully = bSuccess.ToString();

            return View();
        }


        public ActionResult GeneralStatus(StoreKeys storeKey)
        {
            Debug.WriteLine(string.Format("GeneralStatus({0})", storeKey.ToString()));
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var rec = store.GetStoreMaintenanceStatus();

            return View(rec);
        }


        public ActionResult CancelBackgroundTask(StoreKeys storeKey)
        {
            Debug.WriteLine(string.Format("CancelBackgroundTask({0})", storeKey.ToString()));
            var store = MvcApplication.Current.GetWebStore(storeKey);
            store.CancelBackgroundTask();

            var rec = store.GetStoreMaintenanceStatus();

            return View("GeneralStatus", rec);
        }



        public ActionResult RebuildAll(StoreKeys storeKey)
        {
            Debug.WriteLine(string.Format("RebuildAll({0})", storeKey.ToString()));
            var store = MvcApplication.Current.GetWebStore(storeKey);

            // this will fire up a separate task to complete and return immediataely.
            var isStartedNewTask = store.RebuildAll();
            ViewBag.Message = isStartedNewTask ? "Started new operation." : "Existing operation was already running. New operation has been queued.";
            return View();
        }

        public ActionResult RunActionForAllProducts(StoreKeys storeKey, string method, string tag=null)
        {
            string msg;

            if (!string.IsNullOrWhiteSpace(method))
            {
                Debug.WriteLine(string.Format("ForAllProducts({0})", storeKey.ToString()));
                var store = MvcApplication.Current.GetWebStore(storeKey);

                // this will fire up a separate task to complete and return immediataely.
                msg = store.RunActionForAllProducts(method, tag);
                ViewBag.ActionName = method;
            }
            else
            {
                msg = "Missing action.";
            }

            ViewBag.Message = msg;
            ViewBag.StoreKey = storeKey;
            return View();
        }


        public ActionResult RunActionForStore(StoreKeys storeKey, string method, string tag=null)
        {
            string msg;

            if (!string.IsNullOrWhiteSpace(method))
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                // this will fire up a separate task to complete and return immediataely.
                msg = store.RunActionForStore(method, tag);
                ViewBag.ActionName = method;
            }
            else
            {
                msg = "Missing action.";
            }

            ViewBag.Message = msg;
            ViewBag.StoreKey = storeKey;
            return View();
        }



        public ActionResult SpinQuery(StoreKeys storeKey)
        {
            Debug.WriteLine(string.Format("SpinQuery({0})", storeKey.ToString()));
            var store = MvcApplication.Current.GetWebStore(storeKey);

            //int completedCount = 0;

            var rand = new Random();
            var spinCount = rand.Next(3, 4);

            var arbitrary = rand.Next(100, 900);

            for (int i = 0; i < spinCount; i++)
            {
                //var query = new CategoryProductQuery(35)
                //{
                //    PageNo = 4,
                //    PageSize = 20,
                //    OrderBy = ProductSortOrder.Default,
                //    CompletedAction = (list, pageCount) =>
                //    {
                //        lock (this)
                //        {
                //            completedCount++;
                //            //if (completedCount == 100000)
                //            //    Debug.WriteLine("*** done list ****");
                //        }

                //    },
                //};

                //store.SubmitProductQuery(query);


                var query2 = new SearchProductQuery(string.Format("green {0} {1}", arbitrary, i))
                {
                    PageNo = 1,
                    PageSize = 10,
                    OrderBy = ProductSortOrder.Default,
                    CompletedAction = (list, pageCount) =>
                    {

                    },
                };

                store.SubmitProductQuery(query2);

                var criteria = new SearchCriteria()
                {
                    Keywords = string.Format("blue {0} {1}", arbitrary, i),
                    ManufacturerList = new List<int>(),
                    TypeList = new List<int>(),
                    PatternList = new List<int>(),
                    ColorList = new List<int>(),
                    PriceRangeList = new List<int>(),
                };

                var query3 = new AdvSearchProductQuery(criteria)
                {
                    PageNo = 1,
                    PageSize = 10,
                    OrderBy = ProductSortOrder.Default,
                    CompletedAction = (list, pageCount) =>
                    {
                    },
                };

                store.SubmitProductQuery(query3);



            }


            //int completedCount2 = 0;

            //for (int j = 0; j < 10000; j++)
            //{
            //    var query = new SearchProductQuery("red silk")
            //    {
            //        PageNo = 2,
            //        PageSize = 10,
            //        OrderBy = ProductSortOrder.Default,
            //        CompletedAction = (list, pageCount) =>
            //        {
            //            lock (this)
            //            {
            //                completedCount2++;
            //                if (completedCount2 == 10000)
            //                    Debug.WriteLine("*** done search ****");
            //            }

            //        },
            //    };

            //    store.SubmitProductQuery(query);
            //}


            return View();
        }


    }
}
