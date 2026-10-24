using Lck.Utility.Extensions.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class PromotionEventServiceTests : BaseTest
    {

        public IPromotionEventService ServiceInstance { get; set; }

        public EventBetCounterRepository EventBetCounterRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }




        
        [TestMethod]
        public void GetEventsByMerchantAndEventType_NotExistMerchant_Test()
        {
            //檢查會不會撈到不該撈的資料
            var myService = new BaseEventService();
            var queryEvents = myService.GetEventsByMerchantAndEventType(0, null, null);
            Assert.IsTrue(queryEvents.IsEmptyOrNull());
            return;
        }



        [TestMethod]
        public void GetAllPublicEventInfo_Test()
        {
            var query = ServiceInstance.GetAllPublicEventInfo(_TEST_DATA.Default.PlayerID);
            Assert.IsNotNull(query); 
            Assert.IsNotNull(query.RedEnvelope);
            return;
        }


        [TestMethod]
        public void GetLobbyEnableEventsAndMessageHint_Test()
        {
            var query = ServiceInstance.GetLobbyEnableEventsAndMessageHint(_TEST_DATA.Default.PlayerID);
            Assert.IsNotNull(query);
            return;
        }



        [TestMethod]
        public void GetSingleEventDetail_LuckyWheel_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            var enableCheck = ServiceInstance.GetLobbyEnableEventsAndMessageHint(playerID);
            var query = ServiceInstance.GetSingleEventDetail(playerID, new PublicPageSingleEventDetailRequestModel
            {
                EventType = EventTypeEnum.LuckyWheel.ToInteger()
            });

            if (enableCheck.IsEnableLuckyWheel)
            {
                Assert.IsTrue(query != null);
                Assert.IsTrue(query.LuckyWheel != null);
            }

            return;
        }


        [TestMethod]
        public void GetSingleEventDetail_RankCompetition_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            var enableCheck = ServiceInstance.GetLobbyEnableEventsAndMessageHint(playerID);
            var query = ServiceInstance.GetSingleEventDetail(playerID, new PublicPageSingleEventDetailRequestModel
            {
                EventType = EventTypeEnum.RankCompetition.ToInteger()
            });

            if(enableCheck.IsEnableRankingEvent)
            {
                Assert.IsTrue(query != null);
                Assert.IsTrue(query.RankCompetition != null);
            }

            return;
        }


        [TestMethod]
        public void GetEventDescription_Test()
        {
            int eventID = _TEST_DATA.NotSuperMerchant.EventID;

            var query = ServiceInstance.GetEventDescription(_TEST_DATA.Default.PlayerID,  new PublicPageEventDescriptionRequestViewModel
            {
                EventID = eventID,
                CheckCode = AppHelper.GenerateIntegerCheckCode(eventID)
            });

            Assert.IsTrue(query != null);
            return;
        }


        [TestMethod]
        public void GetPrizeRecords_Test()
        {
            var query = ServiceInstance.GetPrizeRecords(_TEST_DATA.Default.PlayerID, new PublicApiEventPrizeRecordRequestModel
            {
                
            });

            Assert.IsTrue(query != null);
            return;
        }



        [TestMethod]
        public void GetAvailableQuest_TotalPoint_And_Receive_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;

            var query = ServiceInstance.GetAvailableQuest(playerID, new PublicPageEventQuestRequestModel());

            if(query.QuestItems.IsNotEmptyNull())
            {
                var pointExchageLcukWheelQuests = query.QuestItems
                    .Where(x => x.EventType == (int)EventTypeEnum.QuestEvent_GetLuckWheelTicket)
                    .Where(x=> x.MainRuleType == EventRuleTypeEnum.ValueCompare_TotalPoint_AllInOne_MustLargeThan.ToInteger())
                    .ToList();

                if(pointExchageLcukWheelQuests.IsNotEmptyNull())
                {
                    var matchFirst = pointExchageLcukWheelQuests.OrderBy(x=> x.MustLargeThanValue).FirstOrDefault();

                    decimal plusValue = 10000000000;

                    //加值積分
                    EventBetCounterRepository.EventCounterValuePlus(new EventCounterValuePlusSPParameter
                    {
                        EventID = matchFirst.EventID,
                        EventPoint = plusValue,
                        BetAmount = plusValue,
                        PayoutAmount = plusValue,
                        GameID = 0,
                        PlayerID = playerID
                    });

                    //清除測試資料
                    EventPrizeRecordRepository.DeleteByModel(new
                    {
                        EventID = matchFirst.EventID,
                        EventPrizeBundleID = matchFirst.EventPrizeBundleID
                    });

                    //清除暫存避免被影響
                    string cacheKey = CacheKey.Events_PrizeRecord_GameID_ExistCheck(playerID, matchFirst.EventPrizeBundleID, 0);
                    CacheHelper.RemoveCacheByKeyword(cacheKey);

                    //領取獎勵
                    ServiceInstance.ReceiveQuestReward(playerID, new PublicPageEventQuestReceiveRequestModel
                    {
                        EventID = matchFirst.EventID,
                        EventPrizeBundleID = matchFirst.EventPrizeBundleID,
                        Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                        SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword
                    });
                    
                    var records = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                    {
                        EventIDs = matchFirst.EventID.ToListCollection(),
                        PlayerIDs = playerID.ToListCollection(),
                        EventPrizeBundleIDs = matchFirst.EventPrizeBundleID.ToListCollection(),
                        CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * 15)
                    });

                    //是否有派發紀錄
                    Assert.IsTrue(records.IsNotEmptyNull());

                    //領取獎勵(第二次重複領取)
                    AssertExpectExceptionProcess(() =>
                    {
                        ServiceInstance.ReceiveQuestReward(playerID, new PublicPageEventQuestReceiveRequestModel
                        {
                            EventID = matchFirst.EventID,
                            EventPrizeBundleID = matchFirst.EventPrizeBundleID,
                            Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                            SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword
                        });

                    }, ApiErrorCode.BusinessLogicDeny_EventRuleNotAllPass);
                }
            }

            return;
        }




        [TestMethod]
        public void GetAvailableQuest_DailyQuest_And_Receive_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;

            var query = ServiceInstance.GetAvailableQuest(playerID, new PublicPageEventQuestRequestModel());

            if (query.QuestItems.IsNotEmptyNull())
            {
                var pointExchageLcukWheelQuests = query.QuestItems
                    .Where(x => x.EventType == (int)EventTypeEnum.QuestEvent_GetLuckWheelTicket)
                    .Where(x => x.IsDailyEvent)
                    .Where(x => x.MainRuleType == EventRuleTypeEnum.ValueCompare_TotalPoint_AllInOne_MustLargeThan.ToInteger())
                    .ToList();

                if (pointExchageLcukWheelQuests.IsNotEmptyNull())
                {
                    var matchFirst = pointExchageLcukWheelQuests.OrderBy(x => x.MustLargeThanValue).FirstOrDefault();

                    decimal plusValue = 10000000000;

                    //加值積分
                    EventBetCounterRepository.EventCounterValuePlus(new EventCounterValuePlusSPParameter
                    {
                        EventID = matchFirst.EventID,
                        EventPoint = plusValue,
                        BetAmount = plusValue,
                        PayoutAmount = plusValue,
                        GameID = 0,
                        EventStatDate = DateTimeHelper.GetCurrentTime(SystemConstant.DailyEventBasicTimezone), //每日型的要加時間
                        PlayerID = playerID
                    });

                    //清除測試資料
                    EventPrizeRecordRepository.DeleteByModel(new
                    {
                        EventID = matchFirst.EventID,
                        PlayerID = playerID,
                        EventPrizeBundleID = matchFirst.EventPrizeBundleID
                    });

                    //清除暫存避免被影響
                    string cacheKey = CacheKey.Events_PrizeRecord_GameID_ExistCheck(playerID, matchFirst.EventPrizeBundleID, 0);
                    CacheHelper.RemoveCacheByKeyword(cacheKey);

                    //領取獎勵
                    ServiceInstance.ReceiveQuestReward(playerID, new PublicPageEventQuestReceiveRequestModel
                    {
                        EventID = matchFirst.EventID,
                        EventPrizeBundleID = matchFirst.EventPrizeBundleID,
                        Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                        SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword
                    });

                    var records = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                    {
                        EventIDs = matchFirst.EventID.ToListCollection(),
                        PlayerIDs = playerID.ToListCollection(),
                        EventPrizeBundleIDs = matchFirst.EventPrizeBundleID.ToListCollection(),
                        CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * 15)
                    });

                    //是否有派發紀錄
                    Assert.IsTrue(records.IsNotEmptyNull());

                    string sql1 = " SET ReceiveTime = '2020-01-01 '";
                    string sql2 = $" WHERE EventID = { matchFirst.EventID} AND EventPrizeBundleID = {matchFirst.EventPrizeBundleID} AND PlayerID = {playerID}";

                    //將獎品領取時間改成其他時間(模擬當天尚未領過)
                    EventPrizeRecordRepository.UpdateWithSqlCommandForMSTest(sql1, sql2);

                    ServiceInstance.ReceiveQuestReward(playerID, new PublicPageEventQuestReceiveRequestModel
                    {
                        EventID = matchFirst.EventID,
                        EventPrizeBundleID = matchFirst.EventPrizeBundleID,
                        Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                        SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword
                    });
                }
            }

            return;
        }


        [TestMethod]
        public void GetAvailableQuest_TotalGameRound_And_Receive_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;

            var query = ServiceInstance.GetAvailableQuest(playerID, new PublicPageEventQuestRequestModel());

            if (query.QuestItems.IsNotEmptyNull())
            {
                var gameRoundQuests = query.QuestItems
                    .Where(x => x.EventType == (int)EventTypeEnum.QuestEvent_GetLuckWheelTicket)
                    .Where(x => !x.IsDailyEvent)
                    .Where(x => x.MainRuleType == EventRuleTypeEnum.ValueCompare_TotalGameRound_SingleGame_MustLargeThan.ToInteger())
                    .Where(x=> x.SpecificGameID > 0)
                    .ToList();

                if (gameRoundQuests.IsNotEmptyNull())
                {
                    var matchFirst = gameRoundQuests.OrderBy(x => x.MustLargeThanValue).FirstOrDefault();

                    int plusValue = 100000;

                    //刪除舊資料、重建資料
                    EventBetCounterRepository.DeleteByModel(new
                    {
                        EventID = matchFirst.EventID,
                        GameID = matchFirst.SpecificGameID,
                        PlayerID = playerID
                    });

                    EventBetCounterRepository.Create(new EventBetCounterEntity
                    {
                        EventID = matchFirst.EventID,
                        GameID = matchFirst.SpecificGameID,
                        PlayerID = playerID,
                        CreateTime = DateTimeHelper.GetCurrentTime(),
                        UpdateTime = DateTimeHelper.GetCurrentTime(),
                        UsagePoint = 0,
                        TotalPoint = 0,
                        TotalRound = plusValue,
                    });

                    //清除測試資料
                    EventPrizeRecordRepository.DeleteByModel(new
                    {
                        EventID = matchFirst.EventID,
                        EventPrizeBundleID = matchFirst.EventPrizeBundleID
                    });

                    //清除暫存避免被影響
                    string cacheKey = CacheKey.Events_PrizeRecord_GameID_ExistCheck(playerID, matchFirst.EventPrizeBundleID, 0);
                    CacheHelper.RemoveCacheByKeyword(cacheKey);

                    //領取獎勵
                    ServiceInstance.ReceiveQuestReward(playerID, new PublicPageEventQuestReceiveRequestModel
                    {
                        EventID = matchFirst.EventID,
                        EventPrizeBundleID = matchFirst.EventPrizeBundleID,
                        Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                        SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword
                    });

                    var records = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                    {
                        EventIDs = matchFirst.EventID.ToListCollection(),
                        PlayerIDs = playerID.ToListCollection(),
                        EventPrizeBundleIDs = matchFirst.EventPrizeBundleID.ToListCollection(),
                        CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * 15)
                    });

                    //是否有派發紀錄
                    Assert.IsTrue(records.IsNotEmptyNull());

                    //領取獎勵(第二次重複領取)
                    AssertExpectExceptionProcess(() =>
                    {
                        ServiceInstance.ReceiveQuestReward(playerID, new PublicPageEventQuestReceiveRequestModel
                        {
                            EventID = matchFirst.EventID,
                            EventPrizeBundleID = matchFirst.EventPrizeBundleID,
                            Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                            SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword
                        });

                    }, ApiErrorCode.BusinessLogicDeny_EventRuleNotAllPass);
                }
            }

            return;
        }



    }
}

