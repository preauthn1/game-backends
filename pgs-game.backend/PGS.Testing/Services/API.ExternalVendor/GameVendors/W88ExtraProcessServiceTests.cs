using game.Entity.Game;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
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

namespace target.Testing
{
    [TestClass]
    public class W88ExtraProcessServiceTests : BaseTest
    {

        public W88ExtraProcessService W88ExtraProcessService { get; set; }

        public MockW88ExtraProcessService MyMockW88ExtraProcessService { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }


        public W88ExtraProcessServiceTests()
        {
            W88ExtraProcessService = new W88ExtraProcessService();
            MyMockW88ExtraProcessService = new MockW88ExtraProcessService();
        }


        public class MockW88ExtraProcessService : W88ExtraProcessService
        {
            protected override W88TransferApiResultModel CallW88TransferApi(WalletTransferTypeEnum type, AGGFishTransferApiRequestaModel model, int playerID, string pgsOrderNo)
            {
                return new W88TransferApiResultModel
                {
                    AfterBalance = RandomHelper.GetRandIntegerNumbers(10000 , 99999),
                    W88TransactionNo = "MSTest_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS()
                };
            }
        }


        [TestMethod]
        public void GameWalletTransfer_Test()
        {
            string token = [REDACTED];
            int playerID = _TEST_DATA.Default.PlayerID;

            var param = new AGGFishTransferApiRequestaModel
            {
                Amount = 100,
                Token = [REDACTED],
                TxtID = token
            };

            var result1 = MyMockW88ExtraProcessService.GameWalletTransfer(WalletTransferTypeEnum.Transfer_In, playerID, param);

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result1.TxtID);
            Assert.IsTrue(result1.AfterBalance > 0);

            var result2 = MyMockW88ExtraProcessService.GameWalletTransfer(WalletTransferTypeEnum.Transfer_In, playerID, param);

            Assert.IsNotNull(result2);
            Assert.IsTrue(result1.TxtID == result2.TxtID);
            Assert.IsTrue(result1.AfterBalance == result2.AfterBalance);
            return;
        }



        [TestMethod]
        public void WriteBonusRecord_Test()
        {
            var result = W88ExtraProcessService.WriteBonusRecord(_TEST_DATA.Default.PlayerID, new AGGFishBonusRecordApiRequestaModel
            {
                Amount = 100,
                Bonustype = 1,
                BonustypeDes = "升級獎勵",
                Receivetime = DateTimeHelper.GetCurrentTime(),
                TxtID = Guid.NewGuid().ToString()
            });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TxtID);
            return;
        }



        [TestMethod]
        public void GetPaymentLink_Test()
        {
            string token = [REDACTED];
            string myLink = "https://example.invalid/";

            var setting = new GameLaunchExtraSettingModel
            {
                ExtraLink = myLink
            };

            CreateVendorSessionToken(token, _TEST_DATA.Default.PlayerID , setting);

            var result = W88ExtraProcessService.GetPaymentLink(_TEST_DATA.Default.PlayerID, new AGGFishGetPaymentLinkApiRequestaModel
            {
                Token = [REDACTED]
            });

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.link == myLink);
            return;
        }


        private void CreateVendorSessionToken(string token , int playerID, GameLaunchExtraSettingModel setting)
        {
            var insert = new Models.Table.VendorSessionTokenEntity
            {
                SessionToken = [REDACTED],
                CreateTime = DateTimeHelper.GetCurrentTime(),
                UpdateTime = DateTimeHelper.GetCurrentTime(),
                ExpiredTime = DateTimeHelper.GetCurrentTime().AddHours(24),
                PlayerID = playerID,
                GameID = 0,
                VendorID = GameVendorTypeEnum.AGGFish.ToInteger(),
                ExtraSettingJson = setting.ConvertToJsonString(),
            };

            VendorSessionTokenRepository.Create(insert);
        }

    }
}

