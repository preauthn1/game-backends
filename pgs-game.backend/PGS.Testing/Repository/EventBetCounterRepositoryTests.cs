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
    public class EventBetCounterRepositoryTests : BaseTest
    {

        public EventBetCounterRepository EventBetCounterRepository { get; set; }


        [TestMethod]
        public void EventCounterValuePlus_Test()
        {
            decimal plusValue = RandomHelper.GetRandIntegerNumbers(100 , 199);

            var result1 = EventBetCounterRepository.EventCounterValuePlus(new Models.Table.EventCounterValuePlusSPParameter
            {
                BetAmount = plusValue,
                EventID = _TEST_DATA.NotSuperMerchant.EventID_LuckyWheel,
                EventPoint = plusValue,
                GameID = 0,
                PayoutAmount = plusValue,
                PlayerID = _TEST_DATA.NotSuperMerchant.MerchantID
            });

            var result2 = EventBetCounterRepository.EventCounterValuePlus(new Models.Table.EventCounterValuePlusSPParameter
            {
                BetAmount = plusValue,
                EventID = _TEST_DATA.NotSuperMerchant.EventID_LuckyWheel,
                EventPoint = plusValue,
                GameID = 0,
                PayoutAmount = plusValue,
                PlayerID = _TEST_DATA.NotSuperMerchant.MerchantID
            });

            Assert.IsNotNull(result1);
            Assert.IsTrue(result1.AfterTotalPoint >= plusValue);
            Assert.IsTrue(result1.AfterTotalBetAmount >= plusValue);
            Assert.IsTrue(result1.AfterTotalPayoutAmount >= plusValue);

            Assert.IsTrue(result2.AfterTotalPoint == result1.AfterTotalPoint + plusValue);
            Assert.IsTrue(result2.AfterTotalBetAmount == result1.AfterTotalBetAmount + plusValue);
            Assert.IsTrue(result2.AfterTotalPayoutAmount == result1.AfterTotalPayoutAmount + plusValue);
            return;
        }

    }
}

