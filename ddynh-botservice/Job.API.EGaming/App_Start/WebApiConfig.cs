using game.GamePlayAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Job.API.EGaming
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        { 
            //全站預設使用json格式 而非xml
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Converters.Add(new IsoDateTimeConverter()
            {
                DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff+08:00"
            });

            settings.Formatting = Formatting.None; //去掉所有空白與縮排 流量會差15~30%
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();


            //全域api紀錄
            config.MessageHandlers.Add(new ApiRequestAndResponseLogHandler());

            //錯誤log處理
            config.Filters.Add(new ApiErrorHandleAttribute());


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
