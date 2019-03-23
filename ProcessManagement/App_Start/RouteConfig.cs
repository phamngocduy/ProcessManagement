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

            
            //"^group"
            //default route
            //TODO: tách file ra 1 route riêng không ngôn ngư
            routes.MapRoute(
                name: "LocalizedDefault",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { controller = "group", action = "index", id = UrlParameter.Optional},
                constraints: new { lang = "en|vi", controller = "home|account|error" },
                namespaces: new[] { "ProcessManagement.Controllers" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "group", action = "index", lang = "en", id = UrlParameter.Optional},
                constraints: new { lang = "en|vi", controller = "home|account|error" },
                namespaces: new[] { "ProcessManagement.Controllers" }
            );
            
            //group control
            routes.MapRoute(
               name: "GroupControlLocalizedDefault",
               url: "{lang}/{groupslug}-{groupid}/{controller}/{action}",
               defaults: new { controller = "group", action = "show"},
               constraints: new { lang = "en|vi", controller = "group|process|processrun" },
               namespaces: new[] { "ProcessManagement.Controllers" }
            );
            routes.MapRoute(
               name: "GroupControlDefault",
               url: "{groupslug}-{groupid}/{controller}/{action}",
               defaults: new { controller = "group", lang = "en", action = "show"},
               constraints: new { lang = "en|vi", controller = "group|process|processrun" },
               namespaces: new[] { "ProcessManagement.Controllers" }
            );

            //group default
            routes.MapRoute(
               name: "GroupLocalizedDefault",
               url: "{lang}/{controller}/{action}/{groupid}",
               defaults: new { controller = "group", action = "index", groupid = UrlParameter.Optional },
               constraints: new { lang = "en|vi", controller = "group|process" },
               namespaces: new[] { "ProcessManagement.Controllers" }
            );
            routes.MapRoute(
               name: "GroupDefault",
               url: "{controller}/{action}/{groupid}",
               defaults: new { controller = "group", action = "index", lang = "en", groupid = UrlParameter.Optional },
               constraints: new { lang = "en|vi", controller = "group|process" },
               namespaces: new[] { "ProcessManagement.Controllers" }
            );

            //File
            routes.MapRoute(
               name: "FileDefault",
               url: "{controller}/{action}/{file}",
               defaults: "",
               constraints: new { controller = "file" },
               namespaces: new[] { "ProcessManagement.Controllers" }
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
