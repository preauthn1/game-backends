using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class DbValueCacheHelperTests : BaseTest
    {

        public GameListRepository GameListRepository { get; set; }


        [TestMethod]
        public void GetMultipleGamesDisplayName_Test()
        {
            var queryGames = GameListRepository.GetPagingByFilter(new Models.ViewModels.QueryGameListRequestViewModel
            {
                PageSize = 100,
                GameVendorTypes = new List<int>
                {
                    GameVendorTypeEnum.AGG.ToInteger(),
                    GameVendorTypeEnum.target.ToInteger(),
                }
            });

            List<int> gameIDs = queryGames.Items.Select(z => z.GameID).ToList();

            var cacheResult1 = DbMultiLanguageTextHelper.GetMultipleGamesDisplayName(gameIDs, LanguageEnum.zh_cn);
            var cacheResult2 = DbMultiLanguageTextHelper.GetMultipleGamesDisplayName(gameIDs, LanguageEnum.en_us);
            return;
        }



        [TestMethod]
        public void GetMultipleGamesDisplayName_V2_Test()
        {
            var queryGames = GameListRepository.GetPagingByFilter(new Models.ViewModels.QueryGameListRequestViewModel
            {
                PageSize = 500,
                GameVendorTypes = new List<int>
                {
                    GameVendorTypeEnum.AGG.ToInteger(),
                    GameVendorTypeEnum.AGGLive.ToInteger(),
                    GameVendorTypeEnum.target.ToInteger(),
                }
            });

            List<int> gameIDs = queryGames.Items.Select(z => z.GameID).ToList();

            int halfCount = gameIDs.Count() / 2;

            var cacheResult1 = DbMultiLanguageTextHelper.GetMultipleGamesDisplayName(gameIDs, LanguageEnum.zh_cn);
            var cacheResult2 = DbMultiLanguageTextHelper.GetMultipleGamesDisplayName(gameIDs, LanguageEnum.en_us);
            var cacheResult3 = DbMultiLanguageTextHelper.GetMultipleGamesDisplayName(gameIDs);

            //確認多語系 英文沒改壞
            if(cacheResult2.IsNotEmptyNull())
            {
                var matchGame = cacheResult2.FirstOrDefault(x => x.GameID == GameListEnum.DFDC_多福多财.ToInteger());
                if(matchGame != null)
                {
                    Assert.IsTrue(matchGame.LanguageText.Replace(" ", "").IsLettersOnly());
                }
            }

            //有值的佔多數
            Assert.IsTrue(cacheResult1.Where(x=> x.LanguageText.IsNotEmptyTextNotNull()).Count() > halfCount);
            Assert.IsTrue(cacheResult2.Where(x => x.LanguageText.IsNotEmptyTextNotNull()).Count() > halfCount);
            Assert.IsTrue(cacheResult3.Where(x => x.LanguageText.IsNotEmptyTextNotNull()).Count() > halfCount);
            return;
        }





        [TestMethod]
        public void GetGameNameBriefModels_Test()
        {
            var games = DbValueCacheHelper.GetGameNameBriefModels(new List<int>
            {
                _TEST_DATA.Default.GameID,
                _TEST_DATA.ExternalGameVendor.GameID,
            });

            Assert_NotEmptyCollection(games);
            Assert.IsTrue(games.Any(x=> x.GameVendorType > 0));
            Assert.IsTrue(games.Any(x => x.GameName.IsNotEmptyTextNotNull()));
            return;
        }


        [TestMethod]
        public void CacheHelper_GetGameNameFromLanguageText()
        {
            var q1 = DbValueCacheHelper.GetGameNameFromLanguageText(_TEST_DATA.Default.GameID);
            var q2 = DbValueCacheHelper.GetGameNameFromLanguageText(_TEST_DATA.Default.ThirdPartyGameID);
            return;
        }


        [TestMethod]
        public void CacheHelper_GetGame_Test()
        {
            var q1 = DbValueCacheHelper.GetGame(_TEST_DATA.Default.GameID);
            var q2 = DbValueCacheHelper.GetGame(_TEST_DATA.Default.ThirdPartyGameID);

            Assert.IsNotNull(q1);
            Assert.IsNotNull(q2);
            return;

        }


    }
}
