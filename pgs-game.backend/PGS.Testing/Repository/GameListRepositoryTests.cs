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
    public class GameListRepositoryTests : BaseTest
    {

        public GameListRepository GameListRepository { get; set; }




        [TestMethod]
        public void GameList_GetPagingByFilter_Test()
        {
            var queryGames = GameListRepository.GetPagingByFilter(new QueryGameListRequestViewModel
            {
                PageIndex = 1,
                PageSize = 10,
                GameIDs = new List<int>
                {
                }
            });             
            
            var queryGames_page2 = GameListRepository.GetPagingByFilter(new QueryGameListRequestViewModel
            {
                PageSize = 10,
                PageIndex = 2,
                GameIDs = new List<int>
                {
                }
            });

            var queryGames_notFound = GameListRepository.GetPagingByFilter(new QueryGameListRequestViewModel
            {
                PageSize = 10,
                GameIDs = new List<int>
                {
                    -1
                }
            });

            List<int> page1_gameIDs = queryGames.Items.Select(x => x.GameID).ToList();

            //page 1 & page 2 不能有相同的
            Assert.IsTrue(queryGames_page2.Items.All(g => !page1_gameIDs.Contains(g.GameID)));
            return;
        }


        [TestMethod]
        public void GameList_GameIdAndGameVendorRuleMatch_Test()
        {
            if (IsSkipTest_WhenSmallerThanTargetTime(_TEST_DATA.Default.Date_NewGameVendor_Intergrate))
                return;

            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true
            });

            foreach(var item in queryGames)
            {
                //確保每一款遊戲的遊戲編號 & 遊戲商都有follow這個規則
                int gameVendor = AppHelper.GetGameVendorByGameID(item.GameID);

                //slot88遊戲 不檢查
                if(item.GameVendorType != GameVendorTypeEnum.Slot88.ToInteger())
                {
                    Assert.IsTrue(gameVendor == item.GameVendorType);
                }
            }

            return;
        }

    }
}

