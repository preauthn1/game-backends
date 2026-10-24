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
using target.Models;
using target.Models.ViewModels.Base;
using NSubstitute;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Primitives;
using target.Utility.Helper.HttpContext;
using target.Utility.Helper.Dependency;
using target.Models.Table;

namespace target.Testing
{
    [TestClass]
    public class GameLaunchLobbyServiceTests : BaseTest
    {
        public LobbyTokenLogRepository LobbyTokenLogRepository { get; set; }

        public ILobbyService LobbyService { get; set; }

        public IGameLaunchService GameLaunchService { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }




        [TestMethod]
        public void LaunchGameAndCheckPlayerExist_Test()
        {
            string uuid = Guid.NewGuid().ToString();

            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;
            int merchantID = _TEST_DATA.NotSuperMerchant.MerchantID;
            string playerName = _TEST_DATA.NotSuperMerchant.PlayerName;

            string fakeJwt = this.GeneratePlayerJwtToken(
                playerID,
                merchantID,
                playerName,
                uuid);


            Guid hashGuid = Guid.NewGuid();

            LobbyTokenLogRepository.ResetPlayerLobbyToken(playerID);

            LobbyTokenLogRepository.Create(new Models.Table.LobbyTokenLogEntity
            {
                Type = LobbyTokenTypeEnum.Default.ToInteger(),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ExpiredTime = DateTimeHelper.GetCurrentTime().AddHours(8),
                IdentityGuid = hashGuid,
                IdentityHash = fakeJwt.MD5Hash(),
                PlayerID = playerID,
                MerchantID = merchantID,
                RawJwtToken = [REDACTED]
            });

            //v3版本開始 是存GUID
            MockHttpHeaderJwtToken(hashGuid.ToString());

            long ts = DateTimeHelper.GetCurrentTimeTimestamp();

            string signKey = SignKeyRuleHelper.GeneratePublicApiLobbySignkey(ts, playerName);

            var result = LobbyService.GetLobbyGamesAndSettings(new LobbyApiGetGamesRequestModel
            {
                SignKey = signKey,
                Ts = ts,
                PlayerName = playerName
            });

            var mySettingKey = MerchantCustomizedSettingKeyEnum.Lobby_LobbyGameSortType;
            var setting = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    MerchantIDs = merchantID.ToListCollection(),
                    CustomizedSettingKeyIDs = mySettingKey.ToInteger().ToListCollection()
                })
                .FirstOrDefault();               
                
            if(setting == null)
            {
                setting = new MerchantCustomizedSettingEntity
                {
                    MerchantID = merchantID,
                    SettingValue = "",
                    CustomizedSettingKey = mySettingKey.ToInteger(),
                    CreateTime = DateTimeHelper.GetCurrentTime()
                };
            }

            setting.SettingValue = LobbyGameSortEnum.Default.ToIntegerString();

            //配置1.
            MerchantCustomizedSettingRepository.CreateOrUpdate(setting.MerchantID , setting.CustomizedSettingKey , setting.SettingValue);

            //測試配置1
            var result2 = LobbyService.GetLobbyGamesAndSettings(new LobbyApiGetGamesRequestModel
            {
                SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword,
                Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                PlayerName = playerName,
                TargetMaintainVendorID = GameVendorTypeEnum.AGG.ToInteger()
            });

            Assert_NotEmptyCollection(result2.Games);

            //配置2.
            setting.SettingValue = LobbyGameSortEnum.BetAmountRanking.ToIntegerString();
            MerchantCustomizedSettingRepository.Update(setting);

            //測試配置2
            var result2_2 = LobbyService.GetLobbyGamesAndSettings(new LobbyApiGetGamesRequestModel
            {
                SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword,
                Ts = DateTimeHelper.GetCurrentTimeTimestamp(),
                PlayerName = playerName,
                TargetMaintainVendorID = GameVendorTypeEnum.AGG.ToInteger()
            });

            Assert_NotEmptyCollection(result2_2.Games);

            //測試如果導到三方遊戲 是否backUrl
            var launchResult = GameLaunchService.GenerateGameLinkForLobby(new LobbyApiGenerateLinkParameterModel
            {
                IsFromIframe = true,
                SignKey = SystemConstant.PublicAPI_SignKey_MagicPassword,
                GameID = _TEST_DATA.ExternalGameVendor.GameID_AGGGame,
                PlayerName = playerName,
                Ts = DateTimeHelper.GetCurrentTimeTimestamp()
            });

            string expectGuidBae64 = hashGuid.ToString().Base64Encode();
            string expectGuidRAW = hashGuid.ToString();

            //因為AGG的規格限制 又會再多包裝一層redirectUrl
            string parseBackurl = launchResult.Url.GetQueryStringValueFromUrlFormat("backurl");

            //預期再次產生的導頁網址 網址backUrl上的token會是原本那一組
            Assert.IsTrue(!string.IsNullOrWhiteSpace(launchResult.GameName));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(launchResult.SiteDisplayName));

            string backUrlBase64 = parseBackurl.GetQueryStringValueFromUrlFormat("backurlBase64");

            if (string.IsNullOrWhiteSpace(backUrlBase64))
            {
                Assert.IsTrue(parseBackurl.Contains(expectGuidBae64));
            }
            else
            {
                string decode = backUrlBase64.Base64Decode();
                Assert.IsTrue(decode.Contains(expectGuidBae64) || decode.Contains(expectGuidRAW));
            }

            return;
        }

        [TestMethod]
        public void GetExternalExitLink_Test()
        {
            var myService = new LobbyService();

            string lobbyUrl = "https://example.invalid";
            string backUrl1 = "https://example.invalid/homePage";
            string backUrl2 = "https://example.invalid/";

            var result1 = myService.GetExternalExitLink(backUrl1, lobbyUrl);
            var result2 = myService.GetExternalExitLink(backUrl2, lobbyUrl);

            Assert.IsTrue(result1.ExternalExitLink.IsNullOrWhiteSpace());
            Assert.IsTrue(result2.ExternalExitLink.IsNotEmptyTextNotNull());

            return;
        }

    }
}

