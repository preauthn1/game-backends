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
    public class PlayerRTPChangeRefServiceTests : BaseTest
    {

        public PlayerRTPChangeRefService ServiceInstance { get; set; }


        public class PlayerRTPChangeRefMockService : PlayerRTPChangeRefService
        {
            protected override ApiResultCheckModel CallApiToUpdateRTP(PlayerRTPChangeRequestModel model)
            {
                return new ApiResultCheckModel
                {
                    IsSuccess = true
                };
            }


            protected override ApiResultCheckModel CallApiToResetRTP(PlayerRTPResetRequestModel model)
            {
                return new ApiResultCheckModel
                {
                    IsSuccess = true
                };
            }

        }


        [TestMethod]
        public void WhenEnterGameChangeRTP_Test()
        {
            var mockService = new PlayerRTPChangeRefMockService();

            var result1 = mockService.WhenEnterGameChangeRTP(new PlayerRTPChangeWhenEnterGameParameter
            {
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
                GameID = _TEST_DATA.ExternalGameVendor.GameID,
            });

            var result2 = mockService.WhenEnterGameChangeRTP(new PlayerRTPChangeWhenEnterGameParameter
            {
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
                GameID = 290088,
            });

            Assert.IsTrue(result1.NewRTP > 80);
            Assert.IsTrue(result2.NewRTP > 80);
            return;
        }



        [TestMethod]
        public void UpdatePlayerGameRTP_Test()
        {
            var mockService = new PlayerRTPChangeRefMockService();

            mockService.UpdatePlayerGameRTP(new PlayerRTPChangeRequestModel
            {
                PlayerID = _TEST_DATA.Default.PlayerID,
                GameID = _TEST_DATA.ExternalGameVendor.GameID,
                RTP = 100
            });

            mockService.ResetPlayerGameRTP(new PlayerRTPResetRequestModel
            {
                PlayerID = _TEST_DATA.Default.PlayerID,
                GameID = _TEST_DATA.ExternalGameVendor.GameID,
            });

            return;
        }



    }
}

