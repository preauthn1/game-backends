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
    public class YesGameIntegrationServiceTests : BaseTest
    {
       
        public BetRecordYesGameRepository BetRecordYesGameRepository { get; set; }


        [TestMethod]
        public void YesGame_GenerateSignKey_Test()
        {
            var queryExistData = BetRecordYesGameRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
                {
                    OrderByColumn = " PartitionTime ",
                    PageSize = 5
                })
                .Items
                .FirstOrDefault();

            if (queryExistData == null)
                return;

            var service = new YesGameIntegrationService();

            var query1 = service.GetByBetRecordNo(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                BetRecordNoPGS = queryExistData.BetRecordNoPGS
            });

            var query2 = service.GetByBetRecordNo(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                BetRecordNo = queryExistData.BetRecordNo,
                IsQueryRecentRange = true,
            });

            var query3 = service.GetByBetRecordNo(new Models.ViewModels.GameVendor.BetRecordInfoQueryParameter
            {
                BetRecordNo = queryExistData.BetRecordNo,
            });

            Assert.IsTrue(query1 != null);
            Assert.IsTrue(query2 != null);
            Assert.IsTrue(query3 != null);
            Assert.IsTrue(query1.BetRecordNoPGS == queryExistData.BetRecordNoPGS);
            Assert.IsTrue(query2.BetRecordNoPGS == queryExistData.BetRecordNoPGS);
            Assert.IsTrue(query3.BetRecordNoPGS == queryExistData.BetRecordNoPGS);
            return;
        }


    }
}

