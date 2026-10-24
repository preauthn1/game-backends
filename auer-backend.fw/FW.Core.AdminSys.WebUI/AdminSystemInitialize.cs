using FW.Core.AdminSys.Provider.Auth;
using FW.Core.Auth;
using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FW.Core.AdminSys.WebUI
{
    public static class AdminSystemInitialize
    {
        public static void Initialize()
        {
            DependencyResolver.SetResolver(new FW.Core.Web.Mvc.EngineDepencyResolver());

            MvcAuthRuleDbProvider.NonAuthTokenCollection.Add("*_Default_*_*");
            GlobalFilters.Filters.Add(new CustomAuthorizeFilter(EngineContext.Current.Resolve<AuthManager>()));
            GlobalFilters.Filters.Add(EngineContext.Current.Resolve<ActionLogFilter>());

            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}