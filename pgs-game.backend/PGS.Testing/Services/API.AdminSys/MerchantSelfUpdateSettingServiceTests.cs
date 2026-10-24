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
    public class MerchantSelfUpdateSettingServiceTests : BaseTest
    {
        public IMerchantSelfUpdateSettingService ServiceInstance { get; set; }

        [TestMethod]
        public void GetMerchantAvailableSelfUpdateSettings_Test()
        {
            var query = ServiceInstance.GetMerchantAvailableSelfUpdateSettings(_TEST_DATA.Default.UserID , _TEST_DATA.Default.MerchantID);
            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void UpdateMerchantLobbyVisibleGames_Test()
        {
            int merchantID = _TEST_DATA.NotSuperMerchant.MerchantID;
            ServiceInstance.UpdateMerchantLobbyVisibleGames(_TEST_DATA.Default.UserID , new MerchantSelfUpdateSettingViewModel
            {
                MerchantID = merchantID,
                LobbyVisibleGames = new List<MerchantGameFlagSelfSettingViewModel>
                {
                    new MerchantGameFlagSelfSettingViewModel
                    {
                        GameID = _TEST_DATA.Default.GameID,
                        IsEnable = false
                    }
                }
            });


            ServiceInstance.UpdateMerchantLobbyVisibleGames(_TEST_DATA.Default.UserID, new MerchantSelfUpdateSettingViewModel
            {
                MerchantID = merchantID,
                LobbyVisibleGames = new List<MerchantGameFlagSelfSettingViewModel>
                {
                    new MerchantGameFlagSelfSettingViewModel
                    {
                        GameID = _TEST_DATA.Default.GameID,
                        IsEnable = true
                    }
                }
            });

            return;
        }


    }
}

