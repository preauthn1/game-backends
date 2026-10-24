using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FW.Core.Web.Mvc
{
    public class JsonpResult : JsonResult
    {
        private static readonly string JsonpCallbackName = "callback";
        private static readonly string CallbackApplicationType = "application/json";

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((JsonRequestBehavior == JsonRequestBehavior.DenyGet) &&
                  String.Equals(context.HttpContext.Request.HttpMethod, "GET"))
            {
                throw new InvalidOperationException();
            }
            var response = context.HttpContext.Response;
            response.Headers.Add("Access-Control-Allow-Origin", "*");

            if (!String.IsNullOrEmpty(ContentType))
                response.ContentType = ContentType;
            else
                response.ContentType = CallbackApplicationType;
            if (ContentEncoding != null)
                response.ContentEncoding = this.ContentEncoding;
            if (Data != null)
            {
                String buffer;
                var request = context.HttpContext.Request;
                var serializer = new JavaScriptSerializer();
                if (request[JsonpCallbackName] != null)
                    buffer = String.Format("{0}({1})", request[JsonpCallbackName], serializer.Serialize(Data));
                else
                    buffer = serializer.Serialize(Data);
                response.Write(buffer);
            }
        }
    }

    public static class ContollerExtensions
    {
        /// <summary>
        /// Extension methods for the controller to allow jsonp.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static JsonpResult Jsonp(this Controller controller, object data)
        {
            JsonpResult result = new JsonpResult()
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

            return result;
        }
    }

}
