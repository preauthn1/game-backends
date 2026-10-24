using FW.Core.Auth;
using FW.Core.Engine;
using FW.Core.Web.WebAPI;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace game.GameAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {


            GlobalConfiguration.Configuration.Formatters.Insert(0, new JsonpMediaTypeFormatter());

            var json = config.Formatters.JsonFormatter;

            json.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            var jsonFormatter = config.Formatters.OfType<JsonpMediaTypeFormatter>().First();

            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "LogonApi",
                routeTemplate: "api/v1/logon",
                defaults: null,
                constraints: null,
                handler: new FW.Core.Web.WebAPI.APIHandler.LogonDelegatingHandler(EngineContext.Current.Resolve<IAPIConsumerProvider>())
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/v1/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = RouteParameter.Optional }
                );


            config.Routes.MapHttpRoute(
              name: "HomePage",
              routeTemplate: "",
              defaults: new
              {
                  controller = "Home",
                  action = "Index"
              }
           );
            
            //毫無意義的檢查機制 移除之
            //config.MessageHandlers.Add(new FW.Core.Web.WebAPI.APIHandler.ServerAuthDelegatingHandler(EngineContext.Current.Resolve<AuthManager>()));
            
            //全域api紀錄
            config.MessageHandlers.Add(new ApiRequestAndResponseLogHandler());

            //錯誤log處理
            config.Filters.Add(new ApiErrorHandleAttribute());


            config.ParameterBindingRules.Insert(0, param =>
            {
                if (param.ParameterType.ToString().IndexOf("System.Nullable") != -1
                        || param.ParameterType == typeof(string))                {
                    return new FW.Core.Web.WebAPI.NullableParameterBinding(param);
                }

                return null;

            });


        }
    }

}
