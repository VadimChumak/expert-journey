using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NewsWebSite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "WithTitle",
               url: "{controller}/{title}-{id}",
               defaults: new { controller = "News", action = "Article" }
           );
            routes.MapRoute(
                name: "AllNews",
                url: "News/All",
                defaults: new { controller = "News", action = "Index" }
                );
            routes.MapRoute(
                name: "MyNews",
                url: "News/My",
                defaults: new { controller = "News", action = "Index", isUserNews = true }
                );
            routes.MapRoute(
               name: "InterestingNews",
               url: "News/Interesting",
               defaults: new { controller = "News", action = "Index", isInterestingNews = true }
               );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "News", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
