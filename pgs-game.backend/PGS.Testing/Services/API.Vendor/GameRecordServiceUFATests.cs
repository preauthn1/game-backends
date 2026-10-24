using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
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
    public class GameRecordServiceUFATests : BaseTest
    {

        public IGameRecordService GameRecordService { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        private static int MSTestDynamicChangeMerchantID = 0;


        public class MyMockGameRecordService: GameRecordService
        {
            protected override bool IsEnableQueryTop1BeforePagingQuery(int merchantID)
            {
                return merchantID == MSTestDynamicChangeMerchantID;
            }
        }



        [TestMethod]
        public void Get5MinutesSummaryReport_Test()
        {
            int pageSize = 20;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var query = GameRecordService.Get5MinutesSummaryReport(new UFA5MinutesReportRequestModel
            {
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = DateTimeHelper.GetCurrentTime().AddDays(-30),
                EndTime = DateTimeHelper.GetCurrentTime(),
                PageSize = pageSize
            });

            Assert.IsTrue(query.Items.Count() <= pageSize);

            if(query.Items.IsNotEmptyNull())
            {
                List<int> playerIDs = query.Items.Select(x => x.PlayerID).Distinct().ToList();

                var players = PlayerRepository.GetByIds(playerIDs);
                Assert.IsTrue(players.All(x => x.MerchantID == merchantID));
            }

            return;
        }


        [TestMethod]
        public void Get5MinutesSummaryReport_Fail_Test()
        {
            int pageSize = 20;
            AssertExpectExceptionProcess(() =>
            {
                var query = GameRecordService.Get5MinutesSummaryReport(new UFA5MinutesReportRequestModel
                {
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    MerchantSecretKey = "QQQQQQQXXXXXX",
                    StartTime = DateTimeHelper.GetCurrentTime().AddDays(-30),
                    EndTime = DateTimeHelper.GetCurrentTime(),
                    PageSize = pageSize
                });

            }, ApiErrorCode.NotFoundMerchant , ApiErrorCode.NotFoundMerchantOrWrongApiKey);
            return;
        }

        
        [TestMethod]
        public void IsReturnEmptyResultByPreExecureSmallQuery_Test()
        {
            var myService = new MyMockGameRecordService();

            DateTime begin = DateTimeHelper.GetCurrentTime().AddDays(-30);
            DateTime end = DateTimeHelper.GetCurrentTime();

            var query = PerGameDetailLogRepository.GetPagingByFilter(new QueryDetailLogRequestViewModel
            {
                GameStartTimeBegin = begin,
                GameStartTimeEnd = end
            });

            if (query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.FirstOrDefault();

                var queryFilter = new GameRecordRequestModel
                {
                    MerchantID = matchFirst.MerchantID,
                    StartTime = begin,
                    EndTime = end
                };

                //case 1. 無開啟商戶配置
                bool isReturnEmptyResult1 = myService.IsReturnEmptyResultByPreQueryTop1(queryFilter, (int)MerchantTypeEnum.Plaform_ProductionEnvironment);

                Assert.IsFalse(isReturnEmptyResult1);

                //case 2. 有開啟商戶配置 + 有資料
                MSTestDynamicChangeMerchantID = matchFirst.MerchantID;

                bool isReturnEmptyResult2 = myService.IsReturnEmptyResultByPreQueryTop1(queryFilter, (int)MerchantTypeEnum.Plaform_ProductionEnvironment);

                Assert.IsFalse(isReturnEmptyResult2);

                DateTime begin_2999 = new DateTime(2999, 1, 1);
                DateTime end_2999 = new DateTime(2999, 1, 1);

                var queryFilter2 = new GameRecordRequestModel
                {
                    MerchantID = matchFirst.MerchantID,
                    StartTime = begin_2999,
                    EndTime = end_2999
                };

                //case 3. 有開啟商戶配置 + 無資料
                bool isReturnEmptyResult3 = myService.IsReturnEmptyResultByPreQueryTop1(queryFilter2, (int)MerchantTypeEnum.Plaform_ProductionEnvironment);
                Assert.IsTrue(isReturnEmptyResult3);
            }

            return;
        }



    }
}

