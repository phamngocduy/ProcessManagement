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
                   "~/Content/libs/font-awesome-4/css/font-awesome.min.css",
                   "~/Content/libs/font-awesome-5/css/all.css",
                   "~/Content/libs/bootstrap/dist/css/bootstrap.min.css",
                   "~/Content/build/css/responsive.css",
                   "~/Content/build/css/grid-boostrap-fixheight.css"
               ));
            bundles.Add(new ScriptBundle("~/Content/js").Include(
                "~/Content/libs/jquery/dist/jquery.min.js",
                "~/Content/libs/bootstrap/dist/js/bootstrap.min.js",
                "~/Content/build/js/custom.min.js",
                "~/Content/build/js/cookie.js",
                "~/Content/build/js/language.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Content/build/scripts/jquery.validate*"));
        }
    }
}
