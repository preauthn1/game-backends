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
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public class EventCommonPrizeSendService : BaseEventService, IEventCommonPrizeSendService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public EventDrawPrizeItemRepository EventDrawPrizeItemRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public ISingleWalletCallbackNotifyProcessRefService SingleWalletCallbackNotifyProcessRefService { get; set; }

        /// <summary>
        /// 發放排名活動獎勵 (PGS自製)
        /// </summary>
        public void SendRankEventPrize(int eventID, RankEventTriggerSendPrizeParameter param)
        {
            var queryEvent = GetEventFullInformation(eventID);

            //如果填寫null 幫忙查詢
            if (param.OrderBy == null)
            {
                var orderByTypeSetting = queryEvent.CustomizedSettings.FirstOrDefault(x => x.SettingKey == (int)EventCustomizedSettingKeyEnum.RankEventCompetiteType);

                if (orderByTypeSetting == null)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel
                    {
                        ColumnName = "RankEventCompetiteType",
                        IsAlwaysNotify = true
                    });
                }

                param.OrderBy = (EventCounterOrderByEnum)orderByTypeSetting.SettingValueOfInt;
            }

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            if (queryEvent.EventType != (int)EventTypeEnum.RankCompetition)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("EventType"));

            if (queryEvent.Status != NormalActiveStatus.Enable.ToInteger() && !SettingHelper.IsMSTest)
                throw new TerproException(ApiErrorCode.InvalidStatus);

            string lockerCacheKey = CacheKey.AdminSys_EventSendPrizeLocker(queryEvent.EventID);

            //設一個短期locker 避免重複執行
            CacheHelper.CheckShortCacheLockerExist(lockerCacheKey, 30);

            var queryPrizeEmptySetting = queryEvent.CustomizedSettings.GetBySettingKey(EventCustomizedSettingKeyEnum.Tag_PrizeEmpty);

            if (queryPrizeEmptySetting != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_EventPrizeEmpty);

            var queryPrizeSetting = queryEvent.PrizeBundles
                .SelectMany(z => z.PrizesItems)
                .Where(z => z.PrizeType == (int)EventPrizeTypeEnum.UseSelectedMultiplePrizeSetting_RankBetAmount)
                .FirstOrDefault();

            if (queryPrizeSetting == null)
                throw new TerproException(ApiErrorCode.NotFoundEventPrize, new ErrorDetailModel("1*"));

            //取得多重獎項設置
            List<EventDrawPrizeItemEntity> queryMultiplePrizes = QueryMultiplePrizeItems(queryEvent.EventID);

            if (queryMultiplePrizes.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundEventPrize, new ErrorDetailModel("2*"));

            var isAnySendRecord = queryMultiplePrizes.Any(z => z.UsageQuantity > 0);

            //已經有某部分的獎項送出了 (曾經執行過發放?) 不得再次呼叫此發放程式
            if (isAnySendRecord)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "已有一部份的派發紀錄，禁止執行發放邏輯");

            var boxItems = ConvertToPrizeBoxItem(queryMultiplePrizes);

            if (boxItems.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_EventPrizeEmpty);

            //取得獎品數量 (要發給前N名)
            int totalPrizeCount = boxItems.Count();

            //取出前N名
            List<EventBetCounterEntity> queryBetCounterByRanking = QueryEventCounter(queryEvent.EventID, totalPrizeCount, param.OrderBy.Value);

            //如果完全沒玩家參加
            if (queryBetCounterByRanking.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundAnyParticipant);

            int parentDrawPrizeID = queryMultiplePrizes.FirstOrDefault().EventDrawPrizeID;

            //獎品全部更新為已派發
            EventDrawPrizeItemRepository.UpdateAllPrizeToMaxUsage(parentDrawPrizeID);

            //標記獎品已發完
            EventCustomizedSettingRepository.Create(new EventCustomizedSettingEntity
            {
                EventID = queryEvent.EventID,
                SettingKey = EventCustomizedSettingKeyEnum.Tag_PrizeEmpty.ToInteger(),
                SettingValue = "1"
            });

            int rank = 0;

            Dictionary<int, EventMultplePrizeBoxItemModel> boxItemsDic = boxItems.ToDictionary(z => z.Sequence, x => x);

            DateTime now = DateTimeHelper.GetCurrentTime();

            List<int> playerIDs = queryBetCounterByRanking.Select(z => z.PlayerID).ToList();

            var queryPlayers = PlayerRepository.GetByIds(playerIDs);

            List<int> merchantIDs = queryPlayers.Select(x => x.MerchantID).Distinct().ToList();

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

            var targetChannel = TelegramChannelEnum.pgs_adminSys_api;

            SendMessageToMultipleChannel($"活動:{queryEvent.EventID} , 共{queryBetCounterByRanking.Count()} 準備開始派獎", targetChannel);

            foreach (var eachPlayer in queryBetCounterByRanking)
            {
                rank++;

                //根據名次派發獎勵
                var matchPrize = boxItemsDic[rank];

                var matchPlayer = queryPlayers.FirstOrDefault(z => z.PlayerID == eachPlayer.PlayerID);

                string orderNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

                try
                {
                    var matchMerchant = queryMerchants.FirstOrDefault(z => z.MerchantID == matchPlayer.MerchantID);

                    bool isSingleWallet = AppHelper.IsSingleWalletMode(matchMerchant.WalletMode);

                    var eventPrizeRecord = new EventPrizeRecordEntity
                    {
                        Status = EventPrizeRecordStatusEnum.AllocateSuccess.ToInteger(),
                        CreateTime = now,
                        ReceiveTime = now,
                        EventID = queryEvent.EventID,
                        EventPrizeType = queryPrizeSetting.PrizeType,
                        EventPrizeItemID = queryPrizeSetting.EventPrizeItemID,
                        EventPrizeBundleID = queryPrizeSetting.EventPrizeBundleID,
                        AmountOrQuantity = matchPrize.AmountOrQuantity,
                        UpdateAmount = matchPrize.AmountOrQuantity,
                        PlayerID = eachPlayer.PlayerID,
                        PlayerName = matchPlayer.PlayerName,
                        MerchantID = matchPlayer.MerchantID,
                        PgsTransactionNo = orderNo,
                        Memo = $"rank: {rank}"
                    };

                    // Step 1. 寫派發紀錄
                    long recordID = EventPrizeRecordRepository.Create(eventPrizeRecord);

                    decimal prizeAmount = matchPrize.AmountOrQuantity;

                    // Step 2. 金幣更新 (這段有跨DB 不適用C# transaction)
                    if (isSingleWallet)
                    {
                        var convertEventInfo = DataModelAndDbValueConvertHelper.ConvertEventPrizeRecordToViewModel(eventPrizeRecord);

                        var apiResult = SingleWalletCallbackNotifyProcessRefService.SendEventPrizeCallbackMerchantApi(new game.Entity.Game.SingleWalletCallbackSendEventBonusParameter
                        {
                            PlayerID = matchPlayer.PlayerID,
                            MerchantID = matchPlayer.MerchantID,
                            Amount = prizeAmount,
                            EventPrizeInfo = convertEventInfo,
                            TransactionNo = eventPrizeRecord.PgsTransactionNo
                        });
                    }
                    else
                    {
                        PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(eachPlayer.PlayerID, 0, prizeAmount, TransactionTypeEnum.EventPrize, recordID.ToString(), "");
                    }
                }
                catch (Exception ex)
                {
                    string msg = $"SendRankEventPrize 錯誤, {eachPlayer.PlayerID} , 派獎單號:{orderNo} , 預期金額:{matchPrize.AmountOrQuantity} , " + ex.Message;
                    SendMessageToMultipleChannel(msg, targetChannel);
                }
            }

            SendMessageToMultipleChannel($"活動:{queryEvent.EventID} ,派獎結束", targetChannel);

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenEventSendAllPrize(eventID);
            return;
        }



        public override void SendMessageToMultipleChannel(string msg, TelegramChannelEnum tgChannel = TelegramChannelEnum.pgs_common_error)
        {
            if (SettingHelper.IsMSTest)
                return;

            base.SendMessageToMultipleChannel(msg, tgChannel);
        }


        protected virtual List<EventDrawPrizeItemEntity> QueryMultiplePrizeItems(int eventID)
        {
            return EventDrawPrizeItemRepository.GetByEventID(eventID);
        }

        protected virtual List<EventBetCounterEntity> QueryEventCounter(int eventID, int pageSize, EventCounterOrderByEnum orderBy)
        {
            return EventBetCounterRepository.GetTopRankingPlayers(eventID, pageSize, orderBy);
        }


        /// <summary>
        /// 根據剩餘數量轉成每個獎品物件 (e.g. 若10個 剩下2個 就只產生2個物件)
        /// </summary>
        private List<EventMultplePrizeBoxItemModel> ConvertToPrizeBoxItem(List<EventDrawPrizeItemEntity> query)
        {
            List<EventMultplePrizeBoxItemModel> boxItems = new List<EventMultplePrizeBoxItemModel>();

            int loopIndex = 0;

            foreach (var item in query)
            {
                if (!item.LimitQuantity.HasValue)
                    continue;

                int remainingPrizeCount = item.LimitQuantity.Value - item.UsageQuantity;

                if (remainingPrizeCount <= 0)
                    continue;

                for (int i = 0; i < remainingPrizeCount; i++)
                {
                    loopIndex++;

                    boxItems.Add(new EventMultplePrizeBoxItemModel
                    {
                        Sequence = loopIndex,
                        AmountOrQuantity = item.AmountOrQuantity,
                        EventDrawPrizeItemID = item.EventDrawPrizeItemID
                    });
                }
            }

            return boxItems;
        }

    }

}
