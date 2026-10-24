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
    public class FakeWalletMockServiceTests : BaseTest
    {

        public IFakeWalletMockService ServiceInstance { get; set; }


        [TestMethod]
        public void FakeWalletMockService_UdpateProcess_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;

            decimal targetAmount = 100;

            var currentAmount = ServiceInstance.GetFakeWalletBalance(playerID);

            var aferAmount1 = ServiceInstance.UpdateFakeWalletAmount(playerID , targetAmount);

            var aferAmount2 = ServiceInstance.UpdateFakeWalletAmount(playerID, targetAmount * -1);

            Assert.IsTrue(currentAmount == aferAmount2);
            return;
        }

    }
}

