using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ProcessManagement
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.LowercaseUrls = true;
            routes.MapMvcAttributeRoutes();
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //default route
            routes.MapRoute(
                name: "LocalizedDefault",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { controller = "group", action = "index", id = UrlParameter.Optional },
                constraints: new { lang = "en|vi", controller = "home|account|error" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "group", action = "index", id = UrlParameter.Optional, lang = "en" },
                constraints: new { lang = "en|vi", controller = "home|account|error" }
            );

            //group list
            routes.MapRoute(
               name: "GroupLocalizedDefault",
               url: "{lang}/{controller}/{action}/{groupid}",
               defaults: new { controller = "group", action = "index", groupid = UrlParameter.Optional },
               constraints: new { lang = "en|vi", controller = "group" }
            );
            routes.MapRoute(
               name: "GroupDefault",
               url: "{controller}/{action}/{groupid}",
               defaults: new { controller = "group", action = "index", groupid = UrlParameter.Optional, lang = "en" },
               constraints: new { lang = "en|vi", controller = "group" }
            );


            //group 
            routes.MapRoute(
               name: "GroupControlLocalizedDefault",
               url: "{lang}/{userslug}/{groupslug}/{action}/{id}",
               defaults: new { controller="group", id = UrlParameter.Optional },
               constraints: new { lang = "en|vi" }
            );
            routes.MapRoute(
               name: "GroupControlDefault",
               url: "{userslug}/{groupslug}/{action}/{id}",
               defaults: new { controller = "group", lang = "en", id = UrlParameter.Optional },
               constraints: new { lang = "en|vi" }
            );



            //routes.MapRoute(
            //    name: "AccountDefault",
            //    url: "{lang}/Account/{action}/{id}",
            //    defaults: new { controller = "Account", action = "Login", lang = UrlParameter.Optional, id = UrlParameter.Optional },
            //    constraints: new { lang = "en|vi"}
            //);
            //routes.MapRoute(
            //   name: "UserDefault",
            //   url: "{lang}/{userslug}/{action}/{id}",
            //   defaults: new { controller = "Group", action = "Index", lang = UrlParameter.Optional, id = UrlParameter.Optional },
            //   constraints: new { lang = "en|vi" }
            //);


        }
    }
}
