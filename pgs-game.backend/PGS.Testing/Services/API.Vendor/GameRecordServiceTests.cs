using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;
using System.Collections.Generic;
using System;
using target.Utility.IOC;
using target.BL.Services;

namespace target.Testing
{
    [TestClass]
    public class GameRecordServiceTests : BaseTest
    {
        public IStatReportService StatReportService { get; set; }

        public IGameRecordService ServiceInstance { get; set; }

        public BetRecordRepository BetRecordRepository { get; set; }

        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }


        [TestMethod]
        public void GetPlayerWinLossReport_Test()
        {
            var query = ServiceInstance.GetPlayerWinLossReport(new PlayerWinLossReportSummaryRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = DateTimeHelper.GetCurrentTime().AddDays(-60),
                EndTime = DateTimeHelper.GetCurrentTime(),
                PageSize = 10,
                PageIndex = 1
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void GetPlayerWinLossReport_PlayerName_Test()
        {
            var query = ServiceInstance.GetPlayerWinLossReport(new PlayerWinLossReportSummaryRequestModel
            {
                PlayerName = _TEST_DATA.Default.PlayerName,
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = DateTimeHelper.GetCurrentTime().AddDays(-60),
                EndTime = DateTimeHelper.GetCurrentTime(),
                PageSize = 10,
                PageIndex = 1
            });

            return;
        }



        [TestMethod]
        public void GetBetRecords_Test()
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var myFilter = new GameRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 10,
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = now.AddDays(30 * -1),
                EndTime = now
            };

            var query = ServiceInstance.GetBetRecords(myFilter);

            Assert_NotEmptyCollection(query.Items);

            if (query.Items.IsNotEmptyNull())
            {
                Assert.IsTrue(query.Items.All(x => x.BetTime >= myFilter.StartTime && x.BetTime < myFilter.EndTime));
                Assert.IsTrue(query.Items.All(x => x.RoundID != "0"));
            }

            return;
        }


