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
using target.DA.Repository;
using System.Collections.Concurrent;
using target.Utility.Helper;
using game.Entity.Game;
using System.Threading.Tasks;
using System.Threading;
using target.Utility.Utility.ThirdParty;
using target.Utility.Helper.HttpContext;
using target.Models;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using Lck.Utility.Extensions.Helper;
using target.Utility.Extensions.Web;
using System.Transactions;
using target.Utility.IOC;

namespace target.BL.Services
{

    public partial class GameVendorSingleWalletService : BaseGameVendorCallbackService, IGameVendorSingleWalletService
    {
        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public BetRecordCustomizedSettingSWSessionTokenRepository BetRecordCustomizedSettingSWSessionTokenRepository { get; set; }

        public BetRecordCustomizedSettingAGGEventRepository BetRecordCustomizedSettingAGGEventRepository { get; set; }

        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public BetRecordRepository BetRecordRepository { get; set; }

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public BetRecordRawWagerTagMappingRepository BetRecordRawWagerTagMappingRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public OneTimesJobSettingRepository OneTimesJobSettingRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogAccountingIgnoreRepository PerGameDetailLogAccountingIgnoreRepository { get; set; }

        public PerGameDetailLogTagMappingRepository PerGameDetailLogTagMappingRepository { get; set; }

        public PlaceBetRaiseRecordRepository PlaceBetRaiseRecordRepository { get; set; }

        public PlayerGameSummaryRepository PlayerGameSummaryRepository { get; set; }

        public PerGameDetailChangeLogRepository PerGameDetailChangeLogRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public IPlayerWalletRefService PlayerWalletRefService { get; set; }

        public ISingleWalletCallbackNotifyProcessRefService SingleWalletCallbackNotifyProcessRefService { get; set; }

        public IGameVendorSingleWalletCommonLogicProcessService GameVendorSingleWalletCommonLogicProcessService { get; set; }


