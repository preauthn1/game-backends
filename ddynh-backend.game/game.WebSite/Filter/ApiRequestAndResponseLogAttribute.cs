using game.Entity.SystemLog;
using game.Service;
using game.Service.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility.ThirdParty;
using example.invalid;
using System.Text;
using System.Web.UI;
using FW.Core.Helper;
using FW.Core.Engine;
using game.Infrastructure;

namespace game.WebSite
{
    public class ApiRequestAndResponseLogAttribute : ActionFilterAttribute
    {

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //若config開關沒打開
            if (!SettingHelper.IsGlobalApiLogEnable)
            {
                base.OnResultExecuted(filterContext);
                return;
            }

            try
            {
                var service = IOCHelper.Resolve<IApiRequestLogService>();

                string responseText = GetResponseText(filterContext);

                var request = System.Web.HttpContext.Current.Request;
                var requestUri = request.Url;
                var requestBody = System.Web.HttpContext.Current.Request.GetRequestBodyString();


                string memo = System.Web.HttpContext.Current.Items["Memo"] as string;
                string uuid = System.Web.HttpContext.Current.Items["UUID"] as string;
                

                var model = new ApiRequestLog
                {
                    IdentityKey = uuid,
                    ResponseStatusCode = (int)filterContext.RequestContext.HttpContext.Response.StatusCode,
                    Doamin = requestUri.Authority,
                    RelativeUrl = requestUri.AbsolutePath,
                    RequestUrl = requestUri.AbsoluteUri,
                    Headers = HeaderValueHelper.GetRequestHeadersJsonByColleaction(request.Headers),
                    HttpMethod = request.HttpMethod,
                    IPAddress = IPHelper.GetIPNew(),
                    IPLocation = string.Empty,
                    RequestBody = requestBody,
                    Memo = memo,
                    ResponseText = responseText,
                    UserIdentity = string.Empty,
                };

                service.CreateLog(model);

            }
            catch (Exception ex)
            {
                string msg = AppHelper.GetCurrentRequestFullUrlDebugInfo() + ex.ToString();

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_payment,
                    Message = msg
                });
            }

            base.OnResultExecuted(filterContext);
        }

        private string GetResponseText(ResultExecutedContext filterContext)
        {
            ContentResult tryContent = filterContext.Result as ContentResult;
            JsonResult tryJson = filterContext.Result as System.Web.Mvc.JsonResult;

            string responseText = string.Empty;

            if (tryContent != null)
            {
                responseText = tryContent.Content;
            }
            else if (tryJson != null)
            {
                responseText = tryJson.Data.ConvertToJsonString();
            }

            return responseText;
        }


    }

}