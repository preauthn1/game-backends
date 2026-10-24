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
using target.Utility;

namespace target.Testing
{
    [TestClass]
    public class WalletCashServiceTests : BaseTest
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public IWalletCashService ServiceInstance { get; set; }


        [TestMethod]
        public void GetPlayerWalletCash_Test()
        {
            string playerName = _TEST_DATA.Default.PlayerName;

            var query = ServiceInstance.GetPlayerWalletCash(new WalletCashPlayerAmountRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = playerName,
                IsIncludeExtraInfo = true
            });

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.ExtraInfo.PlayerName == playerName);
            Assert.IsTrue(query.Amount > 0);
            return;
        }



        [TestMethod]
        public void TransferCashToGameWallet_Test()
        {
            decimal myAmount = 10;
            var query = ServiceInstance.TransferCashToGameWallet(new WalletTransferRequestModel
            {
                Amount = myAmount,
                MerchantOrderNo = RandomHelper.GetOrderNo("UNI"),
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = _TEST_DATA.Default.PlayerName
            });

            Assert.IsNotNull(query);
            Assert.IsTrue(query.BeforeAmount > 0);
            Assert.IsTrue((query.AfterAmount - query.BeforeAmount) == myAmount);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(query.TransactionNo));
            return;
        }



        [TestMethod]
        public void TransferCashToGameWallet_ConvertRateX100_Test()
        {
            int targetMerchantID = _TEST_DATA.TransferRate100.MerchantID;

            decimal convertRate = 100;

            var queryMerchants = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    CustomizedSettingKeyIDs = new List<int>
                    {
                        MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer.ToInteger()
                    },
                    SettingValueMatch = $"{(int)convertRate}"
                })
                .Where(x => x.MerchantID == targetMerchantID)
                .ToList();

            if(queryMerchants.IsNotEmptyNull())
            {
                var matchFirst = queryMerchants.FirstOrDefault();

                var queryMerchant = MerchantRepository.GetById(matchFirst.MerchantID);

                decimal myAmount = RandomHelper.GetRandIntegerNumbers(11 , 19);

                var transfer_in = ServiceInstance.TransferCashToGameWallet(new WalletTransferRequestModel
                {
                    Amount = myAmount,
                    MerchantOrderNo = RandomHelper.GetOrderNo("UNI"),
                    MerchantID = queryMerchant.MerchantID,
                    MerchantSecretKey = queryMerchant.ApiSecretKey,
                    PlayerName = "test_775522"
                });

                Assert.IsNotNull(transfer_in);
                Assert.IsTrue((transfer_in.AfterAmount - transfer_in.BeforeAmount) == myAmount * convertRate);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(transfer_in.TransactionNo));

                decimal outAmount = myAmount - 1;
                var transfer_out = ServiceInstance.TransferCashToPlatformWallet(new WalletTransferRequestModel
                {
                    Amount = outAmount,
                    MerchantOrderNo = RandomHelper.GetOrderNo("UNI"),
                    MerchantID = queryMerchant.MerchantID,
                    MerchantSecretKey = queryMerchant.ApiSecretKey,
                    PlayerName = "test_775522"
                });

                Assert.IsNotNull(transfer_out);
                Assert.IsTrue((transfer_out.BeforeAmount - transfer_out.AfterAmount) == outAmount * convertRate);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(transfer_out.TransactionNo));
            }

            return;
        }





        [TestMethod]
        public void TransferCashToPlatformWallet_Test()
        {
            decimal myAmount = RandomHelper.GetRandIntegerNumbers(11 , 19);
            var query = ServiceInstance.TransferCashToPlatformWallet(new WalletTransferRequestModel
            {
                Amount = myAmount,
                MerchantOrderNo = RandomHelper.GetOrderNo("UNI"),
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = _TEST_DATA.Default.PlayerName
            });

            Assert.IsNotNull(query);
            Assert.IsTrue(query.BeforeAmount > 0);
            Assert.IsTrue((query.BeforeAmount - query.AfterAmount) == myAmount);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(query.TransactionNo));
            return;
        }




        [TestMethod]
        public void Transfer_DuplicateOrderNo_Test()
        {
            string myOrderNo = RandomHelper.GetOrderNo("UNI");

            var param = new WalletTransferRequestModel
            {
                Amount = 10,
                MerchantOrderNo = myOrderNo,
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                PlayerName = _TEST_DATA.Default.PlayerName
            };

            var query1 = ServiceInstance.TransferCashToGameWallet(param);

            AssertExpectExceptionProcess(() =>
            {
                var query2 = ServiceInstance.TransferCashToGameWallet(param);

            }, ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            return;
        }



        [TestMethod]
        public void TransferCashToPlatformWallet_SingleWalletMerchant_Test()
        {
            try
            {
                var query = ServiceInstance.TransferCashToPlatformWallet(new WalletTransferRequestModel
                {
                    Amount = 10,
                    MerchantOrderNo = RandomHelper.GetOrderNo("UNI"),
                    MerchantID = _TEST_DATA.SingleWallet.MerchantID,
                    MerchantSecretKey = _TEST_DATA.SingleWallet.MerchantSecret,
                    PlayerName = _TEST_DATA.Default.PlayerName
                });

                AssertFail();
            }
            catch(Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.BusinessLogicDeny);
            }

            return;
        }



        [TestMethod]
        public void TransferCashToGameWallet_AmountTooSmall_Test()
        {
            AssertExpectExceptionProcess(() =>
            {
                var result = ServiceInstance.TransferCashToGameWallet(new WalletTransferRequestModel
                {
                    Amount = 0.001M,
                    MerchantOrderNo = RandomHelper.GetOrderNo("UNI"),
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    MerchantSecretKey = "XXXXXXXXXXXXXXXXX_Fail",
                    PlayerName = _TEST_DATA.Default.PlayerName
                });

            }, ApiErrorCode.InvalidParameter_NumberValueTooSmall);

            return;
        }


        [TestMethod]
        public void TransferCashToGameWallet_WrongSecretKey_Test()
        {
            try
            {
                var result = ServiceInstance.TransferCashToGameWallet(new WalletTransferRequestModel
                {
                    Amount = 10,
                    MerchantOrderNo = RandomHelper.GetOrderNo("UNI"),
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    MerchantSecretKey = "XXXXXXXXXXXXXXXXX_Fail",
                    PlayerName = _TEST_DATA.Default.PlayerName
                });

                AssertFail();
            }
            catch (Exception ex)
            {
                AssertExpectException(ex, new List<ApiErrorCode>
                {
                    ApiErrorCode.NotFoundMerchant,
                    ApiErrorCode.NotFoundMerchantOrWrongApiKey
                });
            }           

            return;
        }


    }
}

