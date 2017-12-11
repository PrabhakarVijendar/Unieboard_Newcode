using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace UniEBoard
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Secure",
                url: "{controller}/{action}/{id}/{identityToken}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional, identityToken = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "LogOff",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "LogOff", id = UrlParameter.Optional }
            );
        }
    }
}