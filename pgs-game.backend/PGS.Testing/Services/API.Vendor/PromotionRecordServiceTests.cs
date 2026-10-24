using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class PromotionRecordServiceTests : BaseTest
    {

        public IPromotionRecordService ServiceInstance { get; set; }


        [TestMethod]
        public void VendorAPI_GetPrizeRecords_Test()
        {
            int merchantID = _TEST_DATA.Default.MerchantID;
            var query = ServiceInstance.GetPrizeRecords(new PromotionRecordRequestModel
            {
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = DateTimeHelper.GetCurrentTime().AddDays(-90),
                EndTime = DateTimeHelper.GetCurrentTime(),
                PageSize = 20,
            });

            if (query.Items.IsEmptyOrNull())
                return;

            //我方會設定特定的Types不被vendor查到
            Assert.IsTrue(query.Items.All(z => EventDefinitionHelper.PrizeTypes_VendorAPI_VisiblePrizeTypes().Contains(z.EventPrizeType)));
            Assert.IsTrue(query.Items.All(z => z.MerchantID == merchantID));


            var prizeType = query.Items.Select(x => x.EventPrizeType).FirstOrDefault();

            var query2 = ServiceInstance.GetPrizeRecords(new PromotionRecordRequestModel
            {
                EventPrizeType = prizeType,
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = DateTimeHelper.GetCurrentTime().AddDays(-90),
                EndTime = DateTimeHelper.GetCurrentTime(),
                PageSize = 20,
            });

            Assert.IsTrue(query2.Items.All(z => z.EventPrizeType == prizeType));
            return;
        }


        [TestMethod]
        public void VendorAPI_GetPrizeRecords_IsFilterByCreateTime_Test()
        {
            int merchantID = _TEST_DATA.Default.MerchantID;

            var query = ServiceInstance.GetPrizeRecords(new PromotionRecordRequestModel
            {
                MerchantID = merchantID,
                MerchantSecretKey = _TEST_DATA.Default.MerchantSecret,
                StartTime = DateTimeHelper.GetCurrentTime().AddDays(-90),
                EndTime = DateTimeHelper.GetCurrentTime(),
                IsFilterByCreateTime = true
            });

            if (query.Items.IsEmptyOrNull())
                return;

            //我方會設定特定的Types不被vendor查到
            Assert.IsTrue(query.Items.All(z => EventDefinitionHelper.PrizeTypes_VendorAPI_VisiblePrizeTypes().Contains(z.EventPrizeType)));
            Assert.IsTrue(query.Items.All(z => z.MerchantID == merchantID));

            return;
        }


    }
}

