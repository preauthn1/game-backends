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
    public class ExternalApiGameVendorCommonServiceTests : BaseTest
    {

        public IExternalApiGameVendorCommonService ServiceInstance { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        [TestMethod]
        public void CheckIsValidIPAddress_Test()
        {
            Assert.IsTrue(ServiceInstance.CheckIsValidGameVendorIPAddress("0.0.0.0", GameVendorTypeEnum.Funky, false));
            Assert.IsFalse(ServiceInstance.CheckIsValidGameVendorIPAddress("0.0.0.0", GameVendorTypeEnum.Funky, false));

            var queryFunkySetting = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.GameVendorCallbackExtraWhiteListIP)
                .Where(x => x.SubCategory == GameVendorTypeEnum.Funky.ToIntegerString())
                .FirstOrDefault();

            var settingParse = new SystemConfigSettingParseModel(0, queryFunkySetting?.SettingValue);

            if(settingParse.SettingValueOfListString.IsNotEmptyNull())
            {
                string fisrIP = settingParse.SettingValueOfListString.FirstOrDefault();
                Assert.IsTrue(ServiceInstance.CheckIsValidGameVendorIPAddress(fisrIP, GameVendorTypeEnum.Funky, false));
            }

            return;
        }

    }
}

