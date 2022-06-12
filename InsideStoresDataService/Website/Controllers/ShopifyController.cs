using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Threading;
using System.Configuration;
using System.IO;
using Gen4.Util.Misc;
using System.Threading.Tasks;

namespace Website.Controllers
{
    public class ShopifyController : Controller
    {
        public ActionResult Truncate(string tableName)
        {
            var store = MvcApplication.Current.ShopifyStore;

            switch(tableName)
            {
                case "All":
                    store.TruncateLog();
                    store.TruncateProductEvents();
                    store.TruncateProducts();
                    ViewBag.Message = "All tables truncated.";
                    break;

                case "Products":
                    store.TruncateProducts();
                    ViewBag.Message = "Product table truncated.";
                    break;

                case "Log":
                    store.TruncateLog();
                    ViewBag.Message = "Log table truncated.";
                    break;

                case "ProductEvents":
                    store.TruncateProductEvents();
                    ViewBag.Message = "ProductEvents table truncated.";
                    break;

                case "LiveShopifyProducts":
                    store.TruncateLiveShopifyProducts();
                    ViewBag.Message = "LiveShopifyProducts table truncated.";
                    break;

                default:
                    ViewBag.Message = "Invalid truncate request.";
                    break;
            }

            return View("SimpleResult");
        }


        public ActionResult ListRunningLongOperations()
        {
            var store = MvcApplication.Current.ShopifyStore;

            if (store.LongOperations.Count() == 0)
            {
                ViewBag.Message = "No running operations.";
                return View("SimpleResult");
            }
            return View("ListLongOperations", store.LongOperations);
        }

        public ActionResult ShowLongOperation(string key)
        {
            var store = MvcApplication.Current.ShopifyStore;
            var longOp = store.LongOperations.Where(e => e.Key == key).LastOrDefault();

            if (longOp == null)
            {
                ViewBag.Message = "Operation not found.";
                return View("SimpleResult");
            }

            return View("ShowLongOperation", longOp);
        }

        public ActionResult CancelLongOperation(string key)
        {
            var store = MvcApplication.Current.ShopifyStore;

            var longOp = store.LongOperations.Where(e => e.Key == key && e.IsRunning).LastOrDefault();

            if (longOp == null)
            {
                ViewBag.Message = string.Format("Running operation not found with key {0}.", key);
                return View("SimpleResult");
            }

            longOp.Cancel();

            return View("ShowLongOperation", longOp);
        }


        public ActionResult EnqueueVirginReadProducts()
        {
            const string operationKey = "EnqueueVirginReadProducts";

            var store = MvcApplication.Current.ShopifyStore;
            store.LongOperations.RemoveAll(e => e.Key == operationKey && !e.IsRunning);

            // make sure table empty since this is a virgin read
            using (var dc = new ShopifyDataContext(store.ConnectionString))
            {
                if (dc.Products.Count() > 0)
                {
                    ViewBag.Message = "Operation rejected. Products table must be empty.";
                    return View("SimpleResult");
                }
            }

            // do not start another operation if one is running
            if (store.LongOperations.Where(e => e.Key == operationKey && e.IsRunning).Count() == 0)
                store.EnqueueVirginReadProducts();

            var longOp = store.LongOperations.Where(e => e.Key == operationKey).FirstOrDefault();

            if (longOp == null)
            {
                ViewBag.Message = "Operation not found.";
                return View("SimpleResult");
            }

            return View("ShowLongOperation", longOp);
        }

        public ActionResult PullShopifyProductList(bool truncate)
        {
            const string operationKey = "PopulateFromLiveStore";

            var store = MvcApplication.Current.ShopifyStore;
            store.LongOperations.RemoveAll(e => e.Key == operationKey && !e.IsRunning);

            // do not start another operation if one is running
            if (store.LongOperations.Where(e => e.Key == operationKey && e.IsRunning).Count() == 0)
                store.PopulateFromLiveProducts(truncate);

            var longOp = store.LongOperations.Where(e => e.Key == operationKey).FirstOrDefault();


            if (longOp == null)
            {
                ViewBag.Message = "Operation not found.";
                return View("SimpleResult");
            }

            return View("ShowLongOperation", longOp);
        }
    }
}
