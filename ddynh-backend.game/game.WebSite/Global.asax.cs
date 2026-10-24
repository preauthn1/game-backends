using FW.Core.Engine;
using example.invalid;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;

namespace game.WebSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            //DI注入設定
            InitializeIOC();
        }


        private void InitializeIOC()
        {
            List<string> projectList = new List<string>
            {
                "game.Service"
            };

            IOCInitializeHelper.InitializeIOC(projectList);
        }

    }
}
