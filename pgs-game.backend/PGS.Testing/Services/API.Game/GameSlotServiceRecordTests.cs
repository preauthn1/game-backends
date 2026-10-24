using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Linq;

namespace target.Testing
{

    /// <summary>
    /// AI寫的
    /// </summary>
    [TestClass]
    [AIGenerate]
    public class GameSlotServiceRecordTests : BaseTest
    {
        public PlayerSlotBonusRecordRepository PlayerSlotBonusRecordRepository { get; set; }

        public PlayerSlotFreeSpinRecordRepository PlayerSlotFreeSpinRecordRepository { get; set; }

        public PlayerSlotLightningRecordRepository PlayerSlotLightningRecordRepository { get; set; }

        private GameSlotService ServiceInstance { get; set; }

        public GameSlotServiceRecordTests()
        {
            ServiceInstance = new GameSlotService
            {
                PlayerSlotBonusRecordRepository = PlayerSlotBonusRecordRepository,
                PlayerSlotFreeSpinRecordRepository = PlayerSlotFreeSpinRecordRepository,
                PlayerSlotLightningRecordRepository = PlayerSlotLightningRecordRepository
            };
        }

        [TestMethod]
        public void SlotBonusRecord_CreateUpdateGetReset_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            int gameID = _TEST_DATA.Default.GameID;
            int flagID = CreateUniqueFlagID();
            int originID = 101;

            try
            {
                var createModel = new GameSlotBonusRecordRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID,
                    OriginID = originID,
                    TurnID = 1,
                    Location = 2,
                    BetAmount = 10.5M,
                    BetRate = 3,
                    Data = "bonus-data",
                    PatternID = 11
                };

                ServiceInstance.UpdateSlotBonusRecord(createModel);

                var created = ServiceInstance.GetSlotBonusRecord(new GameSlotRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID
                }).SlotBonusRecord.Single();

                Assert.AreEqual(originID, created.OriginID);
                Assert.AreEqual(1, created.TurnID);
                Assert.AreEqual(2, created.Location);
                Assert.AreEqual(10.5M, created.BetAmount);
                Assert.AreEqual(3, created.BetRate);
                Assert.AreEqual("bonus-data", created.Data);
                Assert.AreEqual(11, created.PatternID);

                createModel.Location = 4;
                createModel.PatternID = 12;
                ServiceInstance.UpdateSlotBonusRecord(createModel);

