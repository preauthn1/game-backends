using example.invalid;
using System.Web.Mvc;
using System.Web.Routing;

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
        }
    }
}
