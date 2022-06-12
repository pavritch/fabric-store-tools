using System.Web;
using System.Web.Optimization;

namespace StoreCurator
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui.js"  // this is a reduced/custom version without widgets; needed by contextmenu.js
                        ));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/jquery-ui.css",
                      "~/Content/bootstrap.css",
                      "~/Content/jquery.gridster.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/contextmenu.css",
                      "~/Content/pager.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/libraries").Include(
                        "~/Scripts/Velocity.js",
                        "~/Scripts/imagesloaded.pkgd.js",
                        "~/Scripts/jquery.easing.min.js",
                        "~/Scripts/contextmenu.js",
                        "~/Scripts/jquery.gridster.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/components").Include(
                        "~/Scripts/filterComponent.js", "~/Scripts/productsComponent.js", "~/Scripts/detailsComponent.js"));

        }
    }
}
