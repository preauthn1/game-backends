using Microsoft.AspNetCore.Http;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Text;
using target.Utility.Extensions;
using example.invalid;
using target.Models;
using System.Linq;
using System.Threading.Tasks;
using target.Utility.Extensions.Web;
using target.Web.Modules.Filter;

namespace target.Utility.Helper.Dependency
{
    public class HttpRequestLogHelper
    {

        public static void SetRequestInfo(HttpRequest request, ApiRequestLogEntity model, bool isException = false)
        {
            try
            {
                model.RequestUrl = GenerateFullUrl(request);
                model.RelativeUrl = request.Path;
                model.HttpMethod = request.Method;
                model.Doamin = ""; //request.Host.Value; 在這個系統紀錄這個沒啥意義 感覺只是浪費空間
                model.IdentityKey = GetDeviceIdentityOrMemo(request);

                if(string.IsNullOrWhiteSpace(model.UniqueID))
                {
                    model.UniqueID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
                }

                if (!string.IsNullOrWhiteSpace(model.RelativeUrl))
                {
                    model.RelativeUrl = model.RelativeUrl.TruncateLastWordIfMatch('/');
                }

                var apiConfig = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest();

                //若request config開關 有設關閉/啟用
                if (apiConfig.IsRequestBodyLogEnable)
                {
                    model.RequestBody = request.GetRequestBodyString();
                }
                else
                {
                    if (apiConfig.IsDisabledRequestBodyLogOnlyForWeeks)
                    {
                        //寫死的 意思是 4/15以後 又會自動恢復requestLog了
                        if (DateTimeHelper.GetCurrentTime() < new DateTime(2022, 4, 15))
                        {
                            model.RequestBody = SystemConstant.Default_Text_DisabledLog;
                        }
                        else
                        {
                            model.RequestBody = request.GetRequestBodyString();
                        }
                    }
                    else
                    {
                        model.RequestBody = SystemConstant.Default_Text_DisabledLog;
                    }
                }

                //若開關有設關閉/啟用
                if (isException)
                {
                    //exception的話 一律紀錄
                    model.Headers = request.GetUsefulHeaders().ConvertToJsonString();
                }
                else if (!apiConfig.IsHeaderLogEnable)
                {
                    model.Headers = SystemConstant.Default_Text_DisabledLog;
                }
                else
                {
                    if (apiConfig.HeaderLogLevelType == HeaderLogLevelType.Default)
                    {
                        model.Headers = request.GetUsefulHeaders().ConvertToJsonString();
                    }
                    else
                    {
                        string myAttrDefineKeys = apiConfig.HeaderLogLevelType.GetDescription();
                        List<string> targetKeys = AppHelper.AnalyzeTextFromRawTextBySpaceChar(myAttrDefineKeys);
                        model.Headers = request.GetRequiredHeaders(targetKeys).ConvertToJsonString();
                    }
                }

                //若response開關有設關閉/啟用
                if (!apiConfig.IsResponseLogEnable)
                {
                    model.ResponseText = SystemConstant.Default_Text_DisabledLog;
                }

                model.UserIdentity = GetUserIdentity(request);

                string userIdentity = request.GetHeaderValueByKey(SystemConstant.Header_LogInfo_UserIdentity);

                if (!string.IsNullOrWhiteSpace(userIdentity))
                {
                    model.UserIdentity = userIdentity;
                }

                model.DebugInfo2 += request.GetHeaderValueByKey(SystemConstant.HeaderKey_DebugInfo2);
                model.DebugInfo3 = request.GetHeaderValueByKey(SystemConstant.HeaderKey_ReferOrderNo);

                if (string.IsNullOrWhiteSpace(model.DebugInfo))
                {
                    model.DebugInfo = request.GetHeaderValueByKey(SystemConstant.HeaderKey_DebugInfo1);
                }
            }
            catch (Exception ex)
            {
                string debugMsg = ex.Message;
            }

            return;
        }

        private static string GenerateFullUrl(HttpRequest request)
        {
            string baseUrl = $"{request.Scheme}://{request.Host.Value}";
            string suffixUrl = $"{request.Path}{request.QueryString}";

            //為了減少寫入資料量大小
            if (baseUrl.Contains("example.invalid"))
                return "{BASE_URL}" + suffixUrl;

            if (baseUrl.Contains("example.invalid"))
                return "{BASE_URL}" + suffixUrl;

            return $"{baseUrl}{suffixUrl}";
        }


        private static string GetUserIdentity(HttpRequest request)
        {
            string jwt = request.GetHeaderValueByKey(SystemConstant.HeaderKey_AccessToken);

            string userID = MyJwtHelper.GetUserIdentityTextFromJwt(jwt);

            if (!string.IsNullOrWhiteSpace(userID))
                return userID;

            bool isTryGetPlayerID = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest().IsLoggingPlayerID;

            if (!isTryGetPlayerID)
                return string.Empty;

            if (request.Method == "GET")
            {
                return request.Query["PlayerID"];
            }

            string body = request.GetRequestBodyString();

            if (string.IsNullOrWhiteSpace(body))
                return string.Empty;

            if (body == "***MASK***")
                return string.Empty;

            return TryGetPlayerID(body);
        }

        private static string GetDeviceIdentityOrMemo(HttpRequest request)
        {
            //有些API會用此欄位來記錄額外資訊
            string idenityMemo = request.GetHeaderValueByKey(SystemConstant.HeaderKey_IdentityMemo);

            if (!string.IsNullOrWhiteSpace(idenityMemo))
                return idenityMemo;

            var deviceID = HeaderValueHelper.GetHDCode();
            return deviceID;
        }


        private static string TryGetPlayerID(string jsonBody)
        {
            try
            {
                var parseModel = jsonBody.TryJsonConvertToModel<Dictionary<string, object>>();

                if (parseModel.ContainsKey("PlayerID"))
                    return parseModel["PlayerID"].ToString();

                if (parseModel.ContainsKey("playerID"))
                    return parseModel["playerID"].ToString();

                if (parseModel.ContainsKey("playerid"))
                    return parseModel["playerid"].ToString();
            }
            catch (Exception ex)
            {

            }

            return string.Empty;
        }


        public static ApiRequestLogEntity InstanceResponseLogModel(HttpResponse response)
        {
            var model = new ApiRequestLogEntity();

            try
            {
                model.ResponseStatusCode = response.StatusCode;
                model.ResponseLength = response.Body.Length;

                // Response text log 關閉時，不讀取或配置整份 response body。
                if (!ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest().IsResponseLogEnable)
                {
                    model.ResponseText = SystemConstant.Default_Text_DisabledLog;
                    return model;
                }

                response.Body.Position = 0;
                using var reader = new StreamReader(response.Body, leaveOpen: true);
                model.ResponseText = reader.ReadToEndAsync().Result;
                response.Body.Position = 0;

            }
            catch
            {
                // Logging failure must not affect the response pipeline.
            }

            return model;
        }


    }
}
