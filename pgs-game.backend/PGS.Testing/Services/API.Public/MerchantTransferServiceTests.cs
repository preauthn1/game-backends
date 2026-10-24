using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
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
    public class MerchantTransferServiceTests : BaseTest
    {

        public PlayerRepository PlayerRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }


        public IGameLaunchService GameLaunchService { get; set; }


        public class MockMerchantTransferService: MerchantTransferService
        {
            protected override AGGFishTransferApiDetailDataModel DoW88TransgerLogicAndRetry(WalletTransferTypeEnum type, int playerID, decimal amount, string pgsOrderNo)
            {
                return new AGGFishTransferApiDetailDataModel
                {
                    AfterBalance = 123,
                    PlatformOrderNo = Guid.NewGuid().ToString(),
                    TxtID = pgsOrderNo
                };
            }
        }

        public class MockTransferTimeMerchantTransferService : MerchantTransferService
        {                    
            protected override int GetTransferTimesCount(int playerID, WalletTransferTypeEnum type)
            {                
                return 99;
            }
        }



        [TestMethod]
        public void PgsWalletTransferByMerchantApi_Test()
        {
            var mockService = new MockMerchantTransferService();

            var playerID = _TEST_DATA.LobbyTransferMerchant.PlayerID;

            decimal amount = 10;

            //需要跑一次launch 產生sessionToken
            var launch = GameLaunchService.LaunchGameAndGetGamePageUrlByPlayerID(new InternalTestBundleRequestModel
            {
                DoNotIframe = true,
                GameID = 0,
                PlayerID = playerID,
                PlatformSessionToken = [REDACTED],
            });

            var balance = mockService.GetWalletInfo(playerID);

            var result1 = mockService.PgsWalletTransferByMerchantApi(playerID, new MerchantWalletTransferRequestModel
            {
                Amount = amount,
                Type = WalletTransferTypeEnum.Transfer_In
            });

            var result2 = mockService.PgsWalletTransferByMerchantApi(playerID, new MerchantWalletTransferRequestModel
            {
                Amount = amount,
                Type = WalletTransferTypeEnum.Transfer_Out
            });

            Assert.IsTrue(balance != null);
            Assert.IsTrue(result1.PgsOrderNo.IsNotEmptyTextNotNull());
            Assert.IsTrue(result2.PgsOrderNo.IsNotEmptyTextNotNull());
            return;
        }


        [TestMethod]
        public void TransferTimesExceed_Test()
        {
            var queryPlayer = PlayerRepository.GetPagingByFilter(new QueryPlayerRequestViewModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14),
                PageSize = 5
            });

            if(queryPlayer.Items.IsNotEmptyNull())
            {
                var forbiddenTransferTag = SystemTagEnum.Player_LobbyTransferForbidden.ToInteger();

                var matchFirst = queryPlayer.Items.FirstOrDefault();

                //清除
                PlayerTagMappingRepository.DeleteByModel(new
                {
                    PlayerID = matchFirst.PlayerID,
                    TagID = SystemTagEnum.Player_LobbyTransferForbidden.ToInteger()
                });
                                        
                var mockService = new MockTransferTimeMerchantTransferService();

                AssertExpectExceptionProcess(() =>
                {
                    mockService.CheckRequestTransferTooManyTimesOrForbidden(matchFirst, WalletTransferTypeEnum.Transfer_In);

                }, ApiErrorCode.Forbidden_RequestTooManyTimes);
            }
          
            return;
        }


    }
}

