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
using System;
using target.BL.Services;
using target.Models;

namespace target.Testing
{
    [TestClass]
    public class UsersServiceTests : BaseTest
    {

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public OrganizationRepository OrganizationRepository { get; set; }

        public IUsersService UsersService { get; set; }

        public UserCustomizedSettingRepository UserCustomizedSettingRepository { get; set; }



        [TestMethod]
        public void IsAccountExist_Test()
        {
            string testAccount = Guid.NewGuid().ToString();
            var isExist1 = UsersService.IsAccountExistInMyOrganization(_TEST_DATA.Default.UserID, testAccount);
            var isExist2 = UsersService.IsAccountExist(_TEST_DATA.Default.OrganizationID, testAccount);
            var isExist3 = UsersService.IsAccountExist(_TEST_DATA.Default.OrganizationID, _TEST_DATA.Default.UserName);

            Assert.IsFalse(isExist1);
            Assert.IsFalse(isExist2);
            Assert.IsTrue(isExist3);
            return;
        }



        [TestMethod]
        public void IsContainsOrganizationPrefixCheck_Test()
        {
            int orgID = _TEST_DATA.Default.OrganizationID;

            var queryOrg = OrganizationRepository.GetById(orgID);
            string orgCode = queryOrg.OrganizationCode;

            string account0 = $"test001";
            string account1 = $"{orgCode}-test001";
            string account2 = $"{orgCode}-{orgCode}-test001";
            string account3 = $"{orgCode}{orgCode}test001";
            string account4 = $"{orgCode}-{orgCode}_test001";

            var result0 = UsersService.IsContainsOrganizationPrefixCheck(orgID, account0);
            var result1 = UsersService.IsContainsOrganizationPrefixCheck(orgID, account1);
            var result2 = UsersService.IsContainsOrganizationPrefixCheck(orgID, account2);
            var result3 = UsersService.IsContainsOrganizationPrefixCheck(orgID, account3);
            var result4 = UsersService.IsContainsOrganizationPrefixCheck(orgID, account4);

            Assert.IsFalse(result0.IsContainsTwice);
            Assert.IsFalse(result0.IsContains);

            Assert.IsTrue(result1.IsContains);
            Assert.IsFalse(result1.IsContainsTwice);

            Assert.IsTrue(result2.IsContains);
            Assert.IsTrue(result2.IsContainsTwice);

            Assert.IsFalse(result3.IsContainsTwice);
            Assert.IsFalse(result4.IsContainsTwice);
            return;
        }


        [TestMethod]
        public void GetUsersPagingByFilter_Test()
        {
            var query = UsersService.GetUsersPagingByFilter(_TEST_DATA.Default.UserID, new QueryUserApiRequestParameterModel { PageSize = 10 });
            Assert_NotEmptyCollection(query.Items);
            var firstUser = query.Items.First();
            var riskScoreSetting = UserCustomizedSettingRepository.GetByUniqueKey(firstUser.UserID, UserCustomizedSettingEnum.RiskScore);
            var expectedRiskScore = new SystemConfigSettingParseModel(
                UserCustomizedSettingEnum.RiskScore.ToInteger(),
                riskScoreSetting?.SettingValue)
                .SettingValueOfDecimal;
            Assert.AreEqual(expectedRiskScore, firstUser.RiskScore);
            return;
        }

        [TestMethod]
        public void GetUsersPagingByFilter_HasRiskScoreTag_Test()
        {
            var query = UsersService.GetUsersPagingByFilter(_TEST_DATA.Default.UserID, new QueryUserApiRequestParameterModel
            {
                PageSize = 1000,
                SpecialMerchantFilterType = SpecialFilterTypeOfUsers.HasRiskScoreTag.ToInteger()
            });

            Assert.IsTrue(query.Items.All(x =>
                UserCustomizedSettingRepository.GetByUniqueKey(x.UserID, UserCustomizedSettingEnum.RiskScore) != null));
        }


        [TestMethod]
        public void CheckSqlInjectionAndRiskScore_Test()
        {
            var userService = new UsersService();

            AssertExpectExceptionProcess(() =>
            {
                userService.CheckSqlInjectionAndRiskScore(_TEST_DATA.NotSuperMerchant.UserID, new UserCreateModel
                {
                    DisplayName = "TO-123123123444444\"/**/and/**/DBMS_PIPE.RECEIVE_MESSAGE(\"h\",5)=\"h",
                });

            }, ApiErrorCode.InvalidParameter_Body);

            AssertExpectExceptionProcess(() =>
            {
                userService.CheckSqlInjectionAndRiskScore(_TEST_DATA.NotSuperMerchant.UserID, new UserCreateModel
                {
                    DisplayName = "TO-12312312344444 DBMS_PIPE.RECEIVE_MESSAGE(\"h\",5)= SLEEP(8) ",
                });

            }, ApiErrorCode.InvalidParameter_Body);

            return;
        }



