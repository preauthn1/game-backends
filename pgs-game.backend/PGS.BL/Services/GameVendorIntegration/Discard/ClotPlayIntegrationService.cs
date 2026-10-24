using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility.Utility;
using target.Utility.Extensions;
using System.Linq;
using target.Models.Models;
using target.Utility;
using target.DA.Repository;
using target.BL.Interface;
using target.Models.ViewModels.GameVendor;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using System.ComponentModel;
using target.Models;
using Newtonsoft.Json;
using System.Text.Json;
using System.Transactions;
using target.Utility.Extensions.Web;
using System.Threading.Tasks;
using System.Threading;

namespace target.BL.Services
{

    public class ClotPlayIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.ClotPlay;
            }
        }


        public override string GameVendorPrefix
        {
            get
            {
                return "clotplay";
            }
        }

        public override double TimeZone
        {
            get
            {
                return 8;  //是的, ClotPlay的时区定在+8
            }
        }

        public override double SettlementTimeZone
        {
            get
            {
                return 8;
            }
        }


        public BetRecordClotPlayRepository BetRecordRepository { get; set; }


        private static object myLock = new object();


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            var model = param.ApiResponseError;

            int defaultErrorCode = -1;

            var defaultResult = new ClotPlayAPIErrorResponse
            {
                Code = defaultErrorCode,
                Msg = "Error",
            };
           
            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                defaultResult.Code = 4004; 
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                defaultResult.Code = 1000;
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                defaultResult.Code = defaultErrorCode;
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                //如果retry重複結算 回應餘額
                if (apiUrl.ContainsWihtoutCase("api/seamless/betSettlement"))
                {
                    var parseValue = new SystemConfigSettingParseModel(0, HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_AfterAmount));

                    defaultResult.Code = 0;
                    defaultResult.Data = new ClotPlayBalanceResponse
                    {
                        Balance = parseValue.SettingValueOfDecimal
                    };
                }
                else
                {
                    defaultResult.Code = defaultErrorCode;
                }
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                defaultResult.Code = defaultErrorCode;
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                defaultResult.Code = defaultErrorCode;
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                defaultResult.Code = 1021;
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                defaultResult.Code = 2002;
            }

            defaultResult.Msg = model.Message;

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = 200,
                PGSDebug = debugInfo,
                GameVendorCustomizedResponse = defaultResult
            };
        }


        public ConvertPerGameLogResultModel FormatParsing_ConvertBetRecordToPerGameDetailLog(string betRecordNoPGS)
        {
            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryBetRecord == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }

            if (queryBetRecord.Status != (int)BetRecordStatusEnum.SettleComplete &&
                queryBetRecord.Status != (int)BetRecordStatusEnum.Cancel)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }

            var perGame = BetRecordConvertToPerGame(queryBetRecord);

            return new ConvertPerGameLogResultModel
            {
                PerGameDetailLog = perGame
            };
        }


        public BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
        {
            var queryRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);
            var convert = queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();

            return convert;
        }


        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = SelectColumns_BriefQuery;

            BetRecordClotPlayEntity queryRecord = null;

            List<QueryBetRecordGeneralRequestViewModel> loopTimeQuery = InstanceLoopTimesQueryTimeRange(model, isFilterGame: false);

            //根據指定的條件不同。最多會進行三次查詢
            foreach (var filter in loopTimeQuery)
            {
                var queryRecords = BetRecordRepository.GetByFilter(filter, selectColumns);

                if (queryRecords.IsNotEmptyNull())
                {
                    queryRecord = queryRecords.FirstOrDefault();
                    break;
                }
            }

            if (queryRecord == null)
                return null;

            var result = queryRecord.ReturnInjecteObject<BetRecordBriefInfoModel>();
            return result;
        }


        public void WriteRawBetRecordWhenPlaceBet(InsertRawBetRecordRequiredInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();

            var insert = InstanceInsertBetRecordEntity<BetRecordClotPlayEntity>(model);
            insert.PartitionTime = model.PartitionTime;

            BetRecordRepository.Create(insert);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.ClotPlay).Rule_ModelNotNull();

            var settleInfo = rawInfo.ClotPlay.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = model.PayoutGamePoint;
            decimal jackpotPoint = 0;
            decimal taxAmount = 0;

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
            parseModel.TaxAmount = taxAmount / model.ConvertRateWhenPlaceBet;

            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                RoundID = settleInfo.game_round_id,
                NewBetAmount = model.NewBetAmount,
                ThirdPartySettleTime = DateTimeHelper.ConvertUnknowFormatTextToUtcDateTime(settleInfo.updated_at).AddHours(-8), //-8是因為要轉成UTC+0
            };

            parseModel.ExtraColumn.UseBetAmountAsValidAmount = true;

            var result = UpdateRawBetRecord(parseModel);
            return result;
        }


        public SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.GetCurrentTime();

            //查詢 (不包在transaction裡面) update的時候 where條件會包含status
            var queryExist = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            //檢查狀態合法
            RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, BetRecordStatusEnum.SettleComplete, model.PayoutAmount);
            RepositoryHelper.CheckBetRecordStatusIsBlackList(queryExist, BetRecordStatusEnum.Cancel);

            SettlementBetRecordInfoModel settleResult = null;

            using (var scope = AppHelper.InstanceTransactionScopeByLevel(IsolationLevel.ReadCommitted))
            {
                queryExist.UpdateTime = now;
                queryExist.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
                queryExist.PayoutAmount = model.PayoutAmount;

                if (model.ExtraColumn.NewBetAmount.HasValue)
                {
                    queryExist.BetAmount = model.ExtraColumn.NewBetAmount.Value;
                }

                // 1. 結算回調的時候對方沒有提供有效流水 自己算
                // 2. 後台手動結算的時候 可以自己輸入有效流水
                if (model.ExtraColumn.UseBetAmountAsValidAmount)
                {
                    queryExist.ValidAmount = queryExist.BetAmount;
                }
                else
                {
                    queryExist.ValidAmount = model.ValidAmount;
                }

                queryExist.RoundID = model.ExtraColumn?.RoundID;
                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;
                queryExist.SettlementTime = now;  
                queryExist.ThirdSettleTime = DateTimeHelper.ReplaceMillisecondsToZero(model.ExtraColumn?.ThirdPartySettleTime);

                BetRecordRepository.UpdateToSettlementStatus(queryExist);

                scope.Complete();

                settleResult = queryExist.ReturnInjecteObject<SettlementBetRecordInfoModel>();
            }

            return settleResult;
        }



        public ExternalVendorCancelBetResultModel CancelBet(string betRecordNoPGS, string memo)
        {
            int merchantID = 0;
            DateTime now = DateTimeHelper.GetCurrentTime();

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                var queryExist = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

                //檢查狀態合法
                RepositoryHelper.CheckBetRecordStatusIsWhiteList(queryExist, BetRecordStatusEnum.Pending);

                merchantID = queryExist.MerchantID;

                queryExist.UpdateTime = now;
                queryExist.SettlementTime = now;
                queryExist.Status = BetRecordStatusEnum.Cancel.ToInteger();

                BetRecordRepository.Update(queryExist);
                scope.Complete();
            }

            return new ExternalVendorCancelBetResultModel
            {

            };
        }


        public ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model)
        {
            var result = new ExternalVendorSessionTokenCheckResultModel
            {

            };

            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            VendorSessionTokenEntity queryVendorSessionToken = [REDACTED];

            if (queryVendorSessionToken == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
            }

            if (model.PlayerID != 0 && queryVendorSessionToken.PlayerID != model.PlayerID)
            {
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
            }

            if (!model.IsSkipCheckExpired && queryVendorSessionToken.ExpiredTime <= DateTimeHelper.GetCurrentTime())
            {
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
            }

            result.PlayerID = queryVendorSessionToken.PlayerID;
            result.GameLaunchExtraSetting = queryVendorSessionToken?.ExtraSettingJson.JsonConvertToModel<GameLaunchExtraSettingModel>();

            return result;
        }

        public SignGenerateCheckResult GenerateSignHash(string jsonText, string secretKey)
        {
            string beforeSign = (jsonText + secretKey);
            string hash = beforeSign.MD5Hash(System.Text.Encoding.UTF8);

            return new SignGenerateCheckResult
            {
                BeforeHash = beforeSign,
                AfterHash = hash
            };
        }


        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            //遊戲商會自動建 不用處理
        }


        public GameListIdInfoBrief GetGameByThirdGameCode(string thirdGameCode)
        {
            return base.GetGameByThirdGameCode_V2(thirdGameCode);
        }


        public ExternalVendorLaunchUrlViewModel GetGameLaunchUrl(ExternalVendorLaunchUrlRequestModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            var queryGame = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameVendorTypes = GameVendor.ToInteger().ToListCollection(),
                    ThirdPartyGameCode = model.ThirdPartyGameCode
                })
                .FirstOrDefault();

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string gameCode = model.ThirdPartyGameCode;
            string token = [REDACTED];

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            var req = new RequestParameters
            {
                ApiUrl = "/launch-game",
                Method = HttpMethodType.HttpPost,
            };

            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            bodyDic.Add("player_unique_id", model.PlayerID.ToString());
            bodyDic.Add("player_name", model.PlayerID.ToString());
            bodyDic.Add("player_currency", ConvertGameVendorCurrencyCode(queryMerchant.Currency));
            bodyDic.Add("game_id", gameCode);
            bodyDic.Add("is_demo", model.IsTrialMode);
            bodyDic.Add("language", lang);
            bodyDic.Add("agent_code", assignAgentID);

            if(!string.IsNullOrWhiteSpace(model.BackUrlShort))
            {
                bodyDic.Add("callback_url", model.BackUrlShort.UrlDecode());
            }

            //var reqeustBody = new
            //{
            //    player_unique_id = model.PlayerID.ToString(),
            //    player_name = model.PlayerID.ToString(),
            //    player_currency = ConvertToClotPlayCurrencyCode(queryMerchant.Currency),
            //    game_id = gameCode,
            //    is_demo = model.IsTrialMode,
            //    language = lang,
            //    agent_code = assignAgentID,
            //    //orientation = "portrait",
            //    callback_url =  ?? "",
            //};

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetGameLaunchUrl", bodyDic);

            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<ClotPlayAPICommonResponse<ClotPlayGameLaunchResponseModel>>(req , bodyDic);

            CheckResponseIsError(apiResult);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = apiResult.ResultObject.Data?.Game_url
            };
        }


        private string ConvertGameVendorCurrencyCode(string currency)
        {
            if (currency == "MYR100")
                return "SCR";

            if (currency == "BETA")
                return "CNY";

            if (currency == "KVND")
                return "VNDK";

            if (currency == "KIDR")
                return "IDRK";

            if (currency == "KMMK")
                return "MMKK";

            if (currency == "KCOP")
                return "COPK";

            return currency;
        }

        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            //參考文件.pdf  5.1  启动游戏 API
            var req = new RequestParameters
            {
                ApiUrl = $"/bet-details?game_round_id={queryBetRecord.RoundID}",
                Method = HttpMethodType.HttpGet,
            };

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryBetRecord.MerchantID, "GetExternalBetDetailInfo", queryBetRecord.BetRecordNoPGS);
                       
            //對應model JiliRecordDetailExternalUrlResultModel.cs
            var res = APIRequestHelper.RequestApiResult<ClotPlayAPICommonResponse<ClotPlayScreenshotResponseModel>>(req);

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = res.ResultObject?.Data?.history_url
            };
        }


        public ApiResultBriefObjectModel GetSingleBetRecordRawResponseByApi(string betRecordNoPGS)
        {
            new { BetRecordNoPGS = betRecordNoPGS }.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            throw new TerproException(ApiErrorCode.NotImplement);
        }


        public string BeautifyJson(string unPrettyJson)
        {
            var jsonElement = JsonConvert.DeserializeObject<dynamic>(unPrettyJson);
            return JsonConvert.SerializeObject(jsonElement, Formatting.Indented);
        }



        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "get-game-list",
                Method = HttpMethodType.HttpGet,
                TimeoutMillisecond = 1000 * 90
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "FetchGames", requestPayload: null);

            var apiResult = APIRequestHelper.RequestApiResult<ClotPlayAPICommonResponse<List<ClotPlayGameListResponseModel>>>(req , null);

            CheckResponseIsError(apiResult);

            var litsResult = new List<ThirdPartyVendorFetchGameListConvertModel>();

            if(apiResult.ResultObject.Data.IsNotEmptyNull())
            {
                var slotsGames = apiResult.ResultObject.Data;

                foreach (var item in slotsGames)
                {
                    Dictionary<LanguageEnum, string> langDic = new Dictionary<LanguageEnum, string>();
                    langDic.Add(LanguageEnum.zh_cn, item.name_cn);
                    langDic.Add(LanguageEnum.zh_tw, item.name_cn);
                    langDic.Add(LanguageEnum.en_us, item.name);

                    string thirdGameCode = item.slug;

                    litsResult.Add(new ThirdPartyVendorFetchGameListConvertModel
                    {
                        GameCode = $"{GameVendorPrefix}_" + thirdGameCode,
                        GameName = item.name,
                        GameType = GameTypeEnum.Slot.ToInteger(),
                        LanguageText = langDic,
                        GameVendorType = GameVendorTypeEnum.Jili.ToInteger(),
                        ThirdPartyGameCode = thirdGameCode
                    });
                }
            }

            return litsResult;
        }



        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string begin_UTC8 = model.StartTimeUTC.ToString("yyyy-M-dd HH:mm:ss");
            string end_UTC8 = model.EndTimeUTC.ToString("yyyy-M-dd HH:mm:ss");

            int pageSize = 1000;
            model.Page = model.Page <= 0 ? 1 : model.Page;

            string queryString = $"?updated_at_from={begin_UTC8}&updated_at_to={end_UTC8}&perPage={pageSize}&page={model.Page}&agent_code={assignAgentID}";

            var req = new RequestParameters
            {
                ApiUrl = "/get-transaction-details" + queryString,
                Method = HttpMethodType.HttpGet,
                TimeoutMillisecond = 1000 * 90
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "FetchWagers" , null);
           
            var apiResult = APIRequestHelper.RequestApiResult<ClotPlayFetchWagersApiResponseModel>(req);

            var result =  new ExternalVendorFetchWagersResult
            {
                ResponseText = apiResult.ResponseText,
                PageSize = pageSize
            };

            var myResultObj = apiResult.ResultObject;

            if (myResultObj != null && myResultObj.Data != null && myResultObj.Data.IsNotEmptyNull())
            {
                result.ConvertInsertEntity = myResultObj.Data
                          .Select(x => new ThirdPartyFetchRecordBriefEntity
                          {
                              BetAmount = x.bet_amount,
                              PayoutAmount = x.winning,
                              WagerID = x.Id,
                              SettleTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.updated_at, 4),
                              GameCode = x.game_id,
                              BetTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.created_at, 4),
                              VendorConfigID = model.VendorConfigID,
                              DebugInfo1 = x.game_round_id
                          })
                          .ToList();
            }

            return result;
        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameIDs = model.Debug_GameIDs,
                GameVendorTypes = GameVendor.ToInteger().ToListCollection()
            });

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            var queryVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            var firstMatchGame = queryGames.OrderBy(x => x.GameID).FirstOrDefault();

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            bool isProductionDB = SettingHelper.IsProductionDbConnection;

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string date_from_Text = model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd");
            string date_to_Text = model.EndTimeUTC.AddDays(-1).AddHours(8).ToString("yyyy-MM-dd");

            string queryString = $"date_from={date_from_Text}&date_to={date_to_Text}&agent_code={assignAgentID}";

            var req = new RequestParameters
            {
                ApiUrl = $"/get-daily-report?{queryString}",
                Method = HttpMethodType.HttpGet,
            };
                         
            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "FetchEachGameAccountingReport", null);

            req.AdvancedSetting.Log_DebugInfo1 = model.VendorConfigID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<ClotPlayAPICommonResponse<List<ClotPlayDailyReportAPIResponseModel>>>(req);

            if(apiResult.ResultObject != null && apiResult.ResultObject.Data.IsNotEmptyNull())
            {
                var fetchReporttData = apiResult.ResultObject.Data;

                if (queryVendorConfig.SupportCurrency.IsNotEmptyTextNotNull())
                {
                    fetchReporttData = fetchReporttData.Where(x => x.currency == queryVendorConfig.SupportCurrency).ToList();
                }

                decimal bet = fetchReporttData.Select(x=> x.total_turnover).Sum();
                decimal payout = fetchReporttData.Select(x => x.total_winning).Sum();
                int totalCount = fetchReporttData.Select(x => x.round_count).Sum();

                viewModels.Add(new GameVendorAccountingSummaryReportModel
                {
                    ThirdPartyGameCode = firstMatchGame.ThirdPartyGameCode,
                    StatDate = model.StartTimeUTC.AddHours(8),
                    TotalBetAmount = bet,
                    TotalBetCount = totalCount,
                    TotalPayoutAmount = payout,                    
                    TotalWinLoss = AppHelper.CalculateWinLoss(bet, payout),
                });
            }

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = assignAgentID,
                EachGameSummary = viewModels
            };
        }


        private Dictionary<string, object> ConvertDetailObjectToDic(object detailObject)
        {
            if (detailObject == null)
                return new Dictionary<string, object>();

            Dictionary<string, object> valueParse = new Dictionary<string, object>();

            try
            {
                valueParse = detailObject.ConvertToJsonString().JsonConvertToModel<Dictionary<string, object>>();
            }
            catch
            {

            }

            return valueParse ?? new Dictionary<string, object>();
        }


        private void CheckResponseIsError<T>(ApiRequestResultModel<ClotPlayAPICommonResponse<T>> res) where T : class
        {
            var gameVendorText = GameVendor.ToString().ToLower();
            var gameVendorID = GameVendor.ToInteger();

            var extraInfo = new ErrorDetailModel
            {
                ColumnName = $"{gameVendorID} , {DateTimeHelper.GetCurrentTimeYYYY_MM_DD_HH_MM_SS(8)} (GMT+8)",
                LogExtraInfo = gameVendorText
            };

            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
            {
                extraInfo.ColumnName += $"statusCode: {res.ResponseStatusCode}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail, extraInfo);

            if (res.ResultObject.Code != 0)
            {
                extraInfo.LogExtraInfo = res.ResponseText;
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }
        }


        private void SetRequestInfoByMerchantID(RequestParameters req, int merchantID, string apiCategory, object requestPayload)
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

            SetCommonRequestInfo(req, apiCategory, requestPayload);
        }


        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, object requestPayload)
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            SetCommonRequestInfo(req, apiCategory, requestPayload);
        }


        private RequestParameters InstanceRequestParameter(string apiUrl, HttpMethodType method, object requestPayload)
        {
            var req = new RequestParameters
            {
                ApiUrl = "/singleWallet/LoginWithoutRedirect",
                Method = HttpMethodType.HttpGet,
            };

            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            string payloadText = requestPayload == null ? "" : requestPayload.ConvertToJsonString();
            string sign = $"{payloadText}{ts}{secretKey}";

            req.AddHeaderData("X-Atgame-Mchid", assignMerchantID);
            req.AddHeaderData("X-Atgame-Timestamp", ts.ToString());
            req.AddHeaderData("X-Atgame-Sign", sign.ToUpper());

            return req;
        }



        private void SetCommonRequestInfo(RequestParameters req, string apiCategory, object requestPayload)
        {
            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;

            req.AddHeaderData("Authorization", secretKey);

            req.LogAction = WriteRequestLog;
            req.UserAgent = "target";
            req.AdvancedSetting = new AdvancedSetting
            {
                IsIgnoreJsonParsingColumnError = true,
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.ToString(),
            };

            req.BaseUrl = apiDomain;
        }

    }
}
