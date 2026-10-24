using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using System.Collections.Generic;
using target.Utility;

namespace target.Testing
{
    [TestClass]
    public class GameResultServiceTests : BaseTest
    {
        public IGameResultService GameResultService { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }


        [TestMethod]
        public void WriteSettlementResult_Test()
        {
            var result = GameResultService.UpdateSettlementAmount(new GamePlayResultRequestModel
            {
                PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
                ServerID = _TEST_DATA.Default.ServerID,
                TradeID = _TEST_DATA.Default.TradeID,
                UpdateAmount = 10,
            });

            Assert.IsNotNull(result);
            return;
        }


        [TestMethod]
        public void UpdateSettlementAmount_V2_Test()
        {
            var result = GameResultService.UpdateSettlementAmount(new GamePlayResultRequestModel
            {
                PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
                ServerID = _TEST_DATA.Default.ServerID,
                TradeID = _TEST_DATA.Default.TradeID,
                UpdateAmount = 10,
                IsPlaceBetProcess = true,
                IPAddress = _TEST_DATA.Default.IPAddress,
                BetInfo = new SettlementBetInfoRequestModel
                {
                    TransactionNo = _TEST_DATA.Default.TransactionNo_PgsBet,
                    BetAmount = 10,
                    PayoutAmount = 10,
                    ValidAmount = 10
                }
            });

            Assert.IsNotNull(result);
            return;
        }


        [TestMethod]
        public void WritePerGameLog_V2_Test()
        {
            decimal amount = _TEST_DATA.Default.BetAmount_MustDelete;
            string pgsOrderNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            var result = GameResultService.WritePerGameLog(new PerGameLogRequestModel
            {
                PerGameLog = new PerGameLogItemRequestModel
                {
                    TaxAmount = 0,
                    GameData = "遊戲歷程記錄blabla",
                    GameID = _TEST_DATA.Default.GameID,
                    GameBeginTime = DateTimeHelper.GetCurrentTime(),
                    GameEndTime = DateTimeHelper.GetCurrentTime().AddMinutes(2),
                    ServerID = _TEST_DATA.Default.ServerID,
                    UserGameDetailLogs = new List<PerGameDetailLogInfoViewModel>
                    {
                        new PerGameDetailLogInfoViewModel
                        {
                            BetAmount = amount,
                            CurrentAmount = amount,
                            UpdateAmount = amount,
                            ValidAmount = amount ,
                            PayoutAmount = amount,
                            PlayerID = _TEST_DATA.NotSuperMerchant.PlayerID,
                            TradeID = _TEST_DATA.Default.TradeID_MustDelete,
                            TransactionNoList = new List<string>
                            {
                                pgsOrderNo
                            },
                            TransactionNo = pgsOrderNo,
                        }
                    },
                    
                    //UserGameDetail = $"{_TEST_DATA.UnitTestMerchant.PlayerID}:{amount}:{amount}:{_TEST_DATA.Default.TradeID_MustDelete}"
                }
            });

            Assert.IsNotNull(result);
            return;
        }



        [TestMethod]
        public void WritePerGameLog_V2_MultipePlayers_Test()
        {
            decimal amount = _TEST_DATA.Default.BetAmount_MustDelete;
            string pgsOrderNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
            string pgsOrderNo2 = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
            string pgsOrderNo3 = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            int playerID_1 = _TEST_DATA.NotSuperMerchant.PlayerID;
            int merchantID_1 = _TEST_DATA.NotSuperMerchant.MerchantID;
            string playerName_1 = _TEST_DATA.NotSuperMerchant.PlayerName;

            var result = GameResultService.WritePerGameLog(new PerGameLogRequestModel
            {
                PerGameLog = new PerGameLogItemRequestModel
                {
                    TaxAmount = 0,
                    GameData = "遊戲歷程記錄blabla",
                    GameID = _TEST_DATA.Default.GameID,
                    GameBeginTime = DateTimeHelper.GetCurrentTime(),
                    GameEndTime = DateTimeHelper.GetCurrentTime().AddMinutes(2),
                    ServerID = _TEST_DATA.Default.ServerID,
                    UserGameDetailLogs = new List<PerGameDetailLogInfoViewModel>
                    {
                        new PerGameDetailLogInfoViewModel
                        {
                            BetAmount = amount,
                            CurrentAmount = amount,
                            UpdateAmount = amount,
                            ValidAmount = amount ,
                            PayoutAmount = amount,
                            PlayerID = playerID_1,
                            TradeID = _TEST_DATA.Default.TradeID_MustDelete,
                            TransactionNoList = new List<string>
                            {
                                pgsOrderNo
                            },
                            TransactionNo = pgsOrderNo,
                        },
                        new PerGameDetailLogInfoViewModel
                        {
                            BetAmount = amount,
                            CurrentAmount = amount,
                            UpdateAmount = amount,
                            ValidAmount = amount ,
                            PayoutAmount = amount + 10,
                            PlayerID = _TEST_DATA.Default.PlayerID,
                            TradeID = _TEST_DATA.Default.TradeID_MustDelete,
                            TransactionNoList = new List<string>
                            {
                                pgsOrderNo2
                            },
                            TransactionNo = pgsOrderNo2,
                        },
                        new PerGameDetailLogInfoViewModel
                        {
                            BetAmount = amount,
                            CurrentAmount = amount,
                            UpdateAmount = amount,
                            ValidAmount = amount ,
                            PayoutAmount = amount + 50,
                            PlayerID = _TEST_DATA.SingleWallet.PlayerID,
                            TradeID = _TEST_DATA.Default.TradeID_MustDelete,
                            TransactionNoList = new List<string>
                            {
                                pgsOrderNo3
                            },
                            TransactionNo = pgsOrderNo3,
                        }
                    },
                }
            });

            Assert.IsNotNull(result);

            var queryPerGame1 = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(pgsOrderNo);
            var queryPerGame2 = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(pgsOrderNo2);

            Assert.IsTrue(queryPerGame1.MerchantID == merchantID_1);
            Assert.IsTrue(queryPerGame1.PlayerID == playerID_1);
            Assert.IsTrue(queryPerGame1.PlayerName == playerName_1);

            Assert.IsTrue(queryPerGame2.MerchantID > 0 );
            Assert.IsTrue(queryPerGame2.PlayerName.IsNotEmptyTextNotNull());
            return;
        }


    }
}

