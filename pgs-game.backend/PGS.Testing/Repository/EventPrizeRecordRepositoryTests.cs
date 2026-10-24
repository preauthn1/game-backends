using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class EventPrizeRecordRepositoryTests : BaseTest
    {
        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }


        [TestMethod]
        public void GetJoinInfoByID_Test()
        {
            decimal plusValue = RandomHelper.GetRandIntegerNumbers(100 , 199);

            var query = EventPrizeRecordRepository.GetPagingByFilter(new QueryEventPrizeRecordRequestViewModel
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                PageSize = 10
            });

            if(query.Items.IsEmptyOrNull())
            {
                return;
            }

            var matchFirst = query.Items.FirstOrDefault();

            var queryDetail = EventPrizeRecordRepository.GetJoinDetailInfoByID(matchFirst.Id);

            Assert.IsTrue(queryDetail != null);
            Assert.IsTrue(queryDetail.UpdateAmount == matchFirst.UpdateAmount);
            return;
        }

    }
}

