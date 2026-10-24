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
    public class SettingKeyEnumLogicHelperTests : BaseTest
    {


        [TestMethod]
        public void GetMerchantSettingOrDefaultValue_Test()
        {
            var defaultValue1 = SettingKeyEnumLogicHelper.GetMerchantSettingOrDefaultValue(MerchantCustomizedSettingKeyEnum.DefaultWalletBalance, null);
            var defaultValue2 = SettingKeyEnumLogicHelper.GetMerchantSettingOrDefaultValue(MerchantCustomizedSettingKeyEnum.Style_LoadingStyleMode, null);

            Assert.IsNotNull(defaultValue1);
            Assert.IsNotNull(defaultValue2);

            return;
        }



        [TestMethod]
        public void SettingKeyEnumLogicHelper_Test()
        {
            var defaultValue1 = SettingKeyEnumLogicHelper.GetMerchantSettingOrDefaultValue(MerchantCustomizedSettingKeyEnum.DefaultWalletBalance, null);
            var defaultValue2 = SettingKeyEnumLogicHelper.GetMerchantSettingOrDefaultValue(MerchantCustomizedSettingKeyEnum.Style_LoadingStyleMode, null);

            Assert.IsNotNull(defaultValue1);
            Assert.IsNotNull(defaultValue2);

            return;
        }


        [TestMethod]
        public void IsWarningCallbackUrl_Test()
        {
            string myUrl1 = "https://qqqqqq.live/yyyyy/target";
            string myUrl2 = "https://qqqqqq.live/yyyyy/target/";
            string myUrl3 = "https://qqqqqq.live/pgsgaming";
            string myUrl4 = "https://qqqqqq.live/pgsapi";
            string myUrl5 = "https://target.live/sw-api";

            Assert.IsTrue(SettingKeyEnumLogicHelper.IsWarningCallbackUrl(myUrl1));
            Assert.IsTrue(SettingKeyEnumLogicHelper.IsWarningCallbackUrl(myUrl2));

            Assert.IsFalse(SettingKeyEnumLogicHelper.IsWarningCallbackUrl(myUrl3));
            Assert.IsFalse(SettingKeyEnumLogicHelper.IsWarningCallbackUrl(myUrl4));
            Assert.IsFalse(SettingKeyEnumLogicHelper.IsWarningCallbackUrl(myUrl5));
            return;
        }

    }
}

