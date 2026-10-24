using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.IOC;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.BL.Services
{

    public partial class GameVendorSingleWalletCommonLogicProcessService : BaseGameVendorCallbackService, IGameVendorSingleWalletCommonLogicProcessService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public GameVendorEventBonusCallbackRecordRepository GameVendorEventBonusCallbackRecordRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        //這些Service要自主呼叫 InstanceAllService() 才會註冊
        public IPlayerWalletRefService PlayerWalletRefService { get; set; }

        public ISingleWalletCallbackNotifyProcessRefService SingleWalletMerchantBetProcessRefService { get; set; }


        public override bool IsAutoInstanceAllRepository
        {
            get
            {
                return false;
            }
        }

        public override bool IsAutoInstanceAllService
        {
            get
            {
                return false;
            }
        }


        public UpdateBalanceSPResult UpdateWalletAmountSettleProcess(GameVendorTypeEnum vendorID, SettleBalanceUpdateProcessConfig config, ConvertPerGameLogResultModel convertPerGameResult)
        {
            base.InstanceAllRepositoryAndService(true, true);

            var perGameTags = convertPerGameResult.ExtraAddTags ?? new List<SystemTagEnum>();

            UpdateBalanceSPResult updateResult;

            var convertPerGame = convertPerGameResult.PerGameDetailLog;

            //轉帳模式
            var transactionType = TransactionTypeEnum.SingleWallet_BetSettlement;

            var vendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendorID);

            //AGG虛寶卡 (假如拋錯誤就算了...?)
            if (config.ReferEventPrizeType != EventPrizeTypeEnum.Default)
            {
                var bonusResult = new UpdateBalanceSPResult();

                string prizeName = "AGG活動獎勵*";

                if (config.ReferEventPrizeType == EventPrizeTypeEnum.AGG_Freespin)
                {
                    prizeName = "AGG freespin";
                }
                else if (config.ReferEventPrizeType == EventPrizeTypeEnum.AGG_ItemCard)
                {
                    prizeName = "AGG虛寶卡";
                }

                try
                {
                    var resultBonus = GameVendorSendEventBonus(vendorID, new GameVendorWalletTransactionRequestParamter
                    {
                        SessionToken = "",
                        PlayerID = convertPerGame.PlayerID,
                        Amount = convertPerGame.PayoutAmount,
                        TransactionID = convertPerGame.ThirdPartyBetRecordNo,
                        TransactionType = TransactionTypeEnum.EventPrize,
                        Extra = new GameVendorWalletBonusExtraParamter
                        {
                            GameID = convertPerGame.GameID,
                            PgsTransactionNo = convertPerGame.TransactionNo
                        },
                        EventPrizeInfo = new GameVendorConvertToEventPrizeInfoModel
                        {
                            Amount = convertPerGame.PayoutAmount,
                            ThirdOrderNo = convertPerGame.ThirdPartyBetRecordNo,
                            BonusTypeName = prizeName,
                            EventPrizeType = config.ReferEventPrizeType.ToInteger(),
                            ReceiveTime = convertPerGame.GameTime,
                            ReferValidAmount = config.EventBonusDetail?.ReferValidAmount,
                            ReferThirdPartyEventID = config.EventBonusDetail?.ReferThirdPartyEventID
                        }
                    });

                    bonusResult.AfterAmount = resultBonus.AfterAmount;
                    bonusResult.BeforeAmount = resultBonus.BeforeAmount;
                }
                catch(Exception ex)
                {
                    string debug666 = "";
                }

                return bonusResult;
            }

            decimal walletUpdateAmount_payout = config.Wager_PayoutAmountWalletUpdate;

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(config.MerchantWalletMode);

            if (isSingleWallet)
            {
                //單錢包模式
                SingleWalletModeSettleApiResponseModel swResult = null;

                var extraInfo = new SettleBetCallbackExtraInfo
                {
                    RoundID = config.Wager_RoundID,
                };

                DataModelConvertHelper.SetExtraValueByPerGameTags(extraInfo, perGameTags);

                //請統一用這個寫法instance
                SingleWalletSettleCallbackParameter settleParam = DataModelConvertHelper.InstanceSingleWalletSettleCallbackParameter(convertPerGame, walletUpdateAmount_payout, config.RequestUUID, extraInfo);
                settleParam.SessionTokenPGS = config.SessionToken;

                //0元注單，如果符合幾個條件、要以bonus settle的方式通知對方 (此機制希望能逐漸淘汰)
                //三方遊戲至少包含  Jili (e.g. Crazy Pusher)、ClotPlay、某些遊戲商的捕魚?
                if (GameVendorExtraLogicHelper.IsExecuteBonusSettleProcess(vendorID, convertPerGame.BetAmount))
                {
                    if (vendorID != GameVendorTypeEnum.Jili)
                    {
                        SendMessageToMultipleChannel($"注單 {convertPerGame.TransactionNo} 投注額0元. 觸發 BonusProcessCallbackMerchantApi");
                    }

                    swResult = SingleWalletMerchantBetProcessRefService.BonusProcessCallbackMerchantApi(settleParam);
                }
                else
                {
                    swResult = SingleWalletMerchantBetProcessRefService.SettlemenetProcessCallbackMerchantApi(settleParam);
                }

                decimal swAfterAmount = swResult.Balance;

                updateResult = new UpdateBalanceSPResult
                {
                    BeforeAmount = swAfterAmount - settleParam.PayoutAmount,
                    AfterAmount = swAfterAmount, //後面的程式碼會轉成gamePoint
                    Code = ApiErrorCode.Success
                };
            }
            else
            {
                //轉帳模式可以直接不處理後續
                if (config.IsSkipUpdateBalance)
                {
                    return new UpdateBalanceSPResult
                    {
                        BeforeAmount = -1,
                        AfterAmount = -1
                    };
                }

                if (walletUpdateAmount_payout == 0)
                {
                    //派獎0元 直接撈當前錢包金幣 進行下一步驟
                    var walletAmount = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(convertPerGame.PlayerID, "", isLockQuery: false);

                    updateResult = new UpdateBalanceSPResult
                    {
                        AfterAmount = walletAmount.Amount,
                        BeforeAmount = walletAmount.Amount
                    };
                }
                else
                {
                    DelayRandomSecondsForAGGLive(vendorID, config.Wager_RoundID); //aggLive特有的delay邏輯 (很鳥的寫法)

                    updateResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(convertPerGame.PlayerID, convertPerGame.GameID, walletUpdateAmount_payout, transactionType, convertPerGame.TransactionNo, "");
                }
            }

            return updateResult;
        }



        public GameVendorWalletTransactionResultModel GameVendorSendEventBonus(GameVendorTypeEnum vendorID, GameVendorWalletTransactionRequestParamter model)
        {
            base.InstanceAllRepositoryAndService(true, true);

            model.RuleFor(x => x.TransactionID).Rule_NotEmptyText();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.EventPrizeInfo).Rule_ModelNotNull();

            if(!IsAllowZeroPrizeAmount(model?.EventPrizeInfo?.EventPrizeType))
            {
                model.RuleFor(x => x.Amount).Rule_NotZeroNotMinus().Rule_NotTooSmall(0.001M);
            }

            model.Extra = model.Extra ?? new GameVendorWalletBonusExtraParamter();

            HeaderValueHelper.AppendReferOrderNo(model.TransactionID);

            var queryPlayer = PlayerRepository.GetBriefByCache(model.PlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            //臨時補救處理方案
            var skipProcessCheck = IsAGGDelayTooMuchTime(vendorID, model);

            if (skipProcessCheck.IsSpcialProcess)
            {
                return skipProcessCheck.Result;
            }

            var queryMerchant = GetMerchantValidateInfoByMemoryCache(queryPlayer.MerchantID, vendorID);

            //ps. 這隻API暫時不檢查遊戲商維護 (維護期間也允許轉帳呼叫)
            //ps. 這隻API暫時不檢查遊戲商維護 (維護期間也允許轉帳呼叫)
            CheckIsFishTransferAPIMaintain();

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);

            var queryExistRecords = GameVendorEventBonusCallbackRecordRepository.GetByFilter(new QueryGameVendorEventBonusCallbackRecordRequestViewModel
            {
                GameVendorID = vendorID.ToInteger(),
                ThirdOrderNo = model.TransactionID,
            })
            .FirstOrDefault();

            GameListEntity queryGame = null;

            if (model.ThirdPartyGameCode.IsNotEmptyTextNotNull())
            {
                queryGame = GameListRepository.GetGameCacheByVendorGameCode(vendorID.ToInteger(), model.ThirdPartyGameCode);
            }
            else if (model.Extra.GameID.NotNullNotZeroNotMinus())
            {
                queryGame = GameListRepository.GetSingleByCache(model.Extra.GameID.Value);
            }

            if (queryGame != null)
            {
                model.EventPrizeInfo.GameID = queryGame.GameID;
            }

            var resultModel = new GameVendorWalletTransactionResultModel();

            //單錢包商戶、轉帳商戶的邏輯不同
            if (isSingleWallet)
            {
                //單錢包商戶的紀錄必須確保 PlatformOrderNo有值的才算成立。
                if (queryExistRecords != null && queryExistRecords.PlatformOrderNo.IsNotEmptyTextNotNull())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }

                //也許會有資料已建立。但是三方單號未填寫的案例
                if (queryExistRecords == null)
                {
                    string pgsTransactionNo = AssignTransactionNoOrGenerateNew(model.Extra?.PgsTransactionNo);

                    var insertModel = new GameVendorEventBonusCallbackRecordEntity
                    {
                        CreateTime = DateTimeHelper.GetCurrentTime(),
                        MerchantID = queryPlayer.MerchantID,
                        PlayerID = queryPlayer.PlayerID,
                        PlayerName = queryPlayer.PlayerName,
                        TransactionNo = pgsTransactionNo,
                        ThirdOrderNo = model.TransactionID,
                        Type = model.TransactionType.ToInteger(),
                        GameVendorID = vendorID.ToInteger(),
                        UpdateAmount = model.Amount,
                        Memo = model.Memo
                    };

                    GameVendorEventBonusCallbackRecordRepository.Create(insertModel);
                    queryExistRecords = insertModel;
                }

                string sessionTokenPGS = model.SessionToken;

                if (string.IsNullOrWhiteSpace(sessionTokenPGS))
                {
                    sessionTokenPGS = SingleWalletMerchantSessionTokenRepository.GetPlayerLastSessionToken(queryPlayer.PlayerID)?.SessionTokenPGS;
                }

                var apiParam = new SingleWalletTransferApiParameter
                {
                    SessionTokenPGS = sessionTokenPGS,
                    Amount = model.Amount,
                    PlayerID = queryPlayer.PlayerID,
                    TransactionNo = queryExistRecords.TransactionNo,
                    Memo = model.Memo.Truncate(50)
                };

                SingleWalletTransferApiPlatformResponse transferResult = null;

                //寫入 prizeRecord
                var prizeResult = CreateEventPrize(queryPlayer, model.EventPrizeInfo, queryExistRecords.TransactionNo);
                var modelConvert = ConvertEventPrizeRecordToViewModel(prizeResult.PrizeRecord , model.EventPrizeInfo.ReferThirdPartyEventID);

                //callback 通知請求
                transferResult = SingleWalletMerchantBetProcessRefService.SendEventPrizeCallbackMerchantApi(new SingleWalletCallbackSendEventBonusParameter
                {
                    Amount = model.Amount,
                    MerchantID = queryPlayer.MerchantID,
                    PlayerID = queryPlayer.PlayerID,
                    TransactionNo = queryExistRecords.TransactionNo,
                    EventPrizeInfo = modelConvert
                });

                if (transferResult == null)
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_SingleWalletCallbackFail, new ErrorDetailModel($"{queryExistRecords.TransactionNo}"));

                queryExistRecords.PlatformOrderNo = transferResult.MerchantOrderNo;
                queryExistRecords.AfterAmount = transferResult.AfterAmount;

                GameVendorEventBonusCallbackRecordRepository.Update(queryExistRecords);

                resultModel = new GameVendorWalletTransactionResultModel
                {
                    BeforeAmount = queryExistRecords.AfterAmount - model.Amount,
                    AfterAmount = queryExistRecords.AfterAmount,
                    PlayerID = queryPlayer.PlayerID,
                    PgsTransactionNo = queryExistRecords.TransactionNo
                };
            }
            else
            {
                string pgsOrderNo = queryExistRecords?.TransactionNo;

                if (string.IsNullOrWhiteSpace(pgsOrderNo))
                {
                    pgsOrderNo = AssignTransactionNoOrGenerateNew(model.Extra?.PgsTransactionNo);
                }

                CreateEventPrize(queryPlayer, model.EventPrizeInfo, pgsOrderNo);

                //一般轉帳模式
                if (queryExistRecords != null)
                {
                    if (model.Extra?.IsThrowDuplicateException == true)
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo);
                    }

                    resultModel = new GameVendorWalletTransactionResultModel
                    {
                        BeforeAmount = queryExistRecords.AfterAmount - queryExistRecords.UpdateAmount,
                        AfterAmount = queryExistRecords.AfterAmount,
                        PlayerID = queryPlayer.PlayerID,
                        PgsTransactionNo = queryExistRecords.TransactionNo
                    };
                }
                else
                {
                    //呼叫轉帳SP
                    var result = PlayerWalletRepository.SendEventBonusForGameVendor(new GameVendorWalletTransferParameterModel
                    {
                        PlayerID = model.PlayerID,
                        OrderNo_PGS = pgsOrderNo,
                        TranferType = model.TransactionType,
                        GameVendorID = vendorID.ToInteger(),
                        GameID = model.Extra?.GameID,
                        OrderNo_GameVendor = model.TransactionID,
                        Amount = model.Amount,
                    });

                    if (!result.IsSuccess)
                        throw new TerproException((ApiErrorCode)result.ErrorCode);

                    resultModel = new GameVendorWalletTransactionResultModel
                    {
                        BeforeAmount = result.BeforeAmount,
                        AfterAmount = result.AfterAmount,
                        PlayerID = queryPlayer.PlayerID,
                        PgsTransactionNo = pgsOrderNo
                    };
                }
            }

            SetGamePointWithConvertRate(resultModel, queryPlayer.MerchantID, vendorID);
            return resultModel;
        }


        private string AssignTransactionNoOrGenerateNew(string extraTransactionNo)
        {
            if (!string.IsNullOrWhiteSpace(extraTransactionNo))
            {
                return extraTransactionNo;
            }

            var pgsOrderNoResult = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();
            return pgsOrderNoResult.TransactionNo;
        }


        private void DelayRandomSecondsForAGGLive(GameVendorTypeEnum vendorID, string roundID)
        {
            //aggLive如果同一局有好幾筆注單結算。很容易SQL卡住...死結
            //因此判斷到有cache存在 就隨機delay幾秒
            //很廢的寫法...
            //也許可以改成如果是aggLive就使用sql之前google的方法? 至少不會影響到其他遊戲商 (ps. 我很懷疑那個寫法會變成單線程)
            if (vendorID == GameVendorTypeEnum.AGGLive && roundID.IsNotEmptyTextNotNull())
            {
                string cacheKey = CacheKey.ExternalAPI_BetRecordSettle_LockByRound(roundID);

                var cacheValue = CacheHelper.GetCacheValue<string>(cacheKey);

                if (cacheValue == null || string.IsNullOrWhiteSpace(cacheValue))
                {
                    CacheHelper.SetCacheValue(cacheKey, "OK", new TimeSpan(0, 3, 0));
                }
                else
                {
                    var rndWaitTime = RandomHelper.GetRandIntegerNumbers(500, 1500);

                    //隨機延遲一點時間(毫秒)
                    Thread.Sleep(rndWaitTime);
                }
            }
        }

        private EventPrizeRecordVendorApiViewModel ConvertEventPrizeRecordToViewModel(EventPrizeRecordEntity queryPrize , string thirdCampaignID)
        {
            if (queryPrize == null)
                throw new TerproException(ApiErrorCode.InvalidParameter_ModelIsNull, new ErrorDetailModel("queryPrize"));

            var convert = DataModelAndDbValueConvertHelper.ConvertEventPrizeRecordToViewModel(queryPrize);

            //eventName在這邊查 不要再改這邏輯了 (會影響到某個地方)
            var queryEvent = EventRepository.GetById(queryPrize.EventID);
            convert.EventName = DataModelAndDbValueConvertHelper.GetEventDisplayName(queryEvent);
            convert.ReferCampaignID = thirdCampaignID;

            return convert;
        }


        public (long EventPrizeRecordID, EventPrizeRecordEntity PrizeRecord) CreateEventPrize(PlayerBriefModel queryPlayer, GameVendorConvertToEventPrizeInfoModel eventInfo, string transactionNo)
        {
            //有傳物件才會進行此邏輯處理
            if (eventInfo == null)
                return (0, null);

            eventInfo.RuleFor(x => x.ThirdOrderNo).Rule_NotEmptyText();

            if(!IsAllowZeroPrizeAmount(eventInfo.EventPrizeType))
            {
                eventInfo.RuleFor(x => x.Amount).Rule_NotZero();
            }

            try
            {
                decimal amount = eventInfo.Amount.ToAbs();

                DateTime today_UTC8 = DateTimeHelper.GetCurrentTime().AddHours(8);
                DateTime receiveTime = eventInfo.ReceiveTime;

                var queryExistRecord = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                {
                    ThirdOrderNo = eventInfo.ThirdOrderNo,
                    MerchantIDs = queryPlayer.MerchantID.ToListCollection(),

                }).FirstOrDefault();

                //如果已經寫過資料了
                if (queryExistRecord != null)
                {
                    return (queryExistRecord.Id, queryExistRecord);
                }

                if (receiveTime <= DateTimeHelper.GetDefaultTime())
                {
                    receiveTime = DateTimeHelper.GetCurrentTime();
                }

                //如果跨日了。改成我方的時間
                if (receiveTime.AddHours(8).Date != today_UTC8.Date)
                {
                    receiveTime = DateTimeHelper.GetCurrentTime();
                }

                int targetEventSubType = EventSubTypeEnum.Other.ToInteger();
                var prizeType = EventPrizeTypeEnum.FishGame_GameBonus; //暫無其他分類

                if (eventInfo.EventPrizeType.NotNullNotZeroNotMinus())
                {
                    var myEventType = (EventPrizeTypeEnum)eventInfo.EventPrizeType.Value;
                    prizeType = myEventType;

                    switch (myEventType)
                    {
                        case EventPrizeTypeEnum.AGG_CashCard:
                        case EventPrizeTypeEnum.AGG_GiftCode:
                        case EventPrizeTypeEnum.AGG_BonusMission:
                        case EventPrizeTypeEnum.AGG_Ranking:
                        case EventPrizeTypeEnum.AGG_Raffle:
                        case EventPrizeTypeEnum.AGG_VIP:
                        case EventPrizeTypeEnum.AGG_OtherBonus:
                        case EventPrizeTypeEnum.AGG_ItemCard:
                        case EventPrizeTypeEnum.AGG_Freespin:
                            targetEventSubType = EventSubTypeEnum.AGGBonusOther.ToInteger();
                            break;

                        default:

                            //有可能不是AGG系列
                            //targetEventSubType = EventSubTypeEnum.AGGBonusOther.ToInteger();
                            break;
                    }
                }
                else if (eventInfo.IsFishGameBonus)
                {
                    prizeType = EventPrizeTypeEnum.FishGame_GameBonus;
                    targetEventSubType = EventSubTypeEnum.FishBonusPrize.ToInteger();
                }
                else if (eventInfo.IsOtherBonus)
                {
                    prizeType = EventPrizeTypeEnum.GameVendor_EventBonus;
                    targetEventSubType = EventSubTypeEnum.Other.ToInteger();
                }

                int targetEventID = 0;

                //部分獎勵會建立活動來對應
                var queryDefaultAssignEvent = EventRepository.GetByFilter(new QueryEventRequestViewModel
                {
                    EventTypes = EventTypeEnum.EmptyEvent.ToInteger().ToListCollection(),
                    EventSubTypes = targetEventSubType.ToListCollection(),

                }).FirstOrDefault();

                if (queryDefaultAssignEvent != null)
                {
                    targetEventID = queryDefaultAssignEvent.EventID;
                }

                var insert = new EventPrizeRecordEntity
                {
                    Status = EventPrizeRecordStatusEnum.AllocateSuccess.ToInteger(),
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    AmountOrQuantity = amount,
                    UpdateAmount = amount,
                    EventID = targetEventID,
                    EventPrizeBundleID = 0,
                    EventPrizeType = prizeType.ToInteger(),
                    EventPrizeItemID = 0,
                    GameID = eventInfo.GameID ?? 0,
                    MerchantID = queryPlayer.MerchantID,
                    PlayerName = queryPlayer.PlayerName,
                    ThirdOrderNo = eventInfo.ThirdOrderNo,
                    PlayerID = queryPlayer.PlayerID,
                    Memo = eventInfo.BonusTypeName.Truncate(100), //AGG三方會在這邊紀錄json
                    ReceiveTime = receiveTime,
                    ReferenceNo = transactionNo,
                    PgsTransactionNo = transactionNo
                };

                EventPrizeRecordDetailEntity detailInfo = null;

                if(insert.EventPrizeType == (int)EventPrizeTypeEnum.AGG_Freespin)
                {
                    detailInfo = new EventPrizeRecordDetailEntity
                    {
                        CreateTime = DateTimeHelper.GetCurrentTime(),
                        ReceiveTime = receiveTime,
                        ReferValidAmount = eventInfo.ReferValidAmount,
                        ReferThirdPartyEventID = eventInfo.ReferThirdPartyEventID
                    };                        
                }

                long resultID = EventPrizeRecordRepository.Create(insert , detailInfo);
                insert.Id = resultID;

                return (resultID, insert);
            }
            catch (Exception ex)
            {
                SendMessageToMultipleChannel($"CreateEventPrize 失敗 , {eventInfo.ThirdOrderNo} , {ex.Message}");

                if (SettingHelper.IsMSTest)
                {
                    throw;
                }
            }

            return (0, null);
        }


        /// <summary>
        /// 一個過渡期補救方法，針對符合的單 會採用特殊處理機制
        /// </summary>
        public (bool IsSpcialProcess, GameVendorWalletTransactionResultModel Result) IsAGGDelayTooMuchTime(GameVendorTypeEnum vendorID, GameVendorWalletTransactionRequestParamter model)
        {
            if (vendorID != GameVendorTypeEnum.AGG)
                return (false, null);

            if (model.EventPrizeInfo == null)
                return (false, null);

            DateTime currentTime = DateTimeHelper.GetCurrentTime();
            DateTime gameVendorTime = model.EventPrizeInfo.ReceiveTime;

            //判斷是上個月的資料
            bool isLastMonthData = gameVendorTime <= (new DateTime(2025, 12, 1)).AddHours(-8);

            if (!isLastMonthData)
                return (false, null);

            string tranNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime().TransactionNo;

            var result = new GameVendorWalletTransactionResultModel
            {
                BeforeAmount = 0,
                AfterAmount = model.Amount,
                AfterGamePoint = model.Amount,
                BeforeGamePoint = 0,
                PlayerID = model.PlayerID,
                PgsTransactionNo = tranNo
            };

            try
            {
                EazyTempValueRepository.Create(new EazyTempValueEntity
                {
                    UUID = Guid.NewGuid(),
                    Amount1 = model.Amount,
                    BetRecordNo = model.TransactionID,
                    BetRecordNoPGS = tranNo,
                    Memo1 = model.Memo.Truncate(50),
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    PlayerID = model.PlayerID,
                    DebugInfo = $"{gameVendorTime}",
                    Type1 = EazyTempValueTypeEnum.AGGBonusWinSkipProcess.ToInteger(),
                });

            }
            catch
            {

            }

            return (true, result);
        }

        private bool IsAllowZeroPrizeAmount(int? eventPrizeType)
        {
            if (eventPrizeType == null)
                return false;

            if (eventPrizeType == EventPrizeTypeEnum.AGG_Freespin.ToInteger())
                return true;

            return false;
        }


        private void SetGamePointWithConvertRate(GameVendorWalletTransactionResultModel model, int merchantID, GameVendorTypeEnum gameVendor)
        {
            var settingKeyResult = GetMerchantConvertRate(merchantID, gameVendor);
            model.BeforeGamePoint = ConvertMoneyToGamePoint(model.BeforeAmount, settingKeyResult.ConvertRate);
            model.AfterGamePoint = ConvertMoneyToGamePoint(model.AfterAmount, settingKeyResult.ConvertRate);
            return;
        }


        private void CheckIsFishTransferAPIMaintain()
        {
            var queryVendorAPIMaintain = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_IsFishingTranferAPIMaintain);

            if (queryVendorAPIMaintain != null &&
               queryVendorAPIMaintain.SettingValueOfInt == SystemMaintainStatusEnum.SystemUuderMaintenance.ToInteger())
            {
                throw new TerproException(ApiErrorCode.SystemMaintain);
            }
        }


    }

}