        public SingleWalletPlayerExtraInfoModel GetPlayerExtraInfo(GameVendorTypeEnum vendorID, int playerID)
        {
            bool isIgnoreAccounting = PlayerTagMappingRepository.IsPlayerTagExistByCache(playerID, SystemTagEnum.Player_GameVendorAcoountingIgnoreThisPlayer.ToInteger());

            var result = new SingleWalletPlayerExtraInfoModel
            {
                PlayerID = playerID,
                IsAccountingIgnoreThisPlayer = isIgnoreAccounting
            };

            if (vendorID == GameVendorTypeEnum.AGGFish)
            {
                var queryPlayer = PlayerRepository.GetBriefByCache(playerID);
                var querySetting = VendorConfigSettingRepository.GetTargetMerchantVendorSettings(queryPlayer.MerchantID, vendorID);

                result.AGGFishMerchantID = querySetting.GetBySettingKey(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
                result.AGGFishOperatorID = querySetting.GetBySettingKey(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            }

            return result;
        }


        public SingleWalletGetWalletBalanceResultViewModel GetWalletBalance(GameVendorTypeEnum vendorID, SingleWalletGetWalletBalanceRequestParameter model)
        {
            int playerID = GetPlayerIdFromRequest(model);

            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            PlayerRepository.CheckStatusAndThrowException(queryPlayer);

            var queryMerchant = GetMerchantValidateInfoByMemoryCache(queryPlayer.MerchantID, vendorID);

            var queryWallet = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(queryPlayer.PlayerID, model.SessionToken, isLockQuery: false);

            string currency = queryMerchant.Currency;

            decimal gamePoint = ConvertMoneyToGamePoint(queryWallet.Amount, queryMerchant.ConvertRate);

            return new SingleWalletGetWalletBalanceResultViewModel
            {
                PlayerID = playerID,
                PlayerName = queryPlayer.PlayerName,
                BalanceAmount = queryWallet.Amount,
                BalanceGamePoint = gamePoint,
                Currency = currency
            };
        }


        public void CheckPlayerIsValidAndSystemMaintain(GameVendorTypeEnum vendorID, int playerID)
        {
            var validModel = new { PlayerID = playerID };

            validModel.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            PlayerRepository.CheckStatusAndThrowException(queryPlayer);

            var queryMerchant = GetMerchantValidateInfoByMemoryCache(queryPlayer.MerchantID, vendorID);

            //AGG捕魚因為採用skip模式。只能在auth情況檢查
            if (vendorID == GameVendorTypeEnum.AGGFish)
            {
                CheckFishGameEnableOrThrowException(vendorID, queryPlayer.MerchantID);
            }

            CheckSystemMaintainOrSkipTag(vendorID, queryMerchant, playerID);
        }


        private void CheckFishGameEnableOrThrowException(GameVendorTypeEnum vendorID, int merchantID)
        {
            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                Status = NormalActiveStatus.Enable.ToInteger(),
                GameVendorTypes = vendorID.ToInteger().ToListCollection()
            });

            if (queryGames.IsNotEmptyNull())
            {
                var queryGameMapping = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
                {
                    MerchantIDs = merchantID.ToListCollection(),
                    GameIDs = queryGames.Select(x => x.GameID).ToList(),
                });

                bool isAnyEnableFishGame = queryGameMapping.Any(x => x.Status == NormalActiveStatus.Enable.ToInteger());

                if (!isAnyEnableFishGame)
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_GameStatus);
                }
            }
            else
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_GameStatus);
            }
        }


        private bool CheckIsSkipAccountingPlayer(int playerID)
        {
            try
            {
                string cacheKey = CacheKey.GlobalAccountingIgnorePlayerIDs();
                var cacheTime = CacheHelper.InstanceMemoryCacheTime();

                var queryData = CacheHelper.GetCacheOrQueryByMemoryCache(cacheKey, () =>
                {
                    string selectColumns = " PlayerID ";

                    var query = PlayerTagMappingRepository.GetByFilter(new QueryPlayerTagMappingRequestViewModel
                    {
                        TagIDs = SystemTagEnum.Player_GameVendorAcoountingIgnoreThisPlayer.ToInteger().ToListCollection()

                    }, selectColumns);

                    return new AccountingIgnorePlayerTempModel
                    {
                        PlayerIDs = query.Select(x => x.PlayerID).ToList()
                    };
                }, cacheTime);

                if (queryData == null || queryData.PlayerIDs.IsEmptyOrNull())
                    return false;

                return queryData.PlayerIDs.Any(id => id == playerID);
            }
            catch
            {

            }

            return false;
        }

        public SingleWalletVendorCustomizedResponseConvertModel PlaceBet(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model)
        {
            List<TimeDebugModel> debugTimeList = new List<TimeDebugModel>();

            DateTime processBeginTime = DateTimeHelper.GetCurrentTime();

            AppendTimeDebug(debugTimeList, "Debug_ProcessBeginTime");

            model.RuleFor(z => z.BetAmount).Rule_NotMinus();
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();
            model.ExtraInfo = model.ExtraInfo ?? new SingleWalletPlaceBetExtraParameter();

            //判斷是否已經扣款過 betAmount金額
            bool isDelayTooMuchTime = false;

            int playerID = GetPlayerIdFromRequest(model);

            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            PlayerRepository.CheckStatusAndThrowException(queryPlayer);

            AppendTimeDebug(debugTimeList, "Debug_BeforeGetMerchantInfo");

            int merchantID = queryPlayer.MerchantID;

            //這邊是用memoryCache存的 (真的很少改變的數值才建議存這邊)
            var queryMerchant = GetMerchantValidateInfoByMemoryCache(merchantID, vendorID);

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);

            HeaderValueHelper.AppendReferOrderNo(model.BetRecordNo);

            AppendTimeDebug(debugTimeList, "Debug_AferGetMerchantInfo");

            IGameVendorIntegrationService vendorService = ServiceInstanceHelper.InstanceVendorService(vendorID);

            //檢查遊戲存在/遊戲開啟
            var queryGame = QueryGameByCache(vendorService, model.GameCode);

            //檢查遊戲商維護
            CheckSystemMaintainOrSkipTag(vendorID, queryMerchant, playerID);

            //檢查商戶是否開啟遊戲
            if (!queryMerchant.EnableGameIDs.Contains(queryGame.GameID))
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_GameStatus);
            }

            //嚴格token模式
            if (queryMerchant.IsRestrictGameTokenMode)
            {
                CheckRestrictModeAndGameMatch(model.ExtraInfo, queryPlayer.MerchantID, queryGame.GameID);
            }

            bool isSkipAccountingData = CheckIsSkipAccountingPlayer(queryPlayer.PlayerID);
            bool isZeroBetEventPrize = false;

            if (model.ExtraInfo.ReferEventPrizeType != EventPrizeTypeEnum.Default)
            {
                isZeroBetEventPrize = true;
                isSkipAccountingData = true;
            }
            else if (isSkipAccountingData)
            {
                var queryGameDetail = GameListRepository.GetGameCacheByVendorGameCode(vendorID.ToInteger(), model.GameCode);

                if (!queryGameDetail.IsSupportForAccountingIgnorePlayer)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_PlayerNotSupportThisGame, new ErrorDetailModel
                    {
                        ColumnName = "SupportForAccountingIgnorePlayer",
                        IsAlwaysNotify = true
                    });
                }
            }

            //檢查商戶設置 是否有禁止特殊注單，根據遊戲商不同。會有特殊的操作模式
            CheckPlaceBetForbiddenByGameVendorSpecialOperate(model, queryMerchant);

            AppendTimeDebug(debugTimeList, "002");

            var transactionNoGenerate = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();
            string transactionNoPGS = transactionNoGenerate.TransactionNo;

            // step 1. 檢查單號是否重複、產生短效型cache、是否為加注型注單
            var recordCheckResult = CheckDuplicateRecordAndException(vendorID, model, vendorService, queryGame, playerID, transactionNoPGS, queryMerchant);

            // 針對二合一結算的遊戲商。0元注單會有補救機制 (直接返回、進入settle程序)
            if (recordCheckResult.IsWagerZeroBetAndWatingSettle ||
               recordCheckResult.IsForceGoToSettleProcess)
            {
                return new SingleWalletVendorCustomizedResponseConvertModel
                {
                    CommonInfo = new GameVendorBetCommonInfo
                    {
                        BetRecordNoPGS = recordCheckResult.ExistBetRecordNoPGS,
                        GameID = queryGame.GameID,
                        PlayerID = playerID,
                        Currency = queryMerchant.Currency,
                        BetRecordNo = model.BetRecordNo,
                        IsSingleWallet = isSingleWallet
                    }
                };
            }

            AppendTimeDebug(debugTimeList, "003");

            if (recordCheckResult.IsExecuteRaiseProcess)
            {
                transactionNoPGS = recordCheckResult.ExistBetRecordNoPGS;
            }

            //需要經過 遊戲幣與法幣轉換
            decimal betPoint = model.BetAmount;
            decimal betAmount = ConvertGamePointToMoney(betPoint, queryMerchant.ConvertRate);

            if (betAmount == 0 && !isZeroBetEventPrize)
            {
                //檢查遊戲商是否支援0元投注
                if (!IsSupportZeroBet(vendorID, model))
                {
                    model.RuleFor(z => z.BetAmount).Rule_NotZeroNotMinus();
                }
            }

            //禁止超過小數點兩位
            if (RegexRuleHelper.IsOverThanDecimalPlaces(betAmount, 4))
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueMustPlacesTwo, new ErrorDetailModel("BetAmount"));

            AppendTimeDebug(debugTimeList, "005");

            // step 2. callback調用 & 更新錢包金額(禁止錢包被扣到負數)
            UpdateBalanceSPResult updateResult = UpdateWalletAmount_PlaceBetPrcoess(model, queryPlayer, queryMerchant, queryGame.GameID, transactionNoPGS, betAmount);

            AppendTimeDebug(debugTimeList, "008");

            List<string> debugTimes = debugTimeList
               .Select(z => z.Time.ToString("HH:mm:ss"))
               .ToList();

            string debugTime = $"time:" + string.Join(",", debugTimes);

            try
            {
                if (!recordCheckResult.IsExecuteRaiseProcess)
                {
                    //==============================
                    // Case 1. 標準流程
                    //==============================

                    // step 3. 寫入一筆單號對應Mapping
                    BetRecordTransactionNoMappingRepository.Create(transactionNoPGS, model.BetRecordNo, vendorID.ToInteger());

                    var requiredInfo = new InsertRawBetRecordRequiredInfoModel
                    {
                        GameID = queryGame.GameID,
                        ThirdGameCode = queryGame.ThirdPartyGameCode,
                        MerchantID = queryPlayer.MerchantID,
                        PlayerID = queryPlayer.PlayerID,
                        PlayerName = queryPlayer.PlayerName,
                        BetRecordNoPGS = transactionNoPGS,
                        BetRecordNo = model.BetRecordNo,
                        BetAmount = betAmount,
                        BetGamePoint = betPoint,
                        ValidAmount = model?.ExtraInfo?.ReferValidAmount,
                        ConvertRate = queryMerchant.ConvertRate,
                        VendorConfigID = queryMerchant.VendorConfigID,
                        AmountBeforeBet = updateResult.BeforeAmount,
                        PartitionTime = transactionNoGenerate.TimeWithHour
                    };

                    // step 4. 寫入一筆投注紀錄 (raw betRecord)
                    vendorService.WriteRawBetRecordWhenPlaceBet(requiredInfo, model.GameVendorRawRequestInfo);

                    // step 5. 額外的debug資訊紀錄在另張table (清除佔空間的資料也比較容易)
                    var insertEntity = new BetRecordExtraInfoEntity
                    {
                        BetRecordNoPGS = requiredInfo.BetRecordNoPGS,
                        PartitionTime = requiredInfo.PartitionTime,
                        ExtraInfo = $"{RuntimeHelper.GetK8sInstanceName()} , {debugTime}",
                        IPAddress = model.PlayerIPAddress ?? ""
                    };

                    BetRecordExtraInfoRepository.CreateTryCatch(insertEntity, false);

                    // step 5-2 標記特殊標籤
                    if (isSkipAccountingData)
                    {
                        BetRecordTagMappingRepository.CreateIfNotExist(requiredInfo.BetRecordNoPGS, SystemTagEnum.BetRecord_AccountingIgnore);
                    }

                    // step 5-3 AGG虛寶卡/freespin
                    if (isZeroBetEventPrize)
                    {
                        try
                        {
                            //過渡期 先兩張表都寫入
                            BetRecordCustomizedSettingRepository.Create(requiredInfo.BetRecordNoPGS, BetRecordCustomizedSettingEnum.ReferEventPrize, model.ExtraInfo.ReferEventPrizeType.ToIntegerString());

                            BetRecordCustomizedSettingAGGEventRepository.Create(requiredInfo.BetRecordNoPGS, BetRecordCustomizedSettingEnum.ReferEventPrize, model.ExtraInfo.ReferEventPrizeType.ToIntegerString());
                        }
                        catch
                        {

                        }
                    }

                    // step 5-5 加注型遊戲會紀錄額外資訊
                    if (recordCheckResult.IsSupportRaiseProcessGame)
                    {
                        var now = DateTimeHelper.GetCurrentTime();

                        PlaceBetRaiseRecordRepository.Create(new PlaceBetRaiseRecordEntity
                        {
                            BetRecordNoPGS = transactionNoPGS,
                            RaiseTransactionNo = model.RaiseTransactionNo ?? "",
                            Amount = model.BetAmount,
                            CreateTime = now,
                            UpdateTime = now,
                            PlayerID = queryPlayer.PlayerID,
                            GameID = queryGame.GameID,
                            PartitionTime = transactionNoGenerate.TimeWithHour,
                            Status = BetRecordRaiseBetStatusEnum.Accept.ToInteger()
                        });
                    }
                }
                else
                {
                    //==============================
                    // Case 2. 加注流程 (僅部分遊戲商)
                    //==============================

                    DoRaiseBetProcess(vendorID, model, queryPlayer.PlayerID, queryGame.GameID, recordCheckResult.ExistBetRecordNoPGS);
                }

                decimal aftetAmount = updateResult.AfterAmount;
                decimal aftetGamePoint = ConvertMoneyToGamePoint(aftetAmount, queryMerchant.ConvertRate);

                decimal beforeAmount = updateResult.BeforeAmount;
                decimal beforeGamePoint = ConvertMoneyToGamePoint(beforeAmount, queryMerchant.ConvertRate);

                DateTime processEndTime = DateTimeHelper.GetCurrentTime();

                double spendTime = (processEndTime - processBeginTime).TotalMilliseconds;
                isDelayTooMuchTime = spendTime >= 8000;

                // step 7. 如果前半段花太多時間，需要先檢查是否此單已被標記Cancel
                if (isDelayTooMuchTime)
                {
                    CheckCancelTagExistAndAutoRefund(vendorID, model.BetRecordNo, transactionNoPGS, model.GameCode, spendTime, queryMerchant.WalletMode, debugTime);
                }

                return new SingleWalletVendorCustomizedResponseConvertModel
                {
                    CommonInfo = new GameVendorBetCommonInfo
                    {
                        BetRecordNo = model.BetRecordNo,
                        BetRecordNoPGS = transactionNoPGS,
                        GameID = queryGame.GameID,
                        Currency = queryMerchant.Currency,
                        BeforeGamePoint = beforeGamePoint,
                        AfterGamePoint = aftetGamePoint,
                        PlayerID = playerID,
                        IsSingleWallet = isSingleWallet
                    },
                };

            }
            catch (Exception ex)
            {
                var myParameter = new PlaceBetExceptionProcessExtraParameter
                {
                    BetAmount = betAmount,
                    VendorID = vendorID,
                    CallbackParam = model,
                    IsDelayTooMuchTime = isDelayTooMuchTime,
                    QueryPlayer = queryPlayer,
                    QueryGame = queryGame,
                    QueryMerchant = queryMerchant,
                    TransactionNoPGS = transactionNoPGS,
                    Exception = ex
                };

                //包含通知、標記tag、寫入eazTempValue
                ExtraProcessWhenPlaceBetException(myParameter);
                throw;
            }
        }

        public SingleWalletVendorCustomizedResponseConvertModel SettleBet(GameVendorTypeEnum vendorID, GameVendorSettleCallbackParameter model)
        {
            List<TimeDebugModel> debugTimeList = new List<TimeDebugModel>();

            AppendTimeDebug(debugTimeList, "Debug_ProcessBeginTime");

            //紀錄單號比較方便追蹤
            HeaderValueHelper.AppendReferOrderNo(model.BetRecordNo);

            IGameVendorIntegrationService vendorService = ServiceInstanceHelper.InstanceVendorService(vendorID);

            var queryGame = QueryGameByCache(vendorService, model.GameCode);

            var vendorDefine = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendorID);

            model.SettleExtraInfo = model.SettleExtraInfo ?? new ThirdGameSettleExtraParameter();

            //部分遊戲商 是直接轉錢進去玩 不透過一般的placeBet流程、因為結算的時候會直接建單
            CreateBetRecordInSettleProcess(vendorID, model, queryGame.GameID);

            var myFilter = new BetRecordInfoQueryParameter
            {
                BetRecordNo = model.BetRecordNo,
                BetRecordNoPGS = model.SettleExtraInfo?.BetRecordNoPGS, //僅特殊情況會有值
                ThirdPartyGameCode = queryGame?.ThirdPartyGameCode,
                GameID = queryGame.GameID,
                PlayerID = model.PlayerID,
                IsQueryRecentRange = true,
                DebugSourceMethod = "SettleBet"
            };

            //嘗試從cache取得pgs單號 (用PK查速度會較好)
            SetBetRecordNoPGS_IfCacheExist(vendorID, model, myFilter);

            AppendTimeDebug(debugTimeList, "Debug_Before_Step1_1");

            // step 1-1. 檢查注單存在 (檢查狀態是在 UpdateRawBetRecordAfterSettlement 處理)
            var queryExistBetRecord = vendorService.GetByBetRecordNo(myFilter);

            if (queryExistBetRecord == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);
            }

            if (queryExistBetRecord.BetRecordNo != model.BetRecordNo)
            {
                throw new TerproException(ApiErrorCode.NotFoundBetRecord, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    LogExtraInfo = "SettleBet BetRecordNo單號無法匹配。cache可能異常",
                });
            }

            //幫助Debug的Header資訊
            if (queryExistBetRecord != null)
            {
                HeaderValueHelper.AppendDebugInfoForException(queryExistBetRecord.BetRecordNoPGS);
                HeaderValueHelper.AppendLogInfo_IdentityKey(queryExistBetRecord.PlayerID.ToString());
                HeaderValueHelper.AppendPlayerID(queryExistBetRecord.PlayerID);
            }

            MerchantValidateInfoCacheModel queryMerchant = null;

            if (queryExistBetRecord.Status == BetRecordStatusEnum.SettleComplete.ToInteger() ||
                vendorID == GameVendorTypeEnum.AGG)
            {
                queryMerchant = GetMerchantValidateInfoByMemoryCache(queryExistBetRecord.MerchantID, vendorID);

                //exception處理可能會用到
                HeaderValueHelper.AppendMerchantCurrency(queryMerchant.Currency);
                HeaderValueHelper.AppendBetRecordNo(model.BetRecordNo);

                decimal payoutAmount = queryExistBetRecord.PayoutAmount;

                if (queryExistBetRecord.Status != BetRecordStatusEnum.SettleComplete.ToInteger())
                {
                    payoutAmount = model.PayoutGamePoint;
                }

                decimal afterAmount = 0;

                if (queryExistBetRecord.AmountBeforeBet.HasValue)
                {
                    afterAmount = queryExistBetRecord.AmountBeforeBet.Value - queryExistBetRecord.BetAmount + payoutAmount;
                }

                HeaderValueHelper.AppendAfterAmount(afterAmount);

                if (vendorID == GameVendorTypeEnum.Jili)
                {
                    SetJiliTempHeader(model.SessionToken, queryExistBetRecord, queryMerchant);
                }
            }

            //GPI的遊戲會用到特殊exception判斷
            ThrowExceptionBy_GPILogic(model, queryExistBetRecord);

            //RG、Vplus 特有機制 每次freeSpin都會結算
            if (model.SettleExtraInfo != null)
            {
                //部分遊戲商會寫入額外紀錄 (每一筆的detail)
                ExtraProcessForNoneCompletedSettle(model, queryExistBetRecord);

                if (model.SettleExtraInfo.IsNotCompleteSettleSubItem)
                {
                    var queryMerchantRG = GetMerchantValidateInfoByMemoryCache(queryExistBetRecord.MerchantID, vendorID);

                    decimal extraPayoutAdjust = model.PayoutGamePoint;

                    //RG會在controller層 有另外邏輯處理
                    if (vendorID == GameVendorTypeEnum.RG)
                    {
                        extraPayoutAdjust = 0;
                    }

                    return new SingleWalletVendorCustomizedResponseConvertModel
                    {
                        CommonInfo = new GameVendorBetCommonInfo
                        {
                            Currency = queryMerchantRG.Currency,
                            BeforeGamePoint = queryExistBetRecord.AmountBeforeBet ?? 0,
                            AfterGamePoint = (queryExistBetRecord.AmountBeforeBet ?? 0) - queryExistBetRecord.BetAmount + extraPayoutAdjust,
                            BetRecordNo = queryExistBetRecord.BetRecordNo,
                            BetRecordNoPGS = queryExistBetRecord.BetRecordNoPGS,
                            PlayerID = queryExistBetRecord.PlayerID,
                            GameID = queryGame.GameID,
                            WalletUpdateAmount = 0,
                            ThirdSettleDate = queryExistBetRecord.CreateTime,
                            IsSingleWallet = AppHelper.IsSingleWalletMode(queryMerchantRG.WalletMode),
                        },
                    };
                }
            }

            string requestUUID = Guid.NewGuid().ToString();

            decimal convertRateWhenPlaceBet = queryExistBetRecord.ConvertRate;

            if (convertRateWhenPlaceBet <= 0)
            {
                convertRateWhenPlaceBet = GetMerchantConvertRate(queryExistBetRecord.MerchantID, vendorID).ConvertRate;
            }

            decimal myPayoutGamePoint = model.PayoutGamePoint;
            decimal myPayoutAmount = myPayoutGamePoint / convertRateWhenPlaceBet;

            var requiredInfo = new SettlementBetRecordRequiredBriefInfoModel
            {
                MerchantID = queryExistBetRecord.MerchantID,
                GameID = queryGame.GameID,
                BetRecordNoPGS = queryExistBetRecord.BetRecordNoPGS,
                PayoutGamePoint = myPayoutGamePoint,
                PayoutAmount = myPayoutAmount,
                DebugExtraMemo = requestUUID,
                QueryBetAmountBeforeUpdate = queryExistBetRecord.BetAmount,
                ConvertRateWhenPlaceBet = convertRateWhenPlaceBet,
            };

            if (model.SettleExtraInfo != null && model.SettleExtraInfo.NewBetAmount.HasValue)
            {
                //預扣型注單
                requiredInfo.NewBetAmount = model.SettleExtraInfo.NewBetAmount.Value;
            }
            else if (vendorDefine.IsRaiseProcessGameExist)
            {
                //加注型注單。結算後還會更新投注金額(N次加總)
                requiredInfo.NewBetAmount = GetTotalBetAmountForRaiseBet(queryExistBetRecord.GameID, queryExistBetRecord.BetRecordNoPGS);
            }

            AppendTimeDebug(debugTimeList, "Debug_Before_QueryMerchant");

            //查詢商戶 (部分邏輯下 會在更前面查詢過一次)
            if (queryMerchant == null)
            {
                queryMerchant = GetMerchantValidateInfoByMemoryCache(queryExistBetRecord.MerchantID, vendorID);
            }

            // step 2. 更新 raw records (輸贏金額、狀態)
            var settlementInfo = vendorService.UpdateRawBetRecordAfterSettlement(requiredInfo, model.GameVendorRawRequestInfo);

            // step 2-2 額外的string資訊 紀錄在子表中 (空間&索引考量)
            if (SettingHelper.IsDev)
            {
                string debugInfo = $"k8s:{RuntimeHelper.GetK8sInstanceName()},UUID:{requestUUID},{settlementInfo.DebugInfoForNextProcess}";
                BetRecordExtraInfoRepository.CreateOrUpdateAsync(requiredInfo.BetRecordNoPGS, debugInfo);
            }

            // step 2-3 標記特殊tag
            if (model.SettleExtraInfo != null && model.SettleExtraInfo.IsSkipUpdateBalance)
            {
                //ps. 請勿呼叫async。可能導致捕魚遊戲致命錯誤(單錢包通知的部分會判斷此)
                BetRecordTagMappingRepository.CreateTryCatch(new BetRecordTagMappingEntity
                {
                    BetRecordNoPGS = requiredInfo.BetRecordNoPGS,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    TagID = SystemTagEnum.BetRecord_IsSkipPayoutProcess.ToInteger(),
                });
            }

            // step 2-4 延遲結算標籤
            if (settlementInfo.SettlementTime.HasValue && (settlementInfo.SettlementTime.Value - settlementInfo.CreateTime).TotalMinutes >= 5)
            {
                BetRecordTagMappingRepository.CreateTryCatch(new BetRecordTagMappingEntity
                {
                    BetRecordNoPGS = requiredInfo.BetRecordNoPGS,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    TagID = SystemTagEnum.BetRecord_IsDelaySettle.ToInteger(),
                });
            }

            bool isSkipAccountingData = CheckIsSkipAccountingPlayer(queryExistBetRecord.PlayerID);

            //如果有標記屬於活動成本
            if (model.SettleExtraInfo.ReferEventPrizeType != EventPrizeTypeEnum.Default)
            {
                isSkipAccountingData = true;
            }

            bool mustWritePerGame = !isSkipAccountingData;

            decimal walletUpdateAmount_payout = 0;

            AppendTimeDebug(debugTimeList, "Debug_Before_Step3");

            try
            {
                string debugText = "";

                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");

                // step 3. 轉成perGameLog格式
                var convertPerGameResult = vendorService.FormatParsing_ConvertBetRecordToPerGameDetailLog(queryExistBetRecord.BetRecordNoPGS);
                var convertPerGame = convertPerGameResult.PerGameDetailLog;

                // step 3-1. 部分遊戲商有額外tag要判斷
                CheckExtraTagToAdd(model, convertPerGameResult);

                // step 3-2. 強制結束流程 (如果 投注/派彩/有效金額 都是0元...捕魚會發生這種狀況)
                if (convertPerGameResult.IsAllAmountZeroThenSkipProcess)
                {
                    return new SingleWalletVendorCustomizedResponseConvertModel
                    {
                        CommonInfo = new GameVendorBetCommonInfo
                        {
                            Currency = queryMerchant.Currency,
                            BeforeGamePoint = 0,
                            AfterGamePoint = 0,
                            BetRecordNo = queryExistBetRecord.BetRecordNo,
                            BetRecordNoPGS = queryExistBetRecord.BetRecordNoPGS,
                            GameID = queryGame.GameID,
                            WalletUpdateAmount = 0,
                            ThirdSettleDate = convertPerGame.ThirdSettleDate,
                        },
                    };
                }

                AppendTimeDebug(debugTimeList, "Debug_Before_Step4");
                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");

                // step 4. 寫入perGameLog
                long perGameDetailID = 0;

                if (mustWritePerGame)
                {
                    convertPerGameResult.ExtraAddTags = convertPerGameResult.ExtraAddTags ?? new List<SystemTagEnum>();

                    //標記 freeGame
                    if (model.SettleExtraInfo.IsFreeGame)
                    {
                        convertPerGameResult.ExtraAddTags.Add(SystemTagEnum.PerGameDetailLog_FreeGame);
                    }

                    //標記 JP
                    if (model.SettleExtraInfo.IsJackpot)
                    {
                        convertPerGameResult.ExtraAddTags.Add(SystemTagEnum.PerGameDetailLog_Jackpot);
                    }

                    //標記 JILI特殊標籤
                    if (model.SettleExtraInfo.IsJiliPreserveGame)
                    {
                        convertPerGameResult.ExtraAddTags.Add(SystemTagEnum.PerGameDetailLog_JiliPreserveGame);
                    }

                    //標準perGame (計算帳務)
                    perGameDetailID = WritePerGameLog(convertPerGameResult, settlementInfo);
                }
                else
                {
                    //特殊perGame (不記帳)
                    WritePerGameLogAccountingIgnore(convertPerGameResult, settlementInfo);
                }

                walletUpdateAmount_payout = convertPerGame.PayoutAmount;


                var processConfig = new SettleBalanceUpdateProcessConfig
                {
                    SessionToken = [REDACTED],
                    RequestUUID = requestUUID,
                    ReferEventPrizeType = model.SettleExtraInfo.ReferEventPrizeType,
                    IsSkipUpdateBalance = model.SettleExtraInfo.IsSkipUpdateBalance,
                    MerchantWalletMode = queryMerchant.WalletMode,
                    Wager_RoundID = settlementInfo.Extra_RoundID,
                    Wager_PayoutAmountWalletUpdate = walletUpdateAmount_payout,
                    EventBonusDetail = new SettleProcessEventDetailModel
                    {
                        ReferThirdPartyEventID = model.SettleExtraInfo?.ReferThirdPartyEventID,
                        ReferValidAmount = queryExistBetRecord.ValidAmount
                    }
                };

                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");


                // step 5. 回調Callback & 更新錢包金額、還包含特殊活動bonus寫入 (轉帳/單錢包)
                UpdateBalanceSPResult updateResult = GameVendorSingleWalletCommonLogicProcessService.UpdateWalletAmountSettleProcess(vendorID, processConfig, convertPerGameResult);

                // step 5-2. 額外金額refund (Jili特定遊戲會用到)
                if (model.SettleExtraInfo != null && model.SettleExtraInfo.RefundPreserveAmount.NotNullNotZeroNotMinus())
                {
                    var myExtraConfig = processConfig.ReturnInjecteObject<SettleBalanceUpdateProcessConfig>();
                    myExtraConfig.Wager_PayoutAmountWalletUpdate = model.SettleExtraInfo.RefundPreserveAmount.Value;

                    GameVendorSingleWalletCommonLogicProcessService.UpdateWalletAmountSettleProcess(vendorID, myExtraConfig, convertPerGameResult);
                }

                // PGS後台中 [當前金幣] 慣例上是顯示 未下注前(不含預扣)的金幣
                // 但是在三方遊戲架構下，BeforeAmountc會拿到 已預扣betAmount的金額，因此統一轉換一致的格式)
                // decimal currentAmount = updateResult.BeforeAmount + settlementInfo.BetAmount;

                decimal afterAmount = updateResult.AfterAmount;
                decimal afterGamePoint = ConvertMoneyToGamePoint(afterAmount, convertRateWhenPlaceBet);
                decimal beforeGamePoint = ConvertMoneyToGamePoint(updateResult.BeforeAmount, convertRateWhenPlaceBet);

                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");

                // step 6. 如果特定遊戲商有指定要記錄afterAmount (ps. 效能會較差)
                if (mustWritePerGame)
                {
                    var perGameInfo = convertPerGameResult.PerGameDetailLog;

                    if (vendorDefine.IsWriteAmountAfterSettle)
                    {
                        BetRecordCustomizedSettingRepository.Create(perGameInfo.TransactionNo, BetRecordCustomizedSettingEnum.AmountAfterSettle, updateResult.AfterAmount.ToString());
                    }
                    else
                    {
                        //超過一定時間才結算的單 很容易發生beforeAmount、afterAmount數字次序奇怪。因此強制紀錄afterAmount
                        if ((perGameInfo.GameTime - queryExistBetRecord.CreateTime).TotalSeconds > 180)
                        {
                            BetRecordCustomizedSettingRepository.Create(perGameInfo.TransactionNo, BetRecordCustomizedSettingEnum.AmountAfterSettle, updateResult.AfterAmount.ToString());
                        }
                    }
                }

                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");

                // step 7. 更新玩家遊戲數據統計
                if (mustWritePerGame)
                {
                    PlayerGameSummaryRepository.PlayerGameSummaryValuePlus(new PlayerGameSummaryUpdateParameter
                    {
                        PayoutAmount = convertPerGameResult.PerGameDetailLog.PayoutAmount,
                        BetAmount = convertPerGameResult.PerGameDetailLog.BetAmount,
                        ValidAmount = convertPerGameResult.PerGameDetailLog.ValidAmount,
                        TaxAmount = convertPerGameResult.PerGameDetailLog.TaxAmount,
                        WinLossAmount = convertPerGameResult.PerGameDetailLog.UpdateAmount,
                        GameID = convertPerGameResult.PerGameDetailLog.GameID,
                        PlayerID = convertPerGameResult.PerGameDetailLog.PlayerID
                    });
                }

                AppendTimeDebug(debugTimeList, "Debug_Before_EventProcess");

                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");


                if (model.SettleExtraInfo != null && convertPerGameResult.PerGameDetailLog.JackpotAmount > 0)
                {
                    // step 7-2. 玩家事件紀錄
                    string behaviorText = $"JP: {convertPerGameResult.PerGameDetailLog.JackpotAmount} {convertPerGameResult.PerGameDetailLog.TransactionNo}";
                    PlayerTagMappingRepository.CreatePlayerBehaviorEvent(queryExistBetRecord.PlayerID, behaviorText);
                }

                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");


                // step 8. 活動流程/派獎
                if (mustWritePerGame)
                {
                    var EventProcessService = IOCHelper.ResolveServiceByCode<IEventProcessService>();

                    var eventProcessParam = AppHelper.PerGameConvertToEventProcessModel(convertPerGame, queryExistBetRecord.CreateTime);
                    EventProcessService.WhenSettlement(eventProcessParam);
                }

                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");


                // step 9. 部分遊戲商有自己獨立的額外邏輯處理
                ExtraProcessByGameVendor(vendorID, convertPerGame);

                DateTime min_time = debugTimeList.Select(x => x.Time).Min();
                DateTime max_time = debugTimeList.Select(x => x.Time).Max();

                //如果花費太多時間 記錄一些debug資訊
                if ((max_time - min_time).TotalSeconds > 6)
                {
                    var debug_time_list = debugTimeList.Select(z => z.Time.ToString("HH:mm:ss")).ToList();
                    string debug_time_text = $"time: {string.Join(",", debug_time_list)} ";

                    BetRecordExtraInfoRepository.CreateOrUpdateAsync(requiredInfo.BetRecordNoPGS, debug_time_text);
                }

                debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");

                //2025/06 應該不需要了
                //HeaderValueHelper.AppendDebugInfo(debugText);

                return new SingleWalletVendorCustomizedResponseConvertModel
                {
                    CommonInfo = new GameVendorBetCommonInfo
                    {
                        Currency = queryMerchant.Currency,
                        BeforeGamePoint = beforeGamePoint,
                        AfterGamePoint = afterGamePoint,
                        BetRecordNo = queryExistBetRecord.BetRecordNo,
                        BetRecordNoPGS = queryExistBetRecord.BetRecordNoPGS,
                        GameID = queryGame.GameID,
                        WalletUpdateAmount = walletUpdateAmount_payout,
                        ThirdSettleDate = convertPerGame.ThirdSettleDate,
                    },
                };

            }
            catch (TerproException ex)
            {
                //需要debug一些問題
                if (SettingHelper.IsMSTest)
                {
                    string msg = $"{RuntimeHelper.GetK8sInstanceName()} PGS單號:{queryExistBetRecord.BetRecordNoPGS} , ⚠️SettleBet 出錯了。原因為： {ex.GetMessageAndErrorLine()} ";

                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                    {
                        Channel = SlackChannelEnum.n_error_important,
                        Message = msg,
                    });
                }

                throw;
            }
            catch (Exception ex)
            {
                var tagID = SystemTagEnum.BetRecord_SettleProcess_NotCompleted;

                string msg = $"{RuntimeHelper.GetK8sInstanceName()} PGS單號:{queryExistBetRecord.BetRecordNoPGS} , ⚠️SettleBet 出錯了。是否需要補救資料(perGame?) 或 手動捕錢、可參考TagID: {(int)tagID} , {ex.GetMessageAndErrorLine()} ";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_error_important,
                    Message = msg,
                });

                //寫入紀錄 (可能要人工處理)
                Guid uuid = Guid.NewGuid();

                BetRecordTagMappingRepository.CreateOrQueueRetry(queryExistBetRecord.BetRecordNoPGS, tagID, uuid);

                EazyTempValueRepository.CreateOrQueueRetry(new EazyTempValueEntity
                {
                    BetRecordNoPGS = queryExistBetRecord.BetRecordNoPGS,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    Type1 = tagID.ToInteger(),
                    Amount1 = walletUpdateAmount_payout,
                    MerchantID = queryMerchant.MerchantID,
                    GameID = queryExistBetRecord?.GameID,
                    PlayerID = queryExistBetRecord?.PlayerID,
                    UUID = uuid,
                    Memo1 = ex.GetMessageAndErrorLine()
                });

                throw;
            }

        }

        private static void SetJiliTempHeader<T>(string sessionToken, T queryExistBetRecord, MerchantValidateInfoCacheModel queryMerchant) where T : IBetRecordBasicInfo
        {
            decimal afterAmount = 0;

            if (queryExistBetRecord.AmountBeforeBet.HasValue)
            {
                afterAmount = queryExistBetRecord.AmountBeforeBet.Value - queryExistBetRecord.BetAmount + queryExistBetRecord.PayoutAmount;
            }

            var jiliResponse = new JiliBetAPIResponseModel
            {
                Balance = afterAmount,
                Currency = queryMerchant.Currency,
                Token = [REDACTED],
                Username = queryExistBetRecord.PlayerID.ToString(),
            };

            HeaderValueHelper.AppendResultJsonWhenException(jiliResponse.ConvertToJsonString());
        }

        private static void ThrowExceptionBy_GPILogic(GameVendorSettleCallbackParameter model, BetRecordBriefInfoModel queryExistBetRecord)
        {
            if (model.PayoutGamePoint == 0 && queryExistBetRecord.Status == (int)BetRecordStatusEnum.SettleComplete)
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadySettlement);
            }
        }


        /// <summary>
        /// RG/SM 會有 complete=0 的結算 (FG裡面的每一筆小注單)
        /// </summary>
        private void ExtraProcessForNoneCompletedSettle(GameVendorSettleCallbackParameter model, BetRecordBriefInfoModel queryExistBetRecord)
        {
            if (model.SettleExtraInfo.RGGameResultID.IsNotEmptyTextNotNull() && model.SettleExtraInfo.IsWriteRGDetailItem)
            {
                var rep = IOCHelper.InstanceRepository<RGGameResultDetailRepository>();

                rep.CreateTryCatch(new RGGameResultDetailEntity
                {
                    BetRecordNoPGS = queryExistBetRecord.BetRecordNoPGS,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    GameResultID = model.SettleExtraInfo?.RGGameResultID,
                    PartitionTime = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(queryExistBetRecord.BetRecordNoPGS).Time.Value
                });
            }

            if (model.SettleExtraInfo.SMTransactionID.IsNotEmptyTextNotNull())
            {
                var rep = IOCHelper.InstanceRepository<WagerTransactionMappingVPlusRepository>();

                var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(queryExistBetRecord.BetRecordNoPGS);

                var insertEntity = new WagerTransactionMappingVPlusEntity
                {
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    BetRecordNoPGS = queryExistBetRecord.BetRecordNoPGS,
                    TransactionID = model.SettleExtraInfo.SMTransactionID,
                    IsComplete = model.SettleExtraInfo.MultipleSettleIsComplete,
                    Amount = model.SettleExtraInfo.MultipleSettlePayoutAmount,
                };

                rep.CreateTryCatch(insertEntity);
            }

            if (model.SettleExtraInfo.FumaTransactionID.IsNotEmptyTextNotNull())
            {
                var rep = IOCHelper.InstanceRepository<WagerTransactionMappingFumaRepository>();

                var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(queryExistBetRecord.BetRecordNoPGS);

                var insertEntity = new WagerTransactionMappingFumaEntity
                {
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    BetRecordNoPGS = queryExistBetRecord.BetRecordNoPGS,
                    TransactionID = model.SettleExtraInfo.FumaTransactionID,
                    IsComplete = model.SettleExtraInfo.MultipleSettleIsComplete,
                    Amount = model.SettleExtraInfo.MultipleSettlePayoutAmount,
                };

                rep.CreateTryCatch(insertEntity);
            }
        }



        private void CheckExtraTagToAdd(GameVendorSettleCallbackParameter param, ConvertPerGameLogResultModel perGameResult)
        {
            perGameResult.ExtraAddTags = perGameResult.ExtraAddTags ?? new List<SystemTagEnum>();

            CheckAnyTagToAdd(perGameResult);

            //標記 freeGame標籤
            if (param.SettleExtraInfo != null)
            {
                if (param.SettleExtraInfo.IsFreeGame)
                {
                    perGameResult.ExtraAddTags.Add(SystemTagEnum.PerGameDetailLog_FreeGame);
                }

                if (param.SettleExtraInfo.IsBuyFeature)
                {
                    perGameResult.ExtraAddTags.Add(SystemTagEnum.PerGameDetailLog_AGGBuyFeature);
                }
            }
        }


        public SingleWalletVendorCustomizedResponseConvertModel CancelBet(GameVendorTypeEnum vendorID, SingleWalletCancelBetRequestParameter model)
        {
            IGameVendorIntegrationService vendorService = ServiceInstanceHelper.InstanceVendorService(vendorID);

            //log記錄用
            HeaderValueHelper.AppendBetRecordNo(model.BetRecordNo);
            HeaderValueHelper.AppendReferOrderNo(model.BetRecordNo);

            var queryFilter = new BetRecordInfoQueryParameter
            {
                BetRecordNo = model.BetRecordNo,
                BetRecordNoPGS = model.ExtraInfo?.BetReocrdNoPGS,
                ThirdPartyGameCode = model?.GameCode,
                GameID = model.ExtraInfo?.GameID,
                IsQueryRecentRange = true,
                MaxQueryHourRange = DetermineMaxQueryHourRange(vendorID),
                DebugSourceMethod = "CancelBet"
            };

            //有指定的話 這邊查詢成本會降低些
            if (model.ExtraInfo != null && model.ExtraInfo.MaxHourRange.HasValue)
            {
                queryFilter.MaxQueryHourRange = model.ExtraInfo.MaxHourRange.Value;
            }

            //嘗試查出gameID 應該可以幫助一些查詢效能
            if (queryFilter.GameID == null && !string.IsNullOrWhiteSpace(queryFilter.ThirdPartyGameCode))
            {
                var queryGame = GameListRepository.GetGameCacheByVendorGameCode((int)vendorID, queryFilter.ThirdPartyGameCode);

                if (queryGame != null)
                {
                    queryFilter.GameID = queryGame.GameID;
                }
            }

            SetGameIdToFilter_ForCancel(vendorService, model.GameCode, queryFilter);

            SetPgsTransactionNoToFilter(vendorID, model, queryFilter);

            var delayCheck = DelayFewSecondsBeforeQueryForCancelPrcoess(vendorID, queryFilter.BetRecordNo, queryFilter.GameID);

            //Step 1. 查詢注單
            var queryBetRecord = vendorService.GetByBetRecordNo(queryFilter);

            //部分遊戲商在cancel之前最好delay幾秒再查一次注單
            if (queryBetRecord == null && delayCheck.IsDelayQueryAgain)
            {
                Thread.Sleep(delayCheck.DelayMilliseconds);

                queryBetRecord = vendorService.GetByBetRecordNo(queryFilter);
            }

            if (queryBetRecord == null)
            {
                CancelNotFoundProcess(vendorID, model);

                var errorCode = ApiErrorCode.NotFoundBetRecord;

                //怕其他遊戲商改壞 姑且這樣處理吧
                if (vendorID == GameVendorTypeEnum.Galaxy ||
                   vendorID == GameVendorTypeEnum.AGG)
                {
                    errorCode = ApiErrorCode.NotFoundBetRecordWhenCancelProcess;
                }

                var exceptionParam = new ErrorDetailModel
                {
                    LogExtraInfo = $"{model.BetRecordNo}",
                };

                throw new TerproException(errorCode, exceptionParam);
            }

            HeaderValueHelper.AppendDebugInfoForException(queryBetRecord.BetRecordNoPGS);

            var gameVendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendorID);

            //Step 2-1  檢查是否需要延遲cancel (會delay個幾秒再繼續流程)
            DelayFewSecondsForCancelProcess(gameVendorDef, queryBetRecord);

            //Step 2-2. 檢查是否為加注型注單
            var checkRaiseGames = CheckIsRaiseBetGames(gameVendorDef, queryBetRecord.GameID, queryBetRecord.BetRecordNoPGS, model.RaiseTransactionNo);

            //Step 3. 更新狀態 (狀態檢查在裡面實作)
            if (!checkRaiseGames.IsRaiseRecord || checkRaiseGames.IsCancelFullBetRecord)
            {
                vendorService.CancelBet(queryBetRecord.BetRecordNoPGS, memo: "");
            }

            try
            {
                decimal refundAmount = queryBetRecord.BetAmount;

                //Step 3-2. 加注型注單的退款額外處理
                if (checkRaiseGames.IsRaiseRecord)
                {
                    refundAmount = checkRaiseGames.RaiseRefundAmount;

                    //如果要cancel整張單。就不做這種處理了。直接取消主表的狀態即可
                    if (!checkRaiseGames.IsCancelFullBetRecord)
                    {
                        //狀態設為取消
                        PlaceBetRaiseRecordRepository.UpdateRaiseRecordStatus(queryBetRecord.BetRecordNoPGS, model.RaiseTransactionNo, BetRecordRaiseBetStatusEnum.Cancel);
                    }
                }

                //Step 4 額外的string資訊 紀錄在子表中 (空間&索引考量)
                string debugInfo = $"to Cancel: {DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS()} , Current status:{queryBetRecord.Status}";
                //BetRecordExtraInfoRepository.CreateOrUpdateAsync(queryBetRecord.BetRecordNoPGS, debugInfo);

                UpdateBalanceSPResult updateResult = null;

                var queryMerchant = GetMerchantValidateInfoByMemoryCache(queryBetRecord.MerchantID, vendorID);

                // Step 5. 更新錢包金幣
                if (CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                {
                    // [單錢包模式] 請求三方API 告知取消注單
                    var result = SingleWalletCallbackNotifyProcessRefService.CancelProcessCallbackMerchantApi(new SingleWalletModeCancelApiParameter
                    {
                        MerchantID = queryBetRecord.MerchantID,
                        GameID = queryBetRecord.GameID,
                        PlayerID = queryBetRecord.PlayerID,
                        BetAmount = queryBetRecord.BetAmount,
                        TransactionNo = queryBetRecord.BetRecordNoPGS,
                    });

                    updateResult = new UpdateBalanceSPResult
                    {
                        AfterAmount = result.Balance,
                        Code = ApiErrorCode.Success
                    };
                }
                else
                {
                    // [轉帳模式] 退還金幣
                    updateResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryBetRecord.PlayerID, queryBetRecord.GameID, refundAmount, TransactionTypeEnum.BetRecord_SystemCancelRefundWallet, queryBetRecord.BetRecordNoPGS, "");
                }

                decimal convertRateWhenPlaceBet = queryBetRecord.ConvertRate;

                if (convertRateWhenPlaceBet <= 0)
                {
                    convertRateWhenPlaceBet = GetMerchantConvertRate(queryBetRecord.MerchantID, vendorID).ConvertRate;
                }

                decimal afterGamePoint = updateResult.AfterAmount * convertRateWhenPlaceBet;
                decimal beforeGamePoint = updateResult.BeforeAmount * convertRateWhenPlaceBet;

                var queryPlayer = PlayerRepository.GetBriefByCache(queryBetRecord.PlayerID);

                return new SingleWalletVendorCustomizedResponseConvertModel
                {
                    CommonInfo = new GameVendorBetCommonInfo
                    {
                        PlayerID = queryBetRecord.PlayerID,
                        Currency = queryMerchant.Currency,
                        BetRecordNo = queryBetRecord?.BetRecordNo,
                        BetRecordNoPGS = queryBetRecord?.BetRecordNoPGS,
                        BeforeGamePoint = beforeGamePoint,
                        AfterGamePoint = afterGamePoint,
                        IsSingleWallet = AppHelper.IsSingleWalletMode(queryMerchant.WalletMode),
                    }
                };
            }
            catch (Exception ex)
            {
                string msg = $" CancelBet 後半段發生錯誤了。 可能需要人工補救資料? {vendorID.ToString()} `{queryBetRecord.BetRecordNoPGS}` 單錢包商戶可能需手動發通知? {ex.GetMessageAndErrorLine()}";
                SendMessageToMultipleChannel(msg);

                throw;
            }
        }

        private void SetPgsTransactionNoToFilter(GameVendorTypeEnum vendorID, SingleWalletCancelBetRequestParameter model, BetRecordInfoQueryParameter queryFilter)
        {
            //如果已有pgs單號 不需處理此
            if (queryFilter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                return;

            var placeBetCache = GetBetRecordPlaceBetTempCache(vendorID, model.BetRecordNo, queryFilter.GameID ?? 0);

            //如果有cache 嘗試用PK單號查詢
            if (placeBetCache.IsExist)
            {
                queryFilter.BetRecordNoPGS = placeBetCache.BetRecordNoPGS;
                queryFilter.BetRecordNo = null;
            }
        }

        private void SetGameIdToFilter_ForCancel(IGameVendorIntegrationService serviceInstance, string thirdGameCode, BetRecordInfoQueryParameter queryFilter)
        {
            if (queryFilter.GameID.HasValue)
                return;

            var queryGame = QueryGameByCache(serviceInstance, thirdGameCode);

            if (queryGame != null)
            {
                queryFilter.GameID = queryGame.GameID;
            }
        }

        private RaiseBetCancelCheckResult CheckIsRaiseBetGames(GameVendorDefinitionConfigModel vendorDef, int gameID, string betRecordNoPGS, string raiseTransactionNo)
        {
            var checkResult = new RaiseBetCancelCheckResult();

            if (!vendorDef.IsRaiseProcessGameExist)
            {
                return checkResult;
            }

            if (string.IsNullOrWhiteSpace(raiseTransactionNo))
            {
                return checkResult;
            }

            //如果有支援加注型遊戲 (加注的cancel可能只cancel第二筆。但整局沒有退、還是照常結算)
            var gameSetting = GameListCustomizedSettingRepository.GetParseModelByCache(gameID, GameListCustomizedSettingKeyEnum.IsSupportRaiseProcessWhenPlaceBet);

            bool isSupportRaiseProcess = gameSetting.SettingValueOfBoolean;

            if (isSupportRaiseProcess)
            {
                var raiseList = PlaceBetRaiseRecordRepository.GetRaiseRecordsByPgsNo(betRecordNoPGS, isLockQuery: true);

                var match = raiseList.FirstOrDefault(x => x.RaiseTransactionNo == raiseTransactionNo);

                if (match == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundBetRecord);
                }

                if (match.Status != BetRecordRaiseBetStatusEnum.Accept.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel);
                }

                //檢查是否所有子單都被cancel了。
                bool isAllCancel = raiseList
                    .Where(x => x.RaiseTransactionNo != raiseTransactionNo)
                    .Where(x => x.Status == BetRecordRaiseBetStatusEnum.Accept.ToInteger())
                    .IsEmptyOrNull();

                checkResult.IsRaiseRecord = true;
                checkResult.RaiseRefundAmount = match.Amount;
                checkResult.IsCancelFullBetRecord = isAllCancel;
            }

            return checkResult;
        }


        private (bool IsDelayQueryAgain, int DelayMilliseconds) DelayFewSecondsBeforeQueryForCancelPrcoess(GameVendorTypeEnum gameVendor, string betRecordNo, int? gameID)
        {
            bool isDelayQuery = false;
            int delayMilliseconds = 0;

            //jili是投注/結算 二合一 因此如果太快返回notFound可能會有注單被帳務排除的問題
            if (gameVendor == GameVendorTypeEnum.Jili ||
                gameVendor == GameVendorTypeEnum.AT)
            {
                isDelayQuery = true;

                delayMilliseconds = 1000 * 5;

                //AT捕魚很煩... cancel不傳gameID 會導致這邊...
                if (gameVendor == GameVendorTypeEnum.AT)
                {
                    delayMilliseconds = 1000 * 10;

                    var cacheQuery = GetBetRecordPlaceBetTempCache(gameVendor, betRecordNo, gameID ?? 0);

                    if (cacheQuery.IsExist && cacheQuery.BetRecordNoPGS.IsNotEmptyNull())
                    {
                        delayMilliseconds = 1000 * 15;
                    }
                }
                else if (gameID.HasValue)
                {
                    var cacheQuery = GetBetRecordPlaceBetTempCache(gameVendor, betRecordNo, gameID.Value);

                    if (cacheQuery.IsExist && cacheQuery.BetRecordNoPGS.IsNotEmptyNull())
                    {
                        delayMilliseconds = 1000 * 15;
                    }
                }
            }

            return (isDelayQuery, delayMilliseconds);
        }


        private void DelayFewSecondsForCancelProcess(GameVendorDefinitionConfigModel gameVendorDef, BetRecordBriefInfoModel betRecord)
        {
            //如果遊戲商沒有二合一流程 不檢查此(避免浪費API回應時間)
            if (!gameVendorDef.HasBetAndSettleBundleProcess)
                return;

            //如果還處於未結算的狀態
            if (betRecord.Status == BetRecordStatusEnum.Pending.ToInteger())
            {
                var cacheKey = CacheKey.ExternalAPI_BetRecord_LockShortTermForCancelAPI(betRecord.BetRecordNoPGS);

                var cacheValue = CacheHelper.GetCacheValue<string>(cacheKey);

                if (!string.IsNullOrWhiteSpace(cacheValue))
                {
                    //如果檢查到有短效型cache。延遲10秒再進行cancel檢查
                    Thread.Sleep(1000 * 10);
                }
            }
        }


        /// <summary>
        /// 有部分遊戲商是 投注&結算直接合併在一個動作。
        /// </summary>
        public SingleWalletVendorCustomizedResponseConvertModel PlaceBetAndSettle(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model)
        {
            model.RuleFor(z => z.BetAmount).Rule_NotMinus();
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();
            model.RuleFor(z => z.ExtraInfo).Rule_ModelNotNull();

            var extraInfo = model.ExtraInfo;

            extraInfo.RuleFor(x => x.PayoutGamePoint).Rule_NotNull().Rule_NotMinus();

            int playerID = GetPlayerIdFromRequest(model);

            //step 1. 先建立注單(如果中途拋錯誤就會強制結束)
            var betResult = this.PlaceBet(vendorID, model);

            //step 2. 先建立一個短效型cache。時間內禁止被cancel
            AddShortTermLockCache(betResult);

            //step 3. 結算
            var settleResult = this.SettleBet(vendorID, new GameVendorSettleCallbackParameter
            {
                BetRecordNo = model.BetRecordNo,
                PlayerID = playerID,
                GameCode = model.GameCode,
                PayoutGamePoint = extraInfo.PayoutGamePoint.Value,
                SessionToken = [REDACTED],
                GameVendorRawRequestInfo = model.GameVendorRawRequestInfo,
                SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    IsFreeGame = model.ExtraInfo?.IsFreeGame_BetAndSettleBundle ?? false,
                    BetRecordNoPGS = betResult.CommonInfo.BetRecordNoPGS  //使用此欄位查詢效能較好
                }
            });

            return settleResult;
        }


        /// <summary>
        /// 純粹寫紀錄、不做結算行為
        /// </summary>
        public SingleWalletVendorCustomizedResponseConvertModel WriteBetBetRecordWithoutUpdateBalance(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model)
        {
            model.RuleFor(z => z.BetAmount).Rule_NotMinus();
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();
            model.RuleFor(z => z.ExtraInfo).Rule_ModelNotNull();

            var extraInfo = model.ExtraInfo;

            extraInfo.RuleFor(x => x.PayoutGamePoint).Rule_NotNull().Rule_NotMinus();

            int playerID = GetPlayerIdFromRequest(model);

            model.ExtraInfo = model.ExtraInfo ?? new SingleWalletPlaceBetExtraParameter();
            model.ExtraInfo.IsSkipUpdateBalance = true;

            //step 1. 先建立注單(如果中途拋錯誤就會強制結束)
            var betResult = this.PlaceBet(vendorID, model);

            //step 2. 先建立一個短效型cache。時間內禁止被cancel
            AddShortTermLockCache(betResult);

            var settleParam = new GameVendorSettleCallbackParameter
            {
                BetRecordNo = model.BetRecordNo,
                PlayerID = playerID,
                GameCode = model.GameCode,
                PayoutGamePoint = extraInfo.PayoutGamePoint.Value,
                SessionToken = [REDACTED],
                GameVendorRawRequestInfo = model.GameVendorRawRequestInfo,
                SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    IsSkipUpdateBalance = true,
                    BetRecordNoPGS = betResult.CommonInfo.BetRecordNoPGS
                }
            };

            //step 3. 結算
            var settleResult = this.SettleBet(vendorID, settleParam);
            return settleResult;
        }


        /// <summary>
        /// 修改 已結算的改單 (e.g. 改金額)
        /// </summary>
        public SingleWalletChangeSettleInfoResponseModel ChangeSettleAmount(GameVendorTypeEnum vendorID, SingleWalletChangeBetPayoutRequestParameter model)
        {
            model.RuleFor(z => z.NewSettleInfo).Rule_ModelNotNull();

            IGameVendorIntegrationService vendorService = ServiceInstanceHelper.InstanceVendorService(vendorID);

            //Step 1. 查詢注單
            var queryBetRecord = vendorService.GetByBetRecordNo(new BetRecordInfoQueryParameter
            {
                BetRecordNo = model.BetRecordNo,
                GameID = model.ExtraInfo?.GameID,
                ThirdPartyGameCode = model?.GameCode,
                IsQueryRecentRange = true
            });

            if (queryBetRecord == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundBetRecord, new ErrorDetailModel
                {
                    LogExtraInfo = $"{model.BetRecordNo}",
                });
            }

            var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(queryBetRecord.BetRecordNoPGS);

            if (queryPerGame == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);
            }

            SystemTagEnum settleChangedTag = SystemTagEnum.PerGameDetailLog_ChangeSettleAmount;

            var queryExistTag = PerGameDetailLogTagMappingRepository.GetByFilter(new QueryPerGameDetailLogTagMappingRequestViewModel
            {
                PerGameDetailLogIDs = queryPerGame.Id.ToListCollection(),
                TagIDs = settleChangedTag.ToInteger().ToListCollection(),
                GameTimeBegin = queryPerGame.GameTime,
                GameTimeEnd = queryPerGame.GameTime.AddMinutes(1),
                IsLockQuery = true
            });

            //tag已存在 禁止重複操作
            if (queryExistTag.IsNotEmptyNull())
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_InvalidBetRecordOperate);
            }

            var afterSettleInfo = model.NewSettleInfo;

            string ip = IPHelper.GetIP();

            decimal afterPayoutAmount = afterSettleInfo.PayoutAmount;

            if (afterSettleInfo.MinusPayoutAmount.HasValue)
            {
                afterPayoutAmount = queryPerGame.PayoutAmount + afterSettleInfo.MinusPayoutAmount.Value;
            }

            decimal afterWinLossAmount = afterPayoutAmount - queryBetRecord.BetAmount;

            var before = new PerGameChangeSettleInfoParameter
            {
                BetAmount = queryPerGame.BetAmount,
                UpdateAmount = queryPerGame.UpdateAmount,
                PayoutAmount = queryPerGame.PayoutAmount,
                TaxAmount = queryPerGame.TaxAmount,
                ValidAmount = queryPerGame.ValidAmount,
            };

            var after = new PerGameChangeSettleInfoParameter
            {
                BetAmount = queryPerGame.BetAmount,
                UpdateAmount = afterWinLossAmount,
                PayoutAmount = afterPayoutAmount,
                TaxAmount = afterSettleInfo.TaxAmount,
                ValidAmount = afterSettleInfo.ValidAmount,
            };

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                var insertTag = PerGameDetailLogTagMappingRepository.InstanceInsertEntity(queryPerGame, settleChangedTag.ToInteger());

                // Step 1. 標記tag (如果重複標記就會拋錯誤)
                PerGameDetailLogTagMappingRepository.Create(insertTag);

                // Step 2. 標記tag
                BetRecordTagMappingRepository.CreateIfNotExist(queryBetRecord.BetRecordNoPGS, SystemTagEnum.BetRecord_ChangeSettleAmount);

                // Step 3. 更新perGame
                PerGameDetailLogRepository.ChangePerGameSettleAmount(queryPerGame.Id, after);

                // Step 4. 更新BetRecord
                BetRecordRepository.ChangeSettleAmount(new BetRecordChangeSettleInfoParameter
                {
                    GameVendorType = vendorID.ToInteger(),
                    TaxAmount = afterSettleInfo.TaxAmount,
                    ValidAmount = afterSettleInfo.ValidAmount,
                    PayoutAmount = afterPayoutAmount,
                    WinLossAmount = afterWinLossAmount,
                    BetRecordNoPGS = queryBetRecord.BetRecordNoPGS
                });

                // Step 5. 寫異動紀錄
                PerGameDetailChangeLogRepository.Create(new PerGameDetailChangeLogEntity
                {
                    TransactionNo = queryBetRecord.BetRecordNoPGS,
                    PerGameDetailLogID = queryPerGame.Id,
                    CreateBy = model.UpdateBy,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    IPAddress = ip,
                    MerchantID = queryPerGame.MerchantID,
                    BeforeJson = before.ConvertToJsonString(),
                    AfterJson = after.ConvertToJsonString(),
                });

                scope.Complete();
            }

            // ps. 後半段沒有包在transaction 因為會有跨DB錯誤
            var queryMerchant = MerchantRepository.GetById(queryPerGame.MerchantID);

            var isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);

            //如果 新的派彩金額-之前的派彩金額，還有未償還的
            decimal adjustPayoutAmount = (after.PayoutAmount - before.PayoutAmount);

            bool isAdjustBalanceRequired = adjustPayoutAmount != 0;

            string msg = @$"單號: {queryBetRecord.BetRecordNoPGS} , 觸發已結算注單改單流程, 預計會調整玩家錢包金額:{adjustPayoutAmount} , 
                            另外可能需要通知商戶方帳務紀錄要重撈異動。";

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_pgs_finance,
                Message = msg,
                SkipNotifyWhenUnitTest = true
            });


            if (!isAdjustBalanceRequired && queryBetRecord.BetRecordNoPGS != "58735dbb1075392d426d873b3a8cddbd")
            {
                return new SingleWalletChangeSettleInfoResponseModel();
            }

            //調整玩家錢包餘額
            if (isSingleWallet)
            {
                // 2-1 單錢包商戶
                if (!model.IsSkipSeamlessCallback)
                {
                    SingleWalletCallbackNotifyProcessRefService.ChangeSettleInfoProcessPassToMerchantApi(new SingleWalletChangeSettleInfoApiParameter
                    {
                        TransactionNo = queryBetRecord.BetRecordNoPGS,
                        DebugProcessUUID = ""
                    });
                }
            }
            else
            {
                var transactionType = TransactionTypeEnum.BetRecord_ChangeSettleInfo;

                // 2-2 轉帳模式，更新PGS自己的錢包金額
                if (adjustPayoutAmount > 0)
                {
                    PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPerGame.PlayerID, queryPerGame.GameID, adjustPayoutAmount, transactionType, queryPerGame.TransactionNo, ip);
                }
                else
                {
                    var currentWallet = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(queryPerGame.PlayerID, "");

                    if (currentWallet.Amount >= adjustPayoutAmount)
                    {
                        PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPerGame.PlayerID, queryPerGame.GameID, adjustPayoutAmount, transactionType, queryPerGame.TransactionNo, ip);
                    }
                    else
                    {
                        string msg2 = @$"單號: {queryBetRecord.BetRecordNoPGS} 注單結果修改, 預計要扣除玩家金幣、但是餘額不足無法扣除全部:{adjustPayoutAmount} , 餘額僅剩:{currentWallet.Amount}";
                        SendMessageToMultipleChannel(msg2);

                        if (currentWallet.Amount > 0)
                        {
                            PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPerGame.PlayerID, queryPerGame.GameID, currentWallet.Amount, transactionType, queryPerGame.TransactionNo, ip);
                        }
                    }
                }
            }

            string msg3 = @$"單號: {queryBetRecord.BetRecordNoPGS}
