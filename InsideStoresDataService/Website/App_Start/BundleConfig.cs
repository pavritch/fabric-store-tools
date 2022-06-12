using System.Web;
using System.Web.Optimization;

namespace Website
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //System.Web.Optimization.BundleTable.EnableOptimizations = false;
            bundles.UseCdn = true;   //enable CDN support

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include("~/Scripts/jquery.signalR-2.2.0.min.js"));
            // was throwing exception when had the verion tag.
            //bundles.Add(new ScriptBundle("~/bundles/signalr").Include("~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/Site.css"));

#if false // this is how it was built by default
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}
