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
    public class TicklerCampaignsController : Controller
    {
        public class TicklerMetrics
        {
            public TicklerMetrics(IWebStore store)
            {
                Key = store.StoreKey;

                IsTicklerCampaignsEnabled = store.IsTicklerCampaignsEnabled;

                if (store.IsTicklerCampaignsEnabled)
                {
                    IsRunning = !store.IsTicklerCampaignQueueProcessingPaused;
                    CountActiveCampaigns = store.TicklerCampaignsManager.GetCampaignCount(TicklerCampaignStatus.Running);
                    CountStagedCampaigns = store.TicklerCampaignsManager.GetCampaignCount(TicklerCampaignStatus.Staged);
                    CountSuspendedCampaigns = store.TicklerCampaignsManager.GetCampaignCount(TicklerCampaignStatus.Suspended);
                }
                else
                {
                    IsRunning = false;
                    CountActiveCampaigns = 0;
                    CountStagedCampaigns = 0;
                    CountSuspendedCampaigns = 0;
                }
            }

            public StoreKeys Key { get; set; }
            public bool IsTicklerCampaignsEnabled { get; set; }
            public bool IsRunning { get; set; }
            public int CountActiveCampaigns { get; set; }
            public int CountStagedCampaigns { get; set; }
            public int CountSuspendedCampaigns { get; set; }

            static public Dictionary<StoreKeys, TicklerMetrics> GetMetrics()
            {
                // pass in a metrics object for each store.

                var model = new Dictionary<StoreKeys, Website.Controllers.TicklerCampaignsController.TicklerMetrics>();
                foreach (var store in MvcApplication.Current.WebStores.Values)
                {
                    var metrics = new TicklerCampaignsController.TicklerMetrics(store);
                    model.Add(metrics.Key, metrics);
                }
                return model;
            }
        }


        public ActionResult PauseTicklerCampaignProcessing(StoreKeys storeKey)
        {

            ViewBag.Heading = "Pause Tickler Campaign Processing";
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                store.PauseTicklerCampaignProcessing();
                ViewBag.Message = "Command completed.";
            }
            catch(Exception Ex)
            {
                ViewBag.Message = string.Format("Exception: {0}", Ex.Message);
            }

            return View("Status");
        }

        public ActionResult ResumeTicklerCampaignProcessing(StoreKeys storeKey)
        {

            ViewBag.Heading = "Resume Tickler Campaign Processing";
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                store.ResumeTicklerCampaignProcessing();
                ViewBag.Message = "Command completed.";
            }
            catch (Exception Ex)
            {
                ViewBag.Message = string.Format("Exception: {0}", Ex.Message);
            }

            return View("Status"); 
        }


        public ActionResult MoveStagedTicklerCampaignsToRunning(StoreKeys storeKey)
        {

            ViewBag.Heading = "Move Staged Tickler Campaigns To Running";
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                store.MoveStagedTicklerCampaignsToRunning();
                ViewBag.Message = "Command completed.";
            }
            catch (Exception Ex)
            {
                ViewBag.Message = string.Format("Exception: {0}", Ex.Message);
            }

            return View("Status");
        }


        public ActionResult DeleteStagedTicklerCampaigns(StoreKeys storeKey)
        {

            ViewBag.Heading = "Delete Staged Tickler Campaigns";
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                store.DeleteStagedTicklerCampaigns();
                ViewBag.Message = "Command completed.";
            }
            catch (Exception Ex)
            {
                ViewBag.Message = string.Format("Exception: {0}", Ex.Message);
            }

            return View("Status");
        }



        public ActionResult SuspendRunningTicklerCampaigns(StoreKeys storeKey)
        {

            ViewBag.Heading = "Suspend Running Tickler Campaigns";
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                store.SuspendRunningTicklerCampaigns();
                ViewBag.Message = "Command completed.";
            }
            catch (Exception Ex)
            {
                ViewBag.Message = string.Format("Exception: {0}", Ex.Message);
            }

            return View("Status");
        }



        public ActionResult ResumeSuspendedTicklerCampaigns(StoreKeys storeKey)
        {

            ViewBag.Heading = "Resume Suspended Tickler Campaigns";
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);
                store.ResumeSuspendedTicklerCampaigns();
                ViewBag.Message = "Command completed.";
            }
            catch (Exception Ex)
            {
                ViewBag.Message = string.Format("Exception: {0}", Ex.Message);
            }

            return View("Status");
        }



        // GET: /TicklerCampaigns/order/{storekey}/{OrderNumber}

        /// <summary>
        /// Notify about a new order placed on website.
        /// </summary>
        /// <remarks>
        /// All websites will transmit. Data service will decide what if anything to do.
        /// </remarks>
        /// <returns>JSON true or false</returns>
        public JsonResult NewOrderNotification(StoreKeys storeKey, int orderNumber)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);


