using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.BL.Services
{

    public partial class AGGIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService, IAGGInternalAPIService
    {


        public ExternalVendorLaunchUrlViewModel GetGameLaunchUrlByAPI_V202502(ExternalVendorLaunchUrlRequestModel model)
        {
            if (model.GameID % 1000 == 999)
            {
                return GetLobbyUrlByAPI_V202502(model);
            }

            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string defaultBetAmountSetting = GetVendorSettingByTempVariable(VendorConfigSettingEnum.AGG_DefaultBetAmount, false)?.SettingValue;

            string sessionToken = [REDACTED];
            string gameCode = model.ThirdPartyGameCode;
            string returnUrl = model.BackUrlEncode;

            var disableBF = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_DisableAGGBuyFeature);

            if (string.IsNullOrWhiteSpace(defaultBetAmountSetting))
            {
                defaultBetAmountSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.AGG_DefaultBetAmount).SettingValue;
            }

            string relativeUrl = model.IsTrialMode ? "/game/trial" : "/game/launch";

            try
            {
                //V2版本一律要先創建namespace
                CreateAGGNamespaceByAPI(model.MerchantID);
            }
            catch
            {

            }


            var req = new RequestParameters
            {
                ApiUrl = "/v2/game/url",
                Method = HttpMethodType.HttpPost,
            };

            string apiCategory = "GetGameLaunchUrl";

            SetCommonRequestInfo(req, apiCategory, apiCategory);
            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();
            req.AdvancedSetting.Log_MerchantID = model.MerchantID;
            req.AdvancedSetting.Log_PlayerID = model.PlayerID;

            string lang = ConvertToGameVendorLangCode(model.Language);

            string backUrlParam = string.Empty;
            string defaultBetAmount = string.Empty;


            if (SettingHelper.IsLocalhostWebSiteName)
            {
                //lobby似乎有專屬url
                //gameCode = "lobby";
            }

            Dictionary<string, object> postBody = new Dictionary<string, object>();

            //請注意boolean值 不要用字串
            //請注意boolean值 不要用字串
            //請注意boolean值 不要用字串

            postBody.Add("userToken", sessionToken);
            postBody.Add("gameId", gameCode);
            postBody.Add("lang", lang);
            postBody.Add("username", model.PlayerName);
            postBody.Add("isTrial", model.IsTrialMode);
            postBody.Add("currency", ConvertToAGGCurrencyObject(model.Currency));

            if (disableBF.SettingValueOfBoolean)
            {
                postBody.Add("hbf", true);
            }

            if (!string.IsNullOrWhiteSpace(model.BackUrlEncode))
            {
                postBody.Add("backUrl", model.BackUrlEncode);
                postBody.Add("target", "parent");
            }

            if (!string.IsNullOrWhiteSpace(defaultBetAmountSetting))
            {
                postBody.Add("defaultBet", defaultBetAmountSetting);
            }

            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<AGGV2APIUrlResultResponseModel>(req, postBody);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res, apiCategory);

            string finalUrl = res.ResultObject?.Url;

            //取代成pgs域名
            finalUrl = ReplaceUrlDomainToPgsBrand(finalUrl);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = finalUrl
            };
        }


        public ExternalVendorLaunchUrlViewModel GetLobbyUrlByAPI_V202502(ExternalVendorLaunchUrlRequestModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string defaultBetAmountSetting = GetVendorSettingByTempVariable(VendorConfigSettingEnum.AGG_DefaultBetAmount, false)?.SettingValue;

            string sessionToken = [REDACTED];
            string gameCode = model.ThirdPartyGameCode;
            string returnUrl = model.BackUrlEncode;

            var disableBF = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_DisableAGGBuyFeature);

            string disableBF_Flag = disableBF.SettingValueOfBoolean ? "true" : "false";
            string relativeUrl = model.IsTrialMode ? "/game/trial" : "/game/launch";

            //V2版本一律要先創建namespace
            CreateAGGNamespaceByAPI(model.MerchantID);

            var req = new RequestParameters
            {
                ApiUrl = "/v2/game/lobby/url",
                Method = HttpMethodType.HttpPost,
            };

            string apiCategory = "GameLaunchUrl_Lobby";

            SetCommonRequestInfo(req, model.MerchantID.ToString(), apiCategory);

            string lang = ConvertToGameVendorLangCode(model.Language);

            string backUrlParam = string.Empty;
            string defaultBetAmount = string.Empty;

            Dictionary<string, object> postBody = new Dictionary<string, object>();
            postBody.Add("userToken", sessionToken);
            postBody.Add("language", lang);

            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<AGGV2APIUrlResultResponseModel>(req, postBody);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res, apiCategory);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = res.ResultObject?.Url
            };
        }


        public string GetRecordReplayHistoryByApi_V202502(ExternalVendorGetSingleBetRecordRequestParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.BetRecordNo).Rule_NotEmptyText();

            var req = new RequestParameters
            {
                ApiUrl = "v2/game/replay",
                Method = HttpMethodType.HttpPost,
            };

            string apiCategory = "GameReplay";

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, model.MerchantID, apiCategory, model.QueryReplayExtraParameter?.DebugReferenceUUID);

            var currentLang = HeaderValueHelper.GetCurrentLanguage();

            string beginText = null;
            string endText = null;

            if (model.QueryReplayExtraParameter != null)
            {
                if (model.QueryReplayExtraParameter.BeginTime.HasValue)
                {
                    beginText = model.QueryReplayExtraParameter.BeginTime.Value.ToISO8601();
                }

                if (model.QueryReplayExtraParameter.EndTime.HasValue)
                {
                    endText = model.QueryReplayExtraParameter.EndTime.Value.ToISO8601();
                }
            }

            var body = new
            {
                gameId = model.GameCode,
                wagerId = model.BetRecordNo,
                startdate = beginText,
                enddate = endText,
                lang = ConvertToGameVendorLangCode(currentLang),
                currency = ConvertToAGGCurrencyObject(queryMerchant.Currency)
            };

            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<AGGV2APIWagerReplayResponseModel>(req, body);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res, apiCategory);

            string finalUrl = res.ResultObject?.Url;

            if (finalUrl.ContainsWihtoutCase("example.invalid"))
            {
                finalUrl = finalUrl.Replace("example.invalid/game/replay", "example.invalid/game/replay");
            }

            return finalUrl;
        }



        public void ChangeLobbyEventEnableFlag(IDGLobbyEventChangeEnableFlagParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "v2/integrator/hierarchy/nodes/event",
                Method = HttpMethodType.HttpPut,
            };

            string apiCategory = model.IsEnable ? "LobbyEvent_Enable" : "LobbyEvent_Disable";

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, model.MerchantID, apiCategory, "");

            var body = new
            {
                requestId = Guid.NewGuid().ToString(),
                agentId = model.MerchantID,
                enableEvent = model.IsEnable,
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res, apiCategory);

            return;
        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport_V202502(ExternalVendorGetReportRequestParameter filter)
        {
            filter.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "command/v2/wager/summary",
                Method = HttpMethodType.HttpPost,
            };

            SetRequestInfoByVendorConfigID(req, filter.VendorConfigID, "FetchEachGameAccountingReport");

            string aggIntegrator = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;

            var twDate = filter.StartTimeUTC.AddHours(8);

            var para = new
            {
                requestId = Guid.NewGuid().ToString(),
                startDate = filter.StartTimeUTC.ToISO8601(),
                endDate = filter.EndTimeUTC.AddMilliseconds(-3).ToISO8601(),  //以日為單位查詢的話 必須是 23:59:59
                groupByGameId = true,
                filterIsFreePlay = false,

                //groupByGame = true,
                //timezone = "+08:00",
                //agentHierarchiesGroup = new
                //{
                //    prefixFilter = SystemConstant.AGG_RootAgentName,
                //    recursive = true,
                //    levels = 1
                //}
            };

            var res = APIRequestHelper.RequestApiResult<AGGAccountingAPIV2ResponseModel>(req, para);

            CheckStatusCodeAndNotify(res);

            if (res.ResultObject.records.IsEmptyOrNull())
                return new ExternalVendorGetReportResultModel();

            var converts = res.ResultObject.records
                .Select(z =>
                {
                    var convert = new GameVendorAccountingSummaryReportModel
                    {
                        ThirdPartyGameCode = z.gameId,
                        TotalBetAmount = z.totalBet,
                        TotalBetCount = z.wagersCount,
                        TotalPayoutAmount = (z.totalJpWin + z.totalWin),
                        TotalValidBetAmount = z.totalBet,
                        StatDate = twDate,
                        TotalWinLoss = z.totalJpWin + z.totalWin - z.totalBet
                    };
                    return convert;
                }).ToList();

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = aggIntegrator,
                ResponseText = res.ResponseText,
                EachGameSummary = converts
            };
        }

        public AGGBonusSummaryAPIResponse FetchBonusSummaryRerport(AGGReportQueryRequestParameter filter)
        {
            filter.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "command/v2/bonus/summary",
                Method = HttpMethodType.HttpPost,
            };

            SetRequestInfoByVendorConfigID(req, filter.VendorConfigID, "FetchBonusSummaryRerport");
            req.TimeoutMillisecond = 60 * 1000;

            string aggIntegrator = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;

            var twDate = filter.StartTimeUTC.AddHours(8);

            var para = new
            {
                requestId = Guid.NewGuid().ToString(),
                startDate = filter.StartTimeUTC.ToISO8601(),
                endDate = filter.EndTimeUTC.AddMilliseconds(-3).ToISO8601(),  //以日為單位查詢的話 必須是 23:59:59
                groupByKey = "bonus_channel",
            };

            var res = APIRequestHelper.RequestApiResult<AGGBonusSummaryAPIResponse>(req, para);

            CheckStatusCodeAndNotify(res);

            return res.ResultObject;
        }


        public ApiResultBriefObjectModel GetSingleBetRecordRawResponseByApi_V202502(string betRecordNoPGS)
        {
            new { betRecordNoPGS = betRecordNoPGS }.RuleFor(x => x.betRecordNoPGS).Rule_NotEmptyText();

            var queryRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryRecord.GameID);

            var req = new RequestParameters
            {
                ApiUrl = "game/record/findByWager",
                Method = HttpMethodType.HttpPost,
            };

            var query = GetAGGRecordsByInternalApi_V202502(new ExternalVendorFetchWagersRequestParameter
            {
                StartTimeUTC = queryRecord.CreateTime.AddHours(-1),
                EndTimeUTC = queryRecord.ThirdSettleTime.Value.AddHours(2),
                Extra_SearchWagerID = queryRecord.BetRecordNo,
                Page = 1,
                VendorConfigID = queryRecord.VendorConfigID.Value,
            }, 10);

            return new ApiResultBriefObjectModel
            {
                ResponseText = query.ResponseText,
                ResultObject = query.WagersV2,
                RequestUUID = "",
            };
        }


        /// <summary>
        /// AGG撈注單目前遇過的問題
        /// 2025/01 有特定注單撈不到
        /// 2025/01 撈到所有注單後，篩選 endTime特定日期。實際筆數與他們後台筆數不符
        /// 2025/02 因網路異常，導致以下注單遺失/重複寫單 (2025/02/03 的TG訊息)
        /// </summary>
        /// <exception cref="TerproException"></exception>
        public AGGAccountingRecordsResponseModel GetAGGRecordsByInternalApi_V202502(ExternalVendorFetchWagersRequestParameter model, int pageSize)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();

            //外層可能已經呼叫過了
            if (this._tempVendorConfigSettings.IsEmptyOrNull())
            {
                SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);
            }

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl, true)?.SettingValue;
            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            DateTime begin = model.StartTimeUTC;
            DateTime end = model.EndTimeUTC;

            var req = new RequestParameters
            {
                BaseUrl = apiUrl,
                ApiUrl = "command/v2/wager/find",
                Method = HttpMethodType.HttpPost,
            };

            req.TimeoutMillisecond = 1000 * 180;
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = "FetchWagers",
                Log_SubCategory = GameVendor.ToString(),
                Log_IsSkipWriteResponseText = true,
            };

            req.UserAgent = "target";
            req.AddHeaderData("X-API-KEY", encryptKey);
            req.AddHeaderData("X-INTEGRATOR", assignMerchantID);

            var para = new Dictionary<string, object>();
            para.Add("startDate", begin.ToISO8601());
            para.Add("endDate", end.ToISO8601());
            para.Add("pageSize", pageSize);
            para.Add("page", model.Page);

            if (!string.IsNullOrWhiteSpace(model.GameCode))
            {
                para.Add("gameId", model.GameCode);
            }

            if (!string.IsNullOrWhiteSpace(model.Extra_SearchWagerID))
            {
                para.Add("wagerId", model.Extra_SearchWagerID);
            }

            var apiResult = APIRequestHelper.RequestApiResult<AGGAccountingRecordsResponseModel>(req, para);
            var result = apiResult.ResultObject;

            if (apiResult.ResponseStatusCode != 200 || result == null)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel($"{apiResult.ResponseText}"));
            }

            result.ResponseText = apiResult.ResponseText;
            result.WagersV2 = new List<ThirdPartyFetchRecordBriefEntity>();

            if (result != null && result.Wagers.IsNotEmptyNull())
            {
                foreach (var item in result.Wagers)
                {
                    DateTime convert = DateTime.Parse(item.beginDateTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    DateTime settleTime = DateTime.Parse(item.endDateTIme, null, System.Globalization.DateTimeStyles.RoundtripKind);

                    //v2版的寫法似乎不需要排除此
                    //if (settleTime >= model.EndTimeUTC)
                    //{
                    //    SendMessageToTelegram($"單號: {item.wagerId} 被排除");
                    //    continue;
                    //}

                    result.WagersV2.Add(new ThirdPartyFetchRecordBriefEntity
                    {
                        WagerID = item.wagerId,
                        BetAmount = item.totalBet,
                        PayoutAmount = item.totalWin + item.totalJpWin,
                        BetTime = convert,
                        GameCode = item.gameId,
                        SettleTime = settleTime,
                        VendorConfigID = model.VendorConfigID,
                        PlayerID = Int32.Parse(item.userId),
                        DebugInfo1 = $"{begin.ToString("MM-dd HH:mm")}~{end.ToString("MM-dd HH:mm")}"
                    });
                }
            }

            return result;
        }


        /// <summary>
        /// 取得異常注單
        /// </summary>
        public ApiResultBriefModel<AGGGetExceptionalWagersResponseModel> GetExceptionalRecords_V202502(AGGGetExceptionalWagersReqeustParameter model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/game/wager/exceptional",
                Method = HttpMethodType.HttpGet,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "GetExceptionalRecords", model.DebugReferenceUUID);

            if (SettingHelper.IsMSTest)
            {
                req = new RequestParameters
                {
                    BaseUrl = "http://example.invalid",
                    ApiUrl = "/testcase/400Exception",
                    Method = HttpMethodType.HttpGet,
                };
            }

            var res = APIRequestHelper.RequestApiResult<AGGGetExceptionalWagersResponseModel>(req);

            CheckStatusCodeAndNotify(res, isIgnoreUnknowError: true);

            return ConvertToBriefModel(res);
        }

        public AGGNamespaceCreateResponseModel CreateAGGHierarchyNodes_EachLayer(int merchantID)
        {
            var queryHierarchy = MerchantHierarchyRepository.GetById(merchantID);

            AGGNamespaceCreateResponseModel result = null;

            if (queryHierarchy == null)
            {
                result = CreateAGGNamespaceByAPI_TargetMerchant(merchantID);
            }
            else
            {
                List<int> merchantIDs = AppHelper.GetHierarchyMerchantIDs(queryHierarchy.Hierarchy);

                foreach (int eachMerchantID in merchantIDs)
                {
                    if (IsHierarchyAlreadyCreated(eachMerchantID))
                        continue;

                    SetVendorConfigSettingsToTempVariableByMerchantID(eachMerchantID);

                    try
                    {
                        result = CreateAGGNamespaceByAPI_TargetMerchant(eachMerchantID);
                    }
                    catch
                    {

                    }
                }
            }

            //重製全域變數 避免被洗成別組配置
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);
            return result;
        }


        private bool IsHierarchyAlreadyCreated(int merchantID)
        {
            var settingKey = MerchantCustomizedSettingKeyEnum.GameSetting_IDGNameSpace_CreateTime;
            var last_createTime = MerchantCustomizedSettingRepository.GetMerchantSettingKeyWithoutCache(merchantID, settingKey, isReturnNull: false);

            //有資料 代表已經創建過了
            if (!string.IsNullOrWhiteSpace(last_createTime.SettingValue))
            {
                return true;
            }

            return false;
        }


        public AGGNamespaceCreateResponseModel CreateAGGNamespaceByAPI_TargetMerchant(int merchantID)
        {
            //有資料 代表已經創建過了
            if (IsHierarchyAlreadyCreated(merchantID))
            {
                return null;
            }

            //外層可能已經呼叫過了
            if (this._tempVendorConfigSettings.IsEmptyOrNull())
            {
                SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);
            }

            var settingKey = MerchantCustomizedSettingKeyEnum.GameSetting_IDGNameSpace_CreateTime;

            var queryMyHierarchy = MerchantHierarchyRepository.GetById(merchantID);

            var aggHierarchy = ConvertToAGGHierarchyTree(queryMyHierarchy , merchantID);
           
            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl, true)?.SettingValue;
            var jsonSetting = GetAGGExtraSetting();

            var queryMerchant = MerchantRepository.GetById(merchantID);

            string merchantIDText = merchantID.ToString();

            var req = new RequestParameters
            {
                BaseUrl = jsonSetting.API2_HOST,
                ApiUrl = "v2/integrator/hierarchy/node/add",
                Method = HttpMethodType.HttpPost,
            };

            SetCommonRequestInfo(req, merchantIDText, "hierarchy_node_add");

            req.BaseUrl = jsonSetting.API2_HOST;

            Dictionary<string, object> postBody = new Dictionary<string, object>();

            postBody.Add("requestId", Guid.NewGuid().ToString());
            postBody.Add("agentId", merchantIDText);
            postBody.Add("name", merchantIDText);

            if (aggHierarchy.ParentMerchantID > 0)
            {
                postBody.Add("parentId", aggHierarchy.ParentMerchantID.ToString());
            }

            var apiResult = APIRequestHelper.RequestApiResult<AGGAPICommonResponseModel>(req, postBody);
            var result = apiResult.ResultObject;

            bool isConflictError = apiResult.ResponseText.Contains("Conflict") && apiResult.ResponseText.Contains("10301");

            if ((isConflictError))
            {
                //視為成功
            }
            else if (apiResult.ResponseStatusCode != 200 || result == null || string.IsNullOrWhiteSpace(apiResult.ResultObject.RequestId))
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    LogExtraInfo = apiResult.ResponseText
                });
            }

            string tsText = DateTimeHelper.GetCurrentTimeTimestamp().ToString();
            MerchantCustomizedSettingRepository.CreateOrUpdate(merchantID, (int)settingKey, tsText);

            return null;
        }

        /// <summary>
        /// 特殊情境使用 基本上是 no-jp 那種案例
        /// </summary>
        public AGGNamespaceCreateResponseModel CreateAGGNamespaceByAPI_SpecifyVendorConfigID(int merchantID , int vendorConfigID)
        {            
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            var settingKey = MerchantCustomizedSettingKeyEnum.GameSetting_IDGNameSpace_CreateTime;

            var queryMyHierarchy = MerchantHierarchyRepository.GetById(merchantID);

            var aggHierarchy = ConvertToAGGHierarchyTree(queryMyHierarchy , merchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl, true)?.SettingValue;

            var jsonSetting = GetAGGExtraSetting();

            var queryMerchant = MerchantRepository.GetById(merchantID);

            string merchantIDText = merchantID.ToString();

            var req = new RequestParameters
            {
                BaseUrl = jsonSetting.API2_HOST,
                ApiUrl = "v2/integrator/hierarchy/node/add",
                Method = HttpMethodType.HttpPost,
            };

            SetCommonRequestInfo(req, merchantIDText, "hierarchy_node_add");

            req.BaseUrl = jsonSetting.API2_HOST;

            Dictionary<string, object> postBody = new Dictionary<string, object>();

            postBody.Add("requestId", Guid.NewGuid().ToString());
            postBody.Add("agentId", merchantIDText);
            postBody.Add("name", merchantIDText);

            if (aggHierarchy.ParentMerchantID > 0)
            {
                postBody.Add("parentId", aggHierarchy.ParentMerchantID.ToString());
            }

            var apiResult = APIRequestHelper.RequestApiResult<AGGAPICommonResponseModel>(req, postBody);
            var result = apiResult.ResultObject;

            bool isConflictError = apiResult.ResponseText.Contains("Conflict") && apiResult.ResponseText.Contains("10301");

            if ((isConflictError))
            {
                //視為成功
            }
            else if (apiResult.ResponseStatusCode != 200 || result == null || string.IsNullOrWhiteSpace(apiResult.ResultObject.RequestId))
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    LogExtraInfo = apiResult.ResponseText
                });
            }

            return null;
        }




        /// <summary>
        /// 觸發異常注單處理
        /// </summary>
        public string TriggerExceptionalWagerRetry_V202502(AGGGetExceptionalWagersReqeustParameter model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "/game/wager/exceptional/retry",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "TriggerExceptionalWagerRetry", model.DebugReferenceUUID);

            string body = "[]";

            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            CheckStatusCodeAndNotify(res);

            return res.ResponseText;
        }

      
        public AGGGetPlayerRTPSettingV2ResponseModel GetPlayerRTPSettings_V202502(AGGGetPlayerRTPSettingV2ReqeustParameter model)
        {
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "command/v2/option/get",
                Method = HttpMethodType.HttpPost,
            };

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);


            string requestUUD = $"{model.PlayerID}_{DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS()}";

            var queryMerchant = MerchantRepository.GetByPlayerID(model.PlayerID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetPlayerRTPSettings", requestUUD);

            var body = new
            {
                userId = model.PlayerID,
                gameId = queryGame.ThirdPartyGameCode,
                currency = ConvertToAGGCurrencyObject(queryMerchant.Currency),
            };

            var res = APIRequestHelper.RequestApiResult<AGGGetPlayerRTPSettingV2ResponseModel>(req, body);

            ApiResultCheckModel apiResultCheck = CheckResponseErrorByKeyword(res, requestUUD, model.IsAutoThrowException);
            return res.ResultObject;
        }


        public ApiResultCheckModel UpdatePlayerRTPSettings_V202502(AGGUpdatePlayerRTPSettingV2ReqeustParameter model)
        {
            model.RuleFor(x => x.GameIDs).Rule_IsNotEmptyCollection();
            model.RuleFor(x => x.RTP).Rule_NotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "command/v2/option/set",
                Method = HttpMethodType.HttpPost,
            };

            var queryGames = GameListRepository.GetByIds(model.GameIDs);

            if (queryGames.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundGame);

            string requestUUD = $"{model.PlayerID}_{DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS()}";


            var queryMerchant = MerchantRepository.GetByPlayerID(model.PlayerID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetPlayerRTPSettings", requestUUD);

            var body = new
            {
                userId = model.PlayerID,
                gameIds = queryGames.Select(x => x.ThirdPartyGameCode),
                currency = ConvertToAGGCurrencyObject(queryMerchant.Currency),
                option = new
                {
                    rtp = model.RTP
                }
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            ApiResultCheckModel apiResultCheck = CheckResponseErrorByKeyword(res, requestUUD, model.IsAutoThrowException);
            return apiResultCheck;
        }

        public ApiResultCheckModel DeletePlayerRTPSettings_V202502(AGGUpdatePlayerRTPSettingV2ReqeustParameter model)
        {
            model.RuleFor(x => x.GameIDs).Rule_IsNotEmptyCollection();
            model.RuleFor(x => x.RTP).Rule_NotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = "command/v2/option/delete",
                Method = HttpMethodType.HttpPost,
            };

            var queryGames = GameListRepository.GetByIds(model.GameIDs);

            if (queryGames.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundGame);

            string requestUUD = $"{model.PlayerID}_{DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS()}";

            var queryMerchant = MerchantRepository.GetByPlayerID(model.PlayerID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetPlayerRTPSettings", requestUUD);

            var body = new
            {
                userId = model.PlayerID,
                gameIds = queryGames.Select(x => x.ThirdPartyGameCode),
                names = new List<string> { "winLimit", "rtp", "winMultiLimit" },
                currency = ConvertToAGGCurrencyObject(queryMerchant.Currency),
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            ApiResultCheckModel apiResultCheck = CheckResponseErrorByKeyword(res, requestUUD, model.IsAutoThrowException);
            return apiResultCheck;
        }



        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();

            int pageSize = 10000;

            model.Page = model.Page - 1; //此遊戲商的page是從0開始

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            var param = new RetryLoopParameter
            {
                MaxTimes = 2,
                DelayMilliseconds = 1000 * 5,
                ExceptionNotifyLevel = LoopExceptionNotifyLevelEnum.OnlyWhenLastTimeExceptioin
            };

            AGGAccountingRecordsResponseModel apiResult = null;

            AppHelper.ExecuteMethodAndRetryMaxTimes(param, (loopParam) =>
            {
                apiResult = GetAGGRecordsByInternalApi_New(model, pageSize);
                return true;
            });

            var result = new ExternalVendorFetchWagersResult
            {
                PageSize = pageSize,
                ResponseText = apiResult.ResponseText,
                ConvertInsertEntity = apiResult.WagersV2,
            };

            return result;
        }

        public object RequestGoThroughAPIByType(AGGGoThroughAPITypeEnum apiType, AGGGoThroughReqeustParameter model)
        {
            var commonParam = model.CommonParams;

            commonParam.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            string apiUrl = apiType.GetDescription();

            //每隻API都會有這欄位
            model.KeyValues.Add("currency", ConvertCurrencyCode(commonParam.Currency));
            model.KeyValues.Add("requestId", Guid.NewGuid().ToString());

            var req = new RequestParameters
            {
                ApiUrl = apiUrl,
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, model.CommonParams.MerchantID, apiType.ToString());

            var res = APIRequestHelper.RequestApiResult<object>(req, model.KeyValues);

            return res.ResultObject;
        }


        private string ReplaceUrlDomainToPgsBrand(string myUrl)
        {
            if (string.IsNullOrWhiteSpace(myUrl))
                return myUrl;

            //暫時先只有screen回放功能支持此功能
            if (!myUrl.Contains("https://clientapi"))
                return myUrl;

            //if (!SettingHelper.IsLocalhostWebSiteName)
            //    return myUrl;

            //if (!SettingHelper.IsLocalhostWebSiteName && !SettingHelper.IsAdminSysAPI)
            //    return myUrl;

            var queryReplaceSetting = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                UniqueOnwerQuery = new UniqueParentCustomizedSettingQuery
                {
                    OwnerID = GameVendor.ToInteger(),
                    Type = OneToManyCustomizedSettingTypeEnum.GameVendorDoaminReplace
                }
            });

            if (queryReplaceSetting.IsNotEmptyNull())
            {
                foreach (var item in queryReplaceSetting)
                {
                    if (string.IsNullOrWhiteSpace(item.SettingValue))
                        continue;

                    var parseValue = new SystemConfigSettingParseModel(0, item.SettingValue);

                    if (parseValue.SettingValueOfListString.CountOrZero() != 2)
                        continue;

                    var replace_source = parseValue.SettingValueOfListString[0].Trim();
                    var replace_to = parseValue.SettingValueOfListString[1].Trim();

                    myUrl = myUrl.Replace(replace_source, replace_to);
                }
            }

            return myUrl;
        }


        private AGGExtraJsonSettingModel GetAGGExtraSetting(bool isThrowException = true)
        {
            string json = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_ExtraJsonSetting, isThrowException)?.SettingValue;

            if (string.IsNullOrWhiteSpace(json))
            {
                return new AGGExtraJsonSettingModel();
            }

            var parse = json.JsonConvertToModel<AGGExtraJsonSettingModel>();
            return parse;
        }



        private void CheckStatusCodeAndNotify<T>(ApiRequestResultModel<T> apiResult, bool isIgnoreUnknowError = false , string extraDisplayMsg = "")
        {
            int statusCode = apiResult.ResponseStatusCode;

            bool is400Status = statusCode >= 400;

            string timeoutMsg = apiResult.IsTimeoutException ? "逾時錯誤" : "";

            if (apiResult.Exception != null || is400Status)
            {
                bool isNotify = true;

                string apiUrl = apiResult?.RelativeUrl;

                if (isIgnoreUnknowError && apiResult.ResponseText.Contains("unknown integratorId"))
                {
                    isNotify = false;
                }

                //太吵了 跳過這個
                if (apiUrl.ContainsWihtoutCase("game/wager/exceptional") && statusCode == 404)
                {
                    isNotify = false;
                }

                if (apiUrl.ContainsWihtoutCase("game/wager/exceptional"))
                {
                    isNotify = false;
                }

                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel
                {
                    ColumnName = extraDisplayMsg,
                    IsAlwaysNotify = isNotify,
                    LogExtraInfo = $" {GameVendor.ToString()} {apiResult.DebugInfo.RequestParameters.AdvancedSetting?.Log_ApiCategory} API: {apiUrl} {timeoutMsg} sCode: {statusCode} , {apiResult.ResponseText.Truncate(150)}",
                });
            }
        }


    }
}
