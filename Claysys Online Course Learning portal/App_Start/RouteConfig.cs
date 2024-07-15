using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Claysys_Online_Course_Learning_portal
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            // Add route for AdminController
            routes.MapRoute(
           name: "Admin",
           url: "Admin/{action}/{id}",
           defaults: new { controller = "Admin", action = "UserManagement", id = UrlParameter.Optional }
       );
        }
    }
}
