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
    public class AdminSysSimpleUpdateServiceTests : BaseTest
    {

        public IAdminSysSimpleUpdateService ServiceInstance { get; set; }


        [TestMethod]
        public void SimpleUpdateByParameter_PlayerMemo_Test()
        {
            ServiceInstance.SimpleUpdateByParameter(_TEST_DATA.Default.UserID , new SimpleUdpateRequestModel
            {
                UpdatePlayerMemo = new PlayerMemoUpdateColumnRequestModel
                {
                    PlayerID = _TEST_DATA.Default.PlayerID,
                    Memo = "MSTest"+ DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS()
                }
            });

            return;
        }

    }
}

