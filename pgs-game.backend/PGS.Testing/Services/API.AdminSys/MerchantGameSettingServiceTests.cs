using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;
using System.Collections.Generic;
using System;

namespace target.Testing
{
    [TestClass]
    public class MerchantGameSettingServiceTests : BaseTest
    {

        public IMerchantGameSettingService ServiceInstance { get; set; }


        [TestMethod]
        public void GetGameSettingByMerchantID_Test()
        {
            var query = ServiceInstance.GetGameSettingByMerchantID(_TEST_DATA.Default.MerchantID);

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.GameSettingItems);
            return;
        }


        [TestMethod]
        public void CopySetting_Test()
        {
            ServiceInstance.CopySetting(_TEST_DATA.Default.UserID , new MerchantGameSettingCopyRequestModel
            {
                SourceMerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                CopyToMerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                IsDeleteOriginalGames = true
            });

            return;
        }



        [TestMethod]
        public void GetEnableGamesForSubAgents_Test()
        {
            var query1 = ServiceInstance.GetEnableGamesForSubAgents(_TEST_DATA.Default.UserID, _TEST_DATA.CreditAgent.MerchantID);
            var query2 = ServiceInstance.GetEnableGamesForSubAgents(_TEST_DATA.Default.UserID, _TEST_DATA.CreditAgent.Layer2_MerchantID);

            Assert.IsTrue(query1.AvailableGamesDropdown.IsNotEmptyNull());
            Assert.IsTrue(query2.AvailableGamesDropdown.IsNotEmptyNull());
            return;
        }



        [TestMethod]
        public void GetGameSettingByMerchantIDForAgentSelfEdit_Test()
        {
            var query1 = ServiceInstance.GetGameSettingByMerchantIDForAgentSelfEdit(_TEST_DATA.Default.UserID, _TEST_DATA.CreditAgent.MerchantID);
            var query2 = ServiceInstance.GetGameSettingByMerchantIDForAgentSelfEdit(_TEST_DATA.Default.UserID, _TEST_DATA.CreditAgent.Layer2_MerchantID);

            Assert.IsTrue(query1.GameSettingItems.IsNotEmptyNull());
            Assert.IsTrue(query2.GameSettingItems.IsNotEmptyNull());
            return;
        }


        [TestMethod]
        public void BatchUpdateGameOpenFlagByAgentSelf_Test()
        {
            int userID = _TEST_DATA.Default.UserID;
            int targetMerchantID = _TEST_DATA.CreditAgent.Layer2_MerchantID;
            int targetMerchantID_sub = _TEST_DATA.CreditAgent.Layer3_MerchantID;

            var query1 = ServiceInstance.GetGameSettingByMerchantIDForAgentSelfEdit(userID, targetMerchantID);
            query1.SyncSubAgentIDs = new List<int>
            {
                targetMerchantID_sub
            };

            ServiceInstance.BatchUpdateGameOpenFlagByAgentSelf(userID, query1);

            var query2 = ServiceInstance.GetGameSettingByMerchantIDForAgentSelfEdit(userID, targetMerchantID);

            Assert.IsTrue(query1.GameSettingItems.Where(x => x.MerchantGameEnableFlag).Count() == query2.GameSettingItems.Where(x => x.MerchantGameEnableFlag).Count());
            return;
        }




    }
}

