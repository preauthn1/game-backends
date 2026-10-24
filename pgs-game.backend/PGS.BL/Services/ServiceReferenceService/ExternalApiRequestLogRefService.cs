using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Utility.Utility;
using target.Utility;

namespace target.BL.Services
{

    public class ExternalApiRequestLogRefService : BaseService, IExternalApiRequestLogRefService
    {
        public ExternalApiRequestLogRepository ExternalApiRequestLogRepository { get; set; }

        public ExternalApiOtherRequestLogRepository ExternalApiOtherRequestLogRepository { get; set; }

        public SingleWalletMerchantApiRequestLogRepository SingleWalletMerchantApiRequestLogRepository { get; set; }

        public SingleWalletMerchantApiRequestLogBriefRepository SingleWalletMerchantApiRequestLogBriefRepository { get; set; }



        public void Create(ApiRequestLogTemporaryInfoModel model)
        {
            try
            {
                //先用BWG做個嘗試
                if (TableNameConvertHelper.IsEnableElasticLogByTable(ElasticLogTableTypeEnum.ExternalApiRequestlog))
                {
                    WriteThirdPartyRequestLogToQueue(model);
                }
                else
                {
                    var insertModel = InstanceInsertEntity(model);
                    ExternalApiRequestLogRepository.Create(insertModel);
                }
            }
            catch (Exception ex)
            {
                string msg = $"ExternalApiRequestLog create 失敗 , {ex.GetMessageAndErrorLine()}";

                if (SettingHelper.IsLocalhostWebSiteName)
                {
                    SendMessageToMultipleChannel(msg);
                }
            }
        }



        private void WriteThirdPartyRequestLogToQueue(ApiRequestLogTemporaryInfoModel model)
        {
            var logInfo = model.RequestParameters?.AdvancedSetting ?? new AdvancedSetting();
            var defaultLogInfo = logInfo ?? new AdvancedSetting();

            var tranResult = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();

            string requestUrl = model.RequestParameters.GetFullUrl();

            var insertModel = new BriefApiRequestLogThirdPartyEntity
            {
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ResponseTime = (int)model.Response.ResponseTime,
                ResponseStatusCode = model.Response.ResponseStatusCode,
                RelativeUrl = model.RequestParameters.GetRelativeUrl().Truncate(50),
                Category = (logInfo?.Log_ApiCategory ?? "").Truncate(30),
                SubCategory = (logInfo?.Log_SubCategory ?? "").Truncate(30),
                MerchantID = logInfo.Log_MerchantID,
                PlayerID = logInfo.Log_PlayerID,
                ReferOrderNo = logInfo?.Log_ReferenceTransactionNo,
                UniqueID = tranResult.TransactionNo,
            };

            //舊版的邏輯都是填寫在這個欄位中 嘗試解析
            if (insertModel.MerchantID == 0 &&
               RegexRuleHelper.IsMerchantID(logInfo.Log_DebugInfo1))
            {
                insertModel.MerchantID = Int32.Parse(logInfo.Log_DebugInfo1);
            }

            var insertLog = insertModel.ReturnInjecteObject<FullInfoThirdPartyApiRequestLogESModel>();

            string text_payload = model.RequestContent;
            string text_headers = ToKeyValueDicJsonString(model.RequestParameters);
            string text_response = defaultLogInfo.Log_IsSkipWriteResponseText ? "**skip insert**" : model.Response.ResponseContent;

            insertLog.RequestContent = text_payload;
            insertLog.RequestHeader = text_headers;
            insertLog.ResponseText = text_response;
            insertLog.WebSiteName = SettingHelper.WebSiteName;
            insertLog.RequestUrlFull = requestUrl;

            EazyQueueThirdAPIRequestLogHelper.Append(insertLog);
        }


        public void CreateToOtherTable(ApiRequestLogTemporaryInfoModel model)
        {
            try
            {
                var insertModel = InstanceInsertEntity(model);
                ExternalApiOtherRequestLogRepository.Create(insertModel);
            }
            catch (Exception ex)
            {
                string msg = $"ExternalApiOtherRequestLog create 失敗 , {ex.GetMessageAndErrorLine()}";

                if (SettingHelper.IsLocalhostWebSiteName)
                {
                    SendMessageToMultipleChannel(msg);
                }
            }
        }



