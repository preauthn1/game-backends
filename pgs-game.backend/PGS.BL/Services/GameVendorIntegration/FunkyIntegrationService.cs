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
using System.Transactions;
using Lck.Utility.Extensions.Helper;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class FunkyIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.Funky;
            }
        }


        public override string GameVendorPrefix
        {
            get
            {
                return GameVendorDefinitionHelper.GetVendorCodePrefix(GameVendor);
            }
        }

        public override double TimeZone
        {
            get
            {
                return 0;
            }
        }


        public BetRecordFunkyRepository BetRecordRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            //funky的API錯誤碼 有定義了10組左右的errorCode 末三碼會轉成Funky文件定義的編號 (請參考文件 错误代码列表)
            ApiErrorCode defaultError = ApiErrorCode.FunkyApi_InvalidInput;

            var model = param.ApiResponseError;
            int errorCode = model.Code % 1000;

            if (errorCode < 400 || errorCode > 415)
            {
                errorCode = defaultError.ToInteger() % 1000;
            }

            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                //遊戲未定義
                errorCode = 10005;
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                errorCode = 409;
            }
            else if (model.Code == (int)ApiErrorCode.NotFoundBetRecord)
            {
                //遊戲未定義
                errorCode = 404;
            }
            else if (model.Code == (int)ApiErrorCode.WalletTranferFail_WalletAmountNotEnough)
            {
                //錢包餘額不足
                errorCode = 402;
            }

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = 200,
                GameVendorCustomizedResponse = new FunkyAPIResponseModel<string>
                {
                    ErrorCode = errorCode,
                    ErrorMessage = ((ApiErrorCode)model.Code).ToString(),
                    Data = null
                }
            };
        }



        public ConvertPerGameLogResultModel FormatParsing_ConvertBetRecordToPerGameDetailLog(string betRecordNoPGS)
        {
            var queryBetRecord = GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryBetRecord == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }

            if (queryBetRecord.Status != (int)BetRecordStatusEnum.SettleComplete)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }

            var perGame = BetRecordConvertToPerGame(queryBetRecord);

            return new ConvertPerGameLogResultModel
            {
                PerGameDetailLog = perGame
            };
        }


        public void WriteRawBetRecordWhenPlaceBet(InsertRawBetRecordRequiredInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            var insert = InstanceInsertBetRecordEntity<BetRecordFunkyEntity>(model);
            insert.PartitionTime = model.PartitionTime;

            BetRecordRepository.Create(insert);
        }


        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.Funky).Rule_ModelNotNull();

            var settleInfo = rawInfo.Funky.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = settleInfo.betResultReq.winAmount;
            decimal validAmount = settleInfo.betResultReq.effectiveStake;

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.ValidAmount = validAmount / model.ConvertRateWhenPlaceBet;

            var result = UpdateRawBetRecord(parseModel);
            return result;
        }


        public SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.ReplaceMillisecondsToZero(DateTimeHelper.GetCurrentTime());

            //查詢 (不包在transaction裡面) update的時候 where條件會包含status
            var queryExist = GetByBetRecordNoPGS(model.BetRecordNoPGS);

            //檢查狀態合法
            RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, BetRecordStatusEnum.SettleComplete, model.PayoutAmount);
            RepositoryHelper.CheckBetRecordStatusIsBlackList(queryExist, BetRecordStatusEnum.Cancel);

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                queryExist.UpdateTime = now;
                queryExist.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
                queryExist.PayoutAmount = model.PayoutAmount;
                queryExist.ValidAmount = model.ValidAmount;
                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;
                queryExist.SettlementTime = now;
                queryExist.ThirdSettleTime = now;

                //特殊遊戲處理，如果是捕魚遊戲 對方會預扣所有錢包金額 導致投注金額/派彩金額 等於 錢包金額
                //因此投注金額 改成使用 validAmount & 派彩金額則是 輸贏+有效流水
                if (queryExist.GameID == 250009 || queryExist.GameID == 250010 || queryExist.GameID == 250021)
                {
                    decimal payout = queryExist.PayoutAmount - queryExist.BetAmount;
                    queryExist.BetAmount = model.ValidAmount;
                    queryExist.PayoutAmount = model.ValidAmount + queryExist.WinLossAmount;
                }

                BetRecordRepository.UpdateToSettlementStatus(queryExist);
                scope.Complete();

                var recordInfo = queryExist.ReturnInjecteObject<SettlementBetRecordInfoModel>();
                return recordInfo;
            }

        }


        public FunkyAPIResponseModel<FunkySingleWalletAPICheckBetResponseModel> CheckBet(string betRecordNo)
        {
            var queryRecord = BetRecordRepository.GetByBetRecordNo(betRecordNo);

            if (queryRecord == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);
            }

            //R：正在进行
            //W：赢了
            //L：输了
            //C：取消
            //D：平局

            string statusText = string.Empty;

            if (queryRecord.Status == BetRecordStatusEnum.Pending.ToInteger())
            {
                statusText = "R";
            }
            else if (queryRecord.Status == BetRecordStatusEnum.Cancel.ToInteger())
            {
                statusText = "C";
            }
            else if (queryRecord.WinLossAmount > 0)
            {
                statusText = "W";
            }
            else if (queryRecord.WinLossAmount < 0)
            {
                statusText = "L";
            }
            else
            {
                statusText = "D";
            }

            return new FunkyAPIResponseModel<FunkySingleWalletAPICheckBetResponseModel>
            {
                Data = new FunkySingleWalletAPICheckBetResponseModel
                {
                    Stake = queryRecord.BetAmount,
                    RefNo = queryRecord.BetRecordNo,
                    WinAmount = queryRecord.PayoutAmount,
                    Status = statusText,
                    StatementDate = queryRecord.SettlementTime.HasValue ? queryRecord.SettlementTime.Value.AddHours(8).ToString("yyyy-MM-dd") : ""
                }
            };
        }



        public ExternalVendorCancelBetResultModel CancelBet(string betRecordNoPGS, string memo)
        {
            var queryRecord = GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryRecord == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);
            }

            //根據遊戲商要求 實作不同的errorCode
            if (queryRecord.Status == BetRecordStatusEnum.SettleComplete.ToInteger())
            {
                throw new TerproException(ApiErrorCode.FunkyApi_Bet_Was_Already_Settled);
            }

            if (queryRecord.Status == BetRecordStatusEnum.Cancel.ToInteger())
            {
                throw new TerproException(ApiErrorCode.FunkyApi_Bet_Was_Already_Cancelled);
            }

            if (queryRecord.Status != BetRecordStatusEnum.Pending.ToInteger())
            {
                throw new TerproException(ApiErrorCode.FunkyApi_Bet_Was_Already_Cancelled);
            }

            //更新狀態
            queryRecord.Status = BetRecordStatusEnum.Cancel.ToInteger();
            queryRecord.UpdateTime = DateTimeHelper.GetCurrentTime();

            BetRecordRepository.Update(queryRecord);

            return new ExternalVendorCancelBetResultModel
            {
            };
        }

        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = SelectColumns_BriefQuery;

            BetRecordFunkyEntity queryRecord = null;

            model.IsPartitionMatchQuery = true;

            List<QueryBetRecordGeneralRequestViewModel> loopTimeQuery = InstanceLoopTimesQueryTimeRange(model, isFilterGame: false);

            //根據指定的條件不同。最多會進行三次查詢
            foreach (var filter in loopTimeQuery)
            {
                filter.DbQueryExtraConfig = new DbQueryConfig
                {
                    IsForceConvertToDbValueType = true
                };

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


        public BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
        {
            var queryRecord = GetByBetRecordNoPGS(betRecordNoPGS);
            return queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();
        }



        public ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model)
        {
            var defaultResult = new ExternalVendorSessionTokenCheckResultModel();

            string headers = HeaderValueHelper.GetHeaderValueByKey("Authentication");

            //檢查遊戲商的header是否有密鑰
            if (headers != "23d828a8-0bba-49e8-90a6-9007bb50de1f")
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);

            if (model.IsSkipCheckSessionToken)
                return defaultResult;

            model.RuleFor(x => x.SessionOrToken).Rule_NotEmptyText();

            var query = VendorSessionTokenRepository.GetTokenByCache(model.SessionOrToken);

            if (query == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
            }

            if (model.PlayerID != 0 && query.PlayerID != model.PlayerID)
            {
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
            }

            if (query.ExpiredTime <= DateTimeHelper.GetCurrentTime())
            {
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
            }

            return defaultResult;
        }


        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            //funky會自動建 不用處理
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

            //參考文件.pdf  5.1  启动游戏 API
            var req = new RequestParameters
            {
                ApiUrl = "/Funky/Game/LaunchGame",
                Method = HttpMethodType.HttpPost,
            };

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, model.MerchantID, "LaunchGame", model.DebugReferenceUUID);

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            if (model.Language == LanguageEnum.zh_tw)
            {
                model.Language = LanguageEnum.zh_cn;
            }

            bool isAlwaysTrialMode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_IsAlwaysTrialMode, false).SettingValueOfBoolean;

            //如果有強制指定要用試玩模式
            if (isAlwaysTrialMode)
            {
                model.IsTrialMode = true;

                var queryGame = GameListRepository.GetById(model.GameID);

                var trailModeMerchant = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.GameSetting_TrialModeDefaultMerchant);

                //捕魚遊戲的網址比較特殊
                if (queryGame.GameType != (int)GameTypeEnum.Fishing &&
                    trailModeMerchant.SettingValueOfInt == model.MerchantID)
                {
                    var urlSetting = GetVendorSettingByTempVariable(VendorConfigSettingEnum.LaunchGameStaticLinkUrl, false);

                    if(!string.IsNullOrWhiteSpace(urlSetting.SettingValue))
                    {
                        //Funky遊戲商希望我們使用這種模式
                        return new ExternalVendorLaunchUrlViewModel
                        {
                            Url = $"{urlSetting.SettingValue}/demo?gameCode={model.ThirdPartyGameCode}&lang=EN".ReplaceDuplicateUrlChar()
                        };
                    }
                }
            }

            var body = new
            {
                gameCode = model.ThirdPartyGameCode,
                userName = model.PlayerName,
                playerId = model.PlayerID,
                currency = ConvertGameVendorCurrencyCode(model.Currency),
                language = model.Language.ToString().ToUpper(),
                playerIp = "0.0.0.0", //使用假IP 馬來西亞 (對方禁止使用內部IP)
                sessionId = model.SessionToken,
                isTestAccount = model.IsTrialMode,
                redirectUrl = model.BackUrlEncode
            };

            var res = RequestApiResult<FunkyVendorApiResponseModel<FunkyVendorApiLaunchResponseModel>>(req, body);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res);

            if (res.ResultObject.errorCode != 0)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, InstanceErrorLogInfo(res));

            if (res.ResultObject.data.gameUrl.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected, InstanceErrorLogInfo(res));

            string gameUrl = res.ResultObject.data.gameUrl;
            string token = [REDACTED];
            string finalUrl = $"{gameUrl}?token=[REDACTED]";

            //

            //if (!string.IsNullOrWhiteSpace(model.BackUrlEncode))
            //{
            //    finalUrl += $"&redirectUrl={model.BackUrlEncode}";
            //}

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = finalUrl
            };
        }


        public string ConvertGameVendorCurrencyCode(string currency)
        {
            if (currency == "MYR100")
                return "pMYR";

            if (currency == "BETA")
                return "MYR";

            if (currency == "KVND")
                return "kVND";

            if (currency == "KIDR")
                return "kIDR";

            if (currency == "KMMK")
                return "kMMK";

            if (currency == "KCOP")
                return "kCOP";

            return currency;
        }



        private ErrorDetailModel InstanceErrorLogInfo<T>(ApiRequestResultModel<T> res, bool isAlwaysNotify = false)
        {
            var gameVendorID = GameVendor.ToInteger();

            return new ErrorDetailModel
            {
                IsAlwaysNotify = isAlwaysNotify,
                ColumnName = $"{gameVendorID} statusCode: {res.ResponseStatusCode} ,{DateTimeHelper.GetCurrentTimeYYYY_MM_DD_HH_MM_SS(8)} (GMT+8) ",
                LogExtraInfo = "FunkyAPI_" + res.RelativeUrl + "_" + res.ResponseText,
            };
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            //if(SettingHelper.IsProduction && IPHelper.IsCompanyIP())
            //{
            //    throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow, new ErrorDetailModel("You need use VPN. We don't allow Taiwan IPAddress."));
            //}

            var queryBetRecord = GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            //參考文件.pdf  8.1  获取赌注详情 API
            var req = new RequestParameters
            {
                ApiUrl = "/Funky/Bet/GetBetDetail",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryBetRecord.MerchantID, "GetBetDetail");

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            var body = new
            {
                gameCode = queryGame.ThirdPartyGameCode,
                playerId = queryBetRecord.PlayerID,
                refNo = queryBetRecord.BetRecordNo,
                lang = "EN"
            };

            var res = RequestApiResult<FunkyVendorApiResponseModel<FunkyVendorApiGetDetailUrlResponseModel>>(req, body);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res);

            if (res.ResultObject.errorCode != 0)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = res.ResultObject.data.url
            };
        }



        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            var queryEN = FetchGameListByAPI(vendorConfigID, "EN");
            var queryCN = FetchGameListByAPI(vendorConfigID, "ZH_CN");

            var groupByCategory = queryEN
                .GroupBy(x => x.gameTypeDescription)
                .Select(x => new
                {
                    GameType = x.Key,
                    Count = x.Count()
                })
                .ToList();

            var gameTypes = string.Join("／", queryEN.Select(z => z.gameTypeDescription).Distinct().ToList());

            //Slot Game
            //Fish Game
            //Multiplayer
            //Keno

            List<ThirdPartyVendorFetchGameListConvertModel> listResult = new List<ThirdPartyVendorFetchGameListConvertModel>();

            foreach (var item in queryEN)
            {
                string gameCode = GameVendorPrefix + "_" + item.gameCode;

                var newInstance = new ThirdPartyVendorFetchGameListConvertModel
                {
                    GameName = item.gameName,
                    GameCode = GameVendorPrefix + "_" + item.gameCode,
                    GameVendorType = this.GameVendorID,
                    ThirdPartyGameCode = item.gameCode,
                    GameType = GameTypeEnum.None.ToInteger(),
                    LanguageText = new Dictionary<LanguageEnum, string>()
                };

                switch (item.gameTypeDescription)
                {
                    case "Slot Game":
                        newInstance.GameType = GameTypeEnum.Slot.ToInteger();
                        break;

                    case "Table Game":
                        newInstance.GameType = GameTypeEnum.BoardGame.ToInteger();
                        break;

                    case "Fish Game":
                        newInstance.GameType = GameTypeEnum.Fishing.ToInteger();
                        break;

                    case "Keno":
                        break;

                    default:
                        break;
                }

                newInstance.LanguageText.Add(LanguageEnum.en_us, item.gameName);

                var matchCN = queryCN.FirstOrDefault(x => x.gameCode == item.gameCode);

                if (matchCN != null)
                {
                    newInstance.LanguageText.Add(LanguageEnum.zh_tw, AppHelper.SimplifiedToTraditional(matchCN.gameName));
                    newInstance.LanguageText.Add(LanguageEnum.zh_cn, matchCN.gameName);
                }

                listResult.Add(newInstance);
            }

            List<string> expectGameIDs = new List<string>
            {
                "60279",
                "60272",
                "60274",
                "60276",
                "60277",
                "602716",
                "602850",
                "602713",
                "602712",
                "190264",
                "190263",
                "190262",
                "190261",
                "880261",
                "8802638",
                "60261",
                "102626",
                "802601",
                "102624",
                "102635",
                "102636",
                "102630",
                "102632",
                "102625",
                "102629",
                "102637",
                "102634",
                "102633",
                "802604",
            };

            var myfetchGames = queryEN.Where(x => expectGameIDs.Contains(x.gameCode)).ToList();

            if (myfetchGames.Count() != expectGameIDs.Count())
            {
                string qqq = "獲取遊戲清單不一致";
            }

            listResult = listResult
                .Where(x => expectGameIDs.Contains(x.ThirdPartyGameCode))
                .ToList();

            //只撈特定遊戲類別
            //List<int> availableGameTypes = new List<int>
            //            {
            //                GameTypeEnum.Slot.ToInteger(),
            //                GameTypeEnum.BoardGame.ToInteger()
            //            };

            //listResult = listResult
            //    .Where(x => availableGameTypes.Contains(x.GameType))
            //    .ToList();

            return listResult;
        }


        private List<FunkyVendorApiGetGamesItemModel> FetchGameListByAPI(int vendorConfigID, string languageCode)
        {
            //參考文件.pdf  4.1  获取游戏列表 API
            var req = new RequestParameters
            {
                ApiUrl = "/Funky/Game/GetGameList",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, vendorConfigID, "GetGames");

            var body = new
            {
                language = languageCode,
                gameType = 0
            };

            var res = RequestApiResult<FunkyVendorApiRootResponseModel>(req, body);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res);

            if (res.ResultObject.errorCode != 0)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            if (res.ResultObject.gameList.IsEmptyOrNull())
            {
                return new List<FunkyVendorApiGetGamesItemModel>();
            }

            return res.ResultObject.gameList;
        }


        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();

            //參考文件.pdf  4.1  获取游戏列表 API
            var req = new RequestParameters
            {
                ApiUrl = "/Funky/Report/GetSeamlessBetList",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "GetSeamlessBetList");

            req.BaseUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;

            var queryConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            int pageSize = 1000;

            // 2024/02 備註
            // Funky應該是搜尋betTime，因為只搜尋特定日期 可能會漏掉前一日 23:59:58分的單... 
            var body = new
            {
                startTime = model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss.fff+0800"),
                endTime = model.EndTimeUTC.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss.fff+0800"),
                page = model.Page,
                currency = ConvertGameVendorCurrencyCode(queryConfig.SupportCurrency)
            };

            bool isRequiredNextPage = false;

            var res = RequestApiResult<FunkyVendorWagerApiRootResponseModel>(req, body);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res);

            var listOfWagers = new List<ThirdPartyFetchRecordBriefEntity>();

            if (res.ResultObject != null)
            {
                if (res.ResultObject.errorCode != 0)
                {
                    string debugggggggg = "";
                }
            }

            if (res.ResultObject != null && res.ResultObject.Data.IsNotEmptyNull())
            {
                if (res.ResultObject.TotalPage > res.ResultObject.CurrentPage)
                {
                    isRequiredNextPage = true;
                }

                foreach (var item in res.ResultObject.Data)
                {
                    int playerID = 0;

                    if (item.playerId.IsNumeric())
                    {
                        playerID = Int32.Parse(item.playerId);
                    }

                    DateTime betTime = DateTime.Parse(item.betTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    DateTime settleDate = DateTime.Parse(item.statementDate, null, System.Globalization.DateTimeStyles.RoundtripKind);

                    listOfWagers.Add(new ThirdPartyFetchRecordBriefEntity
                    {
                        WagerID = item.refNo,
                        BetAmount = item.betAmount,
                        PayoutAmount = item.winLoss + item.betAmount,
                        GameCode = item.gameCode,
                        BetTime = betTime,
                        SettleTime = settleDate,
                        VendorConfigID = model.VendorConfigID,
                        PlayerID = playerID,
                    });
                }
            }

            return new ExternalVendorFetchWagersResult
            {
                PageSize = pageSize,
                ConvertInsertEntity = listOfWagers,
                ResponseText = res.ResponseText,
                Funky = new FunkyFetchWagersApiModel
                {
                    IsRequiredNextPage = isRequiredNextPage
                }
            };
        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            var backOfficeApiResult = FetchReportByBackOfficeApiAndCache(model);
            return backOfficeApiResult;
        }



        private ExternalVendorGetReportResultModel FetchReportByBackOfficeApiAndCache(ExternalVendorGetReportRequestParameter model)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            var queryVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            if (string.IsNullOrWhiteSpace(queryVendorConfig.SupportCurrency))
            {
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel("SupportCurrency", true));
            }

            //如果有多個總代。會根據總代來決定使用不同的後台帳號 (e.g. Funky有分成 target、 PGS_V3 兩個總代)
            var rootAgentCategory = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue3, isRequiredException: false)?.SettingValue;

            var cacheKeyLogin = CacheKey.Job_FunkyBackOfficeLoginToken(rootAgentCategory);

            var loginTokenCache = CacheHelper.GetCacheOrQuery(cacheKeyLogin, () =>
            {
                FunkyBackOfficeAPILoginResultObject loginResult = LoginByBackOfficeAPI(rootAgentCategory);
                return loginResult;

            }, expiredMinute: 5);

            if (loginTokenCache == null || loginTokenCache.Token.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel("login token"));
            }

            string dateText = $"{model.StartTimeUTC.ToString("yyyy-MM-dd")}_{model.EndTimeUTC.ToString("yyyy-MM-dd")}";
            var cacheKey = CacheKey.Job_FunkyBackOfficeApiReport(rootAgentCategory, dateText);

            var queryCacheItems = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var fetchReport = FetchDailyReportByBackOfficeAPI(loginTokenCache, model.StartTimeUTC, model.EndTimeUTC);
                   return fetchReport;

               }, expiredMinute: 5);


            SetVendorConfigSettingsToTempVariableByVendorConfigID(queryVendorConfig.VendorConfigID);

            var funkyAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1, isRequiredException: false)?.SettingValue;

            string targetCurrency = ConvertGameVendorCurrencyCode(queryVendorConfig.SupportCurrency.Trim().ToUpper()).ToUpper();

            //只查出幣別有對應到的那筆報表數據
            queryCacheItems = queryCacheItems
                .Where(x => x.Debug_Memo == targetCurrency)
                .ToList();

            if (!string.IsNullOrWhiteSpace(funkyAgentID))
            {
                queryCacheItems = queryCacheItems
                 .Where(x => x.Debug_AgentID == funkyAgentID)
                 .ToList();
            }

            var currencyMatchData = queryCacheItems.FirstOrDefault();

            if (currencyMatchData != null)
            {
                return new ExternalVendorGetReportResultModel
                {
                    EachGameSummary = new List<GameVendorAccountingSummaryReportModel>
                    {
                        currencyMatchData
                    }
                };
            }

            return new ExternalVendorGetReportResultModel
            {
                EachGameSummary = new List<GameVendorAccountingSummaryReportModel>(),
            };
        }


        private FunkyBackOfficeAPILoginResultObject LoginByBackOfficeAPI(string rootAgentCategory)
        {
            var querySettings = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                MatchOtherSettingValue3 = rootAgentCategory,
                GameVendorID = GameVendor.ToInteger(),
                SettingKeys = new List<int>
                    {
                        (int)VendorConfigSettingEnum.BackOffice_BackOfficeApiUrl,
                        (int)VendorConfigSettingEnum.BackOffice_BackOfficeUserName,
                        (int)VendorConfigSettingEnum.BackOffice_BackOfficePassword,
                    },
            })
                .Where(z => !string.IsNullOrWhiteSpace(z.Value))
                .ToList();

            if (querySettings.IsEmptyOrNull() || querySettings.Count() < 3)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    ColumnName = $"{GameVendor} , BackOfficeApiUrl、BackOfficeUserName"
                });
            }

            var baseUrl = GetSettingValueByKey(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeApiUrl);
            var account = GetSettingValueByKey(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeUserName);
            var password = [REDACTED], VendorConfigSettingEnum.BackOffice_BackOfficePassword);

            var param = new RequestParameters
            {
                BaseUrl = baseUrl,
                ApiUrl = "dobby/api/login",
                Method = HttpMethodType.HttpPost
            };

            SetRequestInfoBackOffice(param, "BO_login");

            var body = new
            {
                accountId = account,
                password = [REDACTED]
            };

            var apiResult = RequestApiResult<FunkyBackOfficeAPILoginResponse>(param, body);

            CheckResponseIsError(apiResult);

            if (apiResult.ResultObject.returnCode == "0" && apiResult.ResultObject.token.IsNotEmptyTextNotNull())
            {
                return new FunkyBackOfficeAPILoginResultObject
                {
                    SettingApiUrl = baseUrl,
                    Token = [REDACTED]
                };
            }

            throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel("login", true));
        }


        private List<GameVendorAccountingSummaryReportModel> FetchDailyReportByBackOfficeAPI(FunkyBackOfficeAPILoginResultObject model, DateTime startTimeUTC, DateTime endUTC)
        {
            var queryMatchFirstGame = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameVendorTypes = GameVendor.ToInteger().ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger()
            })
                .FirstOrDefault();

            var param = new RequestParameters
            {
                BaseUrl = model.SettingApiUrl,
                ApiUrl = "dobby/api/report/searchWinLoseReport",
                Method = HttpMethodType.HttpPost,
                AdvancedSetting = new AdvancedSetting(),
            };

            SetRequestInfoBackOffice(param, "BO_searchWinLoseReport");

            param.AddHeaderData("Authorization", "Bearer " + model.Token);

            //查單日 範例: 2023-03-01 ~ 2023-03-01 
            var body = new
            {
                startDate = startTimeUTC.AddHours(8).ToString("yyyy-MM-dd"),
                endDate = endUTC.AddHours(8).AddSeconds(-1).ToString("yyyy-MM-dd"),
                fpId = "All",
                gameProvider = "All",
                includeTestAccount = false,
                isMerged = false
            };

            var apiResult = RequestApiResult<FunkyBackOfficeReportSummaryApiResponse>(param, body);

            CheckResponseIsError(apiResult);

            var targetDate = startTimeUTC.AddHours(8).Date;

            if (apiResult.ResultObject.returnCode == "0" && apiResult.ResultObject.data != null)
            {
                var viewModels = new List<GameVendorAccountingSummaryReportModel>();

                foreach (var eachOperateID in apiResult.ResultObject.data)
                {
                    //Funky偶爾會有怪怪的問題 (同時出現 pMYR、PMYR)
                    var funkyEachCurrecnyReport = eachOperateID.winLoseList
                        .GroupBy(x => (x.currency ?? "").ToUpper())
                        .Select(x => new
                        {
                            Currency = x.Key,
                            Items = x.ToList()
                        })
                        .ToList();

                    foreach (var item in funkyEachCurrecnyReport)
                    {
                        viewModels.Add(new GameVendorAccountingSummaryReportModel
                        {
                            Debug_Memo = item.Currency,
                            Debug_AgentID = eachOperateID.operatorName,
                            StatDate = targetDate,
                            TotalBetAmount = item.Items.Select(x => x.validTurnover).Sum(),
                            TotalValidBetAmount = item.Items.Select(x => x.validTurnover).Sum(),
                            TotalBetCount = item.Items.Select(x => x.betCount).Sum(),
                            TotalWinLoss = item.Items.Select(x => x.playerWinLose).Sum(),
                            TotalPayoutAmount = item.Items.Select(x => x.playerWinLose).Sum() + item.Items.Select(x => x.validTurnover).Sum(),
                            ThirdPartyGameCode = queryMatchFirstGame.ThirdPartyGameCode, //隨便填一個遊戲 (這家遊戲商只有全加總報表)
                        });
                    }
                }

                return viewModels;
            }

            throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel("login", true));
        }


        private BetRecordFunkyEntity GetByBetRecordNoPGS(string betRecordNoPGS)
        {
            return BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS, isNVarchar: true);
        }


        private ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters req, object model = null)
        {
            var apiResult = APIRequestHelper.RequestApiResult<T>(req, model);
            return apiResult;
        }


        private string GetSettingValueByKey(List<VendorConfigSettingEntity> settings, VendorConfigSettingEnum key)
        {
            var value = settings.FirstOrDefault(x => x.SettingKey == (int)key)?.Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    ColumnName = $"{GameVendor} , VendorConfigSetting , settingKey:{key.ToInteger()}"
                });
            }

            return value;
        }



        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res)
        {
            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, InstanceErrorLogInfo(res));

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail, InstanceErrorLogInfo(res));
        }



        private void SetRequestInfoByDBSetting(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "")
        {
            //先查出配置並放到暫存變數
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

            SetRequestInfo(req, apiCategory, requestUUID);
        }

        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, string requestUUID = "")
        {
            //先查出配置並放到暫存變數
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            SetRequestInfo(req, apiCategory, requestUUID);
        }


        private void SetRequestInfo(RequestParameters req, string apiCategory, string requestUUID = "")
        {
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.GetDescriptionOrToString(),
                Log_RequestUUID = requestUUID
            };

            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
            string opeatorID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string apiToken = [REDACTED];

            if (apiDomain.IsNullOrWhiteSpace() || opeatorID.IsNullOrWhiteSpace() || apiToken.IsNullOrWhiteSpace())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted);
            }

            req.BaseUrl = apiDomain;
            req.UserAgent = opeatorID;
            req.AddHeaderData("Authentication", apiToken);
            req.AddHeaderData("X-Request-ID", Guid.NewGuid().ToString());
        }

        private void SetRequestInfoBackOffice(RequestParameters req, string apiCategory)
        {
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.ToString(),
            };
        }


    }
}
