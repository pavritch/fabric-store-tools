using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Website.Controllers
{
    [AuthorizeAPI]
    [AsyncTimeout(60 * 1000)]
    public class ShareASaleController : Controller
    {
        // must return JsonNetResult() to take advantage of json.net serialization attributes

        [HttpGet]
        public async Task<JsonResult> DownloadFeed(int merchantID)
        {
            var data = await MvcApplication.Current.ShareASaleManager.DownloadDataFeed(merchantID);
            return new JsonNetResult(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetProducts(StoreKeys storeKey, int productID, int count=12)
        {
            // seed depends on store, product (or category, etc.) and which day of year, so changes every day.
            var seed = string.Format("{0}:{1}:{2}:{3}", storeKey.ToString(), productID, DateTime.Now.DayOfYear, DateTime.Now.Hour).ToSeed();

            var productList = MvcApplication.Current.ShareASaleManager.GetRandomProductList(count, seed);
            return new AffiliateProductListActionResult(productList);
        }

    }
}
