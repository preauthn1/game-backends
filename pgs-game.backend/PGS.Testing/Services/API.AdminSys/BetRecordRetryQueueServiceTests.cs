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
    public class BetRecordRetryQueueServiceTests : BaseTest
    {

        public IBetRecordService ServiceInstance { get; set; }



        [TestMethod]
        public void GetNotifyQueueList_Test()
        {
            var query = ServiceInstance.GetNotifyQueueList(_TEST_DATA.Default.UserID, new QueryBetRecordMerchantNotifyQueueRequestParameter
            {

            });

            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void ResetNotifyQueue_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var query = ServiceInstance.GetNotifyQueueList(userID, new QueryBetRecordMerchantNotifyQueueRequestParameter
            {
                CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 90),
                CreateTimeEnd = DateTimeHelper.GetCurrentTime().AddDays(-1 * 60),
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items.FirstOrDefault();

                ServiceInstance.ResetNotifyQueue(userID, matchFirst.Id);
            }

            return;
        }


    }
}

