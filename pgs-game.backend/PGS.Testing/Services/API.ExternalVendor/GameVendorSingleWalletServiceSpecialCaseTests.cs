using game.Entity.Game;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Org.BouncyCastle.Asn1.Ocsp;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
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
    public partial class GameVendorSingleWalletServiceSpecialCaseTests : BaseTest
    {
        public GameListRepository GameListRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogAccountingIgnoreRepository PerGameDetailLogAccountingIgnoreRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public IGameVendorSingleWalletService ServiceInstance { get; set; }



        [TestMethod]
        public void GameVendor_ItemCard_AGG_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;

            var wallet_before = PlayerWalletRepository.GetPlayerWalletByType(playerID, WalletTypeEnum.GameWallet);

            var result = DoMSTest_GameVendor_ItemCard_AGG_Test(playerID, _TEST_DATA.Default.MerchantID);

            var wallet_after = PlayerWalletRepository.GetPlayerWalletByType(playerID, WalletTypeEnum.GameWallet);

            Assert.IsTrue((wallet_after.Amount - wallet_before.Amount) == result.UpdateAmount);       
            return;
        }



        [TestMethod]
        public void GameVendor_RG_SignHash_Test()
        {
            var RGService = new RGIntegrationService();

            MockCommonHttpHeader("POST");

            var rawBody = @"{""request_datetime"":""2025-02-19 17:10:02"",""transaction_datetime"":""2025-02-19 09:10:02"",""result_type"":1,""result_id"":60690,""bet_id"":60689,""game_code"":""1101"",""game_type"":""Slots"",""game_name"":""Fortune Creature"",""member_code"":""1000027"",""merchant_code"":""SMSTGPEGASUS"",""agent_code"":""SMSTGPEGASUSMA"",""bet_amount"":0.25,""total_payout"":0,""valid_bet_amount"":0.25,""payout"":0,""win_lose"":-0.25,""bet_completed"":1,""currency"":""MYR"",""hash"":""945cdd198a211df02e5ba3ee347a386ff3490a7cbeba96caea2dcce7b5f71724""}";

            HeaderValueHelper.AppendOverridePayaloadForLog(rawBody);

            RGService.CheckRequestSignHashIsValid(new RGSignHashCheckParameter
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                AssignMerchantCode = "SMSTGPEGASUS",
                AssignAgentCode = "SMSTGPEGASUSMA",
                PlayerID = 1000027
            });

            return;
        }



        private (string Memo , decimal UpdateAmount) DoMSTest_GameVendor_ItemCard_AGG_Test(int targetPlayerID, int targetMerchantID)
        {
            GameVendorTypeEnum targetGameVendor = GameVendorTypeEnum.AGG;

            int merchantID = targetMerchantID;
            var randomTestData = GenerateTestRandomData(targetGameVendor, merchantID);

            var matchFirst = randomTestData.GameOfThisVendor;
            string orderNo = randomTestData.OrderNo;
            decimal betAmount = randomTestData.BetAmount;
            decimal payoutAmount = randomTestData.PayoutAmount;
            var prizeType = EventPrizeTypeEnum.AGG_ItemCard;

            var resultPlaceBet = ServiceInstance.PlaceBet(targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = targetPlayerID,
                BetAmount = betAmount,
                BetRecordNo = orderNo,
                GameCode = matchFirst.ThirdPartyGameCode,
                SessionToken = [REDACTED],
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    ReferEventPrizeType = prizeType
                },
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
                SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    ReferEventPrizeType = prizeType
                },
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
                                }
                            }
                        }
                    }
                }
            });

            //通用assert檢查項目 
            AssertGeneralProcessAfterSettleBet(resultPlaceBet, resultSettle, betAmount, payoutAmount);

            return ("" , payoutAmount);
        }


        private (
               GameListEntity GameOfThisVendor,
               decimal BetAmount,
               decimal PayoutAmount,
               string OrderNo
           )
       GenerateTestRandomData(GameVendorTypeEnum targetGameVendor, int merchantID, List<int> specificGameIDs = null)
        {
            var queryMerchangGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                AssignMerchantIDs = merchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                GameIDs = specificGameIDs,
                GameVendorTypes = targetGameVendor.ToInteger().ToListCollection(),
            });

            var matchFirst = queryMerchangGames
                .Where(x => x.GameID % 1000 != 999)
                .ToList()
                .OrderByRandom()
                .Take(20)
                .OrderBy(x => x.IsSupportSingleWallet ? 0 : 999) //刻意讓raise類型的遊戲排序在後面
                .FirstOrDefault();


            if (matchFirst == null)
                throw new TerproException(ApiErrorCode.NotFoundGame, "商戶沒開通遊戲?");

            string orderNo = Guid.NewGuid().ToString();
            decimal betAmount = RandomHelper.GetRandIntegerNumbers(100, 150);
            decimal payoutAmount = RandomHelper.GetRandIntegerNumbers(100, 300);

            return (matchFirst, betAmount, payoutAmount, orderNo);
        }



        private PerGameDetailLogAccountingIgnoreEntity AssertGeneralProcessAfterSettleBet(
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

            var queryPerGame = PerGameDetailLogAccountingIgnoreRepository.GetByFilter(new QueryPerGameDetailLogAccountingIgnoreRequestViewModel
                {
                    TransactionNo = pgsTransactionNo,
                    GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1),
                    GameStartTimeEnd = DateTimeHelper.GetCurrentTime()
                })
                .FirstOrDefault();

            Assert.IsNotNull(queryPerGame);
            Assert.IsTrue(queryPerGame.BetAmount == totalBetAmount);
            Assert.IsTrue(queryPerGame.PayoutAmount == payoutAmount);

            var queryPerGame_original = PerGameDetailLogRepository.GetByFilter(new QueryDetailLogRequestViewModel
            {
                TransactionNo = pgsTransactionNo,
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime()
            })
            .FirstOrDefault();

            Assert.IsTrue(queryPerGame_original == null);

            //派獎記錄
            var queryPrize = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                {
                    CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1),
                    CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
                    ThirdOrderNo = resultSettle.CommonInfo.BetRecordNo
                })
                .FirstOrDefault();

            Assert.IsTrue(queryPrize != null);
            Assert.IsTrue(queryPrize.UpdateAmount == resultSettle.CommonInfo.WalletUpdateAmount);

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

            //var queryItemCard = BetRecordTagMappingRepository.GetByUniqueKey(pgsTransactionNo, (int)SystemTagEnum.BetRecord_VirtualItemCard);
            //Assert.IsTrue(queryItemCard != null);

            var queryReferPrzie = BetRecordCustomizedSettingRepository.GetByUniqueKey(pgsTransactionNo, BetRecordCustomizedSettingEnum.ReferEventPrize);
            Assert.IsTrue(new SystemConfigSettingParseModel(0, queryReferPrzie.SettingValue).SettingValueOfInt == (int)EventPrizeTypeEnum.AGG_ItemCard);

            return queryPerGame;
        }

    }
}

