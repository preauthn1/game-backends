using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class AdminSysValidateCheckLogicRefTests : BaseTest
    {
        public GameListRepository GameListRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public IAdminSysValidateCheckLogicRefService ServiceInstance { get; set; }


        [TestMethod]
        public void CheckSingleWalletMerchantNotSupprtGame_Test()
        {
            var notSupportSwGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameVendorTypes = GameVendorTypeEnum.target.ToInteger().ToListCollection()
                })
                .Where(x => !x.IsSupportSingleWallet)
                .Select(x => x.GameID)
                .ToList();

            AssertExpectExceptionProcess(() =>
            {
                ServiceInstance.CheckSingleWalletMerchantNotSupprtGame(_TEST_DATA.SingleWallet.MerchantID, notSupportSwGames);

            }, ApiErrorCode.Forbidden_SingleWalletMerchantDoNotSupportGame);

            return;
        }


        [TestMethod]
        public void CheckGameVendorNotSupprtCurrencyAndThrow_Test()
        {
            ServiceInstance.CheckGameVendorNotSupprtCurrencyAndThrow("MYR", GameVendorTypeEnum.Jili.ToInteger());
            ServiceInstance.CheckGameVendorNotSupprtCurrencyAndThrow("MYR", GameVendorTypeEnum.AGG.ToInteger());

            int gameVendor_funky = GameVendorTypeEnum.Funky.ToInteger();

            ServiceInstance.CheckGameVendorNotSupprtCurrencyAndThrow("MYR", gameVendor_funky);

            AssertExpectExceptionProcess(() =>
            {
                ServiceInstance.CheckGameVendorNotSupprtCurrencyAndThrow("SGD", gameVendor_funky);
            }, ApiErrorCode.Forbidden_NotSupportThis);

            return;
        }




        [TestMethod]
        public void CheckIPAddressAndSourceIsValid_Test()
        {
            var query1 = ServiceInstance.CheckIPAddressAndSourceIsValid(SystemConstant.CompanyIPAddress, "qqqqq", RestrictLevel.MaxRestrictLevel);
            var query2 = ServiceInstance.CheckIPAddressAndSourceIsValid("0.0.0.0", "rory", RestrictLevel.AllowTrustUserName);

            Assert.IsTrue(query1.IsValidateSuccess);
            Assert.IsTrue(query2.IsValidateSuccess);

            var query2_1 = ServiceInstance.CheckIPAddressAndSourceIsValid("0.0.0.0", "yyyyyy", RestrictLevel.MaxRestrictLevel);
            var query2_2 = ServiceInstance.CheckIPAddressAndSourceIsValid("0.0.0.0", "rory", RestrictLevel.MaxRestrictLevel);
            var query2_3 = ServiceInstance.CheckIPAddressAndSourceIsValid("0.0.0.0", "marshall", RestrictLevel.MaxRestrictLevel);
            var query2_4 = ServiceInstance.CheckIPAddressAndSourceIsValid("0.0.0.0", "QQQQQ", RestrictLevel.AllowTrustUserName);
            var query2_5 = ServiceInstance.CheckIPAddressAndSourceIsValid("0.0.0.0", "marshall", RestrictLevel.MaxRestrictLevel);

            Assert.IsFalse(query2_1.IsValidateSuccess);
            Assert.IsFalse(query2_2.IsValidateSuccess);
            Assert.IsFalse(query2_3.IsValidateSuccess);
            Assert.IsFalse(query2_4.IsValidateSuccess);
            Assert.IsFalse(query2_5.IsValidateSuccess);

            return;
        }



        [TestMethod]
        public void IsTrustInternalUserNameAllowLogin_Test()
        {
            var query1 = ServiceInstance.IsTrustInternalUserNameAllowLogin(SystemConstant.CompanyIPAddress, "ion-cs777", 0);
            var query2 = ServiceInstance.IsTrustInternalUserNameAllowLogin(SystemConstant.CompanyIPAddress, "IoN-Cs777", 0);

            Assert.IsTrue(query1);
            Assert.IsTrue(query2);

            var query2_1 = ServiceInstance.IsTrustInternalUserNameAllowLogin(SystemConstant.CompanyIPAddress, "target-joker2", 0);
            var query2_2 = ServiceInstance.IsTrustInternalUserNameAllowLogin(SystemConstant.CompanyIPAddress, "IoN-Cs7778", 0);
            var query2_3 = ServiceInstance.IsTrustInternalUserNameAllowLogin(SystemConstant.CompanyIPAddress, "APGS-jOkEr", 0);

            Assert.IsFalse(query2_1);
            Assert.IsFalse(query2_2);
            Assert.IsFalse(query2_3);
            return;
        }


        [TestMethod]
        public void IsWhiteListIPAddressForAdminSys_Test()
        {
            var myService = new AdminSysValidateCheckLogicRefService();

            var result_1 = myService.IsWhiteListIPAddressForAdminSys("0.0.0.0", "marshall");

            Assert.IsFalse(result_1);

            return;
        }



        [TestMethod]
        public void CheckMerchantJackpotGameAndThrow_Test()
        {
            var merchantIDs = new List<int>
            {
                _TEST_DATA.Default.MerchantID
            };

            ServiceInstance.CheckMerchantJackpotGameAndThrow(merchantIDs, _TEST_DATA.Default.GameID.ToListCollection());
            ServiceInstance.CheckMerchantJackpotGameAndThrow(merchantIDs, (650001).ToListCollection());
            return;
        }



        [TestMethod]
        public void CheckMerchantOnlySupportIDGVisibleGamesAndThrow_Test()
        {
            var merchantIDs = new List<int>
            {
                _TEST_DATA.Default.MerchantID
            };

            ServiceInstance.CheckMerchantOnlySupportIDGVisibleGamesAndThrow(merchantIDs, _TEST_DATA.Default.GameID.ToListCollection());
            ServiceInstance.CheckMerchantOnlySupportIDGVisibleGamesAndThrow(merchantIDs, _TEST_DATA.Default.ThirdPartyGameID.ToListCollection());
            return;
        }




        [TestMethod]
        public void CheckMerchantOnlySupportIDGVisibleGamesAndThrow_ForbiddenMerchant_Test()
        {
            var matcMerchant = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.GameSetting_OnlySupportIDGVisibleGames.ToInteger().ToListCollection(),
                    SettingValueMatch = "1"
                })
                .OrderByRandom()
                .FirstOrDefault();

            if (matcMerchant == null)
                return;

            List<int> merchantIDs = new List<int> { matcMerchant.MerchantID };

            ServiceInstance.CheckMerchantOnlySupportIDGVisibleGamesAndThrow(merchantIDs, _TEST_DATA.Default.GameID_AGG.ToListCollection());

            AssertExpectExceptionProcess(() =>
            {
                ServiceInstance.CheckMerchantOnlySupportIDGVisibleGamesAndThrow(merchantIDs, _TEST_DATA.Default.GameID_Funky.ToListCollection());

            }, ApiErrorCode.Forbidden_NotSupportThisGame);


            return;
        }


    }
}