#if DEBUG
                if (orderNumber == 0 && (store.StoreKey == StoreKeys.InsideWallpaper || store.StoreKey == StoreKeys.InsideFabric))
                {
                    // come up with a random swatch order
                    using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                    {
                        // variantName column is TEXT, so cannot do a compare in SQL directly
                        var allItems = dc.Orders_ShoppingCarts.Where(e => e.CreatedOn > DateTime.Parse("12/1/2016")).Select(e => new { e.OrderNumber, e.OrderedProductVariantName }).ToList();
                        allItems.RemoveAll(e => !string.Equals(e.OrderedProductVariantName, "Swatch", StringComparison.OrdinalIgnoreCase));
                        // get a distinct list of orders having at least one swatch, pick one at random
                        var orderNumbers = allItems.Select(e => e.OrderNumber).Distinct().ToList();
                        orderNumbers.Shuffle();
                        orderNumber = orderNumbers.FirstOrDefault();
                    }
                }
#endif

                store.NewOrderNotification(orderNumber);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


        // GET: /TicklerCampaigns/add/{kind}/{storekey}/{customerID}

        /// <summary>
        /// Add a new campaign. Called from ASPDNSF websites.
        /// </summary>
        /// <returns>JSON true or false</returns>
        public async Task<JsonResult> AddByCustomerID(StoreKeys storeKey, TicklerCampaignKind kind, int customerID)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                if (!store.IsTicklerCampaignsEnabled || store.TicklerCampaignsManager == null)
                    throw new Exception("Tickler camapaigns not enabled.");

#if DEBUG
                if (customerID == 0)
                {
                    using (var dc = new AspStoreDataContextReadOnly(store.ConnectionString))
                    {
                        customerID = dc.Customers.Where(e => e.Email == "pavritch@pcdynamics.com").Select(e => e.CustomerID).FirstOrDefault();
                    }
                }
