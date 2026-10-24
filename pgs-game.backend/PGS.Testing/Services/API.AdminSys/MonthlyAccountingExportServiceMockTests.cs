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
    public class MonthlyAccountingExportServiceMockTests : BaseTest
    {
        public AccountingGroupRepository AccountingGroupRepository { get; set; }


        private static DateTime DefaultDate = new DateTime(2000, 1, 1);

        private static int _MerchantID_1 = _TEST_DATA.Default.MerchantID;
        private static int _MerchantID_2 = _TEST_DATA.NotSuperMerchant.MerchantID;

        private static int RandomNumber1 = RandomHelper.GetRandIntegerNumbers(10000, 20000);
        private static int RandomNumber2 = RandomHelper.GetRandIntegerNumbers(10000, 20000);
        private static int RandomNumber3 = RandomHelper.GetRandIntegerNumbers(10000, 20000);


        public class MonthlyAccountingExportMockService : MonthlyAccountingExportServiceV2
        {
            protected override void PreQueryAllDailyWinLoss(double timezone, DateTime begin, DateTime end)
            {
                int gameID_AGG = GameVendorTypeEnum.AGG.ToInteger() + 1;
                int gameID_Funky = GameVendorTypeEnum.Funky.ToInteger() + 1;

                int merchant_1 = _MerchantID_1;
                int merchant_2 = _MerchantID_2;


                _preQueryDailyWinlossByTimezone.Add(8, new List<MerchantGameSummaryDataEntity>
                {
                    new MerchantGameSummaryDataEntity {

                        MerchantID = merchant_1,
                        StatTime = DefaultDate,
                        BetAmount = 1,
                        PayoutAmount = 0,
                        WinLossAmount = RandomNumber1,
                        GameID = gameID_AGG,
                    },
                     new MerchantGameSummaryDataEntity {

                        MerchantID = merchant_1,
                        StatTime = DefaultDate.AddDays(1),
                        BetAmount = 2,
                        PayoutAmount = 0,
                        WinLossAmount = RandomNumber2,
                        GameID = gameID_AGG,
                    },

                      new MerchantGameSummaryDataEntity {

                        MerchantID = merchant_1,
                        StatTime =  DefaultDate.AddDays(2),
                        BetAmount = 3,
                        PayoutAmount = 0,
                        WinLossAmount = RandomNumber3,
                        GameID = gameID_AGG,
                    },

                     new MerchantGameSummaryDataEntity {

                        MerchantID = merchant_1,
                        StatTime = DefaultDate,
                        BetAmount = 1,
                        PayoutAmount = 0,
                        WinLossAmount = RandomNumber1 * 2,
                        GameID = gameID_Funky,
                    },

                    new MerchantGameSummaryDataEntity {

                        MerchantID = merchant_1,
                        StatTime = DefaultDate.AddDays(1),
                        BetAmount = 2,
                        PayoutAmount = 0,
                        WinLossAmount = RandomNumber2 * 2,
                        GameID = gameID_Funky,
                    },

                    new MerchantGameSummaryDataEntity {

                        MerchantID = merchant_1,
                        StatTime =  DefaultDate.AddDays(2),
                        BetAmount = 3,
                        PayoutAmount = 0,
                        WinLossAmount = RandomNumber3 * 2,
                        GameID = gameID_Funky,
                    },

                    new MerchantGameSummaryDataEntity {

                        MerchantID = merchant_2,
                        StatTime =  DefaultDate.AddDays(2),
                        BetAmount = 3,
                        PayoutAmount = 0,
                        WinLossAmount = RandomNumber3 * 2,
                        GameID = gameID_Funky,
                    }

                });
                return;
            }

            protected override List<EachMerchantEventPrizeTotalSummaryModel> QueryEachMerchantEventPrizeTotalsum(List<int> merchantIDs, DateTime begin, DateTime end)
            {
                return new List<EachMerchantEventPrizeTotalSummaryModel>
                {
                    new EachMerchantEventPrizeTotalSummaryModel
                    {
                        MerchantID =  _MerchantID_1,
                        UpdateAmount = RandomNumber1,
                        EventID = 123,
                        EventPrizeType = EventPrizeTypeEnum.DrawCash_ByDrawPrizeSetting.ToInteger(),
                    },
                    new EachMerchantEventPrizeTotalSummaryModel
                    {
                        MerchantID =  _MerchantID_2,
                        UpdateAmount = RandomNumber2 * 2,
                        EventID = 123,
                        EventPrizeType = EventPrizeTypeEnum.DrawCash_ByDrawPrizeSetting.ToInteger(),
                    }
                };
            }
        }



        /// <summary>
        /// 透過MockService 餵一點假資料
        /// </summary>
        [TestMethod]
        public void GetMonthlyAccountingReport_Test()
        {
            var mockService = new MonthlyAccountingExportMockService();

            var queryGroup = AccountingGroupRepository.GetByFilter(new QueryAccountingGroupRequestViewModel
                {

                })
                .Where(x => x.Name.ContainsWihtoutCase("MSTest"))
                .FirstOrDefault();

            mockService.GetMonthlyAccountingReport(new AccountingSettingGroupRequestModel
            {
                AccountingGroupID = queryGroup.AccountingGroupID,
                TargetMonth = DefaultDate,
                IsPreQuerySummary = true
            });

            //分別查詢商戶1 商戶2
            var matchMerchant = mockService._eachAggregativeMerchantAccountingRows
                .Where(x => x.MerchantID == _MerchantID_1)
                .FirstOrDefault();

            var merchant_2_data = mockService._eachAggregativeMerchantAccountingRows
                .Where(x => x.MerchantID == _MerchantID_2)
                .FirstOrDefault();

            var totalWinloss_AGG = matchMerchant.ReferMerchantDetail.WinLossItems
                .Where(x=> x.GameVendorID == GameVendorTypeEnum.AGG.ToInteger())
                .Select(z => z.MerchantProfitWinLossAmount)
                .Sum();

            var totalWinloss_Funky = matchMerchant.ReferMerchantDetail.WinLossItems
                .Where(x => x.GameVendorID == GameVendorTypeEnum.Funky.ToInteger())
                .Select(z => z.MerchantProfitWinLossAmount)
                .Sum();

            Assert.IsTrue(totalWinloss_AGG * -1 == (RandomNumber1 + RandomNumber2 + RandomNumber3));
            Assert.IsTrue(totalWinloss_Funky * -1 == (RandomNumber1 + RandomNumber2 + RandomNumber3) * 2);

            var totalEventCost_DefaultMerchant = matchMerchant.ReferMerchantDetail
                .PromotionCostItems
                .Select(z => z.UpdateAmount)
                .Sum();

            var totalEventCost_Merchant2 = merchant_2_data.ReferMerchantDetail
                .PromotionCostItems
                .Select(z => z.UpdateAmount)
                .Sum();

            var totalEventCost_all = mockService
                ._eachAggregativeMerchantAccountingRows
                .Select(z=> z.ReferMerchantDetail)
                .SelectMany(z => z.PromotionCostItems)
                .Select(z => z.UpdateAmount)
                .Sum();

            decimal marchant2_value = RandomNumber2 * 2;

            Assert.IsTrue(totalEventCost_DefaultMerchant == RandomNumber1);
            Assert.IsTrue(totalEventCost_Merchant2 == marchant2_value);
            Assert.IsTrue(totalEventCost_all == RandomNumber1 + marchant2_value);
            return;
        }






    }
}

