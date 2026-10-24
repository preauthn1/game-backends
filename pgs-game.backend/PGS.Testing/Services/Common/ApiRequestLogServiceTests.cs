using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
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
using System.Threading;

namespace target.Testing
{
    [TestClass]
    [DoNotParallelize]
    public class ApiRequestLogServiceTests : BaseTest
    {

        public IApiRequestLogService ServiceInstance { get; set; }


        [TestMethod]
        public void CreateTryCatch_Test()
        {
            string defaulText = "MSTest";
            string tableName = "[dbo].[ApiRequestLogAdminSysAPI]";

            var now = DateTimeHelper.GetCurrentTime();

            ServiceInstance.CreateTryCatch(tableName, new ApiRequestLogEntity
            {
                RequestUrl = defaulText,
                RelativeUrl = defaulText,
                ResponseText = defaulText,
                Doamin = defaulText,
                RequestBody = defaulText,
                WebSiteName = SettingHelper.WebSiteName,
                IPAddress = _TEST_DATA.Default.IPAddress,
                IPLocation = string.Empty,
                Headers = string.Empty,
                CreateTime = now,
                HttpMethod = "POST",
                ResponseStatusCode = 200
            });

            return;
        }


        [TestMethod]
        public void CreateErrorLogTryCatch_V2_Tests()
        {
            //清光queue
            EazyQueueApiErrorLogHelper.GetMultipleItems(2500);

            var logModel = new ApiRequestLogEntity
            {
                RelativeUrl = "/api/test/error",
                DebugInfo3 = "refer-order-no",
                ExtraDebug_ErrorCode = 10001
            };

            try
            {
                ServiceInstance.CreateErrorLogTryCatch(
                    SettingHelper.ApiRequestLogTableName_Error,
                    logModel);

                var queueItems = EazyQueueApiErrorLogHelper.GetMultipleItems(2500);

                Assert.AreEqual(1, queueItems.Count);
                Assert.AreSame(logModel, queueItems[0].ApiRequestLogEntity);
                Assert.IsNotNull(queueItems[0].ExtraInfoV2);
                Assert.AreEqual(
                    ElasticLogTableTypeEnum.ApiRequestLogPublicAPIError,
                    queueItems[0].ExtraInfoV2.TableType);
                Assert.AreEqual(10001, queueItems[0].ExtraInfoV2.ErrorCode);
                Assert.AreEqual("refer-order-no", queueItems[0].ExtraInfoV2.ReferOrderNo);
                Assert.IsFalse(string.IsNullOrWhiteSpace(queueItems[0].ExtraInfoV2.UniqueID));
            }
            finally
            {
                EazyQueueApiErrorLogHelper.GetMultipleItems(2500);
            }
        }


        [TestMethod]
        public void ReadBufferingFileAndSyncToRemote_Test()
        {
            ServiceInstance.ReadQueueAndSyncApiRequestLogToRemote();

            Thread.Sleep(1000 * 6);

            ServiceInstance.ReadQueueAndSyncApiRequestLogToRemote();
            return;
        }


        [TestMethod]
        public void WriteSWLogToDB_Test()
        {
            var now = DateTimeHelper.GetCurrentTime();

            var myItems = new List<SingleWalletMerchantApiRequestLogEntity>();
            myItems.Add(new SingleWalletMerchantApiRequestLogEntity
            {
                UniqueID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo(),
                CreateTime = now,
                Category = "175001",
                MerchantID = 175001,
                RequestContent = "",
                RequestHeader = "",
                RequestID = Guid.NewGuid().ToString(),
                ReferenceTransactionNo = Guid.NewGuid().ToString(),
                WebSiteName = SettingHelper.WebSiteName,
                RequestUrl = "http://0.0.0.0:3109/target/bet/cancel",
                SubCategory = "target/bet/cancel",
            });

            ServiceInstance.WriteSWLogToStorage(myItems);
            return;
        }


    }
}