        [TestMethod]
        public void GetBetRecords_Yesterday_Test()
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var myFilter = new GameRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 10,
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = now.AddDays(-1),
                EndTime = now
            };

            var query = ServiceInstance.GetBetRecords(myFilter);
            return;
        }


        [TestMethod]
        public void GetPagingByJoinBriefTable_Test()
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var myFilter = new QueryDetailLogRequestViewModel
            {
                PageIndex = 1,
                PageSize = 10,
                MerchantIDs = _TEST_DATA.Default.MerchantID.ToListCollection(),
                GameStartTimeBegin = now.AddDays(30 * -1),
                GameStartTimeEnd = now.AddMinutes(-5),
            };

            // 2024/5/1 才上線的調整
            if (myFilter.GameStartTimeBegin < new DateTime(2024, 5, 1))
            {
                myFilter.GameStartTimeBegin = new DateTime(2024, 5, 1);
            }

            var query1 = PerGameDetailLogRepository.GetPagingByJoinBriefTable(myFilter);
            var query2 = PerGameDetailLogRepository.GetPagingByFilter(myFilter);

            Assert.IsTrue(query1.TotalCount == query2.TotalCount);
            Assert.IsTrue(query1.Items.Count() == query1.Items.Count());

            if (query1.Items.IsNotEmptyNull())
            {
                Assert.IsTrue(query1.Items.Select(x => x.BetAmount).Sum() == query2.Items.Select(x => x.BetAmount).Sum());
                Assert.IsTrue(query1.Items.Select(x => x.UpdateAmount).Sum() == query2.Items.Select(x => x.UpdateAmount).Sum());
            }

            return;
        }


        [TestMethod]
        public void GetBetRecords_IsSearchExtraDetail_Test()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(_TEST_DATA.Default.Date_NewGameVendor_Intergrate))
                return;

            DateTime now = DateTimeHelper.GetCurrentTime();

            var query = ServiceInstance.GetBetRecords(new GameRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 200,
                IsSearchExtraDetail = true,
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = now.AddDays(30 * -1),
                EndTime = now
            });

            var debug001 = query.Items.Where(x => x.ExtraInfo != null).ToList();
            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void GetBetRecords_IsExchangeToOriginalRate_100Convert_Test()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(_TEST_DATA.Default.Date_NewGameVendor_Intergrate))
                return;

            DateTime now = DateTimeHelper.GetCurrentTime();

            var myFilter = new GameRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 200,
                IsSearchExtraDetail = true,
                MerchantID = _TEST_DATA.TransferRate100.MerchantID,
                MerchantSecretKey = _TEST_DATA.TransferRate100.MerchantSecret,
                StartTime = now.AddDays(60 * -1),
                EndTime = now
            };

            var query1 = ServiceInstance.GetBetRecords(myFilter);

            if (query1.Items.IsNotEmptyNull())
            {
                decimal expectRate = _TEST_DATA.TransferRate100.TransferConvertRate;

                var total1 = query1.Items.Select(x => x.BeforeAmount).Sum();
                var total2 = query1.Items.Select(x => x.WinLossAmount).Sum();
                var total3 = query1.Items.Select(x => x.ValidAmount).Sum();
                var total4 = query1.Items.Select(x => x.PayoutAmount).Sum();

                myFilter.IsExchangeToOriginalRate = true;

                var query2 = ServiceInstance.GetBetRecords(myFilter);

                var total2_1 = query2.Items.Select(x => x.BeforeAmount).Sum();
                var total2_2 = query2.Items.Select(x => x.WinLossAmount).Sum();
                var total2_3 = query2.Items.Select(x => x.ValidAmount).Sum();
                var total2_4 = query2.Items.Select(x => x.PayoutAmount).Sum();

                Assert.IsTrue(total1 == total2_1 * expectRate);
                Assert.IsTrue(total2 == total2_2 * expectRate);
                Assert.IsTrue(total3 == total2_3 * expectRate);
                Assert.IsTrue(total4 == total2_4 * expectRate);
            }

            return;
        }


        [TestMethod]
        public void GetBetRecords_ByPlayerName_Test()
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var query = ServiceInstance.GetBetRecords(new GameRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 10,
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                MerchantSecretKey = _TEST_DATA.NotSuperMerchant.MerchantSecret,
                PlayerName = _TEST_DATA.NotSuperMerchant.PlayerName,
                StartTime = now.AddDays(30 * -1),
                EndTime = now
            });

            Assert_NotEmptyCollection(query.Items);

            var query_Yesterday = ServiceInstance.GetBetRecords(new GameRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 10,
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                MerchantSecretKey = _TEST_DATA.NotSuperMerchant.MerchantSecret,
                PlayerName = _TEST_DATA.NotSuperMerchant.PlayerName,
                StartTime = now.AddDays(-1),
                EndTime = now
            });

            return;
        }



        [TestMethod]
        public void IsQueryByBriefJoinTable_Test()
        {
            var myService = new GameRecordService();

            var isQueryByBriefTable_1 = myService.IsQueryByBriefJoinTable(new GameRecordRequestModel
            {
                MerchantID = 175001,
                StartTime = DateTimeHelper.GetCurrentTime().AddHours(-1),
                EndTime = DateTimeHelper.GetCurrentTime()
            });

            var isQueryByBriefTable_201 = myService.IsQueryByBriefJoinTable(new GameRecordRequestModel
            {
                MerchantID = 175001,
                StartTime = DateTimeHelper.GetCurrentTime().AddHours(-1),
                EndTime = DateTimeHelper.GetCurrentTime(),
                PlayerName = "test123"
            });

            var isQueryByBriefTable_202 = myService.IsQueryByBriefJoinTable(new GameRecordRequestModel
            {
                MerchantID = 175001,
                StartTime = DateTimeHelper.GetCurrentTime().AddDays(-30),
                EndTime = DateTimeHelper.GetCurrentTime().AddDays(-29)
            });


            Assert.IsTrue(isQueryByBriefTable_1);
            Assert.IsFalse(isQueryByBriefTable_201);
            Assert.IsFalse(isQueryByBriefTable_202);

            return;
        }


        [TestMethod]
        public void GetPendingAndExprectCancelRecords_Test()
        {
            var begin = DateTimeHelper.GetCurrentTime().AddDays(-30);
            var end = DateTimeHelper.GetCurrentTime();

            var query = BetRecordRepository.GetPendingAndExprectCancelRecords(GameVendorTypeEnum.AGG, begin, end);
            return;
        }


        [TestMethod]
        public void GetRecordByGameVendorAndPgsRecordNo_Test()
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var queryAGGList = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PartitionTimeBegin = now.AddDays(-14),
                PartitionTimeEnd = now,
                PageSize = 10
            });

            if (queryAGGList.Items.IsEmptyOrNull())
            {
                var gameVendor = GameVendorTypeEnum.AGG;
                var matchFirst = queryAGGList.Items.FirstOrDefault();

                var query = BetRecordRepository.GetRecordByGameVendorAndPgsRecordNo(gameVendor.ToInteger(), matchFirst.BetRecordNoPGS);
                Assert.IsTrue(query.BetRecordNoPGS == matchFirst.BetRecordNoPGS);

                //順便測試另一個sql
                BetRecordRepository.ChangeSettleAmount(new BetRecordChangeSettleInfoParameter
                {
                    BetRecordNoPGS = query.BetRecordNoPGS,
                    GameVendorType = gameVendor.ToInteger(),
                    PayoutAmount = query.PayoutAmount,
                    TaxAmount = query.TaxAmount,
                    ValidAmount = query.ValidAmount,
                    WinLossAmount = query.WinLossAmount
                });
            }

            return;
        }


        [TestMethod]
        public void GetBetRecords_ByPlayerName_ExpectEmptyResult_Test()
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var query = ServiceInstance.GetBetRecords(new GameRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 10,
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                MerchantSecretKey = _TEST_DATA.NotSuperMerchant.MerchantSecret,
                PlayerName = "XXXXXXXXXQQQQQQQXXXXXXX",
                StartTime = now.AddDays(30 * -1),
                EndTime = now
            });

            Assert.IsTrue(query.TotalCount == 0);
            return;
        }



        [TestMethod]
        public void GetSingleWalletBetRecordByNo_NULL_Test()
        {
            var query = ServiceInstance.GetBetRecordByTransactionNo(new SingleWalletGameRecordRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                TransactionNo = "YYOOXX"
            });

            Assert.IsNull(query);
            return;
        }


        [TestMethod]
        public void GetSingleWalletBetRecordByNo_Test()
        {
            //清空其他測試注冊的mock
            IOCHelper.MockInstanceMapping = new Dictionary<string, object>();

            var query = ServiceInstance.GetBetRecordByTransactionNo(new SingleWalletGameRecordRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                TransactionNo = _TEST_DATA.Default.TransactionNo_PgsBet
            });

            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void GetReportSummary_Test()
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            var query = ServiceInstance.GetReportSummary(new VendorReportSummaryRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = now.AddDays(90 * -1),
                EndTime = now
            });

            Assert_NotEmptyCollection(query.GameSummary);
            return;
        }


        [TestMethod]
        public void Compare_VendorAPIReport_And_AdminAPIReport_Test()
        {
            int daysRange = 3;
            var previousDay_TW = DateTimeHelper.GetCurrentTime(8).AddDays(-1 * daysRange).Date;

            DateTime begin = previousDay_TW.AddHours(-1 * 8);
            DateTime end = begin.AddDays(daysRange);

            int merchantID = _TEST_DATA.Default.MerchantID;

            var queryVendorReport = ServiceInstance.GetReportSummary(new VendorReportSummaryRequestModel
            {
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = begin,
                EndTime = end
            });

            var queryAdminAPI = StatReportService.GetMerchantSummaryStatReport(_TEST_DATA.Default.UserID, new QueryMerchantSummaryStatReportReportApiRequestParameterModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                Begin = begin,
                End = end
            });

            Assert.IsTrue(queryAdminAPI.Merchants.Count() == 1);
            Assert.IsTrue(queryVendorReport != null);

            var matchStatReport = queryAdminAPI.Merchants.FirstOrDefault();

            Assert.IsTrue(matchStatReport.GameSummary.BetAmount == queryVendorReport.TotalBetAmount);
            Assert.IsTrue(matchStatReport.GameSummary.WinLossAmount == queryVendorReport.TotalWinLossAmount);
            return;
        }



        [TestMethod]
        public void GetFishGameDetailRecords_Test()
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            int pageSize = 10;
            var query = ServiceInstance.GetFishGameDetailRecords(new FishDetailRecordRequestModel
            {
                PageIndex = 1,
                PageSize = 10,
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                RoundSettleTimeBegin = now.AddDays(30 * -1),
                RoundSettleTimeEnd = now
            });

            Assert.IsTrue(query.Items.Count() <= pageSize);
            return;
        }





    }
}

