using Newtonsoft.Json;
using Repository.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Agent77
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(Object sender, EventArgs e)
        {            
            Exception lastError = Server.GetLastError();
            if (lastError != null)
            {              
                if (lastError is HttpAntiForgeryException)
                {
                    Server.ClearError();
                    Response.Redirect(Request.AppRelativeCurrentExecutionFilePath);
                }
            }
        }
    }
}
