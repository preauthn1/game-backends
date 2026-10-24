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
using target.Models;
using target.BL.Services;

namespace target.Testing
{
    [TestClass]
    public class ForbiddenCreateAccountMerchantTest : BaseTest
    {

        public PlayerRepository PlayerRepository { get; set; }

        public IGameLaunchService GameLaunchService { get; set; }

        public IWalletCashService WalletCashService { get; set; }

        public IPlayerVendorService PlayerVendorService { get; set; }



        [TestMethod]
        public void ForbiddenCreateAccountMerchant_LaunchGameAndGetGamePageUrl_Test()
        {
            var merchantID = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantID;

            var result1 = GameLaunchService.LaunchGameAndGetGamePageUrl(new GameLaunchRequestModel
            {
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantSecret,
                PlayerName = GenerateTestAccountByDate("Launch"),
                GameID = 0
            });

            var result2 = GameLaunchService.LaunchGameAndGetGamePageUrl(new GameLaunchRequestModel
            {
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantSecret,
                PlayerName = GenerateTestAccountByDate("Launch"),
                GameID = _TEST_DATA.Default.GameID_AGG
            });

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            return;
        }


        [TestMethod]
        public void ForbiddenCreateAccountMerchant_Transfer_Test()
        {
            var merchantID = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantID;

            string playerName = GenerateTestAccountByDate("Transfer");

            var result1 = WalletCashService.TransferCashToGameWallet(new WalletTransferRequestModel
            {
                MerchantSecretKey = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantSecret,
                MerchantID = merchantID,
                PlayerName = playerName,
                Amount = 100,
                MerchantOrderNo = Guid.NewGuid().ToString()
            });

            var result2 = WalletCashService.TransferCashToPlatformWallet(new WalletTransferRequestModel
            {
                MerchantSecretKey = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantSecret,
                MerchantID = merchantID,
                PlayerName = playerName,
                Amount = 100,
                MerchantOrderNo = Guid.NewGuid().ToString()
            });


            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(playerName, merchantID);

            Assert.IsNotNull(queryPlayer);
            Assert.IsNotNull((queryPlayer.CreateTime - DateTimeHelper.GetCurrentTime()).TotalHours <= 48);
            return;
        }

        [TestMethod]
        public void ForbiddenCreateAccountMerchant_CreateNewPlayer_Test()
        {
            var merchantID = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantID;

            string playerName = GenerateTestAccountByDate("Create");

            PlayerVendorService.CreateNewPlayer(new CreatePlayerAccountExistRequestModel
            {
                PlayerName = playerName,
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantSecret
            });

            WalletCashService.GetPlayerWalletCash(new WalletCashPlayerAmountRequestModel
            {
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.ForbiddenCreateAccountMerchant.MerchantSecret,
                PlayerName = playerName,
            });

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(playerName, merchantID);

            Assert.IsNull(queryPlayer);
            return;
        }




        private string GenerateTestAccountByDate(string prefixText)
        {
            return $"MSTest_{prefixText}_" + DateTimeHelper.GetCurrentTime(8).ToString("yyyyMMdd");
        }


    }
}

