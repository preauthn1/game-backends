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
    public class AGGBetRecordJobServiceTests : BaseTest
    {
        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        private static List<BetRecordAGGEntity> _fakeMockData = new List<BetRecordAGGEntity>();

        public class MockAGGBetRecordJobService : AGGBetRecordJobService
        {
            protected override List<BetRecordAGGEntity> GetPendingBetRecords(double previousDays, string selectColumns)
            {                
                return _fakeMockData;
            }

            protected override string TriggerAGGApi(string betRecordNoPGS)
            {
                return "OK";
            }
        }


        [TestMethod]
        public void ResolveAGGExceptionalWagers_Test()
        {
            var service = new MockAGGBetRecordJobService();

            //使用mock
            var substitute = Substitute.For<IAGGInternalAPIService>();

            var query = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 3,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime().AddDays(-2)
            });

            if (query.Items.IsEmptyOrNull())
                return;

            var convertToFakeDate = query.Items
                .Select(z => new AGGGetExceptionalWagersItemModel
                {
                    wagerId = z.BetRecordNo,
                    userId = z.PlayerID.ToString()
                })
                .ToList();

            var fakeResponse = new ApiResultBriefModel<AGGGetExceptionalWagersResponseModel>
            {
                ResultObject = new AGGGetExceptionalWagersResponseModel
                {
                    wagers = convertToFakeDate
                }
            };

            //這些依賴三方API的 都mock假資料
            substitute.GetExceptionalRecords(null).ReturnsForAnyArgs(fakeResponse);
            substitute.TriggerExceptionalWagerRetry(null).ReturnsForAnyArgs("OK");

            service.AGGInternalAPIService = substitute;

            service.CleanUpAGGExceptionalWagers();
            return;
        }


        [TestMethod]
        public void CheckPendingRecordAndTriggerSettle_Test()
        {
            var mockService = new MockAGGBetRecordJobService();

            //使用mock
            var substitute = Substitute.For<IAGGInternalAPIService>();

            var query = BetRecordAGGRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 3,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime()
            });

            if (query.Items.IsEmptyOrNull())
                return;

            _fakeMockData = query.Items;

            mockService.AGGInternalAPIService = substitute;

            var param = new QueryPendingRecordJobParameter
            {
                PreviousDays = 3
            };

            mockService.CheckPendingRecordAndTriggerSettle(param);
            return;
        }

    }
}

