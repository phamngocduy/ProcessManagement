using System.Web;
using System.Web.Optimization;

namespace ProcessManagement
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include(
                   "~/Content/vendor/bootstrap/css/bootstrap.min.css",
                   "~/Content/vendor/font-awesome/css/font-awesome.min.css",
                   "~/Content/vendor/toastr/toastr.min.css",
                   "~/Content/vendor/jquery-datatable/dataTables.bootstrap4.min.css",
                   "~/Content/build/css/main.css",
                   "~/Content/build/css/color_skins.css",
                   "~/Content/build/css/custom.css"
               ));
            bundles.Add(new ScriptBundle("~/Content/js").Include(
                "~/Content/build/bundles/libscripts.bundle.js",
                "~/Content/build/bundles/vendorscripts.bundle.js",
                "~/Content/vendor/toastr/toastr.js",
                "~/Content/build/bundles/mainscripts.bundle.js",
                "~/Content/build/bundles/datatablescripts.bundle.js",
                "~/Content/build/js/pages/ui/dialogs.js",
                "~/Content/build/js/cookie.js",
                "~/Content/build/js/theme.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Content/build/scripts/jquery.validate*"));
        }
    }
}
