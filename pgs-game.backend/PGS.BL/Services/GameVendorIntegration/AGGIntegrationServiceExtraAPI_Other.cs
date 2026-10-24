using DocumentFormat.OpenXml.Wordprocessing;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace target.BL.Services
{

    public partial class AGGIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService, IAGGInternalAPIService
    {
        private const string ColumnName_FreespinID = "freeSpinId";

        private const string ColumnName_FreespinUseStatus = "freeSpinUseStatus";

        private const string ColumnName_BudgetID = "budgetId";

        private const string ColumnName_ClientID = "clientId";

        private const string ColumnName_UserID = "userId";
        
        public AGGGameBettingTemplateAPIResponseModel GetGameBettingInfo(AGGGameBettingInfoReqeustParameter model)
        {           
            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/betting-template/integrator/list",
                Method = HttpMethodType.HttpPost,
            };

            string category = "GetGameBettingInfo";
            string targetCurrency = "";

            if (model.MerchantID.HasValue)
            {
                int merchantID = model.MerchantID.Value;
                SetRequestInfoByMerchantID(req, merchantID, category, Guid.NewGuid().ToString());

                var queryMerchant = MerchantRepository.GetById(merchantID);
                targetCurrency = queryMerchant.Currency;
            }
            else if(model.VendorConfigID.HasValue)
            {
                int vendorConfigID = model.VendorConfigID.Value;
                SetRequestInfoByVendorConfigID(req, vendorConfigID, category, Guid.NewGuid().ToString());

                var queryVendorConfig = VendorConfigRepository.GetById(vendorConfigID);
                targetCurrency = queryVendorConfig.SupportCurrency;
            }

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                currency = ConvertToAGGCurrencyObject(targetCurrency),
                creditRate = 1
            };

            var res = APIRequestHelper.RequestApiResult<AGGGameBettingTemplateAPIResponseModel>(req, body);

            CheckStatusCodeAndNotify(res , isIgnoreUnknowError: true);
            return res.ResultObject;
        }


        public AGGFreespinListAPIResponseModel Freespin_GetList(AGGGameFreespinListReqeustParameter model)
        {
            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-campaign/list",
                Method = HttpMethodType.HttpPost,
            };

            string category = "Freespin_GetList";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                currency = ConvertToAGGCurrencyObject(targetCurrency),
                startDate = model.StartTime.Value.ToISO8601(),
                endDate = model.EndTime.Value.ToISO8601(),
                freeSpinId = model.FreespinID ?? "", //非必要
                clientId = targetMerchantID,
                page = model.PageIndex - 1,
                pageSize = model.PageSize,
            };

            var bodyDic = AnoymousToDictionary(body);

            RemoveColumnIfEmptyNull(bodyDic, ColumnName_FreespinID);

            var res = APIRequestHelper.RequestApiResult<AGGFreespinListAPIResponseModel>(req, bodyDic);

            CheckStatusCodeAndNotify(res);
            return res.ResultObject;
        }



        public AGGFreespinSetAPIResponseModel Freespin_Set(AGGGameFreespinSetReqeustParameter model)
        {
            //這支API 強制要求要指定merchantID
            model.RuleFor(x=> x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-campaign/set",
                Method = HttpMethodType.HttpPost,
            };

            string category = "Freespin_Set";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            req.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),

                freeSpinCampaign = new 
                {
                    //freeSpinId = model.VendorAPIParam.FreeSpinId,
                    currency = ConvertToAGGCurrencyObject(targetCurrency),
                    creditRate = 1,
                    clientId = targetMerchantID,
                    rounds = model.VendorAPIParam.Rounds,
                    gameList = model.VendorAPIParam.GameSettings.Select(x=> new
                        {
                            bet = x.Bet,
                            gameId = x.ThirdGameID
                        })
                    .ToList(),
                    userIdList = model.PlayerIDs.Select(z=> z.ToString()).ToList(),
                    startDate = model.VendorAPIParam.StartTime.ToISO8601(),
                    endDate = model.VendorAPIParam.EndTime.ToISO8601(),
                    useEndDate = model.VendorAPIParam.UseEndTime.ToISO8601(),
                    budgetId = model.VendorAPIParam.BudgetId,
                }
            };

            var res = APIRequestHelper.RequestApiResult<AGGFreespinSetAPIResponseModel>(req, body);

            string extraDebugMsg = res?.ResultObject?.message;

            CheckStatusCodeAndNotify(res , false , extraDebugMsg);

            return res.ResultObject;
        }


        public AGGFreespinDisableAPIResponseModel Freespin_Disable(AGGAPIFreespinDisableReqeustParameter model)
        {
            //這支API 強制要求要指定merchantID
            model.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-campaign/disable",
                Method = HttpMethodType.HttpPost,
            };

            string category = "Freespin_Disable";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                freeSpinId = model.FreeSpinID,
                currency = ConvertToAGGCurrencyObject(targetCurrency),
            };

            var res = APIRequestHelper.RequestApiResult<AGGFreespinDisableAPIResponseModel>(req, body);

            CheckStatusCodeAndNotify(res);

            return res.ResultObject;
        }


        public AGGAPIFreespinStatusResponseModel Freespin_Status(AGGAPIFreespinStatusReqeustParameter model)
        {
            if(!model.VendorConfigID.HasValue)
            {
                model.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();
            }

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-campaign/status",
                Method = HttpMethodType.HttpPost,
            };

            string category = "Freespin_Status";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                currency = ConvertToAGGCurrencyObject(targetCurrency),
                startDate = model.VendorAPIParam.StartTime.ToISO8601(),
                endDate = model.VendorAPIParam.EndTime.ToISO8601(),
                freeSpinUseStatus = model.VendorAPIParam.freeSpinUseStatus,
                freeSpinId = model.VendorAPIParam.FreeSpinID,
                page = model.VendorAPIParam.PageIndex - 1,
                pageSize = model.VendorAPIParam.PageSize,
                clientId = (model.VendorAPIParam.MerchantID <= 0 ? "" : model.VendorAPIParam.MerchantID.ToString()),
                userId = model.UserID.HasValue ? model.UserID.Value.ToString() : "",
            };

            var bodyDic = AnoymousToDictionary(body);

            RemoveColumnIfEmptyNull(bodyDic, ColumnName_ClientID);
            RemoveColumnIfEmptyNull(bodyDic, ColumnName_FreespinID);
            RemoveColumnIfEmptyNull(bodyDic, ColumnName_UserID);
            RemoveColumnIfEmptyNull(bodyDic, ColumnName_FreespinUseStatus);

            var res = APIRequestHelper.RequestApiResult<AGGAPIFreespinStatusResponseModel>(req, bodyDic);

            CheckStatusCodeAndNotify(res);

            return res.ResultObject;
        }


        public AGGAPIFreespinRecordResponseModel Freespin_Record(AGGAPIFreespinRecordRequestParameter model)
        {
            //這支API 強制要求要指定merchantID
            model.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-campaign/record",
                Method = HttpMethodType.HttpPost,
            };

            string category = "Freespin_Record";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                currency = ConvertToAGGCurrencyObject(targetCurrency),
                startDate = model.VendorAPIParam.StartTime.ToISO8601(),
                endDate = model.VendorAPIParam.EndTime.ToISO8601(),
                freeSpinId = model.VendorAPIParam.FreeSpinID,
                page = model.VendorAPIParam.PageIndex - 1,
                pageSize = model.VendorAPIParam.PageSize,
            };

            var bodyDic = AnoymousToDictionary(body);

            RemoveColumnIfEmptyNull(bodyDic, ColumnName_FreespinID);

            var res = APIRequestHelper.RequestApiResult<AGGAPIFreespinRecordResponseModel>(req, bodyDic);

            CheckStatusCodeAndNotify(res);

            return res.ResultObject;
        }


        public AGGAPIFreespinSummaryResponseModel Freespin_Summary(AGGAPIFreespinSummaryRequestParameter model)
        {
            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-campaign/summary",
                Method = HttpMethodType.HttpPost,
            };

            string category = "Freespin_Summary";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                currency = ConvertToAGGCurrencyObject(targetCurrency),
                startDate = model.VendorAPIParam.StartTime.ToISO8601(),
                endDate = model.VendorAPIParam.StartTime.ToISO8601(),
                freeSpinId = model.VendorAPIParam.FreeSpinID,
                groupBySetting = (new List<string> { "FreeSpinId", "GameId", "Month" }).ToArray(),
            };

            var res = APIRequestHelper.RequestApiResult<AGGAPIFreespinSummaryResponseModel>(req, body);

            CheckStatusCodeAndNotify(res);

            return res.ResultObject;
        }

        public AGGAPIFreespinBudgetListResponseModel FreespinSpin_Budget_List(AGGAPIFreespinBudgetListRequestParameter model)
        {
            //這支API 強制要求要指定merchantID
            model.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-budget/list",
                Method = HttpMethodType.HttpPost,
            };

            string category = "FreespinSpin_Budget_List";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                currency = ConvertToAGGCurrencyObject(targetCurrency),
                budgetId = model.VendorAPIParam.BudgetId,
                clientId = model.MerchantID.ToString(),
            };

            var bodyDic = AnoymousToDictionary(body);

            RemoveColumnIfEmptyNull(bodyDic, ColumnName_BudgetID);

            var res = APIRequestHelper.RequestApiResult<AGGAPIFreespinBudgetListResponseModel>(req, bodyDic);

            CheckStatusCodeAndNotify(res);

            return res.ResultObject;
        }

        public AGGAPIFreespinBudgetSetResponseModel FreespinSpin_Budget_Set(AGGAPIFreespinBudgetSetRequestParameter model)
        {
            //這支API 強制要求要指定merchantID
            model.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-budget/set",
                Method = HttpMethodType.HttpPost,
            };

            string category = "FreespinSpin_Budget_Set";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            req.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                freeSpinBudget = new
                {
                    budgetId = model.VendorAPIParam.BudgetId ?? "",
                    currency = ConvertToAGGCurrencyObject(targetCurrency),
                    enable = model.VendorAPIParam.Enable,
                    name = model.VendorAPIParam.Name,
                    clientId = model.MerchantID.ToString(),
                    remark = model.VendorAPIParam.Remark ?? targetMerchantID.ToString(),
                }
            };

            //var bodyDic = AnoymousToDictionary(body);
            //RemoveColumnIfEmptyNull(bodyDic, ColumnName_BudgetID);

            var res = APIRequestHelper.RequestApiResult<AGGAPIFreespinBudgetSetResponseModel>(req, body);

            string extraDebugMsg = res?.ResultObject?.message;

            CheckStatusCodeAndNotify(res , false , extraDebugMsg);

            return res.ResultObject;
        }


        public AGGAPIFreespinBudgetDepositResponseModel FreespinSpin_Budget_Deposit(AGGAPIFreespinBudgetDepositRequestParameter model)
        {
            //這支API 強制要求要指定merchantID
            model.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-budget/deposit",
                Method = HttpMethodType.HttpPost,
            };

            string category = "FreespinSpin_Budget_Deposit";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                freeSpinBudgetWallet = new 
                {
                    budgetId = model.VendorAPIParam.BudgetId,
                    currency = ConvertToAGGCurrencyObject(targetCurrency),
                    amount = model.VendorAPIParam.Amount,
                    rounds = model.VendorAPIParam.Rounds,
                }              
            };

            var res = APIRequestHelper.RequestApiResult<AGGAPIFreespinBudgetDepositResponseModel>(req, body);

            CheckStatusCodeAndNotify(res);

            return res.ResultObject;
        }


        public AGGAPIFreespinBudgetDepositResponseModel FreespinSpin_Budget_Withdrawal(AGGAPIFreespinBudgetWithdrawalRequestParameter model)
        {
            //這支API 強制要求要指定merchantID
            model.RuleFor(x => x.MerchantID).Rule_IsNotNullNotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/game/freespin-budget/withdraw",
                Method = HttpMethodType.HttpPost,
            };

            string category = "FreespinSpin_Budget_Withdrawal";
            string targetCurrency = "";
            string targetMerchantID = "";

            var result = SetFreespinGlobalVariable(model, req, category);
            targetCurrency = result.TargetCurrency;
            targetMerchantID = result.TargetMerchantID;

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                freeSpinBudgetWallet = new
                {
                    budgetId = model.VendorAPIParam.BudgetId,
                    currency = ConvertToAGGCurrencyObject(targetCurrency),
                    amount = model.VendorAPIParam.Amount,
                    rounds = model.VendorAPIParam.Rounds,
                }
            };

            var res = APIRequestHelper.RequestApiResult<AGGAPIFreespinBudgetDepositResponseModel>(req, body);

            CheckStatusCodeAndNotify(res);

            return res.ResultObject;
        }



        public AGGGameSettingApiResponseModel GetAGGPromotionEventSettings(int merchantID, AGGPromotionSettingServiceTypeEnum serviceType)
        {
            return GetAGGPromotionEventSettings_V202301(merchantID, serviceType);
        }


        public AGGGameSettingApiResponseModel SetAGGPromotionSettingFlag(AGGGameSettingFlagParameter model)
        {
            return SetAGGPromotionSettingFlag_V202301(model);
        }


        public void CheckBonusRetryRecordAndTrigger(AGGGetBonusRetryReqeustParameter model)
        {
            var query = GetBonusRetryRecords(model);

            if (query.IsEmptyOrNull())
                return;

            return;

            //對方提供的API 使用方法不符合預期
            //對方提供的API 使用方法不符合預期
            //對方提供的API 使用方法不符合預期
            //對方提供的API 使用方法不符合預期
            //對方提供的API 使用方法不符合預期

            List<string> targetIDs = query.Select(x => x.TransactionId).ToList();

            if (SettingHelper.IsLocalhostWebSiteName)
            {
                List<string> matchKeyword = new List<string>();

                matchKeyword = new List<string>
                {
                    "BonusWin_OceanGameItem_4a9a9eb4bb3b4807b563810ec3366c36_0" ,
                    "BonusWin_OceanGameItem_c26e789470144a649a50710570581851_0",
                    "BonusWin_OceanGameItem_21da483b90bd40bea10be361f5401560_0",
                };

                if (matchKeyword.IsEmptyOrNull())
                {
                    matchKeyword = targetIDs
                      .Take(50)
                      .ToList();
                }

                targetIDs = query
                    .Where(x => matchKeyword.Any(text => x.TransactionId.Contains(text)))
                    .Select(x => x.TransactionId).ToList();
            }

            var splitBySize = targetIDs.SplitListByArraySize(100);

            foreach (var eachSmallArray in splitBySize)
            {
                TriggerBonusRetry(model, eachSmallArray);
                Thread.Sleep(1000 * 5);
            }

        }


        public List<AGGBonusRetryRecordsDetailItemModel> GetBonusRetryRecords(AGGGetBonusRetryReqeustParameter model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/bonus/retry/record",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "GetBonusRetryRecord", Guid.NewGuid().ToString());

            var queryVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            string targetCurrency = queryVendorConfig.SupportCurrency;

            if (string.IsNullOrWhiteSpace(targetCurrency))
            {
                targetCurrency = "MYR";
            }

            var body = new
            {
                currency = ConvertToAGGCurrencyObject(targetCurrency)
            };

            var res = APIRequestHelper.RequestApiResult<AGGBonusRetryRecordsResponseModel>(req, body);

            CheckStatusCodeAndNotify(res);

            if (res.ResultObject != null && res.ResultObject.Records.IsNotEmptyNull())
            {
                var debug = res.ResultObject.Records.Where(x => x.TransactionId.Contains("OceanGameItem_ac11ca7f13904963ba59e058f167262e")).ToList();
            }

            return res.ResultObject.Records;
        }


        public string TriggerBonusRetry(AGGGetBonusRetryReqeustParameter model, List<string> transactionIDs)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/command/v2/bonus/retry",
                Method = HttpMethodType.HttpPost,
            };

            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "TriggerBonusRetry", Guid.NewGuid().ToString());

            var queryVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            var body = new
            {
                transactionIds = transactionIDs
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            var insertHisotry = new EazyTempValueEntity
            {
                UUID = Guid.NewGuid(),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                Type1 = EazyTempValueTypeEnum.AGGBonusWinRetryTriggerRecord.ToInteger(),
                DebugInfo = string.Join(",", transactionIDs)
            };

            EazyTempValueRepository.Create(insertHisotry);

            return res.ResponseText;
        }



        public AGGGameSettingApiResponseModel GetAGGPromotionEventSettings_V202301(int merchantID, AGGPromotionSettingServiceTypeEnum serviceType)
        {
            //外層可能已經呼叫過了
            if (this._tempVendorConfigSettings.IsEmptyOrNull())
            {
                SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);
            }

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl, true)?.SettingValue;
            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            var jsonSetting = GetAGGExtraSetting();

            var queryMerchant = MerchantRepository.GetById(merchantID);

            var req = new RequestParameters
            {
                BaseUrl = jsonSetting.API2_HOST,
                ApiUrl = "v2/game/setting",
                Method = HttpMethodType.HttpPost,
            };

            req.TimeoutMillisecond = 1000 * 180;
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = "game-setting",
                Log_SubCategory = GameVendor.ToString(),
                Log_DebugInfo2 = serviceType.ToString(),
            };

            req.UserAgent = "target";
            req.AddHeaderData("X-API-KEY", encryptKey);
            req.AddHeaderData("X-INTEGRATOR", assignMerchantID);

            var para = new
            {
                requestId = Guid.NewGuid(),
                serviceId = serviceType.ToString(),
                currencyCode = ConvertCurrencyCode(queryMerchant.Currency),
                @namespace = merchantID.ToString(),
            };

            var apiResult = APIRequestHelper.RequestApiResult<AGGGameSettingApiResponseModel>(req, para);
            var result = apiResult.ResultObject;

            //if (apiResult.ResponseStatusCode != 200 || result == null)
            //{
            //    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel($"{apiResult.ResponseText}"));
            //}

            return result;
        }


        public AGGGameSettingApiResponseModel SetAGGPromotionSettingFlag_V202301(AGGGameSettingFlagParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            int merchantID = model.MerchantID;

            //外層可能已經呼叫過了
            if (this._tempVendorConfigSettings.IsEmptyOrNull())
            {
                SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);
            }

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl, true)?.SettingValue;
            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            var jsonSetting = GetAGGExtraSetting();

            var queryMerchant = MerchantRepository.GetById(merchantID);

            string enableOrDisable = model.IsDisable ? "disable" : "enable";

            var req = new RequestParameters
            {
                BaseUrl = jsonSetting.API2_HOST,
                ApiUrl = $"v2/game/setting/{enableOrDisable}",
                Method = HttpMethodType.HttpPost,
            };

            req.TimeoutMillisecond = 1000 * 180;
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = $"game-setting-{enableOrDisable}",
                Log_SubCategory = GameVendor.ToString(),
                Log_DebugInfo2 = model.ServiceType.ToString(),
            };

            req.UserAgent = "target";
            req.AddHeaderData("X-API-KEY", encryptKey);
            req.AddHeaderData("X-INTEGRATOR", assignMerchantID);

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("requestId", Guid.NewGuid().ToString());
            para.Add("serviceId", model.ServiceType.ToString());
            para.Add("currencyCode", ConvertCurrencyCode(queryMerchant.Currency));
            para.Add("namespace", merchantID.ToString());

            if (model.SettingID.IsNotEmptyTextNotNull())
            {
                para.Add("settingId", model.SettingID);
            }

            var apiResult = APIRequestHelper.RequestApiResult<string>(req, para);
            var result = apiResult.ResultObject;

            if (apiResult.ResponseStatusCode != 200 || result == null)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    LogExtraInfo = apiResult.ResponseText
                });
            }

            return null;
        }


        private (string TargetCurrency, string TargetMerchantID) SetFreespinGlobalVariable<T>(T model, RequestParameters req, string category) where T : IMerchantIDVendorConfigID
        {
            if (model.MerchantID.HasValue)
            {
                int merchantID = model.MerchantID.Value;
                SetRequestInfoByMerchantID(req, merchantID, category, Guid.NewGuid().ToString());

                var queryMerchant = MerchantRepository.GetById(merchantID);

                return (queryMerchant.Currency, merchantID.ToString());
            }
            else if (model.VendorConfigID.HasValue)
            {
                int vendorConfigID = model.VendorConfigID.Value;
                SetRequestInfoByVendorConfigID(req, vendorConfigID, category, Guid.NewGuid().ToString());

                var queryVendorConfig = VendorConfigRepository.GetById(vendorConfigID);

                return (queryVendorConfig.SupportCurrency, "");
            }


            throw new TerproException(ApiErrorCode.InvalidParameter);
        }

        public static Dictionary<string, object> AnoymousToDictionary(object obj)
        {
            return obj.GetType()
                      .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                      .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj));
        }

        private void RemoveColumnIfEmptyNull(Dictionary<string, object> dic, string columnName)
        {
            if (dic[columnName] == null || dic[columnName] == "")
            {
                dic.Remove(columnName);
            }
        }

    }

}