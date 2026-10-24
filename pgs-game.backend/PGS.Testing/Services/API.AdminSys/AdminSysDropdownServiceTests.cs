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

namespace target.Testing
{
    [TestClass]
    public class AdminSysDropdownServiceTests : BaseTest
    {

        public IAdminSysDropdownService ServiceInstance { get; set; }



        [TestMethod]
        public void GetEnumsDropdownByTypeName_Test()
        {
            var dropdownList1 = ServiceInstance.GetEnumsDropdownByTypeName(typeof(MerchantTypeEnum).Name);
            var dropdownList2 = ServiceInstance.GetEnumsDropdownByTypeName(typeof(MerchantTypeEnum).Name.ToLower());
            var dropdownList3 = ServiceInstance.GetEnumsDropdownByTypeName(typeof(NormalActiveStatus).Name);
            var dropdownList4 = ServiceInstance.GetEnumsDropdownByTypeName(typeof(WalletTransferTypeEnum).Name);
            var dropdownList5 = ServiceInstance.GetEnumsDropdownByTypeName(typeof(WalletTransferTypeEnum).Name.ToUpper());

            Assert_NotEmptyCollection(dropdownList1);
            Assert_NotEmptyCollection(dropdownList2);
            Assert_NotEmptyCollection(dropdownList3);
            Assert_NotEmptyCollection(dropdownList4);
            Assert_NotEmptyCollection(dropdownList5);
            return;
        }



        [TestMethod]
        public void GetAvailableMerchants_Test()
        {
            var query = ServiceInstance.GetAvailableMerchants(_TEST_DATA.Default.UserID , new QueryDropdownMerchantRequestViewModel 
            { 
                IsIncludeCategory = true,
                IsIncludeExtraInfo = true
            });

            var querySimplify = ServiceInstance.GetAvailableMerchants(_TEST_DATA.Default.UserID, new QueryDropdownMerchantRequestViewModel
            {
                IsIncludeCategory = true,
                IsSimplifyItems = true
            });

            var queryByOtherUser = ServiceInstance.GetAvailableMerchants(_TEST_DATA.NotSuperMerchant.UserID , new QueryDropdownMerchantRequestViewModel
            {
                IsIncludeExtraInfo = true
            });

            var queryByCreditAccount = ServiceInstance.GetAvailableMerchants(_TEST_DATA.CreditAgent.UserID, new QueryDropdownMerchantRequestViewModel
            {

            });

            var queryWithExclude = ServiceInstance.GetAvailableMerchants(_TEST_DATA.CreditAgent.UserID, new QueryDropdownMerchantRequestViewModel
            {
                IsExcludeSpecificAgent = true
            });

            Assert_NotEmptyCollection(query);
            Assert_NotEmptyCollection(querySimplify);
            Assert_NotEmptyCollection(queryByOtherUser);
            Assert_NotEmptyCollection(queryByCreditAccount);
            Assert_NotEmptyCollection(queryWithExclude);

            Assert.IsTrue(queryByOtherUser.Count() <= 5);  //此帳號 只能看到特定幾家商戶 (ps. 建議不要亂改此帳號的分配商戶)
            return;
        }


        [TestMethod]
        public void GetCurrency_Test()
        {
            var query1 = ServiceInstance.GetCurrency(_TEST_DATA.Default.UserID);
            var query2 = ServiceInstance.GetCurrency(_TEST_DATA.CreditAgent.UserID);
            var query3 = ServiceInstance.GetCurrency(_TEST_DATA.NotSuperMerchant.UserID);

            Assert.IsTrue(query1.Count() > 70);
            Assert.IsTrue(query2.Count() == 1);
            Assert.IsTrue(query3.Count() == 1);

            return;
        }


        [TestMethod]
        public void GetAvailableMerchants_FilterOrgID_Test()
        {
            var filter = new QueryDropdownMerchantRequestViewModel
            {
                OrganizationID = _TEST_DATA.Default.OrganizationID
            };

            var query = ServiceInstance.GetAvailableMerchants(_TEST_DATA.Default.UserID, filter);
            Assert_NotEmptyCollection(query);
            return;
        }



        [TestMethod]
        public void GetAvailableOrgs_Test()
        {
            var query = ServiceInstance.GetAvailableOrgs(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query);
            return;
        }



        [TestMethod]
        public void GetIMChannels_Test()
        {
            var query1 = ServiceInstance.GetIMChannels(_TEST_DATA.Default.UserID);
            var query2 = ServiceInstance.GetIMChannels(_TEST_DATA.NotSuperMerchant.UserID);
            var query3 = ServiceInstance.GetIMChannels(_TEST_DATA.CreditAgent.UserID);
            return;
        }



        [TestMethod]
        public void GetEvents_Test()
        {
            var myFilter = new LazyCommonRequestModel();
            var query1 = ServiceInstance.GetEvents(_TEST_DATA.Default.UserID , myFilter);
            var query2 = ServiceInstance.GetEvents(_TEST_DATA.CreditAgent.UserID , myFilter);

            Assert.IsTrue(query1.IsNotEmptyNull());
            return;
        }