                var updated = ServiceInstance.GetSlotBonusRecord(new GameSlotRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID
                }).SlotBonusRecord.Single();

                Assert.AreEqual(4, updated.Location);
                Assert.AreEqual(12, updated.PatternID);

                AssertExpectExceptionProcess(() =>
                {
                    ServiceInstance.UpdateSlotBonusRecord(new GameSlotBonusRecordRequestModel
                    {
                        PlayerID = playerID,
                        GameID = gameID,
                        FlagID = flagID,
                        OriginID = originID,
                        TurnID = 2,
                        Location = 4,
                        BetAmount = 1,
                        BetRate = 1
                    });
                }, ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

                ServiceInstance.ResetSlotBonusRecord(new GameSlotBonusRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID,
                    OriginID = originID
                });

                var resetResult = ServiceInstance.GetSlotBonusRecord(new GameSlotRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID
                });

                Assert.AreEqual(0, resetResult.SlotBonusRecord.Count);
            }
            finally
            {
                DeleteBonusRecords(playerID, gameID, flagID);
            }
        }

        [TestMethod]
        public void SlotFreeSpinRecord_CreateAndGet_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            int gameID = _TEST_DATA.Default.GameID;
            int flagID = CreateUniqueFlagID();

            try
            {
                ServiceInstance.UpdateSlotFreeSpinRecord(new GameSlotFreeSpinRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID,
                    OriginID = 201,
                    FreeSpinGroup = 2,
                    ChooseRate = 5,
                    BetAmount = 20.25M,
                    Times = 8,
                    WinAmount = 35.75M,
                    WinTimes = 3,
                    Data = "free-spin-data",
                    PatternID = 21
                });

                var result = ServiceInstance.GetSlotFreeSpinRecord(new GameSlotRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID
                }).SlotFreeSpinRecord.Single();

                Assert.AreEqual(201, result.OriginID);
                Assert.AreEqual(2, result.FreeSpinGroup);
                Assert.AreEqual(5, result.ChooseRate);
                Assert.AreEqual(20.25M, result.BetAmount);
                Assert.AreEqual(8, result.Times);
                Assert.AreEqual(35.75M, result.WinAmount);
                Assert.AreEqual(3, result.WinTimes);
                Assert.AreEqual("free-spin-data", result.Data);
                Assert.AreEqual(21, result.PatternID);
            }
            finally
            {
                DeleteFreeSpinRecords(playerID, gameID, flagID);
            }
        }

        [TestMethod]
        public void SlotFreeSpinRecord_GetMissingRecord_CreatesDefaultRecord_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            int gameID = _TEST_DATA.Default.GameID;
            int flagID = CreateUniqueFlagID();

            try
            {
                var result = ServiceInstance.GetSlotFreeSpinRecord(new GameSlotRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID
                }).SlotFreeSpinRecord.Single();

                Assert.AreEqual(0, result.OriginID);
                Assert.AreEqual(0, result.FreeSpinGroup);
                Assert.AreEqual(0, result.ChooseRate);
                Assert.AreEqual(0M, result.BetAmount);
                Assert.AreEqual(0, result.Times);
                Assert.AreEqual(0M, result.WinAmount);
                Assert.AreEqual(0, result.WinTimes);
                Assert.AreEqual(string.Empty, result.Data);
                Assert.AreEqual(0, result.PatternID);
            }
            finally
            {
                DeleteFreeSpinRecords(playerID, gameID, flagID);
            }
        }

        [TestMethod]
        public void SlotLightningRecord_CreateUpdateDelete_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;
            int gameID = _TEST_DATA.Default.GameID;
            int flagID = CreateUniqueFlagID();
            var request = new GameSlotRequestModel
            {
                PlayerID = playerID,
                GameID = gameID,
                FlagID = flagID
            };

            try
            {
                ServiceInstance.UpdateSlotLightningRecord(new GameSlotLightningRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID,
                    Data = "lightning-data",
                    PatternID = 31
                });

                var created = ServiceInstance.GetSlotLightningRecord(request).SlotLightingRecord.Single();
                Assert.AreEqual("lightning-data", created.Data);
                Assert.AreEqual(31, created.PatternID);

                ServiceInstance.UpdateSlotLightningRecord(new GameSlotLightningRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID,
                    Data = "updated-lightning-data",
                    PatternID = 32
                });

                var updated = ServiceInstance.GetSlotLightningRecord(request).SlotLightingRecord.Single();
                Assert.AreEqual("updated-lightning-data", updated.Data);
                Assert.AreEqual(32, updated.PatternID);

                ServiceInstance.UpdateSlotLightningRecord(new GameSlotLightningRequestModel
                {
                    PlayerID = playerID,
                    GameID = gameID,
                    FlagID = flagID,
                    Data = string.Empty
                });

                var deleted = ServiceInstance.GetSlotLightningRecord(request);
                Assert.AreEqual(0, deleted.SlotLightingRecord.Count);
            }
            finally
            {
                DeleteLightningRecords(playerID, gameID, flagID);
            }
        }

        private static int CreateUniqueFlagID()
        {
            return Math.Max(1, Guid.NewGuid().GetHashCode() & int.MaxValue);
        }

        private void DeleteBonusRecords(int playerID, int gameID, int flagID)
        {
            var records = PlayerSlotBonusRecordRepository.GetByFilter(new QueryPlayerSlotBonusRecordRequestViewModel
            {
                PlayerID = playerID,
                GameID = gameID,
                FlagID = flagID
            });

            foreach (PlayerSlotBonusRecordEntity record in records)
            {
                PlayerSlotBonusRecordRepository.Delete(record.ID);
            }
        }

        private void DeleteFreeSpinRecords(int playerID, int gameID, int flagID)
        {
            var records = PlayerSlotFreeSpinRecordRepository.GetByFilter(new QueryPlayerSlotFreeSpinRecordRequestViewModel
            {
                PlayerID = playerID,
                GameID = gameID,
                FlagID = flagID
            });

            foreach (PlayerSlotFreeSpinRecordEntity record in records)
            {
                PlayerSlotFreeSpinRecordRepository.Delete(record.ID);
            }
        }

        private void DeleteLightningRecords(int playerID, int gameID, int flagID)
        {
            var records = PlayerSlotLightningRecordRepository.GetByFilter(new QueryPlayerSlotLightningRecordRequestViewModel
            {
                PlayerID = playerID,
                GameID = gameID,
                FlagID = flagID
            });

            foreach (PlayerSlotLightingRecordEntity record in records)
            {
                PlayerSlotLightningRecordRepository.Delete(record.ID);
            }
        }
    }
}
