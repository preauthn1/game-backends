using Lck.Utility.Extensions.Helper;
using Org.BouncyCastle.Asn1.Ocsp;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.BL.Services
{
    public partial class ApiRequestLogService : BaseService, IApiRequestLogService
    {
      
        public ApiRequestLogRepository ApiRequestLogRepository { get; set; }

        public ApiRequestLogV2024Repository ApiRequestLogV2024Repository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public SingleWalletMerchantApiRequestLogRepository SingleWalletMerchantApiRequestLogRepository { get; set; }

        public SingleWalletMerchantApiRequestLogBriefRepository SingleWalletMerchantApiRequestLogBriefRepository { get; set; }

        private static string _k8sInstanceName = "";


        public ApiRequestLogService()
        {
            SetK8SInstanceName();
        }


        public void CreateTryCatch(string tableName, ApiRequestLogEntity model)
        {
            try
            {
                SetExtraProperty(model);

                var extraInfo = DataModelConvertHelper.InstanceLogQueueExtraModel(tableName);

                WriteApiRequestLogToStorage(new List<ApiRequestLogQueueTempModel>
                    {
                        new ApiRequestLogQueueTempModel
                        {
                            ApiRequestLogEntity = model,
                            ExtraInfoV2 = extraInfo
                        }
                    });                
            }
            catch (Exception ex)
            {
                if (SettingHelper.IsMSTest)
                {
                    throw;
                }

                string extraInfo = $" {model.RelativeUrl} `{RuntimeHelper.GetK8sInstanceName()}` " + ex.Message;
                string msg = ex.GetCommonNotifyMessage("ApiRequestLog CreateTryCatch", extraInfo);

                //到底是不是有可能timeout呢 (很常態?)
                if (SettingHelper.IsProduction)
                {
                    MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                    {
                        Message = msg,
                        TelegramChannelEnum = TelegramChannelEnum.pgs_common_error
                    });
                }
            }
        }

        public void CreateErrorLogTryCatch(string tableName, ApiRequestLogEntity model)
        {           
            if (TableNameConvertHelper.IsEnableElasticLogByTableName(tableName))
            {
                int logTableType = SettingHelper.ApiRequestLog_ErrorLogTableType;

                if (logTableType > 0)
                {
                    CreateErrorLogTryCatch_V2((ElasticLogTableTypeEnum)logTableType, model);
                }
                else
                {
                    CreateErrorLogTryCatch_V1(tableName, model);
                }
            }
            else
            {
                CreateErrorLogTryCatch_V1(tableName, model);
            }
        }


        private void CreateErrorLogTryCatch_V1(string tableName, ApiRequestLogEntity model)
        {
            try
            {
                SetExtraProperty(model);

                ApiRequestLogRepository.CreateToTargetTable(tableName, model);
            }
            catch (Exception ex)
            {
                if (SettingHelper.IsMSTest)
                {
                    throw;
                }

                string extraInfo = $" {model.RelativeUrl} `{RuntimeHelper.GetK8sInstanceName()}` " + ex.Message;
                string msg = ex.GetCommonNotifyMessage("ApiRequestLog CreateErrorLogTryCatch", extraInfo);
            }
        }

        /// <summary>
        /// 2026/02 特定table採用新版儲存方式
        /// </summary>
        private void CreateErrorLogTryCatch_V2(ElasticLogTableTypeEnum tableType, ApiRequestLogEntity model)
        {
         
            try
            {
                SetExtraProperty(model);

                var tranNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
                int merchantID = 0;

                //若有填寫單號 則用自己填的為主
                if(RandomHelper.CheckIsPgsUniqueSequenceOrderNoAndRecentDays(model.UniqueID).IsPgsOrderNo)
                {
                    tranNo = model.UniqueID;
                }

                string headerMerchantID = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_Temperary_MerchantID);
                string referOrderNo = model.DebugInfo3;

                if (headerMerchantID.IsNumbericOfPositiveInt32())
                {
                    merchantID = Int32.Parse(headerMerchantID);
                }

                EazyQueueApiErrorLogHelper.Append(new ApiRequestLogQueueTempModel
                {
                    ApiRequestLogEntity = model,
                    ExtraInfoV2 = new ApiRequestLogQueueExtraInfoV2026Model
                    {
                        MerchantID = merchantID,
                        ErrorCode = model.ExtraDebug_ErrorCode,
                        ReferOrderNo = referOrderNo,
                        UniqueID = tranNo,
                        TableType = tableType
                    }                 
                });
            }
            catch (Exception ex)
            {
                if (SettingHelper.IsMSTest)
                {
                    throw;
                }

                string extraInfo = $" {model.RelativeUrl} `{RuntimeHelper.GetK8sInstanceName()}` " + ex.Message;
                string msg = ex.GetCommonNotifyMessage("ApiRequestLog CreateErrorLogTryCatch_V2", extraInfo);
            }
        }


        /// <summary>
        /// 一筆一筆寫入太浪費sql conn了
        /// 因此提供另一種機制: 每N秒統一寫一批進去DB
        /// </summary>
        public void WriteToQueueBuffering(ApiRequestLogQueueTempModel model)
        {
            //relative長度異常。可能是爬蟲或奇怪的惡意請求 直接忽略
            if (SettingHelper.IsProduction &&
                model.ApiRequestLogEntity.RelativeUrl.IsNotEmptyTextNotNull() &&
                model.ApiRequestLogEntity.RelativeUrl.Count() > 201)
            {
                return;
            }

            SetExtraProperty(model.ApiRequestLogEntity);

            EazyQueueApiRequestLogHelper.Append(model);
        }



        private bool IsSkipInsertByDBSetting(string myTableName)
        {
            var querySetting = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.Json_DisableWriteLog_TableName);

            if(string.IsNullOrWhiteSpace(querySetting.SettingValue))
            {
                return false;
            }

            myTableName = myTableName.Replace("[dbo].", "");
            myTableName = myTableName.Replace("[", "").Replace("]", "").Trim();

            foreach (var table in querySetting.SettingValueOfListString)
            {
                if(table == myTableName)
                {
                    return true;
                }
            }

            return false;
        }


        private MappingTagValuesEntity InstanceMappingValueEntity(ApiRequestLogV2025Entity convert , ApiReqeustLogTagValueMappingTypeEnum type , string settingValue)
        {
            return new MappingTagValuesEntity
            {
                SettingValue = settingValue,
                PartitionTime = convert.PartitionTime,
                UniqueID = convert.UniqueID,
                SettingKey = type.ToInteger(),
            };
        }



        private void InsertDataToDB_CommonLogTable(List<ApiRequestLogQueueTempModel> myItems)
        {
            string logTableName = SettingHelper.ApiRequestLogTableName;

            var lisfOfInsert1 = new List<ApiRequestLogEntity>();
            var lisfOfInsert1_2 = new List<ApiRequestLogEntity>();

            foreach (var item in myItems)
            {               
                if (IsSkipInsertByDBSetting(logTableName))
                {
                    continue;
                }

                var convert = item.ApiRequestLogEntity.ReturnInjecteObject<ApiRequestLogEntity>();
                convert.WebSiteName = RuntimeHelper.GetK8sInstanceName();
                convert.ExecutionTime = item.ApiRequestLogEntity.ExecutionTime.ParseNullToZero();

                lisfOfInsert1.Add(convert);
            }

            // Step 1. 主表寫入
            ApiRequestLogRepository.BulkInsert(logTableName, lisfOfInsert1, IsolationLevel.ReadUncommitted);
        }


        private void SendMessageToChannel(string msg)
        {
            string k8sName = RuntimeHelper.GetK8sInstanceName();
            string machineName = RuntimeHelper.RuntimeID;
            msg = $" {machineName} {k8sName} " + msg;

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_error_pgs_common,
                Message = msg
            });
        }

        private void SetExtraProperty(ApiRequestLogEntity model)
        {
            var now = DateTimeHelper.GetCurrentTime();
            model.CreateTime = now;
            model.CreateTimeCategory = 0;
            model.IPAddress = IPHelper.GetIP();
            model.WebSiteName = SettingHelper.WebSiteName;
            model.Memo = _k8sInstanceName;
            model.RelativeUrl = model.RelativeUrl.Truncate(200);

            if(model.DebugInfo2.CountOrZero() <= 200)
            {
                model.DebugInfo2 += model.StackTrace;
                model.DebugInfo2 = model.DebugInfo2.Truncate(2000);
            }
        }


        private void SetK8SInstanceName()
        {
            if (string.IsNullOrWhiteSpace(_k8sInstanceName))
            {
                _k8sInstanceName = RuntimeHelper.GetK8sInstanceName(isInstanceNameOnly: true);

                if (!string.IsNullOrWhiteSpace(_k8sInstanceName))
                {
                    _k8sInstanceName = _k8sInstanceName.Replace("backend-api-", "");
                }
            }
        }

    }



}
