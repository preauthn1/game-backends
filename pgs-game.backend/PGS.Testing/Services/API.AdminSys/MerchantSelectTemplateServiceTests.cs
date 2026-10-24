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
    public class MerchantSelectTemplateServiceTests : BaseTest
    {

        public IMerchantSelectTemplateService ServiceInstance { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }


        [TestMethod]
        public void CreateTemplate_Test()
        {
            int userID = _TEST_DATA.Default.UserID;
            List<int> includeMerchantIDs = new List<int>
            {
                _TEST_DATA.Default.MerchantID
            };

            var input = new MerchantSelectTemplateRequestModel
            {
                DisplayName = "MSTest_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS(),
                CategoryID = DataCategoryEnum.MerchantSelect_GameVendorMaintain.ToInteger(),
                ValueType = MerchantSelectValueTypeEnum.SelectIncludeMerchants.ToInteger(),
                IncludeMerchantIDs = includeMerchantIDs
            };

            var result = ServiceInstance.CreateTemplate(userID, input);

            Assert.IsTrue(result.ResultId > 0);


            var detailInfo = ServiceInstance.GetByTemplateID(userID, result.ResultId);

            Assert.IsTrue(detailInfo.IncludeMerchantIDs.Count() == includeMerchantIDs.Count());
            Assert.IsTrue(detailInfo.IncludeMerchantIDs.OrderBy(x=> x).FirstOrDefault() == includeMerchantIDs.OrderBy(x => x).FirstOrDefault());

            int newTemplateID = result.ResultId;

            input.TemplateID = newTemplateID;

            ServiceInstance.UpdateTemplate(userID, input);

            //清除資料
            ServiceInstance.Delete(userID, newTemplateID);
            return;
        }



        [TestMethod]
        public void CreateTemplate_AgentMode_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            int topAgentMerchantID = _TEST_DATA.CreditAgent.MerchantID;

            var queryHierarchyAgents = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = topAgentMerchantID
            });

            var result = ServiceInstance.CreateTemplate(userID, new MerchantSelectTemplateRequestModel
            {
                DisplayName = "MSTest_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS(),
                CategoryID = DataCategoryEnum.MerchantSelect_GameVendorMaintain.ToInteger(),
                ValueType = MerchantSelectValueTypeEnum.SelectAgentAndExcludeMerchants.ToInteger(),
                ParentMerchantID = topAgentMerchantID,
            });

            Assert.IsTrue(result.ResultId > 0);

            var detailInfo = ServiceInstance.GetByTemplateID(userID, result.ResultId);

            Assert.IsTrue(detailInfo.IncludeMerchantIDs.Count() == queryHierarchyAgents.Count());

            //清除資料
            ServiceInstance.Delete(userID, result.ResultId);
            return;
        }






    }
}

