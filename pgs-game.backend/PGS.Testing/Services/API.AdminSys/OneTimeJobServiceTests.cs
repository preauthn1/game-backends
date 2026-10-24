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
    public class OneTimeJobServiceTests : BaseTest
    {

        public IOneTimeJobManagementService ServiceInstance { get; set; }


        [TestMethod]
        public void GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryOneTimeJobApiRequestParameterModel());
            Assert.IsNotNull(query);
            return;
        }



        [TestMethod]
        public void Create_Test()
        {
            var query = ServiceInstance.Create(_TEST_DATA.Default.UserID, new OneTimeJobCreateRequestModel
            {
                SettingJson = "",
                IsAlwaysNotify = false,
                Type = OneTimesJobTypeEnum.SendMessageToNotifyChannel.ToInteger(),
                Memo = "MSTest_" +  Guid.NewGuid().ToString(),
                NextTime = DateTimeHelper.GetCurrentTime()
            });

            ServiceInstance.Delete(_TEST_DATA.Default.UserID, query.ResultLongId);
            Assert.IsNotNull(query);
            return;
        }


    }
}

