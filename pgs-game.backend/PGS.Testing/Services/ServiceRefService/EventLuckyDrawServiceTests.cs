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
    public class EventLuckyDrawServiceTests : BaseTest
    {
        public IEventLuckyDrawService ServiceInstance { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public EventDrawPrizeItemRepository EventDrawPrizeItemRepository { get; set; }

        public EventBetCounterRepository EventBetCounterRepository { get; set; }


        public class FakeEventLuckyDrawService : EventLuckyDrawService
        {
            public override List<EventDrawPrizeItemEntity> GetDrawPrizeItems(int eventID)
            {
                var query = base.GetDrawPrizeItems(eventID);

                if (query.IsEmptyOrNull())
                    return query;

                var matchFirst = query.FirstOrDefault();

                //清空資料
                EventDrawPrizeItemRepository.ResetUsageByEventParentDrawID(matchFirst.EventDrawPrizeID);

                //DB的TEST資料是沒有限數量的 這邊為了模擬改成有數量
                foreach (var item in query)
                {
                    item.LimitQuantity = 20;
                }

                return query;
            }
        }

        public class Fake_V2_PrizeLimit_EventLuckyDrawService : EventLuckyDrawService
        {
            public override List<EventDrawPrizeItemEntity> GetDrawPrizeItems(int eventID)
            {
                var query = base.GetDrawPrizeItems(eventID);

                if (query.IsEmptyOrNull())
                    return query;

                //ps. 這個活動有特別設置 獎品數量都只有1個
                return query;
            }
        }



        [TestMethod]
        public void RadomHelper_LongNumber_Test()
        {
            long max = 10;

            for(int i = 0; i <= 99999; i++)
            {
                //驗證產生的數字不含最後一個數字 0~10 則最大的數字只有9
                var rnd = RandomHelper.GetRandLongNumbers(0, max);
                Assert.IsTrue(rnd < max);
            }

            return;
        }


        [TestMethod]
        public void PutPrizeToDrawBox_Test()
        {
            var myService = new EventLuckyDrawService();

            var boxResult = myService.PutPrizeToDrawBox(new List<EventDrawPrizeItemEntity>
            {
                new EventDrawPrizeItemEntity
                {
                    EventDrawPrizeItemID = 1,
                    LimitQuantity = 1,
                    ProbabilityWeights = 1
                },
                new EventDrawPrizeItemEntity
                {
                    EventDrawPrizeItemID = 2,
                    LimitQuantity = 10,
                    ProbabilityWeights = 10
                },
                new EventDrawPrizeItemEntity
                {
                    EventDrawPrizeItemID = 3,
                    LimitQuantity = 100,
                    ProbabilityWeights = 100
                },
            });

            return;
        }



        [TestMethod]
        public void DrawRandomPrizeFromBox_Test()
        {
            var myService = new EventLuckyDrawService();

            var boxResult = myService.PutPrizeToDrawBox(new List<EventDrawPrizeItemEntity>
            {
                new EventDrawPrizeItemEntity
                {
                    EventDrawPrizeItemID = 1,
                    LimitQuantity = 1,
                    ProbabilityWeights = 1
                },
                new EventDrawPrizeItemEntity
                {
                    EventDrawPrizeItemID = 2,
                    LimitQuantity = 1,
                    ProbabilityWeights = 10
                },
                new EventDrawPrizeItemEntity
                {
                    EventDrawPrizeItemID = 3,
                    LimitQuantity = 1,
                    ProbabilityWeights = 50
                },
            });

            List<int> drawPrizeIDs = new List<int>();

            for(int i = 0; i < 10000; i++)
            {
                var prizeItem = myService.DrawRandomPrizeFromBox(boxResult);
                drawPrizeIDs.Add(prizeItem);
            }

            var countOf1 = drawPrizeIDs.Where(x => x == 1).Count();
            var countOf2 = drawPrizeIDs.Where(x => x == 2).Count();
            var countOf3 = drawPrizeIDs.Where(x => x == 3).Count();

            //一萬次機率來說 總次數一定是 3 > 2 > 1
            Assert.IsTrue(countOf3 >= countOf2);
            Assert.IsTrue(countOf2 >= countOf1);
            return;
        }



        [TestMethod]
        public void DrawRandomPrizeFromBox_Case2_50_vs_50_Test()
        {
            var myService = new EventLuckyDrawService();

            //兩個獎項的數量不同 但透過權重配置 應該還會是一樣的機率
            var boxResult = myService.PutPrizeToDrawBox(new List<EventDrawPrizeItemEntity>
            {
                new EventDrawPrizeItemEntity
                {
                    EventDrawPrizeItemID = 1,
                    LimitQuantity = 50,
                    ProbabilityWeights = 100
                },
                new EventDrawPrizeItemEntity
                {
                    EventDrawPrizeItemID = 2,
                    LimitQuantity = 5,
                    ProbabilityWeights = 1000
                },
            });

            List<int> drawPrizeIDs = new List<int>();
            int maxTimes = 10000;

            for (int i = 0; i < maxTimes; i++)
            {
                var prizeItem = myService.DrawRandomPrizeFromBox(boxResult);
                drawPrizeIDs.Add(prizeItem);
            }

            var countOf1 = drawPrizeIDs.Where(x => x == 1).Count();
            var countOf2 = drawPrizeIDs.Where(x => x == 2).Count();

            //允許5%的誤差
            int expectTimes = (int)(maxTimes / 2 * 0.95);

            //理論上機率是50
            Assert.IsTrue(countOf1 >= expectTimes);
            Assert.IsTrue(countOf2 >= expectTimes);
            return;
        }



        [TestMethod]
        public void LuckyWheel_NormalDraw_And_EmptyDraw_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;
            int eventID = _TEST_DATA.NotSuperMerchant.EventID_LuckyWheel;

            var query = EventBetCounterRepository.GetByFilter(new QueryEventBetCounterRequestViewModel
                {
                    EventIDs = eventID.ToListCollection(),
                    PlayerIDs = playerID.ToListCollection()
                })
                .FirstOrDefault();

            if(query == null)
            {
                throw new TerproException(ApiErrorCode.NotImplement);
            }

            //重製已消耗積分
            query.UsagePoint = 0;
            EventBetCounterRepository.Update(query);

            //清空已領取數
            EventDrawPrizeItemRepository.ResetUsageByEventID(eventID  , newUsageQuantity: 0);

            // Case 1. 一搬正常領獎
            var drawResult = ServiceInstance.UseEventPointDrawRandomPrize(playerID, eventID);

            //強制讓獎品全部被領完
            EventDrawPrizeItemRepository.ResetUsageByEventID(eventID, newUsageQuantity: 999999);

            // Case 2. 獎品已被領完
            AssertExpectExceptionProcess(() =>
            {
                var drawResult = ServiceInstance.UseEventPointDrawRandomPrize(playerID, eventID);

            }, ApiErrorCode.BusinessLogicDeny_EventPrizeEmpty);

            EventDrawPrizeItemRepository.ResetUsageByEventID(eventID, newUsageQuantity: 0);
            return;
        }


        [TestMethod]
        public void DrawRandomPrizeFromBox_AllItemEmpty_Test()
        {
            var myService = new EventLuckyDrawService();

            var boxResult = myService.PutPrizeToDrawBox(new List<EventDrawPrizeItemEntity>
                {
                    new EventDrawPrizeItemEntity
                    {
                        EventDrawPrizeItemID = 1,
                        LimitQuantity = 50,
                        UsageQuantity = 50,
                        ProbabilityWeights = 100
                    },
                    new EventDrawPrizeItemEntity
                    {
                        EventDrawPrizeItemID = 2,
                        LimitQuantity = 10,
                        UsageQuantity = 10,
                        ProbabilityWeights = 1000
                    },
                });

            Assert.IsTrue(boxResult.ItemsRange.IsEmptyOrNull());
            return;
        }



        [TestMethod]
        public void DrawRandomPrize_DrawPrizeWithoutLimit_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;
            int eventID = _TEST_DATA.NotSuperMerchant.EventID_RedEnvelope;

            //直接送一張抽獎券
            GenerateTicketForTest(playerID, eventID);

            ServiceInstance.UseTicketDrawRandomPrize(playerID, eventID);
            return;
        }




        [TestMethod]
        public void DrawRandomPrize_DrawToEmpty_Test()
        {
            var fakeServiceV2 = new Fake_V2_PrizeLimit_EventLuckyDrawService();

            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;
            int eventID = _TEST_DATA.NotSuperMerchant.EventID_RedEnvelope_PrizeCountRestrict;

            //清空已領取數
            EventDrawPrizeItemRepository.ResetUsageByEventID(eventID);

            var prizeItems = fakeServiceV2.GetDrawPrizeItems(eventID);

            var matchFirstPrize = prizeItems.FirstOrDefault();

            var totalPrizeCount = prizeItems
                .Select(x => x.LimitQuantity)
                .Sum();

            var listResult = new List<LuckyDrawResultViewModel>();

            var fakeParameter = new LuckyDrawRequestModel
            {
                EventID = eventID,
                PlayerID = playerID,
                GameID = _TEST_DATA.NotSuperMerchant.EventGameID,
                InitialStatus = EventPrizeRecordStatusEnum.AllocateSuccessNotReceive,
                BetTime = DateTimeHelper.GetCurrentTime(),
                BetRecordNoPGS = RandomHelper.GetOrderNo("MSTest"),
                BetAmount = 1000
            };

            for (int i=0; i < totalPrizeCount; i++)
            {
                var result = fakeServiceV2.DrawRandomPrize(fakeParameter);
                listResult.Add(result);
            }

            var uniqueIDs = listResult.Select(x => x.ItemDetail.EventDrawPrizeItemID).Distinct().ToList();
            Assert.IsTrue(uniqueIDs.Count() == totalPrizeCount);

            //獎品預計已抽完 應該拋錯誤
            AssertExpectExceptionProcess(() =>
            {
                var result = fakeServiceV2.DrawRandomPrize(fakeParameter);

            }, ApiErrorCode.BusinessLogicDeny_EventPrizeEmpty);

            return;
        }



        [TestMethod]
        public void DrawRandomPrize_DrawPrizeHasLimitQuantity_Test()
        {
            var fakeService = new FakeEventLuckyDrawService();

            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;
            int eventID = _TEST_DATA.NotSuperMerchant.EventID_RedEnvelope;

            fakeService.DrawRandomPrize(new LuckyDrawRequestModel
            {
                EventID = eventID,
                PlayerID = playerID,
                GameID = _TEST_DATA.NotSuperMerchant.EventGameID,
                InitialStatus = EventPrizeRecordStatusEnum.AllocateSuccessNotReceive,
                BetTime = DateTimeHelper.GetCurrentTime(),
                BetRecordNoPGS = RandomHelper.GetOrderNo("MSTest"),
                BetAmount = 1000
            });

            return;
        }



        [TestMethod]
        public void DrawPrize_UpdateDrawPrizeStatusAndSendBalance_Test()
        {
            var fakeService = new FakeEventLuckyDrawService();

            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;
            int eventID = _TEST_DATA.NotSuperMerchant.EventID_RedEnvelope;

            //先抽一個紅包
            fakeService.DrawRandomPrize(new LuckyDrawRequestModel
            {
                EventID = eventID,
                PlayerID = playerID,
                GameID = _TEST_DATA.NotSuperMerchant.EventGameID,
                InitialStatus = EventPrizeRecordStatusEnum.AllocateSuccessNotReceive,
                BetTime = DateTimeHelper.GetCurrentTime(),
                BetRecordNoPGS = RandomHelper.GetOrderNo("MSTest"),
                BetAmount = 1000
            });

            //領取
            ServiceInstance.UpdateDrawPrizeStatusAndSendBalance(playerID , eventID);
            return;
        }


        [TestMethod]
        public void DrawRandomPrize_UseTicketDrawRandomPrize_Test()
        {
            var fakeService = new FakeEventLuckyDrawService();

            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;
            int eventID = _TEST_DATA.NotSuperMerchant.EventID_RedEnvelope;

            //直接送一張抽獎券
            GenerateTicketForTest(playerID, eventID);

            fakeService.UseTicketDrawRandomPrize(playerID, eventID);
            return;
        }


        private void GenerateTicketForTest(int playerID, int eventID , EventPrizeRecordStatusEnum status = EventPrizeRecordStatusEnum.AllocateSuccess)
        {
            string randomNo = RandomHelper.GetOrderNo("MSTest");
            DateTime now = DateTimeHelper.GetCurrentTime();

            EventPrizeRecordRepository.Create(new Models.Table.EventPrizeRecordEntity
            {
                Status = status.ToInteger(),
                EventID = eventID,
                AmountOrQuantity = 1,
                EventPrizeType = EventPrizeTypeEnum.Ticket_RedEnvelope.ToInteger(),
                CreateTime = now,
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                PlayerID = playerID,
                GameID = _TEST_DATA.Default.GameID,
                PlayerName = _TEST_DATA.NotSuperMerchant.PlayerName,
                EventPrizeBundleID = -1,
                EventPrizeItemID = -1,
                ReferenceNo = randomNo,
                ReferenceAmount = 100,
            });
        }
    }
}

