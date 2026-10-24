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
    public class PlayerWalletRepositoryTests : BaseTest
    {

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public TransactionRepository TransactionRepository { get; set; }

        public TransferRecordRepository TransferRecordRepository { get; set; }



        [TestMethod]
        public void UpdateWalletAmountForbiddenMinus_Test()
        {
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;

            decimal amount = 0.001M;

            var updateResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(playerID, _TEST_DATA.Default.GameID, amount ,TransactionTypeEnum.EventPrize, "", "");

            Assert.IsTrue((updateResult.AfterAmount - updateResult.BeforeAmount) == amount);
            return;
        }




        [TestMethod]
        public void WalletTransfer_Test()
        {
            int merchantID = _TEST_DATA.NotSuperMerchant.MerchantID;
            int playerID = _TEST_DATA.NotSuperMerchant.PlayerID;

            decimal amount1 = 101;
            decimal amount2 = 102;

            string orderNo_pgs = Guid.NewGuid().ToString();
            string orderNo_merchant = Guid.NewGuid().ToString();

            var updateResult1 = PlayerWalletRepository.WalletTransfer(new PlayerWalletTransferModel
            {
                Amount = amount1,
                ConvertRate = 1,
                PlayerID = playerID,
                OrderNo_PGS = orderNo_pgs,
                OrderNo_Merchant = orderNo_merchant,
                TranferType = WalletTransferTypeEnum.Transfer_In
            });

            AssertExpectExceptionProcess(() =>
            {
                var updateResult2 = PlayerWalletRepository.WalletTransfer(new PlayerWalletTransferModel
                {
                    Amount = amount2,
                    ConvertRate = 1,
                    PlayerID = playerID,
                    OrderNo_PGS = orderNo_pgs,
                    OrderNo_Merchant = orderNo_merchant,
                    TranferType = WalletTransferTypeEnum.Transfer_In
                });
            }, ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            var queryTransactionLog = TransactionRepository.GetByFilter(new QueryTransactionRecordRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                PlayerIDs = playerID.ToListCollection(),
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMinutes(-3),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime().AddMinutes(3),
            });

            var queryTransferLog = TransferRecordRepository.GetByFilter(new QueryTransferRecordRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                PlayerIDs = playerID.ToListCollection(),
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMinutes(-3),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime().AddMinutes(3),
            });

            Assert.IsTrue(queryTransactionLog.Any(x => x.Amount == amount1));
            Assert.IsFalse(queryTransactionLog.Any(x => x.Amount == amount2));

            Assert.IsTrue(queryTransferLog.Where(x => x.MerchantOrderNo == orderNo_merchant).Count() == 1);
            Assert.IsTrue(queryTransferLog.Any(x => x.Amount == amount1));
            Assert.IsFalse(queryTransferLog.Any(x => x.Amount == amount2));
            return;
        }



    }
}

