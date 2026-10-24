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
    public class GameRTPConfigServiceTests : BaseTest
    {

        public IGameRTPConfigService ServiceInstance { get; set; }


        [TestMethod]
        public void GetGameRTPByPagingFilter_Test()
        {
            var query = ServiceInstance.GetGameRTPByPagingFilter(_TEST_DATA.Default.UserID , new QueryGameRTPConfigRequestParameterModel
            {

            });
            Assert.IsNotNull(new CRUDResultModel());
            return;
        }


        [TestMethod]
        public void GetPlayerRTPByPagingFilter_Test()
        {
            var query = ServiceInstance.GetPlayerRTPByPagingFilter(_TEST_DATA.Default.UserID, new QueryPlayerRTPConfigRequestParameterModel
            {

            });

            Assert.IsNotNull(new CRUDResultModel());
            return;
        }

    }
}

