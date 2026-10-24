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

namespace target.BL.Services
{

    public class AGGFishIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.AGGFish;
            }
        }

        public override double TimeZone
        {
            get
            {
                return 0;
            }
        }


        public BetRecordAGGFishRepository BetRecordRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public AGGFishDetailRecordRepository AGGFishDetailRecordRepository { get; set; }


        private static List<int> API_SUCCESS_CODES = new List<int>
        {
            0,
            1000
        };



        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            var model = param.ApiResponseError;
            int retunrCode = 9999; //其他錯誤 (請帶入自訂錯誤訊息)
            int statusCode = 200;

            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                retunrCode = 9998; //系統維護中
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                retunrCode = 2001; //data 不能為空值
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                retunrCode = 9999; //自訂錯誤訊息
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                retunrCode = 2005; //注單編號已結算
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                retunrCode = 2006; //注單編號不存在
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                retunrCode = 9999; //自訂錯誤訊息
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                retunrCode = 1001; //token 驗證失敗
            }

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            if (model.Code == ApiErrorCode.BusinessLogicDeny_SimulateErrorOccur.ToInteger() ||
              model.Code == ApiErrorCode.InternalServerError.ToInteger())
            {
                statusCode = 500;
            }

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = statusCode,
                PGSDebug = debugInfo,
                GameVendorCustomizedResponse = new AGGLiveErrorResponseModel
                {
                    code = retunrCode,
                    message = model.Message,
                }
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
            perGame.TaxAmount = queryBetRecord.TaxAmount;

            bool isZeroRecord = perGame.BetAmount == 0 && perGame.PayoutAmount == 0 && perGame.ValidAmount == 0;

            return new ConvertPerGameLogResultModel
            {
                IsAllAmountZeroThenSkipProcess = isZeroRecord,
                PerGameDetailLog = perGame
            };
        }



        public BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
        {
            var queryRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);
            var convert = queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();
            convert.ExtraInfo1 = queryRecord.RoundID;

            return convert;
        }



        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = "  Id , BetRecordNoPGS , BetRecordNo , Status , ConvertRate , BetAmount , CreateTime , PlayerID , MerchantID , GameID  ";
            var queryRecord = BetRecordRepository.GetByBetRecordNo(model.BetRecordNo, "", selectColumns);

            if (queryRecord == null)
                return null;

            var result = queryRecord.ReturnInjecteObject<BetRecordBriefInfoModel>();
            return result;
        }



        public void WriteRawBetRecordWhenPlaceBet(InsertRawBetRecordRequiredInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            string roundID = rawInfo.AGGFish?.PlaceBetInfo?.roundid;

            var insert = InstanceInsertBetRecordEntity<BetRecordAGGFishEntity>(model);
            insert.RoundID = roundID;
            insert.ThirdPartyGameCode = model.ThirdGameCode;

            BetRecordRepository.Create(insert);
        }


        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.AGGFish).Rule_ModelNotNull();

            var settleInfo = rawInfo.AGGFish.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = model.PayoutGamePoint;
            decimal jackpotPoint = settleInfo.jackpot ? settleInfo.winlose : 0;
            decimal taxAmount = 0;

            if (settleInfo.external == null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "三方請求資訊不完整。缺少 external 物件");

            parseModel.ValidAmount = settleInfo.external.BetAmount;
            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
            parseModel.TaxAmount = taxAmount / model.ConvertRateWhenPlaceBet;
            parseModel.DebugExtraMemo += $"Rounds:{settleInfo.external.Rounds}";
            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                RoundID = settleInfo.roundid,
                ThirdPartySettleTime = ParsingSettleTime(settleInfo.bettime)
            };

            var result = UpdateRawBetRecord(parseModel);
            return result;
        }


        private DateTime? ParsingSettleTime(string timeText)
        {
            if (string.IsNullOrWhiteSpace(timeText))
                return null;

            try
            {
                return DateTime.Parse(timeText);
            }
            catch
            {

            }

            return null;
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.GetCurrentTime();

            //查詢 (不包在transaction裡面) update的時候 where條件會包含status
            var queryExist = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            //檢查狀態合法
            RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, BetRecordStatusEnum.SettleComplete, model.PayoutAmount);

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                queryExist.UpdateTime = now;
                queryExist.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
                queryExist.PayoutAmount = model.PayoutAmount;
                queryExist.JackpotAmount = model.JackpotAmount;
                queryExist.TaxAmount = model.TaxAmount;
                queryExist.ThirdSettleTime = model.ExtraColumn?.ThirdPartySettleTime;

                if (queryExist.ThirdSettleTime.HasValue)
                {
                    queryExist.ThirdSettleDate = queryExist.ThirdSettleTime.Value.AddHours(8).Date;
                }

                if (queryExist.BetAmount == 0)
                {
                    queryExist.BetAmount = model.ValidAmount;
                }

                queryExist.ValidAmount = model.ValidAmount;
                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;
                queryExist.SettlementTime = now;
                queryExist.ManualSettleUserID = model.UserID;

                BetRecordRepository.UpdateToSettlementStatus(queryExist);
                scope.Complete();

                var recordInfo = queryExist.ReturnInjecteObject<SettlementBetRecordInfoModel>();
                return recordInfo;
            }

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

            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            return new ExternalVendorCancelBetResultModel
            {

            };
        }


        public ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model)
        {
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

            if (!model.IsSkipCheckExpired && query.ExpiredTime <= DateTimeHelper.GetCurrentTime())
            {
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
            }

            //AGGLive 如果要檢查hash 則一定要傳token
            var queryPlayer = PlayerRepository.GetSingleByCache(query.PlayerID);


            SetVendorConfigSettingsToTempVariableByMerchantID(queryPlayer.MerchantID);

            if (!model.IsSkipCheckSignHash)
            {
                var queryHashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken);

                var rawPayload = MyHttpContext.Current.Request.GetRequestBodyString();

                var dic = rawPayload.JsonConvertToModel<Dictionary<string, object>>();

                var payload_data = dic["data"];

                var jsonText = payload_data.ConvertToJsonString();

                string myHash = GenerateSignHash(jsonText, queryHashKey.SettingValue).AfterHash;

                if (myHash != model.EncryptSign && !RegexRuleHelper.IsHashMagicPassword(model.EncryptSign))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError, new ErrorDetailModel
                    {
                        IsAlwaysNotify = true
                    });
                }
            }

            return new ExternalVendorSessionTokenCheckResultModel
            {
                PlayerID = queryPlayer.PlayerID,
                GameLaunchExtraSetting = query.ExtraSettingJson.JsonConvertToModel<GameLaunchExtraSettingModel>()
            };
        }

        public SignGenerateCheckResult GenerateSignHash(string jsonText, string secretKey)
        {
            string beforeSign = (jsonText + secretKey);
            string hash = beforeSign.MD5Hash();

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


        public string GetSingleBetRecordByApi(ExternalVendorGetSingleBetRecordRequestParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.BetRecordNo).Rule_NotEmptyText();

            //參考文件.pdf  5.1  启动游戏 API
            var req = new RequestParameters
            {
                ApiUrl = "game/record/replayByWager",
                Method = HttpMethodType.HttpPost,
            };

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, model.MerchantID, "GetBetDetail", model.QueryReplayExtraParameter?.DebugReferenceUUID);


            var body = new
            {
                gameId = model.GameCode,
                wagerId = model.BetRecordNo,
                lang = "zh-TW",
                currency = new
                {
                    currencyCode = queryMerchant.Currency
                }
            };


            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            //對方API透過303 + redirect的方式回傳
            if (res.ResponseStatusCode == 303)
            {
                return res.DebugInfo.RedirectLocationUrl;
            }

            //檢查http錯誤、model解析是否成功
            // CheckResponseIsError(res);

            //if (res.ResultObject.errorCode != 0)
            //    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            //if (res.ResultObject.data.gameUrl.IsNullOrWhiteSpace())
            //    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected);

            return res.ResponseText;
        }

        public GameListIdInfoBrief GetGameByThirdGameCode(string thirdGameCode)
        {
            return base.GetGameByThirdGameCode_V2(thirdGameCode);
        }

        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            throw new TerproException(ApiErrorCode.NotImplement);
        }

        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            throw new TerproException(ApiErrorCode.NotImplement);
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

            if (string.IsNullOrWhiteSpace(queryGame.ThirdPartySecondCode))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("ThirdPartySecondCode"));

            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string assignOperator = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string gameCode = model.ThirdPartyGameCode;
            string token = [REDACTED];

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            //對方技術說是使用 vi_VN、th_TH

            string roomType = "";
            string vcode = queryGame.ThirdPartySecondCode;  //遊戲商代碼
            var vip = 0;

            var req = new RequestParameters
            {
                ApiUrl = "/play/login",
                Method = HttpMethodType.HttpPost,
            };

            //merchantid string ­營運商編號
            //usertoken string 營運商token
            //vcode string 遊戲商代碼
            //gameid string 由現代馬
            //language string 
            //launchfrom string 
            //currency string 
            //roomType string
            //vip string

            var postParam = new
            {
                merchantid = assignMerchantID,
                @operator = assignOperator,
                usertoken = [REDACTED],
                sessiontoken = "",
                vcode = vcode,
                gameid = gameCode,
                language = lang,
                launchfrom = "",
                currency = model.Currency,
                roomtype = roomType,
                vip = vip,
                backurl = model.BackUrlEncode
            };

            var hashResult = ToHashEncrypt(postParam, encryptKey);
            var postBody = new
            {
                data = postParam,
                hash = hashResult.AfterHash
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryMerchant.MerchantID, "GetGameLaunchUrl", token, "");
            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<AGGLiveLaunchGameResponseModel>(req, postBody);

            CheckResponseIsError(apiResult);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = apiResult.ResultObject.result
            };
        }


        private (string BefroeHash, string AfterHash) ToHashEncrypt(object param, string encryptKey)
        {
            Dictionary<string, object> columnValues = new Dictionary<string, object>();

            //對方說會以我方的欄位順序當基準加密
            foreach (var item in param.GetType().GetProperties())
            {
                columnValues.Add(item.Name, item.GetValue(param));
            }

            string before = columnValues.ConvertToJsonString() + encryptKey;
            string after = before.MD5Hash();

            return (before, after);
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            throw new TerproException(ApiErrorCode.NotImplement);

            //var queryBetRecord = BetRecordRepository.GetByBetRecordNo(model.BetRecordNo, model.ThirdPartyGameCode);

            //if (queryBetRecord == null)
            //    throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            //var apiResult = GetSingleBetRecordByApi(new ExternalVendorGetSingleBetRecordRequestParameter
            //{
            //    BetRecordNo = model.BetRecordNo,
            //    MerchantID = model.MerchantID,
            //    GameCode = model.ThirdPartyGameCode
            //});

            //return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            //{
            //    InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
            //    Url = apiResult
            //};
        }


        public string BeautifyJson(string unPrettyJson)
        {
            var jsonElement = JsonConvert.DeserializeObject<dynamic>(unPrettyJson);
            return JsonConvert.SerializeObject(jsonElement, Formatting.Indented);
        }



        //public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        //{
        //    model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
        //    model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
        //    model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

        //    var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
        //    {
        //        IsNotDelete = true,
        //        GameVendorTypes = GameVendor.ToInteger().ToListCollection()
        //    });

        //    var req = new RequestParameters
        //    {
        //        ApiUrl = "/api/operator/DailyReport",
        //        Method = HttpMethodType.HttpPost,
        //    };

        //    //apiToken、api域名 吃資料庫設置
        //    SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "DailyReport", model.DebugRequestUUID);

        //    string merchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
        //    string operatorID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
        //    string hashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

        //    var viewModels = new List<GameVendorAccountingSummaryReportModel>();

        //    double myTimeZone = 8;

        //    var firstFishGame = queryGames.FirstOrDefault();

        //    var apiResult = FetchPlatformDailyReport(new ExternalVendorGetReportRequestParameter
        //    {
        //        StartTimeUTC = model.StartTimeUTC,
        //        EndTimeUTC = model.EndTimeUTC,
        //        VendorConfigID = model.VendorConfigID
        //    });

        //    foreach (var item in apiResult.result)
        //    {
        //        viewModels.Add(new GameVendorAccountingSummaryReportModel
        //        {
        //            GameCode = firstFishGame.ThirdPartyGameCode,
        //            StatDate = item.dateParse,
        //            TotalBetAmount = item.total_bet,
        //            TotalBetCount = example.invalid,
        //            TotalPayoutAmount = item.total_win,
        //            TotalWinLoss = item.win_lose
        //        });
        //    }




        //    //迴圈查每一款遊戲
        //    foreach (var eachGame in queryGames)
        //    {
        //        req.AdvancedSetting.Log_DebugInfo2 = merchantID;

        //        var data = new
        //        {
        //            merchantid = merchantID,
        //            @operator = operatorID,
        //            vcode = eachGame.ThirdPartySecondCode,
        //            gameid = eachGame.ThirdPartyGameCode,
        //            testaccount = "",
        //            starttime = model.StartTimeUTC.AddHours(myTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
        //            endtime = model.EndTimeUTC.AddHours(myTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
        //            timezone = ((int)myTimeZone)  //如果用double會出現加密錯誤...?
        //        };

        //        //此遊戲商時區為0
        //        var body = new
        //        {
        //            data = data,
        //            hash = GenerateSignHash(data.ConvertToJsonString(), hashKey).AfterHash,
        //        };

        //        var res = APIRequestHelper.RequestApiResult<AGGAccountingReportResponse>(req, body);

        //        if (res.ResultObject == null || res.ResultObject.result.IsEmptyOrNull())
        //            continue;

        //        foreach (var item in res.ResultObject.result)
        //        {
        //            viewModels.Add(new GameVendorAccountingSummaryReportModel
        //            {
        //                GameCode = item.game_id,
        //                StatDate = DateTime.Parse(item.date),
        //                TotalBetAmount = item.total_bet,
        //                TotalBetCount = item.total_count,
        //                TotalPayoutAmount = item.total_win,
        //                TotalWinLoss = item.win_lose
        //            });
        //        }
        //    }

        //    return new ExternalVendorGetReportResultModel
        //    {
        //        AssignVendorID = merchantID,
        //        //ResponseText = res.ResponseText,
        //        EachGameSummary = viewModels
        //    };
        //}

        public AGGFishWalletBalanceResponse FetchPlayerFishWalletBalanace(int playerID)
        {
            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var req = new RequestParameters
            {
                ApiUrl = "/api/operator/UserBalance",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryPlayer.MerchantID, "FetchFishWalletBalance", playerID.ToString());

            string vendorMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string operatorID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string hashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            req.AdvancedSetting.Log_DebugInfo2 = vendorMerchantID;

            var data = new
            {
                merchantid = vendorMerchantID,
                @operator = operatorID,
                username = playerID.ToString()
            };

            //此遊戲商時區為0
            var body = new
            {
                data = data,
                hash = GenerateSignHash(data.ConvertToJsonString(), hashKey).AfterHash,
            };

            var res = APIRequestHelper.RequestApiResult<AGGFishWalletBalanceResponse>(req, body);

            if (res.ResultObject == null || res.ResultObject.result == null)
            {

            }

            return res.ResultObject;
        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameVendorTypes = GameVendor.ToInteger().ToListCollection()
            });

            var req = new RequestParameters
            {
                //ApiUrl = "/api/operator/DailyReport", //舊版使用這隻API

                ApiUrl = "/api/operator/RoundDailyReport",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "RoundDailyReport", model.DebugRequestUUID);

            string merchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string operatorID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string hashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            double myTimeZone = 8;

            //迴圈查每一款遊戲
            foreach (var eachGame in queryGames)
            {
                req.AdvancedSetting.Log_DebugInfo2 = merchantID;

                var data = new
                {
                    merchantid = merchantID,
                    @operator = operatorID,
                    vcode = eachGame.ThirdPartySecondCode,
                    gameid = eachGame.ThirdPartyGameCode,
                    //testaccount = "",
                    starttime = model.StartTimeUTC.AddHours(myTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                    endtime = model.EndTimeUTC.AddHours(myTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                    timezone = ((int)myTimeZone)  //如果用double會出現加密錯誤...?
                };

                //此遊戲商時區為0
                var body = new
                {
                    data = data,
                    hash = GenerateSignHash(data.ConvertToJsonString(), hashKey).AfterHash,
                };

                var res = APIRequestHelper.RequestApiResult<AGGFishRoundDailyReportResult>(req, body);

                if (res.ResultObject == null || res.ResultObject.result.IsEmptyOrNull())
                    continue;

                foreach (var item in res.ResultObject.result)
                {
                    if (item == null)
                        continue;

                    decimal totalWin = item.total_win;

                    if(item.total_win_v1 > 0)
                    {
                        totalWin = item.total_win_v1;
                    }

                    viewModels.Add(new GameVendorAccountingSummaryReportModel
                    {
                        ThirdPartyGameCode = eachGame.ThirdPartyGameCode,
                        StatDate = DateTime.Parse(item.date),
                        TotalBetAmount = item.total_bet,
                        TotalBetCount = item.total_round,
                        TotalPayoutAmount = totalWin,
                        TotalWinLoss = AppHelper.CalculateWinLoss(item.total_bet , totalWin)
                    });
                }
            }

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = merchantID,
                EachGameSummary = viewModels,
            };
        }


        public AGGFishPlatformDailyReportResult FetchPlatformDailyReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var req = new RequestParameters
            {
                ApiUrl = "/api/operator/PlatformDailyReport",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "PlatformDailyReport", model.DebugRequestUUID);

            string merchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string operatorID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string hashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            double myTimeZone = 8;

            req.AdvancedSetting.Log_DebugInfo2 = merchantID;

            var data = new
            {
                merchantid = merchantID,
                @operator = operatorID,
                starttime = model.StartTimeUTC.AddHours(myTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                endtime = model.EndTimeUTC.AddHours(myTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                timezone = ((int)myTimeZone)  //如果用double會出現加密錯誤...?
            };

            var hashResult = GenerateSignHash(data.ConvertToJsonString(), hashKey);

            //此遊戲商時區為0
            var body = new
            {
                data = data,
                hash = hashResult.AfterHash,
            };

            var res = APIRequestHelper.RequestApiResult<AGGFishPlatformDailyReportResult>(req, body);

            if (res.ResultObject == null || !API_SUCCESS_CODES.Contains(res.ResultObject.code))
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);
            }

            var myResult = res.ResultObject;

            if (myResult.result.IsNotEmptyNull())
            {
                foreach (var item in myResult.result)
                {
                    item.dateParse = DateTimeOffset.Parse(item.date).DateTime;
                }
            }

            return myResult;
        }


        public AGGFishRoundDailyReportResult FetchRoundBaseDailyReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var req = new RequestParameters
            {
                ApiUrl = "/api/operator/RoundDailyReport",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "RoundDailyReport", model.DebugRequestUUID);

            string merchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string operatorID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string hashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            double myTimeZone = 8;

            req.AdvancedSetting.Log_DebugInfo2 = merchantID;

            var data = new
            {
                merchantid = merchantID,
                @operator = operatorID,
                vcode = "fishking",
                gameid = "FishingKing",
                starttime = model.StartTimeUTC.AddHours(myTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                endtime = model.EndTimeUTC.AddHours(myTimeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                timezone = ((int)myTimeZone)  //如果用double會出現加密錯誤...?
            };

            var hashResult = GenerateSignHash(data.ConvertToJsonString(), hashKey);

            //此遊戲商時區為0
            var body = new
            {
                data = data,
                hash = hashResult.AfterHash,
            };

            var res = APIRequestHelper.RequestApiResult<AGGFishRoundDailyReportResult>(req, body);

            if (res.ResultObject == null || !API_SUCCESS_CODES.Contains(res.ResultObject.code))
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);
            }

            var myResult = res.ResultObject;

            if (myResult.result.IsNotEmptyNull())
            {
                foreach (var item in myResult.result)
                {
                    item.DateParse = DateTimeOffset.Parse(item.date).DateTime.Date;
                }
            }

            return myResult;
        }


        public List<AGGFishBetRecordDetailApiResponse> FetchBetRecordsByRoundID(AGGFishBetRecordApiRequestParameter model)
        {
            List<AGGFishBetRecordDetailApiResponse> fetchRecords = new List<AGGFishBetRecordDetailApiResponse>();

            double minutesRange = 60;

            //因為AGG的起始時間似乎會有1-2分鐘的誤差問題。往前撈5分鐘 資料才會抓到
            DateTime startTime = model.StartTimeUTC.AddMinutes(-5);

            //三方API每次查詢range限制為60分鐘
            List<DateTime> timeList = GetSplitDatetimeList(startTime, model.EndTimeUTC, minutesRange);

            foreach (DateTime time in timeList)
            {
                var clone = model.ReturnInjecteObject<AGGFishBetRecordApiRequestParameter>();
                clone.StartTimeUTC = time;
                clone.EndTimeUTC = time.AddMinutes(minutesRange);

                var apiResult = FetchBetRecords(clone);

                if (apiResult.result.IsNotEmptyNull())
                {
                    fetchRecords.AddRange(apiResult.result);
                }
            }

            //對方API有bug 會混其他單號的資料?
            fetchRecords = fetchRecords.Where(x => x.RoundID == model.RoundID).ToList();
            return fetchRecords;
        }


        public List<AGGFishDetailRecordEntity> ConvertToFishDetailRecordEntity(List<AGGFishBetRecordDetailApiResponse> fetchRecords, AGGFishBetRecordPgsRoundInfo recordInfo)
        {
            if (fetchRecords.IsEmptyOrNull())
                return new List<AGGFishDetailRecordEntity>();

            var now = DateTimeHelper.GetCurrentTime();

            var insertData = fetchRecords.Select(x => new AGGFishDetailRecordEntity
            {
                BetAmount = x.TotalBet,
                PayoutAmount = x.TotalWin,
                WinLossAmount = x.WinLose,
                FishType = x.FishType,
                FishTypeText = x.FishTypeDesc,
                GameID = recordInfo.GameID,
                PlayerID = recordInfo.PlayerID,
                MerchantID = recordInfo.MerchantID,
                PlayerName = recordInfo.PlayerName,
                PgsRoundID = recordInfo.BetRecordNoPGS,
                RoundID = x.RoundID,
                WagerID = x.BetNumber,
                BetTime = DateTime.Parse(x.BetTime.value),
                RoundSettleTime = recordInfo.SettleTime,
                CreateTime = now,
            })
               .ToList();

            return insertData;
        }


        public void FetchBetRecordsAndInsertToDB(AGGFishBetRecordApiRequestParameter model, AGGFishBetRecordPgsRoundInfo recordInfo)
        {
            recordInfo.RuleFor(x => x.SettleTime).Rule_NotEmptyDateTime();
            recordInfo.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            recordInfo.RuleFor(x => x.PlayerName).Rule_NotEmptyText();
            recordInfo.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            recordInfo.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            List<AGGFishBetRecordDetailApiResponse> fetchRecords = FetchBetRecordsByRoundID(model);

            if (fetchRecords.IsEmptyOrNull())
                return;

            try
            {
                var insertData = ConvertToFishDetailRecordEntity(fetchRecords, recordInfo);

                //數據正確性比隊
                if (recordInfo.DataCorrectValidInfo != null)
                {
                    decimal totalBet = insertData.Select(x => x.BetAmount).Sum();
                    decimal totalWinLoss = insertData.Select(x => x.WinLossAmount).Sum();

                    if (totalBet != recordInfo.DataCorrectValidInfo.TotalBetAmount ||
                       totalWinLoss != recordInfo.DataCorrectValidInfo.TotalWinLoss)
                    {
                        string msg = $"AGG捕魚 {recordInfo.BetRecordNoPGS} ,fetch的明細注單金額匹配失敗";
                        SendNotifyToChannel(msg);

                        //標記標籤
                        BetRecordTagMappingRepository.CreateOrQueueRetry(recordInfo.BetRecordNoPGS, SystemTagEnum.BetRecord_MustCheckFishDetailInfo);
                    }
                }

                AGGFishDetailRecordRepository.BulkInsert(insertData);
            }
            catch (Exception ex)
            {
                string prefix = $"FetchBetRecordsAndInsertToDB , 單號:{recordInfo.BetRecordNoPGS} ";
                string msg = ex.GetCommonNotifyMessage(prefix);
                SendNotifyToChannel(msg);
            }
        }


        public AGGFishBetRecordApiReportResponse FetchBetRecords(AGGFishBetRecordApiRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            if ((model.EndTimeUTC - model.StartTimeUTC).TotalMinutes > 60)
                throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge);

            var req = new RequestParameters
            {
                ApiUrl = "/api/operator/BetRecords",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 90 * 1000
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "BetRecords", "");

            string merchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string operatorID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string hashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            var param = new
            {
                merchantid = merchantID,
                @operator = operatorID,
                vcode = "fishking",
                gameid = "FishingKing",
                betnumber = "",
                roundid = model.RoundID,
                roundstring = "",
                username = "",
                starttime = model.StartTimeUTC.ToString("yyyy-MM-dd HH:mm:ss"),  //時間格式為"2022-04-11 00:00:00"
                endtime = model.EndTimeUTC.ToString("yyyy-MM-dd HH:mm:ss"),
                timezone = 0,
                pagesize = 10000,
                pageno = 1,
            };

            var data = new
            {
                data = param,
                hash = GenerateSignHash(param.ConvertToJsonString(), hashKey).AfterHash,
            };

            var res = APIRequestHelper.RequestApiResult<AGGFishBetRecordApiReportResponse>(req, data);

            if (res.ResultObject == null || res.ResultObject.result.IsEmptyOrNull())
            {

            }

            return res.ResultObject;
        }

        public AGGFishMoneyRecordResponse FetchMoneyRecord(AGGFishFetchMoneyRecordRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var req = new RequestParameters
            {
                ApiUrl = "/api/operator/MoneyLog",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 90 * 1000
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "FetchMoneyRecord", "");

            string merchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string operatorID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string hashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            var param = new
            {
                merchantid = merchantID,
                @operator = operatorID,
                starttime = model.StartTimeUTC.ToString("yyyy-MM-dd HH:mm:ss"),  //時間格式為"2022-04-11 00:00:00"
                endtime = model.EndTimeUTC.ToString("yyyy-MM-dd HH:mm:ss"),
                timezone = 0,
                pagesize = model.PageSize,
                pageno = model.PageIndex,
                username = model.PlayerID.ToString(),
                type = model.Type.NotNullNotZeroNotMinus() ? model.Type.Value.ToString() : ""
            };

            var data = new
            {
                data = param,
                hash = GenerateSignHash(param.ConvertToJsonString(), hashKey).AfterHash,
            };

            var res = APIRequestHelper.RequestApiResult<AGGFishMoneyRecordResponse>(req, data);

            if (res.ResultObject == null || res.ResultObject.result.IsEmptyOrNull())
            {

            }

            return res.ResultObject;
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


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res) where T : IAGGLiveResponse
        {
            var extraInfo = new ErrorDetailModel(GameVendor.ToString());

            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
            {
                extraInfo.ColumnName += $"statusCode: {res.ResponseStatusCode}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail, extraInfo);

            if (res.ResultObject.code != 0)
            {
                extraInfo.LogExtraInfo = res.ResponseText;

                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }
        }


        private void SetRequestInfoByDBSetting(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

            SetCommonRequestInfo(req, apiCategory, requestUUID, debugInfo2);
        }


        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            SetCommonRequestInfo(req, apiCategory, requestUUID, debugInfo2);
        }


        private void SetCommonRequestInfo(RequestParameters req, string apiCategory, string requestUUID, string debugInfo2)
        {
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.ToString(),
                Log_DebugInfo2 = debugInfo2,
                Log_RequestUUID = requestUUID
            };

            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
            req.BaseUrl = apiDomain;
        }

        private List<DateTime> GetSplitDatetimeList(DateTime begin, DateTime end, double splitMinutes)
        {
            List<DateTime> dateList = new List<DateTime>();

            int looptimes = -1;

            while (true)
            {
                looptimes++;

                DateTime myTime = begin.AddMinutes(splitMinutes * looptimes);

                if (myTime >= end)
                    break;

                dateList.Add(myTime);
            }

            return dateList;
        }



    }
}
