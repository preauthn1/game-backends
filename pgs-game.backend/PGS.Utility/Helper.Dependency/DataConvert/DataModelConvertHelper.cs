using game.Entity.Game;
using target.Models;
using target.Models.Attributes;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions;
using target.Utility.Helper.HttpContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace target.Utility.Helper
{
    public class DataModelConvertHelper
    {

        public static UserCreateModel ConvertToUserCreateModel(string accountWithPrefix , MerchantCopyCreateUserAccountExtraProperty model)
        {
            return new UserCreateModel
            {
                UserName = accountWithPrefix,
                DisplayName = accountWithPrefix,
                OrganizationID = model.OrganizationID,
                RoleID = model.RoleID,
                Password = [REDACTED],
                ConfirmPassword = [REDACTED],
                TimeZoneAreaID = TimeZoneAreaEnum.Singapore_0800.ToInteger(),
            };
        }


       



        public static List<CustomizedColumnDisplayColumnViewModel> ConvertToAllColumnsViewModel(Dictionary<string , object> dic)
        {
            var columns = dic
                    .Select(z => z.Key)
                    .Select(nameOfColumn => new CustomizedColumnDisplayColumnViewModel(nameOfColumn))
                    .ToList();

            return columns;
        }


        public static void ConvertToAllColumnsViewModel(CustomizedColumnDynamicQueryResultViewModel model)
        {
            if (model.Items.IsEmptyOrNull())
                return;

            model.ExtraInfo = model.ExtraInfo ?? new CustomizeQueryMethodResultModel();
            model.ExtraObject = model.ExtraObject ?? new CustomizedColumnExtraInfoViewModel();
            model.ExtraObject.Columns = ConvertToAllColumnsViewModel(model.Items.FirstOrDefault());

            var dic = new Dictionary<string, string>();

            //用一些懶人方法 做出很簡易的 欄位翻譯mapping
            var allEnums = ExtensionOfEnum.GetEnumAllValue<EazyLabelTextEnum>();

            foreach (var eazyLabel in allEnums)
            {
                dic.Add(eazyLabel.ToString().ToCamelCase(), eazyLabel.GetDescription());
            }

            model.ExtraObject.ColumnTranslateMapping = dic;
        }

        public static void SetDebugInfoBeforeWriteLog(ApiRequestLogEntity logModel, ApiResponseErrorModel error)
        {
            logModel.ResponseStatusCode = 400;
            logModel.DebugInfo = error.Code.ToString();
            logModel.ExtraDebug_ErrorCode = error.Code;
            logModel.ResponseText = error.ConvertToJsonString();
        }


        public static void SetDebugInfoByException(ApiRequestLogEntity logModel, TerproException errorException)
        {
            if (errorException == null)
                return;

            if (errorException.ErrorDetail == null)
                return;

            logModel.DebugInfo2 += errorException.ErrorDetail.LogExtraInfo;
            logModel.DebugInfo2 = logModel.DebugInfo2.Truncate(500);
        }



        public static SingleWalletMerchantApiRequestLogEntity ConvertToSingleWalletLogEntity(FullInfoSingleWalletCallbackRequestLogESModel model)
        {
            if(model == null)             
                return null;            

            var convert = model.ReturnInjecteObject<SingleWalletMerchantApiRequestLogEntity>();
            convert.RequestUrl = model?.RequestUrlFull;
            convert.MerchantID = model.MerchantID <= 0 ? (int?)null : model.MerchantID;
            convert.ReferenceTransactionNo = model.ReferOrderNo;

            if(string.IsNullOrWhiteSpace(convert.DebugInfo2) && model.PlayerID > 0)
            {
                convert.DebugInfo2 = model.PlayerID.ToString();
            }

            return convert;
        }

        public static SingleWalletMerchantApiRequestLogQueryResult ConvertToSingleWalletLogQueryResult(FullInfoSingleWalletCallbackRequestLogESModel model)
        {
            if (model == null)
                return null;

            var convert1 = ConvertToSingleWalletLogEntity(model);
            var convert2 = convert1.ReturnInjecteObject<SingleWalletMerchantApiRequestLogQueryResult>();
            convert2.ErrorCode = model.ErrroCode;

            return convert2;
        }


        public static ApiRequestLogErrprBriefEntity ConvertToBriefErrorLogEntity(ApiRequestLogQueueTempModel model)
        {
            var convert = model.ApiRequestLogEntity.ReturnInjecteObject<ApiRequestLogErrprBriefEntity>();
            convert.MerchantID = model.ExtraInfoV2.MerchantID;
            convert.PlayerID = model.ExtraInfoV2.PlayerID;
            convert.UniqueID = model.ExtraInfoV2.UniqueID;
            convert.ErrorCode = model.ExtraInfoV2.ErrorCode;
            convert.ExecutionTime = model.ApiRequestLogEntity.ExecutionTime.ParseNullToZero();
            convert.ReferOrderNo = model.ExtraInfoV2.ReferOrderNo;

            convert.RelativeUrl = convert.RelativeUrl.Truncate(100);
            convert.HttpMethod = convert.HttpMethod.Truncate(10);
            convert.IPAddress = convert.IPAddress.Truncate(50);
            convert.ReferOrderNo = convert.ReferOrderNo.Truncate(50);

            return convert;
        }


        public static FullInfoSingleWalletCallbackRequestLogESModel ConvertToSingleWalletFullInfoLogESModel(SingleWalletMerchantApiRequestLogEntity model , int? errorCode)
        {
            var convert = model.ReturnInjecteObject<FullInfoSingleWalletCallbackRequestLogESModel>();
            convert.RelativeUrl = model.RequestUrl.ParseUrlAndGetRelativeUrl();
            convert.RequestUrlFull = model.RequestUrl;
            convert.ErrroCode = errorCode ?? 0;

            return convert;

        }

        public static FullInfoApiRequestLogESModel ConvertToElasticFullRequestModel(ApiRequestLogQueueTempModel item)
        {
            var convertES = item.ApiRequestLogEntity.ReturnInjecteObject<FullInfoApiRequestLogESModel>();
            convertES.RequestHeader = item.ApiRequestLogEntity.Headers;
            convertES.ResponseText = item.ApiRequestLogEntity.ResponseText;
            convertES.RequestContent = item.ApiRequestLogEntity.RequestBody;
            convertES.RequestUrlFull = item.ApiRequestLogEntity.RequestUrl;
            convertES.Category = item.ApiRequestLogEntity.RelativeUrl;
            convertES.ResponseTime = item.ApiRequestLogEntity.ExecutionTime.ParseNullToZero();

            if (item.ExtraInfoV2 != null)
            {
                convertES.ReferOrderNo = item.ExtraInfoV2.ReferOrderNo;
                convertES.UniqueID = item.ExtraInfoV2.UniqueID;
            }
            
            if (item.ExtraInfoV2 != null && item.ExtraInfoV2.MerchantID > 0)
            {
                convertES.MerchantID = item.ExtraInfoV2.MerchantID;
            }
            else if (RegexRuleHelper.IsMerchantID(item.ApiRequestLogEntity.UserIdentity))
            {
                convertES.MerchantID = Int32.Parse(item.ApiRequestLogEntity.UserIdentity);
            }

            if (item.ExtraInfoV2 != null && item.ExtraInfoV2.PlayerID > 0)
            {
                convertES.PlayerID = item.ExtraInfoV2.PlayerID;
            }
            else if (item.ApiRequestLogEntity.IdentityKey.IsNumbericOfPositiveInt32())
            {
                int playerID = Int32.Parse(item.ApiRequestLogEntity.IdentityKey);

                if(playerID != convertES.MerchantID)
                {
                    convertES.PlayerID = playerID;
                }
            }

            return convertES;
        }

        public static ApiRequestLogQueueExtraInfoV2026Model InstanceLogQueueExtraModel(string tableName)
        {
            ApiRequestLogQueueExtraInfoV2026Model extraModel = new ApiRequestLogQueueExtraInfoV2026Model();

            var logTableType = TableNameConvertHelper.GetElasticsearchIndexLogType(tableName);

            if (TableNameConvertHelper.IsEnableElasticLogByTableName(tableName) || 
                logTableType == ElasticLogTableTypeEnum.ApiRequestLogExternalVendorApi)
            {
                //大部分的API站台不需要處理此，減少一點點記憶體空間浪費
                string referOrderNo = MyHttpContext.Current.Request.GetHeaderValueByKey(SystemConstant.HeaderKey_ReferOrderNo);
                extraModel.ReferOrderNo = referOrderNo;

                var orderResult = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();
                extraModel.UniqueID = orderResult.TransactionNo;

                string merchantIDText = MyHttpContext.Current.Request.GetHeaderValueByKey(SystemConstant.HeaderKey_Temperary_MerchantID);

                if(RegexRuleHelper.IsMerchantID(merchantIDText))
                {
                    extraModel.MerchantID = Int32.Parse(merchantIDText);
                }

                string playerIDText = MyHttpContext.Current.Request.GetHeaderValueByKey(SystemConstant.Header_LogInfo_UserIdentity);
                string playerIDText2 = MyHttpContext.Current.Request.GetHeaderValueByKey(SystemConstant.HeaderKey_PlayerID);

                if (playerIDText.IsNumbericOfPositiveInt32() && playerIDText != merchantIDText)
                {
                    extraModel.PlayerID = Int32.Parse(playerIDText);
                }
                else if (playerIDText2.IsNumbericOfPositiveInt32() && playerIDText2 != merchantIDText)
                {
                    extraModel.PlayerID = Int32.Parse(playerIDText2);
                }
            }

            return extraModel;
        }




        public static double GetTimeZoneByAreaID(int timeZoneArea)
        {
            TimeZoneAreaEnum timeZone = (TimeZoneAreaEnum)timeZoneArea;

            if (!timeZone.IsDefinedValue())
                timeZone = TimeZoneAreaEnum.China_0800;

            return GetTimeZoneByEnum(timeZone);
        }
        

        public static double GetTimeZoneByEnum(TimeZoneAreaEnum timeZoneArea)
        {
            try
            {
                string timeZoneValue = timeZoneArea.GetAttributeFromEnum<ExtraAttributeValue>()?.Value;
                return double.Parse(timeZoneValue);
            }
            catch
            {

            }

            return 8.0;
        }


        /// <summary>
        /// #001 override 函式
        /// 如果有新加欄位、下面另一個多載Function 可能也要改code (API.Job的部分)
        /// </summary>
        public static SingleWalletSettleCallbackParameter InstanceSingleWalletSettleCallbackParameter(PerGameDetailLogEntity convertPerGame, decimal walletUpdateAmount_payout, string requestUUID, SettleBetCallbackExtraInfo extraInfo)
        {
            return new SingleWalletSettleCallbackParameter
            {
                PlayerID = convertPerGame.PlayerID,
                PayoutAmount = walletUpdateAmount_payout,
                BetAmount = convertPerGame.BetAmount,
                GameID = convertPerGame.GameID,
                MerchantID = convertPerGame.MerchantID,
                TaxAmount = convertPerGame.TaxAmount,
                ValidAmount = convertPerGame.ValidAmount,
                BetRecordNoPGS = convertPerGame.TransactionNo,
                SettlementTime = convertPerGame.GameTime,
                PlayerName = convertPerGame.PlayerName,
                DebugProcessUUID = requestUUID,
                ExtraInfo = extraInfo
            };
        }

        /// <summary>
        /// #002 override 函式 (API.Job使用)
        /// 上面的Function也要記得改
        /// </summary>
        public static SingleWalletSettleCallbackParameter InstanceSingleWalletSettleCallbackParameter(BetRecordGeneralColumnModel queryBetReocrd, string requestUUID, SettleBetCallbackExtraInfo extraInfo)
        {
            return new SingleWalletSettleCallbackParameter
            {
                PlayerID = queryBetReocrd.PlayerID,
                TaxAmount = queryBetReocrd.TaxAmount,
                SettlementTime = queryBetReocrd.SettlementTime,
                BetRecordNoPGS = queryBetReocrd.BetRecordNoPGS,
                BetAmount = queryBetReocrd.BetAmount,
                GameID = queryBetReocrd.GameID,
                MerchantID = queryBetReocrd.MerchantID,
                PayoutAmount = queryBetReocrd.PayoutAmount,
                ValidAmount = queryBetReocrd.ValidAmount,
                DebugProcessUUID = requestUUID,
                PlayerName = queryBetReocrd.PlayerName,
                ExtraInfo = extraInfo
            };
        }


        public static void SetExtraValueByPerGameTags(SettleBetCallbackExtraInfo extraInfo, List<SystemTagEnum> perGameTags)
        {
            perGameTags = perGameTags ?? new List<SystemTagEnum>();

            extraInfo.IsBuyFeature = perGameTags.Any(z => z == SystemTagEnum.PerGameDetailLog_AGGBuyFeature);
            extraInfo.IsJackpot = perGameTags.Any(z => z == SystemTagEnum.PerGameDetailLog_Jackpot);
            return;
        }


        public static List<DropdownValueViewModel> ConvertEnumToDropdownModels<T>() where T : System.Enum
        {
            var enums = ExtensionOfEnum.GetEnumAllValue<T>();
            return enums.Select(enumValue => new DropdownValueViewModel
            {
                Value = enumValue.ToInteger().ToString(),
                ValueInt = enumValue.ToInteger(),
                Text = MultiLanguageTextHelper.GetEnumsCultureText(enumValue)
            })
            .ToList();
        }
           

    }
}