#endif


                Debug.WriteLine(string.Format("Add tickler campaign for customer: {0}", customerID));

                bool result = false;

                switch(kind)
                {
                    case TicklerCampaignKind.AbandonedCart:
                        result = await store.TicklerCampaignsManager.CreateAbandonedCartCampaign(customerID);
                        break;

                    default:
                        throw new Exception("Unknown kind.");
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The caller (store) might want to show the email for feedback on the page before final submit. Called from ASPDNSF websites.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="kind"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public JsonResult GetUnsubscribeEmail(StoreKeys storeKey, string kind, string token)
        {
            try
            {
                // kind must be a known identifier: ac

                var store = MvcApplication.Current.GetWebStore(storeKey);

                if (!store.IsTicklerCampaignsEnabled || store.TicklerCampaignsManager == null)
                    throw new Exception("Tickler camapaigns not enabled.");

                switch (kind)
                {
                    case "ac":
                        var parsedToken = AbandonedCartTicklerCampaignHandler.ParseUnsubscribeToken(token, TicklerCampaignKind.AbandonedCart);
                        if (!parsedToken.IsValid)
                            throw new Exception("Invalid token.");
#if DEBUG
                        parsedToken.CustomerID = store.TicklerCampaignsManager.GetCustomerIDFromEmail("pavritch@pcdynamics.com").GetValueOrDefault();
#endif
                        var email = store.TicklerCampaignsManager.GetCustomerEmailAddress(parsedToken.CustomerID);

                        if (string.IsNullOrWhiteSpace(email))
                            throw new Exception("Invalid token.");

                        return Json(email, JsonRequestBehavior.AllowGet);


                    case "os":
                        var parsedToken2 = OnlySwatchesTicklerCampaignHandler.ParseUnsubscribeToken(token, TicklerCampaignKind.OnlySwatches);
                        if (!parsedToken2.IsValid)
                            throw new Exception("Invalid token.");
#if DEBUG
                        parsedToken2.CustomerID = store.TicklerCampaignsManager.GetCustomerIDFromEmail("pavritch@pcdynamics.com").GetValueOrDefault();
#endif
                        var email2 = store.TicklerCampaignsManager.GetCustomerEmailAddress(parsedToken2.CustomerID);

                        if (string.IsNullOrWhiteSpace(email2))
                            throw new Exception("Invalid token.");

                        return Json(email2, JsonRequestBehavior.AllowGet);

                    default:
                        throw new Exception("Unrecognized tickler campaign unsubscribe kind.");
                }

            }
            catch
            {
                return Json("Error: Invalid or unknown email.", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Unsubscribe from a specific campaign.  Called from ASPDNSF websites.
        /// </summary>
        /// <param name="storeKey"></param>
        /// <param name="kind"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public JsonResult Unsubscribe(StoreKeys storeKey, string kind, string token)
        {
            // kind:  must be a supported identifier relating to a campaign type.
            // "ac" for abandoned cart campaign.

            // this is not a valid cust/camp id, but the hash is correct (ac)
            //https://www.insidefabric.com/Unsubscribe.aspx?t=00000000010000000002708BCA3BF89E48953A2A9E41581B720D925D42B89BB6169AB4E2B440D4420676

            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                if (!store.IsTicklerCampaignsEnabled || store.TicklerCampaignsManager == null)
                    throw new Exception("Tickler camapaigns not enabled.");

                switch(kind)
                {
                    case "ac":
                        var parsedToken = AbandonedCartTicklerCampaignHandler.ParseUnsubscribeToken(token, TicklerCampaignKind.AbandonedCart);
                        if (!parsedToken.IsValid)
                            throw new Exception("Invalid token.");

                        Debug.WriteLine(string.Format("Unsubscribe tickler campaign: {0}", parsedToken.CampaignID));
#if DEBUG
                        parsedToken.CustomerID = store.TicklerCampaignsManager.GetCustomerIDFromEmail("pavritch@pcdynamics.com").GetValueOrDefault();
#endif

                        var result = store.TicklerCampaignsManager.UnsubscribeCampaign(parsedToken.CampaignID, parsedToken.CustomerID);

                        return Json(result, JsonRequestBehavior.AllowGet);


                    case "os":
                        var parsedToken2 = OnlySwatchesTicklerCampaignHandler.ParseUnsubscribeToken(token, TicklerCampaignKind.OnlySwatches);
                        if (!parsedToken2.IsValid)
                            throw new Exception("Invalid token.");

                        Debug.WriteLine(string.Format("Unsubscribe tickler campaign: {0}", parsedToken2.CampaignID));
#if DEBUG
                        parsedToken2.CustomerID = store.TicklerCampaignsManager.GetCustomerIDFromEmail("pavritch@pcdynamics.com").GetValueOrDefault();
#endif

                        var result2 = store.TicklerCampaignsManager.UnsubscribeCampaign(parsedToken2.CampaignID, parsedToken2.CustomerID);

                        return Json(result2, JsonRequestBehavior.AllowGet);



                    default:
                        throw new Exception("Unrecognized tickler campaign unsubscribe kind.");
                }

            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
