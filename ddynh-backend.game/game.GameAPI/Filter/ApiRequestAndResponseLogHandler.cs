using FW.Core.Helper;
using game.Entity.SystemLog;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility.ThirdParty;
using game.Service.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using example.invalid.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Linq;

namespace game.GameAPI
{
    public class ApiRequestAndResponseLogHandler : DelegatingHandler
    {

        //這些API頻率太高 內容也沒啥營養 不紀錄log
        private List<string> excludeUrl = new List<string>
        {
            "/api/v1/AdminSys/GetRightByToken",
            "/api/v1/AdminSys/AddAdminLog",
            "/api/v1/AdminSys/GetAllUserRight",
            "/api/v1/Money/GetConvertLogCountByStatus",
            "/api/v1/User/GetLogonQuestionCount",
            "/api/v1/User/GetOnlineVipUserCount",
            "/api/v1/User/GetPlatformDownloadIncorrectCount",
            "/api/v1/User/GetUnReplyQuestionCount",
            "/api/v1/Warning/ExistWarning",
            "/api/v1/Area/GetPlatform",
            "/api/v1/Game/GetGame"
        };


        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // let other handlers process the request
            HttpResponseMessage result = await base.SendAsync(request, cancellationToken);

            //若config開關沒打開
            if (!SettingHelper.IsGlobalApiLogEnable)
            {
                return result;
            }            

            bool isErroException = (int)result.StatusCode >= 400;

            if(isErroException)
            {
                HttpError error = null;

                if (result.TryGetContentValue<HttpError>(out error))
                {
                    //常見的一般exception 錯誤Log由另一隻filter處理
                    bool isCodeException = !string.IsNullOrWhiteSpace(error.StackTrace);

                    if (isCodeException)
                    {
                        return result;
                    }
                }
            }
            
            try
            {
                string responseBody = string.Empty;
                long responseLength = 0;
                if (result.Content != null)
                {
                    responseBody = await result.Content.ReadAsStringAsync();
                    responseLength = result.Content.Headers.ContentLength ?? 0;
                }

                var requestUri = request.RequestUri;
                string relativeUrl = requestUri.AbsolutePath;
                
                //有特定幾隻API 請求頻率太高 不紀錄log 避免浪費資源
                if (excludeUrl.Contains(relativeUrl))
                {
                    return result;
                }

                var contextRequest = System.Web.HttpContext.Current.Request;
                var requestBody = contextRequest.GetRequestBodyString();

                string memo = contextRequest.Headers[SystemConst.AdminSys_AlwaysHeaderMemo];

                var model = new ApiRequestLog
                {
                    IdentityKey = "",
                    ResponseStatusCode = (int)result.StatusCode,
                    Doamin = requestUri.Authority,
                    RelativeUrl = requestUri.AbsolutePath,
                    RequestUrl = requestUri.AbsoluteUri,
                    Headers = HeaderValueHelper.GetRequestHeadersJsonByColleaction(contextRequest.Headers),
                    HttpMethod = contextRequest.HttpMethod,
                    IPAddress = IPHelper.GetIPNew(),
                    IPLocation = string.Empty,
                    RequestBody = requestBody,
                    Memo = memo,
                    ResponseText = responseBody,
                    ResponseLength = responseLength,
                    DebugInfo2 = contextRequest.Headers[SystemConst.AdminSys_AlwaysHeaderUniqueRequestID],
                    UserIdentity = GetUserNameHeader(contextRequest),
                };

                model.RequestUrl = model.RequestUrl ?? string.Empty;

                string urlToLower = model.RequestUrl.ToLower();

                //如果有可能紀錄到password的API 不紀錄
                //如果有可能紀錄到password的API 不紀錄
                if (SystemConst.PasswordApiUrls.Any(keyword => urlToLower.Contains(keyword)))
                {
                    model.RequestBody = "*** MASK ***";
                }


                var service = new ApiRequestLogService();
                service.CreateLog_GameApi(model);

            }
            catch (Exception ex)
            {
                string msg = AppHelper.GetCurrentRequestFullUrlDebugInfo() + ex.ToString();

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_api_gameapi,
                    Message = msg
                });
            }

            return result;
        }


        private string GetUserNameHeader(HttpRequest request)
        {
            var userName = request.Headers[SystemConst.AdminSys_AlwaysHeaderUserName];
            return userName ?? string.Empty;
        }

    }


}