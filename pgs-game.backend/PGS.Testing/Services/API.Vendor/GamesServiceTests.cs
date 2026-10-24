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
    public class GamesServiceTests : BaseTest
    {

        public IGamesService ServiceInstance { get; set; }


        [TestMethod]
        public void GetAllGames_Test()
        {
            var query = ServiceInstance.GetMerchantEnableGames(new GameInfoRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                SpecialOrderSequence = 1001
            });

            Assert_NotEmptyCollection(query);

            if(query.IsNotEmptyNull() && query.Count() > 4)
            {
                //應該大部分的遊戲都是允許launch的
                var halfCount = query.Count() / 2;
                Assert.IsTrue(query.Where(x=> x.IsLaunchAvailable).Count() >= halfCount);
                Assert.IsTrue(query.Where(x => x.GameName.IsNotEmptyTextNotNull()).Count() >= halfCount);
            }

            return;
        }


      
    }
}

