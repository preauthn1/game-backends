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
    public class PlayerRepositoryTests : BaseTest
    {

        public PlayerRepository PlayerRepository { get; set; }


        [TestMethod]
        public void GetByPlayerNameAndMerchantID_Test()
        {
            var target_playerName = _TEST_DATA.Default.PlayerName;
            var target_merchantID = _TEST_DATA.Default.MerchantID;

            var query = PlayerRepository.GetByPlayerNameAndMerchantID(target_playerName, target_merchantID);

            Assert.IsNotNull(query.PlayerID == _TEST_DATA.Default.PlayerID);
            Assert.IsNotNull(query.PlayerName == target_playerName);
            Assert.IsNotNull(query.MerchantID == target_merchantID);
            return;
        }


        [TestMethod]
        public void GetByPlayerNameAndMerchantID_Case2_Test()
        {
            var queryPlayers = PlayerRepository.GetPagingByFilter(new QueryPlayerRequestViewModel
            {
                PageSize = 10
            });

            foreach (var item in queryPlayers.Items.OrderByRandom().Take(3))
            {
                var queryEachPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(item.PlayerName, item.MerchantID);

                Assert.IsNotNull(queryEachPlayer.PlayerID == item.PlayerID);
                Assert.IsNotNull(queryEachPlayer.PlayerName == item.PlayerName);
                Assert.IsNotNull(queryEachPlayer.MerchantID == item.MerchantID);
            }

            return;
        }



        [TestMethod]
        public void GetByLargeAccounts_Test()
        {
            var target_merchantID = _TEST_DATA.Default.MerchantID;

            var query1000 = PlayerRepository.GetPagingByFilter(new QueryPlayerRequestViewModel
            {
                MerchantID = target_merchantID,
                PageSize = 1000
            });

            if(query1000.Items.IsNotEmptyNull())
            {
                var accounts = query1000.Items.Select(q => q.PlayerName).ToList();

                var query = PlayerRepository.GetByLargeAccounts(target_merchantID , accounts);

                string print_1 = string.Join(",", query1000.Items.Select(x => x.PlayerID).OrderBy(z => z));
                string print_2 = string.Join(",", query.Select(x => x.PlayerID).OrderBy(z => z));

                Assert.IsTrue(print_1 == print_2);
            }

            return;
        }


    }
}

