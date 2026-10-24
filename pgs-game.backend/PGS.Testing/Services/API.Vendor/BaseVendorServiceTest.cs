using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;
using System.Collections.Generic;
using System;
using target.Models;
using target.BL.Services;

namespace target.Testing
{
    [TestClass]
    public class BaseVendorServiceTest : BaseTest
    {

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }


        [TestMethod]
        public void CheckRequestTooManyTimes_Test()
        {
            var baseVendor = new BaseVendorService();

            var rndKey = Guid.NewGuid().ToString();

            var result1 = baseVendor.CheckRequestTooManyTimes(_TEST_DATA.Default.MerchantID, rndKey, "GetRecords");
            var result2 = baseVendor.CheckRequestTooManyTimes(_TEST_DATA.Default.MerchantID, rndKey, "GetRecords");
            var result3 = baseVendor.CheckRequestTooManyTimes(_TEST_DATA.Default.MerchantID, rndKey, "GetRecords");

            Assert.IsTrue(result3.Counter > 1);
            return;
        }

        [TestMethod]
        public void IsValidateIPAddressOK_Test()
        {
            var baseVendor = new BaseVendorService();

            string myIPv6 = "2506:da19:8e9:f31f:43c5:ef98:660b:e83a";

            List<string> whiteList = new List<string>
            {
                "0.0.0.0",
                "0.0.0.0",
                "68.68.*",
                myIPv6,
            };

            Assert.IsTrue(baseVendor.IsValidateIPAddressOK(whiteList , "0.0.0.0").IsValidateOK);
            Assert.IsTrue(baseVendor.IsValidateIPAddressOK(whiteList, "0.0.0.0").IsValidateOK);
            Assert.IsTrue(baseVendor.IsValidateIPAddressOK(whiteList, myIPv6).IsValidateOK);
            Assert.IsFalse(baseVendor.IsValidateIPAddressOK(whiteList, "9906:da19:8e9:f31f:43c5:ef98:660b:e83a").IsValidateOK);
            Assert.IsFalse(baseVendor.IsValidateIPAddressOK(whiteList, "0.0.0.0").IsValidateOK);
            Assert.IsFalse(baseVendor.IsValidateIPAddressOK(whiteList, "AAAAAA").IsValidateOK);

            //like query
            Assert.IsTrue(baseVendor.IsValidateIPAddressOK(whiteList, "0.0.0.0").IsValidateOK);
            Assert.IsFalse(baseVendor.IsValidateIPAddressOK(whiteList, "0.0.0.0").IsValidateOK);
            Assert.IsFalse(baseVendor.IsValidateIPAddressOK(whiteList, "0.0.0.0").IsValidateOK);


            Assert.IsTrue(baseVendor.IsValidateIPAddressOK(new List<string> { "*" }, "0.0.0.0").IsValidateOK);
            Assert.IsTrue(baseVendor.IsValidateIPAddressOK(new List<string> { "*" }, "0.0.0.0").IsValidateOK);

            return;
        }


        [TestMethod]
        public void IsValidateIPAddressByCheckOneAndTwo_DbSettingIPs_Test()
        {
            var baseVendor = new BaseVendorService();

            int merchantID_1 = _TEST_DATA.Default.MerchantID;
            int merchantID_2 = _TEST_DATA.Default.MerchantID - 5000;

            List<string> whiteList = new List<string>
            {
                "0.0.0.0",
                "0.0.0.0",
            };

            string myIP = "0.0.0.0";

            MerchantCustomizedSettingRepository.CreateOrUpdate(merchantID_1, (int)MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_CustomizedIPs, myIP);

            Assert.IsFalse(baseVendor.IsValidateIPAddressByCheckOneAndTwo(whiteList, myIP , merchantID_2).IsValidateOK);
            Assert.IsTrue(baseVendor.IsValidateIPAddressByCheckOneAndTwo(whiteList, myIP, merchantID_1).IsValidateOK);
            Assert.IsFalse(baseVendor.IsValidateIPAddressByCheckOneAndTwo(whiteList, "0.0.0.0", merchantID_1).IsValidateOK);
            return;
        }


    }
}

