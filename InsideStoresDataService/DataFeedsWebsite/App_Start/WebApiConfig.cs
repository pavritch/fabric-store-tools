using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace DataFeedsWebsite
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // since this project was converted from MVC 3, 
            // the original standard routing is used instead
            // of the newer WebAPI.

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

        }
    }
}
