using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;
using System.Collections.Generic;
using System;

namespace target.Testing
{
    [TestClass]
    public class MerchantGameSummaryDataRepositoryTests : BaseTest
    {

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }



        [TestMethod]
        public void GetAgentDailyGameReport_Test()
        {
            int merchantID = _TEST_DATA.CreditAgent.MerchantID;

            var myFilter = new QueryAgentDailyGameSummaryReportRequestParameterModel
            {
                MerchantID = merchantID,
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 7),
                End = DateTimeHelper.GetCurrentTime(),
                TimeZone = 8,
                PageIndex = 1,
                PageSize = 10
            };

            var qqqq = MerchantGameSummaryDataRepository.GetAgentDailyGameReportPagedList(myFilter);
            return;
        }


    }
}

