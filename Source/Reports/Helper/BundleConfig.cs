using System.Web;
using System.Web.Optimization;

namespace Reports.Presentation
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-1.10.2.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryUI").Include("~/Scripts/jquery-ui-1.10.4.custom.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqgrid").Include("~/Scripts/jquery.jqGrid-4.5.4/js/i18n/grid.locale-en.js",
                   "~/Scripts/jquery.jqGrid-4.5.4/js/jquery.jqGrid.src.js"));

            bundles.Add(new ScriptBundle("~/Scripts/jqueryUploader").Include("~/Scripts/jquery.fileupload.js"));
            bundles.Add(new ScriptBundle("~/Scripts/jqueryUploaderIframe").Include("~/Scripts/jquery.iframe-transport.js"));

            bundles.Add(new ScriptBundle("~/bundles/jsapi").Include("~/Scripts/jsapi.JS"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));

           // bundles.Add(new ScriptBundle("~/bundles/JQSelecter").Include("~/Scripts/JQGRID.js"));

           //// bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need. 
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));




            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/bootstrap.css", "~/Content/Site.css", "~/Content/WrapBootstrap_files/bootstrap.min.css", "~/Content/jquery.fileupload-ui-noscript.css",
               "~/Content/jquery.fileupload-ui.css",
                      "~/Content/le-frog/jquery-ui-1.10.4.custom.css", "~/Scripts/jquery.jqGrid-4.5.4/src/css/ui.jqgrid.css"));  
              
        }
    }
}
