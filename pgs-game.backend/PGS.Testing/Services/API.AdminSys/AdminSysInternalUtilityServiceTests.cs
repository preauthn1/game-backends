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
    public class AdminSysInternalUtilityServiceTests : BaseTest
    {
        public IAdminSysInternalUtilityService ServiceInstance { get; set; }


        [TestMethod]
        public void GenerateSqlInsertCommand_Test()
        {
            var result = ServiceInstance.GenerateSqlInsertCommand(new LazyCommonRequestModel
            {
                TargetID = _TEST_DATA.Default.GameID_AGG,
                Table = "gameList",
            });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SQL);
            return;
        }


        [TestMethod]
        public void GetCommonFavoritSetting_Test()
        {
            var result = ServiceInstance.GetCommonFavoritSetting(new LazyCommonRequestModel
            {

            });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.FileName_Seamless_CN.IsNotEmptyTextNotNull());
            Assert.IsTrue(result.FileName_Seamless_EN.IsNotEmptyTextNotNull());
            return;
        }



        [TestMethod]
        public void GetFavoritSetting_Test()
        {
            var result = ServiceInstance.GetFavoritSetting(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {

            });

            Assert.IsNotNull(result);
            return;
        }


    }
}

