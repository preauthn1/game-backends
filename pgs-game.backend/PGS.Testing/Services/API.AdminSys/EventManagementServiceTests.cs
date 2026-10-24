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
using System.Threading;

namespace target.Testing
{
    [TestClass]
    public class EventManagementServiceTests : BaseTest
    {

        public IGameLaunchService GameLaunchService { get; set; }

        public IEventManagementService ServiceInstance { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public PlayerRTPConfigRepository PlayerRTPConfigRepository { get; set; }


        [TestMethod]
        public void EventManagementService_GetByPagingFilter_Test()
        {
            //過濾掉空白活動
            List<int> eventTypes = ExtensionOfEnum.GetEnumAllValue<EventTypeEnum>()
                .Where(z => z != EventTypeEnum.EmptyEvent)
                .Select(z => z.ToInteger())
                .ToList();

            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryEventApiRequestParameterModel
            {
                IsIncludeDeleteData = true,
                PageSize = 10
            });

            Assert.IsNotNull(new CRUDResultModel());
            Assert_NotEmptyCollection(query.Items);

            var matchFirst = query.Items
                .Where(z => eventTypes.Contains(z.EventType))
                .Where(z => z.EventType != (int)EventTypeEnum.EmptyEvent)
                .Where(z => z.EventType != (int)EventTypeEnum.DefaultFishBonusPrize)
                .FirstOrDefault();

            //順便驗證下 單筆資料查詢
            var queryEventDetail = ServiceInstance.GetEventDetail(_TEST_DATA.Default.UserID, matchFirst.EventID);

            Assert.IsNotNull(queryEventDetail);
            Assert.IsTrue(queryEventDetail.PrizeBundles.IsNotEmptyNull());
            Assert.IsTrue(queryEventDetail.PrizeBundles.SelectMany(z => z.RuleBundles).IsNotEmptyNull());

            return;
        }


        [TestMethod]
        public void TestGameEnter_AndRTPChange_Test()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(new DateTime(2025, 6, 1)))
            {
                return;
            }

            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;

            //先刪除掉既有資料
            EventPrizeRecordRepository.DeleteByModel(new { PlayerID = playerID });
            PlayerRTPConfigRepository.DeleteByModel(new { PlayerID = playerID });

            //預期進遊戲後 應該會獲得活動獎勵
            GameLaunchService.LaunchGameAndGetGamePageUrlByPlayerID(new InternalTestBundleRequestModel
            {
                PlayerID = playerID,
                GameID = _TEST_DATA.NotSuperMerchant.EventGameID,
            });

            DateTime recently = DateTimeHelper.GetCurrentTime().AddMinutes(-10);

            Thread.Sleep(1000);

