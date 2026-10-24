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
    public class PlayerVendorServiceTests : BaseTest
    {
        public PlayerRepository PlayerRepository { get; set; }

        public IPlayerVendorService ServiceInstance { get; set; }


        [TestMethod]
        public void IsAccountExist_Test()
        {
            var query = ServiceInstance.IsAccountExist(new QueryPlayerAccountExistRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = _TEST_DATA.Default.PlayerName
            });

            Assert.IsTrue(query.IsExist);
            return;
        }




        [TestMethod]
        public void IsAccountExist_NotExist_Test()
        {
            var query = ServiceInstance.IsAccountExist(new QueryPlayerAccountExistRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = "tttttt666667777"
            });

            Assert.IsFalse(query.IsExist);
            return;
        }




        [TestMethod]
        public void ForcePlayerLogout_Test()
        {
            int merchantID = _TEST_DATA.Default.MerchantID;

            var queryPlayers = PlayerRepository.GetPagingByFilter(new QueryPlayerRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                PageSize = 5
            });

            var firstMatch = queryPlayers.Items.OrderByRandom().FirstOrDefault();

            var result = ServiceInstance.ForcePlayerLogout(new PlayerLogoutRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = firstMatch.PlayerName
            });

            return;
        }




    }
}

