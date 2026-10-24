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
    public class BOAnnouncementServiceTests : BaseTest
    {

        public IBOAnnouncementService ServiceInstance { get; set; }


        [TestMethod]
        public void BOAnnouncement_GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryBOAnnouncementApiRequestParameterModel());
            return;
        }


        [TestMethod]
        public void BOAnnouncement_GetByPagingFilter_User2_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.NotSuperMerchant.UserID, new QueryBOAnnouncementApiRequestParameterModel());
            return;
        }


        [TestMethod]
        public void BOAnnouncement_GetVisibleAnnouncements_Test()
        {
            var query1 = ServiceInstance.GetVisibleAnnouncements(_TEST_DATA.Default.UserID);
            var query2 = ServiceInstance.GetVisibleAnnouncements(_TEST_DATA.NotSuperMerchant.UserID);           
            return;
        }

        [TestMethod]
        public void BOAnnouncement_Create_Update_Delete_Test()
        {
            var insert = new BOAnnouncementCreateRequestModel
            {
                BeginTime = DateTimeHelper.GetCurrentTime(),
                EndTime = DateTimeHelper.GetCurrentTime().AddDays(1),
                Content = "Content",
                Title = "Title",
                HintType = BOAnnouncementHintTypeEnum.CommonText.ToInteger(),
                AssignParticipate = new AssignParticipateRequest
                {
                    AssignType = AssignTypeEnum.All.ToInteger(),
                }
            };

            var result1 = ServiceInstance.Create(_TEST_DATA.Default.UserID, insert);

            var update = insert.ReturnInjecteObject<BOAnnouncementUpdateRequestModel>();
            update.BOAnnouncementID = result1.ResultId;

            var result2 = ServiceInstance.Update(_TEST_DATA.Default.UserID, update);
            var result3 = ServiceInstance.Delete(_TEST_DATA.Default.UserID, update.BOAnnouncementID);
            return;
        }



        [TestMethod]
        public void BOAnnouncement_Create_Test()
        {
            var result1 = ServiceInstance.Create(_TEST_DATA.Default.UserID , new BOAnnouncementCreateRequestModel
            {
                Title = "MSTest",
                Content = "MSTest",
                Type = BOAnnouncementTypeEnum.Common.ToInteger(),
                HintType = BOAnnouncementHintTypeEnum.CommonText.ToInteger(),
                BeginTime = DateTimeHelper.GetCurrentTime(),
                EndTime = DateTimeHelper.GetCurrentTime().AddHours(24),
                AssignParticipate = new AssignParticipateRequest
                {
                    AssignType = AssignTypeEnum.All.ToInteger(),
                },
            });

            var query2 = ServiceInstance.Delete(_TEST_DATA.Default.UserID , result1.ResultId);
            return;
        }



    }

}

