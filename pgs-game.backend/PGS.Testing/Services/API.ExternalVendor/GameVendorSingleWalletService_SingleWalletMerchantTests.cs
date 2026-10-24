using game.Entity.Game;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace target.Testing
{
    [TestClass]
    public class GameVendorSingleWalletService_SingleWalletMerchantTests : BaseTest
    {
        public GameListRepository GameListRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public IGameVendorSingleWalletService ServiceInstance { get; set; }

        public IGameVendorSingleWalletFishingProcessService GameVendorSingleWalletFishingProcessService { get; set; }

        public IGameVendorSingleWalletService IGameVendorSingleWalletService { get; set; }




        [TestCleanup]
        public void CleanUp()
        {
            IOCHelper.MSTestRemoveAllMocks();
            return;
        }


        [TestMethod]
        public void GameVendor_PlaceBet_AGG_Test()
        {
            DoMSTest_GameVendor_PlaceBet_AGG_Test(_TEST_DATA.SingleWallet.PlayerID, _TEST_DATA.SingleWallet.MerchantID);
            return;
        }



        private void DoMSTest_GameVendor_PlaceBet_AGG_Test(int targetPlayerID , int targetMerchantID)
        {
            //使用mock
            var substitute = Substitute.For<IBasicSemalessCallbackService>();

            substitute.PlaceBet(null).ReturnsForAnyArgs(new SingleWalletModePlaceBetApiResponseModel
            {
                Balance = _TEST_DATA.SingleWallet.BalanceAmount,
                TransactionNo = RandomHelper.GetOrderNo("MSTest")
            });

            substitute.Settlement(null).ReturnsForAnyArgs(new SingleWalletModeSettleApiResponseModel
            {
                Balance = _TEST_DATA.SingleWallet.BalanceAmount,
                TransactionNo = RandomHelper.GetOrderNo("MSTest")
            });

            IOCHelper.MSTestAddMockInstance<IBasicSemalessCallbackService>(substitute);

            //若使用mockService 需手動註冊service
            ServiceInstance = IOCHelper.Resolve<IGameVendorSingleWalletService>();

            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGG;
            var queryMerchangGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                AssignMerchantIDs = targetMerchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger()
            });

            var matchFirst = queryMerchangGames
                .OrderBy(z => z.GameVendorType == targetGameVendor.ToInteger() ? 0 : 999)
                .FirstOrDefault();

            string orderNo = Guid.NewGuid().ToString();

            decimal betAmount = RandomHelper.GetRandIntegerNumbers(100, 150);
            decimal payoutAmount = RandomHelper.GetRandIntegerNumbers(100, 300);

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = targetPlayerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                SessionToken = [REDACTED],
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGG = new AGGApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new AGGSingleWalletAPITransactionRequestModel
                        {
                            WagerId = orderNo,
                            TransactionId = orderNo,
                            GameId = matchFirst.ThirdPartyGameCode,
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = targetPlayerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGG = new AGGApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new AGGSingleWalletAPITransactionRequestModel
                        {
                            WagerId = orderNo,
                            TransactionId = orderNo,
                            GameId = matchFirst.ThirdPartyGameCode,
                            UserID = targetPlayerID,
                            Operations = new List<AGGSingleWalletOperationRequestModel>()
                        }
                    }
                }
            });

            Assert.IsTrue(!string.IsNullOrWhiteSpace(resultSettle.CommonInfo.BetRecordNoPGS));
            Assert.IsNotNull(resultSettle.CommonInfo);

            string pgsTransactionNo = resultPlaceBet.CommonInfo.BetRecordNoPGS;

            var queryExist = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(pgsTransactionNo);

            Assert.IsNotNull(queryExist);
            Assert.IsTrue(queryExist.BetAmount == betAmount);
            return;
        }



        [TestMethod]
        public void GameVendor_GameVendorWalletTransfer_TransferMode_Test()
        {
            var vendorID = GameVendorTypeEnum.Mario;
            int playerID = _TEST_DATA.Default.PlayerID;

            var queryWallet1 = PlayerWalletRepository.GetPlayerWalletByType(playerID, WalletTypeEnum.GameWallet);

            var param1 = new GameVendorWalletTransactionRequestParamter
            {
                Amount = 100,
                PlayerID = playerID,
                TransactionID = Guid.NewGuid().ToString(),
                TransactionType = TransactionTypeEnum.GameVendorTransferWallet_To_GameVendorWallet
            };

            var param2 = new GameVendorWalletTransactionRequestParamter
            {
                Amount = 100,
                PlayerID = playerID,
                TransactionID = Guid.NewGuid().ToString(),
                TransactionType = TransactionTypeEnum.GameVendorTransferWallet_To_PgsWallet
            };

            var result1 = GameVendorSingleWalletFishingProcessService.GameVendorWalletTransfer(vendorID, param1);
            var result2 = GameVendorSingleWalletFishingProcessService.GameVendorWalletTransfer(vendorID, param2);

            var queryWallet2 = PlayerWalletRepository.GetPlayerWalletByType(playerID, WalletTypeEnum.GameWallet);

            Assert.IsTrue(queryWallet1.Amount == queryWallet2.Amount);
            return;
        }


        [TestMethod]
        public void GameVendor_SendEventPrizeAndUpdateBalance_AT_Test()
        {
            var vendorID = GameVendorTypeEnum.AT;
            var transactionType = TransactionTypeEnum.EventPrize;
            decimal amount = 1;
            var bonusParam = new GameVendorConvertToEventPrizeInfoModel
            {
                Amount = amount,
                BonusTypeName = "升級獎勵",
                IsFishGameBonus = true,
                ThirdOrderNo = Guid.NewGuid().ToString(),
                ReceiveTime = DateTimeHelper.GetCurrentTime(),
             };

            var param_Transfer = new GameVendorWalletTransactionRequestParamter
            {
                Amount = amount,
                PlayerID = _TEST_DATA.Default.PlayerID,
                TransactionID = Guid.NewGuid().ToString(),
                TransactionType = transactionType,
                EventPrizeInfo = bonusParam
            };

            var bonusParam2 = bonusParam.ReturnInjecteObject<GameVendorConvertToEventPrizeInfoModel>();
            bonusParam2.ThirdOrderNo = Guid.NewGuid().ToString();

            var param_SingleWallet = new GameVendorWalletTransactionRequestParamter
            {
                Amount = amount,
                PlayerID = _TEST_DATA.SingleWallet.PlayerID,
                TransactionID = Guid.NewGuid().ToString(),
                TransactionType = transactionType,
                EventPrizeInfo = bonusParam2,
            };

            var result1 = IGameVendorSingleWalletService.GameVendorSendEventBonus(vendorID, param_Transfer);
            var result2 = IGameVendorSingleWalletService.GameVendorSendEventBonus(vendorID, param_SingleWallet);
            return;
        }



        [TestMethod]
        public void GameVendor_SendEventPrizeAndUpdateBalance_RG_Test()
        {
            var vendorID = GameVendorTypeEnum.RG;
            var transactionType = TransactionTypeEnum.EventPrize;
            decimal amount = 1;

            var bonusParam = new GameVendorConvertToEventPrizeInfoModel
            {
                Amount = amount,
                BonusTypeName = "特殊獎勵",
                IsFishGameBonus = true,
                ThirdOrderNo = Guid.NewGuid().ToString(),
                ReceiveTime = DateTimeHelper.GetCurrentTime(),
            };

            var param_Transfer = new GameVendorWalletTransactionRequestParamter
            {
                Amount = amount,
                PlayerID = _TEST_DATA.Default.PlayerID,
                TransactionID = Guid.NewGuid().ToString(),
                TransactionType = transactionType,
                EventPrizeInfo = bonusParam,
                Extra = new GameVendorWalletBonusExtraParamter
                {
                    
                }
            };

            var bonusParam2 = bonusParam.ReturnInjecteObject<GameVendorConvertToEventPrizeInfoModel>();
            bonusParam2.ThirdOrderNo = Guid.NewGuid().ToString();

            var param_SingleWallet = new GameVendorWalletTransactionRequestParamter
            {
                Amount = amount,
                PlayerID = _TEST_DATA.SingleWallet.PlayerID,
                TransactionID = Guid.NewGuid().ToString(),
                TransactionType = transactionType,
                EventPrizeInfo = bonusParam2,
                Extra = new GameVendorWalletBonusExtraParamter
                {
                    
                }
            };

            var result1 = IGameVendorSingleWalletService.GameVendorSendEventBonus(vendorID, param_Transfer);
            var result2 = IGameVendorSingleWalletService.GameVendorSendEventBonus(vendorID, param_SingleWallet);
            return;
        }




        [TestMethod]
        public void GameVendor_GameVendorWalletTransfer_SingleWalletMode_Test()
        {
            var vendorID = GameVendorTypeEnum.Mario;

            int playerID = _TEST_DATA.SingleWallet.PlayerID;

            decimal amount = 10;

            var param1 = new GameVendorWalletTransactionRequestParamter
            {
                Amount = amount,
                PlayerID = playerID,
                TransactionID = Guid.NewGuid().ToString(),
                TransactionType = TransactionTypeEnum.GameVendorTransferWallet_To_GameVendorWallet
            };

            var param2 = new GameVendorWalletTransactionRequestParamter
            {
                Amount = amount,
                PlayerID = playerID,
                TransactionID = Guid.NewGuid().ToString(),
                TransactionType = TransactionTypeEnum.GameVendorTransferWallet_To_PgsWallet
            };

            var wallet1 = ServiceInstance.GetWalletBalance(vendorID, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = playerID,
            });

            var result_deposit = GameVendorSingleWalletFishingProcessService.GameVendorWalletTransfer(vendorID, param1);

            var wallet2 = ServiceInstance.GetWalletBalance(vendorID, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = playerID,
            });

            var result_withdrawal = GameVendorSingleWalletFishingProcessService.GameVendorWalletTransfer(vendorID, param2);

            var wallet3 = ServiceInstance.GetWalletBalance(vendorID, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = playerID,
            });


            Assert.IsTrue((wallet1.BalanceAmount - wallet2.BalanceAmount) == amount);
            Assert.IsTrue((wallet3.BalanceAmount - wallet2.BalanceAmount) == amount);
            Assert.IsTrue(wallet2.BalanceAmount == result_withdrawal.BeforeAmount);
            Assert.IsTrue(wallet3.BalanceAmount == result_withdrawal.AfterAmount);
            return;
        }


    }
}

