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
    public class CampaignsController : Controller
    {

        // GET: /Campaigns/Subscribers/{storekey}/{listKey}/{verb}?email={address}

        /// <summary>
        /// 
        /// </summary>
        /// <returns>JSON true or false</returns>
        public async Task<JsonResult> Subscribers(StoreKeys storeKey, string listKey, CampaignSubscriberListAction mode, string email)
        {
            try
            {
                var store = MvcApplication.Current.GetWebStore(storeKey);

                var result = await store.Subscribers(listKey, mode, email);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
