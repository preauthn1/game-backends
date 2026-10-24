using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Helper.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class FunkyIntegrationServiceTests : BaseTest
    {

        public BetRecordFunkyRepository BetRecordFunkyRepository { get; set; }



        [TestMethod]
        public void GetByBetRecordNo_Test()
        {
            FunkyIntegrationService FunkyIntegrationService = new FunkyIntegrationService();

            var queryWagers = BetRecordFunkyRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageIndex = 1,
                PageSize = 10,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-7),
            });

            if (queryWagers.Items.IsEmptyOrNull())
                return;

            var matchFirst = queryWagers.Items.FirstOrDefault();

            var queryData = FunkyIntegrationService.GetByBetRecordNo(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                BetRecordNo = matchFirst.BetRecordNo,
            });

            Assert.IsTrue(queryData.BetRecordNoPGS == matchFirst.BetRecordNoPGS);
            Assert.IsTrue(queryData.BetRecordNo == matchFirst.BetRecordNo);
            Assert.IsTrue(queryData.BetAmount == matchFirst.BetAmount);
            return;
        }



    }
}

