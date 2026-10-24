using Lck.Utility.Extensions.Helper;
using Org.BouncyCastle.Asn1.Ocsp;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.BL.Services
{

    public class UsersService : BaseAdminSysService, IUsersService
    {
        public OrganizationRepository OrganizationRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public RoleRepository RoleRepository { get; set; }

        public RolePermissionMappingRepository RolePermissionMappingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public UserTokenRecordRepository UserTokenRecordRepository { get; set; }

        public UserRoleMappingRepository UserRoleMappingRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public UserRepository UserRepository { get; set; }

        public UserMerchantMappingRepository UserMerchantMappingRepository { get; set; }

        public UserCustomizedSettingRepository UserCustomizedSettingRepository { get; set; }

        public IAdminSysDropdownService AdminSysDropdownService { get; set; }


      
        public PagedList<UsersViewModel> GetUsersPagingByFilter(int userID, QueryUserApiRequestParameterModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter.OrganizationIDs = filter.OrganizationID.ToIntegerListCollection();
            filter.RoleIDs = filter.RoleID.ToIntegerListCollection();
            filter.ReferenceCreditMerchantIDs = filter.ReferenceCreditMerchantID.ToIntegerListCollection();

            if (!currentUser.IsSuperAdminOrg)
            {
                if (HasTargetPermission(userID, "Special_Account_SearchAccountByMultiMerchantAssignMode"))
                {
                    filter.FilterAvailableUsersByCreditMerchantID_NeedSpecialPermission = currentUser.ReferenceCreditMerchant;
                }
                else if (currentUser.IsHierarchyMerchantMode)
                {
                    filter.FilterReferenceCreditMerchantIDByHierarchyMerchantID = currentUser.ReferenceCreditMerchant;
                }
                else
                {
                    if (HasTargetPermission(userID, "OperatorManagementAccounts_AllowViewAllOrgAccounts"))
                    {
                        filter.OrganizationIDs = currentUser.OrganizationID.ToListCollection();
                    }
                    else
                    {
                        filter.MyAccountsOrCreatedByMySelf = currentUser.UserName;
                    }
                }
            }
            else
            {
                if (IsRestrictPermissionForSuperOrgAccount(userID))
                {
                    filter.OrganizationType = OrganizationTypeEnum.Normal.ToInteger();
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.IMChannelID))
            {
                var querySettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger().ToListCollection(),
                    SettingValueMatch = filter.IMChannelID
                });

                if (querySettings.IsEmptyOrNull())
                {
                    filter.MerchantIDsMatchInAssignMerchantOrCreditMerchant = new List<int> { -1 };
                }
                else
                {
                    filter.MerchantIDsMatchInAssignMerchantOrCreditMerchant = querySettings.Select(z => z.MerchantID).ToList();
                }
            }

            SetSpecialFilterType(userID, filter);

            var query = UserRepository.GetPagingByFilter(filter);

            string defaultPasswordHash = AppHelper.GeneratePasswordHash(SystemConstant.Default_EazyPassword);

            var viewModels = ConvertToNewTypePagingObject<UserEntity, UsersViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels<UsersViewModel>(x);
                    convert.IsDefaultPassword = [REDACTED] == defaultPasswordHash;

                    return convert;
                })
                .ToList();

            SetExtraProperty(userID, viewModels.Items);

            return viewModels;
        }


        private void SetSpecialFilterType(int userID, QueryUserApiRequestParameterModel filter)
        {
            if (!filter.SpecialMerchantFilterType.HasValue)
                return;

            if (filter.SpecialMerchantFilterType == (int)SpecialFilterTypeOfUsers.UseDefaultPassword)
            {
                filter.Password = [REDACTED];
            }

            if (filter.SpecialMerchantFilterType == (int)SpecialFilterTypeOfUsers.LastLogin_30Days)
            {
                filter.LastLoginTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-30);
            }

            if (filter.SpecialMerchantFilterType == (int)SpecialFilterTypeOfUsers.AssignDisableMerchant)
            {
                filter.IsAssignDisableMerchant = true;
            }

            if (filter.SpecialMerchantFilterType == (int)SpecialFilterTypeOfUsers.HasRiskScoreTag)
            {
                filter.UserCustomizedSettingExist = UserCustomizedSettingEnum.RiskScore.ToInteger();
            }
        }



        public UsersUpdatePageViewModel GetUserByID(int userID)
        {
            var queryUser = UserRepository.GetById(userID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            var convert = ConvertToViewModels<UsersUpdatePageViewModel>(queryUser);

            var queryRoleSetting = UserRoleMappingRepository.GetByUserID(userID).FirstOrDefault();
            convert.RoleID = queryRoleSetting?.RoleID ?? 0;

            var queryMerchantMapping = UserMerchantMappingRepository.GetByUserID(userID);
            convert.Merchants = queryMerchantMapping.Select(x => x.MerchantID).ToList();

            return convert;

        }


        public void CreateUserByQuickSetting(int userID, MerchantCopyCreateUserAccountExtraProperty model)
        {
            var insertModel = ConvertToUserCreateModel(model);

            this.Create(userID, insertModel);
        }


        public UserCreateModel ConvertToUserCreateModel(MerchantCopyCreateUserAccountExtraProperty model)
        {
            var queryOrg = OrganizationRepository.GetById(model.OrganizationID);

            if (queryOrg == null)
                throw new TerproException(ApiErrorCode.NotFoundOrganization);

            string account = $"{queryOrg.OrganizationCode}-{model.Account}";

            var insertModel = DataModelConvertHelper.ConvertToUserCreateModel(account, model);

            var queryMerchantHierarchy = MerchantHierarchyRepository.GetById(model.MerchantID);

            bool isHierarchyMerchant = queryMerchantHierarchy != null;

            if (model.IsHierarchyMode && isHierarchyMerchant)
            {
                insertModel.ReferenceCreditMerchant = model.MerchantID;
            }
            else
            {
                insertModel.Merchants = model.MerchantID.ToListCollection();
            }

            return insertModel;
        }





        public bool IsAccountExistInMyOrganization(int userID, string account)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);
            return IsAccountExist(currentUser.OrganizationID, account);
        }

        public void IsAccountExistThenThrowException(int organizationID, string account)
        {
            bool isExist = IsAccountExist(organizationID, account);

            if (isExist)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist, new ErrorDetailModel("Account／UserName"));
        }


        public bool IsAccountExist(int organizationID, string account)
        {
            account = account.Trim();

            var queryUser1 = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                OrganizationIDs = organizationID.ToListCollection(),
                Account = account,
                IsIncludeDelete = true
            });

            string accountWithPrefix = IsContainsOrganizationPrefixCheck(organizationID, account).AccountWithPrefix;

            var queryUser2 = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                OrganizationIDs = organizationID.ToListCollection(),
                Account = accountWithPrefix,
                IsIncludeDelete = true
            });

            return queryUser1.Count() > 0 || queryUser2.Count() > 0;
        }


        public bool IsContainsOrganizationPrefix(int organizationID, string account)
        {
            return IsContainsOrganizationPrefixCheck(organizationID, account).IsContains;
        }

        public UserAccountPrefixCheckResult IsContainsOrganizationPrefixCheck(int organizationID, string account)
        {
            var queryOrg = OrganizationRepository.GetByIdOrThrowException(organizationID);
            string accountPrefix = $"{queryOrg.OrganizationCode}-";

            bool isContains = account.Contains(accountPrefix);
            bool isContainsTwice = account.Contains((accountPrefix + accountPrefix));

            string accountWithPrefix = accountPrefix + account;

            if (isContains)
                accountWithPrefix = account;

            return new UserAccountPrefixCheckResult
            {
                IsContains = isContains,
                IsContainsTwice = isContainsTwice,
                AccountWithPrefix = accountWithPrefix
            };
        }



        public void ValidateBeforeCreateUser(int userID, UserCreateModel model, UserCreateValidateParameter config = null)
        {
            config = config ?? new UserCreateValidateParameter();

            model.UserName = model.UserName.TrimOrNull();

            //在更前面嘗試檢查sql injection
            CheckSqlInjectionAndRiskScore(userID, model);

            model.RuleFor(x => x.OrganizationID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.UserName).Rule_NotEmptyText().Rule_IsValidAccountFormat();
            model.RuleFor(x => x.DisplayName).Rule_NotEmptyText().Rule_MaxLengthOK(maxLength: 50);
            model.RuleFor(x => x.Password).Rule_NotEmptyText().Rule_IsValidPasswordFormat();
            model.RuleFor(x => x.ConfirmPassword).Rule_NotEmptyText();
            model.RuleFor(x => x.RoleID).Rule_NotZeroNotMinus();

            ValidateRoleAndOrgIsValidInput(userID, model.RoleID, model.OrganizationID);

            if (model.AssignMerchantMode <= 0 && model.ReferenceCreditMerchant.NotNullNotZeroNotMinus())
            {
                model.AssignMerchantMode = UserAssignMerchantModeEnum.CreditMerchant.ToInteger();
            }

            if (model.AssignMerchantMode <= 0)
            {
                model.AssignMerchantMode = UserAssignMerchantModeEnum.ViewSelectMerchant.ToInteger();
            }

            if (model.IsOperateFromAgentPage)
            {
                model.RuleFor(x => x.ReferenceCreditMerchant).Rule_IsNotNullNotZeroNotMinus();
            }

            if (!RoleRepository.IsExist(model.RoleID))
                throw new TerproException(ApiErrorCode.NotFoundRole);

            if (model.Password != model.ConfirmPassword)
                throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmPasswordNotEqualPassword);

            var queryOrgs = OrganizationRepository.GetById(model.OrganizationID);

            if (queryOrgs == null)
                throw new TerproException(ApiErrorCode.NotFoundOrganization);

            IsAccountExistThenThrowException(queryOrgs.OrganizationID, model.UserName);

            //檢查是否包含兩次前綴 (e.g. 傳入的帳號填寫為  正常為 MYC-test001 卻填成 MYC-MYC-test001
            var prefixCheckResult = IsContainsOrganizationPrefixCheck(model.OrganizationID, model.UserName);

            if (prefixCheckResult.IsContainsTwice)
                throw new TerproException(ApiErrorCode.InvalidParameter_NotAccountFormat, "输入的帐号无需包含前缀字 (Account should not contains prefix word)");

            if (!config.IsSkipCheckMissPrefix && !prefixCheckResult.IsContains && !model.UserName.Contains("marshall"))
                throw new TerproException(ApiErrorCode.InvalidParameter_NotAccountFormat, "输入的帐号需有組織前缀字 (Account should contains prefix word)");

            if (model.ReferenceCreditMerchant.NotNullNotZeroNotMinus())
            {
                var onwerMerchant = MerchantRepository.GetById(model.ReferenceCreditMerchant.Value);

                if (onwerMerchant.Status != NormalActiveStatus.Enable.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus, new ErrorDetailModel($"Merchant {model.ReferenceCreditMerchant} Status"));
                }
            }

            var query = UserCustomizedSettingRepository.GetParseModelByUniqueKey(userID, UserCustomizedSettingEnum.RiskScore);

            //風險分數超標 禁止操作 (ps. 這個檢查請放在最後面、讓異常操作仍可繼續觸發RiskScore)
            if (query.SettingValueOfDecimal > 5.0M)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_OperatorRiskScore, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                });
            }
        }


        public void ValidateRoleAndOrgIsValidInput(int userID, int roleID, int orgID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                var errorDetail = new ErrorDetailModel
                {
                    LogExtraInfo = $"{IPHelper.GetIP()}",
                    RiskRate = 2.0M,
                    IsAlwaysNotify = true
                };

                //因為客服帳號有些屬於superAdmin 先禁止客服有這種權限
                if (HasCustomerServicePermission(userID))
                {
                    errorDetail.ColumnName = "HasCustomerService";
                    CounterRiskUserAndThrowException(userID, new TerproException(ApiErrorCode.InvalidParameter, errorDetail));
                }

                if (orgID != -5566 && orgID != currentUser.OrganizationID)
                {
                    errorDetail.ColumnName = "Org";
                    CounterRiskUserAndThrowException(userID, new TerproException(ApiErrorCode.InvalidParameter, errorDetail));
                }

                var availableRoles = AdminSysDropdownService.GetAvailableRoles(userID)
                    .Select(x => x.ValueInt)
                    .ToList();

                var hasSuperAdminFunctionToken = [REDACTED] QueryRolePermissionMappingRequestViewModel
                {
                    RoleIDs = roleID.ToListCollection(),
                    IsMatchHighRiskPermission = true,
                });

                //高級權限的帳號只能透過內部人數操作
                if (hasSuperAdminFunctionToken || roleID == SystemConstant.AdminSysAPI_SuperRoleID)
                {
                    errorDetail.ColumnName = "RoleID";
                    CounterRiskUserAndThrowException(userID, new TerproException(ApiErrorCode.InvalidParameter, errorDetail));
                }
                else if (!availableRoles.Contains(roleID))
                {
                    errorDetail.ColumnName = "RoleID";
                    CounterRiskUserAndThrowException(userID, new TerproException(ApiErrorCode.InvalidParameter, errorDetail));
                }
            }
            else
            {
                bool hasSuperAdminFunctionToken = [REDACTED] QueryRolePermissionMappingRequestViewModel
                {
                    RoleIDs = roleID.ToListCollection(),
                    IsMatchHighRiskPermission = true,
                });

                if(hasSuperAdminFunctionToken)
                {
                    string myIP = IPHelper.GetIP();
                    bool isCompanyIP = IPHelper.IsCompanyIP(myIP);

                    if(!isCompanyIP)
                    {
                        throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow_CompanyIP , new ErrorDetailModel(myIP));
                    }
                }
            }


            return;
        }

        public CRUDResultModel Create(int userID, UserCreateModel model)
        {
            model.RuleFor(x => x.OrganizationID).Rule_NotZeroNotMinus();

            var logBody = model.ReturnInjecteObject<UserCreateModel>();
            logBody.Password = [REDACTED]";
            logBody.ConfirmPassword = [REDACTED]";

            HeaderValueHelper.AppendOverridePayaloadForLog(logBody.ConvertToJsonString());

            var queryOrgs = OrganizationRepository.GetById(model.OrganizationID);

            if (model.IsAutoAddOrgPrefix)
            {
                model.UserName = $"{queryOrgs.OrganizationCode}-{model.UserName}";
            }

            //各種防呆、資料邏輯檢查 (包含role設定)
            ValidateBeforeCreateUser(userID, model);

            //這段不能包在 Validate那邊 (商戶ID的assign是另一段額外處理的)
            CheckAssignMerchantIsRootAgent(userID, model);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //從 2026/10 開始 不再使用abc123這種密碼 
            if(currentUser.IsSuperAdminOrg)
            {
                if (model.Password == SystemConstant.Default_EazyPassword ||
                    model.Password == SystemConstant.Default_RequestUseDefaultPassword)
                {
                    model.Password = [REDACTED];
                }
            }

            var newEntity = model.ReturnInjecteObject<UserEntity>();

            newEntity.Password = [REDACTED];
            newEntity.Status = NormalActiveStatus.Enable.ToInteger();
            newEntity.CreateTime = DateTimeHelper.GetCurrentTime();
            newEntity.UpdateTime = DateTimeHelper.GetCurrentTime();
            newEntity.LastLogonTime = DateTimeHelper.GetDefaultTime();
            newEntity.UpdateBy = currentUser.UserName;
            newEntity.CreateBy = currentUser.UserName;
            newEntity.DefaultLang = queryOrgs.Lang;
            newEntity.ReferenceCreditMerchant = model.ReferenceCreditMerchant;
            newEntity.Memo = model.Memo.Truncate(50);
            newEntity.AssignMerchantMode = model.AssignMerchantMode;

            if (AppHelper.IsDefaultPasswordHash(newEntity.Password) ||
                AppHelper.IsAMDefaultPassword(newEntity.UserName, newEntity.Password).IsPasswordMatch)
            {
                newEntity.IsResetPasswordWarning = true;
            }

            if (model.Merchants.IsEmptyOrNull() && model.ReferenceCreditMerchant.HasValue)
            {
                model.Merchants = new List<int> { model.ReferenceCreditMerchant.Value };
            }

            if (newEntity.TimeZoneAreaID == 0)
                newEntity.TimeZoneAreaID = TimeZoneAreaEnum.China_0800.ToInteger();

            //step 1. 新增user資料
            var newUserID = UserRepository.Create(newEntity);

            //step 2. 新增role資料
            UserRoleMappingRepository.Create(new UserRoleMappingEntity
            {
                RoleID = model.RoleID,
                UserID = newUserID,
            });

            //stpe 3. 新增merchantMapping
            UserMerchantMappingRepository.DeleteAndInsertNew(newUserID, model.Merchants);

            //stpe 4. 操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.User_Create, newUserID);
            WriteOperateLog(log);


            //如果針對超級組織建帳號。需要提示下
            if (SettingHelper.IsProduction)
            {
                if (queryOrgs.OrganizationType == OrganizationTypeEnum.SuperAdmin.ToInteger())
                {
                    string msg = $" {currentUser.UserName} 新建後台帳號: `{newEntity.UserName}` 此帳號為超級管理組織。請確認不是建錯資料。";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
                }
                else if (model.RoleID == SystemConstant.AdminSysAPI_SuperRoleID)
                {
                    string msg = $" {currentUser.UserName} 新建後台帳號: `{newEntity.UserName}` 此帳號為超級Role。請確認不是建錯資料。";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
                }
            }

            return new CRUDResultModel
            {
                IsSuccess = true,
                ResultId = newUserID
            };
        }



        public void Update(int userID, UserUpdateModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.DisplayName).Rule_NotEmptyText().Rule_MaxLengthOK(maxLength: 50);
            model.RuleFor(x => x.RoleID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TimeZoneAreaID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.AssignMerchantMode).Rule_NotZeroNotMinus();

            var logBody = model.ReturnInjecteObject<UserUpdateModel>();
            logBody.Password = [REDACTED]";
            logBody.ConfirmPassword = [REDACTED]";

            HeaderValueHelper.AppendOverridePayaloadForLog(logBody.ConvertToJsonString());

            //如果有輸入password才檢查
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                //強制要求用新版API (psasword專用)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("not allow input password"));

                if (model.Password != model.ConfirmPassword)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch);
                }
            }

            CheckViewUserPermission(userID, model.UserID);

            if (!RoleRepository.IsExist(model.RoleID))
                throw new TerproException(ApiErrorCode.NotFoundRole);

            //檢查危險的roleID亂填
            ValidateRoleAndOrgIsValidInput(userID, model.RoleID, -5566);

            var queryExist = UserRepository.GetById(model.UserID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                if (model.RoleID <= 100)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter);
                }
            }

            bool isCreditMerchantMode = model.AssignMerchantMode == (int)UserAssignMerchantModeEnum.CreditMerchant;

            queryExist.DisplayName = model.DisplayName;
            queryExist.Status = model.Status;
            queryExist.UpdateTime = DateTimeHelper.GetCurrentTime();
            queryExist.UpdateBy = currentUser.UserName;
            queryExist.TimeZoneAreaID = model.TimeZoneAreaID;
            queryExist.ReferenceCreditMerchant = model.ReferenceCreditMerchant;
            queryExist.Memo = model.Memo.Truncate(50);
            queryExist.AssignMerchantMode = model.AssignMerchantMode;

            if (!isCreditMerchantMode)
            {
                queryExist.ReferenceCreditMerchant = null;
            }

            if (isCreditMerchantMode)
            {
                model.Merchants = new List<int>();
            }

            CheckAssignMerchantIsRootAgent(userID, model);

            //如果有輸入password才更新
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                queryExist.Password = [REDACTED];
            }

            //step 1. 更新 user資料
            UserRepository.Update(queryExist);

            //step 2. 更新 role資料
            UserRoleMappingRepository.DeleteAndInsertNew(model.UserID, model.RoleID.ToListCollection());

            //stpe 3. 更新 merchantMapping
            UserMerchantMappingRepository.DeleteAndInsertNew(model.UserID, model.Merchants);

            //stpe 4. 操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.User_Update, model.UserID);
            WriteOperateLog(log);

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenUpdateUser(model.UserID);
        }


        public CRUDResultModel CopyByUserID(int userID, UserCopyRequestModel model)
        {
            model.UserName = model.UserName.TrimOrNull();

            model.RuleFor(x => x.SourceUserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.UserName).Rule_NotEmptyText().Rule_IsValidAccountFormat();

            CheckViewUserPermission(userID, model.SourceUserID);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var querySourceUser = UserRepository.GetByIdOrThrowException(model.SourceUserID);

            var targetUserInfo = GetCurrentOwnPermissionByCache(querySourceUser.UserID);

            //超級組織帳號禁止copy、超級管理員禁止Copy
            if (SettingHelper.IsProduction)
            {
                ValidateRoleAndOrgIsValidInput(userID, targetUserInfo.Roles.FirstOrDefault(), targetUserInfo.OrganizationID);
            }

            var querySingleUser = GetUserByID(querySourceUser.UserID);

            var queryOrgs = OrganizationRepository.GetById(querySourceUser.OrganizationID);

            var param = new UserCreateModel
            {
                UserName = $"{queryOrgs.OrganizationCode}-{model.UserName}",
                DisplayName = model.UserName,
                Memo = $"copy source {querySourceUser.UserID}",
                Password = [REDACTED],
                ConfirmPassword = [REDACTED],
                OrganizationID = querySourceUser.OrganizationID,
                RoleID = querySingleUser.RoleID,
                TimeZoneAreaID = querySingleUser.TimeZoneAreaID,
                Merchants = querySingleUser.Merchants,
                ReferenceCreditMerchant = querySingleUser.ReferenceCreditMerchant,
                AssignMerchantMode = querySourceUser.AssignMerchantMode,
            };

            if (param.ReferenceCreditMerchant.HasValue)
            {
                param.Merchants = new List<int>();
            }

            var result = Create(userID, param);

            var logs = InstanceOperateLog(userID, UserOperateTypeEnum.CopyUser, userID);
            WriteOperateLog(logs);

            string msg = $"{currentUser.UserName} 操作了 CopyByUserID , {model.UserName} {model.SourceUserID}";
            SendMessageToMultipleChannel("", TelegramChannelEnum.pgs_adminSys_api);

            return result;
        }


        public RequestDefaultPasswordResponseModel RequestDefaultPassword(int userID , RequestDefaultPasswordRequestModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TS).Rule_IsValidTimestamp();

            int targetUserID = model.UserID;

            CheckViewUserPermission(userID, targetUserID);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryUser = UserRepository.GetById(targetUserID);

            var assignMerchants = UserMerchantMappingRepository.GetByUserID(targetUserID);

            int assignMerchantID = assignMerchants.FirstOrDefault()?.MerchantID ?? 0;

            var displayPassword = [REDACTED], queryUser.Password, assignMerchantID);

            if(string.IsNullOrWhiteSpace(displayPassword))
            {
                displayPassword = [REDACTED] define password (if you don't have password. please request AM reset password.)";
            }

            return new RequestDefaultPasswordResponseModel
            {
                UserPassword = [REDACTED]
            };
        }




        public CRUDResultModel UpdateUserPassword(int targetUserID, UpdateCurrentUserPasswordRequestModel model, int currentUserID)
        {
            model.RuleFor(x => x.OriginalPassword).Rule_NotEmptyText();
            model.RuleFor(x => x.NewPassword).Rule_NotEmptyText();
            model.RuleFor(x => x.NewPassword).Rule_IsValidPasswordFormat();
            model.RuleFor(x => x.ConfirmPassword).Rule_NotEmptyText();

            if (model.NewPassword != model.ConfirmPassword)
                throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmPasswordNotEqualPassword);

            CheckViewUserPermission(currentUserID, targetUserID);

            var currentUser = GetCurrentOwnPermissionByCache(currentUserID);

            var queryUser = UserRepository.GetById(targetUserID);

            var originalPasswordHash1 = AppHelper.GeneratePasswordHash(model.OriginalPassword);
            var originalPasswordHash2 = AppHelper.GeneratePasswordHash(model.OriginalPassword.Trim()); //幫忙去除空白符號

            //如果帶Magic可以跳過檢查
            bool isMagicPassword = [REDACTED] == SystemConstant.PublicAPI_SignKey_MagicPassword;

            if (!isMagicPassword && queryUser.Password != originalPasswordHash1 && queryUser.Password != originalPasswordHash2)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_OriginalPasswordNotCorrect);
            }

            var defaultPasswordHash1 = AppHelper.GeneratePasswordHash(SystemConstant.Default_EazyPassword);

            string newPasswordHash = AppHelper.GeneratePasswordHash(model.NewPassword);

            bool isDefaultPassword = [REDACTED];

            //若為true。才允許使用預設密碼 (前端按鈕[重設為預設密碼])
            if (!model.IsResetDefaultPasswordPolicy)
            {
                if (isDefaultPassword)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_ThePasswordTextIsForbidden);
                }

                //可能之後會改版成其它組預設密碼，舊版預設也會禁止使用這組
                if (model.NewPassword.Contains("abc123"))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_ThePasswordTextIsForbidden);
                }
            }

            if(isDefaultPassword)
            {
                model.NewPassword = [REDACTED];
                newPasswordHash = AppHelper.GeneratePasswordHash(model.NewPassword);
            }

            if (model.IsValidateIMChannel)
            {
                model.RuleFor(x => x.ValidateIMChannel).Rule_IsNotNullNotZeroNotMinus();

                List<int> availableMerchantOfUser = new List<int>();

                //依照這種邏輯、如果是superRoot那種(Multiple hierarcht) 應該就無法透過客服來重製密碼了
                if (queryUser.ReferenceCreditMerchant.HasValue)
                {
                    availableMerchantOfUser = new List<int>
                    {
                        queryUser.ReferenceCreditMerchant.Value
                    };
                }
                else
                {
                    availableMerchantOfUser = UserMerchantMappingRepository.GetByUserID(queryUser.UserID).Select(z => z.MerchantID).ToList();
                }

                var queryAvailableChannels = IMChannelRepository.GetMerchantIMChannel(availableMerchantOfUser);

                bool isChannelMatch = queryAvailableChannels.Any(x => x.IMChannelID_Integer == model.ValidateIMChannel);

                if (!isChannelMatch)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch, new ErrorDetailModel
                    {
                        ColumnName = $"{queryUser.UserName} , IMChannel not match. Please contact tech team.",
                        IsAlwaysNotify = true,
                    });
                }
            }

            string oldPassword = [REDACTED];

            queryUser.Password = [REDACTED];
            queryUser.IsResetPasswordWarning = isDefaultPassword ? true : false; //使用預設密碼會要求強制修改
            queryUser.LastPasswordHash = oldPassword;
            queryUser.LastPasswordModifyTime = DateTimeHelper.GetCurrentTime();

            UserRepository.Update(queryUser);

            var log = InstanceOperateLog(currentUserID, UserOperateTypeEnum.UpdateUserPassword, queryUser.UserID, "userID");
            WriteOperateLog(log);

            bool isUpdateOtherUser = targetUserID != currentUserID;                      

            //順手解除登入Locker (次數過多禁止登入)
            CacheRemoveHelper.UnlockFailLoginLocker();

            //管理員操作 順手強制登出
            if (currentUser.IsSuperAdminOrg && isUpdateOtherUser)
            {
                UserTokenRecordRepository.ForceLogoutAdminSysAccounts(targetUserID);
            }

            return new CRUDResultModel
            {
                ResultExtraString = model.NewPassword
            };
        }

        public void ForceLogout(int currentUserID, int targetUserID)
        {
            CheckViewUserPermission(currentUserID, targetUserID);

            var queryUser = UserRepository.GetById(targetUserID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            UserTokenRecordRepository.ForceLogoutAdminSysAccounts(targetUserID);
        }


        public void ResetReferenceCreditMerchant(int userID, int targetUserID)
        {
            CheckViewUserPermission(userID, targetUserID);

            var queryUser = UserRepository.GetById(targetUserID);

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            queryUser.ReferenceCreditMerchant = null;

            UserRepository.Update(queryUser);
        }


        /// <summary>
        /// 輕量版的更新
        /// </summary>
        public void UpdateUserBriefInfo(int currentUserID, UpdateUserBriefInfoRequestModel model)
        {
            var currentUser = GetCurrentOwnPermissionByCache(currentUserID);

            CheckViewUserPermission(currentUserID, model.UserID);

            model.RuleFor(x => x.Status).Rule_NotMinus();
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();

            var queryUser = UserRepository.GetById(model.UserID);

            string memoText = $"status:{queryUser.Status} to {model.Status}";

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            if (queryUser.Status == (int)NormalActiveStatus.DisableAndForbiddenEdit)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("Status"));

            queryUser.Memo = model.Memo;
            queryUser.Status = model.Status;

            UserRepository.Update(queryUser);

            var log = InstanceOperateLog(currentUserID, UserOperateTypeEnum.User_UpdateUserStatus, queryUser.UserID);
            log.Memo = memoText;

            WriteOperateLog(log);
        }



        public void Delete(int userID)
        {
            var queryExist = UserRepository.GetById(userID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            if (!SettingHelper.IsMSTest)
            {
                queryExist.UserName = queryExist.UserName + $"_DEL_{RandomHelper.GetRandLetter(5)}";
                queryExist.UserName = queryExist.UserName.Truncate(50);
            }

            queryExist.Status = NormalActiveStatus.PermanentDelete.ToInteger();
            UserRepository.Update(queryExist);
        }


        public void CheckSqlInjectionAndRiskScore(int userID, UserCreateModel model)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //超級管理組織的話 不需檢查
            if (currentUser.IsSuperAdminOrg)
                return;

            var cloneBody = model.ReturnInjecteObject<UserCreateModel>();

            cloneBody.Password = [REDACTED];
            cloneBody.ConfirmPassword = [REDACTED];
            cloneBody.UserName = cloneBody.UserName.RemoveStart(15);
            cloneBody.DisplayName = cloneBody.DisplayName.RemoveStart(15);
            cloneBody.Memo = cloneBody.Memo.RemoveStart(15);

            string body = cloneBody.ConvertToJsonString();

            if (RegexRuleHelper.IsMatchRequestBodySqlInjectionKeyword(body))
            {
                CounterRiskUserAndThrowException(userID, new TerproException(ApiErrorCode.InvalidParameter_Body, new ErrorDetailModel
                {
                    ColumnName = "sql error",
                    LogExtraInfo = $" ⚠️此請求有SQL injection風險 {IPHelper.GetIP()}",
                    RiskRate = 2.0M,
                    IsAlwaysNotify = true
                }));
            }

            return;
        }


        private T ConvertToViewModels<T>(UserEntity queryUser) where T : UsersViewModel, new()
        {
            var convert = queryUser.ReturnInjecteObject<T>();

            convert.TimeZoneAreaName = MultiLanguageTextHelper.GetTimeZoneDisplayName(queryUser.TimeZoneAreaID);
            convert.TimeZone = ((TimeZoneAreaEnum)queryUser.TimeZoneAreaID).GetAttributeFromEnum<ExtraAttributeValue>()?.Value;

            if (convert.TimeZone.IsNumeric())
            {
                var toNumber = decimal.Parse(convert.TimeZone);

                if (toNumber.IsIntegerValue())
                {
                    convert.TimeZone = ((int)toNumber).ToString();
                }
            }

            return convert;
        }

        /// <summary>
        /// 避免維運人員亂操作 違反預期的設計
        /// </summary>
        private void CheckAssignMerchantIsRootAgent<T>(int userID, T model) where T : IUserMerchantAssign
        {
            if (model.ReferenceCreditMerchant.HasValue)
            {
                //如果是代理樹狀瀏覽
                CheckViewMerchantPermission(userID, model.ReferenceCreditMerchant.Value);

                //禁止創建總代代理 只能創建三、四級
                if (HasTargetPermission(userID, "Agent_MerchantCredit_DisableCreateLayer2Agent"))
                {
                    var queryAgent = MerchantHierarchyRepository.GetById(model.ReferenceCreditMerchant.Value);

                    //另一邊是查詢parentMerchantID 因此判斷level層級不一樣
                    if (AppHelper.GetAgentLayerLevelByHierarchy(queryAgent.Hierarchy) <= 1)
                    {
                        throw new TerproException(ApiErrorCode.Forbidden_DoNotAllowCreateTargetLayerAgent, new ErrorDetailModel("root level"));
                    }
                }
            }

            if (model.ReferenceCreditMerchant.HasValue)
                return;

            if (model.Merchants.IsEmptyOrNull())
                return;

            var merchantIDs = model.Merchants.ToList();

            foreach (int merchantID in merchantIDs)
            {
                CheckViewMerchantPermission(userID, merchantID);
            }

            var queryHierarchyMerchants = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                MerchantIDs = merchantIDs
            })
            .OrderBy(x => AppHelper.GetAgentLayerLevelByHierarchy(x.Hierarchy))
            .ToList();

            if (model.AssignMerchantMode == UserAssignMerchantModeEnum.ViewSelectMerchant.ToInteger())
            {
                //標準型是勾選什麼就看什麼 因此不限制...? 但使用者會不會誤解勾選super是可以看到super底下
                if (queryHierarchyMerchants.Count() > 1)
                {
                    foreach (var item in queryHierarchyMerchants)
                    {
                        var queryMerchantHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                        {
                            MerchantIDs = item.MerchantID.ToListCollection()
                        })
                            .FirstOrDefault();

                        //如果是Root
                        if (queryMerchantHierarchy != null && queryMerchantHierarchy.ParentMerchantID == 0)
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, $" {item.MerchantID} 您所分配的商戶屬於Root代理商戶，系統禁止透過此方式分配。允許方式為將此帳號設為 [代理樹狀瀏覽] 或 分別開多個帳號。");
                        }
                    }
                }
            }
            else if (model.AssignMerchantMode == UserAssignMerchantModeEnum.MultipleHierarchyMerchant.ToInteger())
            {
                List<int> agentMerchantIDs = queryHierarchyMerchants.Select(x => x.MerchantID).ToList();

                //判斷每個商戶都是代理
                var isAllAgentMerchant = merchantIDs.All(id => agentMerchantIDs.Contains(id));

                if (!isAllAgentMerchant)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"您透過此模式勾選的商戶。必須全部都是代理型商戶");
                }
            }
        }


        private void SetExtraProperty(int myUserID, List<UsersViewModel> items)
        {
            if (items.IsEmptyOrNull())
                return;

            var queryOrgs = DbValueCacheHelper.GetOrgs(items.Select(z => z.OrganizationID));

            List<int> userIDs = items.Select(z => z.UserID).ToList();
            List<int> creditMerchantIDs = items.Where(x => x.ReferenceCreditMerchant.HasValue).Select(x => x.ReferenceCreditMerchant.Value).ToList();

            var queryRolesMappings = UserRoleMappingRepository.GetByFilter(new QueryUserRoleMappingRequestViewModel
            {
                UserIDs = userIDs
            });

            List<int> roleIDs = queryRolesMappings.Select(z => z.RoleID).Distinct().ToList();

            var queryRoles = RoleRepository.GetByIds(roleIDs);
            var queryMerchants = MerchantRepository.GetByIds(creditMerchantIDs);

            var riskScoreByUserID = UserCustomizedSettingRepository.GetByFilter(new QueryUserCustomizedSettingRequestViewModel
            {
                UserIDs = userIDs,
                SettingKeys = UserCustomizedSettingEnum.RiskScore.ToInteger().ToListCollection()
            })
                    .ToDictionary(
                        x => x.UserID,
                        x => new SystemConfigSettingParseModel(x.SettingKey, x.SettingValue).SettingValueOfDecimal);

            List<int> referCreditMerchant = items
                .Where(x => x.ReferenceCreditMerchant.NotNullNotZeroNotMinus())
                .Select(x => x.ReferenceCreditMerchant.Value)
                .ToList();

            var queryIMChannels = IMChannelRepository.GetMerchantIMChannel(referCreditMerchant);

            foreach (var item in items)
            {
                item.OrganizationName = queryOrgs.FirstOrDefault(z => z.OrganizationID == item.OrganizationID)?.OrganizationName;
                item.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)item.Status);
                item.AssignMerchantModeText = MultiLanguageTextHelper.GetEnumsCultureText((UserAssignMerchantModeEnum)item.AssignMerchantMode);

                var myRole = queryRolesMappings.FirstOrDefault(x => x.UserID == item.UserID);

                if (myRole != null)
                {
                    var matchRole = queryRoles.FirstOrDefault(z => z.RoleID == myRole.RoleID);

                    item.RoleID = myRole.RoleID;
                    item.RoleName_CN = matchRole?.RoleName;
                    item.RoleName_EN = matchRole?.RoleName;
                }

                if (item.ReferenceCreditMerchant.HasValue)
                {
                    item.ReferenceCreditMerchantName = queryMerchants.FirstOrDefault(x => x.MerchantID == item.ReferenceCreditMerchant.Value)?.Name;

                    var matchChannel = queryIMChannels.Where(x => x.MerchantID == item.ReferenceCreditMerchant).FirstOrDefault();

                    if (matchChannel != null)
                    {
                        item.IMChannelID = matchChannel.IMChannelID_Integer;
                        item.IMChannelName = matchChannel.IMChannelName;
                    }
                }

                item.IsInternalDevelopAccount = (item.Memo ?? "").Contains("內部技術人員使用");
                item.RiskScore = (int)riskScoreByUserID.GetDictionaryOrDefaultValue(item.UserID, 0);
                item.IsSelfAccount = item.UserID == myUserID;
            }
        }

    }
}
