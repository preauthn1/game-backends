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
    public class MerchantHierarchyRepositoryTests : BaseTest
    {

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }



        [TestMethod]
        public void CheckDataForamt_Test()
        {
            var queryAll = MerchantHierarchyRepository.GetAll();

            Assert_NotEmptyCollection(queryAll);

            //檢查是否有資料 結尾不是 {自己的ID}/ 
            var incorrectFormatData = queryAll.Where(x => !x.Hierarchy.EndsWith($"{x.MerchantID}/")).ToList();
            if(incorrectFormatData.IsNotEmptyNull())
            {
                var matchData = incorrectFormatData.FirstOrDefault();
                Assert.Fail($"偵測到資料寫入不正確, 有資料的 Hierarchy 結尾不是／符號 , e.g. {matchData.MerchantID}");
            }

            return;
        }



        [TestMethod]
        public void GetBriefByMerchantIDs_Test()
        {
            int merchantID_1 = _TEST_DATA.CreditAgent.MerchantID;
            int merchantID_2 = _TEST_DATA.CreditAgent.Layer2_MerchantID;

            var query = MerchantHierarchyRepository.GetJoinInfoByMerchantIDs(new List<int>
            {
                _TEST_DATA.NotSuperMerchant.MerchantID,
                _TEST_DATA.Default.MerchantID,
                merchantID_1,
                merchantID_2,
            });

            Assert_NotEmptyCollection(query);

            var match1 = query.Where(m => m.MerchantID == merchantID_1).FirstOrDefault();
            var match2 = query.Where(m => m.MerchantID == merchantID_2).FirstOrDefault();

            Assert.IsTrue(match1 != null);
            Assert.IsTrue(match2 != null);

            Assert.IsTrue(match1.MerchantName.IsNotEmptyTextNotNull());
            Assert.IsTrue(match2.MerchantName.IsNotEmptyTextNotNull());
            Assert.IsTrue(match2.ParentMerchantID > 0);
            Assert.IsTrue(match2.MerchantName != match2.ParentMerchantName);
            return;
        }



        [TestMethod]
        public void GetByFilter_FilterHierarchyOfTargerMerchant_Test()
        {
            int targetMerchantID = _TEST_DATA.CreditAgent.MerchantID;
            var query = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = targetMerchantID
            });

            Assert_NotEmptyCollection(query);
            Assert.IsTrue(query.All(x => x.Hierarchy.Contains($"{targetMerchantID}/")));
            return;
        }

    }
}

