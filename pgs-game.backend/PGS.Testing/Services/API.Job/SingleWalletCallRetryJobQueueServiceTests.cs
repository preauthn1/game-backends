using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SingleWalletCallRetryJobQueueServiceTests : BaseTest
    {
        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

      
        [TestMethod]
        public void CleanUpAGGExceptionalWagers_Test()
        {
            if (IsNotDeveloperComputerName())
                return;

            var singleWalletCallRetryJobQueueService = new SingleWalletCallRetryJobQueueService();

            //var callbackObj = singleWalletCallRetryJobQueueService.InstanceSettleCallbackParameters(new BetRecordMerchantNotifyQueueEntity
            //{
            //    BetRecordNoPGS = "43M9701aabe85a3a4f15816ef825f800",
            //    LastRequestUUID = Guid.NewGuid().ToString()
            //});

            return;
        }


        [TestMethod]
        public void InstanceSettleCallbackParameters_Test()
        {
            var queryPerGame = PerGameDetailLogRepository.GetPagingByFilter(new QueryDetailLogRequestViewModel
            {
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime(),
                PageSize = 50,
            })
            .Items;

            var queryAT = queryPerGame.Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == GameVendorTypeEnum.AT.ToInteger()).FirstOrDefault();
            var queryAGG = queryPerGame.Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == GameVendorTypeEnum.AGG.ToInteger()).FirstOrDefault();
            var queryJili = queryPerGame.Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == GameVendorTypeEnum.Jili.ToInteger()).FirstOrDefault();

            ValidateMethodNotError(queryAT);
            ValidateMethodNotError(queryAGG);
            ValidateMethodNotError(queryJili);
        }


        private void ValidateMethodNotError(PerGameDetailLogEntity perGame)
        {
            if (perGame == null)
                return;

            var singleWalletCallRetryJobQueueService = new SingleWalletCallRetryJobQueueService();

            var callbackObj = singleWalletCallRetryJobQueueService.InstanceSettleCallbackParameters(new BetRecordMerchantNotifyQueueEntity
            {
                BetRecordNoPGS = perGame.TransactionNo,
                LastRequestUUID = Guid.NewGuid().ToString()
            });

            Assert.IsTrue(callbackObj.BetAmount == perGame.BetAmount);
            Assert.IsTrue(callbackObj.PayoutAmount == perGame.PayoutAmount);
            Assert.IsTrue(callbackObj.BetRecordNoPGS == perGame.TransactionNo);
            Assert.IsTrue(callbackObj.SettlementTime == perGame.GameTime);
            return;
        }
    }
}

