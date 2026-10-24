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
using target.Utility.Helper.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class FishingIntegrationServiceTests : BaseTest
    {
        [TestMethod]
        public void Fishing_GenerateSignKey_Test()
        {
            /*
             {   
                "account" : "1145358",    
                "channel" : "cf_h5_crazyfish_os_f9",    
                "orderId" : "2000000782-2565",   
                "sign" : "47a5857d96d2b35e5b77bb3a44131e2e",    
                "stake" : "38.570000",    
                "token" : "d0c2abb4-fa0f-4b23-85d4-c4e8ede75a20" 
             }             
            */

            var para1 = new FishingSingleWalletPlaceBetRequestModel
            {
                Sign = "",
                Account = "1145358",
                Channel = "cf_h5_crazyfish_os_f9",
                OrderId = "2000000782-2565",
                Stake = 38.570000M,
                Token = [REDACTED]"
            };

            CheckSignHashExpected(para1, secretKey: "e3lmjAWnLl44FNfU", expectText: "47a5857d96d2b35e5b77bb3a44131e2e");


            //臨時測試用
            var para_test = new FishingSingleWalletPlaceBetRequestModel
            {
                Sign = "",
                Account = "1349794",
                Channel = "cf_h5_crazyfish_f9_thai",
                OrderId = "2000002365-1557",
                Stake = 1000.000000M,
                Token = [REDACTED]"
            };

            //有需要測試再自行
            //CheckSignHashExpected(para_test, secretKey: "u3sUfG8wWRJ3fQM1", expectText: "8b5b25c563a58a23b31cefe968e79577");

            return;
        }


        private void CheckSignHashExpected(object myObject, string secretKey, string expectText)
        {
            CrazyFishingIntegrationService ServiceInstance = new CrazyFishingIntegrationService();
            var hashResult = ServiceInstance.GenerateSignKey(myObject, secretKey);

            if (hashResult.AfterHash != expectText)
            {
                SignKeyRuleHelper.GenerateSignErrorMessage(myObject, hashResult, expectText, secretKey);
                Assert.IsTrue(hashResult.AfterHash == expectText);
            }

            return;
        }

    }
}

