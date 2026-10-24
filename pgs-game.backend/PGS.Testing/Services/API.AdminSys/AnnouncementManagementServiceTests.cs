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
    public class AnnouncementManagementServiceTests : BaseTest
    {

        public IAnnouncementManagementService ServiceInstance { get; set; }


        [TestMethod]
        public void GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryAnnouncementApiParameterModel
            {
                IsIncludeDeleteData = true
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void QuickCreate_UpdateDetail_Test()
        {
            int userID = _TEST_DATA.Default.UserID;
            var result = ServiceInstance.QuickCreateEmptyAnnoucement(userID, new AnnouncementCreateRequestModel
            {
                Memo = "MSTest_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS(),
                Type = AnnouncementTypeEnum.PopupAdvertisement.ToInteger()
            });

            int newID = result.ResultId;

            var queryDetail = ServiceInstance.GetDetail(userID, newID);

            queryDetail.AssignMerchants = new List<int>
            {
                _TEST_DATA.NotSuperMerchant.MerchantID
            };

            ServiceInstance.UpdateDetail(userID, queryDetail);

            ServiceInstance.Delete(userID, newID);
            return;
        }

    }
}

