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
using FW.Core.Logger;
using game.Infrastructure;

namespace game.WebSite
{
    public class PaymentApiExceptionFilterAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            LogManager.GetLogger("exception").LogError(filterContext.Exception.ToString());

            //若config開關沒打開
            if(!SettingHelper.IsGlobalApiLogEnable)
            {
                base.OnException(filterContext);
                return;
            }

            try
            {
                var service = IOCHelper.Resolve<IApiRequestLogService>();

                string exceptionMsg = "Exception: " + filterContext.Exception.Message;

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
                    ResponseText = exceptionMsg,
                    UserIdentity = string.Empty,
                    StackTrace = filterContext.Exception.StackTrace,
                    DebugInfo = filterContext.Exception.GetErrorOccurLine()
                };


                if(SettingHelper.IsAlwaysSlackNotifyWhenExceotion)
                {
                    string slackMsg = $" 錯誤啦，{exceptionMsg}，{model.IdentityKey}{model.Memo} , {model.RelativeUrl}";
                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                    {
                        Message = slackMsg,
                        Channel = SlackChannelEnum.n_api_payment
                    });
                }

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


            base.OnException(filterContext);
        }
        
     
    }

}