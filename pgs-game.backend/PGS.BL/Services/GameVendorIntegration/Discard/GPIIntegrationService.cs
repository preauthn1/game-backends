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

    public class GPIIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.GPI;
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


        public BetRecordGPIRepository BetRecordRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public PlaceBetRaiseRecordRepository PlaceBetRaiseRecordRepository { get; set; }

        public GPICasinoRoundRecordMappingRepository GPICasinoRoundRecordMappingRepository { get; set; }


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            int retunrCode = -1; //其他錯誤 (請帶入自訂錯誤訊息)

            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            var model = param.ApiResponseError;
            bool isCancelApi = apiUrl.Contains("cancelBet") || apiUrl.Contains("cancelSessionBet");

            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                retunrCode = -1; //系統維護中
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                retunrCode = -119; //data 不能為空值、參數錯誤
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                retunrCode = -1;
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                retunrCode = 1; //注單編號已結算
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                retunrCode = 1; //注單編號已結算
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                retunrCode = -201; //注單編號不存在
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                retunrCode = -4; //餘額不足
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                retunrCode = -48; //token 驗證失敗
            }

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = 200,
                PGSDebug = debugInfo,
                GameVendorCustomizedResponse = new GPIAPIExceptionCommonResponse
                {
                    error_code = retunrCode,
                    error_msg = model.Message,
                }
            };
        }


        public object FormatParsing_ConvertToPlaceBetResponseResult(SingleWalletPlaceBetDebitAmountResponseParameter placeBetInfo)
        {
            return new
            {
                error_code = 0,
                error_msg = "Success",
                before = placeBetInfo.BeforeGamePoint,
                after = placeBetInfo.AfterGamePoint,
                exchange_rate = 1,
                trx_id = placeBetInfo.BetRecordNoPGS,
            };
        }


        public object FormatParsing_ConvertToSettleBetResponseResult(SettlementConvertRequiredInfoModel model)
        {
            return new
            {
                error_code = 0,
                error_msg = "Success",
                before = model.BeforeGamePoint,
                after = model.AfterGamePoint,
                exchange_rate = 1,
                trx_id = model.BetRecordNoPGS + "_settle",
            };
        }


        public object FormatParsing_ConvertToCancelBetResponseResult(CancelBetConvertRequiredInfoModel model)
        {
            return new
            {
                error_code = 0,
                error_msg = "Success",
                before = model.BeforeGamePoint,
                after = model.AfterGamePoint,
                exchange_rate = 1,
                trx_id = model.BetRecordNoPGS + "_cancel",
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


        public bool IsMultipleRaiseBet(string betRecordNo, string thirdGameCode, int gameID)
        {
            //這家遊戲商。casino類的gameID都是數字。因此只針對gameCode是數字的處理
            if (!thirdGameCode.IsNumeric())
                return false;

            var raiseSupport = GameListCustomizedSettingRepository.GetParseModelByCache(gameID, GameListCustomizedSettingKeyEnum.IsSupportRaiseProcessWhenPlaceBet);

            var isSupportRaise = raiseSupport.SettingValueOfBoolean;

            if (!isSupportRaise)
                return false;

            var myFilter = new BetRecordInfoQueryParameter
            {
                BetRecordNo = betRecordNo,
                GameID = gameID,
                IsQueryRecentRange = true
            };

            // step 1-1. 檢查注單存在 (檢查狀態是在 UpdateRawBetRecordAfterSettlement 處理)
            var queryExistBetRecord = GetByBetRecordNo(myFilter);

            if (queryExistBetRecord == null)
                return false;

            var raiseRecords = PlaceBetRaiseRecordRepository.GetRaiseRecordsByPgsNo(queryExistBetRecord.BetRecordNoPGS, isLockQuery: false);

            if (raiseRecords.IsEmptyOrNull())
                return false;

            if (raiseRecords.Count() <= 1)
                return false;

            //true即代表 有多筆加注紀錄。
            return true;
        }



        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = " * ";

            BetRecordGPIEntity queryRecord = null;

            //特殊情況會使用此方式查詢
            if(model.BetRecordNoPGS.IsNotEmptyTextNotNull())
            {
                var query = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);
                return query.ReturnInjecteObject<BetRecordBriefInfoModel>();
            }

            List<QueryBetRecordGeneralRequestViewModel> loopTimeQuery = InstanceLoopTimesQueryTimeRange(model);

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

            var insert = InstanceInsertBetRecordEntity<BetRecordGPIEntity>(model);
            insert.PartitionTime = model.PartitionTime;
            insert.RoundID = rawInfo.GPI.PlaceBetInfo?.round_id;
            insert.ThirdPartyGameCode = model.ThirdGameCode;

            BetRecordRepository.Create(insert);

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            //如果是casino類型的遊戲 需寫入額外資料
            if (IsGPICasinoGames(queryGame))
            {
                GPICasinoRoundRecordMappingRepository.Create(new GPICasinoRoundRecordMappingEntity
                {
                    RoundID = insert.RoundID,
                    BetRecordNoPGS = insert.BetRecordNoPGS
                });
            }
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.GPI).Rule_ModelNotNull();

            var settleInfo = rawInfo.GPI.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = model.PayoutGamePoint;
            decimal jackpotPoint = 0;
            decimal taxAmount = 0;

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
            parseModel.TaxAmount = taxAmount / model.ConvertRateWhenPlaceBet;

            bool isUseBetAmountAsValidAmount = false;

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            if (queryGame != null)
            {
                isUseBetAmountAsValidAmount = queryGame.GameType == GameTypeEnum.Slot.ToInteger();
            }

            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                RoundID = "",
                NewBetAmount = model.NewBetAmount,
                UseBetAmountAsValidAmount = isUseBetAmountAsValidAmount, //slots遊戲已投注額來算、反之則用輸贏流水
            };

            //if (settleInfo.turnover.HasValue)
            //{
            //    parseModel.ValidAmount = settleInfo.turnover.Value;
            //}
            //else
            //{
            //    parseModel.ExtraColumn.UseBetAmountAsValidAmount = true;
            //}

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
                queryExist.JackpotAmount = model.JackpotAmount;
                queryExist.TaxAmount = model.TaxAmount;
                queryExist.ThirdSettleTime = model.ExtraColumn?.ThirdPartySettleTime;
                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;

                if (model.ExtraColumn.NewBetAmount.HasValue)
                {
                    queryExist.BetAmount = model.ExtraColumn.NewBetAmount.Value;
                }

                if (queryExist.ThirdSettleTime.HasValue)
                {
                    queryExist.ThirdSettleDate = queryExist.ThirdSettleTime.Value.AddHours(8).Date;
                }

                // 1. 結算回調的時候對方沒有提供有效流水 自己算
                // 2. 後台手動結算的時候 可以自己輸入有效流水
                if (model.ExtraColumn.UseBetAmountAsValidAmount)
                {
                    queryExist.ValidAmount = queryExist.BetAmount;
                }
                else
                {
                    if (model.IsAdminSysManualOperate)
                    {
                        queryExist.ValidAmount = model.ValidAmount;
                    }
                    else
                    {
                        queryExist.ValidAmount = queryExist.WinLossAmount.ToAbs();
                    }
                }

                queryExist.SettlementTime = now;
                queryExist.ManualSettleUserID = model.UserID;

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

            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            return new ExternalVendorCancelBetResultModel
            {

            };
        }

        public (int GameID, string ThirdGameCode, bool isGPICasinoGame) GetGameCodeByGameCodeLogic(string gpiGameCode)
        {
            var matchGame = GameListRepository.GetGameCacheByGPIGameCode(gpiGameCode);
        
            if (matchGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            bool isGPICasinoGame = IsGPICasinoGames(matchGame);
            return (matchGame.GameID, matchGame.ThirdPartyGameCode, isGPICasinoGame);
        }


        public List<string> GetPgsTransactionNoByGPIRoundID(string roundID)
        {
            var query = GPICasinoRoundRecordMappingRepository.GetByFilter(new QueryGPIRoundRecordMappingRequestViewModel
            {
                RoundID = roundID
            });

            return query.Select(x => x.BetRecordNoPGS).ToList() ?? new List<string>();
        }


        private bool IsGPICasinoGames(GameListEntity game)
        {
            bool isPokerGame = (game.GameType == (int)GameTypeEnum.BoardGame || game.GameType == (int)GameTypeEnum.Live);
            bool isGPICasinoGame = game.ThirdPartyGameCode.IsNumeric() && isPokerGame;
            return isGPICasinoGame;
        }


        public ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model)
        {
            var result = new ExternalVendorSessionTokenCheckResultModel
            {

            };

            VendorSessionTokenEntity queryVendorSessionToken = [REDACTED];

            if (string.IsNullOrWhiteSpace(model.SessionOrToken) && model.PlayerID > 0)
            {
                if (model.PlayerID <= 0)
                {
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);
                }

                queryVendorSessionToken = [REDACTED];
            }
            else
            {
                model.RuleFor(x => x.SessionOrToken).Rule_NotEmptyText();

                queryVendorSessionToken = [REDACTED];
            }

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

            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string jsonSettingValue = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_ExtraJsonSetting)?.SettingValue;

            if (string.IsNullOrWhiteSpace(jsonSettingValue))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("GameVendor_ExtraJsonSetting"));
            }

            var jsonParse = jsonSettingValue.TryJsonConvertToModel<GPIVendorConfigJsonSettingModel>();

            if (jsonParse == null)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel("GPI VendorConfigJsonSetting"));
            }

            var entranceGameType = GameListCustomizedSettingRepository.GetGameSettingParseModel(model.GameID, GameListCustomizedSettingKeyEnum.GPI_GameLaunch_EntranceType);

            GameTypeEnum targetEntranceType = GameTypeEnum.Slot;

            if (string.IsNullOrWhiteSpace(entranceGameType.SettingValue))
            {
                targetEntranceType = (GameTypeEnum)queryGame.GameType;
            }
            else
            {
                targetEntranceType = (GameTypeEnum)entranceGameType.SettingValueOfInt;
            }

            string gameCode = model.ThirdPartyGameCode;
            string token = [REDACTED];

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            string trialParam = model.IsTrialMode ? "1" : "0";

            var isH5 = model.DeviceType == (int)HDTypeEnum.Unknow || model.DeviceType == (int)HDTypeEnum.Mobile;

            string baseUrl = string.Empty;
            string queryString = string.Empty;
            string finaUrl = string.Empty;

            //根據不同遊戲入口有不同參數
            switch (targetEntranceType)
            {
                case GameTypeEnum.Slot:
                    baseUrl = (jsonParse.SlotsWeb + "/").ReplaceDuplicateUrlChar();
                    queryString = $@"?token=[REDACTED]";
                    finaUrl = baseUrl + $"{gameCode}" + queryString;
                    break;

                case GameTypeEnum.Lottery:
                    baseUrl = (jsonParse.LotteryWeb + "/").ReplaceDuplicateUrlChar();
                    queryString = $@"?lang={lang}&vendor={assignAgentID}&ticket={token}&game={gameCode}";
                    finaUrl = baseUrl + queryString;
                    break;

                case GameTypeEnum.Arena:
                    baseUrl = (jsonParse.P2PWeb + "/").ReplaceDuplicateUrlChar();
                    queryString = $@"?op={assignAgentID}&lang={lang}&fun={trialParam}&token=[REDACTED]";
                    finaUrl = baseUrl + $"{gameCode}/" + queryString;
                    break;

                case GameTypeEnum.BoardGame:
                case GameTypeEnum.Live:
                    if (isH5)
                    {
                        baseUrl = (jsonParse.CasinoH5 + "/").ReplaceDuplicateUrlChar();
                        queryString = $@"?token=[REDACTED]";
                    }
                    else
                    {
                        baseUrl = (jsonParse.CasinoWeb).ReplaceDuplicateUrlChar();
                        queryString = $@"?token=[REDACTED]";
                    }
                    finaUrl = baseUrl + queryString;
                    break;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel("targetEntranceType"));
            }

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = finaUrl
            };
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);


            //參考文件.pdf  5.1  启动游戏 API
            var req = new RequestParameters
            {
                ApiUrl = "/GetGameDetailUrl",
                Method = HttpMethodType.HttpPost,
                ContentType = "application/x-www-form-urlencoded"
            };

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryBetRecord.MerchantID, "GetExternalBetDetailInfo", queryBetRecord.BetRecordNoPGS);

            var httpParameters = new
            {
                WagersId = queryBetRecord.BetRecordNo,
                Lang = "zh-CN",
            };

            List<string> excludeColumns = new List<string> { "Lang" };

            throw new TerproException(ApiErrorCode.NotImplement);


            //var signKeyResult = GenerateSignKey(httpParameters, assignAgentID, encryptKey, excludeColumns);

            ////對應model JiliRecordDetailExternalUrlResultModel.cs
            //var res = APIRequestHelper.RequestApiResult<JiliRecordDetailExternalUrlResultModel>(req, signKeyResult.QueryStringWithKey);

            //return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            //{
            //    InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
            //    Url = res.ResultObject?.Data?.Url
            //};
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


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var queryLiveGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameIDs = model.Debug_GameIDs,
                GameVendorTypes = GameVendor.ToInteger().ToListCollection()
            });


            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            //迴圈查每一款遊戲
            foreach (var eachGame in queryLiveGames)
            {
                //var req = new RequestParameters
                //{
                //    ApiUrl = "/GetBetRecordSummary",
                //    Method = HttpMethodType.HttpGet,
                //    TimeoutMillisecond = 1000 * 90
                //};

                ////已確認 要轉成-4時區來查沒錯。
                //var httpParameters = new
                //{
                //    StartTime = model.StartTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                //    EndTime = model.EndTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                //    GameId = eachGame.ThirdPartyGameCode,
                //};

                //List<string> excludeColumns = new List<string> { "GameId" };

                //var signKeyResult = GenerateSignKey(httpParameters, assignAgentID, encryptKey, excludeColumns);

                //string finalQueryString = $"{signKeyResult.QueryStringText}&Key={signKeyResult.SignKey}";

                ////apiToken、api域名 吃資料庫設置
                //SetCommonRequestInfo(req, "JiliDailyReport", assignAgentID, model.VendorConfigID.ToString());
                //req.ApiUrl += "?" + finalQueryString;

                //var apiResult = APIRequestHelper.RequestApiResult<JiliReportSummaryhApiModel>(req);

                //if (apiResult.ResultObject == null || apiResult.ResultObject.ErrorCode == 101)
                //    continue;

                //CheckResponseIsError(apiResult);

                ////可能包含多幣別。取投注量最大的 (其他幣別應該是測試資料)
                //var report = apiResult.ResultObject.Data.OrderByDescending(x => x.BetAmount).FirstOrDefault();

                //viewModels.Add(new GameVendorAccountingSummaryReportModel
                //{
                //    GameCode = eachGame.ThirdPartyGameCode,
                //    StatDate = model.StartTimeUTC.AddHours(8),
                //    TotalBetAmount = report.BetAmount.ToAbs(),
                //    TotalBetCount = report.WagersCount,
                //    TotalPayoutAmount = report.PayoffAmount,
                //    TotalWinLoss = (report.BetAmount - report.PayoffAmount),
                //    Memo = $"{eachGame.GameID}_{eachGame.GameName}",
                //});
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


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res) where T : JiliAPICommonResponse
        {
            var gameVendorText = GameVendor.ToString().ToLower();

            var extraInfo = new ErrorDetailModel
            {
                ColumnName = gameVendorText,
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

            if (res.ResultObject.ErrorCode != 0)
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
            req.UserAgent = "target";
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


    }
}
