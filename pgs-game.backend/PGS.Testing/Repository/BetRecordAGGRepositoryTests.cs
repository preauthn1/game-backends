using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
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
    public class BetRecordAGGRepositoryTests : BaseTest
    {
        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }


        [TestMethod]
        public void BetRecordAGGRepository_GetByFilter_Test()
        {
            var query1 = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 10,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-7),
                Status = BetRecordStatusEnum.SettleComplete.ToInteger()
            });

            string selectColumns = $" {nameof(BetRecordAGGEntity.BetRecordNoPGS)} ";
            var query2 = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 10,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-7),
                Status = BetRecordStatusEnum.SettleComplete.ToInteger()

            } , selectColumns);

            return;
        }


        [TestMethod]
        public void BetRecordAGGRepository_Create_Update_DynamicParametersMode_Test()
        {
            var randomNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();

            var now = DateTimeHelper.GetCurrentTime();
            var time1 = DateTimeHelper.GetCurrentTime().AddSeconds(10);
            var time2 = new DateTime(time1.Year, time1.Month, time1.Day, time1.Hour, time1.Minute, time1.Second);

            var myEntity = new BetRecordAGGEntity
            {
                BetRecordNoPGS = randomNo.TransactionNo,
                BetRecordNo = Guid.NewGuid().ToString(),
                PartitionTime = randomNo.TimeWithHour,
                Status = BetRecordStatusEnum.Pending.ToInteger(),
                GameID = _TEST_DATA.Default.GameID_AGG,
                PlayerID = _TEST_DATA.Default.PlayerID,
                PlayerName = _TEST_DATA.Default.PlayerName,
                MerchantID = _TEST_DATA.Default.MerchantID,
                ConvertRate = 1,
                BetAmount = 1.5M,
                ValidAmount = 0.5M,
                CreateTime = now,
                UpdateTime = now,
                AmountBeforeBet = 0,
                VendorConfigID = 50004,
            };

            int gameVendor = AppHelper.GetGameVendorByGameID(myEntity.GameID);

            BetRecordTransactionNoMappingRepository.Create(myEntity.BetRecordNoPGS, myEntity.BetRecordNo, gameVendor);

            BetRecordAGGRepository.Create(myEntity);

            decimal myPayoutAmount = RandomHelper.GetRandIntegerNumbers(2, 10);
            myEntity.PayoutAmount = myPayoutAmount;
            myEntity.SettlementTime = time2;
            myEntity.ThirdSettleTime = time2;
            myEntity.Status = BetRecordStatusEnum.Cancel.ToInteger();

            BetRecordAGGRepository.UpdateToSettlementStatus(myEntity);

            var queryAgain = BetRecordAGGRepository.GetByBetRecordNoPGS(myEntity.BetRecordNoPGS);

            Assert.IsTrue(queryAgain != null);
            Assert.IsTrue(queryAgain.ThirdSettleTime == time2);
            Assert.IsTrue(queryAgain.PayoutAmount == myPayoutAmount);
            return;
        }



        [TestMethod]
        public void BetRecordAGGRepository_GetByFilter2_Test()
        {
            string selectColumns = " TOP 1 BetRecordNoPGS ";

            var query1 = BetRecordAGGRepository.GetByFilter(new QueryBetRecordGeneralRequestViewModel
            {

            } , selectColumns);

            Assert.IsTrue(query1.IsNotEmptyNull());
            return;
        }



        [TestMethod]
        public void BetRecordAGGRepository_UpdateToSettlementStatus_Test()
        {
            var query = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 1,
                PartitionTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 60),
                PartitionTimeEnd = DateTimeHelper.GetCurrentTime().AddDays(-1),
                OrderByColumn = " BetRecordNoPGS ",
                Status = BetRecordStatusEnum.SettleComplete.ToInteger(),
            });

            var matchFirst = query.Items.FirstOrDefault();
            var transactionNo = matchFirst.BetRecordNoPGS;
            matchFirst.Status = BetRecordStatusEnum.Pending.ToInteger();

            BetRecordAGGRepository.Update(matchFirst);

            matchFirst.PayoutAmount = RandomHelper.GetRandIntegerNumbers(10 , 50);
            matchFirst.Status = BetRecordStatusEnum.SettleComplete.ToInteger();

            BetRecordAGGRepository.UpdateToSettlementStatus(matchFirst);

            var queryAgain = BetRecordAGGRepository.GetByBetRecordNoPGS(transactionNo);

            BetRecordTagMappingRepository.CreateIfNotExist(transactionNo, SystemTagEnum.BetRecord_MSTestSpecialTag);

            Assert.IsTrue(matchFirst.Status == queryAgain.Status);
            Assert.IsTrue(matchFirst.PayoutAmount == queryAgain.PayoutAmount);
            return;
        }



        [TestMethod]
        public void GetByBetRecordNoPGS_Test()
        {
            var query = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 5,
                PartitionTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14),
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.FirstOrDefault();

                var queryAgain = BetRecordAGGRepository.GetByBetRecordNoPGS(matchFirst.BetRecordNoPGS);

                Assert.IsTrue(matchFirst.BetRecordNoPGS == queryAgain.BetRecordNoPGS);
                Assert.IsTrue(matchFirst.BetRecordNo == queryAgain.BetRecordNo);
                Assert.IsTrue(matchFirst.BetAmount == queryAgain.BetAmount);
                Assert.IsTrue(matchFirst.PlayerID == queryAgain.PlayerID);
            }

            return;
        }

    }
}

