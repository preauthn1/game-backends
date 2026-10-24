using System.Linq;
using System;
using example.invalid;
using example.invalid.Http;
using example.invalid.Http.Formatting;
using example.invalid.Http.Headers;
using System.Web.Http.Filters;
using System.Data;
using System.Collections.Generic;
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;
using game.Entity.SystemLog;
using FW.Core.Helper;
using game.Infrastructure.Extensions;
using System.Web;
using game.Service.Service;
using game.Entity.ViewModel.GamePlayAPI.Core;
using game.Entity;
using FW.Core.Engine;
using game.Service;

namespace game.GamePlayAPI
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
            string errorMsg = "";

            var customizeException = context.Exception as TerproException;

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
                DebugInfo3 = "",                    
                WebSiteName = SettingHelper.WebSiteName,
                StackTrace = context.Exception.StackTrace
            };

            var apiObject = new ApiResponseErrorModel
            {
                Code = 500001,
                Data = null,
                Message = ""
            };


            bool isTerproException = customizeException != null;

            if (isTerproException)
            {

                errorMsg = customizeException.ErrroCode.GetDescription();
                model.DebugInfo3 = customizeException.ErrroCode.ToIntegerString();

                if (customizeException.ErrroCode.ToString().Contains("InvalidParameter"))
                {
                    if (!string.IsNullOrWhiteSpace(customizeException?.ErrorDetail?.ColumnName))
                    {
                        errorMsg = errorMsg + $" ({customizeException.ErrorDetail.ColumnName})";
                    }
                }

                //如果有指定要強制覆蓋msg
                if (customizeException.ErrorDetail != null)
                {
                    if (!string.IsNullOrWhiteSpace(customizeException.ErrorDetail.ReplaceMessage))
                    {
                        errorMsg = customizeException.ErrorDetail.ReplaceMessage;
                    }
                }

                model.DebugInfo = errorMsg;
                
                apiObject.Code = customizeException.ErrroCode.ToInteger();
                apiObject.Data = customizeException.ErrorDetail;
            }
            else
            {
                errorMsg = context.Exception.GetDebugMessage();

                //如果是production環境 錯誤訊息不顯示太直白
                if (SettingHelper.IsProduction)
                {
                    errorMsg = "系统忙碌中或无法正确回应，请稍后再做尝试";
                }
            }

            model.RequestUrl = model.RequestUrl ?? string.Empty;

            string urlToLower = model.RequestUrl.ToLower();

            //如果有可能紀錄到password的API 不紀錄
            if (SystemConst.PasswordApiUrls.Any(keyword => urlToLower.Contains(keyword)))
            {
                model.RequestBody = "*** MASK ***";
            }

            apiObject.Message = errorMsg;
            model.ResponseText = apiObject.ConvertToJsonString();

            var service = IOCHelper.Resolve<IApiRequestLogService>();
            string tableName = SettingHelper.ApiRequestLogTableName;
            service.CreateLogToTargetTable(model, tableName);



            if(isTerproException && SettingHelper.IsAlwaysSlackNotifyWhenTerproExceotion)
            {
                string guidMsg = requestGuid.IsEmptyOrNull() ? "" : $"guid: `{requestGuid}` ";
                string userNameMsg = userName.IsEmptyOrNull() ? "" : $"user: `{userName}`";

                string slackMsg = GenerateSlackDebugMessage(context, errorMsg, model, guidMsg, userNameMsg);

                SlackChannelEnum channel = GetTargetSlackChannel();

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = channel,
                    Message = slackMsg
                });
            }
            else if (!isTerproException && SettingHelper.IsAlwaysSlackNotifyWhenExceotion)
            {
                string guidMsg = requestGuid.IsEmptyOrNull() ? "" : $"guid: `{requestGuid}` ";
                string userNameMsg = userName.IsEmptyOrNull() ? "" : $"user: `{userName}`";

                string slackMsg = GenerateSlackDebugMessage(context, errorMsg, model, guidMsg, userNameMsg);

                var channel = GetTargetSlackChannel();

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = channel,
                    Message = slackMsg
                });
            }


            var statusCode = HttpStatusCode.BadRequest;
            HttpResponseMessage errorResponse = new HttpResponseMessage();
            errorResponse.StatusCode = statusCode;
            errorResponse.Content = new ObjectContent<ApiResponseErrorModel>(apiObject, new JsonMediaTypeFormatter(), "application/json");



            context.Response = errorResponse;
            context.Response.StatusCode = statusCode;
            context.Response.ReasonPhrase = typeof(TerproException).Name;  //某段程式碼有做為判斷依據
        }

        private static string GenerateSlackDebugMessage(HttpActionExecutedContext context, string errorMsg, ApiRequestLog model, string guidMsg, string userNameMsg)
        {
            string errorLine = context.Exception.GetErrorOccurLine().Truncate(400);

            string slackMsg = $"錯誤啦！ `{model.RelativeUrl}`  {guidMsg} {userNameMsg} ," +
                $" {Environment.NewLine}{errorMsg}{context.Exception.Message} {Environment.NewLine} ``` {errorLine} ```";

            return slackMsg;
        }

        private static SlackChannelEnum GetTargetSlackChannel()
        {
            var channel = SlackChannelEnum.n_dd_api_game_merchant;

            string websiteName = SettingHelper.WebSiteName ?? "";

            var isJobAPI = websiteName.ToLower().Contains("job");

            if (isJobAPI)
            {
                channel = SlackChannelEnum.n_dd_job_third_party;
            }

            return channel;
        }

        private string GetUserNameHeader(HttpRequest request)
        {
            var userName = request.Headers[SystemConst.AdminSys_AlwaysHeaderUserName];
            return userName ?? string.Empty;
        }


    }
}