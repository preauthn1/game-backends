using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http.Formatting;
using System.Web.Http;

namespace gitlab_hook
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //全站預設使用json格式 而非xml
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
