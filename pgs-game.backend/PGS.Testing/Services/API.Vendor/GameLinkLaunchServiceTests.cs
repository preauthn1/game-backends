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
using target.Utility.IOC;
using target.Models.ViewModels.GameVendor;
using target.BL.Services;

namespace target.Testing
{
    [TestClass]
    public class GameLinkLaunchServiceTests : BaseTest
    {

        public IGameLinkService GameLinkService { get; set; }

        public IGameLaunchService GameLaunchService { get; set; }

        public IPlayerJwtCheckService IPlayerJwtCheckService { get; set; }

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }


        [TestMethod]
        public void CopyValueFromOriginalRequestModel_Test()
        {
            var launchService = new GameLaunchService();

            var emptyModel = new GameLaunchRequestModel();
            var valueSourceModel = new GameLaunchRequestModel();
            valueSourceModel.IsTestMode = true;
            valueSourceModel.BackUrl = "http://example.invalid";
            valueSourceModel.IsTrialModeToLaunch = true;
            valueSourceModel.LiveCasinoLevelID = "ABC";

            //不需要覆蓋的
            valueSourceModel.GameID = 123;
            valueSourceModel.PlayerName = "QQQQQQQQ";

            launchService.CopyValueFromOriginalRequestModel(emptyModel, valueSourceModel);

            //應該不同
            Assert.IsTrue(emptyModel.GameID != valueSourceModel.GameID);
            Assert.IsTrue(emptyModel.PlayerName != valueSourceModel.PlayerName);

            //應該相同
            Assert.IsTrue(emptyModel.IsTestMode == valueSourceModel.IsTestMode);
            Assert.IsTrue(emptyModel.BackUrl == valueSourceModel.BackUrl);
            Assert.IsTrue(emptyModel.IsTrialModeToLaunch == valueSourceModel.IsTrialModeToLaunch);
            Assert.IsTrue(emptyModel.LiveCasinoLevelID == valueSourceModel.LiveCasinoLevelID);
            return;
        }




        [TestMethod]
        public void GameLaunchService_LaunchByTrialMode_Test()
        {
            int originalMerchantID = _TEST_DATA.Default.MerchantID;
            var launchResult = GameLaunchService.LaunchGameCommonProcess(new GameLaunchRequestModel
            {
                MerchantID = originalMerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = "MSTest" + DateTimeHelper.GetCurrentTimeTimestamp(),
                GameID = _TEST_DATA.Default.GameID,
                BackUrl = "https://example.invalid/",
                IsTrialModeToLaunch = true
            });

            //若使用trialMode會被強制轉換成另一組商戶才對
            Assert.IsTrue(launchResult.PlayerJwtModel.MerchantID != originalMerchantID);
            return;
        }



        [TestMethod]
        public void GameLaunchService_LaunchPgsGame_And_CheckTokenExist_Test()
        {
            int targetGameID = _TEST_DATA.Default.GameID;

            var gameVendor = AppHelper.GetGameVendorByGameID(targetGameID);

            //_TEST_DATA.Default.GameID 提供的gameID 必須是PGS遊戲。此邏輯不允許變動
            Assert.IsTrue(gameVendor == GameVendorTypeEnum.target.ToInteger());

            var launchResult = GameLaunchService.LaunchGameCommonProcess(new GameLaunchRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = "MSTest" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS(),
                GameID = targetGameID,
                BackUrl = "https://example.invalid/",
                IsFromLobbyPage = true,
            });

            var md5Hash = example.invalid.Access_Token.MD5Hash();

