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
    public class VendorConfigServiceTests : BaseTest
    {
        public VendorConfigRepository VendorConfigRepository { get; set; }

        public IVendorConfigService ServiceInstance { get; set; }


        [TestMethod]
        public void GetByPagingFilter_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID , new QueryVendorConfigApiRequestParameterModel
            {
                PageSize = 10
            });

            if(query.Items.IsNotEmptyNull())
            {
                var first = query.Items.FirstOrDefault();

                //順手測試 另個案例2
                GetVendorConfigSettinsAndAssert(first.VendorConfigID);

                //順手測試 另個案例3
                ServiceInstance.SyncDefaultVendorConfigMapping(first.VendorConfigID);
            }


            Assert_NotEmptyCollection(query.Items);
            Assert.IsNotNull(new CRUDResultModel());
            return;
        }


        [TestMethod]
        public void GetSingleVendorConfigDetail_Test()
        {
            var query = ServiceInstance.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryVendorConfigApiRequestParameterModel
            {
                IsIncludeDeleteData = false,
                PageSize = 10
            });

            if (query.Items.IsNotEmptyNull())
            {
                var first = query.Items.FirstOrDefault();

                var queryDetail = ServiceInstance.GetSingleVendorConfigDetail(_TEST_DATA.Default.UserID, first.VendorConfigID);

                Assert.IsTrue(queryDetail != null);
            }

            return;
        }







        [TestMethod]
        public void GetMerchantVendorConfigMappings_Test()
        {
            var query = ServiceInstance.GetMerchantVendorConfigMappings(_TEST_DATA.Default.UserID, _TEST_DATA.Default.MerchantID);

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Mappings);
            return;
        }


        [TestMethod]
        public void CreateEmptyVendorConfig_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var result1 = ServiceInstance.CreateEmptyVendorConfig(userID, new VendorConfigCreateRequestModel
            {
                Name = $"MSTest_{DateTimeHelper.GetCurrentTimeTimestamp()}",
                GameVendorID = GameVendorTypeEnum.AGG.ToInteger()
            });

            var result2 = ServiceInstance.CreateEmptyVendorConfig(userID, new VendorConfigCreateRequestModel
            {
                Name = $"MSTest_Live_{DateTimeHelper.GetCurrentTimeTimestamp()}",
                GameVendorID = GameVendorTypeEnum.AGGLive.ToInteger()
            });

            ServiceInstance.DeleteVendorConfig(userID, result1.ResultId);
            ServiceInstance.DeleteVendorConfig(userID, result2.ResultId);
            return;
        }


        private void GetVendorConfigSettinsAndAssert(int vendorConfigID)
        {
            var query = ServiceInstance.GetVendorConfigSettins(vendorConfigID);
            Assert_NotEmptyCollection(query.Groups);
            return;
        }


    }
}

