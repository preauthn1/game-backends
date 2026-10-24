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
    public class AnnouncementServiceTests : BaseTest
    {

        public IAnnouncementService ServiceInstance { get; set; }

        public AnnouncementReadRepository AnnouncementReadRepository { get; set; }


        [TestMethod]
        public void GetNotReadAnnoucement_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;

            var query = ServiceInstance.GetNotReadAnnoucementList(playerID);

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.FirstOrDefault();
                ServiceInstance.SetMessageRead(playerID, new AnnouncementReadRequestModel
                {
                    AnnouncementID = matchFirst.AnnouncementID
                });

                //清空還原已讀
                AnnouncementReadRepository.DeleteByModel(new
                {
                    AnnouncementID = matchFirst.AnnouncementID
                });
            }

            return;
        }

    }
}

