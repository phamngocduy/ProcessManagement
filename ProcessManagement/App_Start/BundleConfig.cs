using ProcessManagement.App_Start;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Optimization;

namespace ProcessManagement
{
    public class BundleConfig
    {
      
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            Bundle cssBundle = new StyleBundle("~/Content/css")
                .Include("~/Content/vendor/bootstrap/css/bootstrap.min.css")
                .Include("~/Content/vendor/toastr/toastr.min.css")
                .Include("~/Content/vendor/jquery-confirm/dist/jquery-confirm.min.css")
                .Include("~/Content/vendor/jquery-datatable/dataTables.bootstrap4.min.css")
                .Include("~/Content/build/css/color_skins.css")
                .Include("~/Content/build/css/main.css")
                .Include("~/Content/build/css/custom.css");

            cssBundle.Orderer = new PassthruBundleOrderer();
            bundles.Add(cssBundle);

            Bundle scriptBundle = new ScriptBundle("~/Content/js")
                .Include("~/Content/build/bundles/libscripts.bundle.js")
                .Include("~/Content/build/bundles/vendorscripts.bundle.js")
                .Include("~/Content/vendor/toastr/toastr.js")
                .Include("~/Content/vendor/jquery-confirm/dist/jquery-confirm.min.js")
                .Include("~/Content/build/bundles/mainscripts.bundle.js")
                .Include("~/Content/build/bundles/datatablescripts.bundle.js")
                .Include("~/Content/build/js/button-ripple.js")
                .Include("~/Content/build/js/custom.js")
                .Include("~/Content/build/js/pages/forms/custom-validation.js")
                .Include("~/Content/build/scripts/jquery.signalR-2.4.1.min.js");

            scriptBundle.Orderer = new PassthruBundleOrderer();
            bundles.Add(scriptBundle);

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //    "~/Content/vendor/bootstrap/css/bootstrap.min.css",
            //    "~/Content/vendor/font-awesome-5/css/all.css",
            //    "~/Content/vendor/font-awesome/css/font-awesome.min.css",
            //    "~/Content/vendor/toastr/toastr.min.css",
            //    "~/Content/vendor/jquery-confirm/dist/jquery-confirm.min.css",
            //    "~/Content/vendor/jquery-datatable/dataTables.bootstrap4.min.css",
            //    "~/Content/build/css/color_skins.css"
            //   ));
            //bundles.Add(new ScriptBundle("~/Content/js").Include(
            //    "~/Content/build/bundles/libscripts.bundle.js",
            //    "~/Content/build/bundles/vendorscripts.bundle.js",
            //    "~/Content/vendor/toastr/toastr.js",
            //    "~/Content/vendor/jquery-confirm/dist/jquery-confirm.min.js",
            //    "~/Content/build/bundles/mainscripts.bundle.js",
            //    "~/Content/build/bundles/datatablescripts.bundle.js",
            //    "~/Content/build/js/pages/ui/dialogs.js",
            //    "~/Content/build/js/button-ripple.js",
            //    "~/Content/build/js/custom.js"
            //));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Content/build/scripts/jquery.validate*"));
        }
    }
}
