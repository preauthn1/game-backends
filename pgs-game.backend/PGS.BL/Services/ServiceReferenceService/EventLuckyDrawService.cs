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
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using Lck.Utility.Extensions.Helper;
using System.Transactions;
using target.Utility;
using target.Utility.Extensions.Web;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public class EventLuckyDrawService : BaseEventService, IEventLuckyDrawService
    {

        public PlayerRepository PlayerRepository { get; set; }

        public EventDrawPrizeItemRepository EventDrawPrizeItemRepository { get; set; }

        public EventDrawPrizeRepository EventDrawPrizeRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public ISingleWalletCallbackNotifyProcessRefService SingleWalletCallbackNotifyProcessRefService { get; set; }



        public LuckyDrawResultViewModel UseTicketDrawRandomPrize(int playerID, int eventID)
        {
            var drawResult = DrawRandomPrize(new LuckyDrawRequestModel
            {
                EventID = eventID,
                PlayerID = playerID,
                InitialStatus = EventPrizeRecordStatusEnum.AllocateSuccessReceiveSuccess,
                IsUseDrawTicket = true
            });

            return drawResult;
        }

        public LuckyDrawResultViewModel UseEventPointDrawRandomPrize(int playerID, int eventID)
        {
            var eventInfo = this.GetEventFullInformationByCache(eventID);

            var pointSetting = this.GetEventRuleSettingByType(eventInfo, EventRuleTypeEnum.ValueCompare_ConsumePointPerDraw);

            if (pointSetting == null || !pointSetting.Value.IsNumeric())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("ValueCompare_ConsumePointPerDraw"));

            var maxDrawTimesSetting = this.GetCustomizedSettingOrDefaultValue(eventInfo, EventCustomizedSettingKeyEnum.MaxDrawTimes);

            decimal pointPerDraw = decimal.Parse(pointSetting.Value);

            int maxDrawTimes = Int32.Parse(maxDrawTimesSetting);

            if (pointPerDraw <= 0)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("ValueCompare_ConsumePointPerDraw"));

            var drawResult = DrawRandomPrize(new LuckyDrawRequestModel
            {
                BetRecordNoPGS = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo(),
                ConsumeEventPointToDraw = pointPerDraw,
                MaxDrawTimes = maxDrawTimes,
                EventID = eventID,
                PlayerID = playerID,
                InitialStatus = EventPrizeRecordStatusEnum.AllocateSuccessReceiveSuccess,
            });

            return drawResult;
        }



        /// <summary>
        /// 如果是單錢包商戶 發送失敗也不會exception
        /// </summary>
        public LuckyDrawResultViewModel UpdateDrawPrizeStatusAndSendBalance(int playerID, int eventID)
        {
            EventPrizeRecordEntity firstTicket = null;

            try
            {
                using (var scope = AppHelper.InstanceTransactionScopeByLevel())
                {
                    var queryNotReceive = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                    {
                        IsLockQuery = false,
                        PageSize = 1,
                        EventIDs = eventID.ToListCollection(),
                        PlayerIDs = playerID.ToListCollection(),
                        PrizeTypes = EventDefinitionHelper.RandomDrawPrizeTypes(),
                        Status = EventPrizeRecordStatusEnum.AllocateSuccessNotReceive.ToInteger(),
                    });

                    if (queryNotReceive.IsEmptyOrNull())
                        throw new TerproException(ApiErrorCode.NotFoundTicketOrNotEnough);

                    //取出未領取的第一個獎項
                    firstTicket = queryNotReceive.OrderBy(z => z.CreateTime).FirstOrDefault();

                    //更新1. 將狀態變更為已領取
                    EventPrizeRecordRepository.UpdateStatusToReceiveSuccess(firstTicket.Id, isThrowException: true);

                    scope.Complete();
                }

                DateTime now = DateTimeHelper.GetCurrentTime();

                // 更新2. 金幣更新 (這段有跨DB 不適用C# transaction)
                var updateSPResult = SendEventBonusBalanceByWalletType(firstTicket.PlayerID, firstTicket.UpdateAmount.Value, firstTicket.Id.ToString() , firstTicket.Id);
               
                //清除cache
                CacheRemoveHelper.RemoveCache_WhenReceivePrize(playerID, eventID);

                var result = new LuckyDrawResultViewModel
                {
                    GameID = firstTicket.GameID,
                    GameName = DbMultiLanguageTextHelper.GetGameDisplayName(firstTicket.GameID),
                    PrizeRate = firstTicket.AmountOrQuantity,
                    DrawPrizeRecordID = firstTicket.Id,
                    PrizeAmount = firstTicket.UpdateAmount.Value,
                    ReferenceTransactionNo = firstTicket.ReferenceNo,
                    ReferenceBetAmount = firstTicket.ReferenceAmount.Value,
                    ReferenceTransactionTime = firstTicket.CreateTime,
                    ReferenceSendTime = now,
                    SwCallbackTransactionNo = firstTicket.PgsTransactionNo,

                    //其實這邊的before、after 沒有用在前端刷新金幣。(所以就算回傳0也不至於出現bug)
                    BeforeAmount = updateSPResult.BeforeAmount,
                    AfterAmount = updateSPResult.AfterAmount,
                };

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.GetCommonNotifyMessage("UpdateDrawPrizeStatusAndSendBalance", $"PlayerID: {playerID} , EventID:{eventID}. ");
                throw;
            }
        }
            

        public LuckyDrawResultViewModel DrawRandomPrize(LuckyDrawRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();

            int eventID = model.EventID;

            var eventInfo = this.GetEventFullInformationByCache(eventID);

            if (eventInfo == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            DateTime nowTime = DateTimeHelper.GetCurrentTime();

            if (!SettingHelper.IsMSTest && eventInfo.Status != NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus_EventDisable);

            if (eventInfo.BeginTime > nowTime || nowTime > eventInfo.EndTime)
                throw new TerproException(ApiErrorCode.InvalidStatus_EventDisable);

            var matchEventPrize = eventInfo.PrizeBundles
                .SelectMany(z => z.PrizesItems)
                .Where(z => EventDefinitionHelper.RandomDrawPrizeTypes().Contains(z.PrizeType))
                .FirstOrDefault();

            if (matchEventPrize == null)
                throw new TerproException(ApiErrorCode.NotFoundEventPrize);

            int prizeBundleID = matchEventPrize.EventPrizeItemID;
            int prizeItemID = matchEventPrize.EventPrizeItemID;

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var queryDrawPrize = EventDrawPrizeRepository.GetByFilter(new QueryEventDrawPrizeRequestViewModel
            {
                EventIDs = eventID.ToListCollection()

            }).FirstOrDefault();

            if (queryDrawPrize == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var drawResult = new LuckyDrawResultViewModel();

            var pgsTranactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            using (var scope = AppHelper.InstanceTransactionScopeByLevel(IsolationLevel.ReadCommitted))
            {
                List<EventDrawPrizeItemEntity> queryDrawItems = GetDrawPrizeItems(eventID);

                //包含權重增值的抽獎箱
                var prizeDrawBoxWithWeight = PutPrizeToDrawBox(queryDrawItems);

                if (prizeDrawBoxWithWeight.ItemsRange.IsEmptyOrNull())
                {
                    //標記獎品已發完
                    EventCustomizedSettingRepository.AddSettingTagIfNotExist(eventID, EventCustomizedSettingKeyEnum.Tag_PrizeEmpty);
                    scope.Complete();

                    //清除cacche
                    CacheRemoveHelper.RemoveCache_WhenUpdateEvent(eventID);

                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_EventPrizeEmpty);
                }

                //特殊類型1: 如果是消耗抽獎券的類型
                if (model.IsUseDrawTicket)
                {
                    var queryTickets = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                    {
                        IsLockQuery = false,
                        EventIDs = eventID.ToListCollection(),
                        PlayerIDs = queryPlayer.PlayerID.ToListCollection(),
                        PrizeTypes = EventPrizeTypeEnum.Ticket_RedEnvelope.ToInteger().ToListCollection(),
                        Status = EventPrizeRecordStatusEnum.AllocateSuccess.ToInteger(),
                    });

                    if (queryTickets.IsEmptyOrNull())
                        throw new TerproException(ApiErrorCode.NotFoundTicketOrNotEnough);

                    var firstTicket = queryTickets.OrderBy(z => z.CreateTime).FirstOrDefault();

                    // 更新1. 將抽獎券狀態變更為已使用
                    EventPrizeRecordRepository.UpdateStatusToUsage(firstTicket.Id, isThrowException: true);

                    //偷懶 (使用派發抽獎券的時間當作投注時間...可能會有幾秒誤差)
                    model.BetTime = firstTicket.CreateTime;
                    model.BetRecordNoPGS = firstTicket.ReferenceNo;
                    model.BetAmount = firstTicket.ReferenceAmount.HasValue ? firstTicket.ReferenceAmount.Value : 0;
                    model.GameID = firstTicket.GameID;
                }

                //特殊類型2: 消耗點數來進行抽獎
                if (model.ConsumeEventPointToDraw.NotNullNotZeroNotMinus())
                {
                    if (model.MaxDrawTimes <= 0)
                    {
                        model.MaxDrawTimes = 9999;
                    }

                    decimal maxUsagePoint = model.ConsumeEventPointToDraw.Value * model.MaxDrawTimes;
                    ConsumeEventPoint(model.PlayerID, model.EventID, 0, model.ConsumeEventPointToDraw.Value, maxUsagePoint);
                }

                //紅包類型的獎項 要檢查一些必填項目
                if (queryDrawPrize.Type == EventMultiplePrizeTypeEnum.SendBalanceByBetAmountRate.ToInteger())
                {
                    model.RuleFor(z => z.BetTime).Rule_NotEmptyDateTime();
                    model.RuleFor(z => z.BetAmount).Rule_NotZeroNotMinus();
                    model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
                }

                //從箱子抽一個出來
                int matchPrizeItemID = DrawRandomPrizeFromBox(prizeDrawBoxWithWeight);

                var targetPrize = queryDrawItems.FirstOrDefault(z => z.EventDrawPrizeItemID == matchPrizeItemID);

                // 更新2. 更新使用數量
                EventDrawPrizeItemRepository.UsageQuantityPlus(targetPrize.EventDrawPrizeItemID, isThrowException: true);

                bool isOverMaxValue = false;

                decimal calculateBetAmount = model.BetAmount;

                if (queryDrawPrize.MaxReferenceValue.HasValue)
                {
                    if (calculateBetAmount > queryDrawPrize.MaxReferenceValue.Value)
                    {
                        isOverMaxValue = true;
                        calculateBetAmount = queryDrawPrize.MaxReferenceValue.Value;
                    }
                }

                drawResult.ReferenceBetAmount = calculateBetAmount; //顯示max投注金額 減少使用者來申訴
                string memoInfo = string.Empty;


                //發彩金有分成 倍率/直接送現金
                if (queryDrawPrize.Type == EventMultiplePrizeTypeEnum.SendBalanceByBetAmountRate.ToInteger())
                {
                    string extraMemo = isOverMaxValue ? "*maxBetAmount" : "";
                    decimal targetRate = targetPrize.AmountOrQuantity;
                    drawResult.PrizeAmount = calculateBetAmount * targetRate;
                    drawResult.PrizeRate = targetRate;

                    memoInfo = $" {calculateBetAmount} x {targetRate} = {Math.Round(drawResult.PrizeAmount, 2)} {extraMemo}";
                }
                else
                {
                    drawResult.PrizeAmount = targetPrize.AmountOrQuantity;
                }

                DateTime now = DateTimeHelper.GetCurrentTime();

                drawResult.ItemDetail = new PrizeDrawResultItemDetailModel
                {
                    EventDrawPrizeItemID = targetPrize.EventDrawPrizeItemID,
                    Amount = drawResult.PrizeAmount
                };

                drawResult.ReferenceSendTime = now;

                var insertRecord = new EventPrizeRecordEntity
                {
                    Status = model.InitialStatus.ToInteger(),
                    UpdateAmount = drawResult.PrizeAmount,
                    AmountOrQuantity = targetPrize.AmountOrQuantity,
                    CreateTime = now,
                    EventID = eventInfo.EventID,
                    EventPrizeItemID = prizeItemID,
                    EventPrizeBundleID = prizeBundleID,
                    EventPrizeType = matchEventPrize.PrizeType,
                    GameID = model.GameID,
                    MerchantID = queryPlayer.MerchantID,
                    PlayerID = queryPlayer.PlayerID,
                    PlayerName = queryPlayer.PlayerName,
                    ReferenceNo = model.BetRecordNoPGS,
                    ReferenceAmount = calculateBetAmount,
                    Memo = memoInfo,
                    PgsTransactionNo = pgsTranactionNo
                };

                if (model.InitialStatus == EventPrizeRecordStatusEnum.AllocateSuccessReceiveSuccess)
                {
                    insertRecord.ReceiveTime = now;
                }

                // 更新3. 寫入一筆紀錄
                drawResult.DrawPrizeRecordID = EventPrizeRecordRepository.Create(insertRecord);

                scope.Complete();
            }


            if (model.InitialStatus == EventPrizeRecordStatusEnum.AllocateSuccessReceiveSuccess)
            {
                // 更新4. 金幣更新 (這段有跨DB 不適用C# transaction)
                var spResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPlayer.PlayerID, 0, drawResult.PrizeAmount, TransactionTypeEnum.EventPrize_RedEnvelope, drawResult.DrawPrizeRecordID.ToString(), "");

                drawResult.BeforeAmount = spResult.BeforeAmount;
                drawResult.AfterAmount = spResult.AfterAmount;
            }

            drawResult.GameID = model.GameID;
            drawResult.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(model.GameID);
            drawResult.ReferenceTransactionNo = model.BetRecordNoPGS;
            drawResult.ReferenceTransactionTime = model.BetTime;
            drawResult.SwCallbackTransactionNo = pgsTranactionNo;

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenReceivePrize(model.PlayerID, eventID);

            return drawResult;
        }


        public virtual int DrawRandomPrizeFromBox(EventRandomPrizeDrawBox prizeDrawBoxWithWeight)
        {
            //擲骰子
            long randomValue = RandomHelper.GetRandLongNumbers(0, prizeDrawBoxWithWeight.MaxValue);

            //取得骰子對應點數位置的獎品
            int matchPrizeItemID = prizeDrawBoxWithWeight.ItemsRange
                .Where(x => randomValue >= x.Begin && randomValue < x.End)
                .FirstOrDefault()
                .PrizeItemID;

            return matchPrizeItemID;
        }


        public virtual List<EventDrawPrizeItemEntity> GetDrawPrizeItems(int eventID)
        {
            return EventDrawPrizeItemRepository.GetByFilter(new QueryEventDrawPrizeItemRequestViewModel
            {
                IsLockQuery = true,
                EventIDs = eventID.ToListCollection(),
                IsQuantityStillEnough = true
            });
        }


        /// <summary>
        /// 將獎項放進 抽獎箱(包含權重的增值)
        /// </summary>
        public virtual EventRandomPrizeDrawBox PutPrizeToDrawBox(List<EventDrawPrizeItemEntity> queryDrawItems)
        {
            var prizeRangeResult = new EventRandomPrizeDrawBox
            {
                MaxValue = 0,
                ItemsRange = new List<EventRandomDrawPrizeRangeItemModel>()
            };

            //如果獎品都發完了就返回
            if (queryDrawItems.IsEmptyOrNull())
                return prizeRangeResult;

            if (queryDrawItems.All(x => x.LimitQuantity.HasValue && (x.LimitQuantity - x.UsageQuantity) <= 0))
            {
                return prizeRangeResult;
            }

            long beginValue = 0;
            long endValue = 0;

            foreach (var eachDrawPrize in queryDrawItems)
            {
                int remaningCount = 1;

                //若無限制獎品數量 -> 視為每個獎品各1個 機率則依照權重
                //若有限制獎品數量 -> 根據剩餘數量*權重 讓box塞滿獎品
                if (eachDrawPrize.LimitQuantity.HasValue)
                {
                    remaningCount = eachDrawPrize.LimitQuantity.Value - eachDrawPrize.UsageQuantity;

                    if (remaningCount <= 0)
                    {
                        //獎金以抽完
                        continue;
                    }
                }

                bool isWeightExist = eachDrawPrize.ProbabilityWeights.HasValue;

                if (isWeightExist)
                {
                    endValue = (long)(beginValue + remaningCount * eachDrawPrize.ProbabilityWeights.Value);
                }
                else
                {
                    endValue = beginValue + 1;
                }

                //設定每個獎品的亂數值區間 (根據weight決定)
                //e.g.  
                //1獎  0~10 
                //2獎  10~100
                //3獎  100~1000
                prizeRangeResult.ItemsRange.Add(new EventRandomDrawPrizeRangeItemModel
                {
                    Begin = beginValue,
                    End = endValue,
                    PrizeItemID = eachDrawPrize.EventDrawPrizeItemID
                });

                beginValue = endValue;
            }

            prizeRangeResult.MaxValue = prizeRangeResult.ItemsRange.Select(x => x.End).Max();
            return prizeRangeResult;
        }

        private UpdateBalanceSPResult SendEventBonusBalanceByWalletType(int playerID, decimal amount, string referenceNo, long eventPrizeRecordID)
        {
            var queryMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
                {
                    PlayerID = playerID
                })
                .FirstOrDefault();

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);

            if (isSingleWallet)
            {
                var queryEvnetPrize = EventPrizeRecordRepository.GetById(eventPrizeRecordID);

                var convertEventInfo = DataModelAndDbValueConvertHelper.ConvertEventPrizeRecordToViewModel(queryEvnetPrize);

                var apiResult = SingleWalletCallbackNotifyProcessRefService.SendEventPrizeCallbackMerchantApi(new game.Entity.Game.SingleWalletCallbackSendEventBonusParameter
                {
                    PlayerID = playerID,
                    MerchantID = queryMerchant.MerchantID,
                    Amount = amount,
                    EventPrizeInfo = convertEventInfo,
                    TransactionNo = queryEvnetPrize.PgsTransactionNo
                });

                if(apiResult == null)
                {
                    //SendMessageToMultipleChannel($"發送獎勵 {queryEvnetPrize.PgsTransactionNo} 單錢包callback失敗", TelegramChannelEnum.pgs_common_error);
                    return new UpdateBalanceSPResult();
                }

                return new UpdateBalanceSPResult
                {
                    BeforeAmount = apiResult.BeforeAmount,
                    AfterAmount = apiResult.AfterAmount
                };
            }
            else
            {
                // 更新2. 金幣更新 (這段有跨DB 不適用C# transaction)
                var updateSPResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(playerID, 0, amount, TransactionTypeEnum.EventPrize_RedEnvelope, referenceNo, "");

                if (updateSPResult == null)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail, new ErrorDetailModel
                    {
                        IsAlwaysNotify = true
                    });
                }
                              
                return updateSPResult;
            }
        }

    }

}
