using System.Web;
using System.Web.Optimization;

namespace Customize
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
                      "~/Scripts/select2.js"
                      , "~/Scripts/bootstrap-datepicker.js"
                      , "~/Scripts/bootstrap-notify.js"
                      , "~/Scripts/alertify.js"
                      , "~/Scripts/nprogress.js"
                      , "~/Scripts/gridmvc.js"
                      , "~/Scripts/jQuery.flashMessage.js"
                      , "~/Scripts/jquery.slimscroll.min.js"
                      , "~/Scripts/jquery.animsition.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/clibs").Include(
          "~/Scripts/FunctionForMultiSelect.js"
          , "~/Scripts/ProjLibFormatting.js"
          , "~/Scripts/CheckForDuplicateDocNo.js"
          , "~/Scripts/Notification.js"
           , "~/Scripts/ChargeCalculation.js"));

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
                     "~/Content/css/select2.css"
                     , "~/Content/nprogress.css"
                     , "~/Content/Gridmvc.css"
                     , "~/Content/gridmvc.datepicker.css"
                     ));

            bundles.Add(new StyleBundle("~/Content/alertifycss").Include(
                      "~/Content/alertifyjs/alertify.css"
                      , "~/Content/alertifyjs/themes/bootstrap.css"));

            bundles.Add(new StyleBundle("~/Content/jqueryuicss").Include(
                 "~/Content/jquery.ui.theme.css",
                 "~/Content/themes/base/autocomplete.css",
                 "~/Content/themes/base/menu.css",
                 "~/Content/animsition.min.css"));

            bundles.Add(new StyleBundle("~/Content/clibcss").Include(
                     "~/Content/ProjLib.css",
                     "~/Content/Notification.css",
                      "~/Content/IconCss.css"
                     ));

            BundleTable.EnableOptimizations = true;
        }
    }
}
