using game.Entity.Game;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SingleWalletMerchantBetProcessRefServiceTests : BaseTest
    {


        [TestMethod]
        public void PayoutProcessWithDifferentApiLogic_Test()
        {
            var serviceInstance = InstanceMockService();

            var param = new SingleWalletSettleCallbackParameter
            {
                MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                BetAmount = 100,
                PayoutAmount = 100,
                ValidAmount = 100,
                TaxAmount = 0,
                SettlementTime = DateTimeHelper.GetCurrentTime(),
                BetRecordNoPGS = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo(),
                GameID = _TEST_DATA.Default.GameID,
                PlayerID = _TEST_DATA.SingleWallet.PlayerID,
                PlayerName = _TEST_DATA.SingleWallet.PlayerName,
            };

            var result = serviceInstance.PayoutProcessWithDifferentApiLogic(param);

            ResetMockRegister();
            return;
        }


        [TestMethod]
        public void CancelProcessWithDifferentApiLogic_Test()
        {
            var serviceInstance = InstanceMockService();
            serviceInstance.CancelProcessWithDifferentApiLogic(new SingleWalletModeCancelApiParameter
            {
                GameID = _TEST_DATA.Default.GameID,
                MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                PlayerID = _TEST_DATA.SingleWallet.PlayerID,
                TransactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo()
            });

            ResetMockRegister();
            return;
        }


        private SingleWalletCallbackNotifyProcessRefService InstanceMockService()
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

            var myService = new SingleWalletCallbackNotifyProcessRefService();
            return myService;
        }

    }
}

