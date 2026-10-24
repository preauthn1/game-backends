using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using Lck.Utility.Extensions.Helper;
using target.Models.Attributes;
using target.Models.ViewModels.API.AdminSys;
using target.Utility;
using System.Transactions;
using target.Models.CultureText;

namespace target.BL.Services
{

    public class MerchantCreditService : BaseAdminSysService, IMerchantCreditService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantCreditRepository MerchantCreditRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public UserRepository UserRepository { get; set; }

        public UserMerchantMappingRepository UserMerchantMappingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }



        public PagedList<MerchantCreditViewModel> GetByPagingFilter(int userID, QueryMerchantCreditApiRequestParameterModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (filter.MerchantID.HasValue)
            {
                CheckViewMerchantPermission(userID, filter.MerchantID.Value);
                filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            }
            else
            {
                if (!currentUser.IsSuperAdminOrg)
                {
                    //特殊邏輯 dev-swAdmin 這個帳號可以瀏覽的商戶 是透過SP定期處理的
                    //特殊邏輯 dev-swAdmin 這個帳號可以瀏覽的商戶 是透過SP定期處理的
                    //特殊邏輯 dev-swAdmin 這個帳號可以瀏覽的商戶 是透過SP定期處理的

                    //如果介面有指定篩選
                    if (filter.HierarchyOfTargetMerchant.HasValue)
                    {
                        if (currentUser.IsHierarchyMerchantMode || currentUser.IsMultipleHierarchyMode)
                        {
                            CheckViewMerchantPermission(userID, filter.HierarchyOfTargetMerchant.Value);
                        }
                        else
                        {
                            //請別拿掉這邏輯 (標準勾選商戶的模式。不想讓他瀏覽底下樹狀的商戶..如果開放了 他們又會反映那其他頁怎麼不能)
                            throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
                        }
                    }
                    else
                    {
                        //代理只能查自己所屬的樹狀
                        filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
                    }
                }
            }

            //允許搜尋多重 merchantIDs (僅管理員適用)
            if (currentUser.IsSuperAdminOrg)
            {
                if (filter.Keyword.IsNotEmptyTextNotNull() && filter.Keyword.Length > 10)
                {
                    List<int> ids = AppHelper.AnalyzeIDsFromRawText(filter.Keyword);

                    if (ids.IsNotEmptyNull() && ids.Count() >= 2)
                    {
                        filter.MerchantIDs = ids;
                        filter.Keyword = string.Empty;
                    }
                }
            }

            if (filter.IMChannelMappingID.HasValue)
            {
                filter.MerchantCustomizedSetting_SettingKey = MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger();
                filter.MerchantCustomizedSetting_SettingValue = filter.IMChannelMappingID.Value.ToString();
            }

            filter.OrderByColumn = ""; //直接強制覆蓋前端傳的 (*2024/09/18 臨時移除)
            var query = MerchantRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<MerchantEntity, MerchantCreditViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<MerchantCreditViewModel>();
                    convert.IsSelfCredit = x.MerchantID == currentUser.ReferenceCreditMerchant;
                    convert.IsProductionEnvType = x.Type == MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger();
                    convert.IsSingleWallet = CommonLogicHelper.IsSingleWalletMode(x.WalletMode);
                    convert.MerchantType = x.Type;

                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items, filter);

            return viewModels;
        }

        private PreQueryDocumentUrlGlobalVariableModel PreQueryUrlSettingForGlobalVariable()
        {
            var externalUrl_dev = SystemConstant.DocumentUrl_ExternalAPI_VendorLaunchText_Dev;
            var externalUrl_pro = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.DocumentUrl_ExternalAPI_VendorLaunchTest);
            var vendorUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.DocumentUrl_VendorAPIUrl);
            var vendorUrlUAT = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.DocumentUrl_VendorAPIUrl_UAT);
            var BOPortalUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.DocumentUrl_BOPortalUrl);

            return new PreQueryDocumentUrlGlobalVariableModel
            {
                ExternalAPI_VendorLaunchTest_Dev = externalUrl_dev + "/vendor-test/launch/{0}/{1}",
                ExternalAPI_VendorLaunchTest_Pro = externalUrl_pro.SettingValue + "/vendor-test/launch/{0}/{1}",
                DocumentUrl_BOPortalUrl = BOPortalUrl.SettingValue,
                DocumentUrl_VendorAPIUrl = vendorUrl.SettingValue,
                DocumentUrl_VendorAPIUrl_UAT = vendorUrlUAT.SettingValue,
            };
        }


        public List<MerchantCreditExportViewModel> GetMerchantCreditExportColumns(int userID, QueryMerchantCreditApiRequestParameterModel filter)
        {
            filter = filter ?? new QueryMerchantCreditApiRequestParameterModel();
            filter.PageSize = 10000;
            filter.PageIndex = 1;
            filter.IsExportExcel = true;

            var myLang = HeaderValueHelper.GetCurrentLanguage();

            var queryMerchants = GetByPagingFilter(userID, filter).Items;

            if (queryMerchants.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundMerchant);
            }

            List<int> singleWalletMerchantIDs = queryMerchants.Where(x => x.IsSingleWallet).Select(z => z.MerchantID).ToList();

            var swMerchantSettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = singleWalletMerchantIDs,
                CustomizedSettingKeyIDs = new List<int>
                    {
                        MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain.ToInteger(),
                        MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken.ToInteger()
                    }
            });

            if (filter.IsIncludeParentMerchant)
            {
                var myFilter = filter.ReturnInjecteObject<QueryMerchantCreditApiRequestParameterModel>();
                myFilter.MerchantID = filter.ParentMerchantID;
                myFilter.ParentMerchantID = null;

                var queryParent = GetByPagingFilter(userID, myFilter);

                if (queryParent.Items.IsNotEmptyNull())
                {
                    queryMerchants.AddRange(queryParent.Items);
                }
            }

            List<int> merchantIDs = queryMerchants.Select(x => x.MerchantID).ToList();

            if (merchantIDs.Count() > 1000)
            {
                throw new TerproException(ApiErrorCode.Forbidden_DataTooMuchPleaseTryReduce);
            }

            var queryUsers = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                ReferenceCreditMerchantIDs = merchantIDs,
            });

            if (queryUsers.IsEmptyOrNull() && merchantIDs.Count() == 1)
            {
                queryUsers = UserRepository.GetByFilter(new QueryUserRequestViewModel
                {
                    MerchantIDsMatchInAssignMerchantOrCreditMerchant = merchantIDs
                });
            }

            //如果creditMode沒有匹配帳號，會嘗試用另一種方式...
            var queryUsersByAssigns = UserMerchantMappingRepository.GetByFilter(new QueryUserMerchantMappingRequestViewModel
            {
                MerchantIDs = merchantIDs
            });

            List<UserEntity> queryUsers_noneReferMerchant = new List<UserEntity>();

            //單筆匯出不會查這種資訊
            queryUsers_noneReferMerchant = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                OrganizationType = OrganizationTypeEnum.Normal.ToInteger(),                
                UserIDs = queryUsersByAssigns.Select(x => x.UserID).Distinct().ToList(),
                AccountFuzzyQuery = "Admin" //只接受有這種關鍵字的帳號
            });

            var viewModels = new List<MerchantCreditExportViewModel>();

            string passwordSelfPasswordText = "⚠️" + MultiLanguageTextHelper.GetCultureTextByFieldName("Common_LabelColumn_userSelfDefinePassword");

            PreQueryDocumentUrlGlobalVariableModel preQuerySettings = PreQueryUrlSettingForGlobalVariable();

            foreach (var merchant in queryMerchants)
            {
                string merchantIDText = merchant.MerchantID.ToString();

                bool isProductionMerchant = merchant.MerchantType == MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger();

                //dev帳號自動格式 會包含merchantID
                var matchUsers = queryUsers
                    .Where(x => x.ReferenceCreditMerchant == merchant.MerchantID || x.UserName.Contains(merchantIDText))
                    .ToList();

                if (matchUsers.IsEmptyOrNull())
                {
                    var queryUsersByAssignMode = queryUsersByAssigns.Where(x => x.MerchantID == merchant.MerchantID).ToList();

                    if (queryUsersByAssignMode.IsEmptyOrNull())
                    {
                        matchUsers = InstanceEmptyAccountModel(merchant.MerchantID);
                    }
                    else
                    {
                        if (queryUsersByAssignMode.Count() == 1)
                        {
                            var myUserID = queryUsersByAssignMode.FirstOrDefault().UserID;

                            var matchFirst = queryUsers_noneReferMerchant.FirstOrDefault(x => x.UserID == myUserID);

                            if (matchFirst != null)
                            {
                                matchUsers = new List<UserEntity>
                                {
                                    new UserEntity
                                    {
                                        ReferenceCreditMerchant = merchant.MerchantID,
                                        UserName = matchFirst.UserName,
                                        Password =  [REDACTED]
                                    }
                                };
                            }
                            else
                            {
                                matchUsers = InstanceEmptyAccountModel(merchant.MerchantID);
                            }
                        }
                        else
                        {
                            matchUsers = InstanceEmptyAccountModel(merchant.MerchantID);
                        }
                    }
                }

                string warningText = "(*⚠️" + MultiLanguageTextHelper.GetCultureTextByFieldName("Common_HintMsg_EndpointUrlNotSetup") + ")";

                foreach (var queryEachUser in matchUsers)
                {
                    var parseModel = merchant.ReturnInjecteObject<MerchantCreditExportViewModel>();
                    parseModel.BackOfficeUrl = preQuerySettings.DocumentUrl_BOPortalUrl;
                    parseModel.ApiUrl = MerchantApiKeyCopyHelper.GeneratePgsVendorAPIPath(preQuerySettings, isProductionMerchant, "/v1");
                    parseModel.UserAccount = queryEachUser.UserName;

                    parseModel.UserPassword = [REDACTED], queryEachUser.Password, merchant.MerchantID);

                    if(string.IsNullOrWhiteSpace(parseModel.UserPassword))
                    {
                        parseModel.UserPassword = [REDACTED];
                    }

                    if (merchant.IsSingleWallet)
                    {
                        var mySettings = swMerchantSettings.Where(x => x.MerchantID == merchant.MerchantID).ToList();

                        var endpointUrl = mySettings
                            .Where(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain)
                            .FirstOrDefault()?
                            .SettingValue;

                        if (string.IsNullOrWhiteSpace(endpointUrl))
                        {
                            parseModel.Memo = warningText;
                            parseModel.IsEndpointNotSetup = true;
                        }

                        parseModel.SingleWalletPgsIdentity = mySettings
                            .Where(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken)
                            .FirstOrDefault()?
                            .SettingValue;
                    }

                    parseModel.VendorCode = SettingHelper.SeamlessAPIVendorCode;


                    if (!isProductionMerchant)
                    {
                        parseModel.Memo_ThisIsDevKey = "⚠️" + MerchantApiKeyCopyHelper.GenearteMessages_ThisIsForDevOrProduction(myLang, isProductionMerchant);

                        parseModel.Memo_DevBETACoin = string.Join(" ", MerchantApiKeyCopyHelper.GenearteMessages_BetaCoinHint(myLang));

                        parseModel.Memo_WhereToDownloadDocument = string.Join(" ", MerchantApiKeyCopyHelper.GenearteMessages_WhereToDownloadDocument(myLang));
                    }

                    viewModels.Add(parseModel);
                }
            }

            viewModels = viewModels
                .OrderBy(x => x.MerchantID == filter.ParentMerchantID ? 0 : 999)
                .ToList();

            int singleMerchantID_or_Zero = merchantIDs.Count() == 1 ? merchantIDs.FirstOrDefault() : 0;

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.Agent_ExportMerchantCredential, singleMerchantID_or_Zero);
            log.DebugInfo = string.Join(",", merchantIDs);

            bool isNotify = merchantIDs.Count() > 10;

            WriteOperateLog(log, isNotify);

            return viewModels;
        }


        private List<UserEntity> InstanceEmptyAccountModel(int merchantID)
        {
            return new List<UserEntity>
                        {
                            new UserEntity
                            {
                                ReferenceCreditMerchant = merchantID,
                                UserName = "尚未建立帐号(Account not created)",
                                Password = [REDACTED] not created)"
                            }
                        };
        }



        public MerchantCreditEazyCheckModel GetMyCredit(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (currentUser.IsSuperAdminOrg)
            {
                return new MerchantCreditEazyCheckModel
                {
                    IsSuperAdmin = true,
                    IsUnlimit = true,
                    CreditAmount = SystemConstant.UnlimitCreditAmount
                };
            }

            if (currentUser.IsMultipleHierarchyMode)
            {
                return new MerchantCreditEazyCheckModel
                {
                    IsSuperAdmin = false,
                    IsUnlimit = true,
                    CreditAmount = SystemConstant.UnlimitCreditAmount
                };
            }

            if (!currentUser.IsHierarchyMerchantMode)
            {
                return new MerchantCreditEazyCheckModel
                {
                    IsSuperAdmin = false,
                    CreditAmount = -1,
                    IsUnlimit = false,
                };
            }

            var queryMyMerchantCredit = MerchantCreditRepository.GetById(currentUser.ReferenceCreditMerchant);

            if (queryMyMerchantCredit == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant, new ErrorDetailModel($"MerchantCredit {currentUser.ReferenceCreditMerchant}"));

            return new MerchantCreditEazyCheckModel
            {
                IsSuperAdmin = false,
                IsUnlimit = queryMyMerchantCredit.IsUnlimit,
                CreditAmount = queryMyMerchantCredit.CreditAmount
            };
        }


        public MerchantCreditUpdatePreviewViewModel PreviewCreditUpdate(int userID, int targetMerchantID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var viewModel = new MerchantCreditUpdatePreviewViewModel();

            var merchantCheckResult = CheckMyMerchantAndTargetMerchantIsAvailableToCreditAdjust(userID, targetMerchantID);

            var queryTargerMerchant = merchantCheckResult.TargetMerchant;

            if (currentUser.IsMultipleHierarchyMode)
            {
                viewModel.MyCredit = new MerchantCreditPreviewItemModel
                {
                    MerchantID = 0,
                    MerchantName = "超級管理組織",
                    IsSuperAdmin = false,
                    IsMultipleHierarchy = true,
                    IsUnlimit = true,
                    CreditAmount = SystemConstant.UnlimitCreditAmount
                };
            }
            else if (!currentUser.IsSuperAdminOrg)
            {
                var queryMyMerchant = merchantCheckResult.MyMerchant;
                var queryMyMerchantCredit = MerchantCreditRepository.GetByIdOrThrowException(queryMyMerchant.MerchantID);

                viewModel.MyCredit = new MerchantCreditPreviewItemModel
                {
                    MerchantID = queryMyMerchantCredit.MerchantID,
                    MerchantName = queryMyMerchant.Name,
                    IsSuperAdmin = false,
                    IsUnlimit = queryMyMerchantCredit.IsUnlimit,
                    CreditAmount = queryMyMerchantCredit.CreditAmount
                };
            }
            else
            {
                viewModel.MyCredit = new MerchantCreditPreviewItemModel
                {
                    MerchantID = 0,
                    MerchantName = "超級管理組織",
                    IsSuperAdmin = true,
                    IsUnlimit = true,
                    CreditAmount = SystemConstant.UnlimitCreditAmount
                };
            }

            var queryTargetCredit = MerchantCreditRepository.GetById(targetMerchantID);

            //如果不存在 新增一筆
            if (queryTargetCredit == null)
            {
                queryTargetCredit = new MerchantCreditEntity
                {
                    CreditAmount = 0,
                    IsUnlimit = false,
                    MerchantID = targetMerchantID
                };

                MerchantCreditRepository.Create(queryTargetCredit);
            }

            viewModel.TargetMerchantCredit = new MerchantCreditPreviewItemModel
            {
                MerchantID = queryTargetCredit.MerchantID,
                MerchantName = queryTargerMerchant.Name,
                Currency = queryTargerMerchant.Currency,
                IsSuperAdmin = false,
                IsUnlimit = queryTargetCredit.IsUnlimit,
                CreditAmount = queryTargetCredit.CreditAmount
            };

            return viewModel;
        }


        public void UpdateCreditAmount(int userID, MerchantCreditUpdateRequestModel model)
        {
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.UpdateAmount).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PlusType).Rule_NotZero();

            model.UpdateAmount = model.UpdateAmount.ToAbs();

            decimal updateAmountOfChild = model.UpdateAmount;
            int targetMerchantID = model.MerchantID;
            int parentMerchantID = 0;

            CheckViewMerchantPermission(userID, targetMerchantID);

            if (model.PlusType == PlusOrMinusEnum.Minus.ToInteger())
                updateAmountOfChild = model.UpdateAmount * -1;

            decimal updateAmountOfParent = updateAmountOfChild * -1;

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var merchantCheckResult = CheckMyMerchantAndTargetMerchantIsAvailableToCreditAdjust(userID, targetMerchantID);

            var queryCredit = MerchantCreditRepository.GetByIdOrThrowException(targetMerchantID);

            if (queryCredit.IsUnlimit)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);
            }

            if (queryCredit.CreditAmount + updateAmountOfChild < 0)
            {
                throw new TerproException(ApiErrorCode.WalletTranferFail_MerchantCreditAmountNotEnough);
            }

            //超級管理員 或 多重代理 可以無視餘額操作
            bool isUnlimiSuperTopup = currentUser.IsSuperAdminOrg || currentUser.IsMultipleHierarchyMode;

            //檢查上級代理的餘額
            if (!isUnlimiSuperTopup)
            {
                parentMerchantID = merchantCheckResult.MyMerchant.MerchantID;

                var queryParentCredit = MerchantCreditRepository.GetByIdOrThrowException(parentMerchantID);

                if (queryParentCredit.CreditAmount + updateAmountOfParent < 0)
                {
                    throw new TerproException(ApiErrorCode.WalletTranferFail_MerchantCreditAmountNotEnough);
                }
            }

            var type = MerchantCreditChangeTypeEnum.Agent_UpdateCredit;

            if (currentUser.IsSuperAdminOrg)
            {
                type = MerchantCreditChangeTypeEnum.SuperAdmin_UpdateCredit;
            }

            model.Memo = model.Memo.Truncate(50);

            string referebceOrderNo = RandomHelper.GetOrderNo("CR");


            if (!isUnlimiSuperTopup)
            {
                //扣除/補回 自己的額度
                MerchantCreditRepository.UpdateCreditAmount(new MerchantCreditUpdateSPParameter
                {
                    MerchantID = parentMerchantID,
                    UpdateAmount = updateAmountOfParent,
                    IpAddress = IPHelper.GetIP(),
                    Memo = model.Memo,
                    Operator = currentUser.UserName,
                    Type = type.ToInteger(),
                    ReferenceOrderNo = referebceOrderNo,
                });
            }

            //更新別人的額度
            MerchantCreditRepository.UpdateCreditAmount(new MerchantCreditUpdateSPParameter
            {
                MerchantID = targetMerchantID,
                UpdateAmount = updateAmountOfChild,
                IpAddress = IPHelper.GetIP(),
                Memo = model.Memo.Truncate(50),
                Operator = currentUser.UserName,
                Type = type.ToInteger(),
                FromMerchantID = currentUser.ReferenceCreditMerchant,
                ReferenceOrderNo = referebceOrderNo,
            });


            //寫異動紀錄
            var instance = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateMerchantCreditAmount, targetMerchantID);
            instance.Memo = $"參考单号: {referebceOrderNo} (额度异动纪录)";

            WriteOperateLog(instance);
        }

        /// <summary>
        /// 檢查通用的檢查 是否可以操作信用額度轉帳
        /// </summary>
        private (MerchantEntity MyMerchant, MerchantEntity TargetMerchant) CheckMyMerchantAndTargetMerchantIsAvailableToCreditAdjust(int userID, int targetMerchantID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryTargetMerchant = MerchantRepository.GetByIdOrThrowException(targetMerchantID);

            if (queryTargetMerchant.Status != NormalActiveStatus.Enable.ToInteger())
            {
                throw new TerproException(ApiErrorCode.InvalidStatus, new ErrorDetailModel("Merchant Status"));
            }

            if (CommonLogicHelper.IsSingleWalletMode(queryTargetMerchant.WalletMode))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("Merchant WalletMode"));
            }

            //超級管理員不會有代理錢包的配置
            if (currentUser.IsMultipleHierarchyMode)
            {
                return (null, queryTargetMerchant);
            }
            else if (!currentUser.IsSuperAdminOrg)
            {
                int myMerchantID = currentUser.ReferenceCreditMerchant;

                if (myMerchantID == 0)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_YourAgentNotAssignReferenceCreditMerchant);
                }

                if (targetMerchantID == myMerchantID)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_UpdateSelfCreditIsRejected);
                }

                var queryMyMerchants = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    HierarchyOfTargerMerchant = myMerchantID,
                }, " MerchantID ");

                if (!queryMyMerchants.Any(z => z.MerchantID == targetMerchantID))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
                }

                var queryMyMerchant = MerchantRepository.GetByIdOrThrowException(myMerchantID);

                if (queryMyMerchant.Currency != queryTargetMerchant.Currency)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_CurrencyNotMatch);
                }

                if (queryMyMerchant.Status != NormalActiveStatus.Enable.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("Merchant Status"));
                }

                if (CommonLogicHelper.IsSingleWalletMode(queryMyMerchant.WalletMode))
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("Merchant WalletMode"));
                }

                return (queryMyMerchant, queryTargetMerchant);
            }
            else
            {
                return (null, queryTargetMerchant);
            }
        }


        private void SetExtraProperty(List<MerchantCreditViewModel> items, QueryMerchantCreditApiRequestParameterModel filter)
        {
            List<int> merchantIDs = items.Select(z => z.MerchantID).Distinct().ToList();
            List<int> singleWalletMerchantIDs = items.Where(x => CommonLogicHelper.IsSingleWalletMode(x.WalletMode)).Select(z => z.MerchantID).Distinct().ToList();

            var queryCredit = MerchantCreditRepository.GetByIds(merchantIDs);

            var querySettingsV2 = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantIDs,
                CustomizedSettingKeyIDs = new List<int>
                {
                    MerchantCustomizedSettingKeyEnum.System_IsMerchantCreditMode.ToInteger(),
                    MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer.ToInteger(),
                    MerchantCustomizedSettingKeyEnum.AGGLive_DefaultVipLevel.ToInteger(),
                    MerchantCustomizedSettingKeyEnum.Common_AgentMemo.ToInteger(),
                }
            });

            var queryHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                MerchantIDs = merchantIDs
            });

            var queryIMChannel = IMChannelRepository.GetMerchantIMChannel(merchantIDs.ConvertLargeListToEmpty(), false);

            var queryUsers = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                ReferenceCreditMerchantIDs = merchantIDs
            }, " UserID , ReferenceCreditMerchant ");

            var singleWalletDoaminSettings = MerchantCustomizedSettingRepository.GetMultipleMerchantSettings(singleWalletMerchantIDs, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);

            List<int> parentMerchantID = queryHierarchy.Select(z => z.ParentMerchantID).Distinct().ToList();

            var queryParentMerchants = DbValueCacheHelper.GetMerchants(parentMerchantID);

            var queryEnableGameCount = new List<QueryMerchantCountResultModel>();

            var queryParentEnableGameCount = new List<QueryMerchantCountResultModel>();

            if (filter.PageSize < 100)
            {
                queryEnableGameCount = MerchantGameSettingRepository.GetEachMerchantEnableGameCount(merchantIDs);

                queryParentEnableGameCount = MerchantGameSettingRepository.GetEachMerchantEnableGameCount(parentMerchantID);
            }

            foreach (var item in items)
            {
                item.ExtraTags = item.ExtraTags ?? new List<string>();
                item.ExtraTagsBlackColor = item.ExtraTagsBlackColor ?? new List<string>();

                var mySettings = querySettingsV2.Where(z => z.MerchantID == item.MerchantID).ToList();

                item.WalletModeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantWalletModeEnum)item.WalletMode);
                item.CurrencyText = item.Currency + "｜" + MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>(item.Currency);
                item.ChargeCurrencyText = MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>(item.ChargeCurrency);
                item.MerchantTypeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantTypeEnum)item.MerchantType);
                item.IsCreditModeEnable = false;
                item.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)item.Status);
                item.EnableGamesCount = queryEnableGameCount.FirstOrDefault(x => x.MerchantID == item.MerchantID)?.TotalCount ?? 0;
                item.WhiteListServerIPs = SystemConstant.PgsServerWhiteListIPs;

                if (!filter.IsExportExcel)
                {
                    item.ApiSecretKey = "";
                }

                if (item.Memo.IsNotEmptyTextNotNull() &&
                  (item.Memo.Contains("別名") || item.Memo.Contains("別稱")))
                {
                    item.MemoDisplay = item.Memo;
                }

                if (item.IsSingleWallet)
                {
                    var matchSetting = singleWalletDoaminSettings.FirstOrDefault(x => x.MerchantID == item.MerchantID);
                    item.IsSingleWalletDoaminNotSetup = string.IsNullOrWhiteSpace(matchSetting?.SettingValue);
                }

                var matchCredit = queryCredit.FirstOrDefault(z => z.MerchantID == item.MerchantID);

                if (matchCredit != null)
                {
                    item.CreditAmount = matchCredit.CreditAmount;
                    item.IsUnlimit = matchCredit.IsUnlimit;
                }

                var matchCreditSetting = mySettings.FirstOrDefault(z => z.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.System_IsMerchantCreditMode);

                if (matchCreditSetting != null)
                {
                    item.IsCreditModeEnable = new SystemConfigSettingParseModel(0, matchCreditSetting.SettingValue).SettingValueOfBoolean;
                }

                var vipLevelSetting = mySettings.FirstOrDefault(z => z.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.AGGLive_DefaultVipLevel);

                if (vipLevelSetting != null && vipLevelSetting.SettingValue.IsNotEmptyNull())
                {
                    item.ExtraTags.Add(vipLevelSetting.SettingValue);
                }

                var agentMemo = mySettings.FirstOrDefault(z => z.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_AgentMemo);

                if (agentMemo != null)
                {
                    item.AgentMemo = agentMemo.SettingValue;
                }

                var convertRateSettings = mySettings.FirstOrDefault(z => z.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer);

                if (convertRateSettings != null && convertRateSettings.SettingValue.IsNotEmptyNull())
                {
                    var parseSetting = new SystemConfigSettingParseModel(0, convertRateSettings.SettingValue);

                    if (parseSetting.SettingValueOfDecimal != 1)
                    {
                        item.IsHintTransferConvertRate = true;
                        item.TransferConvertRate = parseSetting.SettingValueOfDecimal;

                        if (parseSetting.SettingValueOfDecimal == 100)
                        {
                            item.ExtraTags.Add("ConvertRate 1:100");
                        }
                        else
                        {
                            item.ExtraTags.Add($"ConvertRate 1:{parseSetting.SettingValueOfDecimal}");
                        }
                    }
                }

                var matchIMChannel = queryIMChannel.FirstOrDefault(x => x.MerchantID == item.MerchantID);

                if ((matchIMChannel?.IMChannelName ?? "").IsNotEmptyTextNotNull())
                {
                    item.ExtraTagsBlackColor.Add(matchIMChannel.IMChannelName);
                }

                var hierarchySettings = queryHierarchy.FirstOrDefault(x => x.MerchantID == item.MerchantID);

                if (hierarchySettings != null)
                {
                    item.ParentMerchantID = hierarchySettings.ParentMerchantID;
                    item.ParentMerchantName = queryParentMerchants.FirstOrDefault(z => z.MerchantID == item.ParentMerchantID)?.Name;

                    var parentGameCount = queryParentEnableGameCount.FirstOrDefault(x => x.MerchantID == item.ParentMerchantID);
                    item.EnableGamesCountOfParent = parentGameCount?.TotalCount ?? 0;

                    item.NotEnableGamesCount = item.EnableGamesCountOfParent - item.EnableGamesCount;

                    if (item.EnableGamesCountOfParent < 0)
                    {
                        item.NotEnableGamesCount = 0;
                    }
                }

                item.NumberOfAccount = queryUsers.Where(x => x.ReferenceCreditMerchant == item.MerchantID).Count();
            }
        }


    }
}