        public SingleWalletMerchantApiRequestLogEntity ConvertToSwCallbackLogEntity(ApiRequestLogTemporaryInfoModel model)
        {
            try
            {
                string relativeUrl = model.RequestParameters.GetRelativeUrl();

                var logInfo = model.RequestParameters?.AdvancedSetting;

                string transactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

                var insertModel = new SingleWalletMerchantApiRequestLogEntity
                {
                    UniqueID = transactionNo,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    ResponseText = model.Response.ResponseContent,
                    ResponseTime = (int)model.Response.ResponseTime,
                    ResponseStatusCode = model.Response.ResponseStatusCode,
                    RequestUrl = model.RequestParameters.GetFullUrl(),
                    RequestContent = model.RequestContent,
                    RequestHeader = ToKeyValueDicJsonString(model.RequestParameters),
                    Memo = string.Empty,
                    WebSiteName = SettingHelper.WebSiteName,
                    Category = (logInfo?.Log_ApiCategory ?? "").Truncate(30),
                    SubCategory = (logInfo?.Log_SubCategory ?? "").Truncate(50),
                    RequestID = logInfo?.Log_RequestUUID.Truncate(50),
                    DebugInfo = logInfo.Log_DebugInfo1,
                    DebugInfo2 = logInfo.Log_DebugInfo2,
                    DebugInfo3 = logInfo.Log_DebugInfo3,
                    MerchantID = logInfo.Log_CategoryIntValue,
                    ReferenceTransactionNo = logInfo.Log_ReferenceTransactionNo,
                };

                SingleWalletMerchantApiRequestLogRepository.SetExtraProperty(insertModel);
                return insertModel;
            }
            catch (Exception ex)
            {
                string msg = $"ConvertToSwCallbackLogEntity 失敗 , {ex.GetMessageAndErrorLine()}";

                SendMessageToMultipleChannel(msg);
            }

            return null;
        }


        public void CreateSingleWalletApiLog(ApiRequestLogTemporaryInfoModel model)
        {
            try
            {
                var insertModel = ConvertToSwCallbackLogEntity(model);

                //即時寫入 或 背景定時同步
                if (SettingHelper.SWCallbackLogBackgroundSyncEnable)
                {
                    EazyQueueSwCallbackLogHelper.Append(insertModel);
                }
                else
                {
                    var listOfInsert = new List<SingleWalletMerchantApiRequestLogEntity>();
                    listOfInsert.Add(insertModel);

                    SingleWalletMerchantApiRequestLogRepository.BulkInsertToMultipleStorage(listOfInsert, SingleWalletMerchantApiRequestLogBriefRepository);
                }
            }
            catch (Exception ex)
            {
                string msg = $"CreateSingleWalletApiLog 失敗 , {ex.GetMessageAndErrorLine()}";

                if (SettingHelper.IsLocalhostWebSiteName)
                {
                    SendMessageToMultipleChannel(msg);
                }
            }
        }


        private string ToKeyValueDicJsonString(RequestParameters request)
        {
            string method = request.Method.ToString().ToUpper().Replace("HTTP", "");

            if (request.Headers.IsEmptyOrNull())
            {
                var dic = new Dictionary<string, string>();
                dic.Add("Method", method);
                dic.Add("User-Agent", request.UserAgent);
                dic.Add("Content-Type", request.ContentType);
                return dic.ConvertToJsonString();
            }

            try
            {
                var dic = request.Headers?.ToDictionary(x => x.Key, z => z.Value) ?? new Dictionary<string, string>();
                dic.Add("Method", method);
                dic.Add("User-Agent", request.UserAgent);
                dic.Add("Content-Type", request.ContentType);

                return dic.ConvertToJsonString();
            }
            catch
            {

            }

            return null;
        }


        private ExternalApiRequestLogEntity InstanceInsertEntity(ApiRequestLogTemporaryInfoModel model)
        {
            var logInfo = model.RequestParameters?.AdvancedSetting;
            var defaultLogInfo = logInfo ?? new AdvancedSetting();

            return new ExternalApiRequestLogEntity
            {
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ResponseText = defaultLogInfo.Log_IsSkipWriteResponseText ? "**skip insert**" : model.Response.ResponseContent,
                ResponseTime = model.Response.ResponseTime,
                ResponseStatusCode = model.Response.ResponseStatusCode,
                RequestUrl = model.RequestParameters.GetFullUrl(),
                RequestContent = model.RequestContent,
                RequestHeader = ToKeyValueDicJsonString(model.RequestParameters),
                Memo = string.Empty,
                WebSiteName = SettingHelper.WebSiteName,
                Category = logInfo?.Log_ApiCategory ?? "",
                SubCategory = logInfo?.Log_SubCategory ?? "",
                RequestID = logInfo?.Log_RequestUUID.Truncate(50),
                DebugInfo = logInfo?.Log_DebugInfo1,
                DebugInfo2 = logInfo?.Log_DebugInfo2,
                DebugInfo3 = logInfo?.Log_DebugInfo3,
            };
        }

    }
}
