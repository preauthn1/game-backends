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
using target.Utility;
using target.Models;

namespace target.Testing
{
    [TestClass]
    public class MerchantServiceTests : BaseTest
    {

        public IMerchantService MerchantService { get; set; }

        public IMerchantCustomizedSettingService MerchantCustomizedSettingService { get; set; }

        public MerchantRepository MerchantRepository { get; set; }


        [TestMethod]
        public void MerchantGetByPagingByFilter_Test()
        {
            var query = MerchantService.GetByPagingFilter(new QueryMerchantApiRequestParameterModel { PageSize = 10 });
            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void MerchantGetByPagingByFilter_Keyword_Test()
        {
            int merchantID_1 = _TEST_DATA.Default.MerchantID;
            int merchantID_2 = _TEST_DATA.SingleWallet.MerchantID;

            var query = MerchantService.GetByPagingFilter(new QueryMerchantApiRequestParameterModel
            {
                PageSize = 10,
                Keyword = $"{merchantID_1} {merchantID_2}",
            });

            Assert_NotEmptyCollection(query.Items);
            Assert.IsTrue(query.Items.Any(z => z.MerchantID == merchantID_1));
            Assert.IsTrue(query.Items.Any(z => z.MerchantID == merchantID_2));
            return;
        }


        [TestMethod]
        public void MerchantGetByPagingByFilte_FilterOrgID_Test()
        {
            var filter = new QueryMerchantApiRequestParameterModel
            {
                PageSize = 10,
                OrganizationIDs = _TEST_DATA.Default.OrganizationID.ToListCollection()
            };

            var query = MerchantService.GetByPagingFilter(filter);
            Assert_NotEmptyCollection(query.Items);
            return;
        }



        [TestMethod]
        public void Create_Test()
        {
            MerchantService.Create(_TEST_DATA.Default.UserID, new MerchantCreateRequestModel
            {
                WalletMode = MerchantWalletModeEnum.TransferMode.ToInteger(),
                Status = NormalActiveStatus.PermanentDelete.ToInteger(),
                ApiSecretKey = Guid.NewGuid().ToString(),
                Currency = "CNY",
                Name = "MSTest_" + DateTimeHelper.GetCurrentTime().ToString("yyMMddHHmmss"),
                DefaultLanguage = SystemConstant.DefaultLang,
                Type = MerchantTypeEnum.InternalTest.ToInteger(),
                ExtraSettings = new MerchantCreateExtraSettingModel
                {
                    CopyGameSettingFromMerchantID = _TEST_DATA.Default.MerchantID,
                }
            });

            return;
        }


        [TestMethod]
        public void GetMerchantStatusAndHierarchyPreviewInfo_Test()
        {
            int targetMerchantID = _TEST_DATA.CreditAgent.MerchantID;
            var query = MerchantService.GetMerchantStatusAndHierarchyPreviewInfo(_TEST_DATA.Default.UserID, targetMerchantID);
            Assert.IsTrue(query != null);
            Assert.IsTrue(targetMerchantID == query.MerchantID);
            return;
        }



        [TestMethod]
        public void ListAllMerchantCustomizedSettings_Test()
        {
            var queryExist = MerchantService.ListAllMerchantCustomizedSettings(_TEST_DATA.Default.UserID, new MerchantAdvanceListRequestModel
            {
                SettingValueMatch = "*target",
                MerchantSettingKeys = new List<int>
                {
                    MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName.ToInteger(),
                }
            });

            Assert_NotEmptyCollection(queryExist.ExtraHeaders);
            Assert_NotEmptyCollection(queryExist.Items);
            return;
        }


        [TestMethod]
        public void Update_Test()
        {
            var queryExist = MerchantService.GetByID(_TEST_DATA.NotSuperMerchant.MerchantID);

            Assert.IsNotNull(queryExist);

            var updateModel = queryExist.ReturnInjecteObject<MerchantUpdateRequestModel>();
            MerchantService.Update(_TEST_DATA.Default.UserID, updateModel);
            return;
        }


        [TestMethod]
        public void CompareTwoMerchants_Test()
        {
            int userID = _TEST_DATA.Default.UserID;

            //要允許來源與目標相同
            var query1 = MerchantService.CompareTwoMerchants(userID, new MerchantCompareRequestModel
            {
                LeftMerchantID = _TEST_DATA.Default.MerchantID,
                RightMerchantID = _TEST_DATA.Default.MerchantID,
            });

            var query2 = MerchantService.CompareTwoMerchants(userID , new MerchantCompareRequestModel
            {
                LeftMerchantID = _TEST_DATA.Default.MerchantID,
                RightMerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
            });

            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);

            Assert.IsNotNull(query1.LeftMerchant);
            Assert.IsNotNull(query1.RightMerchant);
            Assert.IsNotNull(query2.LeftMerchant);
            Assert.IsNotNull(query2.RightMerchant);
            return;
        }




        [TestMethod]
        public void GetAvailableMerchantCustomizedSettingsWhenCreateMerchant()
        {
            var query = MerchantCustomizedSettingService.GetAvailableMerchantCustomizedSettingsWhenCreateMerchant(_TEST_DATA.Default.UserID);
            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void GetMerchantAdvanceSettings_Test()
        {
            var query = MerchantCustomizedSettingService.GetMerchantAdvanceSettings(_TEST_DATA.Default.UserID, _TEST_DATA.NotSuperMerchant.MerchantID);

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Groups);
            return;
        }


