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
    public class BetRecordTagMappingRepositoryTests : BaseTest
    {

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }


        [TestMethod]
        public void GetNotExistRecordNo_Test()
        {
            var queryExist = BetRecordTagMappingRepository.GetPagingByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                PageSize = 1
            });

            if (queryExist.Items.IsEmptyOrNull())
                return;

            var matchFirst = queryExist.Items.FirstOrDefault();

            List<string> MSTestRecords = new List<string>
            {
                "XXXXXXXXXXXX",
                "YYYYYYYYYYYY",
                matchFirst.BetRecordNoPGS
            };

            var queryNotExist = BetRecordTagMappingRepository.GetNotExistRecordNo(MSTestRecords, (SystemTagEnum)matchFirst.TagID);

            Assert.IsTrue(queryNotExist.Count() == 2);
            Assert.IsFalse(queryNotExist.Contains(matchFirst.BetRecordNoPGS));
            return;
        }


        [TestMethod]
        public void GetByFilter_Test()
        {
            var queryExist = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                BetRecordNoPGSList = "XXXQQQQYYY666SS".ToListCollection()
            });

            //確保沒有改壞 如果查詢出問題 會導致排程出錯誤
            Assert.IsTrue(queryExist.IsEmptyOrNull());
            return;
        }


    }
}

