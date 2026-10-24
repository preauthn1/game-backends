using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class EventCommonPrizeSendServiceTests : BaseTest
    {

        public EventRepository EventRepository { get; set; }

        public EventCustomizedSettingRepository EventCustomizedSettingRepository { get; set; }

        public IEventCommonPrizeSendService EventCommonPrizeSendService { get; set; }



        public class MockEventCommonPrizeSendService : EventCommonPrizeSendService
        {
            protected override List<EventDrawPrizeItemEntity> QueryMultiplePrizeItems(int eventID)
            {
                return new List<EventDrawPrizeItemEntity>
                {
                    new EventDrawPrizeItemEntity
                    {
                        AmountOrQuantity = 10000,
                        UsageQuantity = 0,
                        LimitQuantity = 1,
                    },
                    new EventDrawPrizeItemEntity
                    {
                        AmountOrQuantity = 5000,
                        UsageQuantity = 0,
                        LimitQuantity = 3,
                    },
                    new EventDrawPrizeItemEntity
                    {
                        AmountOrQuantity = 1000,
                        UsageQuantity = 0,
                        LimitQuantity = 10,
                    },
                };
            }

            protected override List<EventBetCounterEntity> QueryEventCounter(int eventID, int pageSize, EventCounterOrderByEnum orderBy)
            {
                return new List<EventBetCounterEntity>
                {
                    new EventBetCounterEntity
                    {
                        PlayerID = _TEST_DATA.Default.PlayerID,
                        TotalBetAmount = 10000,
                    },
                    new EventBetCounterEntity
                    {
                        PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
                        TotalBetAmount = 5000,
                    },
                };
            }

        }



        [TestMethod]
        public void LocalTest_SendRankEventPrize()
        {
            if (IsNotDeveloperComputerName())
                return;

            //int playerID = _TEST_DATA.Not_DefaultMerchant.PlayerID;
            //int eventID = 251220;

            //EventCommonPrizeSendService.SendRankEventPrize(eventID);
            return;
        }


        [TestMethod]
        public void SendRankEventPrize_Test()
        {
            var query = EventRepository.GetPagingByFilter(new QueryEventRequestViewModel
            {
                PageSize = 10,
                EventTypes = EventTypeEnum.RankCompetition.ToInteger().ToListCollection(),
                Status = NormalActiveStatus.PermanentDelete.ToInteger(),
                IsIncludeDeleteData = true,
                OrderByColumn = "EventID",
                IsASC = true
            }).Items;

            if (query.IsEmptyOrNull())
                return;

            var matchEvent = query.FirstOrDefault(x=> x.DisplayName.Contains("MSTest"));

            //清除tag
            EventCustomizedSettingRepository.DeleteByModel(new
            {
                EventID = matchEvent.EventID,
                SettingKey = EventCustomizedSettingKeyEnum.Tag_PrizeEmpty.ToInteger()
            });

            var service = new MockEventCommonPrizeSendService();

            service.SendRankEventPrize(matchEvent.EventID , new RankEventTriggerSendPrizeParameter
            {
                OrderBy = EventCounterOrderByEnum.TotalBetAmount,
            });

            return;
        }


    }
}

