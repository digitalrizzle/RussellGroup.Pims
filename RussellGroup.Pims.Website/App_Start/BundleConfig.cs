using System.Web;
using System.Web.Optimization;

namespace RussellGroup.Pims.Website
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/view-datepickers.js",
                "~/Scripts/respond.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //    "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/jquery.datatable.js").Include(
                "~/Scripts/DataTables-1.10.0/jquery.dataTables.js",
                "~/Scripts/dataTables.bootstrap.js"));

             bundles.Add(new ScriptBundle("~/bundles/jquery.typeahead.js").Include(
                "~/Scripts/bloodhound.js",
                "~/Scripts/handlebars.js",
                "~/Scripts/typeahead.bundle.js",
                "~/Scripts/view-typeahead.js"));

            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/bootstrap.css", new CssRewriteUrlTransformWrapper())
                .Include("~/Content/bootstrap-theme.css", new CssRewriteUrlTransformWrapper())
                .Include("~/Content/bootstrap-datepicker.css", new CssRewriteUrlTransformWrapper()));

            bundles.Add(new StyleBundle("~/Content/jquery.datatable.css")
                .Include("~/Content/dataTables.bootstrap.css", new CssRewriteUrlTransformWrapper()));

            bundles.Add(new StyleBundle("~/Content/jquery.typeahead.css")
                .Include("~/Content/typeahead.css", new CssRewriteUrlTransformWrapper()));

            bundles.Add(new StyleBundle("~/Content/overrides.css")
                .Include("~/Content/Site.css", new CssRewriteUrlTransformWrapper()));
        }

        public class CssRewriteUrlTransformWrapper : IItemTransform
        {
            public string Process(string includedVirtualPath, string input)
            {
                return new CssRewriteUrlTransform().Process("~" + VirtualPathUtility.ToAbsolute(includedVirtualPath), input);
            }
        }
    }
}
