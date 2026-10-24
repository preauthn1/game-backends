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
    public class PlayerTagMappingRepositoryTests : BaseTest
    {

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }


        [TestMethod]
        public void IsPlayerTagExistByCache_Test()
        {
            var query = PlayerTagMappingRepository.GetPagingByFilter(new QueryPlayerTagMappingRequestViewModel
            {
                PageSize = 1
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.FirstOrDefault();

                var isExist1 = PlayerTagMappingRepository.IsPlayerTagExistByCache(matchFirst.PlayerID, matchFirst.TagID);
                var isExist2 = PlayerTagMappingRepository.IsPlayerTagExistByCache(matchFirst.PlayerID, matchFirst.TagID);

                //預期兩次都是true
                Assert.IsTrue(isExist1 == true);
                Assert.IsTrue(isExist1 == isExist2);
            }

            var isExist3 = PlayerTagMappingRepository.IsPlayerTagExistByCache(-1, -1);
            var isExist4 = PlayerTagMappingRepository.IsPlayerTagExistByCache(-1, -1);

            //預期兩次都是false
            Assert.IsTrue(isExist3 == false);
            Assert.IsTrue(isExist3 == isExist4);
            return;
        }


        [TestMethod]
        public void PlayerRiskScorePlus_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;
            PlayerTagMappingRepository.PlayerRiskScorePlus(playerID, 1);
            return;
        }

    }
}

