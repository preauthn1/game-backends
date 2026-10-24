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
    public class ExcelFileParseServiceTests : BaseTest
    {

        public IExcelFileParseService ServiceInstance { get; set; }


        [TestMethod]
        public void GenerateVipLevelDescriptionHtml_Test()
        {
            var result1 = ServiceInstance.GenerateVipLevelDescriptionHtml(EventContentBrandingType.target , MerchantWalletModeEnum.SingleWalletMode);
            var result2 = ServiceInstance.GenerateVipLevelDescriptionHtml(EventContentBrandingType.target, MerchantWalletModeEnum.SingleWalletMode);

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            return;
        }

    }
}

