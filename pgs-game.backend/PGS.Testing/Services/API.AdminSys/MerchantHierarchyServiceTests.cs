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
    public class MerchantHierarchyServiceTests : BaseTest
    {

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public IMerchantHierarchyService ServiceInstance { get; set; }


        [TestMethod]
        public void GetHierarchyTreeView_Test()
        {
            var query = ServiceInstance.GetHierarchyTreeView(_TEST_DATA.Default.UserID , new MerchantHierarchyTreeviewRequestModel
            {
                RootMerchantID = _TEST_DATA.CreditAgent.MerchantID
            });

            Assert.IsNotNull(query.Root);
            Assert.IsTrue(query.TotalAgentCount > 0);
            Assert_NotEmptyCollection(query.Root.Childs);
            return;
        }


        [TestMethod]
        public void CreateMerchantHierarchy_SourceAndTargetSame_Test()
        {
            AssertExpectExceptionProcess(() =>
            {
                ServiceInstance.CreateMerchantHierarchy(_TEST_DATA.Default.UserID, new MerchantHierarchyCreateRequestModel
                {
                    ParentMerchantID = _TEST_DATA.CreditAgent.MerchantID,
                    TargetMerchantID = _TEST_DATA.CreditAgent.MerchantID
                });

            }, ApiErrorCode.InvalidParameter_SourceValueMustNotEqualTargetValue);

            return;
        }



        [TestMethod]
        public void CreateHierarchy_And_Delete_Test()
        {
            ServiceInstance.DeleteMerchantHierarchy(_TEST_DATA.Default.UserID, new MerchantHierarchyDeleteRequestModel
            {
                TargetMerchantID = _TEST_DATA.CreditAgent.Layer3_MerchantID
            });

            ServiceInstance.CreateMerchantHierarchy(_TEST_DATA.Default.UserID, new MerchantHierarchyCreateRequestModel
            {
                ParentMerchantID = _TEST_DATA.CreditAgent.Layer2_MerchantID,
                TargetMerchantID = _TEST_DATA.CreditAgent.Layer3_MerchantID
            });

            return;
        }

        [TestMethod]
        public void MerchantHierarchyMigrate_Test()
        {
            int rootID = _TEST_DATA.CreditAgent.MerchantID;

            var queryTreeAll = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                RootMerchantID = rootID
            });

            var queryLayer3 = queryTreeAll
                .Where(x=> x.MerchantID != _TEST_DATA.CreditAgent.Layer3_MerchantID) //這組ID不要使用
                .Where(x => x.Hierarchy.Split('/')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Count() >= 3)
                .FirstOrDefault();

            int originalParent = queryLayer3.ParentMerchantID;

            //嘗試轉成二級代理
            ServiceInstance.MerchantHierarchyMigrate(_TEST_DATA.Default.UserID,  new MerchantHierarchyMigrateRequestModel
            {
                TargetMerchantID = queryLayer3.MerchantID,
                NewParentMerchantID = queryLayer3.RootMerchantID
            });

            //還原回去原本的
            ServiceInstance.MerchantHierarchyMigrate(_TEST_DATA.Default.UserID, new MerchantHierarchyMigrateRequestModel
            {
                TargetMerchantID = queryLayer3.MerchantID,
                NewParentMerchantID = originalParent
            });

            return;
        }



    }
}

