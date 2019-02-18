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
            routes.MapRoute(
                name: "LocalizedDefault",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { controller = "group", action = "index", id = UrlParameter.Optional },
                constraints: new { lang = "en|vi", controller = "home|account|error|api" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "group", action = "index", lang = "en", id = UrlParameter.Optional },
                constraints: new { lang = "en|vi", controller = "home|account|error|api" }
            );
            
            //group control
            routes.MapRoute(
               name: "GroupControlLocalizedDefault",
               url: "{lang}/{groupslug}-{groupid}/{controller}/{action}/{stepid}/{taskid}",
               defaults: new { controller = "group", stepid = UrlParameter.Optional, taskid = UrlParameter.Optional },
               constraints: new { lang = "en|vi", controller = "group|process" }
            );
            routes.MapRoute(
               name: "GroupControlDefault",
               url: "{groupslug}-{groupid}/{controller}/{action}/{stepid}/{taskid}",
               defaults: new { controller = "group", lang = "en", stepid = UrlParameter.Optional, taskid = UrlParameter.Optional },
               constraints: new { lang = "en|vi", controller = "group|process" }
            );

            //group default
            routes.MapRoute(
               name: "GroupLocalizedDefault",
               url: "{lang}/{controller}/{action}/{groupid}",
               defaults: new { controller = "group", action = "index", groupid = UrlParameter.Optional },
               constraints: new { lang = "en|vi", controller = "group|process" }
            );
            routes.MapRoute(
               name: "GroupDefault",
               url: "{controller}/{action}/{groupid}",
               defaults: new { controller = "group", action = "index", lang = "en", groupid = UrlParameter.Optional },
               constraints: new { lang = "en|vi", controller = "group|process" }
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