您好，此單已進行派獎金額修正，調整後金額為: {afterPayoutAmount}" + Environment.NewLine;

            if (isSingleWallet)
            {
                msg3 += "⚠️貴司為單錢包模式，系統已觸發改單流程callback (bet/settle-change)" + Environment.NewLine +
                        "請通知您的技術團隊，確認是否有收到調用通知";
            }

            SendMessageToMultipleChannel(msg3);

            return new SingleWalletChangeSettleInfoResponseModel
            {

            };
        }


        /// <summary>
        /// (內部使用) 補救遺失的投注資料 (建立betRecord資料，但是不進行扣款)
        /// </summary>
        public SingleWalletVendorCustomizedResponseConvertModel RecoverMissingBetRecord(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model)
        {
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();

            int playerID = GetPlayerIdFromRequest(model);

            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            IGameVendorIntegrationService vendorService = ServiceInstanceHelper.InstanceVendorService(vendorID);

            var transactionNoGenerate = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();
            string transactionNoPGS = transactionNoGenerate.TransactionNo;

            //需要經過 遊戲幣與法幣轉換
            decimal betPoint = model.BetAmount;
            decimal betAmount = ConvertGamePointToMoney(queryPlayer.MerchantID, vendorID, betPoint);

            //檢查遊戲存在/遊戲開啟
            var queryGame = GameListRepository.GetGameCacheByVendorGameCode(vendorID.ToInteger(), model.GameCode);

            // step 1. 寫入一筆單號對應Mapping
            BetRecordTransactionNoMappingRepository.Create(transactionNoPGS, model.BetRecordNo, vendorID.ToInteger());

            var requiredInfo = new InsertRawBetRecordRequiredInfoModel
            {
                GameID = queryGame.GameID,
                MerchantID = queryPlayer.MerchantID,
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                BetRecordNoPGS = transactionNoPGS,
                BetRecordNo = model.BetRecordNo,
                BetAmount = betAmount,
                BetGamePoint = betPoint,
                PartitionTime = transactionNoGenerate.TimeWithHour,
                ConvertRate = GetMerchantConvertRate(queryPlayer.MerchantID, vendorID).ConvertRate
            };

            // step 2. 寫入一筆投注紀錄 (raw betRecord)
            vendorService.WriteRawBetRecordWhenPlaceBet(requiredInfo, model.GameVendorRawRequestInfo);

            return new SingleWalletVendorCustomizedResponseConvertModel
            {
                CommonInfo = new GameVendorBetCommonInfo
                {

                }
            };
        }


        public GameVendorWalletTransactionResultModel GameVendorSendEventBonus(GameVendorTypeEnum vendorID, GameVendorWalletTransactionRequestParamter model)
        {
            return GameVendorSingleWalletCommonLogicProcessService.GameVendorSendEventBonus(vendorID, model);
        }


        private bool IsSupportZeroBet(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model)
        {
            if (model.ExtraInfo != null && model.ExtraInfo.IsSupportZeroBet)
                return true;

            return GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendorID).IsSupportZeroBetAmount;
        }


        private void DoRaiseBetProcess(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model, int playerID, int gameID, string pgsTransactionNo)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(pgsTransactionNo);

            DateTime now = DateTimeHelper.GetCurrentTime();

            //寫入一筆加注紀錄
            PlaceBetRaiseRecordRepository.Create(new PlaceBetRaiseRecordEntity
            {
                BetRecordNoPGS = pgsTransactionNo,
                RaiseTransactionNo = model.RaiseTransactionNo,
                Amount = model.BetAmount,
                CreateTime = now,
                UpdateTime = now,
                PlayerID = playerID,
                GameID = gameID,
                PartitionTime = recover.Time.Value,
                Status = BetRecordRaiseBetStatusEnum.Accept.ToInteger(),
            });
        }


        private UpdateBalanceSPResult UpdateWalletAmount_PlaceBetPrcoess(GameVendorPlaceBetCallbackRequestParameter model, PlayerBriefModel queryPlayer, MerchantValidateInfoCacheModel queryMerchant, int gameID, string transactionNoPGS, decimal betAmount)
        {
            UpdateBalanceSPResult updateResult = null;

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);
            bool isTransferMode = !isSingleWallet;

            int vendorID = AppHelper.GetGameVendorByGameID(gameID);

            var gameVendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendorID);

            //若有指定跳過錢包更新 (印象中是mario捕魚會有此)
            if (model.ExtraInfo.IsSkipUpdateBalance)
            {
                //2023/02/09 邏輯再次調整 (單錢包或轉帳 在此情況下都不會有placeBet流程)
                return new UpdateBalanceSPResult
                {
                    AfterAmount = -1,
                    BeforeAmount = -1
                };
            }

            //AGG虛寶卡
            if (model.ExtraInfo.ReferEventPrizeType != EventPrizeTypeEnum.Default)
            {
                return new UpdateBalanceSPResult
                {
                    AfterAmount = -1,
                    BeforeAmount = -1
                };
            }

            //部分遊戲商沒有傳sessionToken 查出最新一筆token (有多視窗的風險...)
            if (isSingleWallet)
            {
                if (string.IsNullOrWhiteSpace(model.SessionToken))
                {
                    model.SessionToken = [REDACTED];
                }
            }

            string pgsSessionToken = [REDACTED];

            model.ExtraInfo = model.ExtraInfo ?? new SingleWalletPlaceBetExtraParameter();

            #region 已廢棄邏輯

            //if (isSkipUpdateBalanceProcess && isSingleWallet)
            //{
            //    // 2-0-1 且強制有效單、0元注單
            //    var swResult = SingleWalletMerchantBetProcessRefService.PlaceBetProcessPassToMerchantApi(queryMerchant.MerchantID, new SingleWalletModePlaceBetParameter
            //    {
            //        PlayerID = queryPlayer.PlayerID,
            //        DebugProcessUUID = transactionNoPGS,
            //        SessionTokenPGS = pgsSessionToken,
            //        BetAmount = 0,
            //        GameID = queryGame.GameID,
            //        BetRecordNoPGS = transactionNoPGS,
            //        IsSkipUpdateProcess = true
            //    });

            //    decimal swAfterAmount = swResult.Balance;
            //    updateResult = new UpdateBalanceSPResult
            //    {
            //        AfterAmount = swAfterAmount,
            //        Code = ApiErrorCode.SuccessOK
            //    };
            //}

            #endregion
            bool isZeroBetAmountAndSkipProcess = betAmount == 0;

            //以clotPlay為例。仍然會觸發placeBet callback流程 (完全捨棄callback的 只有PGS舊版遊戲)
            if (isZeroBetAmountAndSkipProcess && gameVendorDef.IsSupportZeroBetAmountAndStillCallbackPlaceBet)
            {
                isZeroBetAmountAndSkipProcess = false; //false代表仍會觸發SW callback
            }

            if (isZeroBetAmountAndSkipProcess)
            {
                //0元注單 直接撈錢包金幣 (直接返回。部分遊戲商會接著進入settle環節
                var walletAmount = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(queryPlayer.PlayerID, pgsSessionToken);

                updateResult = new UpdateBalanceSPResult
                {
                    AfterAmount = walletAmount.Amount,
                    BeforeAmount = walletAmount.Amount
                };
            }
            else
            {
                if (isSingleWallet)
                {
                    //自從某個商戶要求。只好做此紀錄了 (2023/08/09)     
                    if (!string.IsNullOrWhiteSpace(pgsSessionToken))
                    {
                        var swToken = [REDACTED];
                        BetRecordCustomizedSettingSWSessionTokenRepository.Create(transactionNoPGS, swToken.SessionTokenRaw);
                    }

                    // 2-1-1 標準單錢包模式 請求對方API
                    var swResult = SingleWalletCallbackNotifyProcessRefService.PlaceBetProcessCallbackMerchantApi(queryMerchant.MerchantID, new SingleWalletModePlaceBetParameter
                    {
                        PlayerID = queryPlayer.PlayerID,
                        DebugProcessUUID = transactionNoPGS,
                        SessionTokenPGS = pgsSessionToken,
                        BetAmount = betAmount,
                        GameID = gameID,
                        BetRecordNoPGS = transactionNoPGS,
                        ExtraInfo = new PlaceBetCallbackExtraInfo
                        {
                            IsBuyFeature = model.ExtraInfo.IsAGGBuyFeature,
                            RoundID = model.ExtraInfo?.RoundID
                        }
                    });

                    decimal swAfterAmount = swResult.Balance;

                    updateResult = new UpdateBalanceSPResult
                    {
                        BeforeAmount = swAfterAmount + betAmount,
                        AfterAmount = swAfterAmount, //後面的程式碼會轉成gamePoint
                        Code = ApiErrorCode.SuccessOK
                    };
                }
                else
                {
                    // 2-1-9 轉帳模式，更新PGS自己的錢包金額
                    var transactionType = TransactionTypeEnum.SingleWallet_PlaceBetDebit;
                    decimal updateAmount = betAmount > 0 ? betAmount * -1 : betAmount;
                    updateResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPlayer.PlayerID, gameID, updateAmount, transactionType, transactionNoPGS, model.PlayerIPAddress);
                }
            }

            if (!updateResult.IsSuccess)
            {
                //如果是AT捕魚 額外紀錄debug資訊
                LogExtraInfoForATFish(model, queryPlayer, gameID, updateResult);

                throw new TerproException((ApiErrorCode)updateResult.Code);
            }

            return updateResult;
        }


        private void LogExtraInfoForATFish(GameVendorPlaceBetCallbackRequestParameter model, PlayerBriefModel queryPlayer, int gameID, UpdateBalanceSPResult updateResult)
        {
            if (AppHelper.GetGameVendorByGameID(gameID) == (int)GameVendorTypeEnum.AT)
            {
                var rawInfo = model.GameVendorRawRequestInfo?.AT?.PlaceBetInfo;

                if (rawInfo != null)
                {
                    EazyTempValueRepository.Create(new EazyTempValueEntity
                    {
                        Type1 = EazyTempValueTypeEnum.ATFishBetFail.ToInteger(),
                        PlayerID = queryPlayer.PlayerID,
                        GameID = gameID,
                        Amount1 = rawInfo.betAmount,
                        Amount2 = rawInfo.winloseAmount,
                        Memo1 = $"ErrorCode: {updateResult.Code.ToInteger()} {updateResult.Code.ToString()}"
                    });
                }
            }
        }

        private GameVendorRecordDuplicateCheckResult CheckDuplicateRecordAndException(GameVendorTypeEnum vendorID, GameVendorPlaceBetCallbackRequestParameter model, IGameVendorIntegrationService vendorService, GameListIdInfoBrief queryGame, int playerID, string pgsTransactionNo, MerchantValidateInfoCacheModel queryMerchant)
        {
            var result = new GameVendorRecordDuplicateCheckResult
            {
                IsSupportRaiseProcessGame = false
            };

            var gameVendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendorID);

            //IsSupportRaiseProcess 在注單不存在的情境也會使用到判斷。
            if (gameVendorDef.IsRaiseProcessGameExist)
            {
                var match = GameListCustomizedSettingRepository.GetParseModelByCache(queryGame.GameID, GameListCustomizedSettingKeyEnum.IsSupportRaiseProcessWhenPlaceBet);
                result.IsSupportRaiseProcessGame = match.SettingValueOfBoolean;
            }

            // 部分遊戲商效能考量 會用短效型cache來做簡易檢查。主要是防止重複鍵單、並且在不查詢db的前提下 (用三方單號查DB效能影響較大)
            if (gameVendorDef.IsCheckBetRecordNoByCacheWhenPlaceBet && !result.IsSupportRaiseProcessGame)
            {
                // 第一次reqeust 無條件通過、並建立一筆cache
                // 第二次request(偶爾會發生)。因檢查到有cache。因此判定此單已存在(步驟一跟步驟二不查詢DB、真的不幸誤判 也只是db unique exception)
                var cacheTemp = GetBetRecordPlaceBetTempCache(vendorID, model.BetRecordNo, queryGame.GameID);

                if (cacheTemp.IsExist)
                {
                    if (gameVendorDef.HasBetAndSettleBundleProcess)
                    {
                        var betRecordNoPGS = cacheTemp.BetRecordNoPGS;

                        Thread.Sleep(1000 * 1);

                        //這種寫法的隱憂 是不是可能越積越多retryThread 導致處理不完crash ...?
                        BetRecordGeneralColumnModel queryWager = null;

                        var retryParam = new RetryLoopParameter
                        {
                            MaxTimes = 3,
                            DelayMilliseconds = 1000 * 3,
                            IsAsync = false,
                        };

                        AppHelper.ExecuteMethodAndRetryMaxTimes(retryParam, (param) =>
                        {
                            queryWager = vendorService.GetByPGSTransactionNo(betRecordNoPGS);
                            return queryWager != null;
                        });

                        if (queryWager == null)
                        {
                            Thread.Sleep(1000 * 5); //故意讓API強制逾時看看 (反正發生案例不到很多 應該不會造成系統大量卡死)

                            string msg = $"單號: {betRecordNoPGS} {model.BetRecordNo}, {vendorID.ToString()} 查無資料與預期不符。若是JILI注單。可能需要標記帳務排除";

                            throw new TerproException(ApiErrorCode.NotFoundBetRecordWhenPlaceBetRetry, new ErrorDetailModel
                            {
                                IsAlwaysNotify = true,
                                LogExtraInfo = msg,
                            });
                        }
                        else if (queryWager.Status == BetRecordStatusEnum.SettleComplete.ToInteger())
                        {
                            var wallet = PlayerWalletRefService.GetWalletAmountByLatestToken(queryWager.PlayerID);
                            HeaderValueHelper.AppendAfterAmount(wallet.Amount);

                            if (vendorID == GameVendorTypeEnum.Jili)
                            {
                                SetJiliTempHeader(model.SessionToken, queryWager, queryMerchant);
                            }

                            throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadySettlement);
                        }
                        else
                        {
                            //特別檢查bet=0 是因為0元注單原則上是不允許cancel的 (通常是bonus注單)
                            if (queryWager.BetAmount == 0 && gameVendorDef.HasBetAndSettleBundleProcess && gameVendorDef.IsSupportZeroBetAmountAndStillCallbackPlaceBet)
                            {
                                result.IsWagerZeroBetAndWatingSettle = true;
                                result.ExistBetRecordNoPGS = queryWager.BetRecordNoPGS;
                            }
                            else if (vendorID == GameVendorTypeEnum.ClotPlay)
                            {
                                result.IsForceGoToSettleProcess = true;
                                result.ExistBetRecordNoPGS = queryWager.BetRecordNoPGS;
                            }
                            else
                            {
                                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo);
                            }
                        }
                    }
                    else
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo);
                    }
                }
                else
                {
                    BetRecordNoExistTemp_SetCacheValue(vendorID, model.BetRecordNo, queryGame.GameID, pgsTransactionNo);
                }
            }
            else
            {
                var queryBetRecord = vendorService.GetByBetRecordNo(new BetRecordInfoQueryParameter
                {
                    BetRecordNo = model.BetRecordNo,
                    GameID = queryGame.GameID,
                    ThirdPartyGameCode = queryGame.ThirdPartyGameCode,
                    IsQueryRecentRange = true,
                    IsPlaceBetProcess = true,
                    MaxQueryHourRange = SystemConstant.BetRecord_LoopQueryDefaultPartitionHoursRange,
                    DebugSourceMethod = "CheckDuplicateRecord"
                });

                //此處只有查詢精簡欄位 請注意
                //此處只有查詢精簡欄位 請注意
                //此處只有查詢精簡欄位 請注意
                if (queryBetRecord != null)
                {
                    result.ExistBetRecordNoPGS = queryBetRecord.BetRecordNoPGS;

                    if (vendorID == GameVendorTypeEnum.CrazyFishing)
                    {
                        //捕魚這家機制不夠完善 直接進入取消注單程序
                        FishingGameSecondTimesCancelProcess(model, queryBetRecord);
                    }
                    else if (queryBetRecord.BetAmount == 0 && gameVendorDef.HasBetAndSettleBundleProcess && gameVendorDef.IsSupportZeroBetAmountAndStillCallbackPlaceBet)
                    {
                        //特別檢查bet=0 是因為0元注單原則上是不允許cancel的 (通常是bonus注單)

                        result.IsWagerZeroBetAndWatingSettle = true;
                        result.ExistBetRecordNoPGS = queryBetRecord.BetRecordNoPGS;

                    }
                    else if (gameVendorDef.IsRaiseProcessGameExist)
                    {
                        //部分遊戲商允許加注流程。並且單號是同一個。處理方式是另一段程式碼
                        if (!result.IsSupportRaiseProcessGame)
                        {
                            //遊戲不支援加注流程 就拋錯誤
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo);
                        }

                        var isDataExist = PlaceBetRaiseRecordRepository.IsRecordExist(queryBetRecord.BetRecordNoPGS, model.RaiseTransactionNo);

                        if (isDataExist)
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo);
                        }

                        if (playerID != queryBetRecord.PlayerID)
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo, new ErrorDetailModel
                            {
                                ReplaceMessage = "查到了同一筆三方單號。但是playerID不匹配。代表目前的注單唯一條件有問題",
                                IsAlwaysNotify = true
                            });
                        }
                        else
                        {
                            result.IsExecuteRaiseProcess = true;
                        }
                    }
                    else
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo);
                    }
                }
            }

            return result;
        }


        private void CancelNotFoundProcess(GameVendorTypeEnum vendorID, SingleWalletCancelBetRequestParameter model)
        {
            //若有指定。忽略此流程
            if (model.ExtraInfo != null && model.ExtraInfo.IsIgnoreCancelNotFoundProcess)
                return;

            DateTime now = DateTimeHelper.GetCurrentTime();

            //標記tag
            BetRecordRawWagerTagMappingRepository.CreateTryCatch(new BetRecordRawWagerTagMappingEntity
            {
                BetRecordNo = model.BetRecordNo,
                GameVendorID = vendorID.ToInteger(),
                CreateTime = now,
                TagID = SystemTagEnum.BetRecord_Cancel_ButNotFound.ToInteger(),

            }, isNotifyWhenException: false);

            var jobType = OneTimesJobTypeEnum.BetRecord_CheckCancelNotFoundAgain;

            string uniqueKey = $"{model.BetRecordNo}_{jobType.ToInteger()}";

            var myObject = new OneTimeCancelNotFoundSettingJsonModel
            {
                BetRecordNo = model.BetRecordNo,
                GameCode = model.GameCode,
                GameVendorID = vendorID.ToInteger(),
            };

            var nextTime = now.AddMinutes(15);
            var insertEntity = OneTimesJobSettingRepository.InstanceInsertEntity(jobType, nextTime, null, false);
            insertEntity.SettingJson = myObject.ConvertToJsonString();
            insertEntity.UniqueKey = uniqueKey;

            //N分鐘後 透過排程再檢查一次是否還是notFound
            OneTimesJobSettingRepository.CreateTryCatch(insertEntity, isNotifyWhenException: false);
        }


        /// <summary>
        /// 如果檢查此注單已被標記取消 將自動觸發取消流程
        /// </summary>
        private void CheckCancelTagExistAndAutoRefund(GameVendorTypeEnum vendorID, string betRecordNo, string betRecordNoPGS, string gameCode, double spendMilliSeconds, int walletMode, string debugTimeText)
        {
            //如果前半段花太多時間，需要先檢查是否此單已被標記Cancel
            var queryCancelTag = BetRecordRawWagerTagMappingRepository.GetByFilter(new QueryBetRecordRawWagerTagMappingRequestViewModel
            {
                BetRecordNo = betRecordNo,
                TagID = SystemTagEnum.BetRecord_Cancel_ButNotFound.ToInteger(),
                GameVendorID = vendorID.ToInteger()

            }).FirstOrDefault();

            if (queryCancelTag == null)
                return;

            //單錢包商戶很常發生 就不特別send通知了
            if (!CommonLogicHelper.IsSingleWalletMode(walletMode))
            {
                string msg = $"遊戲商{vendorID.ToString()} , 單號:{betRecordNoPGS} 將觸發自動cancel單 , 花費時間:{spendMilliSeconds} , {debugTimeText} , k8s: {RuntimeHelper.GetK8sInstanceName()}  ";

                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_error_external_api,
                    Message = msg
                });
            }

            //自動觸發cancel流程
            CancelBet(vendorID, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = betRecordNo,
                GameCode = gameCode,
                ExtraInfo = new CancelExtraParameterModel
                {
                    BetReocrdNoPGS = betRecordNoPGS
                }
            });

            //並且強制拋錯誤 結束此流程
            throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel);
        }


        public long WritePerGameLog(ConvertPerGameLogResultModel convertPerGameResult, SettlementBetRecordInfoModel betRecordInfo)
        {
            var convertPerGame = convertPerGameResult.PerGameDetailLog;

            //step 1. 寫入perGame (2024/06 嘗試改成不寫perGame局號表)
            //if(SettingHelper.IsProduction)
            //{
            //    long perGameID = PerGameLogRepository.Create(new PerGameLogEntity
            //    {
            //        BeginTime = convertPerGame.GameTime,
            //        EndTime = convertPerGame.GameTime,
            //        GameData = "",
            //        ServerID = convertPerGame.ServerID,
            //        GameID = convertPerGame.GameID,
            //        TaxAmount = 0,
            //        CreateTime = DateTimeHelper.GetCurrentTime()
            //    });
            //    convertPerGame.PerGameLogID = perGameID;
            //}

            //step 2. 寫入perGameDetail
            long resultPerGameDetailID = PerGameDetailLogRepository.CreateTransactionScope(convertPerGame);
            convertPerGame.Id = resultPerGameDetailID;

            var tags = convertPerGameResult.ExtraAddTags;

            //step 3. 寫入tags
            if (tags.IsNotEmptyNull())
            {
                tags = tags.Distinct().ToList();

                var insertItems = tags.Select(z =>
                {
                    var instance = PerGameDetailLogTagMappingRepository.InstanceInsertEntity(convertPerGame, z.ToInteger());
                    return instance;
                })
                .ToList();

                try
                {
                    PerGameDetailLogTagMappingRepository.CreateMany(insertItems);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"WritePerGameLog 出現錯誤，{ex.GetMessageAndErrorLine()}";
                    SendMessageToMultipleChannel(errorMsg, TelegramChannelEnum.pgs_externalAPI);
                }
            }

            return resultPerGameDetailID;
        }

        public BetRecordGeneralColumnModel GetWagerBasicInfo(string transactionNo)
        {
            var query = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(transactionNo);

            if (query == null)
                return null;

            IGameVendorIntegrationService vendorService = ServiceInstanceHelper.InstanceVendorService(query.GameVendorID);

            var queryWager = vendorService.GetByPGSTransactionNo(transactionNo);

            return queryWager;
        }


        /// <summary>
        /// 不記帳的perGameLog
        /// </summary>
        public long WritePerGameLogAccountingIgnore(ConvertPerGameLogResultModel convertPerGameResult, SettlementBetRecordInfoModel betRecordInfo)
        {
            var convertPerGame = convertPerGameResult.PerGameDetailLog;

            //step 1. 寫入perGame
            //long perGameID = PerGameLogRepository.Create(new PerGameLogEntity
            //{
            //    BeginTime = convertPerGame.GameTime,
            //    EndTime = convertPerGame.GameTime,
            //    GameData = "",
            //    ServerID = convertPerGame.ServerID,
            //    GameID = convertPerGame.GameID,
            //    TaxAmount = 0,
            //    CreateTime = DateTimeHelper.GetCurrentTime()
            //});
            //convertPerGame.PerGameLogID = perGameID;

            var insertEntity = convertPerGame.ReturnInjecteObject<PerGameDetailLogAccountingIgnoreEntity>();

            //step 2. 寫入perGameDetail
            long resultPerGameDetailID = PerGameDetailLogAccountingIgnoreRepository.Create(insertEntity);
            return resultPerGameDetailID;
        }


        public List<SystemTagEnum> CheckAnyTagToAdd(ConvertPerGameLogResultModel convertPerGameResult)
        {
            convertPerGameResult.ExtraAddTags = convertPerGameResult.ExtraAddTags ?? new List<SystemTagEnum>();

            var newTags = CommonLogicHelper.CheckAnyTagToAdd(convertPerGameResult.PerGameDetailLog) ?? new List<SystemTagEnum>();

            convertPerGameResult.ExtraAddTags.AddRange(newTags);

            return convertPerGameResult.ExtraAddTags;
        }


        public string CheckRestrictModeAndGameMatch(SingleWalletPlaceBetExtraParameter extraInfo, int merchantID, int currentGameID)
        {
            if (extraInfo == null || extraInfo.TokenGameID <= 0)
                return "2";

            int tokenGameID = extraInfo.TokenGameID;

            if (tokenGameID == currentGameID)
                return "3";

            var tokenGameVendor = AppHelper.GetGameVendorByGameID(tokenGameID);
            var currentGameVendor = AppHelper.GetGameVendorByGameID(currentGameID);

            var errorDetail = new ErrorDetailModel
            {
                IsAlwaysNotify = false //2023-12 移除此通知 (不太重要了)
            };

            if (tokenGameVendor != currentGameVendor)
            {
                throw new TerproException(ApiErrorCode.Authorize_TokenAvailableGameNotMatch, errorDetail);
            }

            var isAllowCrossGame = GameVendorDefinitionHelper.IsAllowCrossGameWhenRestrictTokenMode(tokenGameVendor);

            if (!isAllowCrossGame)
            {
                throw new TerproException(ApiErrorCode.Authorize_TokenAvailableGameNotMatch, errorDetail);
            }

            return "9";
        }

        private void ExtraProcessWhenPlaceBetException(PlaceBetExceptionProcessExtraParameter myParam)
        {
            string machineName = RuntimeHelper.GetK8sInstanceName();

            bool isDelayTooMuchTime = myParam.IsDelayTooMuchTime;

            string betRecordNoPGS = myParam.TransactionNoPGS;
            string betRecordNo = myParam.CallbackParam.BetRecordNo;

            //手動補救的錯誤訊息 發到另個頻道(避免訊息太多被忽略了)
            string conditionMsg = isDelayTooMuchTime ? "注單花費太多時間，可能已自動Cancel單" : "後半段DB寫入錯誤、可能需補救補錢、請參考wiki文件 `【資料補救】裡面的SQL步驟 ";
            var targetChannel = isDelayTooMuchTime ? TelegramChannelEnum.pgs_externalAPI : TelegramChannelEnum.pgs_important_error;
            var type = isDelayTooMuchTime ? EazyTempValueTypeEnum.PlaceBetTooMuchTime : EazyTempValueTypeEnum.PlaceBetUnknowException;

            string msg = $" `{machineName}` {myParam.VendorID.ToString()} PlaceBet 錯誤。{conditionMsg}。{Environment.NewLine}⭐單號: {betRecordNo} {betRecordNoPGS} {myParam.QueryPlayer.MerchantID} PlayerID:{myParam.QueryPlayer.PlayerID}, {myParam.Exception.GetMessageAndErrorLine()}";

            bool isSendNotify = true;
            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(myParam.QueryMerchant.WalletMode);
            bool isDuplicateException = myParam.Exception.IsSqlUniqueException();

            //部分情況不發錯誤通知
            if (isDelayTooMuchTime && isSingleWallet)
            {
                isSendNotify = false;
            }
            else if (isDuplicateException)
            {
                isSendNotify = false;
            }

            //175030 單錢包內部使用 (Public API)
            if (isSendNotify || myParam.QueryMerchant.MerchantID == 175030)
            {
                SendMessageToMultipleChannel(msg, targetChannel);
            }

            var now = DateTimeHelper.GetCurrentTime();

            Guid uuid = Guid.NewGuid();

            var targetTagID = SystemTagEnum.BetRecord_PlaceBet_NotCompleted;

            //寫入紀錄 (可能要人工補救處理)
            if (isDelayTooMuchTime)
            {
                targetTagID = SystemTagEnum.BetRecord_PlaceBet_PlaceBetTooMuchTimeAndAutoCancel;
            }

            var eazyTemp = new EazyTempValueEntity
            {
                UUID = uuid,
                BetRecordNoPGS = betRecordNoPGS,
                BetRecordNo = betRecordNo,
                GameID = myParam.QueryGame.GameID,
                PlayerID = myParam.QueryPlayer.PlayerID,
                PlayerName = myParam.QueryPlayer.PlayerName,
                MerchantID = myParam.QueryPlayer.MerchantID,
                Amount1 = myParam.BetAmount,
                CreateTime = now,
                Memo1 = msg,
                Type1 = type.ToInteger(),
            };

            // Step 1. TagMapping表
            BetRecordTagMappingRepository.CreateOrQueueRetry(myParam.TransactionNoPGS, targetTagID, uuid);

            // Step 2. 簡易數值暫存表
            EazyTempValueRepository.CreateOrQueueRetry(eazyTemp);

            var oneTimeEntity = OneTimesJobSettingRepository.InstanceInsertEntity(OneTimesJobTypeEnum.BetRecord_RefundWagerPlacebetException, now.AddMinutes(15), null, false);

            // Step 3. OneTimeJob補救 先實驗性 只針對duplicate exception
            if (isDuplicateException)
            {
                var mySettingJson = new OneTimesEazyJsonModel
                {
                    ReferGuid = uuid,
                };

                oneTimeEntity.SettingJson = mySettingJson.ConvertToJsonString();
                OneTimesJobSettingRepository.CreateOrQueueRetry(oneTimeEntity);
            }
        }


        private void ExtraProcessByGameVendor(GameVendorTypeEnum gameVendor, PerGameDetailLogEntity perGameInfo)
        {
            switch (gameVendor)
            {
                //捕魚的詳細注單似乎不會那麼快同步資料
                case GameVendorTypeEnum.AGGFish:
                    {
                        //var vendorConfig = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
                        //{
                        //    MerchantIDs = perGameInfo.MerchantID.ToListCollection(),
                        //    VendorIDs = gameVendor.ToInteger().ToListCollection()
                        //})
                        //.FirstOrDefault();

                        //var recordInfo = new AGGFishBetRecordPgsRoundInfo
                        //{
                        //    BetRecordNoPGS = perGameInfo.TransactionNo,
                        //    GameID = perGameInfo.GameID,
                        //    MerchantID = perGameInfo.MerchantID,
                        //    PlayerID = perGameInfo.PlayerID,
                        //    PlayerName = perGameInfo.PlayerName,
                        //    SettleTime = perGameInfo.GameTime,
                        //    DataCorrectValidInfo = new AGGFishFetchDataCorrectValidInfo
                        //    {
                        //        TotalBetAmount = perGameInfo.ValidAmount,
                        //        TotalWinLoss = perGameInfo.UpdateAmount
                        //    }
                        //};

                        //var param = new AGGFishBetRecordApiRequestParameter
                        //{
                        //    StartTimeUTC = perGameInfo.BetTime.Value,
                        //    EndTimeUTC = perGameInfo.GameTime,
                        //    RoundID = perGameInfo.ThirdPartyBetRecordNo,
                        //    VendorConfigID = vendorConfig.VendorConfigID,
                        //};

                        //var service = new AGGFishIntegrationService();
                        //service.FetchBetRecordsAndInsertToDB(param, recordInfo);
                        break;
                    }
                default:
                    break;
            }
        }


        private void FishingGameSecondTimesCancelProcess(GameVendorPlaceBetCallbackRequestParameter model, BetRecordBriefInfoModel queryBetRecord)
        {
            if (queryBetRecord.Status == (int)BetRecordStatusEnum.Pending)
            {
                CancelBet(GameVendorTypeEnum.CrazyFishing, new SingleWalletCancelBetRequestParameter
                {
                    BetRecordNo = model.BetRecordNo,
                    GameCode = model.GameCode,
                });
            }

            throw new TerproException(ApiErrorCode.InvalidStatus);
        }


        private GameListIdInfoBrief QueryGameByCache(IGameVendorIntegrationService serviceInstance, string thirdGameCode, bool isCheckNotFound = true)
        {
            //這個return null邏輯不要改 (某幾家遊戲商在cancel流程不會提供gameCode資訊)
            if (string.IsNullOrWhiteSpace(thirdGameCode))
                return null;

            var queryGame = serviceInstance.GetGameByThirdGameCode(thirdGameCode);

            if (isCheckNotFound)
            {
                if (queryGame == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundGame);
                }

                if (queryGame.Status != NormalActiveStatus.Enable.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_GameStatus);
                }
            }

            return queryGame;
        }


        private PlaceBetProcessSystemMaintainCheckModel GetSystemMaintainInfo(GameVendorTypeEnum gameVendor)
        {
            int gameVendorID = gameVendor.ToInteger();

            string cacheKey = CacheKey.MemoryCache_SystemMaintainInfo(gameVendorID);

            var expiredTime = CacheHelper.InstanceMemoryCacheTime();

            var queryCache = CacheHelper.GetCacheOrQueryByMemoryCache(cacheKey, () =>
               {
                   var queryMaintain = VendorMaintenanceRepository.GetVendorMaintenanceByCache(gameVendorID);

                   bool isGlobalSystemMaintain = GetIsGlobalSystemMaintainFlag();

                   return new PlaceBetProcessSystemMaintainCheckModel
                   {
                       QueryVendorMaintenance = queryMaintain,
                       IsGameVendorUnderMaintain = false,
                       IsGlobalSystemMaintain = isGlobalSystemMaintain
                   };

               }, expiredTime);

            var vendorMaintain = queryCache.QueryVendorMaintenance;

            if (vendorMaintain != null)
            {
                var now = DateTimeHelper.GetCurrentTime();

                //cache可能是N分鐘前。而實際設置的EndTime已經結束了 因此要在外層再檢查一次
                if (vendorMaintain.StartTime <= now && now < vendorMaintain.EndTime)
                {
                    queryCache.IsGameVendorUnderMaintain = true;
                }
            }

            return queryCache;
        }


        private void CheckSystemMaintainOrSkipTag(GameVendorTypeEnum vendorID, MerchantValidateInfoCacheModel queryMerchant, int playerID)
        {
            var queryMaintainInfo = GetSystemMaintainInfo(vendorID);

            //內部測試商戶 可以跨過維護檢查
            if (!queryMerchant.IsInternalTestMerchant)
            {
                //檢查遊戲商維護
                if (queryMaintainInfo.IsGameVendorUnderMaintain)
                {
                    VendorMaintenanceRepository.CheckVendorMaintenanceAndSkipTagThenThrowException(vendorID.ToInteger(), playerID);
                }
                else
                {
                    //檢查全系統維護flag
                    if (queryMaintainInfo.IsGlobalSystemMaintain)
                    {
                        throw new TerproException(ApiErrorCode.SystemMaintain);
                    }
                }
            }
        }


        private void RecoverBetRecordData(GameVendorTypeEnum vendorID, GameVendorSettleCallbackParameter model, int gameID)
        {
            if (model.PlayerID <= 0)
            {
                string msg2 = $"{vendorID.ToString()} 單號:{model.BetRecordNo} , 想補救資料。但是playerID為0 無法進行";
                SendMessageToMultipleChannel(msg2);
            }
            else
            {
                var queryPlayer = PlayerRepository.GetById(model.PlayerID);

                string transactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

                var queryMerchantInfo = GetMerchantValidateInfoByMemoryCache(queryPlayer.MerchantID, vendorID);

                var BetRecordRecoverService = IOCHelper.ResolveServiceByCode<IBetRecordRecoverService>();

                BetRecordRecoverService.RecoverBetRecordAndInsertData(new RecoverBetRecordRequiredInfo
                {
                    BetAmount = 0,
                    BetRecordNoPGS = transactionNo,
                    BetRecordNo = model.BetRecordNo,
                    MerchantID = queryPlayer.MerchantID,
                    PlayerID = queryPlayer.PlayerID,
                    VendorConfigID = queryMerchantInfo.VendorConfigID,
                    GameID = gameID,
                });
            }
        }

        private void CreateBetRecordInSettleProcess(GameVendorTypeEnum vendorID, GameVendorSettleCallbackParameter model, int gameID)
        {
            // 如果是AGG捕魚 會有自動產生注單資料的機制
            if (IsWithoutPlacebetProcess(vendorID, model.PlayerID))
            {
                RecoverBetRecordData(vendorID, model, gameID);
            }
        }

        private bool IsWithoutPlacebetProcess(GameVendorTypeEnum vendorID, int playerID)
        {
            return GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendorID.ToInteger()).IsAutoCreateDataWhenSettleProcess;
        }


        private bool GetIsGlobalSystemMaintainFlag()
        {
            var settingKey = SystemConfigSettingEnum.SystemMaintain_IsSwtichToMaintain;

            string cacheKey = CacheKey.DbEntity_ParseModel_SystemConfigSetting(settingKey.ToInteger());

            TimeSpan cacheTime = new TimeSpan(0, 5, 0);

            var queryCache = CacheHelper.GetCacheOrQueryByMemoryCache(cacheKey, () =>
            {
                var query = SystemConfigSettingRepository.GetParseModelByEnum(settingKey);
                return query;

            }, cacheTime);

            //檢查系統維護
            if (SystemConfigSettingRepository.IsSystemMaintain(queryCache))
            {
                return true;
            }

            return false;
        }


        private void BetRecordNoExistTemp_SetCacheValue(GameVendorTypeEnum vendorID, string betRecordNo, int gameID, string pgsTransactionNo)
        {
            var cacheKey = CacheKey.ExternalAPI_BetRecordExistTempCache(vendorID, betRecordNo, gameID);

            //使用短效型的cache來取代 查詢DB
            RedisCache_2.SetCacheValue(cacheKey, pgsTransactionNo, new TimeSpan(0, 3, 0));
        }


        private string BetRecordNoExistTemp_GetCacheValue(GameVendorTypeEnum vendorID, string betRecordNo, int gameID)
        {
            //查詢短效型的cache 是否存在
            var cacheKey = CacheKey.ExternalAPI_BetRecordExistTempCache(vendorID, betRecordNo, gameID);

            return RedisCache_2.GetCacheValue<string>(cacheKey);
        }


        private (bool IsExist, string BetRecordNoPGS) GetBetRecordPlaceBetTempCache(GameVendorTypeEnum vendorID, string betRecordNo, int gameID)
        {
            //AT捕魚投注會提供gameID、Cancel不會提供gameID (會導致單號檢查有麻煩)
            if (AppHelper.GetGameVendorByGameID(gameID) == (int)GameVendorTypeEnum.AT)
            {
                gameID = 0;
            }

            //查詢短效型的cache 是否存在
            string queryCache_transactionNo = BetRecordNoExistTemp_GetCacheValue(vendorID, betRecordNo, gameID);

            if (string.IsNullOrWhiteSpace(queryCache_transactionNo))
            {
                return (false, "");
            }

            return (true, queryCache_transactionNo);
        }

        private void CheckPlaceBetForbiddenByGameVendorSpecialOperate(GameVendorPlaceBetCallbackRequestParameter model, MerchantValidateInfoCacheModel queryMerchant)
        {
            if (model.ExtraInfo != null && model.ExtraInfo.IsAGGBuyFeature)
            {
                var disableSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_DisableAGGBuyFeature);

                if (disableSetting.SettingValueOfInt == (int)NormalYesOrNoEnum.Yes)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_AGG_NotAllowBuyFeature);
                }
            }

            //IDG freespin
            if (model.ExtraInfo != null && model.ExtraInfo.ReferEventPrizeType == EventPrizeTypeEnum.AGG_Freespin)
            {
                var disableSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_DisableAGGFreespin);

                if (disableSetting.SettingValueOfInt == (int)NormalYesOrNoEnum.Yes)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_AGG_NotAllowBuyFeature);
                }
            }

            //單錢包不支持preserve模式
            if (model.ExtraInfo != null && model.ExtraInfo.IsJiliPreserveGame)
            {
                if (AppHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
                }
            }
        }

        private int DetermineMaxQueryHourRange(GameVendorTypeEnum vendorID)
        {
            int maxHourRange = 24 * 10;

            if (vendorID == GameVendorTypeEnum.AGG)
            {
                maxHourRange = 48;
            }
            else if (vendorID == GameVendorTypeEnum.SlotMania)
            {
                maxHourRange = 24 * 61;
            }

            return maxHourRange;
        }


        private decimal ConvertGamePointToMoney(int merchantID, GameVendorTypeEnum gameVendor, decimal point)
        {
            var settingKeyResult = GetMerchantConvertRate(merchantID, gameVendor);
            return ConvertGamePointToMoney(point, settingKeyResult.ConvertRate);
        }


        private void SetBetRecordNoPGS_IfCacheExist(GameVendorTypeEnum vendorID, GameVendorSettleCallbackParameter model, BetRecordInfoQueryParameter queryFilter)
        {
            //已有值 無需處理
            if (queryFilter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                return;

            //不支援cache機制 無需處理
            if (!GameVendorDefinitionHelper.IsCheckBetRecordNoByCacheWhenPlaceBet(vendorID))
                return;

            //AGG維持舊版作法 不要用這種
            if (vendorID == GameVendorTypeEnum.AGG)
                return;

            var cache = GetBetRecordPlaceBetTempCache(vendorID, model.BetRecordNo, queryFilter.GameID ?? 0);

            if (!cache.IsExist)
                return;

            queryFilter.BetRecordNoPGS = cache.BetRecordNoPGS;
        }


        private decimal? GetTotalBetAmountForRaiseBet(int gameID, string betRecordNoPGS)
        {
            var raiseSupportSetting = GameListCustomizedSettingRepository.GetParseModelByCache(gameID, GameListCustomizedSettingKeyEnum.IsSupportRaiseProcessWhenPlaceBet);

            bool isSupportRaise = raiseSupportSetting.SettingValueOfBoolean;

            if (!isSupportRaise)
                return null;

            var queryAcceptBet = PlaceBetRaiseRecordRepository.GetByFilter(new QueryPlaceBetRaiseRecordRequestViewModel
            {
                BetRecordNoPGS = betRecordNoPGS,
                Status = BetRecordRaiseBetStatusEnum.Accept.ToInteger()
            });

            if (queryAcceptBet.IsEmptyOrNull())
                return null;

            return queryAcceptBet.Select(x => x.Amount).Sum();
        }


        private decimal ConvertGamePointToMoney(decimal point, decimal convertRate)
        {
            return point / convertRate;
        }

        /// <summary>
        /// 加入一個短效型cache。如果此注單取消。要稍微延遲處理。避免太快進行cancel的判斷。有可能只是DB忙碌寫入比較慢而導致
        /// </summary>
        private void AddShortTermLockCache(SingleWalletVendorCustomizedResponseConvertModel placeBet)
        {
            string cacheKey = CacheKey.ExternalAPI_BetRecord_LockShortTermForCancelAPI(placeBet.CommonInfo?.BetRecordNoPGS);
            CacheHelper.SetCacheValue(cacheKey, "true", new TimeSpan(0, 0, 15));
        }


        private int GetPlayerIdFromRequest<T>(T model) where T : SingleWalletBasePlayerInfoModel
        {
            if (model.PlayerID != 0)
            {
                AddPlayerIDInfoToHeader(model.PlayerID);
                return model.PlayerID;
            }

            if (model.SessionToken.IsNotEmptyTextNotNull())
            {
                //ps. controller層有檢查過期了 (這邊請勿檢查過期)
                var queryToken = [REDACTED];
                int playerID = queryToken == null ? 0 : queryToken.PlayerID;

                AddPlayerIDInfoToHeader(playerID);
                return playerID;
            }

            //其他遊戲商可能要用playerName反查 (有其他遊戲商在根據狀況實作)
            return 0;
        }


        /// <summary>
        /// 透過此方式，ApiLog會獨立某個欄位記錄是哪個 playerID
        /// </summary>
        private void AddPlayerIDInfoToHeader(int playerID)
        {
            HeaderValueHelper.AppendLogInfo_IdentityKey(playerID.ToString());
        }


        private void AppendTimeDebug(List<TimeDebugModel> debugTimeList, string memo)
        {
            debugTimeList.Add(new TimeDebugModel(memo, DateTimeHelper.GetCurrentTime()));
        }



    }
}
