


using System.Web;
using System.Web.Optimization;

namespace AdminSetup
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            //Scripts bundles
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"                        
                        , "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(                        
                        "~/Scripts/jquery-ui-{version}.js"
                        , "~/Scripts/jquery.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/libs").Include(
                      "~/Scripts/gridmvc.js"
                      , "~/Scripts/bootstrap-notify.js"
                      , "~/Scripts/nprogress.js"
                      , "~/Scripts/jquery.slimscroll.min.js"
                      , "~/Scripts/ProjLibFormatting.js"
                      , "~/Scripts/Notification.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            //CSS bundles

            bundles.Add(new StyleBundle("~/Content/bootstrapcss").Include(
                      "~/Content/bootstrap.css"
                      , "~/Content/bootstrap-theme.css"));

            bundles.Add(new StyleBundle("~/Content/libcss").Include(
                     "~/Content/nprogress.css"
                     , "~/Content/Gridmvc.css"
                     , "~/Content/Notification.css"
                     , "~/Content/gridmvc.datepicker.css"
                      , "~/Content/ProjLib.css"));

            bundles.Add(new StyleBundle("~/Content/icongridcss").Include(
                      "~/Content/IconGrid/component.css"
                      , "~/Content/IconGrid/default.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}
