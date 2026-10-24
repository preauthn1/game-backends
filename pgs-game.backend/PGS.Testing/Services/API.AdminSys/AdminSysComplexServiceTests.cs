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
    public class AdminSysComplexServiceTests : BaseTest
    {
        public MerchantRepository MerchantRepository { get; set; }

        public RoleRepository RoleRepository { get; set; }

        public UserRepository UserRepository { get; set; }

        public IUsersService UsersService { get; set; }

        public IAdminSysComplexService ServiceInstance { get; set; }

        public IMerchantService MerchantService { get; set; }

        [TestMethod]
        public void CopyAndBatchCreateNewMerchant_Test()
        {
            int orgID = _TEST_DATA.Default.OrganizationID;
            string account = "MSTest_v8_"  + $"{DateTimeHelper.GetCurrentTime().ToString("yyyyMMddHH")}";

            //避免資料太多 同一段時間內 只建一筆
            bool isExist = MerchantService.IsMerchantNameExist(account).IsExist;
            bool isExist2 = UsersService.IsAccountExist(orgID , account);

            if (!isExist && !isExist2)
            {
                var firstRole = RoleRepository.GetPagingByFilter(new QueryRoleRequestViewModel()).Items.FirstOrDefault();

                ServiceInstance.CopyAndBatchCreateNewMerchant(_TEST_DATA.Default.UserID, new MerchantBatchCopyAndCreateRequestModel
                {
                    SourceMerchantID = _TEST_DATA.Default.MerchantID,
                    IsCreateUserAccount = true,
                    CreateAccountInfo = new MerchantCopyCreateUserAccountExtraProperty
                    {
                        Account = account,
                        OrganizationID = orgID,
                        RoleID = firstRole.RoleID,
                        Password = [REDACTED]
                    },
                    MerchantNameList = new List<string>
                    {
                        account
                    }
                });

                var queryMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
                {
                    NameMatch = account
                });

                var queryUser = UserRepository.GetByFilter(new QueryUserRequestViewModel
                {
                    Keyword = account
                });

                Assert_NotEmptyCollection(queryMerchant);

                //清除產生的垃圾資料
                MerchantService.Delete(_TEST_DATA.Default.UserID, queryMerchant.FirstOrDefault().MerchantID);
                UsersService.Delete(queryUser.FirstOrDefault().UserID);
            }

            return;
        }

    }
}

