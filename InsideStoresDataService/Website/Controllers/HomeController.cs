using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // so we know to show a message
            ViewBag.IsSomeStoresMissing = MvcApplication.Current.IsSomeStoresMissing;
            ViewBag.IsAllStoresPopulated = MvcApplication.Current.IsAllStoresPopulated;

            return View();
        }

        public ActionResult Maintenance()
        {
            return View();
        }

        public ActionResult TicklerCampaigns()
        {

            return View(TicklerCampaignsController.TicklerMetrics.GetMetrics());
        }

        public ActionResult Test()
        {
            return View();
        }

        public ActionResult ShopifyProducts()
        {
            return View();
        }


        public ActionResult TestWallpaper()
        {
            return View();
        }

        public ActionResult TestRugs()
        {
            return View();
        }

    }
}
