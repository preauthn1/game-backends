using System.Linq;
using System;
using example.invalid;
using example.invalid.Http;
using example.invalid.Http.Formatting;
using example.invalid.Http.Headers;
using System.Web.Http.Filters;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Data;
using System.Collections.Generic;
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;
using game.Entity.SystemLog;
using FW.Core.Helper;
using game.Infrastructure.Extensions;
using System.Web;
using game.Service.Service;

namespace game.GameAPI
{
    public class ApiErrorHandleAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            base.OnException(context);

            var contextRequest = System.Web.HttpContext.Current.Request;
            var requestUri = context.Request.RequestUri;
            string memo = contextRequest.Headers[SystemConst.AdminSys_AlwaysHeaderMemo];
            string requestGuid = contextRequest.Headers[SystemConst.AdminSys_AlwaysHeaderUniqueRequestID];
            string userName = GetUserNameHeader(contextRequest);

            var model = new ApiRequestLog
            {
                IdentityKey = "",
                ResponseStatusCode = 0,
                Doamin = requestUri.Authority,
                RelativeUrl = requestUri.AbsolutePath,
                RequestUrl = requestUri.AbsoluteUri,
                Headers = HeaderValueHelper.GetRequestHeadersJsonByColleaction(contextRequest.Headers),
                HttpMethod = contextRequest.HttpMethod,
                IPAddress = IPHelper.GetIPNew(),
                IPLocation = string.Empty,
                RequestBody = contextRequest.GetRequestBodyString(),
                Memo = memo,
                ResponseText = context.Exception.Message,
                UserIdentity = userName,
                DebugInfo = context.Exception.GetErrorOccurLine(),
                DebugInfo2 = requestGuid,
                StackTrace = context.Exception.StackTrace
            };

            model.RequestUrl = model.RequestUrl ?? string.Empty;

            string urlToLower = model.RequestUrl.ToLower();

            //如果有可能紀錄到password的API 不紀錄
            if (SystemConst.PasswordApiUrls.Any(keyword => urlToLower.Contains(keyword)))
            {
                model.RequestBody = "*** MASK ***";
            }
            
            FW.Core.Logger.LogManager.Default.LogError(String.Format("\r\nAbsoluteUri:{0}\r\nExceptionMessage:{1}\r\nStackTrace:{2}\r\nMessage:{3}\r\nMessageDetail:{4}\r\n",
                           model.RequestBody,
                           context.Exception.Message,
                           context.Exception.StackTrace,
                           context.Exception.Message,
                           ""));

            var service = new ApiRequestLogService();
            service.CreateLog_GameApi(model);

            if(SettingHelper.IsAlwaysSlackNotifyWhenExceotion)
            {
                string slackMsg = $"錯誤啦！ {Environment.NewLine} `{model.RelativeUrl}`  Guid: `{requestGuid}`  操作人： {userName} , {Environment.NewLine}{context.Exception.Message} {Environment.NewLine} ```{context.Exception.GetErrorOccurLine()}```";
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_gameapi,
                    Message = slackMsg
                });
            }

        }


        private string GetUserNameHeader(HttpRequest request)
        {
            var userName = request.Headers[SystemConst.AdminSys_AlwaysHeaderUserName];
            return userName ?? string.Empty;
        }


    }
}