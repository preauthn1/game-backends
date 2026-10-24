using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Core.Repository;
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
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class ApiRequestLogBriefRepositoryTests : BaseTest
    {

        public ApiRequestLogV2024Repository ApiRequestLogV2024Repository { get; set; }



        [TestMethod]
        public void BulkInsert_Test()
        {
            string defaultText = "MSTest_" + DateTimeHelper.GetCurrentTimeYYYYMMDD();

            var now = DateTimeHelper.GetCurrentTime();

            var logModel = new ApiRequestLogQueueTempModel
            {
                ExtraInfoV2 = new ApiRequestLogQueueExtraInfoV2026Model
                {
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    UniqueID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo(),
                    TableType = ElasticLogTableTypeEnum.ApiRequestLogExternalVendorAPIError,
                },
                ApiRequestLogEntity = new ApiRequestLogEntity
                {
                    WebSiteName = "Web.API",
                    CreateTime = now,
                    UserIdentity = "",
                    IdentityKey = "",
                    ResponseText = defaultText,
                    RequestUrl = "https://example.invalid/v1/games",
                    RelativeUrl = "/v1/games",
                    RequestBody = defaultText,
                    IPLocation = "",
                    Headers = defaultText,
                    HttpMethod = "GET",
                    IPAddress = "0.0.0.0",
                    ExecutionTime = 0,
                    ResponseStatusCode = 200,
                    CreateTimeCategory = 0,
                }
            };

            ApiRequestLogV2024Repository.BulkInsertToMultipleStorage(ElasticLogTableTypeEnum.ApiRequestLogExternalVendorAPIError, new List<ApiRequestLogQueueTempModel>
            {
                logModel
            });
        }


    }
}

