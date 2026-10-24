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
using target.Models;
using target.BL.Services;
using System.Collections.Generic;
using target.Models.Table;

namespace target.Testing
{
    [TestClass]
    public class UserLoginServiceTests : BaseTest
    {

        public IUserLoginService UserLoginService { get; set; }

        public IUserInfoService UserInfoService { get; set; }

        public UserRepository UserRepository { get; set; }



        public class MyMockUserLoginService : UserLoginService
        {
            protected override List<UserOperateRecordEntity> GetRecentLoginOperateRecords(int userID)
            {
                return new List<UserOperateRecordEntity>
                {
                    new UserOperateRecordEntity
                    {
                        DeviceID = SystemConstant.DefaultDeviceHDCode,
                        CreateTime = new DateTime(2024 , 1 ,1)
                    },

                    new UserOperateRecordEntity
                    {
                        DeviceID = SystemConstant.DefaultDeviceHDCode,
                        CreateTime = new DateTime(2024 , 1 ,1)
                    },

                    new UserOperateRecordEntity
                    {
                        DeviceID = SystemConstant.DefaultDeviceHDCode,
                        CreateTime = new DateTime(2024 , 1 ,1)
                    },

                    new UserOperateRecordEntity
                    {
                        DeviceID = SystemConstant.DefaultDeviceHDCode,
                        CreateTime = new DateTime(2024 , 1 ,1)
                    },

                    new UserOperateRecordEntity
                    {
                        DeviceID = SystemConstant.DefaultDeviceHDCode,
                        CreateTime = new DateTime(2024 , 1 ,1)
                    },

                    new UserOperateRecordEntity
                    {
                        DeviceID = SystemConstant.DefaultDeviceHDCode,
                        CreateTime = new DateTime(2024 , 1 ,1)
                    },

                    new UserOperateRecordEntity
                    {
                        DeviceID = SystemConstant.DefaultDeviceHDCode,
                        CreateTime = new DateTime(2024 , 1 ,1)
                    },
                };
            }
        }





        [TestMethod]
        public void User_Login_WihtoutPrefix_Test()
        {
            string myAccount = _TEST_DATA.NotSuperMerchant.UserName;

            myAccount = myAccount.Replace("AE-", "");

            AssertExpectExceptionProcess(() =>
            {
                //模擬忘記打prefix
                var query = UserLoginService.Login(new UserLoginRequestModel
                {
                    Account = myAccount,
                    Password = [REDACTED],
                    Device = Guid.NewGuid().ToString(),
                    ServerHash = AppHelper.GenerateHashKeyByServerTime().Hash0,
                    Sign = SystemConstant.AdminSysAPI_Login_MagicSignHash
                });

            }, ApiErrorCode.InvalidParameter_AccountMustPrefixFormatPleaseContactAccountProvider);
                        
            return;
        }


        [TestMethod]
        public void User_Login_Test()
        {
            string loginAccount = _TEST_DATA.Default.UserName;
            var query = UserLoginService.Login(new UserLoginRequestModel 
            { 
                Account = loginAccount, 
                Password = [REDACTED],
                Device = Guid.NewGuid().ToString(),
                ServerHash = AppHelper.GenerateHashKeyByServerTime().Hash0,
                Sign = SystemConstant.AdminSysAPI_Login_MagicSignHash
            });

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.Access_Token);
            Assert.IsNotNull(query.Auth_2FA_QRCode);
            Assert.IsNotNull(query.LoginUserName.ToLower() == loginAccount.ToLower());
            Assert.IsTrue(query.Auth_2FA_QRCode.Contains("base64"));

            UserInfoService.ParseAndCheckIsValidJwtToken(query.Access_Token);
            return;
        }


        [TestMethod]
        public void User_Login_Account002_Test()
        {
            string loginAccount = _TEST_DATA.NotSuperMerchant.UserName;
            var query = UserLoginService.Login(new UserLoginRequestModel
            {
                Account = loginAccount,
                Password = [REDACTED],
                Device = Guid.NewGuid().ToString(),
                ServerHash = AppHelper.GenerateHashKeyByServerTime().Hash0,
                Sign = SystemConstant.AdminSysAPI_Login_MagicSignHash
            });

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.Access_Token);
            Assert.IsNotNull(query.LoginUserName.ToLower() == loginAccount.ToLower());

