using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace StoreCurator
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.IgnoreRoute("Images/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{*pathInfo}");
            routes.IgnoreRoute("Fonts/{*pathInfo}");


            routes.Add(new Route("Products/GetProductsByManufacturer/{manufacturerID}/{format}/{pageSize}/{pageNumber}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(new {controller = "Products", action = "GetProductsByManufacturer",}),
                Constraints = new RouteValueDictionary(new {format = new EnumRouteConstraint<DisplayFormats>()})
            });

            routes.Add(new Route("Products/GetProductsByCategory/{categoryID}/{format}/{pageSize}/{pageNumber}", new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(new { controller = "Products", action = "GetProductsByCategory", }),
                Constraints = new RouteValueDictionary(new { format = new EnumRouteConstraint<DisplayFormats>() })
            });

            routes.Add(new Route("Products/GetProductsByQuery/{format}/{pageSize}/{pageNumber}", new MvcRouteHandler())
            {
                // also requires query parameter to be passed in
                Defaults = new RouteValueDictionary(new { controller = "Products", action = "GetProductsByQuery", }),
                Constraints = new RouteValueDictionary(new { format = new EnumRouteConstraint<DisplayFormats>() })
            });


            routes.MapRoute(
                name: "Index",
                url: "Index",
                defaults: new { controller = "Home", action = "Index"}
            );

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );


            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
