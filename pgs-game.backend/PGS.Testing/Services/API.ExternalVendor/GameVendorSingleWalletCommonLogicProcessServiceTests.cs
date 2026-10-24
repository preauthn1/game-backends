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
    public class GameVendorSingleWalletCommonLogicProcessServiceTests : BaseTest
    {
        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public EventPrizeRecordDetailRepository EventPrizeRecordDetailRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }


        /// <summary>
        /// 這隻是拿來產生json用的模板
        /// </summary>
        [TestMethod]
        public void IsAGGDelayTooMuchTime_Test()
        {
            var myService = new GameVendorSingleWalletCommonLogicProcessService();
            myService.InstanceAllRepositoryAndService(true, true);

            var param1 = new GameVendorWalletTransactionRequestParamter
            {
                EventPrizeInfo = new GameVendorConvertToEventPrizeInfoModel
                {
                    ReceiveTime = DateTimeHelper.GetCurrentTime(),
                }
            };

            var param2 = new GameVendorWalletTransactionRequestParamter
            {
                EventPrizeInfo = new GameVendorConvertToEventPrizeInfoModel
                {
                    ReceiveTime = new DateTime(2025 , 11 , 6),
                }
            };

            var q1 = myService.IsAGGDelayTooMuchTime(GameVendorTypeEnum.AGG, param1);
            Assert.IsTrue(q1.IsSpcialProcess == false);

            var q2 = myService.IsAGGDelayTooMuchTime(GameVendorTypeEnum.AGG, param2);
            Assert.IsTrue(q2.IsSpcialProcess == true);
            Assert.IsTrue(q2.Result.PgsTransactionNo.CountOrZero() == 32);

            var queryTag = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                BetRecordNoPGS = q2.Result.PgsTransactionNo,
                Type = EazyTempValueTypeEnum.AGGBonusWinSkipProcess.ToInteger()
            });

            Assert.IsTrue(queryTag.Count() == 1);
            return;
        }


        [TestMethod]
        public void CreateEventPrize_Test()
        {
            var myService = new GameVendorSingleWalletCommonLogicProcessService();
            myService.InstanceAllRepositoryAndService(true, true);

            var queryPlayer = PlayerRepository.GetBriefByID(_TEST_DATA.Default.PlayerID);

            string tranNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            int prizeType = EventPrizeTypeEnum.AGG_Freespin.ToInteger();

            decimal rndAmount = RandomHelper.GetRandIntegerNumbers(10, 20);
            var eventResult_001 = myService.CreateEventPrize(queryPlayer, new GameVendorConvertToEventPrizeInfoModel
            {
                Amount = rndAmount,
                EventPrizeType = prizeType,
                GameID = _TEST_DATA.Default.GameID_AGG,
                ReceiveTime = DateTimeHelper.GetCurrentTime(),
                ThirdOrderNo = "MSTest_" + Guid.NewGuid().ToString(),
                ReferValidAmount = rndAmount,
                ReferThirdPartyEventID = "MSTest_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS(),
            }, tranNo);

            Assert.IsTrue(eventResult_001.EventPrizeRecordID > 0);
            Assert.IsTrue(eventResult_001.PrizeRecord.EventPrizeType == prizeType);

            var queryInsertData = EventPrizeRecordDetailRepository.GetByFilter(new QueryEventPrizeRecordDetailRequestViewModel
                {
                    EventPrizeRecordIDs = eventResult_001.EventPrizeRecordID.ToListCollection()
                })
                .FirstOrDefault();

            Assert.IsTrue(queryInsertData != null);
            Assert.IsTrue(queryInsertData.ReferValidAmount == rndAmount);

            var eventResult_002 = myService.CreateEventPrize(queryPlayer, new GameVendorConvertToEventPrizeInfoModel
            {
                Amount = RandomHelper.GetRandIntegerNumbers(20 , 30),
                EventPrizeType = EventPrizeTypeEnum.AGG_ItemCard.ToInteger(),
                GameID = _TEST_DATA.Default.GameID_AGG,
                ReceiveTime = DateTimeHelper.GetCurrentTime(),
                ThirdOrderNo = "MSTest_" + Guid.NewGuid().ToString(),
                ReferValidAmount = 10,
                ReferThirdPartyEventID = "MSTest_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS(),
            }, tranNo);

            Assert.IsTrue(eventResult_002.EventPrizeRecordID > 0);

            return;
        }










    }
}

