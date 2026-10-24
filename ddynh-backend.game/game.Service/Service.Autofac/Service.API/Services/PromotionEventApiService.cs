using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Money;

namespace game.Service.Service
{
    public class PromotionEventApiService : BaseService, IPromotionEventApiService
    {
        private UserInfoRepository UserInfoRepository { get; set; }

        private AreaRepository AreaRepository { get; set; }

        private EventSettingRepository EventSettingRepository { get; set; }

        private EventSettingAwardItemRepository EventSettingAwardItemRepository { get; set; }


        private static List<int> AvailableEventTypes = new List<int>
        {
            (int)ApplyEventTypeEnum.Roulette_FlowAmount,
            (int)ApplyEventTypeEnum.Roulette_Recharge,
        };



        public PromotionEventViewModel GetEvents(PromotionEventRequestViewModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Certificate).Rule_NotEmptyText();

            var queryUser = UserInfoRepository.GetById(model.UserID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            int platformID = AreaRepository.GetSingleByCache(queryUser.AreaID).PlatformID;

            var filter = new QueryEventSettingRequestViewModel();
            filter.PlatformIDsText = platformID.ToString();
            filter.EventTypes = AvailableEventTypes;
            DateTime now = DateTime.Now;
            filter.StartTimeEnd = now;
            filter.EndTimeBegin = now;
            filter.PageIndex = 1;
            filter.PageSize = 20;
            var queryEvent = EventSettingRepository.GetPagingByFilter(filter);

            List<PromotionEventItemViewModel> events = queryEvent.PageData
                .Where(x => x.Status == (int)NormalActiveStatusEnum.Enable)
                .Select(x => new PromotionEventItemViewModel
                {
                    EventID = x.ID,
                    EventName = x.EventName,
                    EventType = x.EventType,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                })
                .ToList();

            return new PromotionEventViewModel
            {
                Events = events
            };
        }


        public PromotionEventDetailViewModel GetEventDetail(PromotionEventDetailRequestViewModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Certificate).Rule_NotEmptyText();

            var queryUser = UserInfoRepository.GetById(model.UserID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            int platformID = AreaRepository.GetSingleByCache(queryUser.AreaID).PlatformID;

            var queryEvent = EventSettingRepository.GetById(model.EventID);

            if (queryEvent.Status != (int)NormalActiveStatusEnum.Enable)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var queryAward = EventSettingAwardItemRepository.GetBySettingID(queryEvent.ID);
            List<PromotionEventLuckyRouletteBonusItemViewModel> award = queryAward.Select(y => new PromotionEventLuckyRouletteBonusItemViewModel
            {
                BonusID = y.Id,
                BonusName = y.AwardName,
                Amount = y.AwardAmount,
                BonusType = y.AwardType,
                ImageUrl = y.ImageUrlDomain + y.ImageUrl
            })
                .ToList();

            //計算玩家抽獎卷次數
            int ticketCount = EventSettingRepository.GetLotteryCount(queryUser.UserID, platformID, queryEvent.ID);

            return new PromotionEventDetailViewModel
            {
                BasicInfo = new PromotionEventItemViewModel
                {
                    EventID = queryEvent.ID,
                    EventName = queryEvent.EventName,
                    EventType = queryEvent.EventType,
                    StartTime = queryEvent.StartTime,
                    EndTime = queryEvent.EndTime
                },
                LuckyRouletteInfo = new PromotionEventLuckyRouletteExtrViewModel
                {
                    TicketCount = ticketCount,
                    EventMsg = string.IsNullOrEmpty(queryEvent.Memo) ? "" : queryEvent.Memo,  //"當日累計 100元以上 抽獎一次 ...",
                    TipsMsg = string.IsNullOrEmpty(queryEvent.Tips) ? "" : queryEvent.Tips,  //"",
                    BonusItems = award
                }
            };
        }


        public PromotionEventLuckyRoulettDrawResultModel LuckyRoulette_DrawPrize(PromotionEvenBonusDrawRequestViewModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Certificate).Rule_NotEmptyText();

            var queryUser = UserInfoRepository.GetById(model.UserID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            int platformID = AreaRepository.GetSingleByCache(queryUser.AreaID).PlatformID;

            var queryEvent = EventSettingRepository.GetById(model.EventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            //計算玩家抽獎卷次數
            int ticketCount = EventSettingRepository.GetLotteryCount(queryUser.UserID, platformID, queryEvent.ID);

            //抽獎次數為0
            if (ticketCount == 0)
                return new PromotionEventLuckyRoulettDrawResultModel
                {
                    IsSuccess = false,
                    BonusID = 0,
                    ResultMsg = "抽奖券数量不足",
                    LotteryRemaining = 0
                };

            //抽獎
            var returnModel = EventSettingRepository.GetLottery(queryUser.UserID, platformID, queryEvent.ID);
            return new PromotionEventLuckyRoulettDrawResultModel
            {
                IsSuccess = (returnModel.EventID != 0),
                BonusID = returnModel.EventID,
                LotteryRemaining = returnModel.LotteryRemaining,
                ResultMsg = returnModel.EventMsg,
            };
        }
    }
}
