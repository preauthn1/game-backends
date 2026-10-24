using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{

    /// <summary>
    /// AI寫的
    /// </summary>
    [TestClass]
    [AIGenerate]
    public class BetRecordFetchAndConvertServiceTests : BaseTest
    {
        public VendorConfigRepository VendorConfigRepository { get; set;  }


        [TestMethod]
        public void FetchApiAndInsertToTable_UsesMockVendorWithoutExternalRequest_Test()
        {
            int vendorConfigID = _TEST_DATA.Default.VendorConfigID;

            var vendorService = Substitute.For<IGameVendorIntegrationService>();
            var requests = new List<ExternalVendorFetchWagersRequestParameter>();
            var fakeRecord = new ThirdPartyFetchRecordBriefEntity
            {
                WagerID = "mock-wager-001",
                GameCode = "mock-game",
                BetAmount = -10,
                PayoutAmount = 20,
                BetTime = new DateTime(2026, 1, 1, 0, 1, 0),
                SettleTime = new DateTime(2026, 1, 1, 0, 2, 0),
                VendorConfigID = vendorConfigID
            };

            vendorService
                .FetchWagers(Arg.Do<ExternalVendorFetchWagersRequestParameter>(request => requests.Add(request)))
                .Returns(new ExternalVendorFetchWagersResult
                {
                    PageSize = 100,
                    ConvertInsertEntity = new List<ThirdPartyFetchRecordBriefEntity> { fakeRecord }
                });

            var service = new MockBetRecordFetchAndConvertService
            {
                VendorConfig = new VendorConfigEntity
                {
                    VendorConfigID = vendorConfigID,
                    VendorID = (int)GameVendorTypeEnum.AGG
                },
                VendorService = vendorService
            };

            service.FetchApiAndInsertToTable(new WagetFetchJobParameter
            {
                BeginDateTW = new DateTime(2026, 1, 1, 8, 0, 0),
                EndDateTW = new DateTime(2026, 1, 1, 8, 10, 0),
                VendorConfigID = vendorConfigID,
                SplitMinutes = 60,
                GameCode = "mock-game"
            });

            Assert.AreEqual(1, requests.Count);
            Assert.AreEqual(new DateTime(2025, 12, 31, 23, 59, 0), requests[0].StartTimeUTC);
            Assert.AreEqual(new DateTime(2026, 1, 1, 0, 59, 0), requests[0].EndTimeUTC);
            Assert.AreEqual(vendorConfigID, requests[0].VendorConfigID);
            Assert.AreEqual("mock-game", requests[0].GameCode);
            Assert.AreEqual(1, requests[0].Page);

            Assert.AreEqual(1, service.InsertCalls.Count);
            Assert.AreEqual(GameVendorTypeEnum.AGG, service.InsertCalls[0].Vendor);
            Assert.AreEqual("mock-wager-001", service.InsertCalls[0].Items.Single().WagerID);
            Assert.AreEqual(1, service.CompletedMessages.Count);
            StringAssert.Contains(service.CompletedMessages[0], "FetchApiAndInsertToTable 結束");
        }

        [TestMethod]
        public void FetchApiAndInsertToTable_MockVendorMissingPageSize_Throws_Test()
        {
            int vendorConfigID = _TEST_DATA.Default.VendorConfigID;

            var vendorService = Substitute.For<IGameVendorIntegrationService>();
            vendorService
                .FetchWagers(Arg.Any<ExternalVendorFetchWagersRequestParameter>())
                .Returns(new ExternalVendorFetchWagersResult
                {
                    PageSize = 0,
                    ConvertInsertEntity = new List<ThirdPartyFetchRecordBriefEntity>()
                });

            var service = new MockBetRecordFetchAndConvertService
            {
                VendorConfig = new VendorConfigEntity
                {
                    VendorConfigID = vendorConfigID,
                    VendorID = (int)GameVendorTypeEnum.AGG
                },
                VendorService = vendorService
            };

            var exception = Assert.ThrowsException<TerproException>(() =>
                service.FetchApiAndInsertToTable(new WagetFetchJobParameter
                {
                    BeginDateTW = new DateTime(2026, 1, 1, 8, 0, 0),
                    EndDateTW = new DateTime(2026, 1, 1, 8, 10, 0),
                    VendorConfigID = vendorConfigID,
                    SplitMinutes = 60,
                    GameCode = "mock-game"
                }));

            Assert.AreEqual(ApiErrorCode.BusinessLogicDeny, exception.ErrroCode);
            Assert.AreEqual(0, service.InsertCalls.Count);
            Assert.AreEqual(0, service.CompletedMessages.Count);
        }

        [TestMethod]
        public void FetchApiAndInsertToTable_VendorConfigNotFound_DoesNotCallVendor_Test()
        {
            var vendorService = Substitute.For<IGameVendorIntegrationService>();
            var service = new MockBetRecordFetchAndConvertService
            {
                VendorConfig = null,
                VendorService = vendorService
            };

            AssertExpectExceptionProcess(() =>
            {
                service.FetchApiAndInsertToTable(new WagetFetchJobParameter
                {
                    BeginDateTW = new DateTime(2026, 1, 1, 8, 0, 0),
                    EndDateTW = new DateTime(2026, 1, 1, 8, 10, 0),
                    VendorConfigID = 789,
                    SplitMinutes = 60
                });

            }, ApiErrorCode.NotFoundVendorConfig);
           
            vendorService.DidNotReceive()
                .FetchWagers(Arg.Any<ExternalVendorFetchWagersRequestParameter>());

            Assert.AreEqual(0, service.InsertCalls.Count);
            Assert.AreEqual(0, service.CompletedMessages.Count);
        }

        private class MockBetRecordFetchAndConvertService : BetRecordFetchAndConvertService
        {
            public VendorConfigEntity VendorConfig { get; set; }

            public IGameVendorIntegrationService VendorService { get; set; }

            public List<InsertCall> InsertCalls { get; } = new List<InsertCall>();

            public List<string> CompletedMessages { get; } = new List<string>();

            protected override VendorConfigEntity GetVendorConfig(int vendorConfigID)
            {
                return VendorConfig;
            }

            protected override IGameVendorIntegrationService GetVendorService(GameVendorTypeEnum gameVendor)
            {
                return VendorService;
            }

            protected override void InsertToDB(
                GameVendorTypeEnum vendor,
                List<ThirdPartyFetchRecordBriefEntity> insertItems)
            {
                InsertCalls.Add(new InsertCall
                {
                    Vendor = vendor,
                    Items = insertItems.ToList()
                });
            }

            protected override void SendCompletedMessage(string message)
            {
                CompletedMessages.Add(message);
            }
        }

        private class InsertCall
        {
            public GameVendorTypeEnum Vendor { get; set; }

            public List<ThirdPartyFetchRecordBriefEntity> Items { get; set; }
        }
    }
}
