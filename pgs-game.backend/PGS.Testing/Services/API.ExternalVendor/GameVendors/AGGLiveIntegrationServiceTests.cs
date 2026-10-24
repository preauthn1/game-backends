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
    public class AGGLiveIntegrationServiceTests : BaseTest
    {
       

        [TestMethod]
        public void AGGLive_GenerateSignKey_Test()
        {
            var beforeObject1 = new
            {
                usertoken = [REDACTED]",
                sessiontoken = ""
            };

            CheckSignHashExpected
                (
                    beforeObject1,
                    secretKey: "pdh2osQ3",
                    expectText: "c347f621cc3b575e2b15a2fcad0e2994"
                );


            var beforeObject2 = new
            {
                usertoken = [REDACTED]",
                sessiontoken = ""
            };

            CheckSignHashExpected
              (
                  beforeObject2,
                  secretKey: "rg6c9sbad",
                  expectText: "2c5a8b536f72c5c92cd8626a3cf1d41e"
              );

            return;
        }


        private void CheckSignHashExpected(object myObject, string secretKey, string expectText)
        {
            AGGLiveIntegrationService ServiceInstance = new AGGLiveIntegrationService();

            string jsonText = myObject.ConvertToJsonString();

            var hashResult = ServiceInstance.GenerateSignHash(jsonText, secretKey);

            if(hashResult.AfterHash != expectText)
            {
                SignKeyRuleHelper.GenerateSignErrorMessage(myObject , hashResult , expectText , secretKey);
                Assert.IsTrue(hashResult.AfterHash == expectText);
            }

            return;

        }


    }
}

