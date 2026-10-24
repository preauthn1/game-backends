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
    public class PlayerTagServiceTests : BaseTest
    {

        public IPlayerTagService ServiceInstance { get; set; }


        [TestMethod]
        public void DeleteAndCreateTag_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            int tagID = SystemTagEnum.Player_AllowSkipMaintain.ToInteger();

            ServiceInstance.Delete(_TEST_DATA.Default.UserID, new PlayerTagUpdateRequestModel
            {
                TagID = tagID,
                PlayerID = playerID
            });

            var query = ServiceInstance.Create(_TEST_DATA.Default.UserID , new PlayerTagCreateRequestModel
            {
                TagID = tagID,
                PlayerID = playerID
            });

            return;
        }

    }
}

