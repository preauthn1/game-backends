using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
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
    public class IPAddressListTemplateServiceTests : BaseTest
    {

        public IIPAddressListTemplateService ServiceInstance { get; set; }


        [TestMethod]
        public void GetByPagingFilter_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var query = ServiceInstance.GetByPagingFilter(userID, new QueryIPAddressListTemplateApiRequestParameterModel
            {

            });

            var query2 = ServiceInstance.GetByPagingFilter(_TEST_DATA.CreditAgent.UserID, new QueryIPAddressListTemplateApiRequestParameterModel
            {

            });

            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void Create_Delete_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var createParam = new IPAddressListTemplateCreateRequestModel
            {
                DisplayName = $"MSTest_v8_{DateTimeHelper.GetCurrentTimeTimestamp()}",
                IpAddressWithLinebreak = $"0.0.0.0 {Environment.NewLine} 0.0.0.0   ", //允許空白
            };

            var result = ServiceInstance.Create(userID , createParam);

            int templateID = result.ResultId;

            ServiceInstance.Update(userID, new IPAddressListTemplateUpdateRequestModel
            {
                Id = templateID,
                DisplayName = $"MSTest_{DateTimeHelper.GetCurrentTimeTimestamp()}_Update",
                IpAddressWithLinebreak = $"0.0.0.0 {Environment.NewLine} 0.0.0.0",
            });

            ServiceInstance.UpdateAssignMerchant(userID, new IPAddressListTemplateUpdateAssignMerchantIDRequestModel
            {
                Id = templateID,
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID
            });

            ServiceInstance.UpdateAssignMerchant(userID, new IPAddressListTemplateUpdateAssignMerchantIDRequestModel
            {
                Id = templateID,
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                IsDelete = true
            });

            ServiceInstance.Delete(userID, result.ResultId);
            return;
        }


        [TestMethod]
        public void Create_InvalidIPAddress_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var createParam = new IPAddressListTemplateCreateRequestModel
            {
                DisplayName = $"MSTest_{DateTimeHelper.GetCurrentTimeTimestamp()}",
                IpAddressWithLinebreak = "AAA.BBBB.1.1",
            };

            AssertExpectExceptionProcess(() =>
            {
                var result = ServiceInstance.Create(userID, createParam);

            }, ApiErrorCode.InvalidParameter_NotIPAddressFormat);
            
            return;
        }


        [TestMethod]
        public void AppendNewIP_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            var result = ServiceInstance.GetByPagingFilter(userID, new QueryIPAddressListTemplateApiRequestParameterModel
            {
                PageSize = 5
            });

            if(result.Items.IsNotEmptyNull())
            {
                var matchFirst = result.Items.FirstOrDefault();

                string targetIP = SystemConstant.CompanyIPAddress;

                string newIPs = $"{targetIP},0.0.0.0{Environment.NewLine}0.0.0.0";

                ServiceInstance.AppendNewIpToSetting(userID, new IPAddressListAppendRequestModel
                {
                    Id = matchFirst.Id,
                    IPAddress = newIPs
                });

                var query = ServiceInstance.GetByPagingFilter(userID, new QueryIPAddressListTemplateApiRequestParameterModel
                {
                    IDs = matchFirst.Id.ToListCollection(),
                    IPAddressKeyword = targetIP
                });

                Assert.IsTrue(query.Items.IsNotEmptyNull());
            }

            return;
        }



        [TestMethod]
        public void SetWhiteListIPsForSingleMerchant_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            string myIPsText = @"
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0
0.0.0.0";

            var myIPs = AppHelper.AnalyzeTextFromRawTextBySpaceChar(myIPsText);

            ServiceInstance.SetWhiteListIPsForSingleMerchant(userID, _TEST_DATA.CreditAgent.Layer3_MerchantID , myIPs);

            return;
        }


    }
}

