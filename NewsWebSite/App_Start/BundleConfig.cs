using System.Web.Optimization;

namespace NewsWebSite
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/default").Include(
                    "~/Scripts/jquery-3.1.1.js",
                    "~/Scripts/jquery.unobtrusive-ajax.js",
                    "~/Scripts/materialize.min.js",
                    "~/Scripts/select2.min.js",
                    "~/Scripts/materialize.min.js",
                    "~/Scripts/SideNav.js",
                    "~/Scripts/jquery.signalR-2.2.1.min.js",
                    "~/Scripts/HtmlEncode.js"));

            bundles.Add(new ScriptBundle("~/bundles/formPost").Include(
                        "~/Scripts/formPost.js"));

            bundles.Add(new ScriptBundle("~/bundles/articlePage").Include("~/Scripts/ArticlePage.js"));


            bundles.Add(new ScriptBundle("~/bundles/newsList").Include(
                "~/Scripts/NewsIndex.js"));

            bundles.Add(new StyleBundle("~/bundles/DefaultStyles").Include(
                    "~/Content/Style.css",
                    "~/Content/icon.css",
                    "~/Content/Materialize.css",
                    "~/Content/selectStyle.css",
                    "~/Content/scroll.css"));

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
        }
    }
}
