using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class InternalUtilityServiceTests : BaseTest
    {

        public IInternalUtilityService ServiceInstance { get; set; }


        [TestMethod]
        public void GetMerchantServers_Test()
        {
            var query = ServiceInstance.GetMerchantServers(_TEST_DATA.Default.MerchantID);
            Assert_NotEmptyCollection(query);
            return;
        }


    }
}

