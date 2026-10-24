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
    public class BetSummaryInfoServiceTests : BaseTest
    {

        public IBetSummaryInfoService ServiceInstance { get; set; }


        [TestMethod]
        public void GetBetSummaryByServer_Test()
        {
            var query = ServiceInstance.GetBetSummaryByServer(new BetSummaryRequestModel
            {
                ServerID = _TEST_DATA.Default.ServerID_2,
                Begin = DateTimeHelper.GetCurrentTime().AddHours(-8),
                End = DateTimeHelper.GetCurrentTime(),
            });

            Assert.IsNotNull(query);
            return;
        }

    }
}

