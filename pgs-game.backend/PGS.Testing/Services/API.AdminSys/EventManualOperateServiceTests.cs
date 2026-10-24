using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class EventManualOperateServiceTests : BaseTest
    {

        public IEventManualOperateService ServiceInstance { get; set; }

        public EventRepository EventRepository { get; set; }

        [TestMethod]
        public void PreviewEventPlayerRank_Test()
        {
            var query = ServiceInstance.PreviewEventPlayerRank(_TEST_DATA.Default.UserID , new EventRankPreviewRequestModel
            {
                EventID = _TEST_DATA.Default.EventID_Delete,
            });

            Assert.IsFalse(query.IsPrizeEmpty);
            Assert.IsTrue(query.Players.IsEmptyOrNull());
            return;
        }




        [TestMethod]
        public void PreviewEventPlayerRank_ExistEvent_Test()
        {
            var now = DateTimeHelper.GetCurrentTime();

            var firstMatchEvent = EventRepository.GetPagingByFilter(new QueryEventRequestViewModel
            {
                EventTypes = EventTypeEnum.RankCompetition.ToInteger().ToListCollection(),
                PageIndex = 1,
                PageSize = 5
            })
            .Items
            .OrderBy(x =>
            {
                if (x.EndTime > now && x.BeginTime < now && x.Status == NormalActiveStatus.Enable.ToInteger())
                    return 1;

                return 999;
            })
            .ThenBy(x=> ((int)(x.BeginTime - now).TotalSeconds).ToAbs())
            .FirstOrDefault();

            if(firstMatchEvent != null)
            {
                var query1 = ServiceInstance.PreviewEventPlayerRank(_TEST_DATA.Default.UserID, new EventRankPreviewRequestModel
                {
                    EventID = firstMatchEvent.EventID,
                    OrderByType = -1
                });

                var query2 = ServiceInstance.PreviewEventPlayerRank(_TEST_DATA.Default.UserID, new EventRankPreviewRequestModel
                {
                    EventID = firstMatchEvent.EventID,
                    OrderByType = EventCounterOrderByEnum.TotalBetAmount.ToInteger(),
                });

                var query3 = ServiceInstance.PreviewEventPlayerRank(_TEST_DATA.Default.UserID, new EventRankPreviewRequestModel
                {
                    EventID = firstMatchEvent.EventID,
                    OrderByType = EventCounterOrderByEnum.MaxPayoutRate.ToInteger(),
                });

                return;
            }

            return;
        }


    }
}

