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

namespace target.Testing
{
    [TestClass]
    public class PlayerSuspiciousCheckServiceTests : BaseTest
    {

        public IPlayerSuspiciousCheckService ServiceInstance { get; set; }



        [TestMethod]
        public void CheckPlayerDataIsSuspicious_Test()
        {
            var query1 = ServiceInstance.CheckPlayerDataIsSuspicious(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 60),
                End = DateTimeHelper.GetCurrentTime(),
                PlayerID = _TEST_DATA.Default.PlayerID,
            });

            var query2 = ServiceInstance.CheckPlayerDataIsSuspicious(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1* 60),
                End = DateTimeHelper.GetCurrentTime(),
                PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
            });

            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);
            Assert.IsTrue(query2.GameSummary.IsNotEmptyNull());
            return;
        }


        [TestMethod]
        public void CheckPlayerLiveCasinoGameSuspicious_Test()
        {
            var query1 = ServiceInstance.CheckPlayerLiveCasinoGameSuspicious(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 13),
                End = DateTimeHelper.GetCurrentTime(),
                PlayerID = _TEST_DATA.Default.PlayerID,
            });

            var query2 = ServiceInstance.CheckPlayerLiveCasinoGameSuspicious(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 13),
                End = DateTimeHelper.GetCurrentTime(),
                PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
            });

            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);
           // Assert.IsTrue(query2.GameSummary.IsNotEmptyNull());
            return;
        }

    }
}

