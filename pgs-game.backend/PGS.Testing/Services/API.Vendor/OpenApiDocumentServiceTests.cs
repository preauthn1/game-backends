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
    public class OpenApiDocumentServiceTests : BaseTest
    {

        public IOpenApiDocumentService ServiceInstance { get; set; }


        [TestMethod]
        public void GenerateResponseHtmlForOpenApiDocument_Test()
        {
            var merchantGUID = _TEST_DATA.Default.MerchantGuid;
            var query = ServiceInstance.GenerateResponseHtmlForOpenApiDocument(merchantGUID);
            Assert.IsNotNull(query);
            return;
        }

        [TestMethod]
        public void GetMerchantInfoByGuid_Test()
        {
            var merchantGUID = _TEST_DATA.Default.MerchantGuid;
            var query = ServiceInstance.GetMerchantInfoByGuid(merchantGUID);
            Assert.IsNotNull(query.QueryMerchant);
            Assert_NotEmptyCollection(query.Games);
            return;
        }


        [TestMethod]
        public void HasIONSlotKeyword_Test()
        {
            Assert.IsTrue(ServiceInstance.HasIONSlotKeyword("ION X target"));
            Assert.IsTrue(ServiceInstance.HasIONSlotKeyword("ion slot (IDR) 對接"));
            Assert.IsTrue(ServiceInstance.HasIONSlotKeyword("API ION x TRG"));
            Assert.IsTrue(ServiceInstance.HasIONSlotKeyword("TRG x ION Slot"));
            Assert.IsTrue(ServiceInstance.HasIONSlotKeyword("ION API Intergration"));

            Assert.IsFalse(ServiceInstance.HasIONSlotKeyword("target Intergration"));
            Assert.IsFalse(ServiceInstance.HasIONSlotKeyword("Intergration 測試用"));
            Assert.IsFalse(ServiceInstance.HasIONSlotKeyword("(測試用) AAAABBBB"));
            Assert.IsFalse(ServiceInstance.HasIONSlotKeyword("天馬管理團隊"));
            return;
        }


    }
}

