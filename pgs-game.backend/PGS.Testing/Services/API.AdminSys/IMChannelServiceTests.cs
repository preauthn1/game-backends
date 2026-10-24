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
    public class IMChannelServiceTests : BaseTest
    {

        public IIMChannelService ServiceInstance { get; set; }

        public IPAddressListTemplateRepository IPAddressListTemplateRepository { get; set; }



        [TestMethod]
        public void GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new IMChannelRequestParameterModel
            {
                PageSize = 500,
            });

            Assert_NotEmptyCollection(query.Items);

            foreach(var item in query.Items.OrderBy(x=> Guid.NewGuid()).ToList())
            {
                var setting = ServiceInstance.GetIMChannelCutomziedSettings(_TEST_DATA.Default.UserID, item.IMChannelID);
                Assert.IsTrue(setting.OwnerID == item.IMChannelID);
            }

            return;
        }


        [TestMethod]
        public void GetByPagingFilter_Filter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new IMChannelRequestParameterModel
            {
                SpecialFilterType = SpecialFilterTypeOfIMChannels.AssignProductionMerchantOnly.ToInteger(),
                PageSize = 500,
            });

            return;
        }



        [TestMethod]
        public void CreateNewIMChannel_Test()
        {
            string myName = $"MSTest_s1_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS();

            var result = ServiceInstance.Create(_TEST_DATA.Default.UserID, new IMChannelCreateRequestModel
            {
                ChannelName = myName,
                Type = 20000
            });

            //清除資料
            IPAddressListTemplateRepository.DeleteByNameKeyword_ForMSTest(myName);
            return;
        }


    }
}

