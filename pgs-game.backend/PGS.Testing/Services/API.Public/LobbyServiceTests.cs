using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class LobbyServiceTests : BaseTest
    {

        public ILobbyService ServiceInstance { get; set; }


        private static PlayerTokenJwtModel DefaultPlayerJwtModel
        {
            get
            {
                return new PlayerTokenJwtModel
                {
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    LoginName = _TEST_DATA.Default.PlayerName,
                    LoginUserID = _TEST_DATA.Default.PlayerID,
                };
            }
        }


        public class MockLobbyService : LobbyService
        {
            protected override LobbyTokenParsingResultModel GetTokenAndCheckValid(string token)
            {
                return new LobbyTokenParsingResultModel
                {
                    ParsingTokenModel = DefaultPlayerJwtModel,
                    ParsingRawRequest = new GameLaunchRequestModel
                    {

                    }
                };
            }
        }


        [TestMethod]
        public void GetLobbyGamesAndSettings_Test()
        {
            var mockService = new MockLobbyService();

            var query = mockService.GetLobbyGamesAndSettings(new LobbyApiGetGamesRequestModel
            {
                SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword,
                PlayerName = DefaultPlayerJwtModel.LoginName,
                Ts = DateTimeHelper.GetCurrentTimeTimestamp()
            });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Games);
            Assert.IsTrue(query.PlayerName == DefaultPlayerJwtModel.LoginName);
            return;
        }


        [TestMethod]
        public void GetBalanceAndLobbySettings_Test()
        {
            var mockService = new MockLobbyService();

            var query = mockService.GetLobbySettings(new LobbyApiGetGamesRequestModel
            {
                SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword,
                PlayerName = DefaultPlayerJwtModel.LoginName,
                Ts = DateTimeHelper.GetCurrentTimeTimestamp()
            });

            Assert.IsNotNull(query);
            Assert.IsTrue(query.PlayerName == DefaultPlayerJwtModel.LoginName);
            Assert.IsTrue(query.SiteDisplayName.IsNotEmptyTextNotNull());
            return;
        }



        [TestMethod]
        public void GetPlayerBalanceOnly_Test()
        {
            var mockService = new MockLobbyService();

            var query = mockService.GetPlayerBalanceOnly(new LobbyApiGetGamesRequestModel
            {
                SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword,
                PlayerName = DefaultPlayerJwtModel.LoginName,
                Ts = DateTimeHelper.GetCurrentTimeTimestamp()
            });

            Assert.IsNotNull(query);
            Assert.IsTrue(query.PlayerName == DefaultPlayerJwtModel.LoginName);
            Assert.IsTrue(query.Balance > 0);
            return;
        }





    }
}