        [TestMethod]
        public void GetAvailableRoles_Test()
        {
            var query1 = ServiceInstance.GetAvailableRoles(_TEST_DATA.Default.UserID);
            var query2 = ServiceInstance.GetAvailableRoles(_TEST_DATA.NotSuperMerchant.UserID);

            Assert_NotEmptyCollection(query1);
            Assert_NotEmptyCollection(query2);
            return;
        }



        [TestMethod]
        public void GetVendorConfig_Test()
        {
            var query = ServiceInstance.GetVendorConfig(_TEST_DATA.Default.UserID , null);
            Assert_NotEmptyCollection(query);

            var query2 = ServiceInstance.GetVendorConfig(_TEST_DATA.NotSuperMerchant.UserID, null);
            return;
        }


        [TestMethod]
        public void GetVendorConfigSuuportFetchReportAPI_Test()
        {
            var query = ServiceInstance.GetVendorConfigSuuportFetchReportAPI(_TEST_DATA.Default.UserID, null);
            Assert_NotEmptyCollection(query);

            return;
        }


        [TestMethod]
        public void GetEventTypes_Test()
        {
            var query = ServiceInstance.GetEventTypes();
            Assert_NotEmptyCollection(query);
            return;
        }



        [TestMethod]
        public void GetDropdwonsByEnum_Test()
        {
            var query = ServiceInstance.GetDropdownsByEnum<TagTypeEnum>();
            Assert_NotEmptyCollection(query);
            return;
        }



        [TestMethod]
        public void GetInternalTestPlayerAccounts_Test()
        {
            var query = ServiceInstance.GetInternalTestPlayerAccounts();
            Assert_NotEmptyCollection(query);
            return;
        }


        [TestMethod]
        public void GetDynamicQueryItems_Test()
        {
            var query = ServiceInstance.GetDynamicQueryItems();
            Assert_NotEmptyCollection(query);
            return;
        }


        [TestMethod]
        public void GetInternalMerchants_Test()
        {
            var query = ServiceInstance.GetInternalMerchants(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query);
            return;
        }




        [TestMethod]
        public void GetGameTypes_Test()
        {
            var query = ServiceInstance.GetGameTypes();
            Assert_NotEmptyCollection(query);
            return;
        }


        [TestMethod]
        public void GetGameVendorTypes_Test()
        {
            var query1 = ServiceInstance.GetGameVendorTypes(_TEST_DATA.Default.UserID);
            var query2 = ServiceInstance.GetGameVendorTypesExcludeSlot88(_TEST_DATA.Default.UserID);
            var query3 = ServiceInstance.GetThirdPartyVendors(_TEST_DATA.Default.UserID);

            Assert_NotEmptyCollection(query1);
            Assert_NotEmptyCollection(query2);
            Assert_NotEmptyCollection(query3);
            return;
        }



        [TestMethod]
        public void GetGames_Test()
        {
            var query = ServiceInstance.GetGames(_TEST_DATA.Default.UserID , new QueryGameListApiRequestParameter
            {
            });

            Assert_NotEmptyCollection(query);
            return;
        }


        [TestMethod]
        public void GetRootAgents_Test()
        {
            var query1 = ServiceInstance.GetRootAgents(_TEST_DATA.Default.UserID);
            var query2 = ServiceInstance.GetRootAgents(_TEST_DATA.CreditAgent.UserID);

            Assert_NotEmptyCollection(query1);
            Assert_NotEmptyCollection(query2);
            return;
        }


        [TestMethod]
        public void GetHierarchyMerchants_Test()
        {
            var query1 = ServiceInstance.GetHierarchyMerchants(_TEST_DATA.Default.UserID , new LazyCommonRequestModel());
            var query2 = ServiceInstance.GetHierarchyMerchants(_TEST_DATA.CreditAgent.UserID, new LazyCommonRequestModel());
            var query3 = ServiceInstance.GetHierarchyMerchants(_TEST_DATA.CreditAgent.UserID, new LazyCommonRequestModel
            {
                MaxLayer = 3
            });

            Assert_NotEmptyCollection(query1);
            Assert_NotEmptyCollection(query2);
            Assert_NotEmptyCollection(query3);
            return;
        }


        [TestMethod]
        public void GetPerGameTags_Test()
        {
            var query = ServiceInstance.GetPerGameTags(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query);
            return;
        }



        [TestMethod]
        public void GetServers_Test()
        {
            var query = ServiceInstance.GetServers(_TEST_DATA.Default.UserID , null);
            Assert_NotEmptyCollection(query);
            return;
        }


        [TestMethod]
        public void GetServers_FilterGameID_Test()
        {
            var query = ServiceInstance.GetServers(_TEST_DATA.Default.UserID, new QueryServerListApiRequestParameterModel { GameID = _TEST_DATA.Default.GameID });
            Assert_NotEmptyCollection(query);
            return;
        }



        [TestMethod]
        public void GetTimeZoneAreas_Test()
        {
            var query = ServiceInstance.GetTimeZoneAreas();
            Assert_NotEmptyCollection(query);
            return;
        }

    }
}

