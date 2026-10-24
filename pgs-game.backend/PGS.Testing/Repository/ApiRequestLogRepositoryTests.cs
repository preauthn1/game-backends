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
    public class ApiRequestLogRepositoryTests : BaseTest
    {

        public ApiRequestLogRepository ApiRequestLogRepository { get; set; }


        public ApiRequestLogV2024Repository ApiRequestLogV2024Repository { get; set; }


        [TestMethod]
        public void BulkInsert_Test()
        {
            string defaultText = "MSTest";

            var scopeLevel = System.Transactions.IsolationLevel.Serializable;

            ApiRequestLogRepository.BulkInsert("[dbo].[ApiRequestLogVendorAPI]", new List<Models.Table.ApiRequestLogEntity>
            {
                InstanceEmptyEntity(),
                new Models.Table.ApiRequestLogEntity
                {
                    RequestUrl = defaultText,
                    RelativeUrl = defaultText,
                    Doamin = defaultText,
                    HttpMethod = "POST",
                    RequestBody = "",
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    ResponseStatusCode = 400,
                }
            } , scopeLevel);

            return;
        }


        [TestMethod]
        public void BulkInsertToMultipleStorage_ApiRequestLogVendorAPIError_Test()
        {
            ApiRequestLogV2024Repository.BulkInsertToMultipleStorage(ElasticLogTableTypeEnum.ApiRequestLogVendorAPIError, new List<Models.Table.ApiRequestLogQueueTempModel>
            {
                new ApiRequestLogQueueTempModel
                {
                    ApiRequestLogEntity = InstanceEmptyEntity(),
                    ExtraInfoV2 = new ApiRequestLogQueueExtraInfoV2026Model
                    {
                        MerchantID = _TEST_DATA.Default.MerchantID,
                        UniqueID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo(),
                        ErrorCode = ApiErrorCode.InvalidParameter.ToInteger(),
                    }
                }
            });
            
            return;
        }


        private ApiRequestLogEntity InstanceEmptyEntity()
        {
            string defaultText = "MSTest";

            return new ApiRequestLogEntity
            {
                RequestUrl = defaultText,
                RelativeUrl = defaultText,
                Doamin = defaultText,
                HttpMethod = "POST",
                RequestBody = "",
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ResponseStatusCode = 400,
                StackTrace = defaultText,
                WebSiteName = defaultText,
                DebugInfo = defaultText,
                DebugInfo2 = defaultText,
                DebugInfo3 = defaultText,
                Headers = defaultText,
                IdentityKey = defaultText,
                IPAddress = defaultText,
                IPLocation = defaultText,
                Memo = defaultText,
                UserIdentity = defaultText,
                ResponseText = defaultText,
            };
        }




    }
}