        [TestMethod]
        public void GetMerchantAdvanceSettingsBySpecificKey_Test()
        {
            int targetSettingKey = MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant.ToInteger();

            var query = MerchantCustomizedSettingService.GetMerchantAdvanceSettingsBySpecificKey(
                _TEST_DATA.Default.UserID,
                _TEST_DATA.NotSuperMerchant.MerchantID,
                new MerchantCustomizedSettingQueryParameter
                {
                    SpecificKey = targetSettingKey
                });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Groups);

            var queryAllSettings = query.Groups.SelectMany(z => z.Items).ToList();

            Assert.IsTrue(queryAllSettings.Count() == 1);
            Assert.IsTrue(queryAllSettings.FirstOrDefault().SettingKey == targetSettingKey);
            return;
        }


        [TestMethod]
        public void UpdateSettings_Test()
        {
            int targetSettingKey = MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName.ToInteger();
            int merchantID = _TEST_DATA.NotSuperMerchant.MerchantID;
            var testData = new List<SystemConfigSettingUpdateRequestItemModel>
                {
                    new SystemConfigSettingUpdateRequestItemModel
                    {
                        SettingKey = targetSettingKey,
                        Value = "target"
                    }
                };

            MerchantCustomizedSettingService.UpdateSettings(_TEST_DATA.Default.UserID, new SystemConfigSettingUpdateRequestModel
            {
                MerchantID = merchantID,
                Items = testData
            });

            return;
        }



        [TestMethod]
        public void UpdateSettings_NoneSuperAdmin_Test()
        {
            int userID = _TEST_DATA.NotSuperMerchant.UserID;
            int merchantID = _TEST_DATA.NotSuperMerchant.MerchantID;
            int targetSettingKey = MerchantCustomizedSettingKeyEnum.ImportantSetting_ForbiddenAllEvent.ToInteger();

            var settingData = new List<SystemConfigSettingUpdateRequestItemModel>
                {
                    new SystemConfigSettingUpdateRequestItemModel
                    {
                        SettingKey = targetSettingKey,
                        Value = "1"
                    }
                };

            //預期非管理員不能編輯大部分的settings
            AssertExpectExceptionProcess(() =>
            {
                MerchantCustomizedSettingService.UpdateSettings(userID, new SystemConfigSettingUpdateRequestModel
                {
                    MerchantID = merchantID,
                    Items = settingData
                });

            }, ApiErrorCode.BusinessLogicDeny);
            
            return;
        }



        [TestMethod]
        public void UpdateSettings_MerchantIdEmpty_Test()
        {
            int targetSettingKey = MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant.ToInteger();
            int merchantID = _TEST_DATA.Default.MerchantID;
            var testData = new List<SystemConfigSettingUpdateRequestItemModel>
                {
                    new SystemConfigSettingUpdateRequestItemModel
                    {
                        SettingKey = targetSettingKey,
                        Value = "1"
                    }
                };

            AssertExpectExceptionProcess(() =>
            {
                MerchantCustomizedSettingService.UpdateSettings(_TEST_DATA.Default.UserID, new SystemConfigSettingUpdateRequestModel
                {
                    MerchantID = 0,
                    Items = testData
                });

            }, ApiErrorCode.InvalidParameter_NumberEmptyZero,
               ApiErrorCode.InvalidParameter_PleaseSelectMerchantID);

            return;
        }


        [TestMethod]
        public void GetCopyPreviewInfo_Test()
        {
            int targetMerchantID = _TEST_DATA.NotSuperMerchant.MerchantID;
            var query = MerchantService.GetCopyPreviewInfo(_TEST_DATA.Default.UserID, targetMerchantID);
            Assert.IsNotNull(query);
            Assert.IsNotNull(example.invalid);
            Assert.IsTrue(example.invalid.MerchantID == targetMerchantID);
            return;
        }


        [TestMethod]
        public void BatchUpdateMerchantAdvanceSettings_Test()
        {
            MerchantCustomizedSettingService.BatchUpdateMerchantAdvanceSettings(_TEST_DATA.Default.UserID, new MerchantBatchUpdateSettingRequestModel
            {
                MerchantIDsRawText = $"{_TEST_DATA.Default.MerchantID}",
                SettingKey = MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName.ToInteger(),
                SettingValue = "PGS大廳_" + DateTimeHelper.GetCurrentTimeTimestamp()
            });

            return;
        }


        [TestMethod]
        public void UpdateStatusByAgent_Test()
        {
            MerchantService.UpdateStatusByAgent(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                MerchantID = _TEST_DATA.NotSuperMerchant.MerchantID,
                Status = NormalActiveStatus.Enable.ToInteger()
            });

            try
            {
                //預期要檔權限不合法
                MerchantService.UpdateStatusByAgent(_TEST_DATA.NotSuperMerchant.UserID, new LazyCommonRequestModel
                {
                    MerchantID = _TEST_DATA.Default.MerchantID,
                    Status = NormalActiveStatus.Enable.ToInteger()
                });
            }
            catch (Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
            }

            return;
        }


        [TestMethod]
        public void DoExtraOperateForMerchant_Test()
        {
            MerchantService.DoExtraOperateForMerchant(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                Type = SpecialActionEnum.Merchant_AddApiKeyProvidedTag.ToInteger()
            });

            return;
        }


    }
}