            //檢查是否有獲得獎勵
            var queryConfigs = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection()
            });

            //時間要是最近
            queryConfigs = queryConfigs.Where(z => z.CreateTime >= recently).ToList();
            Assert_NotEmptyCollection(queryConfigs);

            return;
        }



        [TestMethod]
        public void EventManagementService_QuickCreateEventt_EachType_Test()
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<EventTemplateTypeEnum>();

            int userID = _TEST_DATA.Default.UserID;

            var rndValueOfValueCompare_AllInOne = new List<int>
            {
                EventRuleTypeEnum.ValueCompare_TotalPoint_AllInOne_MustLargeThan.ToInteger(),
                EventRuleTypeEnum.ValueCompare_TotalGameRound_AllInOne_MustLargeThan.ToInteger(),
            };

            var rndValueOfValueCompare_ByGame = new List<int>
            {
                EventRuleTypeEnum.ValueCompare_TotalPoint_SingleGame_MustLargeThan.ToInteger(),
                EventRuleTypeEnum.ValueCompare_TotalGameRound_SingleGame_MustLargeThan.ToInteger(),
            };

            foreach (EventTemplateTypeEnum templateType in allEnums)
            {
                //SortByRandom 可增加一點隨機性 讓兩種類別都有機會被測到
                var valueCompareType = rndValueOfValueCompare_AllInOne.OrderByRandom().FirstOrDefault();

                if (templateType == EventTemplateTypeEnum.QuestEvent_SpecificGame_GetLuckWheelTicket_10_Quest)
                {
                    valueCompareType = rndValueOfValueCompare_ByGame.OrderByRandom().FirstOrDefault();
                }

                //新增
                var result = ServiceInstance.QuickCreateEvent(userID, new EventQuickCreateRequestModel
                {
                    DisplayName = "MSTest create",
                    EventTemplateType = templateType.ToInteger(),
                    ExtraEventParameter = new EventExtraParameterModel
                    {
                        EventID = _TEST_DATA.NotSuperMerchant.EventID_LuckyWheel,
                        VendorConfigID = _TEST_DATA.Default.VendorConfigID,
                        IsAccountCreateTimeCheck = true,
                        ValueCompareType = valueCompareType,
                    }
                });

                var detail = ServiceInstance.GetEventDetail(userID, result.EventID);

                int years = 200;
                detail.BeginTime = DateTimeHelper.GetCurrentTime().AddYears(years);
                detail.EndTime = DateTimeHelper.GetCurrentTime().AddDays(10).AddYears(years);
                detail.MerchantSelectTemplateID = _TEST_DATA.Default.MerchantSelectTemplateID;
                detail.SupportGameIDs = new List<int>
                {
                    _TEST_DATA.Default.GameID_AGG
                };

                //某些類型的活動rule要填值
                foreach (var bundle in detail.PrizeBundles)
                {
                    foreach (var bundle2 in bundle.RuleBundles)
                    {
                        foreach (var rule in bundle2.RuleItems)
                        {
                            if (rule.RuleType == (int)EventRuleTypeEnum.SpecificGameID)
                            {
                                rule.Value = _TEST_DATA.Default.GameID_AGG.ToString();
                            }
                        }
                    }
                }

                //測試更新
                ServiceInstance.UpdateEventDetail(userID, detail);

                //產生活動說明預設內容區塊
                ServiceInstance.InitialEventContentBlock(userID, new EventContentBlockInitialRequestModel
                {
                    EventID = result.EventID
                });

                //刪除
                ServiceInstance.DeleteEvent(userID, new EventDeleteRequestModel
                {
                    EventID = result.EventID
                });
            }

            return;
        }



        [TestMethod]
        public void EventManagementService_Copy_Test()
        {
            int sourceEventID = _TEST_DATA.NotSuperMerchant.EventID_RedEnvelope;
            int userID = _TEST_DATA.Default.UserID;

            var result = ServiceInstance.CopyEvent(userID, new EventCopyRequestModel
            {
                SourceEventID = sourceEventID,
                Name = "Copy_" + DateTimeHelper.GetCurrentTime(8).ToString("yyyyMMddHHmmss")
            });

            Assert.IsTrue(result.EventID > 0);

            int newEventID = result.EventID;

            var queryDetail1 = ServiceInstance.GetEventDetail(userID, sourceEventID);
            var queryDetail2 = ServiceInstance.GetEventDetail(userID, newEventID);

            Assert.IsTrue(queryDetail1.SupportGameIDs.Count() == queryDetail2.SupportGameIDs.Count());
            Assert.IsTrue(queryDetail1.MerchantSelectTemplateID == queryDetail2.MerchantSelectTemplateID);
            Assert.IsTrue(queryDetail1.EventType == queryDetail2.EventType);
            Assert.IsTrue(queryDetail1.EventTemplateType == queryDetail2.EventTemplateType);

            ServiceInstance.DeleteEvent(userID, new EventDeleteRequestModel
            {
                EventID = newEventID
            });

            return;
        }



        [TestMethod]
        public void EventManagementService_GetPrizeRecordsByPagingFilter_Test()
        {
            var query1 = ServiceInstance.GetPrizeRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryEventPrizeRecordParameterModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
            });

            var query2 = ServiceInstance.GetPrizeRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryEventPrizeRecordParameterModel
            {
                ReceiveTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                ReceiveTimeEnd = DateTimeHelper.GetCurrentTime(),
            });

            Assert.IsNotNull(new CRUDResultModel());
            Assert_NotEmptyCollection(query1.Items);
            return;
        }




        [TestMethod]
        public void EventManagementService_UpdateEventDetail_Test()
        {
            var queryEvent = ServiceInstance.GetEventDetail(_TEST_DATA.Default.UserID, _TEST_DATA.NotSuperMerchant.EventID);

            Assert.IsNotNull(queryEvent);

            ServiceInstance.UpdateEventDetail(_TEST_DATA.Default.UserID, queryEvent);
            return;
        }

        [TestMethod]
        public void GetEventContentBlock_Test()
        {
            var queryViewModel = ServiceInstance.GetEventContentBlock(_TEST_DATA.Default.UserID, _TEST_DATA.NotSuperMerchant.EventID);

            Assert.IsTrue(queryViewModel.EventContentBlocks.IsNotEmptyNull() || queryViewModel.IsNotInitial);

            if (queryViewModel.EventContentBlocks.IsNotEmptyNull())
            {
                ServiceInstance.UpdateEventContentBlock(_TEST_DATA.Default.UserID, queryViewModel);
            }

            return;
        }


    }
}

