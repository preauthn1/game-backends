using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
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
    public class BetRecordServiceTests : BaseTest
    {

        public IBetRecordService ServiceInstance { get; set; }

        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public BetRecordMerchantNotifyQueueRepository BetRecordMerchantNotifyQueueRepository { get; set; }


        [TestMethod]
        public void BetRecordService_GetUnionBetRecordsByPagingFilter_Test()
        {
            int gameVendor = GameVendorTypeEnum.AGG.ToInteger();
            var query = ServiceInstance.GetUnionBetRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryUnionBetRecordRequestParameterModel
            {
                GameVendorID = gameVendor,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                PageSize = 1
            });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Items);

            if (query.Items.IsNotEmptyNull())
            {
                var firstMatch = query.Items.FirstOrDefault();
                var query2 = ServiceInstance.GetUnionBetRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryUnionBetRecordRequestParameterModel
                {
                    GameVendorID = gameVendor,
                    TransactionNoPGS = firstMatch.BetRecordNoPGS,
                    PageSize = 1
                });

                Assert_NotEmptyCollection(query2.Items);
            }

            return;
        }



        [TestMethod]
        public void BetRecordService_GetUnionBetRecordsByPagingFilter_AGGLive_Test()
        {
            var query = ServiceInstance.GetUnionBetRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryUnionBetRecordRequestParameterModel
            {
                GameVendorID = GameVendorTypeEnum.AGGLive.ToInteger(),
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMonths(-1),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                PageSize = 1
            });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void RecoverNotifyQueueData_Test()
        {
            var query = ServiceInstance.GetUnionBetRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryUnionBetRecordRequestParameterModel
            {
                GameVendorID = GameVendorTypeEnum.AGG.ToInteger(),
                MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                Status = BetRecordStatusEnum.SettleComplete.ToInteger(),
            });

            if (query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.OrderBy(x => Guid.NewGuid().GetHashCode()).FirstOrDefault();
                ServiceInstance.RecoverNotifyQueueData(_TEST_DATA.Default.UserID, matchFirst.BetRecordNoPGS);
            }

            return;
        }


        [TestMethod]
        public void BetRecordService_GetUnionBetRecordsByPagingFilter_PGS_Test()
        {
            var query = ServiceInstance.GetUnionBetRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryUnionBetRecordRequestParameterModel
            {
                GameVendorID = GameVendorTypeEnum.target.ToInteger(),
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                PageSize = 1
            });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Items);
            return;
        }



        [TestMethod]
        public void BetRecordService_GetUnionBetRecordsByPagingFilter_Exception_Test()
        {
            try
            {
                var query = ServiceInstance.GetUnionBetRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryUnionBetRecordRequestParameterModel
                {
                    CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMonths(-1),
                    PageSize = 1
                });
            }
            catch (Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.InvalidParameter_NumberValueNotAllowNull);
                return;
            }

            Assert.Fail("unexpected");
            return;
        }



        [TestMethod]
        public void BetRecordService_GetUnionBetRecordsByPagingFilter_FilterGameVendor_Test()
        {
            int targetVendorID = GameVendorTypeEnum.AGG.ToInteger();
            var query = ServiceInstance.GetUnionBetRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryUnionBetRecordRequestParameterModel
            {
                PageSize = 10,
                GameVendorID = targetVendorID,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime()
            });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Items);
            Assert.IsTrue(query.Items.All(x => x.GameVendor == targetVendorID));
            return;
        }


        [TestMethod]
        public void GetByTransactionNoForPerGamePage_Test()
        {
            var now = DateTimeHelper.GetCurrentTime();

            var queryPerGame = PerGameDetailLogRepository.GetPagingByFilter(new QueryDetailLogRequestViewModel
            {
                GameStartTimeBegin = now.AddDays(-30),
                GameStartTimeEnd = now,
                GameVendorID = GameVendorTypeEnum.AGG.ToInteger(),
                PageSize = 100
            });

            if (queryPerGame.Items.IsNotEmptyNull())
            {
                var firstMatch = queryPerGame.Items.FirstOrDefault();

                var query = ServiceInstance.GetByTransactionNo_ForPerGamePage(_TEST_DATA.Default.UserID, new BetRecordQueryRequestModel
                {
                    TransactionNo = firstMatch.TransactionNo
                });

                Assert.IsNotNull(query);
                Assert.IsNotNull(query.GeneralInfo);
            }

            return;
        }



        [TestMethod]
        public void GetByTransactionNo_Test()
        {
            var queryList = PerGameDetailLogRepository.GetPagingByFilter(new QueryDetailLogRequestViewModel
            {
                OrderByColumn = " GameTime ",
                PageSize = 20,
            });

            //太新的遊戲商不要抓 有時候會有錯誤
            List<int> targetGameVendors = new List<int>
            {
                GameVendorTypeEnum.AGG.ToInteger(),
                GameVendorTypeEnum.Funky.ToInteger(),
                GameVendorTypeEnum.Jili.ToInteger(),
                GameVendorTypeEnum.AT.ToInteger(),
            };

            var thirdGames = queryList.Items
                    .Where(x => x.TransactionNo.IsNotEmptyNull())
                    .Where(x => !AppHelper.IsPgsSelfGame(x.GameID))
                    .Where(x => targetGameVendors.Contains(x.GameID))
                    .Where(x => x.BetAmount > 0)
                    .ToList();

            if (thirdGames.IsNotEmptyNull())
            {
                var matchFirst = thirdGames
                    .OrderByRandom()
                    .FirstOrDefault();

                var query = ServiceInstance.GetByTransactionNo(_TEST_DATA.Default.UserID, new BetRecordQueryRequestModel
                {
                    TransactionNo = matchFirst.TransactionNo
                });

                Assert.IsNotNull(query);
            }

            return;
        }



        [TestMethod]
        public void ManualRefundBetRecord_Test()
        {
            string betRecordNoPGS = _TEST_DATA.Default.TransactionNo_ForRefund;
            var queryExist = BetRecordAGGRepository.GetByBetRecordNoPGS(betRecordNoPGS);
            queryExist.Status = BetRecordStatusEnum.Pending.ToInteger();

            //重製狀態
            BetRecordAGGRepository.Update(queryExist);

            //執行退款
            ServiceInstance.ManualRefundBetRecord(_TEST_DATA.Default.UserID, new BetRecordManualRefundRequestModel
            {
                TransactionNo = betRecordNoPGS
            });

            var queryAgain = BetRecordAGGRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            Assert.IsTrue(queryAgain.Status == BetRecordStatusEnum.Cancel.ToInteger());
            return;
        }


        [TestMethod]
        public void ManualSettleBetRecord_Test()
        {
            string betRecordNoPGS = _TEST_DATA.Default.TransactionNo_ForManualSettle;
            var queryExist = BetRecordAGGRepository.GetByBetRecordNoPGS(betRecordNoPGS);
            queryExist.Status = BetRecordStatusEnum.Pending.ToInteger();

            //重製狀態
            BetRecordAGGRepository.Update(queryExist);

            decimal rndAmount = RandomHelper.GetRandIntegerNumbers(100, 200);

            //手動結算
            ServiceInstance.ManualSettleBetRecord(_TEST_DATA.Default.UserID, new BetRecordManualSettleRequestModel
            {
                Memo = "MSTest",
                PayoutAmount = rndAmount,
                ValidAmount = rndAmount,
                TransactionNo = betRecordNoPGS,
            });

            var queryAgain = BetRecordAGGRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            Assert.IsTrue(queryAgain.PayoutAmount == rndAmount);
            Assert.IsTrue(queryAgain.ValidAmount == rndAmount);
            Assert.IsTrue(queryAgain.Status == BetRecordStatusEnum.SettleComplete.ToInteger());
            return;
        }



        [TestMethod]
        public void ManualDeletePerGameLog_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var query = ServiceInstance.GetUnionBetRecordsByPagingFilter(userID, new QueryUnionBetRecordRequestParameterModel
            {
                GameVendorID = GameVendorTypeEnum.target.ToInteger(),
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-7),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                Status = BetRecordStatusEnum.SettleComplete.ToInteger(),
                PageSize = 10
            });

            if (query.Items.IsEmptyOrNull())
                return;

            int otherPlayerID = _TEST_DATA.SingleWallet.PlayerID;

            var matchFirst = query.Items
                .Where(x => x.PlayerID != otherPlayerID)
                .Where(x => x.GameID <= 2232 || x.GameID > 9999)  //部分新遊戲不穩定
                .OrderByRandom()
                .FirstOrDefault();

            if (matchFirst == null)
                return;

            ServiceInstance.ManualDeletePerGameLog(userID, new BetRecordManualDeletePerGameRequestModel
            {
                ReplaceToThisPlayerID = otherPlayerID,
                ListOfTransactionNoPGS = new List<string>
                {
                    matchFirst.BetRecordNoPGS
                }
            });

            var queryAgain = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(matchFirst.BetRecordNoPGS);

            Assert.IsTrue(queryAgain.PlayerName != matchFirst.PlayerName);
            Assert.IsTrue(queryAgain.PlayerID == otherPlayerID);
            return;
        }



        [TestMethod]
        public void GetNotifyQueueInfo_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var query = BetRecordMerchantNotifyQueueRepository.GetPagingByFilter(new QueryBetRecordMerchantNotifyQueueRequestViewModel
            {
                PageSize = 5,
            });

            if (query.Items.IsEmptyOrNull())
                return;

            var matchFirst = query.Items.FirstOrDefault();

            var queryQueue = ServiceInstance.GetNotifyQueueInfo(userID, matchFirst.Id);
            Assert.IsTrue(queryQueue != null);
            return;
        }


        [TestMethod]
        public void FindNotSettleRecords_Test()
        {
            var query1 = ServiceInstance.FindNotSettleRecords(_TEST_DATA.Default.UserID, new FindNotSettleRecordsRequestModel
            {
                GameVendorID = GameVendorTypeEnum.AGG.ToInteger(),
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-3),
                EndTime = DateTimeHelper.GetCurrentTime(),
            });

            var query2 = ServiceInstance.FindNotSettleRecords(_TEST_DATA.Default.UserID, new FindNotSettleRecordsRequestModel
            {
                GameVendorID = GameVendorTypeEnum.AGGLive.ToInteger(),
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-3),
                EndTime = DateTimeHelper.GetCurrentTime(),
            });

            var query3 = ServiceInstance.FindNotSettleRecords(_TEST_DATA.Default.UserID, new FindNotSettleRecordsRequestModel
            {
                GameVendorID = GameVendorTypeEnum.target.ToInteger(),
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-3),
                EndTime = DateTimeHelper.GetCurrentTime(),
            });

            Assert.IsTrue(query1 != null);
            Assert.IsTrue(query2 != null);
            Assert.IsTrue(query3 != null);
            return;
        }



        [TestMethod]
        public void GetBetRecordDebugSQLAndApiReqeustLog_Test()
        {
            var query = ServiceInstance.GetUnionBetRecordsByPagingFilter(_TEST_DATA.Default.UserID, new QueryUnionBetRecordRequestParameterModel
            {
                GameVendorID = GameVendorTypeEnum.AGG.ToInteger(),
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                PageSize = 20,
            });

            if (query.Items.IsNotEmptyNull())
            {
                var query1 = ServiceInstance.GetBetRecordDebugSQLAndApiReqeustLog(_TEST_DATA.Default.UserID, query.Items.FirstOrDefault()?.BetRecordNoPGS);

                Assert.IsTrue(query1 != null);
                Assert.IsTrue(query1.DebugSQL.IsNotEmptyTextNotNull());
            }

            return;
        }



    }
}

