using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using target.Utility.Extensions;
using target.DA.Repository;
using target.Models.Table;
using target.Utility.Helper;
using target.Models.ViewModels.Base;
using Lck.Utility.Extensions.Helper;
using target.Utility.IOC;
using target.Utility;
using target.Utility.Utility.ThirdParty;
using target.Models;
using target.Utility.Helper.HttpContext;

namespace target.BL.Services
{
    public class BaseAdminSysService : BaseService
    {

        /// <summary>
        /// 寫入後台修改日誌 (其實apiLog可以處理大部分紀錄了)
        /// 但如果是要讓後台可以查的記錄 寫入這張表
        /// </summary>
        protected void WriteOperateLog(int userID, UserOperateTypeEnum type, int targetID, UserOperateChangeLogModel changeLog = null)
        {
            var model = InstanceOperateLog(userID, type, targetID, changeLog);
            WriteOperateLog(model);
        }

        protected void WriteOperateLogAndNotifyCsOperate(int userID, UserOperateRecordEntity entity)
        {
            WriteOperateLog(entity);

            //如果權限受限制的客服帳號
            if (IsRestrictPermissionForSuperOrgAccount(userID))
            {
                //觀察期結束 部分操作可不用通知了
                if (entity.OperateType == (int)UserOperateTypeEnum.UpdateIPAddressTemplate)
                    return;

                var currentUser = GetCurrentOwnPermissionByCache(userID);
                string operateType = ((UserOperateTypeEnum)entity.OperateType).ToString();
                string msg = $"用戶:{currentUser.UserName} ({currentUser.UserID}) 操作了 {operateType}。 資料:{entity.TargetID}、{entity.DebugInfo}";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }
        }


        /// <summary>
        /// 寫入後台修改日誌 (其實apiLog可以處理大部分紀錄了)
        /// 但如果是要讓後台可以查的記錄 寫入這張表
        /// </summary>
        protected void WriteOperateLog(UserOperateRecordEntity entity, bool isChannelNotify = false)
        {
            entity.Memo = entity.Memo.Truncate(500);

            WriteOperateLog(new List<UserOperateRecordEntity>
            {
                entity
            });

            if (isChannelNotify && !SettingHelper.IsMSTest)
            {
                var currentUser = GetCurrentOwnPermissionByCache(entity.UserID);
                string operateType = ((UserOperateTypeEnum)entity.OperateType).ToString();
                string msg = $"用戶:{currentUser.UserName} ({currentUser.UserID}) 操作了 {operateType}。 資料:{entity.TargetID}、{entity.Memo} {entity.DebugInfo}";

                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }
        }



        private string TryGetRequestUrl()
        {
            try
            {
                return MyHttpContext.Current?.Request?.Path;
            }
            catch
            {

            }

            return "";
        }

