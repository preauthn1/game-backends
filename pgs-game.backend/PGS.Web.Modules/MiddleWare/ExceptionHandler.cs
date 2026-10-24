using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using target.BL.Interface;
using target.BL.Services;
using target.Models;
using target.Models.Attributes;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Helper.Dependency;
using target.Utility.IOC;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using example.invalid;
using System.Text.RegularExpressions;
using TG_Hook_API.Models;

namespace target.Web.Modules.Filter
{

    public static class ExceptionHandler
    {

        //特定環境、特定的API SQLException 錯誤訊息不slack通知 (因為很吵)
        public static Dictionary<string, List<string>> SkipErrorNotifyApis = new Dictionary<string, List<string>>
        {
            {
                SystemConstant.Env_Name_Development ,
                new List<string>
                {
                    "game/room/out",
                    "game/slot/bonus",
                    "game/slot/prob",
                    "game/slot/user-assigned-mode",
                    "bet-summary/server",
                    "system/server/re-register",
                    "system/server/robots"
                }
            },

            {
                SystemConstant.Env_Name_QA ,
                new List<string>
                {
                    "game/slot/bonus",
                }
            },

            {
                SystemConstant.Env_Name_Production ,
                new List<string>
                {
                    "game/slot/bonus",
                }
            },
        };

