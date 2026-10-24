using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FW.Core.AdminSys.WebUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "AdminRoute",
                url: "Admin/{controller}/{action}/{id}",
                defaults: new { controller = "Default", action = "Logon", id = UrlParameter.Optional },
                namespaces: new string[] { "FW.Core.AdminSys.WebUI.Controllers" }
            );
        }
    }
}