        public void CounterRiskUserAndThrowException(int userID, TerproException ex)
        {
            decimal riskScore = 1.0M;

            if (ex.ErrorDetail != null)
            {
                riskScore = ex.ErrorDetail.RiskRate;
            }

            string myIPAddress = IPHelper.GetIP();

            var sysConfigRepo = IOCHelper.InstanceRepository<SystemConfigSettingRepository>();

            //如果是黑名單IP。風險分數再+1
            var isMatchForbiddenIP = sysConfigRepo.IsAdminSysForbiddenIPs(myIPAddress);

            if (isMatchForbiddenIP)
            {
                riskScore = riskScore + 1;
            }

            var settingKey = UserCustomizedSettingEnum.RiskScore;

            var repo = IOCHelper.InstanceRepository<UserCustomizedSettingRepository>();
            var updateResult = repo.SettingValuePlus(userID, settingKey, riskScore);

            //首次標記發個通知
            if (updateResult.IsCreate && SettingHelper.IsProduction)
            {
                SendMessageToMultipleChannel($"用戶:{userID} 首次被標記風險 `{TryGetRequestUrl()}` {myIPAddress}");
            }

            bool isLockAccount = SettingHelper.IsProduction && updateResult.AfterValue > 10;

            //if(SettingHelper.IsDev && userID == 8565)
            //{
            //    isLockAccount = true;
            //}

            //如果分數超過 強制鎖定帳號 (MSTest不處理此)
            if (isLockAccount)
            {
                List<int> lockUserIDs = new List<int>();

                try
                {
                    var userRepo = IOCHelper.InstanceRepository<UserRepository>();
                    var userTokenRecordRep = IOCHelper.InstanceRepository<UserTokenRecordRepository>();

                    lockUserIDs = GetBlockRelativeUsers(userRepo, userID);

                    foreach (int eachUserID in lockUserIDs)
                    {
                        //Step 1. 狀態鎖定
                        userRepo.Update(eachUserID, new
                        {
                            Status = NormalActiveStatus.DisableAndForbiddenEdit.ToInteger(),
                        });

                        //Step 2. 強制登出
                        userTokenRecordRep.ForceLogoutAdminSysAccounts(eachUserID);
                    }

                    var log = InstanceOperateLog(SystemConstant.DefaultUserID, UserOperateTypeEnum.User_AutoLockUserStatus, userID, "");
                    log.Memo = $"accounts:{string.Join(",", lockUserIDs)}";

                    WriteOperateLog(log);
                }
                catch (Exception ex2)
                {
                    SendMessageToMultipleChannel($"CounterRiskUserAndThrowException 發生錯誤 {ex2.GetMessageAndErrorLine()}", TelegramChannelEnum.pgs_important_error);
                }

                SendMessageToMultipleChannel($"用戶:{userID} 風險異常分數超標，將自動鎖定帳號, 關聯帳號：{lockUserIDs.Count()}個", TelegramChannelEnum.pgs_important_error);
            }

            throw ex;
        }



        public List<int> GetBlockRelativeUsers(UserRepository userRepo, int currentUserID)
        {
            List<int> lockUserIDs = new List<int>();

            var currentUser = GetCurrentOwnPermissionByCache(currentUserID);

            lockUserIDs.Add(currentUserID);

            if (currentUser.IsHierarchyMerchantMode)
            {
                // 1. 同個代理體系的帳號
                // 2. 由該帳號創建的帳號
                var queryRelativeUsers1 = userRepo.GetByFilter(new QueryUserRequestViewModel
                {
                    ReferenceCreditMerchantIDs = currentUser.ReferenceCreditMerchant.ToListCollection()
                });

                var queryRelativeUsers2 = userRepo.GetByFilter(new QueryUserRequestViewModel
                {
                    CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                    CreateByMatch = currentUser.UserName,
                });

                lockUserIDs.AddRange(queryRelativeUsers1.Select(x => x.UserID).ToList());
                lockUserIDs.AddRange(queryRelativeUsers2.Select(x => x.UserID).ToList());
            }

            lockUserIDs = lockUserIDs.Distinct().ToList();
            return lockUserIDs;
        }


        protected void WriteOperateLog(List<UserOperateRecordEntity> entityList)
        {
            var rep = InstanceRepository<UserOperateRecordRepository>();
            rep.CreateMany(entityList);
        }


        protected void WriteOperateLogAndNotifyAlways(int userID, UserOperateRecordEntity entity)
        {
            WriteOperateLog(entity);

            if (SettingHelper.IsProduction)
            {
                var currentUser = GetCurrentOwnPermissionByCache(userID);
                string operateType = ((UserOperateTypeEnum)entity.OperateType).ToString();
                string msg = $"用戶:{currentUser.UserName} ({currentUser.UserID}) 操作了 {operateType}。 資料:{entity.TargetID}、{entity.DebugInfo}";

                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }
        }


        protected bool IsRestrictFilterRequired(AdminUserCacheModelAdminUpdateViewModel currentUser)
        {
            return !currentUser.IsSuperAdminOrg && SettingHelper.IsProduction && currentUser.AvailableMerchantIDs.Count() >= 3;
        }

        protected UserOperateRecordEntity InstanceOperateLog(int userID, UserOperateTypeEnum type, long targetID, string debugInfo)
        {
            var myInstance = InstanceOperateLog(userID, type, targetID, changeLog: null);
            myInstance.DebugInfo = debugInfo;
            return myInstance;
        }

