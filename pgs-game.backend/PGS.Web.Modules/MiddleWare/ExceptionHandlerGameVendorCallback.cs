using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Models;
using target.Models.Models;
using target.Utility.Helper;
using System;
using System.Linq;
using target.Utility;
using target.Utility.IOC;
using target.BL.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using example.invalid;
using target.Models.Table;
using target.Utility.Helper.Dependency;
using target.Utility.Utility.ThirdParty;
using Microsoft.AspNetCore.Diagnostics;
using System.Data.SqlClient;
using System.Collections.Generic;
using target.Models.Enums;
using target.BL.Services;
using target.Utility.Extensions.Web;
using target.Models.ViewModels;

namespace target.Web.Modules.Filter
{

    /// <summary>
    /// 遊戲商回調API 專用的exception處理
    /// API.ExternalVendor 使用
    /// </summary>
    public static class ExceptionHandlerGameVendorCallback
    {

        public static void UseGameVendorCallbackExceptionHandler(
            this IApplicationBuilder appBuilder,
            Func<IApiRequestLogService> apiRequestLogServiceFactory = null)
        {
            apiRequestLogServiceFactory = apiRequestLogServiceFactory
                ?? (() => IOCHelper.Resolve<IApiRequestLogService>());

            appBuilder.UseExceptionHandler(app =>
            {
                app.Run(async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    if (ex == null)
                        return;

                    string logIDText = "";

                    try
                    {
                        //紀錄客製的log
                        var logModel = new ApiRequestLogEntity();
                        HttpRequestLogHelper.SetRequestInfo(context.Request, logModel, isException: true);

                        if(logModel.UniqueID.IsNotEmptyTextNotNull())
                        {
                            logIDText = $"LogID: {logModel.UniqueID}";
                        }

                        bool isSkipWriteStackTrace = false;
                        var customizeException = ex as TerproException;

                        var res = new ApiResponseErrorModel
                        {
                            Code = ApiErrorCode.InternalServerError.ToInteger(),
                            Message = ex.GetMessageAndErrorLine(),
                            Data = null
                        };

                        if (customizeException != null)
                        {
                            res.Code = customizeException.ErrroCode.ToInteger();
                            res.Data = customizeException.ErrorDetail;
                            res.Message = MultiLanguageTextHelper.GetErrorCodeMsg(customizeException.ErrroCode, customizeException.ErrorDetail);

                            //某些errorCode太基本 不需要紀錄stackTrace (浪費寫入空間)
                            if (res.Code >= 401001 && res.Code <= 401099)
                            {
                                isSkipWriteStackTrace = true;
                            }

                            if (customizeException.ErrorDetail != null)
                            {
                                if (customizeException.ErrorDetail.IsAlwaysNotify || customizeException.ErrorDetail.IsAlwaysNotifySlackOnly)
                                {
                                    string errorMsg = customizeException.Message;
                                    string myMsg = $" `{context.Request.Path}` `{logIDText}` {errorMsg}";
                                    SendMessageToChannel(myMsg, GetNotifySlackChannel());
                                }
                            }
                        }
                        else
                        {
                            int rndText = RandomHelper.GetRandIntegerNumbers(1000, 9999);

                            //如果是production環境 錯誤訊息不顯示太直白
                            if (SettingHelper.IsProduction)
                            {
                                if (string.IsNullOrWhiteSpace(logModel.DebugInfo3))
                                {
                                    //這一段還是優先存單號吧，反之則是紀錄 明文錯誤訊息
                                    logModel.DebugInfo3 = rndText.ToString() + " " + res.Message;
                                }

                                res.Message = MultiLanguageTextHelper.DefaultErrorMsg(rndText.ToString());
                            }

                            SlackNotifyWhenCSharpException(ex, context.Request.Path , logIDText);
                        }

                        //會根據APIConfig定義的 遊戲商不同 轉成各自遊戲商所需要的格式
                        SingleWalletExceptionHandleModel customizedResponse = ConvertApiResponseModelByConfigDefine(res, customizeException?.ErrorDetail?.ExtraData);

                        DataModelConvertHelper.SetDebugInfoBeforeWriteLog(logModel, res);
                        DataModelConvertHelper.SetDebugInfoByException(logModel, customizeException);

                        int statusCode = customizedResponse.StatusCode;

                        //默認是會轉成camelCase (不吃JsonProperty)
                        string responseText = customizedResponse.GameVendorCustomizedResponse == null ? "" : customizedResponse.GameVendorCustomizedResponse.ConvertToJsonStringCamelCase();

                        if (customizedResponse.IsNotCamelCaseFormat)
                        {
                            responseText = customizedResponse.GameVendorCustomizedResponse.ConvertToJsonString();
                        }

                        bool isForceResponseSpecificText = !string.IsNullOrWhiteSpace(customizedResponse.ForceResponseSpecificText);

                        if (isForceResponseSpecificText)
                        {
                            responseText = customizedResponse.ForceResponseSpecificText;
                        }

                        logModel.StackTrace = isSkipWriteStackTrace ? "" : ex.StackTrace;
                        logModel.ResponseStatusCode = statusCode;
                        logModel.ResponseText = responseText;

                        //externalAPI 只會紀錄遊戲商格式的erorrResponse 因此另開欄位紀錄pgs error
                        //2025/02 移除這段。因為會害我無法debug真正原因
                        //logModel.DebugInfo3 = res.ConvertToJsonString();
                        logModel.ExecutionTime = GetRequestExecuteTime(context.Request);

                        //寫log
                        string tableName = SettingHelper.ApiRequestLogTableName_Error;
                        var service = apiRequestLogServiceFactory();
                        service.CreateErrorLogTryCatch(tableName, logModel);

                        context.Response.StatusCode = statusCode;
                        context.Response.ContentType = "application/json";

                        if (customizedResponse.IsXmlFormat)
                        {
                            context.Response.ContentType = "application/xml";
                        }

                        await context.Response.WriteAsync(responseText);
                    }
                    catch (Exception myEx)
                    {
                        //理論上應該不會有錯誤?
                        string msg = "UseSingleWalletVendorExceptionHandler錯誤啦_" + myEx.Message;
                        MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                        {
                            Message = msg,
                            TelegramChannelEnum = TelegramChannelEnum.pgs_single_wallet_erorr
                        });
                    }

                });
            });
        }


        private static SingleWalletExceptionHandleModel ConvertApiResponseModelByConfigDefine(ApiResponseErrorModel model, object extraData)
        {
            var apiCategory = ApiConfigDefineHelper.GetApiConfigSettingByCurrentRequest().ApiCategory;

            int apiCategoryValue = apiCategory.ToInteger();

            if (apiCategoryValue <= 10000)
            {
                return new SingleWalletExceptionHandleModel
                {
                    StatusCode = 400,
                    GameVendorCustomizedResponse = model
                };
            }

            GameVendorTypeEnum convertToVendorID = (GameVendorTypeEnum)apiCategory.ToInteger();

            if (!convertToVendorID.IsDefinedValue())
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);

            var vendorService = ServiceInstanceHelper.InstanceVendorService(convertToVendorID);

            var myParam = new GameVendorExceptionParsingParameter
            {
                ApiResponseError = model,
                ExtraData = extraData
            };

            return vendorService.FormatParsing_ParseExceptionModelToVendorApiResponse(myParam);
        }


        private static void SlackNotifyWhenCSharpException(Exception ex, string url , string logIDText)
        {
            if (!SettingHelper.SlackNotify.IsNotifyEnable_When_SystemException)
                return;

            if (IsSkipChannelNotify(ex))
                return;

            //如果是C#原生的exception
            SlackChannelEnum enumParse = GetNotifySlackChannel();

            string msg = $" `{url}` , `{logIDText}` ," + ex.GetMessageAndErrorLine();
            SendMessageToChannel(msg, enumParse);
        }


        private static void SendMessageToChannel(string msg, SlackChannelEnum targetChannel)
        {
            string debugInfo = $" {HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_DebugInfo2)} {HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_DebugInfoForException)} ";
            string k8sName = RuntimeHelper.GetK8sInstanceName();

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = targetChannel,
                Message = debugInfo + $" `{k8sName}` " + msg,
            });
        }


        private static bool IsSkipChannelNotify(Exception ex)
        {
            string errorMsg = ex.GetMessageAndErrorLine();

            //3092 Unexpected end of request content.， 
            if (errorMsg.Contains("Unexpected") && errorMsg.Contains("end of") && errorMsg.Contains("content"))
            {
                return true;
            }

            return false;
        }


        private static SlackChannelEnum GetNotifySlackChannel()
        {
            string channelName = SettingHelper.SlackNotify.NotifyChannel_When_SystemException;

            if (string.IsNullOrWhiteSpace(channelName))
                return SlackChannelEnum.n_error_pgs_common;

            return GetValidChannelName(channelName);
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


        private static SlackChannelEnum GetValidChannelName(string channelName)
        {
            return ExtensionOfEnum.GetEnumByTextValue<SlackChannelEnum>(channelName, SlackChannelEnum.n_error_game_api);
        }

    }



}