            //確保不拋exception
            IPlayerJwtCheckService.CheckIsTokenExist(md5Hash);
            return;
        }



        [TestMethod]
        public void GenerateGameUrl_LobbyPage_Test()
        {
            string backUrl = "https://example.invalid";
            int merchantID = _TEST_DATA.Default.MerchantID;

            var param = new GameLaunchVendorApiTransferModeRequestModel
            {
                GameID = 0,
                MerchantID = _TEST_DATA.Default.MerchantID,
                PlayerName = _TEST_DATA.Default.PlayerName,
                Lang = SystemConstant.DefaultLang,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                InternalParameter_DoNotIframeUrl = true,
                BackUrl = backUrl,
            };

            var result = GameLinkService.GenerateTransferModeLaunchLink(param);

            Assert.IsTrue(result.Link.IsNotEmptyTextNotNull());
            CheckTokenIsValidAndParsingSuccess(result , param);

            return;
        }

        private void CheckTokenIsValidAndParsingSuccess(
            GameLinkGenerateResultViewModel result , 
            GameLaunchVendorApiTransferModeRequestModel requestParam)
        {
            var tokenValue = result.Link.GetQueryStringValueFromUrlFormat("pgsToken");

            if(string.IsNullOrWhiteSpace(tokenValue))
            {
                tokenValue = result.Link.GetQueryStringValueFromUrlFormat("v3Token");
            }

            //這是跟前端約定好的模式
            if(tokenValue.IsBase64String())
            {
                tokenValue = tokenValue.Base64Decode();
            }

            var tokenParsing = IPlayerJwtCheckService.ParsingLobbyToken(tokenValue);

            Assert.IsNotNull(tokenParsing.ParsingTokenModel);

            Assert.IsTrue(tokenParsing.ParsingRawRequest?.BackUrl == requestParam.BackUrl);
            Assert.IsTrue(tokenParsing.ParsingTokenModel.MerchantID == requestParam.MerchantID);
            Assert.IsTrue(tokenParsing.ParsingTokenModel.LoginName == requestParam.PlayerName);
            return;
        }


        [TestMethod]
        public void GenerateGameUrlForVendorApi_Test()
        {
            string fakeUrl = "https://example.invalid";

            //使用mock
            MockGameVendorService(fakeUrl);

            var result = GameLinkService.GenerateTransferModeLaunchLink(new GameLaunchVendorApiTransferModeRequestModel
            {
                GameID = _TEST_DATA.ExternalGameVendor.GameID_AGGGame,
                MerchantSecretKey = _TEST_DATA.NotSuperMerchant.MerchantSecret,
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                PlayerName = _TEST_DATA.NotSuperMerchant.PlayerName,
                Lang = SystemConstant.DefaultLang,
            });

            Assert.IsTrue(fakeUrl == result.Link);
            return;
        }


        [TestMethod]
        public void GenerateGameUrlForVendorApi_SingleWalletMerchant_Test()
        {
            string fakeUrl = "https://example.invalid";

            //使用mock
            MockGameVendorService(fakeUrl);

            var result = GameLinkService.GenerateSingleWalletLaunchLink(new GameLaunchVendorApiSingleWalletModeRequestModel
            {
                GameID = _TEST_DATA.ExternalGameVendor.GameID,
                MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                MerchantSecretKey = _TEST_DATA.SingleWallet.MerchantSecret,
                PlayerUniqueID = _TEST_DATA.SingleWallet.PlayerName,
                SessionToken = [REDACTED],
                Lang = SystemConstant.DefaultLang,
            });

            Assert.IsTrue(fakeUrl == result.Link);
            return;
        }


        [TestMethod]
        public void AutoDepositTrialWallet_Test()
        {
            var launchService = new GameLaunchService();

            int merchantID = _TEST_DATA.Default.MerchantID;
            var playerName = _TEST_DATA.Default.PlayerName;  //"Unitest_20260311";

            var queryMerchant = MerchantRepository.GetById(merchantID);

            var resultText = launchService.AutoDepositTrialWallet(playerName, queryMerchant);

            Assert.IsTrue(decimal.Parse(resultText) > 0);
            return;
        }


        [TestMethod]
        public void AutoDepositTrialWallet_FailCase_Test()
        {
            var launchService = new GameLaunchService();

            int merchantID = _TEST_DATA.SingleWallet.MerchantID;
            var playerName = _TEST_DATA.SingleWallet.PlayerName;  

            var queryMerchant = MerchantRepository.GetById(merchantID);

            var resultText = launchService.AutoDepositTrialWallet(playerName, queryMerchant);

            Assert.IsTrue(resultText == "");
            return;
        }



        [TestMethod]
        public void GenerateGameLink_VendorMaintainSkipOrNot_Test()
        {
            var id = VendorMaintenanceRepository.Create(new Models.Table.VendorMaintenanceEntity
            {
                VendorID = GameVendorTypeEnum.AGG.ToInteger(),
                StartTime = DateTimeHelper.GetCurrentTime(),
                EndTime = DateTimeHelper.GetCurrentTime().AddSeconds(60),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                UpdateTime = DateTimeHelper.GetCurrentTime(),
                CreateBy = "",
                UpdateBy = "",
                Type = 0
            });

            var result1 = GameLinkService.GenerateSingleWalletLaunchLink(new GameLaunchVendorApiSingleWalletModeRequestModel
            {
                GameID = _TEST_DATA.ExternalGameVendor.GameID_AGGGame,
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerUniqueID = _TEST_DATA.SingleWallet.PlayerName,
                SessionToken = [REDACTED],
                Lang = SystemConstant.DefaultLang,
            });

            Assert.IsTrue(result1.Link.IsNotEmptyTextNotNull());

            try
            {
                var result2 = GameLinkService.GenerateSingleWalletLaunchLink(new GameLaunchVendorApiSingleWalletModeRequestModel
                {
                    GameID = _TEST_DATA.ExternalGameVendor.GameID_AGGGame,
                    MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                    MerchantSecretKey = _TEST_DATA.SingleWallet.MerchantSecret,
                    PlayerUniqueID = _TEST_DATA.SingleWallet.PlayerName,
                    SessionToken = [REDACTED],
                    Lang = SystemConstant.DefaultLang,
                });
            }
            catch(Exception ex)
            {
                VendorMaintenanceRepository.Delete(id);
                AssertExpectException(ex, ApiErrorCode.GameUnderMaintain);
            }
          
            VendorMaintenanceRepository.Delete(id);
            return;
        }



        private void MockGameVendorService(string fakeUrl)
        {
            var substitute = Substitute.For<IGameVendorIntegrationService>();
            substitute.GetGameLaunchUrl(null).ReturnsForAnyArgs(new ExternalVendorLaunchUrlViewModel
            {
                Url = fakeUrl,
            });

            IOCHelper.MSTestAddMockInstance<IGameVendorIntegrationService>(substitute);
        }

    }
}