        protected UserOperateRecordEntity InstanceOperateLog(int userID, UserOperateTypeEnum type, long targetID, UserOperateChangeLogModel changeLog = null)
        {
            bool isMobileDevice = HeaderValueHelper.IsMobileDevice();

            return new UserOperateRecordEntity
            {
                CreateTime = DateTimeHelper.GetCurrentTime(),
                CreateIPAddress = IPHelper.GetIP(),
                ChangeLogInfo = changeLog.ConvertToJsonString(),
                UserID = userID,
                OperateType = type.ToInteger(),
                TargetID = targetID,
                MerchantID = 0,
                Memo = type.ToString(),
                DeviceID = HeaderValueHelper.GetHDCode(),
                DeviceType = (isMobileDevice ? EazyDeviceTypeEnum.Mobile : EazyDeviceTypeEnum.PC).ToInteger()
            };
        }

        /// <summary>
        /// 後台特殊配置，會限制特定的後台角色 只能查特定的遊戲商資料
        /// </summary>
        protected List<int> GetRestrictVendorIDsByUserID(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //超級管理組織 不需要篩選
            if (currentUser.IsSuperAdminOrg)
                return new List<int>();

            int roleID = currentUser.Roles.FirstOrDefault();

            var setting = GetPartnerRestrictSetting();

            var agg = setting.CanOnlyQueryAGG_RoleIDs;
            var aggLive = setting.CanOnlyQueryAGGLive_RoleIDs;
            var aggFish = setting.CanOnlyQueryAGGFish_RoleIDs;

            //需注意json配置 每組roleID 同一時間只能限制一家廠商
            //若同時設 AGG + AGGLive 會以順序上判斷到的第一筆為主
            if (agg.IsNotEmptyNull())
            {
                if (agg.Contains(roleID))
                {
                    return GameVendorTypeEnum.AGG.ToInteger().ToListCollection();
                }
            }

            if (aggLive.IsNotEmptyNull())
            {
                if (aggLive.Contains(roleID))
                {
                    return GameVendorTypeEnum.AGGLive.ToInteger().ToListCollection();
                }
            }

            if (aggFish.IsNotEmptyNull())
            {
                if (aggFish.Contains(roleID))
                {
                    return GameVendorTypeEnum.AGGFish.ToInteger().ToListCollection();
                }
            }

            return new List<int>();
        }

        private PartnerRoleRestrictSettingModel GetPartnerRestrictSetting()
        {
            var rep = InstanceRepository<SystemConfigSettingRepository>();

            var querySettingAGG = rep.GetParseModelByCache(SystemConfigSettingEnum.PartnerSetting_RoleID_ResrtrictQueryAGGOnly);
            var querySettingAGGLive = rep.GetParseModelByCache(SystemConfigSettingEnum.PartnerSetting_RoleID_ResrtrictQueryAGGLiveOnly);
            var querySettingAGGFish = rep.GetParseModelByCache(SystemConfigSettingEnum.PartnerSetting_RoleID_ResrtrictQueryAGGFishOnly);

            return new PartnerRoleRestrictSettingModel
            {
                CanOnlyQueryAGG_RoleIDs = querySettingAGG?.SettingValueOfListInt,
                CanOnlyQueryAGGLive_RoleIDs = querySettingAGGLive?.SettingValueOfListInt,
                CanOnlyQueryAGGFish_RoleIDs = querySettingAGGFish?.SettingValueOfListInt,
            };
        }


        protected int GetDefaultAdminUserID()
        {
            var userRepository = InstanceRepository<UserRepository>();

            var queryUser = userRepository.GetByAccount(SettingHelper.DefaultUserName);

            if (queryUser == null)
            {
                queryUser = userRepository.GetByAccount("marshall");
            }

            if (queryUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            return queryUser.UserID;
        }


        protected bool IsInternalTestAccountPlayerOrMerchantPlayer(int playerID)
        {
            var playerRepository = InstanceRepository<PlayerRepository>();

            var queryPlayer = playerRepository.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var rep1 = InstanceRepository<MerchantCustomizedSettingRepository>();

            var queryMerchantSetting = rep1.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant);

            if (queryMerchantSetting != null && queryMerchantSetting.SettingValueOfBoolean)
                return true;

            return false;
        }


