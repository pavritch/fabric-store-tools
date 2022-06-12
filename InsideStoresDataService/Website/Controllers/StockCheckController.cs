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
    public class StockCheckController : Controller
    {
        // must return JsonNetResult() to take advantage of json.net serialization attributes

        [HttpGet]
        public async Task<JsonResult> Vendors(StoreKeys storeKey)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var list = await store.StockCheckManager.GetStockCheckVendorInformation();
            return new JsonNetResult(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> SingleVariant(StoreKeys storeKey, int variantID, int quantity, string ip)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var stockStatus = await store.StockCheckManager.CheckStockForSingleVariant(variantID, quantity, ip);
            return new JsonNetResult(stockStatus, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> CheckStockForVariants(StoreKeys storeKey, StockCheckQuery[] variants, string ip)
        {
            var store = MvcApplication.Current.GetWebStore(storeKey);

            var stockStatus = await store.StockCheckManager.CheckStockForVariants(variants, ip);
            return new JsonNetResult(stockStatus, JsonRequestBehavior.DenyGet);
        }


        /// <summary>
        /// User wishes to be notified when more stock for this item is available.
        /// </summary>
        /// <param name="storeKey">which website</param>
        /// <param name="variantID">target item.</param>
        /// <param name="quantity">the quantity they had queried for.</param>
        /// <param name="email">notification email</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> NotifyRequest(StoreKeys storeKey, int variantID, int quantity, string email)
        {
            // GET: StoreApi/1.0/StockCheck/{storeKey}/notifyrequest/{variantID}/{quantity}&email={emailaddress}

            var store = MvcApplication.Current.GetWebStore(storeKey);

            var result = await store.StockCheckManager.NotifyRequest(variantID, quantity, email);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Sample of how to call in here for multiple variants in a single request.
        /// </summary>
        /// <remarks>
        /// Do not actually call this - as it's totally fake. Just a sample.
        /// </remarks>
        /// <returns></returns>
        private async Task<List<StockCheckResult>> SamplePost()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:60724/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var uri = "StoreApi/1.0/StockCheck/InsideFabric/Variants";

                    // prepair a list of variants you want to get status for

                    var variants = new List<StockCheckQuery>()
                    {
                        new StockCheckQuery()
                        {
                            VariantId = 2192908,
                            Quantity = 11,
                            ForceFetch = false,
                        },
                    };

                    HttpResponseMessage response = await client.PostAsJsonAsync(uri, variants);

                    response.EnsureSuccessStatusCode(); // throw exception if not success

                    // get the answer back

                    var listVariants = await response.Content.ReadAsAsync<List<StockCheckResult>>();

                    return listVariants;
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return new List<StockCheckResult>();
            }
        }
    }
}
