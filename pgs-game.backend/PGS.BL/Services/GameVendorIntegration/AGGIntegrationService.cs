using DocumentFormat.OpenXml.EMMA;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.IOC;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.BL.Services
{

    public partial class AGGIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService, IAGGInternalAPIService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.AGG;
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


        public BetRecordAGGRepository BetRecordRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            //RS_OK - Successful response.
            //RS_ERROR_UNKNOWN - General error status, for cases without a special error code.
            //RS_ERROR_INVALID_USER_TOKEN - UserToken invalid(or expired) to Integrator's system.
            //RS_ERROR_INVALID_ACCOUNT_TOKEN - AccountToken invalid(or expired) to Integrator's system.
            //RS_ERROR_BANNED_ACCOUNT - User is disabled / locked / banned and can't place bets.
            //RS_ERROR_NOT_ENOUGH_BALANCE - Not enough money(or item) on User's balance.
            //RS_ERROR_WRONG_CURRENCY - User specified currency is invalid or differs from User's wallet currencies.
            //RS_ERROR_DUPLICATE_WAGER - A wager with same id was sent, but there was a different user or game.
            //RS_ERROR_WAGER_DOES_NOT_EXIST - Returned when the wager can't be found on Integrator's side.
            //RS_ERROR_ACCOUNT_DOES_NOT_EXIST - Returned when the account can't be found on Integrator's side.
            //RS_ERROR_INVALID_WAGER_OPERATION - Invalid Operation for this wager's state.
            //RS_ERROR_FORBIDDEN_ITEM - 禁止使用的道具(未達使用條件、未領取、過期、etc..)

            var model = param.ApiResponseError;
            string AGGResponseCode = "RS_ERROR_UNKNOWN";

            // AGG只處理200、500。如果回500 他會瘋狂retry (以前邏輯是這樣嗎...?)
            // 直接把所有改成500有風險...
            int myStatusCode = 400;

            // (特殊案例) 拋錯誤 但是強制回應成功給AGG
            if (model.Code == (int)ApiErrorCode.InvalidStatus_BetRecordAlreadySettlement)
            {
                string currency = TryGetHeaderTempCurrency();
                decimal myAmount = TryGetHeaderTempBalance();

                return new SingleWalletExceptionHandleModel
                {
                    StatusCode = 200,
                    GameVendorCustomizedResponse = new AGGSingleWalletAPIPlaceBetResponseViewModel
                    {
                        StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse,
                        Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                        {
                            ConvertToAGGBalanceModel(myAmount , currency)
                        }
                    }
                };
            }

            // (特殊案例) 拋錯誤 但是強制回應成功給AGG
            else if (model.Code == (int)ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel)
            {
                return new SingleWalletExceptionHandleModel
                {
                    StatusCode = 200,
                    GameVendorCustomizedResponse = new AGGSingleWalletAPIPlaceBetResponseViewModel
                    {
                        StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse,
                        Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                        {
                            ConvertToAGGBalanceModel(0 , "CNY"),
                        }
                    }
                };
            }

            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                //遊戲未定義
                AGGResponseCode = "RS_ERROR_FORBIDDEN_ITEM";
            }
            else if (model.Code == (int)ApiErrorCode.Forbidden_AGG_NotAllowBuyFeature)
            {
                //投注單號已存在
                AGGResponseCode = "RS_ERROR_FORBIDDEN_ITEM";
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                //投注單號已存在
                AGGResponseCode = "RS_ERROR_DUPLICATE_WAGER";
            }
            else if (IsInvalidStatusError((ApiErrorCode)model.Code))
            {
                //狀態不合法
                AGGResponseCode = "RS_ERROR_INVALID_WAGER_OPERATION";
            }
            else if (model.Code == ApiErrorCode.NotFoundBetRecordWhenCancelProcess.ToInteger())
            {
                //查無注單
                AGGResponseCode = "RS_ERROR_WAGER_DOES_NOT_EXIST";
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                //查無注單
                AGGResponseCode = "RS_ERROR_WAGER_DOES_NOT_EXIST";
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                //錢包餘額不足
                AGGResponseCode = "RS_ERROR_NOT_ENOUGH_BALANCE";
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                //身分驗證不合法
                AGGResponseCode = "RS_ERROR_INVALID_USER_TOKEN";
            }
            else if (model.Code == (int)ApiErrorCode.ThirdPartyApiFail_Timeout_SettleProcess)
            {
                return new SingleWalletExceptionHandleModel
                {
                    StatusCode = 500,
                    ForceResponseSpecificText = $"{ApiErrorCode.ThirdPartyApiFail_Timeout_SettleProcess.ToString()}",
                    GameVendorCustomizedResponse = new AGGAPIResponseModel
                    {
                        StatusCode = "ThirdPartyApiFail_Timeout_SettleProcess"
                    }
                };
            }

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            string extraMsg = null;

            // 2023/10/11 
            // 改動最小的方案 先針對這種情境回500 其他情境維持400 statusCode
            if (AGGResponseCode == "RS_ERROR_UNKNOWN")
            {
                myStatusCode = 500;
            }

            extraMsg = $"{model.Message}｜{DateTimeHelper.GetCurrentTime(8).ToString("yyyy-MM-dd HH:mm:ss")}";

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = myStatusCode,
                PGSDebug = debugInfo,
                GameVendorCustomizedResponse = new AGGAPIResponseModel
                {
                    Message = extraMsg,
                    StatusCode = AGGResponseCode
                }
            };
        }


        private string TryGetHeaderTempCurrency()
        {
            string defaultCurrency = "CNY";

            try
            {
                var currency = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_Currency);

                if (string.IsNullOrWhiteSpace(currency))
                    return defaultCurrency;

                return currency;
            }
            catch
            {

            }

            return defaultCurrency;
        }

        private decimal TryGetHeaderTempBalance()
        {
            decimal defaultBalance = 0;

            try
            {
                var balanceAmount = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_AfterAmount);

                if (string.IsNullOrWhiteSpace(balanceAmount))
                    return defaultBalance;

                return decimal.Parse(balanceAmount);
            }
            catch
            {

            }

            return defaultBalance;
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
            perGame.JackpotAmount = queryBetRecord.JackpotAmount;

            return new ConvertPerGameLogResultModel
            {
                PerGameDetailLog = perGame
            };
        }


        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //如果是AGG的注單 優先使用Cache 減少查詢DB的頻率 (placeBet會建資料)
            var queryCache = BetRecordExistTempInfo_GetValue(model.BetRecordNo);

            if (queryCache != null)
                return queryCache;

            //減少查詢的欄位數 減少流量消耗 
            //ps. 如果要移除部分欄位。務必要小心確認其他程式碼有沒有用到 (可能導致嚴重的錯誤)
            string selectColumns = SelectColumns_BriefQuery + " , AmountBeforeBet , ValidAmount ";

            BetRecordAGGEntity queryRecord = null;

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


        public BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
        {
            var queryRecord = GetByBetRecordNoPGS(betRecordNoPGS);
            return queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();
        }


        public void WriteRawBetRecordWhenPlaceBet(InsertRawBetRecordRequiredInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();

            var insert = InstanceInsertBetRecordEntity<BetRecordAGGEntity>(model);
            insert.PartitionTime = model.PartitionTime;

            //TODO 這些欄位可能會直接刪掉、或再做調整
            insert.PlayerName = insert.PlayerName.Truncate(SystemConstant.PlayerAccountMaxLength);

            //活動freespin的情況才有值
            if (model.ValidAmount.HasValue)
            {
                insert.ValidAmount = model.ValidAmount.Value;
            }

            BetRecordRepository.Create(insert);

            // (效能優化手段) AGG特有的提升效能方式 紀錄cache 讓settlement 加快
            var cacheValue = insert.ReturnInjecteObject<BetRecordBriefInfoModel>();
            BetRecordExistTempInfo_SetCache(insert.BetRecordNo, cacheValue);
        }


        private BetRecordBriefInfoModel BetRecordExistTempInfo_GetValue(string thirdWagerID)
        {
            string cacheKey = GenerateThirdTransactionNoCacheKey(thirdWagerID);
            return RedisCache_2.GetCacheValue<BetRecordBriefInfoModel>(cacheKey);
        }

        private void BetRecordExistTempInfo_SetCache(string thirdWagerID, object cacheValue)
        {
            string cacheKey = GenerateThirdTransactionNoCacheKey(thirdWagerID);
            RedisCache_2.SetCacheValue(cacheKey, cacheValue, new TimeSpan(0, 15, 0));
        }

        private void BetRecordExistTempInfo_Remove(string thirdWagerID)
        {
            string cacheKey = GenerateThirdTransactionNoCacheKey(thirdWagerID);
            RedisCache_2.RemoveCacheByKey(cacheKey);
        }


        private string GenerateThirdTransactionNoCacheKey(string thirdPartyWagerID)
        {
            return CacheKey.ExternalAPI_BetRecordExistTempInfo(GameVendor, thirdPartyWagerID, "");
        }



        public string AGGAccountTokenConvertToSessionToken(string token)
        {
            //accountToken是我方自訂的格式 {gameID}__{sesssionToken} 其實只是這樣的格式
            //為何要這樣設計? ....完全忘記了
            var split = token.Split(new string[] { "__" }, StringSplitOptions.None).LastOrDefault();
            return split;
        }


        public decimal GetBetAmountFromOperationModel(AGGSingleWalletAPITransactionRequestModel model)
        {
            if (model.Amount.HasValue)
            {
                return model.Amount.Value;
            }

            var operations = model.Operations ?? model.Meta?.Operations;

            if (operations.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected);

            decimal betAmount = operations.FirstOrDefault(x => x.OperationType == "WagerWithdrawOperation")?.Withdraw ?? 0;
            return betAmount;
        }


        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.AGG).Rule_ModelNotNull();

            var settleInfo = rawInfo.AGG.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = GetPayoutAmount(rawInfo.AGG.SettleBetInfo);
            decimal jackpotPoint = GetJackpotAmount(rawInfo.AGG.SettleBetInfo);

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                ThirdPartySettleTime = GetSettleTimeByRawRequest(rawInfo.AGG.SettleBetInfo)
            };

            var result = UpdateRawBetRecord(parseModel);
            result.DebugInfoForNextProcess = GetExtraInfoByRawRequest(rawInfo.AGG.SettleBetInfo);
            return result;
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.GetCurrentTime();

            //查詢 (不包在transaction裡面) update的時候 where條件會包含status
            var queryExist = GetByBetRecordNoPGS(model.BetRecordNoPGS);

            //檢查狀態合法
            var afterStatus = BetRecordStatusEnum.SettleComplete;
            RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, afterStatus, model.PayoutAmount);

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                queryExist.UpdateTime = now;
                queryExist.Status = afterStatus.ToInteger();
                queryExist.PayoutAmount = model.PayoutAmount;
                queryExist.JackpotAmount = model.JackpotAmount;

                // 1. 結算回調的時候對方沒有提供有效流水 自己算
                // 2. 後台手動結算的時候 可以自己輸入有效流水
                queryExist.ValidAmount = model.IsAdminSysManualOperate ? model.ValidAmount : queryExist.BetAmount;
                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;
                queryExist.SettlementTime = now;

                queryExist.ThirdSettleTime = DateTimeHelper.ReplaceMillisecondsToZero(model.ExtraColumn?.ThirdPartySettleTime);

                if (queryExist.ThirdSettleTime.HasValue)
                {
                    //queryExist.ThirdSettleDate = ConvertSettleTimeToThirdSettleDate(queryExist.ThirdSettleTime.Value);
                }

                BetRecordRepository.UpdateToSettlementStatus(queryExist);
                scope.Complete();

                var recordInfo = queryExist.ReturnInjecteObject<SettlementBetRecordInfoModel>();
                return recordInfo;
            }

        }


        public ExternalVendorCancelBetResultModel CancelBet(string betRecordNoPGS, string memo)
        {
            string cacheKey = string.Empty;
            DateTime now = DateTimeHelper.GetCurrentTime();

            string thirdWagerID = null;

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                var queryExist = GetByBetRecordNoPGS(betRecordNoPGS);

                //檢查狀態合法  (AGG需要判斷特殊errorCode 做特例處理)
                RepositoryHelper.CheckStatusIsPendingForCancelProcess(queryExist);

                queryExist.UpdateTime = now;
                queryExist.Status = BetRecordStatusEnum.Cancel.ToInteger();
                thirdWagerID = queryExist.BetRecordNo;

                BetRecordRepository.Update(queryExist);
                scope.Complete();
            }

            //清除cache避免retry流程沒有等冪性
            //2024/04 註解此。這邊疑似成本很高? 不確定是否跟redis卡住有相關性
            //但是照理說 remove by key 應該還好。成本高的是like query
            //BetRecordExistTempInfo_Remove(thirdWagerID);

            return new ExternalVendorCancelBetResultModel
            {

            };
        }


        public ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model)
        {
            var defaultResult = new ExternalVendorSessionTokenCheckResultModel();

            model.RuleFor(x => x.SessionOrToken).Rule_NotEmptyText();

            var query = VendorSessionTokenRepository.GetTokenBriefByCache(model.SessionOrToken);

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

            var queryPlayer = PlayerRepository.GetSingleByCache(query.PlayerID);

            var queryHierarchy = MerchantHierarchyRepository.GetSingleByCacheOrEmptyModel(queryPlayer.MerchantID);

            defaultResult.PlayerID = query.PlayerID;
            defaultResult.MerchantID = queryPlayer.MerchantID;
            defaultResult.TokenGameID = query.GameID;
            defaultResult.HierarchyMerchantIDs = ConvertToAGGHierarchyTree(queryHierarchy , queryPlayer.MerchantID).HierarchyMerchantIDs;

            return defaultResult;
        }

        public (int ParentMerchantID, List<int> HierarchyMerchantIDs) ConvertToAGGHierarchyTree(MerchantHierarchyEntity queryHierarchy, int merchantID)
        {
            //2026/06 新增的邏輯。如果是特殊格式 強制回應成單一代理 (非樹狀代理)
            if (queryHierarchy.IsSpecialHierarchyFormat)
            {
                return (0, queryHierarchy.MerchantID.ToListCollection());
            }
            else if (queryHierarchy == null || queryHierarchy.MerchantID <= 0)
            {
                return (0, merchantID.ToListCollection());
            }
            else
            {
                return (queryHierarchy.ParentMerchantID, AppHelper.GetHierarchyMerchantIDs(queryHierarchy.Hierarchy));
            }
        }


        public decimal GetPayoutAmount(AGGSingleWalletAPITransactionRequestModel model)
        {
            if (model.Amount.HasValue && model.Amount >= 0)
            {
                return model.Amount.Value;
            }

            return model.Operations.FirstOrDefault(z => z.OperationType == "WagerDepositOperation")?.Deposit ?? 0;
        }


        public decimal GetJackpotAmount(AGGSingleWalletAPITransactionRequestModel model)
        {
            try
            {
                if (model.Operations.IsEmptyOrNull())
                {
                    model.Operations = model.Meta?.Operations;
                }

                var parse = model.Operations.FirstOrDefault(z => z.OperationType == "WagerTagOperation" && z.TagType == "Money");

                if (parse == null || parse.Tag == null)
                    return 0;

                var matchJackpot = parse.Tag.Actions?.FirstOrDefault(z => z.Action == "WinJp");

                if (matchJackpot == null)
                    return 0;

                return matchJackpot.MoneyAmount;
            }
            catch
            {

            }

            return 0;
        }



        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            //遊戲商會自動建 不用處理
        }

        private string ConvertToGameVendorLangCode(LanguageEnum lang)
        {
            return GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, lang);
        }

        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            throw new TerproException(ApiErrorCode.NotImplement);
        }

        public ExternalVendorLaunchUrlViewModel GetGameLaunchUrl(ExternalVendorLaunchUrlRequestModel model)
        {
            var result = GetGameLaunchUrlByAPI(model);
            return result;
        }

        public AGGSingleWalletEachBalanceItemViewModel ConvertToAGGBalanceModel(decimal amount, string currency)
        {
            var result = new AGGSingleWalletEachBalanceItemViewModel
            {
                Amount = Math.Round(amount, 8),
                CurrencyCode = currency,
                IsIso4217 = IsCurrencyIso4217(currency)
            };

            result.CurrencyCode = ConvertCurrencyCode(result.CurrencyCode);
            return result;
        }

        public AGGCurrencyModel ConvertToAGGCurrencyObject(string currency)
        {
            return new AGGCurrencyModel
            {
                currencyCode = ConvertCurrencyCode(currency),
                isIso4217 = IsCurrencyIso4217(currency)
            };
        }


        public string ConvertCurrencyCode(string currency)
        {
            if (currency == "MYR100")
            {
                return "hMYR";
            }

            if (currency == "BETA")
            {
                return "MYR";
            }

            return currency;
        }

        public EventPrizeTypeEnum ConvertAGGBonusTypeToEnum(string bonusChannel)
        {
            var eventPrizeType = EventPrizeTypeEnum.AGG_OtherBonus;

            switch (bonusChannel)
            {
                case "cash_card":
                    eventPrizeType = EventPrizeTypeEnum.AGG_CashCard;
                    break;

                case "gift_code":
                    eventPrizeType = EventPrizeTypeEnum.AGG_GiftCode;
                    break;

                case "vip":
                    eventPrizeType = EventPrizeTypeEnum.AGG_VIP;
                    break;

                case "ranking":
                    eventPrizeType = EventPrizeTypeEnum.AGG_Ranking;
                    break;

                case "raffle_mission":
                case "raffle":
                    eventPrizeType = EventPrizeTypeEnum.AGG_Raffle;
                    break;

                case "roulette":
                    eventPrizeType = EventPrizeTypeEnum.AGG_Roulette;
                    break;

                case "mission_daily":
                case "mission_weekly":
                    eventPrizeType = EventPrizeTypeEnum.AGG_BonusMission;
                    break;

                default:
                    eventPrizeType = EventPrizeTypeEnum.AGG_OtherBonus;
                    break;
            }


            return eventPrizeType;
        }

        private bool IsCurrencyIso4217(string currency)
        {
            if (currency == "MYR100" ||
                currency == "KIDR" ||
                currency == "KVND" ||
                currency == "KMMK" ||
                currency == "KCOP" ||
                currency == "hMYR")
            {
                return false;
            }

            return true;
        }



        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = GetByBetRecordNoPGS(model.BetRecordNoPGS);

            //若資料已被清除 嘗試從perGame撈資料
            if (queryBetRecord == null)
            {
                var perGameRepo = IOCHelper.InstanceRepository<PerGameDetailLogRepository>();

                var queryPerGame = perGameRepo.GetByTransactionNo_WithGameTimeRange(model.BetRecordNoPGS);

                if (queryPerGame == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundOrAlreadyDelete);
                }

                queryBetRecord = new BetRecordAGGEntity
                {
                    GameID = queryPerGame.GameID,
                    BetRecordNo = queryPerGame.ThirdPartyBetRecordNo,
                    MerchantID = queryPerGame.MerchantID,
                    ThirdSettleTime = queryPerGame.GameTime,
                };
            }

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            var apiBegin = queryBetRecord.ThirdSettleTime.HasValue ? queryBetRecord.ThirdSettleTime.Value : queryBetRecord.CreateTime;
            var apiEnd = queryBetRecord.ThirdSettleTime.HasValue ? queryBetRecord.ThirdSettleTime.Value : queryBetRecord.CreateTime.AddHours(1);

            string apiResultUrl = GetRecordReplayHistoryByApi(new ExternalVendorGetSingleBetRecordRequestParameter
            {
                BetRecordNo = queryBetRecord.BetRecordNo,
                MerchantID = queryBetRecord.MerchantID,
                GameCode = queryGame.ThirdPartyGameCode,
                QueryReplayExtraParameter = new QueryReplayExtraParameter
                {
                    BeginTime = apiBegin.AddHours(-1 * 4),
                    EndTime = apiEnd.AddHours(1),
                }
            });

            //取代成pgs域名
            apiResultUrl = ReplaceUrlDomainToPgsBrand(apiResultUrl);

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = apiResultUrl
            };
        }


        public GameListIdInfoBrief GetGameByThirdGameCode(string thirdGameCode)
        {
            return base.GetGameByThirdGameCode_V2(thirdGameCode);
        }



        private string GetExtraInfoByRawRequest(AGGSingleWalletAPITransactionRequestModel rawInfo)
        {
            try
            {
                if (rawInfo.Operations.IsEmptyOrNull())
                {
                    rawInfo.Operations = rawInfo.Meta?.Operations;
                }

                var matchInfo = rawInfo?.Operations?.FirstOrDefault(z => z.TagType == "Operation");

                if (matchInfo == null || matchInfo.Tag == null)
                    return string.Empty;

                return $"RTP:" + matchInfo.Tag.RTP.ToString() + " ";
            }
            catch
            {

            }

            return string.Empty;
        }


        private DateTime? GetSettleTimeByRawRequest(AGGSingleWalletAPITransactionRequestModel rawInfo)
        {
            try
            {
                if (rawInfo.Operations.IsEmptyOrNull())
                {
                    rawInfo.Operations = rawInfo.Meta?.Operations;
                }

                var matchInfo = rawInfo?.Operations?.FirstOrDefault(z => z.OperationType == "WagerLifecycleOperation");

                string targetTimeText = matchInfo?.ExecutionTime;

                if (string.IsNullOrWhiteSpace(targetTimeText))
                {
                    targetTimeText = rawInfo.ExecutionTime;
                }

                if (string.IsNullOrWhiteSpace(targetTimeText))
                {
                    return null;
                }

                var parseResult = DateTime.Parse(targetTimeText).ToUniversalTime();

                return parseResult;
            }
            catch
            {

            }

            return null;
        }

        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res, string apiCategory = "")
        {
            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
            {
                if (res.IsTimeoutException)
                {
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_Timeout, new ErrorDetailModel
                    {
                        ColumnName = apiCategory
                    });
                }
                else
                {
                    string extraInfo = $"{res.ResponseStatusCode} , " + DateTimeHelper.GetCurrentTimeYYYY_MM_DD_HH_MM_SS(8) + " GMT+8 ";
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel(extraInfo));
                }
            }

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail);
        }

        private ApiResultCheckModel CheckResponseErrorByKeyword<T>(ApiRequestResultModel<T> response, string debugUUID, bool isThrowException)
        {
            string responseText = response.ResponseText;

            if (string.IsNullOrWhiteSpace(responseText))
            {
                responseText = response.Exception?.Message;
            }

            ApiResultCheckModel reuslt = new ApiResultCheckModel
            {
                IsSuccess = true,
                DebugUUID = debugUUID,
                ResponseText = responseText
            };

            ApiErrorCode errorCode = ApiErrorCode.Success;

            //檢查http錯誤、model解析是否成功
            if (response.Exception != null)
                errorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;

            else if (response.IsTimeoutException)
                errorCode = ApiErrorCode.ThirdPartyApiFail_Timeout;

            else if (responseText.Contains("reason") && responseText.Contains("code"))
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;
            }

            if (errorCode != ApiErrorCode.Success)
            {
                reuslt.IsSuccess = false;

                if (isThrowException)
                {
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel
                    {
                        IsAlwaysNotify = true,
                        LogExtraInfo = responseText
                    });
                }
            }

            return reuslt;
        }



        private void SetRequestInfoByMerchantID(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "")
        {
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);
            SetCommonRequestInfo(req, apiCategory, requestUUID);
        }

        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, string requestUUID = "")
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);
            SetCommonRequestInfo(req, apiCategory, requestUUID);
        }


        private BetRecordAGGEntity GetByBetRecordNoPGS(string betRecordNoPGS)
        {
            return BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS, isNVarchar: false);
        }


        private void SetCommonRequestInfo(RequestParameters req, string apiCategory, string requestUUID = "")
        {
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.GetDescriptionOrToString(),
                Log_RequestUUID = requestUUID
            };


            //測試環境 直接覆蓋配置資料 (方便本機測試)
            if (SettingHelper.IsDev)
            {
                //querySettings = new List<VendorConfigSettingEntity>
                //{
                //    new VendorConfigSettingEntity
                //    {
                //        SettingKey =  (int)VendorConfigSettingEnum.BetReportApiUrl,
                //        Value = "https://example.invalid"
                //    },
                //    new VendorConfigSettingEntity
                //    {
                //        SettingKey =  (int)VendorConfigSettingEnum.MainAssignVendorID,
                //        Value = "slot88-thb"
                //    },
                //    new VendorConfigSettingEntity
                //    {
                //        SettingKey =  (int)VendorConfigSettingEnum.MainSecretKeyOrApiToken,
                //        Value = "2b2ddbb5-8bab-477b-8193-b2d0e5df9b54"
                //    },
                //};
            }

            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;
            string apiKey = [REDACTED];
            string aggIntegrator = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;

            if (apiDomain.IsNullOrWhiteSpace())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel
                {
                    ColumnName = "apiDomain",
                });
            }

            req.UserAgent = "target";
            req.BaseUrl = apiDomain;
            req.AddHeaderData("X-API-KEY", apiKey);
            req.AddHeaderData("X-INTEGRATOR", aggIntegrator);
            req.AddHeaderData("X-INTEGRATORS", aggIntegrator);
        }


    }
}
