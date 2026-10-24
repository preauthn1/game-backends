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
    public class MerchantCreditChangeLogServiceTests : BaseTest
    {

        public IMerchantCreditChangeLogService ServiceInstance { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }


        [TestMethod]
        public void MerchantCreditChangeLogService_GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryMerchantCreditChangeLogApiRequestParameterModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }



        [TestMethod]
        public void MerchantCreditChangeLogService_CreditAgent_GetByPagingFilter_Test()
        {
            int userID = _TEST_DATA.CreditAgent.UserID;
            int merchantID = _TEST_DATA.CreditAgent.MerchantID;

            var queryMyTreeMerchants = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = merchantID
            });

            List<int> myMerchantIDs = queryMyTreeMerchants.Select(z => z.MerchantID).ToList();

            var query = ServiceInstance.GetByPagingFilter(userID,  new QueryMerchantCreditChangeLogApiRequestParameterModel
            {
                PageSize = 30,
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime(),
            });

            Assert.IsTrue(query.Items.All(z => myMerchantIDs.Contains(z.MerchantID)));
            return;
        }

    }
}

