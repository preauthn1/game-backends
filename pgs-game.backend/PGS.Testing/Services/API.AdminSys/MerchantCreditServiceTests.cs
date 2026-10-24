using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class MerchantCreditServiceTests : BaseTest
    {

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public IMerchantCreditService MerchantCreditService { get; set; }






        [TestMethod]
        public void MerchantCreditService_GetByPagingFilter_AndMerchantPermission_Test()
        {
            var baseServie = new BaseAdminSysService();

            int userID2 = _TEST_DATA.CreditAgent.UserID;
            int userID3 = _TEST_DATA.NotSuperMerchant.UserID;

            
            var query2 = MerchantCreditService.GetByPagingFilter(userID2, new QueryMerchantCreditApiRequestParameterModel
            {
                PageSize = 20,
            });

            var query3 = MerchantCreditService.GetByPagingFilter(userID3, new QueryMerchantCreditApiRequestParameterModel
            {
                PageSize = 20,
            });

           
            var user2 = baseServie.GetCurrentOwnPermissionByCache(userID2);
            var user3 = baseServie.GetCurrentOwnPermissionByCache(userID3);

            if (!user2.IsSuperAdminOrg)
            {
                foreach(var merchant in query2.Items)
                {
                    baseServie.CheckViewMerchantPermission(user2.UserID, merchant.MerchantID);
                }
            }


            if (!user3.IsSuperAdminOrg)
            {
                foreach (var merchant in query3.Items)
                {
                    baseServie.CheckViewMerchantPermission(user3.UserID, merchant.MerchantID);
                }
            }

            return;
        }



        [TestMethod]
        public void MerchantCreditService_GetByPagingFilter_InvalidHacker_Test()
        {
            int userID = _TEST_DATA.CreditAgent.UserID;

            var query = MerchantCreditService.GetByPagingFilter(userID, new QueryMerchantCreditApiRequestParameterModel
            {
                PageSize = 20,
                MerchantID = _TEST_DATA.CreditAgent.MerchantID
            });

            Assert.IsTrue(query.Items.IsNotEmptyNull());

            AssertExpectExceptionProcess(() =>
            {
                var query2 = MerchantCreditService.GetByPagingFilter(userID, new QueryMerchantCreditApiRequestParameterModel
                {
                    PageSize = 20,
                    MerchantID = _TEST_DATA.Default.MerchantID
                });

            }, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            //下級不能查上級
            int userID_Layer2 = _TEST_DATA.CreditAgent.UserID_Layer2;

            AssertExpectExceptionProcess(() =>
            {
                var query3 = MerchantCreditService.GetByPagingFilter(userID_Layer2, new QueryMerchantCreditApiRequestParameterModel
                {
                    PageSize = 20,
                    MerchantID = _TEST_DATA.CreditAgent.MerchantID
                });

            }, ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            return;
        }


        [TestMethod]
        public void MerchantCreditService_GetByPagingFilter_Keyword_Test()
        {
            var query1 = MerchantCreditService.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryMerchantCreditApiRequestParameterModel
            {
                PageSize = 20,
                Keyword = "test"
            });

            var query2 = MerchantCreditService.GetByPagingFilter(_TEST_DATA.Default.UserID, new QueryMerchantCreditApiRequestParameterModel
            {
                PageSize = 20,
                Keyword = "177895 175001 175002 175013 175028"
            });

            return;
        }


        [TestMethod]
        public void MerchantCreditService_SuperAdmin_GetByPagingFilter_Test()
        {
            int userID = _TEST_DATA.Default.UserID;
            var query = MerchantCreditService.GetByPagingFilter(userID, new QueryMerchantCreditApiRequestParameterModel
            {
                Status = NormalActiveStatus.Enable.ToInteger(),
                PageSize = 50,
            });

            Assert_NotEmptyCollection(query.Items);

            if (query.Items.IsNotEmptyNull())
            {
                var matchFisrt = query.Items.FirstOrDefault(x => x.IsCreditModeEnable && !x.IsSingleWallet);
                MerchantCreditService.PreviewCreditUpdate(userID, matchFisrt.MerchantID);
                MerchantCreditService.UpdateCreditAmount(userID, new MerchantCreditUpdateRequestModel
                {
                    Memo = "MSTest",
                    MerchantID = matchFisrt.MerchantID,
                    PlusType = PlusOrMinusEnum.Plus.ToInteger(),
                    UpdateAmount = 100
                });
            }

            return;
        }



        [TestMethod]
        public void MerchantCreditService_CreditAgent_GetByPagingFilter_Test()
        {
            int userID = _TEST_DATA.CreditAgent.UserID;
            int merchantID = _TEST_DATA.CreditAgent.MerchantID;

            var queryMyTreeMerchants = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = merchantID,
            });

            List<int> myMerchantIDs = queryMyTreeMerchants.Select(z => z.MerchantID).ToList();

            var query = MerchantCreditService.GetByPagingFilter(userID, new QueryMerchantCreditApiRequestParameterModel
            {
                PageSize = 30,
                Status = NormalActiveStatus.Enable.ToInteger(),
                WalletMode = MerchantWalletModeEnum.TransferMode.ToInteger(),
            });

            Assert.IsTrue(query.Items.All(z => myMerchantIDs.Contains(z.MerchantID)));

            var targetChild = query.Items.FirstOrDefault(z => z.MerchantID != merchantID && z.IsCreditModeEnable);

            if (targetChild != null)
            {
                //測試增加額度 再扣回
                decimal rndAmount = RandomHelper.GetRandIntegerNumbers(1, 10) * 10;
                MerchantCreditService.UpdateCreditAmount(userID, new MerchantCreditUpdateRequestModel
                {
                    Memo = "MSTest_信用代理",
                    MerchantID = targetChild.MerchantID,
                    PlusType = PlusOrMinusEnum.Plus.ToInteger(),
                    UpdateAmount = rndAmount
                });

                MerchantCreditService.UpdateCreditAmount(userID, new MerchantCreditUpdateRequestModel
                {
                    Memo = "MSTest_信用代理",
                    MerchantID = targetChild.MerchantID,
                    PlusType = PlusOrMinusEnum.Minus.ToInteger(),
                    UpdateAmount = rndAmount
                });
            }

            return;
        }



        [TestMethod]
        public void MerchantCreditService_GetMerchantCreditExportColumns_Test()
        {
            int userID = _TEST_DATA.CreditAgent.UserID;
            int parentMerchantID = _TEST_DATA.CreditAgent.MerchantID;

            var query = MerchantCreditService.GetMerchantCreditExportColumns(userID, new QueryMerchantCreditApiRequestParameterModel
            {
                ParentMerchantID = parentMerchantID,
                IsIncludeDeleteData = true
            });

            return;
        }


        [TestMethod]
        public void UpdateSelfCreditIsRejected_GetByPagingFilter_Test()
        {
            int userID = _TEST_DATA.CreditAgent.UserID;
            int merchantID = _TEST_DATA.CreditAgent.MerchantID;

            try
            {
                MerchantCreditService.UpdateCreditAmount(userID, new MerchantCreditUpdateRequestModel
                {
                    MerchantID = merchantID,
                    PlusType = PlusOrMinusEnum.Plus.ToInteger(),
                    UpdateAmount = 100
                });

                Assert.Fail("驗證不合預期");
            }
            catch (Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.BusinessLogicDeny_UpdateSelfCreditIsRejected);
            }

            return;
        }


        [TestMethod]
        public void MerchantCreditService_GetMyCredit_Test()
        {
            var q1 = MerchantCreditService.GetMyCredit(_TEST_DATA.Default.UserID);
            var q2 = MerchantCreditService.GetMyCredit(_TEST_DATA.CreditAgent.UserID);
            var q3 = MerchantCreditService.GetMyCredit(_TEST_DATA.NotSuperMerchant.UserID);

            Assert.IsTrue(q1.IsUnlimit);
            Assert.IsTrue(q2 != null);
            Assert.IsTrue(q3 != null);

            return;
        }



    }
}

