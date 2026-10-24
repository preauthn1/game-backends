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
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public partial class GameVendorSingleWalletServicePlaceBetTests : BaseTest
    {
        public GameListRepository GameListRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogTagMappingRepository PerGameDetailLogTagMappingRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public PlaceBetRaiseRecordRepository PlaceBetRaiseRecordRepository { get; set; }

        public IGameVendorSingleWalletService ServiceInstance { get; set; }



        [TestMethod]
        public void GameVendor_PlaceBet_Funky_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Funky;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Funky = new FunkyApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new FunkySingleWalletAPIPlaceBetRequestModel
                        {
                            Bet = new FunkySingleWalletAPIPlaceBetDetailRequestModel
                            {
                                Stake = betAmount,
                                GameCode = matchFirst.ThirdPartyGameCode,
                                RefNo = orderNo
                            },
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Funky = new FunkyApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new FunkySingleWalletAPISettleBetRequestModel
                        {
                            betResultReq = new FunkySingleWalletAPISettleBetDetailRequestModel
                            {
                                stake = betAmount,
                                gameCode = matchFirst.ThirdPartyGameCode,
                                winAmount = payoutAmount
                            }
                        }
                    }
                }
            });


            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_Funky_ZeroBetAmount_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Funky;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = 0;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Funky = new FunkyApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new FunkySingleWalletAPIPlaceBetRequestModel
                        {
                            Bet = new FunkySingleWalletAPIPlaceBetDetailRequestModel
                            {
                                Stake = betAmount,
                                GameCode = matchFirst.ThirdPartyGameCode,
                                RefNo = orderNo
                            },
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Funky = new FunkyApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new FunkySingleWalletAPISettleBetRequestModel
                        {
                            betResultReq = new FunkySingleWalletAPISettleBetDetailRequestModel
                            {
                                stake = betAmount,
                                gameCode = matchFirst.ThirdPartyGameCode,
                                winAmount = payoutAmount
                            }
                        }
                    }
                }
            });


            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }


        [TestMethod]
        public void GameVendor_PlaceBet_AGG_Test()
        {
            DoMSTest_GameVendor_PlaceBet_AGG_Test(_TEST_DATA.Default.PlayerID, _TEST_DATA.Default.MerchantID);
            return;
        }


        private void DoMSTest_GameVendor_PlaceBet_AGG_Test(int targetPlayerID, int targetMerchantID)
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGG;

            int merchantID = targetMerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var now = DateTimeHelper.GetCurrentTime();

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
                            {
                                new AGGSingleWalletOperationRequestModel
                                {
                                    OperationType = "WagerDepositOperation",
                                    Deposit = payoutAmount
                                },
                                new AGGSingleWalletOperationRequestModel
                                {
                                    OperationType = "WagerLifecycleOperation",
                                    ExecutionTime = now.ToString("yyyy-MM-dd HH:mm:ss")
                                },
                            }
                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }

        [TestMethod]
        public void GameVendor_PlaceBet_AGG_ZeroBetAmount_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGG;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = 0;

            try
            {
                var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    PlayerID = playerID,
                    BetAmount = betAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
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

                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected);
            }
            catch (Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.InvalidParameter_NumberEmptyZero);
            }

            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_AGG_PayoutAmountIsZero_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGG;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = 0;

            var now = DateTimeHelper.GetCurrentTime();
            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
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

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
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
                            UserID = playerID,
                            Operations = new List<AGGSingleWalletOperationRequestModel>
                            {
                                new AGGSingleWalletOperationRequestModel
                                {
                                    OperationType = "WagerLifecycleOperation",
                                    ExecutionTime = now.ToString("yyyy-MM-dd HH:mm:ss"),
                                }
                            }
                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_AGGLive_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGGLive;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGGLive = new AGGLiveApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new AGGLivePlaceBetDetailRequestModel
                        {
                            bet = betAmount,
                            gameid = matchFirst.ThirdPartyGameCode,
                            betnumber = orderNo,
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGGLive = new AGGLiveApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new AGGLiveSettleDetailRequestModel
                        {
                            bet = betAmount,
                            betnumber = orderNo,
                            fee = betAmount,
                            winlose = payoutAmount,
                            gameid = matchFirst.ThirdPartyGameCode,
                            roundid = Guid.NewGuid().ToString(),
                            bettime = DateTimeHelper.GetCurrentTime().ToString("yyyy-MM-dd")
                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_GPI_Test()
        { 
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄

            return;

            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.GPI;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    GPI = new GPIApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new GPIPlaceBetAPIRequestModel
                        {
                            amount = betAmount,
                            round_id = Guid.NewGuid().ToString(),
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    GPI = new GPIApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new GPISettleAPIRequestModel
                        {
                            amount = payoutAmount,
                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }


        [TestMethod]
        public void GameVendor_PlaceBet_CrazyFishing_Test()
        {
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄

            return;





            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.CrazyFishing;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;


            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Fishing = new FishingApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new FishingSingleWalletPlaceBetRequestModel
                        {
                            Stake = betAmount,
                            Account = playerID.ToString(),
                            OrderId = orderNo
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Fishing = new FishingApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new FishingSingleWalletSettleBetRequestModel
                        {
                            Stake = payoutAmount,
                            Cost = betAmount,
                            PlayId = playerID.ToString(),
                        }
                    }
                }
            });


            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }





        [TestMethod]
        public void GameVendor_PlaceBet_Jili_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Jili;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;


            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            userId = playerID,
                            round = orderNo,
                            wagersTime = DateTimeHelper.GetCurrentTimeTimestamp()
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,                
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            userId = playerID,
                            round = orderNo,
                            wagersTime = DateTimeHelper.GetCurrentTimeTimestamp(),
                        }
                    }
                }
            });


            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }


        [TestMethod]
        public void GameVendor_PlaceBetAndSettle_Jili_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Jili;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultSettle = ServiceInstance.PlaceBetAndSettle(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                RaiseTransactionNo = orderNo,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payoutAmount
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            userId = playerID,
                            round = orderNo,
                            sessionId = orderNo,
                        },
                        SettleBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            userId = playerID,
                            round = orderNo,
                            wagersTime = DateTimeHelper.GetCurrentTimeTimestamp(),
                        }
                    }
                }
            });

            var resultPlaceBet = new SingleWalletVendorCustomizedResponseConvertModel
            {
                CommonInfo = new GameVendorBetCommonInfo
                {
                    GameID = randomTestData.GameOfThisVendor.GameID,
                    BetRecordNoPGS = resultSettle.CommonInfo.BetRecordNoPGS,
                },
            };

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }



        [TestMethod]
        public void GameVendor_RaiseProcessGames_Jili_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Jili;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var gameIDs = _TEST_DATA.Default.SupportRaiseProcessGameIDs;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID , gameIDs);

            var matchFirstGame = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            string raiseOrderNo1 = Guid.NewGuid().ToString();
            string raiseOrderNo2 = Guid.NewGuid().ToString();

            randomTestData.GameOfThisVendor = matchFirstGame;

            var resultPlaceBet1 = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirstGame.ThirdPartyGameCode,
                RaiseTransactionNo = raiseOrderNo1,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payoutAmount
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            userId = playerID,
                            round = raiseOrderNo1,
                            sessionId = orderNo,
                        },
                    }
                }
            });

            var resultPlaceBet2 = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirstGame.ThirdPartyGameCode,
                RaiseTransactionNo = raiseOrderNo2,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payoutAmount
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            userId = playerID,
                            round = raiseOrderNo2,
                            sessionId = orderNo,
                        },
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirstGame.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new JiliBetAPIRequestModel
                        {
                            betAmount = betAmount,
                            userId = playerID,
                            round = orderNo,
                            sessionId = orderNo,
                            wagersTime = DateTimeHelper.GetCurrentTimeTimestamp(),
                        }
                    }
                }
            });

            //通用assert檢查項目 
            decimal totalBetAmount = betAmount * 2;
            var perGame = AssertGeneralProcessAfterSettleBet(resultPlaceBet1, resultSettle, totalBetAmount, payoutAmount);

            var queryRaiseRecords = PlaceBetRaiseRecordRepository.GetByFilter(new QueryPlaceBetRaiseRecordRequestViewModel
            {
                BetRecordNoPGS = resultSettle.CommonInfo.BetRecordNoPGS
            });

            Assert.IsTrue(queryRaiseRecords.Count() == 2);
            return;
        }


        private (
                    GameListEntity GameOfThisVendor,
                    decimal BetAmount,
                    decimal PayoutAmount,
                    string OrderNo
                ) 
            GenerateTestRandomData(GameVendorTypeEnum targetGameVendor, int merchantID , List<int> specificGameIDs = null)
        {
            var queryMerchangGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                AssignMerchantIDs = merchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                GameIDs = specificGameIDs,
                GameVendorTypes = targetGameVendor.ToInteger().ToListCollection(),
            });

            var matchFirst = queryMerchangGames
                .Where(x=> x.GameID % 1000 != 999)
                .ToList()
                .OrderByRandom()
                .Take(20)
                .OrderBy(x=> x.IsSupportSingleWallet ? 0 : 999) //刻意讓raise類型的遊戲排序在後面
                .FirstOrDefault();


            if (matchFirst == null)
                throw new TerproException(ApiErrorCode.NotFoundGame , "商戶沒開通遊戲?");

            string orderNo = Guid.NewGuid().ToString();
            decimal betAmount = RandomHelper.GetRandIntegerNumbers(100, 150);
            decimal payoutAmount = RandomHelper.GetRandIntegerNumbers(100, 300);

            return (matchFirst, betAmount, payoutAmount, orderNo);
        }



        [TestMethod]
        public void GameVendor_PlaceBet_AGGFish_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGGFish;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            bool isCreateBetWhenSettleProcess = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(targetGameVendor).IsAutoCreateDataWhenSettleProcess;

            if(!isCreateBetWhenSettleProcess)
            {
                var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    PlayerID = playerID,
                    BetAmount = betAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AGGFish = new AGGFishApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = new AGGLivePlaceBetDetailRequestModel
                            {
                                bet = betAmount,
                                gameid = matchFirst.ThirdPartyGameCode,
                                betnumber = orderNo,
                            }
                        }
                    }
                });

                var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
                {
                    PlayerID = playerID,
                    PayoutGamePoint = payoutAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AGGFish = new AGGFishApiRawRequestInfoVendor
                        {
                            SettleBetInfo = new AGGFishSettleDetailRequestModel
                            {
                                winlose = payoutAmount,
                                bet = betAmount,
                                gameid = matchFirst.ThirdPartyGameCode,
                                betnumber = orderNo,
                                external = new AGGFishSettleDetailModel
                                {
                                    BetAmount = randomTestData.BetAmount + 100,
                                    Rounds = 10
                                }
                            }
                        }
                    }
                });

                //通用assert檢查項目 
                AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            }

            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_YesGame_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.YesGame;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            bool isCreateBetWhenSettleProcess = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(targetGameVendor).IsAutoCreateDataWhenSettleProcess;

            if (!isCreateBetWhenSettleProcess)
            {
                var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    PlayerID = playerID,
                    BetAmount = betAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        YesGame = new AGGLiveApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = new AGGLivePlaceBetDetailRequestModel
                            {

                            }
                        }
                    }
                });

                var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
                {
                    PlayerID = playerID,
                    PayoutGamePoint = payoutAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        YesGame = new AGGLiveApiRawRequestInfoVendor
                        {
                            SettleBetInfo = new AGGLiveSettleDetailRequestModel
                            {
                                bettime = DateTimeHelper.GetCurrentTime().ToString("yyyy-MM-dd")
                            }
                        }
                    }
                });

                //通用assert檢查項目 
                AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            }

            return;
        }


        [TestMethod]
        public void GameVendor_PlaceBet_Galaxy_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Galaxy;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            bool isCreateBetWhenSettleProcess = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(targetGameVendor).IsAutoCreateDataWhenSettleProcess;

            if (!isCreateBetWhenSettleProcess)
            {
                var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    PlayerID = playerID,
                    BetAmount = betAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        Galaxy = new GalaxyApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = new GalaxyBetAPIRequestModel
                            {
                                SettlementTime = DateTimeHelper.GetCurrentTime(),
                                Amount = betAmount
                            }
                        }
                    }
                });

                var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
                {
                    PlayerID = playerID,
                    PayoutGamePoint = payoutAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        Galaxy = new GalaxyApiRawRequestInfoVendor
                        {
                            SettleBetInfo = new GalaxyBetAPIRequestModel
                            {
                                SettlementTime = DateTimeHelper.GetCurrentTime(),
                                Amount = betAmount
                            }
                        }
                    }
                });

                //通用assert檢查項目 
                AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            }

            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_AGin_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGin;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            bool isCreateBetWhenSettleProcess = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(targetGameVendor).IsAutoCreateDataWhenSettleProcess;

            if (!isCreateBetWhenSettleProcess)
            {
                var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    PlayerID = playerID,
                    BetAmount = betAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AGin = new AGinApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = new AGinBetAPIRequestModel
                            {
                                Settle_SettlementTime = DateTimeHelper.GetCurrentTime(),
                                BetAmount = betAmount
                            }
                        }
                    }
                });

                var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
                {
                    PlayerID = playerID,
                    PayoutGamePoint = payoutAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AGin = new AGinApiRawRequestInfoVendor
                        {
                            SettleBetInfo = new AGinBetAPIRequestModel
                            {
                                Settle_SettlementTime = DateTimeHelper.GetCurrentTime(),
                                BetAmount = betAmount
                            }
                        }
                    }
                });

                //通用assert檢查項目 
                AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            }

            return;
        }




        [TestMethod]
        public void GameVendor_PlaceBet_AT_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AT;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            bool isCreateBetWhenSettleProcess = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(targetGameVendor).IsAutoCreateDataWhenSettleProcess;

            if (!isCreateBetWhenSettleProcess)
            {
                var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    PlayerID = playerID,
                    BetAmount = betAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AT = new ATApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = new ATBetAPIRequestModel
                            {
                                betAmount = betAmount,
                            }
                        }
                    }
                });

                var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
                {
                    PlayerID = playerID,
                    PayoutGamePoint = payoutAmount,
                    BetRecordNo = orderNo,
                    GameCode = matchFirst.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AT = new ATApiRawRequestInfoVendor
                        {
                            SettleBetInfo = new ATBetAPIRequestModel
                            {
                                betAmount = 100,
                                wagersETime = DateTimeHelper.GetCurrentTimeTimestamp()
                            }
                        }
                    }
                });

                //通用assert檢查項目 
                AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            }

            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBetAndSettle_ATGames_Test()
        {
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄
            //此遊戲商已廢棄

            return;

            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.ATGames;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBetAndSettle(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payoutAmount
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    ATGames = new ATGanesApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new ATGamesBetCallbackModel
                        {
                            ctime = DateTimeHelper.GetCurrentTimeTimestamp(),
                        },
                        PlaceBetInfo = new ATGamesBetCallbackModel
                        {

                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultPlaceBet, betAmount, payoutAmount);
            return;
        }


        [TestMethod]
        public void GameVendor_PlaceBetAndSettle_ClotPlay_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.ClotPlay;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBetAndSettle(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payoutAmount
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    ClotPlay = new ClotPlayApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new ClotPlayBetCallbackRequestModel
                        {
                            updated_at = DateTimeHelper.GetCurrentTime().ToString("yyyy-MM-dd HH:mm:ss")
                        },
                        PlaceBetInfo = new ClotPlayBetCallbackRequestModel
                        {

                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultPlaceBet, betAmount, payoutAmount);
            return;
        }


        [TestMethod]
        public void GameVendor_Place_Settle_RG_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.RG;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;
            var now = DateTimeHelper.GetCurrentTime();

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
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

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    RG = new RGApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new RGSettleCallbackRequestModel
                        {
                            transaction_datetime = now.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);

            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_SM_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.SlotMania;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    SlotMania = new SlotManiaApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new SlotManiaBetAPIRequestModel
                        {
                            timestamp = DateTimeHelper.GetCurrentTimeTimestamp(),
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    SlotMania = new SlotManiaApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new SlotManiaBetAPIRequestModel
                        {
                            timestamp = DateTimeHelper.GetCurrentTimeTimestamp(),
                        }
                    }
                }
            });


            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_Fuma_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Fuma;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Fuma = new FumaApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new FumaAPIAllParameterRequestModel
                        {
                            ts = DateTimeHelper.GetCurrentTimeTimestamp()
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Fuma = new FumaApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new FumaAPIAllParameterRequestModel
                        {
                            ts = DateTimeHelper.GetCurrentTimeTimestamp()
                        }
                    }
                }
            });


            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBet_EFG_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.EFG;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    EFG = new EFGlayApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = new EFGPlaceBetAPIRequestModel
                        {
                            updateTime = DateTimeHelper.GetCurrentTimeTimestamp()
                        }
                    }
                }
            });

            var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    EFG = new EFGlayApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new EFGSettleAPIRequestModel
                        {
                            updateTime = DateTimeHelper.GetCurrentTimeTimestamp()
                        }
                    }
                }
            });


            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);
            return;
        }



        [TestMethod]
        public void GameVendor_PlaceBetAndSettle_Mario_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Mario;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.PlaceBetAndSettle(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payoutAmount
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Mario = new MarioApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new MarioGameResultAPIRequestModel
                        {
                           timestamp = DateTimeHelper.GetCurrentTimeTimestamp(),   
                        },
                        PlaceBetInfo = new MarioGameResultAPIRequestModel
                        {
                            
                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultPlaceBet, betAmount, payoutAmount);
            return;
        }


        [TestMethod]
        public void GameVendor_WriteBetBetRecordWithoutUpdateBalance_Mario_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.Mario;

            int playerID = _TEST_DATA.Default.PlayerID;
            int merchantID = _TEST_DATA.Default.MerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;

            var resultPlaceBet = ServiceInstance.WriteBetBetRecordWithoutUpdateBalance(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payoutAmount
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Mario = new MarioApiRawRequestInfoVendor
                    {
                        SettleBetInfo = new MarioGameResultAPIRequestModel
                        {
                            timestamp = DateTimeHelper.GetCurrentTimeTimestamp(),
                        },
                        PlaceBetInfo = new MarioGameResultAPIRequestModel
                        {

                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultPlaceBet, betAmount, payoutAmount);
            return;
        }


        [TestMethod]
        public void GameVendor_AGGFish_SettleBet_NotExistRecordNo_Test()
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGGFish;

            if (GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(targetGameVendor.ToInteger()).IsAutoCreateDataWhenSettleProcess)
            {
                int playerID = _TEST_DATA.Default.PlayerID;
                int merchantID = _TEST_DATA.Default.MerchantID;
                var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

                var matchFirst = randomTestData.GameOfThisVendor;
                string orderNo = randomTestData.OrderNo;
                decimal betAmount = randomTestData.BetAmount;
                decimal payoutAmount = randomTestData.PayoutAmount;

                var resultSettle = ServiceInstance.SettleBet(targetGameVendor, new GameVendorSettleCallbackParameter
                {
                    PlayerID = playerID,
                    SessionToken = [REDACTED],
                    PayoutGamePoint = payoutAmount,
                    BetRecordNo = orderNo,
                    GameCode = randomTestData.GameOfThisVendor.ThirdPartyGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AGGFish = new AGGFishApiRawRequestInfoVendor
                        {
                            SettleBetInfo = new AGGFishSettleDetailRequestModel
                            {
                                external = new AGGFishSettleDetailModel
                                {
                                    BetAmount = betAmount
                                }
                            }
                        }
                    }
                });

                var queryExist = PerGameDetailLogRepository.GetByFilter(new QueryDetailLogRequestViewModel
                {
                    TransactionNo = resultSettle.CommonInfo.BetRecordNoPGS,
                    GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1),
                    GameStartTimeEnd = DateTimeHelper.GetCurrentTime()
                })
                .FirstOrDefault();

                Assert.IsNotNull(queryExist);
                Assert.IsNotNull(queryExist.ValidAmount == betAmount);
                Assert.IsNotNull(queryExist.BetAmount == betAmount);
            }

            return;
        }




        private PerGameDetailLogEntity AssertGeneralProcessAfterSettleBet(
            SingleWalletVendorCustomizedResponseConvertModel resultPlaceBet,
            SingleWalletVendorCustomizedResponseConvertModel resultSettle,
            decimal totalBetAmount,
            decimal payoutAmount)
        {
            Assert.IsNotNull(resultPlaceBet.CommonInfo);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(resultSettle.CommonInfo.BetRecordNo));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(resultSettle.CommonInfo.BetRecordNoPGS));
            Assert.IsTrue(!string.IsNullOrWhiteSpace(resultSettle.CommonInfo.Currency));
            Assert.IsTrue(resultSettle.CommonInfo.WalletUpdateAmount == payoutAmount);
            Assert.IsNotNull(resultSettle.CommonInfo);

            string pgsTransactionNo = resultPlaceBet.CommonInfo.BetRecordNoPGS;

            var queryPerGame = PerGameDetailLogRepository.GetByFilter(new QueryDetailLogRequestViewModel
                {
                    TransactionNo = pgsTransactionNo,
                    GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1),
                    GameStartTimeEnd = DateTimeHelper.GetCurrentTime()
                })
                .FirstOrDefault();

            Assert.IsNotNull(queryPerGame);
            Assert.IsTrue(queryPerGame.BetAmount == totalBetAmount);
            Assert.IsTrue(queryPerGame.PayoutAmount == payoutAmount);


            var gameVendor = AppHelper.GetGameVendorByGameID(queryPerGame.GameID);

            //部分遊戲商的物件結構很複雜 懶得mock了
            //部分已下架的也直接無視
            if(gameVendor != GameVendorTypeEnum.AGG.ToInteger() &&
               gameVendor != GameVendorTypeEnum.CrazyFishing.ToInteger() &&
               gameVendor != GameVendorTypeEnum.GPI.ToInteger())
            {
                Assert.IsTrue(queryPerGame.ThirdSettleDate != null);
            }

            var gameVendorID = AppHelper.GetGameVendorByGameID(resultPlaceBet.CommonInfo.GameID);
            var gameVendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(gameVendorID);

            if(gameVendorDef.IsWriteAmountAfterSettle)
            {
                var setting_afterAmount = BetRecordCustomizedSettingRepository.GetParseModelByUniqueKey(queryPerGame.TransactionNo, BetRecordCustomizedSettingEnum.AmountAfterSettle);

                Assert.IsTrue(setting_afterAmount != null);
                Assert.IsTrue(setting_afterAmount.SettingValueOfDecimal == queryPerGame.AmountBeforeBet + queryPerGame.UpdateAmount);
            }

            IGameVendorIntegrationService vendorService = ServiceInstanceHelper.InstanceVendorService(gameVendor);
            var queryBetRecord = vendorService.GetByPGSTransactionNo(queryPerGame.TransactionNo);

            Assert.IsTrue(queryBetRecord != null);
            Assert.IsTrue(queryBetRecord.ThirdSettleTime != null);

            return queryPerGame;
        }

    }
}

