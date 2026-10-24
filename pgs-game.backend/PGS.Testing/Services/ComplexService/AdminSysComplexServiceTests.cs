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
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class AdminSysComplexServiceTests2 : BaseTest
    {
        public IAdminSysComplexService ServiceInstance { get; set; }

        public IMerchantService MerchantService { get; set; }

        public IUsersService UsersService { get; set; }

        public UserRepository UserRepository { get; set; }

        public BetRecordAGGLiveRepository BetRecordAGGLiveRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public IPAddressListTemplateRepository IPAddressListTemplateRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }


        public class MockAdminSysComplexService : AdminSysComplexService
        {
            protected override AGGLiveBackOfficeRecordDetailApiResponse GetAGGLiveRecordDetailByCacheOrCallApi(int vendorConfigID, string betRecordNoPGS)
            {
                return new AGGLiveBackOfficeRecordDetailApiResponse
                {
                    status = "",
                    code = 1000,
                    result = new AGGLiveBackOfficeRecordDetailApiResult
                    {
                        round = "XXXXXXXXXXXXX",
                        detail = new AGGLiveBackOfficeRecordDetailApiDetail
                        {
                            bet_info = new AGGLiveBackOfficeRecordDetailApiBetInfo
                            {
                                bet_list = new List<AGGLiveBackOfficeRecordDetailApiBetList>(),
                            },
                            round_info = new List<AGGLiveBackOfficeRecordDetailApiRoundInfo>
                            {
                                new AGGLiveBackOfficeRecordDetailApiRoundInfo
                                {
                                    banker_card = new List<string>
                                    {
                                        "AAAA",
                                        "BBBB",
                                    }
                                }
                            }                          
                            
                        }
                    }
                };
            }
        }

        [TestMethod]
        public void BatchCreateNewMerchantByEachParentAndPrefixName_Test()
        {
            int userID = _TEST_DATA.Default.UserID;
            string myAccount = $"MS_Batch" + DateTimeHelper.GetCurrentTime().ToString("yyMMdd");

            var exist1 = MerchantService.IsMerchantNameExist(myAccount + "BETA", isIncludeDeleteData: true).IsExist;
            var exist2 = MerchantService.IsMerchantNameExist(myAccount + "MYR", isIncludeDeleteData: true).IsExist;

            if (exist1 || exist2)
                return;

            var myParam = new MerchantBatchCopyAndCreateRequestModel
            {
                IMChannelID = _TEST_DATA.Default.IMChannelID,
                WalletMode = MerchantWalletModeEnum.TransferMode.ToInteger(),
                ExtraConfig = new ExtraConfigForCopyMerchant
                {
                    PrefixName = myAccount,
                    MultipleParentMerchantIDs = new List<int>
                    {
                        _TEST_DATA.CreditAgent.MerchantID,
                        _TEST_DATA.CreditAgent.Layer2_MerchantID,
                    }
                }
            };

            var queryAgentMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                Currency = "MYR",
                IsRootAgentOnly = true
            })
            .FirstOrDefault();

            if(queryAgentMerchant != null)
            {
                myParam.ExtraConfig.MultipleParentMerchantIDs.Add(queryAgentMerchant.MerchantID);
            }

            //創建代理
            ServiceInstance.BatchCreateNewMerchantByEachParentAndPrefixName(userID, myParam);


            var createBETA = MerchantService.IsMerchantNameExist(myAccount + "BETA", isIncludeDeleteData: true);
            var createMYR = MerchantService.IsMerchantNameExist(myAccount + "MYR", isIncludeDeleteData: true);

            if(createBETA.IsExist)
            {
                MerchantService.Delete(_TEST_DATA.Default.UserID, createBETA.MerchantID);
            }
            if (createMYR.IsExist)
            {
                MerchantService.Delete(_TEST_DATA.Default.UserID, createMYR.MerchantID);
            }

            return;
        }



        [TestMethod]
        public void AppendIPsToSetting_Test()
        {
            var myService = new AdminSysComplexService();

            //需確認測試資料保持在這幾組IPs (不要有額外不必要的)
            string companyIPs = string.Join(",", SystemConstant.PgsServerWhiteListIPs);
            string myNewIP = "0.0.0.0";
            string myInputIPs = companyIPs + "," + myNewIP;

            var result = myService.AppendIPsToSetting(_TEST_DATA.Default.UserID, _TEST_DATA.CreditAgent.MerchantID, myInputIPs);

            Assert.IsTrue(result.ResultID > 0);
            Assert.IsTrue(result.ResultIPAddresss.Count() == 1);
            Assert.IsTrue(result.ResultIPAddresss.FirstOrDefault() == myNewIP);

            //還原資料
            var queryExist = IPAddressListTemplateRepository.GetById(result.ResultID);
            queryExist.IPAddressList = companyIPs;

            IPAddressListTemplateRepository.Update(queryExist);
            return;
        }


        [TestMethod]
        public void CopyMerchantByAgent_Test()
        {
            string account = "CopyByAgent_V3_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHH();

            var queryExist = MerchantService.IsMerchantNameExist(account , isIncludeDeleteData: true).IsExist;

            if (queryExist)
                return;

            int orgID = _TEST_DATA.Default.OrganizationID;

            var result = ServiceInstance.CopyMerchantByAgent(_TEST_DATA.Default.UserID,  new MerchantCopyByAgentRequestModel
            {
                Account = account,
                Name = account,
                OrganizationID = orgID,
                ConfirmPassword = [REDACTED],
                Password = [REDACTED],
                ParentMerchantID = _TEST_DATA.CreditAgent.MerchantID,
                RoleID = _TEST_DATA.Default.RoleID,
                WhiteListIPAddress = "0"
            });

            if(result.ResultId > 0)
            {
                MerchantService.Delete(_TEST_DATA.Default.UserID, result.ResultId);

                var queryExistUser = UserRepository.GetByFilter(new QueryUserRequestViewModel
                    {
                        OrganizationIDs = orgID.ToListCollection(),
                        Keyword = account
                    })
                    .FirstOrDefault();
                    
                if(queryExistUser != null)
                {
                    UsersService.Delete(queryExistUser.UserID);
                }
            }

            return;
        }


        [TestMethod]
        public void CopyMerchantByAgent_SW_Test()
        {
            string account = "CopyByAgent_SW_" + DateTimeHelper.GetCurrentTimeYYYYMMDDHH();

            var queryExist = MerchantService.IsMerchantNameExist(account , isIncludeDeleteData: true).IsExist;

            if (queryExist)
                return;

            int orgID = _TEST_DATA.Default.OrganizationID;

            var result = ServiceInstance.CopyMerchantByAgent(_TEST_DATA.Default.UserID, new MerchantCopyByAgentRequestModel
            {
                Account = account,
                Name = account,
                OrganizationID = orgID,
                WalletMode = MerchantWalletModeEnum.SingleWalletMode.ToInteger(),
                ConfirmPassword = [REDACTED],
                Password = [REDACTED],
                ParentMerchantID = _TEST_DATA.CreditAgent.MerchantID,
                RoleID = _TEST_DATA.Default.RoleID,
                WhiteListIPAddress = "0"
            });

            if (result.ResultId > 0)
            {
                MerchantService.Delete(_TEST_DATA.Default.UserID, result.ResultId); 
                
                var queryExistUser = UserRepository.GetByFilter(new QueryUserRequestViewModel
                    {
                        OrganizationIDs = orgID.ToListCollection(),
                        Keyword = account
                    })
                    .FirstOrDefault();

                if (queryExistUser != null)
                {
                    UsersService.Delete(queryExistUser.UserID);
                }
            }

            return;
        }



        [TestMethod]
        public void ManualSendEventBonus_Test()
        {
            var result = ServiceInstance.ManualSendEventBonus(_TEST_DATA.Default.UserID, new ManualSendEventBonisRequestModel
            {
                EventID = _TEST_DATA.NotSuperMerchant.EventID_RedEnvelope,
                PlayerID = _TEST_DATA.SingleWallet.PlayerID
            });

            Assert.IsTrue(result.SwCallbackTransactionNo.IsNotEmptyTextNotNull());
            return;
        }


        [TestMethod]
        public void ChangeGameUrlDomainByCS_Test()
        {
            OneToManyCustomizedSettingItemRepository.ResetDataForMSTest();
            var result = ServiceInstance.ChangeGameUrlDomainByCS(_TEST_DATA.Default.UserID);
            return;
        }


        [TestMethod]
        public void LaunchGameByAdminSys_Test()
        {
            var url = ServiceInstance.LaunchGameByAdminSys(_TEST_DATA.Default.UserID, new InternalTestBundleRequestModel
            {
                PlayerID = _TEST_DATA.Default.PlayerID,
                DoNotIframe = true,
                GameID = 0,
            });

            Assert.IsTrue(url.IsNotEmptyTextNotNull());
            return;
        }


        [TestMethod]
        public void GetCustmoerServiceUtilityInfo_Test()
        {
            var query = ServiceInstance.GetCustmoerServiceUtilityInfo(_TEST_DATA.Default.UserID);
            Assert.IsNotNull(query);
            return;
        }



        [TestMethod]
        public void GetAGGBetRecordReplayInfoByApi_Test()
        {
            var mockService = new MockAdminSysComplexService();

            var queryRecord = BetRecordAGGLiveRepository.GetPagingByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                PageSize = 1,
                OrderByColumn = "CreateTime",
            });

            if (queryRecord.Items.IsNotEmptyNull())
            {
                var marchFirst = queryRecord.Items.FirstOrDefault();
                var apiResult = mockService.GetBetRecordReplayInfoByGameVendor(new LazyCommonRequestModel
                {
                    Token = [REDACTED]",
                    TransactionNo = marchFirst.BetRecordNoPGS,
                });

                Assert.IsNotNull(apiResult);
                Assert.IsTrue(apiResult.BasicInfo.BetRecordNoPGS == marchFirst.BetRecordNoPGS);
            }

            return;
        }

    }
}

