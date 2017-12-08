using System.Web;
using System.Web.Optimization;

namespace MDashBoard
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"
                        , "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"
                        , "~/Scripts/jquery.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/libs").Include(
                    "~/Scripts/nprogress.js"
                    , "~/Scripts/jquery.slimscroll.min.js"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/clibs").Include(
           "~/Scripts/ProjLibFormatting.js"));


            bundles.Add(new ScriptBundle("~/bundles/dashboardjs").Include(
          "~/plugins/morris/morris.min.js",
          "~/plugins/fastclick/fastclick.js",
          "~/dist/js/app.min.js",
          "~/dist/js/pages/dashboard.js",
          "~/dist/js/demo.js"));


            bundles.Add(new StyleBundle("~/Content/libcss").Include(
                     "~/Content/nprogress.css"
                    ));

            bundles.Add(new StyleBundle("~/Content/bootstrapcss").Include(
                      "~/Content/bootstrap.css"
                      , "~/Content/bootstrap-theme.css"));

            bundles.Add(new StyleBundle("~/Content/jqueryuicss").Include(
                "~/Content/themes/base/menu.css"));

            bundles.Add(new StyleBundle("~/Content/clibcss").Include(
                    "~/Content/ProjLib.css"
                    ));

            bundles.Add(new StyleBundle("~/Content/dashboardcss").Include(
                   "~/Content/font-awesome.min.css",
                   "~/Content/ionicons.min.css",
                   "~/dist/css/AdminLTE.min.css",
                   "~/plugins/iCheck/flat/blue.css",
                   "~/plugins/morris/morris.css"
                   ));

            BundleTable.EnableOptimizations = true;
        }
    }
}