        [TestMethod]
        public void GetUsersPagingByFilter_AgentAccount_Test()
        {
            int userID = _TEST_DATA.CreditAgent.UserID;
            var query = UsersService.GetUsersPagingByFilter(userID, new QueryUserApiRequestParameterModel { PageSize = 1000 });
            Assert_NotEmptyCollection(query.Items);


            var myAccount = query.Items.Where(x => x.UserID == userID).FirstOrDefault();

            Assert.IsNotNull(myAccount);
            Assert.IsTrue(myAccount.ReferenceCreditMerchant > 0);

            var myChilds = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = myAccount.ReferenceCreditMerchant
            });

            var myAgentIDs = myChilds.Select(x => x.MerchantID).ToList();

            //所有能查到的帳號 都要隸屬於自己樹狀底下
            Assert.IsTrue(query.Items.All(x => myAgentIDs.Contains(x.ReferenceCreditMerchant.Value)));
            return;
        }


        [TestMethod]
        public void GetUsersPagingByFilter_AgentAccount_Layer2_Test()
        {
            var queryByAdmin = UsersService.GetUsersPagingByFilter(_TEST_DATA.Default.UserID, new QueryUserApiRequestParameterModel 
            { 
                PageSize = 1000,
                ReferenceCreditMerchantID = _TEST_DATA.CreditAgent.Layer2_MerchantID
            });

            if(queryByAdmin.Items.IsNotEmptyNull())
            {
                var matchAccount = queryByAdmin.Items.FirstOrDefault();

                var queryByAgent = UsersService.GetUsersPagingByFilter(matchAccount.UserID, new QueryUserApiRequestParameterModel
                {
                    PageSize = 1000,
                    ReferenceCreditMerchantID = _TEST_DATA.CreditAgent.Layer2_MerchantID
                });

                var myChilds = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    HierarchyOfTargerMerchant = matchAccount.ReferenceCreditMerchant
                });

                var myAgentIDs = myChilds.Select(x => x.MerchantID).ToList();

                //所有能查到的帳號 都要隸屬於自己樹狀底下
                Assert.IsTrue(queryByAgent.Items.All(x => myAgentIDs.Contains(x.ReferenceCreditMerchant.Value)));
            }

            return;
        }


        [TestMethod]
        public void GetUsersPagingByFilter_InvalidMerchantID_Test()
        {
            int userID = _TEST_DATA.CreditAgent.UserID_Layer2;

            var query1 = UsersService.GetUsersPagingByFilter(userID, new QueryUserApiRequestParameterModel
            {
                PageSize = 10,
                ReferenceCreditMerchantID = _TEST_DATA.Default.MerchantID,
            });

            var query2 = UsersService.GetUsersPagingByFilter(userID, new QueryUserApiRequestParameterModel
            {
                PageSize = 10,
                ReferenceCreditMerchantID = _TEST_DATA.CreditAgent.UserID,
            });

            Assert.IsTrue(query1.Items.IsEmptyOrNull());
            Assert.IsTrue(query2.Items.IsEmptyOrNull());
            return;
        }



        [TestMethod]
        public void GetUsersPagingByFilter_FilterColumn_Test()
        {
            var query = UsersService.GetUsersPagingByFilter(_TEST_DATA.Default.UserID, new QueryUserApiRequestParameterModel
            {
                PageSize = 10,
                IMChannelID = "QQQQQ"
            });

            Assert.IsTrue(query.Items.IsEmptyOrNull());
            return;
        }


        [TestMethod]
        public void GetUserByID_Test()
        {
            var query = UsersService.GetUserByID(_TEST_DATA.Default.UserID);
            Assert.IsNotNull(query);
            Assert.IsTrue(query.UserID == _TEST_DATA.Default.UserID);
            return;
        }



        [TestMethod]
        public void ValidateRoleAndOrgIsValidInput_Test()
        {
            int other_roleID = _TEST_DATA.NotSuperMerchant.RoleID; 

            var myUserService = new UsersService();
            myUserService.ValidateRoleAndOrgIsValidInput(_TEST_DATA.Default.UserID, SystemConstant.AdminSysAPI_SuperRoleID, _TEST_DATA.NotSuperMerchant.OrganizationID);
            myUserService.ValidateRoleAndOrgIsValidInput(_TEST_DATA.Default.UserID, SystemConstant.AdminSysAPI_SuperRoleID, _TEST_DATA.Default.OrganizationID);
            myUserService.ValidateRoleAndOrgIsValidInput(_TEST_DATA.NotSuperMerchant.UserID, other_roleID, _TEST_DATA.NotSuperMerchant.OrganizationID);

            AssertExpectExceptionProcess(() =>
            {
                myUserService.ValidateRoleAndOrgIsValidInput(_TEST_DATA.NotSuperMerchant.UserID, SystemConstant.AdminSysAPI_SuperRoleID, _TEST_DATA.NotSuperMerchant.OrganizationID);

            }, ApiErrorCode.InvalidParameter);

            AssertExpectExceptionProcess(() =>
            {
                myUserService.ValidateRoleAndOrgIsValidInput(_TEST_DATA.NotSuperMerchant.UserID, other_roleID, _TEST_DATA.Default.OrganizationID);

            }, ApiErrorCode.InvalidParameter);

            return;
        }



        [TestMethod]
        public void CopyByUserID_Test()
        {
            var result = UsersService.CopyByUserID(_TEST_DATA.Default.UserID, new UserCopyRequestModel
            {
                SourceUserID = _TEST_DATA.Default.UserID,
                UserName = "MSTest_" + DateTimeHelper.GetCurrentTimeTimestamp().ToString()
            });

            UsersService.Delete(result.ResultId);
            return;
        }



        [TestMethod]
        public void User_Create_Test()
        {
            int userID = _TEST_DATA.Default.UserID;
            int orgID = _TEST_DATA.Default.OrganizationID;
            string orgCode = _TEST_DATA.Default.OrganizationCode;
            string userName = "MSTest_v13" + DateTimeHelper.GetCurrentTime(8).ToString("yyyyMMdd");
            string fullUserName = $"{orgCode}-{userName}";

            var isExist = UsersService.IsAccountExist(orgID, userName);

            if (!isExist)
            {
                var queryTemplateUser = UsersService.GetUserByID(userID);

                var insertResult = UsersService.Create(userID, new UserCreateModel
                {
                    Password = [REDACTED]",
                    ConfirmPassword = [REDACTED]",
                    UserName = fullUserName,
                    OrganizationID = orgID,
                    DisplayName = fullUserName,
                    RoleID = queryTemplateUser.RoleID,
                    TimeZoneAreaID = queryTemplateUser.TimeZoneAreaID,
                    AssignMerchantMode = UserAssignMerchantModeEnum.ViewSelectMerchant.ToInteger(),
                    Merchants = new System.Collections.Generic.List<int>
                    {
                        _TEST_DATA.Default.MerchantID,
                    }
                });

                UsersService.Delete(insertResult.ResultId);
            }

            return;
        }



        [TestMethod]
        public void ValidateBeforeCreateUser_Test()
        {
            int userID = _TEST_DATA.Default.UserID;
            int orgID = _TEST_DATA.Default.OrganizationID;
            string orgCode = _TEST_DATA.Default.OrganizationCode;
            string userName = "MSTest_" + Guid.NewGuid().ToString().Truncate(10);
            string fullUserName = $"{orgCode}-{userName}";

            var myModel = new UserCreateModel
            {
                Password = [REDACTED]",
                ConfirmPassword = [REDACTED]",
                UserName = userName,
                OrganizationID = orgID,
                DisplayName = fullUserName,
                RoleID = _TEST_DATA.Default.RoleID,
                AssignMerchantMode = UserAssignMerchantModeEnum.ViewSelectMerchant.ToInteger(),
                Merchants = new System.Collections.Generic.List<int>
                    {
                        _TEST_DATA.Default.MerchantID,
                    }
            };

            AssertExpectExceptionProcess(() =>
            {
                UsersService.ValidateBeforeCreateUser(userID, myModel);
            }, ApiErrorCode.InvalidParameter_NotAccountFormat);
                    
            return;
        }



        [TestMethod]
        public void User_Update_Test()
        {
            int userID = _TEST_DATA.Default.UserID;
            var query = UsersService.GetUserByID(userID);

            Assert.IsNotNull(query);

            var updateModel = query.ReturnInjecteObject<UserUpdateModel>();
            updateModel.Merchants = query.Merchants;
            updateModel.Status = NormalActiveStatus.Enable.ToInteger(); //永遠保持開啟

            UsersService.Update(userID, updateModel);
            return;
        }


        [TestMethod]
        public void UserUpdateUserBriefInfo_Test()
        {
            UsersService.UpdateUserBriefInfo(_TEST_DATA.Default.UserID, new UpdateUserBriefInfoRequestModel
            {
                Memo = "MSTest修改" + DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                UserID = _TEST_DATA.Default.UserID
            });

            return;
        }




    }
}

