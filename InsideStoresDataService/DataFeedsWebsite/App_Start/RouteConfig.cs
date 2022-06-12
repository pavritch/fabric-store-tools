using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DataFeedsWebsite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*allsvc}", new { allsvc = @".*\.svc(/.*)?" });

            #region Feeds

            // download feeds
            //routes.MapRoute("downloads", "ProductFeeds/{storeKey}/{feedKey}/{feedFile}", 
            //       new
            //        {
            //            controller = "Home",
            //            action = "DownloadProductFeed"
            //        }
            //);


            routes.MapRoute("downloads", "ProductFeeds/{storeKey}/{feedKey}/{feedFile}",
                   new
                   {
                       controller = "Home",
                       action = "DownloadProductFeed",
                       //storeKey = "InsideFabric",
                       //feedKey = "Google",
                       //feedFile = "MyFile.txt",
                   }
            );




            // menu page
            routes.MapRoute("feeds", "Feeds",
                   new
                   {
                       controller = "Home",
                       action = "Feeds"
                   }
            );

            // trigger full regeneration of all product feeds - needs password passed in
            routes.MapRoute("regenerate", "RegenerateAllFeeds",
                   new
                   {
                       controller = "Home",
                       action = "RegenerateAllFeeds"
                   }
            );


            #endregion

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }
    }
}