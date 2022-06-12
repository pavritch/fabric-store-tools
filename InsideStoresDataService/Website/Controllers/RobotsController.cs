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
    public class RobotsController : Controller
    {
        // must return JsonNetResult() to take advantage of json.net serialization attributes

        [HttpGet]
        public async Task<JsonResult> GetRobotData()
        {
            var data = await MvcApplication.Current.RobotManager.GetRobotData();
            return new JsonNetResult(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> AddRobot(string ip, string agent=null)
        {
            var result = await MvcApplication.Current.RobotManager.AddRobot(ip, agent);

            return new JsonNetResult(result, JsonRequestBehavior.AllowGet);
        }
    }
}
