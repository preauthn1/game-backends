using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Core.Helper;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SettingHelperTests : BaseTest
    {

        [TestMethod]
        public void IsEnableElasticLogByTableName_Test()
        {
            Assert.IsTrue(TableNameConvertHelper.IsEnableElasticLogByTableName("ApiRequestLogVendorAPIErrorBrief"));
            Assert.IsTrue(TableNameConvertHelper.IsEnableElasticLogByTableName("ApiRequestLogExternalVendorAPIError"));

            Assert.IsFalse(TableNameConvertHelper.IsEnableElasticLogByTableName("XXXXXXXXX"));
            Assert.IsFalse(TableNameConvertHelper.IsEnableElasticLogByTableName("ApiRequestLogPublicAPIVVVVV"));
            Assert.IsFalse(TableNameConvertHelper.IsEnableElasticLogByTableName("ApiRequestLogExternalXXXXXVendorAPIError"));
            return;           
        }


    }
}
