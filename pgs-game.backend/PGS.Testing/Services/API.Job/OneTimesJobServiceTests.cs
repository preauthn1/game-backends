using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace target.Testing
{
    [TestClass]
    public class OneTimesJobServiceTests : BaseTest
    {

        public OneTimesJobSettingRepository OneTimeJobSettingRepository { get; set; }


        public class MockIOneTimesJobService : OneTimesJobService
        {
            protected override void ResetPlayerRTP(OneTimesJobSettingEntity queryEntity, string debugUUID)
            {
                //不做任何事情 不模擬這段
            }
        }

        [TestMethod]
        public async Task CheckOneTimesJobToExecute_LocalTest_Test()
        {
            //非公司開發電腦 不執行這組test
            if (IsNotDeveloperComputerName())
                return;

            var oneTimesJobService = new OneTimesJobService();
            //await oneTimesJobService.CheckOneTimesJobToExecute();
        }



        [TestMethod]
        public async Task CheckOneTimesJobToExecute_Test()
        {
            var mockService = new MockIOneTimesJobService();

            int targetEventID = 1234;

            string randomMemo = "MSTest_" + Guid.NewGuid().ToString("N");

            OneTimeJobSettingRepository.Create(new OneTimesJobSettingEntity
            {
                SourceID = targetEventID,
                Type = OneTimesJobTypeEnum.Event_RTPEvent_ResetAllPlayer.ToInteger(),
                Status = OneTimesJobStatusEnum.Pending.ToInteger(),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                CreateBy = "MSTest",
                IsAlwaysNotify = false,
                LastDebugUUID = "",
                SettingJson = "",
                Memo = randomMemo,
                NextTime = DateTimeHelper.GetCurrentTime().AddDays(-1),
                RetryTimes = 0,
            });

            //執行排程
            await mockService.CheckOneTimesJobToExecute(targetEventID.ToListCollection());

            var queryExistData = OneTimeJobSettingRepository.GetByFilter(new QueryOneTimeJobSettingRequestViewModel
            {
                MemoMatch = randomMemo
            })
            .FirstOrDefault();

            Assert.IsNotNull(queryExistData);
            Assert.IsTrue(queryExistData.Status == OneTimesJobStatusEnum.Success.ToInteger());
            return;
        }

    }
}