        public static void UseGlobalExceptionHandler(this IApplicationBuilder appBuilder)
        {
            appBuilder.UseExceptionHandler(app =>
            {
                app.Run(async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    //如果有指定要回復ts格式
                    if (SettingHelper.IsDev && HeaderValueHelper.IsResponseTypeScriptClass())
                    {
                        var myApiConfig = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest();

                        string responseText = HeaderValueHelper.HeaderTemp_ApiUniqueRouteName() + Environment.NewLine + Environment.NewLine;
                        responseText += myApiConfig?.Develop_TypeScriptClassDefine;

                        await context.Response.WriteAsync(responseText);
                        return;
                    }

                    if (ex == null)
                        return;

                    var converResult = ConvertToApiModel(ex, context.Request);

                    int statusCode = converResult.StatusCode;
                    ApiResponseErrorModel res = converResult.Response;

                    string tableName = SettingHelper.ApiRequestLogTableName_Error;

                    //寫log writeLog日誌
                    var service = IOCHelper.Resolve<IApiRequestLogService>();
                    service.CreateErrorLogTryCatch(tableName, converResult.LogModel);

                    bool isRedirect = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest().IsRedirectToErrorPage;

                    //根據不同狀況情境。可能會返回跟標準模式不同的model結構
                    if(IsQTechAPIUrl(context))
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";

                        var QTechResult = new
                        {
                            code = "UNKNOWN_ERROR",
                            message = res.Message
                        };

                        await context.Response.WriteAsync(QTechResult.ConvertToJsonString());
                    }
                    else if (GetApiSubCategory() == ApiConfigSubCategoryEnum.ReverseAPI_Notix)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var reverseAPIResult = new
                        {
                            code = $"{res.Code}_ERROR",
                            message = res.Message
                        };

                        await context.Response.WriteAsync(reverseAPIResult.ConvertToJsonString());
                    }
                    else if (!isRedirect)
                    {
                        context.Response.StatusCode = statusCode;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(res.ConvertToJsonString());
                    }
                    else
                    {
                        context.Response.Redirect($"/error?code={res.Code}&msg={res.Message.UrlEncode()}");
                    }

                });
            });
        }

      

        public static (ApiResponseErrorModel Response, ApiRequestLogEntity LogModel, int StatusCode) ConvertToApiModel(Exception ex, HttpRequest request)
        {
            bool isSkipWriteStackTrace = false;
            int statusCode = 400;
            var myDefineException = ex as TerproException;

            //紀錄客製的log
            var logModel = new ApiRequestLogEntity();
            HttpRequestLogHelper.SetRequestInfo(request, logModel, isException: true);

            var res = new ApiResponseErrorModel
            {
                Code = ApiErrorCode.InternalServerError.ToInteger(),
                Message = ex.GetMessageAndErrorLine(),
                Data = null
            };

            string uniqueIDText = "";

            if (logModel.UniqueID.IsNotEmptyTextNotNull())
            {
                uniqueIDText = $" LogID:`{logModel.UniqueID}` ";
            }

            if (myDefineException != null)
            {
                res.Code = myDefineException.ErrroCode.ToInteger();

                var debug_merchantID = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_Temperary_MerchantID);

                double timezone = 8;

                //若有header debug merchantID 會印出 (有利於debug)
                if (!string.IsNullOrWhiteSpace(debug_merchantID) || SettingHelper.IsVendorAPI)
                {
                    string debug_time = DateTimeHelper.GetCurrentTimeYYYY_MM_DD_HH_MM_SS(timezone);
                    string print_time_info = $"{debug_time} (GMT+{((int)timezone)})";
                    string uuidText = Guid.NewGuid().ToString().Truncate(6);

                    string myDebugText1 = $" {debug_merchantID} ／ {print_time_info} ／ {uuidText}";

                    if (myDefineException.ErrorDetail == null)
                    {
                        myDefineException.ErrorDetail = new ErrorDetailModel
                        {
                            DebugExtraInfo = myDebugText1,
                            DebugExtraInfo2 = GetRequestUrlAndQueryForDebug(request),
                        };
                    }
                    else
                    {
                        myDefineException.ErrorDetail.DebugExtraInfo = myDefineException.ErrorDetail.DebugExtraInfo + myDebugText1;
                        myDefineException.ErrorDetail.DebugExtraInfo2 = GetRequestUrlAndQueryForDebug(request);
                    }
                }

                if (myDefineException.ErrorDetail != null)
                {
                    var injectModel = myDefineException.ErrorDetail.ReturnInjecteObject<ErrorDetailModel>();

                    if (SettingHelper.IsProduction)
                    {
                        injectModel.LogExtraInfo = null;
                    }

                    res.Data = injectModel;
                }

                res.Message = MultiLanguageTextHelper.GetErrorCodeMsg(myDefineException.ErrroCode, myDefineException.ErrorDetail);

                //某些errorCode太基本 不需要紀錄stackTrace (浪費寫入空間)
                if ((res.Code >= 401001 && res.Code <= 401099) ||
                     res.Code == (int)ApiErrorCode.Forbidden_RequestTooManyTimes)
                {
                    isSkipWriteStackTrace = true;
                }
                else if (res.Code == ApiErrorCode.WalletTranferFail_MerchantCreditAmountNotEnough.ToInteger() ||
                        res.Code == ApiErrorCode.NotFoundPlayer.ToInteger() ||
                        res.Code == ApiErrorCode.InvalidStatus_MerchantStatus.ToInteger())
                {
                    isSkipWriteStackTrace = true;
                }

                bool isSqlInjectionCheck = IsSqlInjectionRisk(res, request);

                bool isAlwaysNotifyToChannel = ((ApiErrorCode)res.Code).IsAttributeExist<ErrorAlwaysNotifyAttribute>();

                bool isUrgentWarning = false;

                if (isSqlInjectionCheck)
                {
                    isAlwaysNotifyToChannel = true;
                    isUrgentWarning = true;
                }

                //符合其中一種條件即發通知
                if ((myDefineException.ErrorDetail != null && (myDefineException.ErrorDetail.IsAlwaysNotify || myDefineException.ErrorDetail.IsAlwaysNotifySlackOnly))
                    || isAlwaysNotifyToChannel)
                {
                    string errorMsg = myDefineException.GetMessageAndErrorLine();
                    string machineName = RuntimeHelper.RuntimeID;
                    string extraInfo = string.Empty;

                    if (SettingHelper.IsAdminSysAPI)
                    {
                        var userInfo = GetAdminSysUserModelFromJwtHeader(request);
                        extraInfo = $"{Environment.NewLine}用戶:{userInfo?.LoginUserID} ";
                    }

                    if (isSqlInjectionCheck)
                    {
                        extraInfo += "⚠️此請求有SQL injection風險";
                    }

                    string myMsg2 = $" `{request.Path}` {extraInfo} `{machineName}` {uniqueIDText} {errorMsg}";

                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                    {
                        Channel = GetNotifySlackChannel(),
                        Message = myMsg2,
                    });

                    //只有更重要的才會通知到TG
                    if (myDefineException.ErrorDetail.IsAlwaysNotify || isUrgentWarning)
                    {
                        MyTelegramUtility.SendMessage(new TelegramMessageParameter
                        {
                            TelegramChannelEnum = TelegramChannelEnum.pgs_common_error,
                            Message = myMsg2
                        });
                    }
                }

                DataModelConvertHelper.SetDebugInfoByException(logModel, myDefineException);

                //後台API有特殊處理機制 (針對風險異常攻擊)
                ExtraLogicAdminSysError(request, myDefineException);
            }
            else
            {
                //C# 一般Exception錯誤 (非標準ApiErrorException)
                int rndText = RandomHelper.GetRandIntegerNumbers(1000, 9999);

                //如果是production環境 錯誤訊息不顯示太直白
                if (SettingHelper.IsProduction)
                {
                    if (ex.IsSqlTimeoutException())
                    {
                        logModel.DebugInfo3 = res.Message;
                        res.Message = MultiLanguageTextHelper.DefaultTimeoutErrorMsg();
                    }
                    else
                    {
                        string timeText = DateTimeHelper.GetCurrentTimeYYYY_MM_DD_HH_MM_SS(8);

                        logModel.DebugInfo3 = rndText.ToString() + " " + res.Message;
                        res.Message = MultiLanguageTextHelper.DefaultErrorMsg(rndText.ToString()) + $"｜{timeText}";
                    }
                }

                SlackNotifyWhenCSharpException(ex, request.Path, request , logModel);
            }

            logModel.StackTrace = isSkipWriteStackTrace ? "" : ex.StackTrace;
            logModel.ResponseStatusCode = statusCode;

            DataModelConvertHelper.SetDebugInfoBeforeWriteLog(logModel, res);
            logModel.ExecutionTime = GetRequestExecuteTime(request);


            return (res, logModel, statusCode);
        }


        private static string GetRequestUrlAndQueryForDebug(HttpRequest request)
        {
            return request.GetRequestUrlForVendorAPIDebug(maxDisplayLength: 75);
        }


        private static void ExtraLogicAdminSysError(HttpRequest request, TerproException ex)
        {
            if (!SettingHelper.IsAdminSysAPI)
                return;

            bool isRiskErrorCode = false;

            if (ex.ErrroCode == ApiErrorCode.InvalidParameter_OrderByColumn ||
               ex.ErrroCode == ApiErrorCode.NotFoundPageOrAPiUrlPathLengthInvalid)
            {
                isRiskErrorCode = true;
            }

            if (!isRiskErrorCode)
                return;

            try
            {
                var currentUser = GetAdminSysUserModelFromJwtHeader(request);

                if (currentUser == null || currentUser.LoginUserID <= 0)
                    return;

                int userID = currentUser.LoginUserID;

                var myService = new BaseAdminSysService();
                myService.CounterRiskUserAndThrowException(userID, ex);
            }
            catch
            {


            }
        }


        private static AdminUserTokenJwtModel GetAdminSysUserModelFromJwtHeader(HttpRequest request)
        {
            string jwt = request.GetHeaderValueByKey(SystemConstant.HeaderKey_AccessToken);
            AdminUserTokenJwtModel userJwtModel = MyJwtHelper.GetAdminUserModelFromJwtString(jwt);
            return userJwtModel;
        }



        private static int GetRequestExecuteTime(HttpRequest request)
        {
            try
            {
                if (request.Headers.ContainsKey(SystemConstant.HeaderKey_RequestBeginTime))
                {
                    var beginTimeText = request.Headers[SystemConstant.HeaderKey_RequestBeginTime];

                    if (!string.IsNullOrWhiteSpace(beginTimeText))
                    {
                        DateTime beginTime = DateTime.Parse(beginTimeText);
                        DateTime endTime = DateTimeHelper.GetCurrentTime();

                        return ((int)(endTime - beginTime).TotalMilliseconds);
                    }

                }

            }
            catch
            {

            }

            return 0;
        }

        private static void SlackNotifyWhenCSharpException(Exception ex, string url, HttpRequest request, ApiRequestLogEntity logModel)
        {
            if (!SettingHelper.SlackNotify.IsNotifyEnable_When_SystemException)
                return;

            //定義特定的exception不發slack通知 因為很吵
            if (IsSkipSlackNotifyExceptionType(ex, url))
                return;

            string k8sName = RuntimeHelper.GetK8sInstanceName();
            string machineName = RuntimeHelper.RuntimeID;
            string extraInfo = string.Empty;

            var info1 = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.Header_LogInfo_UserIdentity);
            var info2 = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_IdentityMemo);

            if (info1.IsNotEmptyTextNotNull() || info2.IsNotEmptyTextNotNull())
            {
                extraInfo = $" `{info1} {info2}` ";
            }

            string queryString = "";

            if (ex.IsSqlTimeoutException())
            {
                queryString = $"{request.QueryString}";

                if (!string.IsNullOrWhiteSpace(queryString))
                {
                    queryString = $" ```{queryString}``` ";
                }
            }

            string extraNotfiyMsg = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_ExtraNotifyMsg);
            string debugInfo2 = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_DebugInfo2);

            string uniqueIDText = "";

            if (logModel.UniqueID.IsNotEmptyTextNotNull())
            {
                uniqueIDText = $" LogID:`{logModel.UniqueID}` ";
            }

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = GetNotifySlackChannel(),
                Message = $" `{url}` `{machineName}` `{k8sName}` {uniqueIDText} {extraNotfiyMsg} `{debugInfo2}` {extraInfo}, {queryString}" + ex.GetMessageAndErrorLine(),
            });
        }


        public static bool IsSkipSlackNotifyExceptionType(Exception ex, string url)
        {
            string currentEnv = SettingHelper.Env;

            string errorMsg = ex?.Message ?? "";

            //3092 Unexpected end of request content.， 
            if (errorMsg.Contains("Unexpected") && errorMsg.Contains("end of") && errorMsg.Contains("content"))
            {
                return true;
            }

            if (SettingHelper.IsDev)
            {
                //dev有兩種 env名稱
                currentEnv = SystemConstant.Env_Name_Development;
            }

            //針對特地幾隻API 會忽略timeout 錯誤通知
            bool isDefineEnv = SkipErrorNotifyApis.ContainsKey(currentEnv);

            if (!isDefineEnv)
                return false;

            var defineApis = SkipErrorNotifyApis[currentEnv];

            //是否為定義要關閉錯誤通知的API
            if (!defineApis.Any(keyword => url.Contains(keyword)))
                return false;

            bool isSQLException = IsPgsDefinedSqlException(ex);
            bool isTimeoutException = ex.Message.Contains("Timeout");

            if (isSQLException || isTimeoutException)
                return true;

            return false;
        }


        private static bool IsPgsDefinedSqlException(Exception ex)
        {
            var sqlEx = ex as SqlException;

            if (sqlEx == null)
                return false;

            if (ex.IsSqlTimeoutException())
                return true;

            if (ex.IsSqlUniqueException())
                return true;

            return false;
        }

        private static ApiConfigSubCategoryEnum GetApiSubCategory()
        {
            var subCategory = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest().SubCategory;
            return subCategory;
        }


        private static bool IsSqlInjectionRisk(ApiResponseErrorModel error, HttpRequest request)
        {
            //這個範圍內歸類為parameter錯誤
            int errorRange_begin = 403000;
            int errorRange_end = 403135;

            if (error.Code <= errorRange_begin)
                return false;

            if (error.Code > errorRange_end)
                return false;

            string body = request.GetRequestBodyString();

            if (RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword(body))
            {
                return true;
            }

            return false;
        }

        public static SlackChannelEnum GetNotifySlackChannel()
        {
            string channelName = SettingHelper.SlackNotify.NotifyChannel_When_SystemException;

            if (string.IsNullOrWhiteSpace(channelName))
                return SlackChannelEnum.n_error_pgs_common;

            return GetValidChannelName(channelName);
        }


        private static SlackChannelEnum GetValidChannelName(string channelName)
        {
            return ExtensionOfEnum.GetEnumByTextValue<SlackChannelEnum>(channelName, SlackChannelEnum.n_error_game_api);
        }

        private static bool IsQTechAPIUrl(HttpContext context)
        {
            try
            {
                string myUrl = context?.Request?.Path ?? "";

                if (myUrl.ContainsWihtoutCase("qtech/game-result") ||
                    myUrl.ContainsWihtoutCase("qtech/game-launch"))
                {
                    return true;
                }
            }
            catch
            {

            }

            return false;
        }

    }



}