            UserInfoService.ParseAndCheckIsValidJwtToken(query.Access_Token);
            return;
        }



        [TestMethod]
        public void User_Login_DisableUser_FailLogin_Test()
        {
            var queryUsers = UserRepository.GetPagingByFilter(new QueryUserRequestViewModel
            {
                PageSize = 5,
                Status = NormalActiveStatus.Disable.ToInteger()
            });

            var matchFirst = queryUsers.Items.FirstOrDefault();

            if(matchFirst != null)
            {
                AssertExpectExceptionProcess(() =>
                {
                    var query = UserLoginService.Login(new UserLoginRequestModel
                    {
                        Account = matchFirst.UserName,
                        Password = [REDACTED],
                        Device = Guid.NewGuid().ToString(),
                        ServerHash = AppHelper.GenerateHashKeyByServerTime().Hash0,
                        Sign = SystemConstant.AdminSysAPI_Login_MagicSignHash
                    });

                }, ApiErrorCode.InvalidStatus_AccountStatus);                
            }

            return;
        }

        [TestMethod]
        public void User_Login_WrongSignHash_Test()
        {
            AssertExpectExceptionProcess(() =>
            {
                var query = UserLoginService.Login(new UserLoginRequestModel
                {
                    Account = _TEST_DATA.Default.UserName,
                    Password = [REDACTED],
                    Device = Guid.NewGuid().ToString(),
                    ServerHash = AppHelper.GenerateHashKeyByServerTime().Hash0,
                    Sign = "QQQQQYYY"
                });

            }, ApiErrorCode.InvalidParameter_SignKeyExpired);
        }



        [TestMethod]
        public void UserLogin_IsTrustUserByLoginTimesAndDevice_Test()
        {
            var myService = new UserLoginService();
            Assert.IsTrue(myService.IsTrustUserByLoginTimesAndDevice(_TEST_DATA.Default.UserID, SystemConstant.DefaultDeviceHDCode));
            Assert.IsFalse(myService.IsTrustUserByLoginTimesAndDevice(-1, SystemConstant.DefaultDeviceHDCode));
        }


        [TestMethod]
        public void UserLogin_IsTrustUserByLoginTimesAndDevice_LoginCountNotEnough_Test()
        {
            var mockService = new MyMockUserLoginService();
            Assert.IsFalse(mockService.IsTrustUserByLoginTimesAndDevice(_TEST_DATA.Default.UserID, SystemConstant.DefaultDeviceHDCode));
        }




        [TestMethod]
        public void User_Login_ExpiredServerHash_Test()
        {
            var myHash = AppHelper.GenerateHashKeyByServerTime(DateTimeHelper.GetCurrentTime().AddMonths(-3)).Hash0;

            AssertExpectExceptionProcess(() =>
            {
                var query = UserLoginService.Login(new UserLoginRequestModel
                {
                    Account = _TEST_DATA.Default.UserName,
                    Password = [REDACTED],
                    Device = Guid.NewGuid().ToString(),
                    ServerHash = myHash,
                    Sign = SystemConstant.AdminSysAPI_Login_MagicSignHash
                });

            }, ApiErrorCode.InvalidParameter_PageExpiredPleaseRefreshAgain);
        }


        [TestMethod]
        public void User_Login_Fail_Test()
        {
            try
            {
                string failPassword = [REDACTED]";
                var query = UserLoginService.Login(new UserLoginRequestModel
                {
                    Account = _TEST_DATA.Default.UserName,
                    Password = [REDACTED],
                    Device = Guid.NewGuid().ToString(),
                    ServerHash = AppHelper.GenerateHashKeyByServerTime().Hash0,
                    Sign = SystemConstant.AdminSysAPI_Login_MagicSignHash
                });

                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected);

            }
            catch(Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.NotFoundUserLogin);
            }
            
        }


        [TestMethod]
        public void IsPasswordCorrect_Test()
        {
            var myService = new UserLoginService();

            string defaultPassword = [REDACTED];

            string dbPasswordHash1 = AppHelper.GeneratePasswordHash(defaultPassword);
            Assert.IsTrue(myService.IsPasswordCorrect(dbPasswordHash1, $"{defaultPassword}"));
            Assert.IsTrue(myService.IsPasswordCorrect(dbPasswordHash1, $"{defaultPassword}\t"));
            Assert.IsTrue(myService.IsPasswordCorrect(dbPasswordHash1, $"{defaultPassword} "));
            Assert.IsFalse(myService.IsPasswordCorrect(dbPasswordHash1, $"{defaultPassword}4"));

            string dbPasswordHash2 = AppHelper.GeneratePasswordHash("qV3U9x4Ps");
            Assert.IsTrue(myService.IsPasswordCorrect(dbPasswordHash2, "qV3U9x4Ps"));
            Assert.IsFalse(myService.IsPasswordCorrect(dbPasswordHash2, "qV3U9x4Ps\t"));
            Assert.IsFalse(myService.IsPasswordCorrect(dbPasswordHash2, "qV3U9x4Ps "));
            Assert.IsFalse(myService.IsPasswordCorrect(dbPasswordHash2, "abc1234"));
            Assert.IsFalse(myService.IsPasswordCorrect(dbPasswordHash2, "XXXXXXXX"));

            return;
        }

    }
}

