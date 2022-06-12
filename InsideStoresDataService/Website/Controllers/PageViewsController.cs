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

namespace Website.Controllers
{
    public class PageViewsController : Controller
    {

        // GET: /ReportPageViews/{storekey}

        /// <summary>
        /// ASPDNSF websites report in their page view counts each second.
        /// This adds the specified counts to the current second accumulator.
        /// </summary>
        /// <returns>JSON true or false - generally ignored by caller.</returns>
        public JsonResult ReportPageViews(StoreKeys storeKey, int? Home, int? Manufacturer, int? Category, int? Product, int? Other, int? Bot, int? ResponseTimeHigh, int? ResponseTimeLow, int? ResponseTimeAvg, string ResponseTimes)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                List<int> colResponseTimes = new List<int>();

                if (!string.IsNullOrWhiteSpace(ResponseTimes))
                {
                    foreach (var item in ResponseTimes.Split(",", true).Where(e => !string.IsNullOrWhiteSpace(e)))
                    {
                        int value=0;
                        if (int.TryParse(item, out value))
                            colResponseTimes.Add(value);
                    }
                }

                var stats = new PageViewStats(colResponseTimes)
                {
                    Home = Home.GetValueOrDefault(),
                    Manufacturer = Manufacturer.GetValueOrDefault(),
                    Category = Category.GetValueOrDefault(),
                    Product = Product.GetValueOrDefault(),
                    Other = Other.GetValueOrDefault(),
                    Bot = Bot.GetValueOrDefault(),
                    ResponseTimeHigh = ResponseTimeHigh.GetValueOrDefault(),
                    ResponseTimeLow = ResponseTimeLow.GetValueOrDefault(),
                    ResponseTimeAvg = ResponseTimeAvg.GetValueOrDefault()
                };

                store.Performance.PageViews.Bump(stats);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

    }
}
