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
using target.BL.Services;

namespace target.Testing
{
    [TestClass]
    public class BaseAdminSysServiceTests : BaseTest
    {
        public IAdminSysDropdownService ServiceInstance { get; set; }

        public UserCustomizedSettingRepository UserCustomizedSettingRepository { get; set; }

        public UserRepository UserRepository { get; set; }


        public class MockBaseAdminSysService : BaseAdminSysService
        {
            public AdminUserCacheModelAdminUpdateViewModel MockGetCurrentOwnPermissionByCache(int userID)
            {
                return this.GetCurrentOwnPermissionByCache(userID);
            }
        }


        [TestMethod]
        public void GetBlockRelativeUsers_Test()
        {
            var service = new BaseAdminSysService();

            int userID = _TEST_DATA.CreditAgent.UserID;
            var userIDs = service.GetBlockRelativeUsers(UserRepository , userID);

            Assert.IsTrue(userIDs.Contains(userID));
            Assert.IsTrue(userIDs.Count() >= 3);
            return;
        }


        [TestMethod]
        public void MockGetCurrentOwnPermissionByCache_Test()
        {
            var service = new MockBaseAdminSysService();
            var query = service.MockGetCurrentOwnPermissionByCache(_TEST_DATA.Default.UserID);
            Assert.IsTrue(query != null);
            return;
        }


        [TestMethod]
        public void HasTargetPermission_Test()
        {
            var service = new BaseAdminSysService();
            Assert.IsTrue(service.HasTargetPermission(_TEST_DATA.Default.UserID, "MembersManagement"));
            Assert.IsFalse(service.HasTargetPermission(_TEST_DATA.Default.UserID, "Special_AlertNotifyWhenLogin"));

            Assert.IsTrue(service.HasTargetPermission(_TEST_DATA.CreditAgent.UserID, "MembersManagement"));
            Assert.IsFalse(service.HasTargetPermission(_TEST_DATA.CreditAgent.UserID, "SystemSetting_SystemConfigSetting_Update"));
            Assert.IsTrue(service.HasTargetPermission(_TEST_DATA.CreditAgent.UserID, "Special_AlertNotifyWhenLogin"));
            return;
        }


        [TestMethod]
        public void CheckViewUserPermission_Test()
        {
            var service = new BaseAdminSysService();
            service.CheckViewUserPermission(_TEST_DATA.Default.UserID, _TEST_DATA.Default.UserID);
            service.CheckViewUserPermission(_TEST_DATA.Default.UserID, _TEST_DATA.NotSuperMerchant.UserID);
            service.CheckViewUserPermission(_TEST_DATA.Default.UserID, _TEST_DATA.CreditAgent.UserID);


            service.CheckViewUserPermission(_TEST_DATA.CreditAgent.UserID, _TEST_DATA.CreditAgent.UserID);
            service.CheckViewUserPermission(_TEST_DATA.CreditAgent.UserID, _TEST_DATA.CreditAgent.UserID_Layer2);

            //下級代理不能查看上級帳號
            AssertExpectExceptionProcess(() =>
            {
                service.CheckViewUserPermission(_TEST_DATA.CreditAgent.UserID_Layer2, _TEST_DATA.CreditAgent.UserID);

            }, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);


            AssertExpectExceptionProcess(() =>
            {
                service.CheckViewUserPermission(_TEST_DATA.CreditAgent.UserID, _TEST_DATA.Default.UserID);

            }, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);


            AssertExpectExceptionProcess(() =>
            {
                service.CheckViewUserPermission(_TEST_DATA.CreditAgent.UserID, _TEST_DATA.Default.UserID);

            }, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);


            return;
        }



        [TestMethod]
        public void CheckViewMerchantPermission_Test()
        {
            var service = new BaseAdminSysService();

            int adminUserID = _TEST_DATA.Default.UserID;
            service.CheckViewMerchantPermission(adminUserID, _TEST_DATA.Default.MerchantID);
            service.CheckViewMerchantPermission(adminUserID, _TEST_DATA.NotSuperMerchant.MerchantID);
            service.CheckViewMerchantPermission(adminUserID, _TEST_DATA.CreditAgent.MerchantID);

            service.CheckViewMerchantPermission(_TEST_DATA.CreditAgent.UserID, _TEST_DATA.CreditAgent.MerchantID);
            service.CheckViewMerchantPermission(_TEST_DATA.CreditAgent.UserID, _TEST_DATA.CreditAgent.Layer2_MerchantID);
            service.CheckViewMerchantPermission(_TEST_DATA.CreditAgent.UserID_Layer2, _TEST_DATA.CreditAgent.Layer2_MerchantID);

            //下級代理不能查看上級帳號
            AssertExpectExceptionProcess(() =>
            {
                service.CheckViewMerchantPermission(_TEST_DATA.CreditAgent.UserID_Layer2, _TEST_DATA.CreditAgent.MerchantID);

            }, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            //參數填false
            AssertExpectExceptionProcess(() =>
            {
                service.CheckViewMerchantPermission(_TEST_DATA.CreditAgent.UserID_Layer2, _TEST_DATA.CreditAgent.MerchantID , isTriggerRiskScoreProcess: false);

            }, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            AssertExpectExceptionProcess(() =>
            {
                service.CheckViewMerchantPermission(_TEST_DATA.CreditAgent.UserID, _TEST_DATA.Default.MerchantID);

            }, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            return;
        }


        [TestMethod]
        public void CounterRiskUserAndThrowException_Test()
        {
            var service = new BaseAdminSysService();

            int userID = 9999999;

            decimal riskRate = 3.0M;

            var query1 = UserCustomizedSettingRepository.GetByUniqueKey(userID, UserCustomizedSettingEnum.RiskScore);

            var myException = new TerproException(ApiErrorCode.BusinessLogicDeny , new ErrorDetailModel
            {
                RiskRate = riskRate
            });

            AssertExpectExceptionProcess(() =>
            {
                service.CounterRiskUserAndThrowException(userID, myException);

            } , myException.ErrroCode);

            var query2 = UserCustomizedSettingRepository.GetByUniqueKey(userID, UserCustomizedSettingEnum.RiskScore);


            var parse1 = new SystemConfigSettingParseModel(0, query1.SettingValue);
            var parse2 = new SystemConfigSettingParseModel(0, query2.SettingValue);

            Assert.IsTrue((parse2.SettingValueOfDecimal - parse1.SettingValueOfDecimal) == riskRate);
            return;
        }


    }
}

