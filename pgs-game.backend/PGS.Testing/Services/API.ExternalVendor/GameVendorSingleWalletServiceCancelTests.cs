using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace target.Testing
{
    public partial class GameVendorSingleWalletServicePlaceBetTests : BaseTest
    {

        /// <summary>
        /// 取消注單
        /// </summary>
        [TestMethod]
        public void GameVendor_AGG_Cancel_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGG;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGG = new AGGApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new AGGSingleWalletAPITransactionRequestModel
                        {
                            WagerId = orderNo,
                            TransactionId = orderNo,
                            GameId = thirdGameCode,
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);

            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }



        [TestMethod]
        public void GameVendor_Cancel_AGGLive_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGGLive;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGGLive = new AGGLiveApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new AGGLivePlaceBetDetailRequestModel
                        {
                            bet = betAmount,
                            gameid = thirdGameCode,
                            betnumber = orderNo,
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }

        [TestMethod]
        public void GameVendor_Cancel_YesGame_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.YesGame;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    YesGame = new AGGLiveApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new AGGLivePlaceBetDetailRequestModel
                        {
                            bet = betAmount,
                            gameid = thirdGameCode,
                            betnumber = orderNo,
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }


        [TestMethod]
        public void GameVendor_Cancel_Jili_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Jili;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;
            string raiseOrderNo = Guid.NewGuid().ToString();

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                RaiseTransactionNo = raiseOrderNo,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            game = thirdGameCode,
                            round = orderNo,
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                RaiseTransactionNo = raiseOrderNo,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }


        /// <summary>
        /// 取消注單 (加注型注單)
        /// </summary>
        [TestMethod]
        public void GameVendor_CancelRaiseRecord_Jili_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Jili;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var gameIDs = _TEST_DATA.Default.SupportRaiseProcessGameIDs;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID, gameIDs);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            string raiseOrderNo1 = Guid.NewGuid().ToString();
            string raiseOrderNo2 = Guid.NewGuid().ToString();


            var resultPlaceBet1 = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                RaiseTransactionNo = raiseOrderNo1,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            game = thirdGameCode,
                            round = raiseOrderNo1,
                            sessionId = orderNo
                        }
                    }
                }
            });


            var resultPlaceBet2 = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                RaiseTransactionNo = raiseOrderNo2,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            game = thirdGameCode,
                            round = raiseOrderNo2,
                            sessionId = orderNo
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet1.CommonInfo);
            Assert.IsNotNull(resultPlaceBet2.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                RaiseTransactionNo = raiseOrderNo2,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);


            string pgsTransactionNo = resultPlaceBet1.CommonInfo.BetRecordNoPGS;

            AssertRecordStatusIsChanged(targetGameVendor, pgsTransactionNo, BetRecordStatusEnum.Pending);


            var query = PlaceBetRaiseRecordRepository.GetByFilter(new QueryPlaceBetRaiseRecordRequestViewModel
            {
                BetRecordNoPGS = pgsTransactionNo,
            });

            Assert.IsTrue(query.Any(x => x.Status == BetRecordRaiseBetStatusEnum.Cancel.ToInteger()));
            Assert.IsTrue(query.Any(x => x.Status == BetRecordRaiseBetStatusEnum.Accept.ToInteger()));
            return;
        }


        [TestMethod]
        public void GameVendor_Cancel_AT_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AT;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AT = new ATApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new ATBetAPIRequestModel
                        {
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }


        [TestMethod]
        public void GameVendor_Cancel_ATGames_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.ATGames;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AT = new ATApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new ATBetAPIRequestModel
                        {
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }


        [TestMethod]
        public void GameVendor_Cancel_ClotPlay_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.ClotPlay;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    ClotPlay = new ClotPlayApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new ClotPlayBetCallbackRequestModel
                        {

                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }




        [TestMethod]
        public void GameVendor_Cancel_RG_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.RG;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    RG = new RGApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new RGBetCallbackRequestModel
                        {

                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }


        [TestMethod]
        public void GameVendor_Cancel_SM_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.SlotMania;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    RG = new RGApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new RGBetCallbackRequestModel
                        {

                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }



        [TestMethod]
        public void GameVendor_Cancel_GPI_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.GPI;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    GPI = new GPIApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new GPIPlaceBetAPIRequestModel
                        {
                            bundle_id = orderNo,
                            round_id = Guid.NewGuid().ToString(),
                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }


        [TestMethod]
        public void GameVendor_Cancel_Galaxy_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Galaxy;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Galaxy = new GalaxyApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new GalaxyBetAPIRequestModel
                        {

                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }


        [TestMethod]
        public void GameVendor_Cancel_AGin_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGin;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;

            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            string thirdGameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGin = new AGinApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new AGinBetAPIRequestModel
                        {

                        }
                    }
                }
            });

            Assert.IsNotNull(resultPlaceBet.CommonInfo);

            var cancelResult = ServiceInstance.CancelBet(targetGameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = orderNo,
                GameCode = thirdGameCode,
            });

            Assert.IsNotNull(cancelResult.CommonInfo);
            AssertRecordStatusIsChanged(targetGameVendor, resultPlaceBet.CommonInfo.BetRecordNoPGS);
            return;
        }




        private void AssertRecordStatusIsChanged
            (
                GameVendorTypeEnum vendor,
                string pgsTransactionNo,
                BetRecordStatusEnum expectedStatus = BetRecordStatusEnum.Cancel
            )
        {
            var vendorService = ServiceInstanceHelper.InstanceVendorService(vendor);

            var queryRecord = vendorService.GetByPGSTransactionNo(pgsTransactionNo);

            Assert.IsNotNull(queryRecord);
            Assert.IsTrue(queryRecord.Status == expectedStatus.ToInteger());
        }


    }
}

