using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class AGGIntegrationServiceTests : BaseTest
    {
        private AGGIntegrationService ServiceInstance = null;

        public AGGIntegrationServiceTests()
        {
            ServiceInstance = new AGGIntegrationService();
        }

        [TestMethod]
        public void ConvertToAGGCurrencyObject_Test()
        {
            var result1 = ServiceInstance.ConvertToAGGCurrencyObject("MYR100");
            var result2 = ServiceInstance.ConvertToAGGCurrencyObject("MYR");
            var result3 = ServiceInstance.ConvertToAGGCurrencyObject("KVND");

            Assert.IsTrue(result1.currencyCode == "hMYR");
            Assert.IsTrue(result2.currencyCode == "MYR");
            Assert.IsTrue(result3.currencyCode == "KVND");


            Assert.IsTrue(result2.isIso4217);
            Assert.IsFalse(result1.isIso4217);
            Assert.IsFalse(result3.isIso4217);

            return;
        }



    }
}

