using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class VendorAPIComplexServiceTests : BaseTest
    {

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public IVendorAPIComplexService ServiceInstance { get; set; }



        private int TagID = 27999;

        private string Currency = "BETA";

        private string API_KEY = [REDACTED]";



        [TestMethod]
        public void GetVendorAPIBackAPIUrls_Test()
        {
            var query = ServiceInstance.GetVendorAPIBackAPIUrls(new BackUrlsQueryParameterModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret
            });

            Assert.IsNotNull(query);
        }


        [TestMethod]
        public void GetAgentHierarchyReport_Test()
        {
            var filter = new AgentHierarchyReportSummaryRequestModel
            {
                ReportCategoryID = TagID,
                ReportApiKey = [REDACTED],
                Currency = Currency
            };

            CheckTargetMerchantData(_TEST_DATA.CreditAgent.MerchantID , filter);
        }


        [TestMethod]
        public void GetAgentHierarchyReport_NoPermission_Test()
        {
            var filter = new AgentHierarchyReportSummaryRequestModel
            {
                ReportCategoryID = TagID,
                ReportApiKey = [REDACTED],
                Currency = "CNT"
            };

            AssertExpectExceptionProcess(() =>
            {
                CheckTargetMerchantData(_TEST_DATA.Default.MerchantID, filter);

            }, ApiErrorCode.NotFoundMerchant);
        }



        private void CheckTargetMerchantData(int targetMerchantID , AgentHierarchyReportSummaryRequestModel filter)
        {
            var queryExistData = MerchantGameSummaryDataRepository.GetByFilter(new QueryMerchantGameSummaryDataRequestViewModel
                {
                    MerchantIDs = targetMerchantID.ToListCollection(),
                    StatTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 365),
                    StatTimeEnd = DateTimeHelper.GetCurrentTime(),
                })
                .Where(x => x.BetAmount > 0)
                .ToList();

            var matchFirst = queryExistData.FirstOrDefault();

            if (matchFirst != null)
            {
                filter.StartTime = matchFirst.StatTime.AddDays(-1 * 7);
                filter.EndTime = matchFirst.StatTime.AddDays(1);

                var query1 = ServiceInstance.GetRichmondVersionAgentHierarchyReport(filter);
                Assert.IsNotNull(query1);
            }
        }

    }
}