        protected List<int> FilterAvailablePermissionMerchantIDs(int userID, List<int> merchantIDs)
        {
            var queryCurrentUser = GetCurrentOwnPermissionByCache(userID);
            return FilterAvailablePermissionMerchantIDs(queryCurrentUser, merchantIDs);
        }


        protected List<int> FilterAvailablePermissionMerchantIDs(AdminUserCacheModelAdminUpdateViewModel queryCurrentUser, List<int> merchantIDs)
        {
            //如果是超級管理員的組織、不需要檢查、或直接不篩選merchantID
            if (queryCurrentUser.IsSuperAdminOrg)
                return merchantIDs ?? new List<int>();

            //如果一般等級的廠商，沒填寫merchantID、強制指定要篩選IDs (但是後面又會判斷hierarchyMerchant或 multipleHierarchyMode)
            if (merchantIDs.IsEmptyOrNull())
                return queryCurrentUser.AvailableMerchantIDs;

            //如果是多組代理商戶的分配類型、嘗試使用效能較差的檢查方式) (2023/07 的新型檢查法)
            if (queryCurrentUser.IsMultipleHierarchyMode && merchantIDs.Count() <= 3)
            {
                foreach (int merchantID in merchantIDs)
                {
                    CheckViewMerchantPermission(queryCurrentUser.UserID, merchantID);
                }

                return merchantIDs;
            }

            //檢查是否有帶 違反權限的merchantIDs
            if (merchantIDs.Any(x => !queryCurrentUser.AvailableMerchantIDs.Contains(x)))
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            return merchantIDs;
        }



        public AdminUserCacheModelAdminUpdateViewModel GetCurrentOwnPermissionByCache(int userID)
        {
            if (userID <= 0)
                return null;

            string cacheKey = CacheKey.AdminSys_UserOwnPermission(userID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var merchantRep = InstanceRepository<MerchantRepository>();
                var userRep = InstanceRepository<UserRepository>();
                var orgRep = InstanceRepository<OrganizationRepository>();
                var orgMappingRep = InstanceRepository<OrganizationMerchantMappingRepository>();
                var roleMappingRep = InstanceRepository<UserRoleMappingRepository>();
                var userMerchantRep = InstanceRepository<UserMerchantMappingRepository>();
                var hierarchyRep = InstanceRepository<MerchantHierarchyRepository>();

                var queryUser = userRep.GetById(userID);

                if (queryUser == null)
                    throw new TerproException(ApiErrorCode.NotFoundUser, new ErrorDetailModel($"{userID}"));

                var queryOrg = orgRep.GetById(queryUser.OrganizationID);
                var queryMyMerchants = userMerchantRep.GetByUserID(userID);

                bool isSuperAdmin = queryOrg.OrganizationType == OrganizationTypeEnum.SuperAdmin.ToInteger();

                var result = new AdminUserCacheModelAdminUpdateViewModel
                {
                    TimeZone = DataModelConvertHelper.GetTimeZoneByAreaID(queryUser.TimeZoneAreaID),
                    UserID = queryUser.UserID,
                    UserName = queryUser.UserName,
                    OrganizationID = queryUser.OrganizationID,
                    IsSuperAdminOrg = isSuperAdmin,
                    Roles = roleMappingRep.GetByUserID(queryUser.UserID).Select(x => x.RoleID).ToList(),
                    AvailableOrganizationIDs = queryUser.OrganizationID.ToListCollection(),
                    AvailableMerchantIDs = queryMyMerchants.Select(x => x.MerchantID).ToList(),
                    AllEnableMerchantsOrAvailableMerchants = queryMyMerchants.Select(x => x.MerchantID).ToList(),
                    ReferenceCreditMerchant = queryUser.ReferenceCreditMerchant ?? 0,
                    IsMultipleHierarchyMode = queryUser.AssignMerchantMode == UserAssignMerchantModeEnum.MultipleHierarchyMerchant.ToInteger(),
                };

                //如果是超級管理員組織
                if (isSuperAdmin)
                {
                    var queryEnableMerchants = merchantRep.GetByFilter(new QueryMerchantRequestViewModel
                    {
                        Status = NormalActiveStatus.Enable.ToInteger()
                    });

                    result.AllEnableMerchantsOrAvailableMerchants = queryEnableMerchants.Select(z => z.MerchantID).ToList();
                }
                else if (queryUser.ReferenceCreditMerchant.HasValue)
                {
                    var myHierarchyMerchants = hierarchyRep.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                    {
                        HierarchyOfTargerMerchant = queryUser.ReferenceCreditMerchant.Value
                    });

                    List<int> hierarchyMerchants = myHierarchyMerchants
                        .Select(z => z.MerchantID)
                        .Distinct()
                        .ToList();

                    if (hierarchyMerchants.IsNotEmptyNull())
                    {
                        result.AvailableMerchantIDs = hierarchyMerchants;
                        result.AllEnableMerchantsOrAvailableMerchants = hierarchyMerchants;
                    }
                }

                return result;

            }, 10);

