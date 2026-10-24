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

            config.MessageHandlers.Add(new FW.Core.Web.WebAPI.APIHandler.ServerAuthDelegatingHandler(EngineContext.Current.Resolve<AuthManager>()));
            config.MessageHandlers.Add(new HttpErrorDelegatingHandler());

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

    /// <summary>
    /// API访问异常日志记录
    /// </summary>
    public class HttpErrorDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return await base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>((responseToCompleteTask) =>
            {
                var response = responseToCompleteTask.Result;
                HttpError error = null;
                if (response.TryGetContentValue<HttpError>(out error))
                {
                    if (error != null)
                    {                        
                        FW.Core.Logger.LogManager.Default.LogError(String.Format("\r\nAbsoluteUri:{0}\r\nExceptionMessage:{1}\r\nStackTrace:{2}\r\nMessage:{3}\r\nMessageDetail:{4}\r\n", 
                            response.RequestMessage.RequestUri.AbsoluteUri, 
                            error.ExceptionMessage, 
                            error.StackTrace, 
                            error.Message, 
                            error.MessageDetail));
                    }
                }
                return response;
            });
        }
    }
}
