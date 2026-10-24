using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
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
    public class EventProcessServiceTests : BaseTest
    {
        public EventBetCounterRepository EventBetCounterRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public IEventProcessService ServiceInstance { get; set; }


        /// <summary>
        /// 本地端臨時測試 投注結算
        /// </summary>
        [TestMethod]
        public void Local_Debug_Test_EventProcess()
        {
            if (IsNotDeveloperComputerName())
                return;

            var myService = new EventProcessService();

            //int playerID = 1017094;
            //int gameID = 290030;
            //var orderNo = "38870031bd4188524cc480d192faa2f4";
            //var now = DateTimeHelper.GetCurrentTime();

            int playerID = _TEST_DATA.Default.PlayerID;
            int gameID = 290030;
            var orderNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
            var now = DateTimeHelper.GetCurrentTime();

            var debugInfo = myService.WhenSettlement(new EventProcessModel
            {
                GameID = gameID,
                PlayerID = playerID,
                ExtraParameter = new ExtraEventProcessParameter
                {
                    BetRecordNoPGS = orderNo,
                    BetAmount = 100,
                    PayoutAmount = 100,
                    ValidAmount = 100,
                    SettleTime = now,
                    BetTime = now,
                }
            });

            return;
        }



        /// <summary>
        /// 臨時本地測試用
        /// </summary>
        [TestMethod]
        public void LocalTest_EventWhenSettlement()
        {
            if (IsNotDeveloperComputerName())
                return;

            int playerID = 1000000;
            int gameID = 290008;

            var debugInfo = ServiceInstance.WhenSettlement(new EventProcessModel
            {
                isThrowException = true,
                PlayerID = playerID,
                GameID = gameID,
                ExtraParameter = new ExtraEventProcessParameter
                {
                    BetAmount = 100,
                    PayoutAmount = 100
                }
            });

            return;
        }






        [TestMethod]
        public void WhenThirdPartyGameSettlement_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;

            //先清除資料
            EventBetCounterRepository.DeleteByModel(new
            {
                PlayerID = playerID,
                EventID = _TEST_DATA.NotSuperMerchant.EventID
            });

            var debugInfo = ServiceInstance.WhenSettlement(new EventProcessModel
            {
                isThrowException = true,
                PlayerID = playerID,
                GameID = _TEST_DATA.NotSuperMerchant.EventGameID,
                ExtraParameter = new ExtraEventProcessParameter
                {
                    BetAmount = 100,
                    PayoutAmount = 100
                }
            });

            return;
        }


        [TestMethod]
        public void EventProcessService_WhenGameEnter_Test()
        {
            int playerID = _TEST_DATA.Seldom_Use.PlayerID;

            //先清除領獎紀錄
            EventPrizeRecordRepository.DeleteByModel(new
            {
                PlayerID = playerID,
                EventID = _TEST_DATA.NotSuperMerchant.EventID
            });

            var debugInfo = ServiceInstance.WhenEnterGame(new EventProcessModel
            {
                isThrowException = true,
                PlayerID = playerID,
                GameID = _TEST_DATA.NotSuperMerchant.EventGameID,
                ExtraParameter = new ExtraEventProcessParameter
                {
                  
                }
            });

            //預計應該會領到獎品
            var queryExist = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection()
            });

            Assert.IsTrue(queryExist.IsNotEmptyNull());
            return;
        }


    }
}