            return queryCache;
        }


        /// <summary>
        /// 判斷是否: 雖是超級管理組織、但是帳號功能有受到限制
        /// e.g. 客服特殊帳號
        /// </summary>
        protected bool IsRestrictPermissionForSuperOrgAccount(int userID)
        {
            return HasTargetPermission(userID, "Special_SuperAdminOrg_ButRestrictFeature");
        }

        /// <summary>
        /// 是否為我方內部客服帳號
        /// </summary>
        protected bool HasCustomerServicePermission(int userID)
        {
            return HasTargetPermission(userID, "Special_PGS_CustomerService");
        }

        /// <summary>
        /// 包含 公司內部主管、PM也都會有此權限
        /// </summary>
        public bool HasSuperAdminPermission(int userID)
        {
            return HasTargetPermission(userID, "SystemSetting_SystemConfigSetting_Update");
        }

        /// <summary>
        /// 技術人員才允許操作的某些功能
        /// </summary>
        public bool HasTechPermission_AllowToUseDevelopeFeature(int userID)
        {
            return HasTargetPermission(userID, "Special_AllowToUseDevelopeFeature");
        }


        /// <summary>
        /// 只能查看 IDG visible game
        /// </summary>
        public bool HasPermission_OnlyVisibleIDGGames(int userID)
        {
            return HasTargetPermission(userID, "Special_OnlyVisible_IDG_Games");
        }


        public bool HasTargetPermission(int userID, string permissionToken)
        {
            if (userID == SystemConstant.SkipValidateUserID)
                return false;

            var queryCurrentUser = GetCurrentOwnPermissionByCache(userID);

            var myRep = InstanceRepository<RolePermissionMappingRepository>();

            var query = myRep.GetByFilter(new QueryRolePermissionMappingRequestViewModel
            {
                RoleIDs = queryCurrentUser.Roles,
                PermissionToken = [REDACTED]
            });

            if (query.IsNotEmptyNull())
                return true;

            var userPermissionRep = InstanceRepository<UserPermissionMappingRepository>();

            var query2 = userPermissionRep.GetByFilter(new QueryUserPermissionMappingRequestViewModel
            {
                UserIDs = queryCurrentUser.UserID.ToListCollection(),
                PermissionToken = [REDACTED]
            });

            return query2.IsNotEmptyNull();
        }



        protected void CheckViewPlayerPermission(int userID, int playerID, bool isTriggerRiskScoreProcess = true)
        {
            var rep = InstanceRepository<PlayerRepository>();

            var queryPlayer = rep.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            CheckViewMerchantPermission(userID, queryPlayer.MerchantID, isTriggerRiskScoreProcess);
        }


        public void CheckViewMerchantPermission(int userID, int targetMerchantID, bool isTriggerRiskScoreProcess = true)
        {
            if (userID == SystemConstant.SkipValidateUserID)
                return;

            if (targetMerchantID >= 0 && !RegexRuleHelper.IsMerchantID(targetMerchantID))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    ColumnName = "Merchant",
                    LogExtraInfo = $"{targetMerchantID} , {IPHelper.GetIP()}"
                });
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            bool hasPermission = IsViewMerchantPermission(currentUser, targetMerchantID); ;

            if (hasPermission)
                return;

            var myException = new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            //風險分數觸發 (判斷後台惡意攻擊、API漏洞嘗試)
            if (isTriggerRiskScoreProcess)
            {
                CounterRiskUserAndThrowException(userID, myException);
            }
            else
            {
                throw myException;
            }
        }


        public virtual void CheckViewUserPermission(int currentUserID, int targetUserID)
        {
            var para = new
            {
                UserID = currentUserID,
                TargetUserID = targetUserID
            };

            para.RuleFor(z => z.UserID).Rule_NotZeroNotMinus();
            para.RuleFor(z => z.TargetUserID).Rule_NotZeroNotMinus();

            if (currentUserID == targetUserID)
            {
                return;
            }

            var currentUser = GetCurrentOwnPermissionByCache(currentUserID);

            //超級管理員
            if (currentUser.IsSuperAdminOrg)
                return;

            var UserRepository = InstanceRepository<UserRepository>();

            var query = UserRepository.GetByIds(new List<int>
            {
                currentUserID,
                targetUserID,
            });

            var matchCurrentUser = query.FirstOrDefault(x => x.UserID == currentUserID);
            var matchTargetUser = query.FirstOrDefault(x => x.UserID == targetUserID);

            if (matchCurrentUser == null || matchTargetUser == null)
                throw new TerproException(ApiErrorCode.NotFoundUser);

            if (currentUser.IsHierarchyMerchantMode)
            {
                var isViewPermission = UserRepository.IsViewUserPermissionByCreditMerchant(targetUserID, currentUser.ReferenceCreditMerchant);

                if (isViewPermission)
                {
                    return;
                }
            }
            else
            {
                //如果同組織OK
                if (matchCurrentUser.OrganizationID == matchTargetUser.OrganizationID)
                {
                    return;
                }
            }

            //理論上有較低的機率跑到這邊 因此先執行 IsHierarchyMerchantMode 效能較好
            if (HasTargetPermission(currentUserID, "Special_Account_SearchAccountByMultiMerchantAssignMode"))
            {
                //允許透過兩種條件 (符合其中之一)
                var queryByHierarchy = UserRepository.GetByFilter(new QueryUserRequestViewModel
                {
                    UserIDs = targetUserID.ToListCollection(),
                    FilterAvailableUsersByCreditMerchantID_NeedSpecialPermission = currentUser.ReferenceCreditMerchant
                });

                if (queryByHierarchy.IsNotEmptyNull())
                {
                    return;
                }
            }

            CounterRiskUserAndThrowException(currentUserID, new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow));
        }


        public void CheckViewOrganizationPermission(int userID, int orgID, bool isTriggerRiskScoreProcess = true)
        {
            if (userID == SystemConstant.SkipValidateUserID)
                return;

            if (orgID <= 0)
                return;

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (currentUser.IsSuperAdminOrg)
                return;


            var userRep = IOCHelper.InstanceRepository<UserRepository>();

            var queryUser = userRep.GetById(userID);

            if(queryUser.OrganizationID == orgID)
            {
                return;
            }

            var myException = new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow , new ErrorDetailModel("OrgID"));

            //風險分數觸發 (判斷後台惡意攻擊、API漏洞嘗試)
            if (isTriggerRiskScoreProcess)
            {
                CounterRiskUserAndThrowException(userID, myException);
            }
            else
            {
                throw myException;
            }
        }







        protected bool IsViewMerchantPermission(AdminUserCacheModelAdminUpdateViewModel currentUser, int merchantID)
        {
            if (currentUser == null)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            if (currentUser.IsSuperAdminOrg)
                return true;

            if (currentUser.AvailableMerchantIDs.Contains(merchantID))
                return true;

            //若沒有權限。則嘗試進行另一種查詢
            //從hierarchy確認權限 (針對MYC代理有做某種特殊帳號配置、讓Super級代理可以跨幣別查詢全部...)
            var hierRep = IOCHelper.InstanceRepository<MerchantHierarchyRepository>();
            var queryHierarchy = hierRep.GetById(merchantID);

            if (queryHierarchy != null)
            {
                //先查出目標商戶的所有樹狀結構
                List<int> hierarchyMerchantIDs = AppHelper.GetHierarchyMerchantIDs(queryHierarchy.Hierarchy)
                    .Where(myID => myID != merchantID)
                    .ToList();

                //如果當前帳號有分配 能查看root或parent。則視為權限合法
                bool isAnyPermissionByParent = hierarchyMerchantIDs.Any(myID => IsViewMerchantPermission(currentUser, myID));

                if (isAnyPermissionByParent)
                {
                    return true;
                }
            }

            var merchantRep = IOCHelper.InstanceRepository<MerchantRepository>();  

            var queryMerchant = merchantRep.GetById(merchantID);

            if(queryMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
            {
                //很瞎的方法 這邊加這個邏輯是因為AM創建dev商戶 會因為順序問題 被擋permission錯誤 所以加了一個例外判斷
                //Dev商戶 而且是30秒內創建的 允許直接return true
                if((DateTimeHelper.GetCurrentTime() - queryMerchant.CreateTime).TotalSeconds <= 30)
                {
                    return true;
                }
            }

            return false;
        }


        protected void ShowHintErrorMessageWhenSearchByPlayerID(int userID, int? playerID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //馬來代理的毛(低能)太多了 防呆訊息要加強
            if (!currentUser.IsSuperAdminOrg && playerID.HasValue)
            {
                var rep = InstanceRepository<PlayerRepository>();
                var queryPlayerExist = rep.GetBriefByID(playerID.Value);

                if (queryPlayerExist == null || !IsViewMerchantPermission(currentUser, queryPlayerExist.MerchantID))
                {
                    throw new TerproException(ApiErrorCode.NotFoundPlayer_IsThisAccountOrPlayerID);
                }
            }
        }


        protected (List<int> PlayerIDs, bool IsPlayerNotFound, List<int> PlayerMerchantIDs) GetPlayerIDsByPlayerName(int userID, string playerName, List<int> existArrayPlayerIDs , int? extraFilterByMerchantID = null)
        {
            List<int> emptyMerchantIDs = new List<int>();

            if (string.IsNullOrEmpty(playerName))
                return (existArrayPlayerIDs, false, emptyMerchantIDs);

            var rep = InstanceRepository<PlayerRepository>();

            //此處不篩選merchantID (下面程式碼才篩選、因為如果丟100+筆 merchantIDs 會查超慢)
            var queryPlayers = rep.GetByPlayerName(playerName, null, isLockQuery: false);

            if (queryPlayers.IsEmptyOrNull())
                return (new List<int>(), true, emptyMerchantIDs);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (currentUser.IsSuperAdminOrg)
            {
                if (extraFilterByMerchantID.HasValue)
                {
                    queryPlayers = queryPlayers
                        .Where(z => z.MerchantID == extraFilterByMerchantID.Value)
                        .ToList();
                }

                return (queryPlayers.Select(z => z.PlayerID).ToList(), false, emptyMerchantIDs);
            }
            else
            {
                var availableMerchantPlayers = queryPlayers
                    .Where(z => currentUser.AvailableMerchantIDs.Contains(z.MerchantID))
                    .ToList();

                if(extraFilterByMerchantID.HasValue)
                {
                    availableMerchantPlayers = availableMerchantPlayers
                        .Where(z => z.MerchantID == extraFilterByMerchantID.Value)
                        .ToList();
                }

                var playerIDs = availableMerchantPlayers.Select(x => x.PlayerID).ToList();
                var merchantIDs = availableMerchantPlayers.Select(x => x.MerchantID).ToList();

                return (playerIDs, availableMerchantPlayers.IsEmptyOrNull(), merchantIDs);
            }
        }



    }
}
