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
    public class ThirdPartyGameVendorTestFunctionTests : BaseTest
    {


        [TestMethod]
        public void TestAGGLive_Hash_Test()
        {
            //非公司開發電腦 不執行這組test
            if (IsNotDeveloperComputerName())
                return;

            var data = new
            {
                merchantid = "target-viet01",
                vcode = "audere",
                gameid = "niuniu",
                starttime = "2021-12-24 00:00:00",
                endtime = "2021-12-25 00:00:00",
                timezone = 8
            };

            string secretKey = "pdh2osQ3";
            var aggLiveService = new AGGLiveIntegrationService();
            var hashResult = aggLiveService.GenerateSignHash(data.ConvertToJsonString() , secretKey);

            return;
        }

    }
}

