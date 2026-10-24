using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Job.API.APL
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            InitializeIOC();
        }

        private void InitializeIOC()
        {
            List<string> projectList = new List<string>
            {
                "game.Service",
                "game.Service.Job"
            };

            IOCInitializeHelper.InitializeIOC(projectList);
        }
    }
}
