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
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using target.Utility.Extensions.Web;
using target.Core.Helper;
using target.Utility.Utility.ThirdParty;
using System.Transactions;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public class MerchantHierarchyService : BaseAdminSysService, IMerchantHierarchyService
    {
        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantHierarchyChildMappingRepository MerchantHierarchyChildMappingRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public IMerchantSelectTemplateService MerchantSelectTemplateService { get; set; }

        public IAdminSysValidateCheckLogicRefService IAdminSysValidateCheckLogicRefService { get; set; }
 



        public MerchantHierarchyTreeviewViewModel GetHierarchyTreeView(int userID, MerchantHierarchyTreeviewRequestModel filter)
        {
            if (!filter.RootMerchantID.HasValue && !filter.SearchHierarchyMemberMerchantID.HasValue)
                throw new TerproException(ApiErrorCode.InvalidParameter);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg && filter.RootMerchantID.HasValue)
            {
                if (currentUser.ReferenceCreditMerchant != filter.RootMerchantID)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
                }
            }

            //透過樹狀裡面的某一位id來搜尋
            if (filter.SearchHierarchyMemberMerchantID.HasValue)
            {
                var query = MerchantHierarchyRepository.GetById(filter.SearchHierarchyMemberMerchantID.Value);

                if (query == null)
                    throw new TerproException(ApiErrorCode.NotFoundMerchantHierarchy);

                filter.RootMerchantID = query.RootMerchantID;
            }

            var queryHierarchyByRoot = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                RootMerchantID = filter.RootMerchantID
            });

            if (queryHierarchyByRoot == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant, new ErrorDetailModel("Not agent hierarchy"));

            List<int> merchantIDs = queryHierarchyByRoot.Select(z => z.MerchantID).ToList();
            var queryMerchantDic = MerchantRepository
                .GetByFilter(new QueryMerchantRequestViewModel
                {
                    MerchantIDs = merchantIDs
                })
                .ToDictionary(x => x.MerchantID, x => x);

            var viewModel = new MerchantHierarchyTreeviewViewModel();

            //先查出最高層數
            var eachMerchantLayer = queryHierarchyByRoot
                .Select(z =>
                 {
                     var convert = new
                     {
                         MerchantID = z.MerchantID,
                         ParentMerchantID = z.ParentMerchantID,
                         Hierarchy = z.Hierarchy,
                         Layer = z.Hierarchy.Split('/').Where(z => !string.IsNullOrWhiteSpace(z)).Count()
                     };

                     return convert;
                 })
                .ToList();

            int maxLayers = eachMerchantLayer.Select(z => z.Layer).DefaultIfEmpty().Max();
            maxLayers = maxLayers <= 1 ? 1 : maxLayers;

            var allChildItems = new List<MerchantHierarchyTreeviewItemViewModel>();

            var rootMerchant = queryHierarchyByRoot
                .OrderBy(x => x.RootMerchantID == x.MerchantID ? 0 : 1)
                .ThenBy(x => x.Hierarchy.CountOrZero())
                .FirstOrDefault();

            var vendorConfig_agg = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
                {
                    MerchantIDs = rootMerchant.MerchantID.ToListCollection(),
                    VendorIDs = GameVendorTypeEnum.AGG.ToInteger().ToListCollection(),
                })
                .FirstOrDefault();

            string aggVendorConfigInfo = "";
            string line = Environment.NewLine;

            if (vendorConfig_agg != null)
            {
                var assignVendorID = VendorConfigSettingRepository.GetAssignVendorIdByVendorConfigID(vendorConfig_agg.VendorConfigID);
                aggVendorConfigInfo = $"服務器配置：{assignVendorID.Value} (#{assignVendorID.VendorConfigID})";
            }                     

            //從最底層的model開始instance
            for (int i = maxLayers; i > 0; i--)
            {
                var currentLayers = eachMerchantLayer.Where(z => z.Layer == i).ToList();
                var convertItems = new List<MerchantHierarchyTreeviewItemViewModel>();

                convertItems = currentLayers.Select(z =>
                {
                    string merchantName = "*商戶不存在/已刪除";

                    if (queryMerchantDic.ContainsKey(z.MerchantID))
                    {
                        merchantName = queryMerchantDic[z.MerchantID].Name;
                    }

                    var myModel = new MerchantHierarchyTreeviewItemViewModel
                    {
                        MerchantID = z.MerchantID,
                        ParentMerchantID = z.ParentMerchantID,
                        MerchantHierarchy = z.Hierarchy,
                        CopyInfo = $"{aggVendorConfigInfo}{line}商戶ID：{z.MerchantID}{line}樹狀結構：{z.Hierarchy}",
                        Name = merchantName,
                        Childs = allChildItems.Where(x => x.ParentMerchantID == z.MerchantID).ToList(),
                        Layer = i
                    };

                    return myModel;
                }).ToList();

                allChildItems.AddRange(convertItems);
            }

            viewModel.Root = allChildItems.Where(z => z.MerchantID == filter.RootMerchantID).FirstOrDefault();

            if (filter.IsShowSpecificTreeOnly)
            {
                //只返回指定代理下方的tree
                viewModel.Root = allChildItems
                    .Where(x => x.MerchantID == filter.SearchHierarchyMemberMerchantID)
                    .FirstOrDefault();
            }

            viewModel.RootMerchantID = viewModel.Root.MerchantID;
            var maxLayerLimitSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(viewModel.RootMerchantID, MerchantCustomizedSettingKeyEnum.System_AgentHierarchyMaxLayer);

            viewModel.HierarchyMaxLayerLimit = maxLayerLimitSetting.SettingValueOfInt;

            var childAgents = GetAllChilds(viewModel.Root).Select(x => x.MerchantID);
            viewModel.TotalAgentCount = childAgents.Distinct().Count();

            return viewModel;
        }


        private List<MerchantHierarchyTreeviewItemViewModel> GetAllChilds(MerchantHierarchyTreeviewItemViewModel self)
        {
            List<MerchantHierarchyTreeviewItemViewModel> allChilds = new List<MerchantHierarchyTreeviewItemViewModel>();

            allChilds.Add(self);

            if (self.Childs.IsEmptyOrNull())
            {

            }
            else
            {
                foreach (var item in self.Childs)
                {
                    allChilds.AddRange(GetAllChilds(item));
                }
            }

            return allChilds;
        }


        public void MerchantHierarchyMigrate(int userID, MerchantHierarchyMigrateRequestModel model)
        {
            model.RuleFor(x => x.TargetMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.NewParentMerchantID).Rule_NotZeroNotMinus();

            //雙方需要都建立過hierarcy
            var queryTargetHier = MerchantHierarchyRepository.GetByIdOrThrowException(model.TargetMerchantID);
            var queryParentHier = MerchantHierarchyRepository.GetByIdOrThrowException(model.NewParentMerchantID);

            if (queryTargetHier.MerchantID == model.NewParentMerchantID)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            if (queryTargetHier.ParentMerchantID == model.NewParentMerchantID)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            if (queryParentHier.ParentMerchantID == queryTargetHier.MerchantID)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            var queryTargetMerchant = MerchantRepository.GetByIdOrThrowException(model.TargetMerchantID);
            var queryParentMerchant = MerchantRepository.GetByIdOrThrowException(model.NewParentMerchantID);

            CheckViewMerchantPermission(userID, model.TargetMerchantID);
            CheckViewMerchantPermission(userID, model.NewParentMerchantID);

            if (queryTargetMerchant.Currency != queryParentMerchant.Currency)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_CurrencyNotMatch);

            string originalParentText = queryTargetHier.ParentMerchantID == 0 ? "" : $"{queryTargetHier.ParentMerchantID}/";

            //有可能改變有可能不改變、但一律update
            var beforeRootID = queryTargetHier.RootMerchantID;
            var newRootID = queryParentHier.RootMerchantID;

            string originalHierarchy = queryTargetHier.Hierarchy;
            string newHierarchy = $"{queryParentHier.Hierarchy}{queryTargetHier.MerchantID}/";

            MerchantHierarchyRepository.MigrateHierarchy(new MerchantHierarchyMigrateParameter
            {
                TargetMerchantID = queryTargetHier.MerchantID,
                NewRootID = newRootID,
                NewParentMerchantID = queryParentHier.MerchantID,
                KeywordReplaceFrom = originalHierarchy,
                KeywordReplaceTo = newHierarchy
            });

            //重新產生hierarchy
            ExecuteProcessWhenHierarchyChanged(newRootID);

            //如果是轉移到其他樹。可能需要連舊的tree都一起更新
            if (beforeRootID != newRootID)
            {
                ExecuteProcessWhenHierarchyChanged(beforeRootID);
            }

            string debugInfo = $"original:{queryTargetHier.Hierarchy} , newParent:{model.NewParentMerchantID}";
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.Agent_AgentMigrate, model.TargetMerchantID, debugInfo);

            WriteOperateLog(log);
        }


        public void CreateAsRootMerchantHierarchy(int userID, int merchantID)
        {
            var model = new { MerchantID = merchantID };
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();

            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            CheckViewMerchantPermission(userID, merchantID);

            var queryExist = MerchantHierarchyRepository.GetById(merchantID);

            if (queryExist != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            MerchantHierarchyRepository.Create(new MerchantHierarchyEntity
            {
                MerchantID = merchantID,
                ParentMerchantID = 0,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                RootMerchantID = merchantID,
                Hierarchy = $"{merchantID}/"
            });

            ExecuteProcessWhenHierarchyChanged(merchantID);
        }



        public CRUDResultModel CreateMerchantHierarchyWithTryCatch(int userID, MerchantHierarchyCreateRequestModel model)
        {
            try
            {
                this.CreateMerchantHierarchy(userID, model);
                return new CRUDResultModel
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                string msg = $"CreateMerchantHierarchy 失敗 : {model.TargetMerchantID} {model.ParentMerchantID} , {ex.GetMessageAndErrorLine()}";
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Message = msg,
                    Channel = SlackChannelEnum.n_error_adminsys
                });

                return new CRUDResultModel
                {
                    IsSuccess = false,
                    DebugMessage = $"部分项目设定失败。{ex.Message} (MerchantHierarchy) ",
                };
            }

        }


        public void CreateMerchantHierarchy(int userID, MerchantHierarchyCreateRequestModel model)
        {
            model.RuleFor(z => z.TargetMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.ParentMerchantID).Rule_NotZeroNotMinus();

            if (model.TargetMerchantID == model.ParentMerchantID)
                throw new TerproException(ApiErrorCode.InvalidParameter_SourceValueMustNotEqualTargetValue);

            var queryExist = MerchantHierarchyRepository.GetById(model.TargetMerchantID);

            if (queryExist != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            var queryTargetMerchant = MerchantRepository.GetByIdOrThrowException(model.TargetMerchantID);

            //查出Parent & 包含一些防呆檢查1
            var queryParent = ValidParentHierarchyBeforeCreateMerchantHierarchy(model.ParentMerchantID);

            //查出Parent & 包含一些防呆檢查2
            var queryParentMerchant = ValidParentBasicInfoBeforeCreateMerchant(new MerchantParentAgentValidateBeforeCreateParameter
            {
                ParentMerchantID = model.ParentMerchantID,
                Currency = queryTargetMerchant.Currency,
                MerchantType = queryTargetMerchant.Type
            });

            CheckViewMerchantPermission(userID, model.ParentMerchantID);

            MerchantHierarchyRepository.Create(new MerchantHierarchyEntity
            {
                MerchantID = model.TargetMerchantID,
                ParentMerchantID = queryParent.MerchantID,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                RootMerchantID = queryParent.RootMerchantID,
                Hierarchy = queryParent.Hierarchy + $"{model.TargetMerchantID}/"
            });

            ExecuteProcessWhenHierarchyChanged(queryParent.RootMerchantID , isCreateNewMerchant: true);

            string debugInfo = $"original:0 , newParent:{queryParent.MerchantID}";
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.Agent_AgentHierarchyUpdate, model.TargetMerchantID, debugInfo);
        }

       
        public MerchantHierarchyEntity ValidParentHierarchyBeforeCreateMerchantHierarchy(int parentMerchantID)
        {
            var model = new
            {
                ParentMerchantID = parentMerchantID
            };

            model.RuleFor(z => z.ParentMerchantID).Rule_NotZeroNotMinus();

            var queryParentHierarchy = MerchantHierarchyRepository.GetById(parentMerchantID);

            if (queryParentHierarchy == null)
                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel("Parent"));

            CheckTargetMerchantHierarchyCanCreateChild(queryParentHierarchy.MerchantID);
            return queryParentHierarchy;
        }


        public MerchantEntity ValidParentBasicInfoBeforeCreateMerchant(MerchantParentAgentValidateBeforeCreateParameter model)
        {
            var queryParentMerchant = MerchantRepository.GetSingleByCache(model.ParentMerchantID);

            if(queryParentMerchant.Status != NormalActiveStatus.Enable.ToInteger())
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus , new ErrorDetailModel($"Parent Merchant {model.ParentMerchantID} status"));
            }

            if (!string.IsNullOrWhiteSpace(model.Currency))
            {
                if (queryParentMerchant.Currency != model.Currency)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_CurrencyNotMatch);
                }
            }

            if (model.MerchantType.HasValue)
            {
                if (queryParentMerchant.Type != model.MerchantType)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_MerchantTypeNotMatch);
                }
            }

            return queryParentMerchant;
        }


        public void ValidParentAndEnableGames(int parentMerchantID , List<int> enableGameIDs)
        {
            var querySettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = parentMerchantID.ToListCollection(),
                CustomizedSettingKeyIDs = new List<int>
                {
                    MerchantCustomizedSettingKeyEnum.GameSetting_OnlySupportIDGVisibleGames.ToInteger()
                }
            });

            var IDGvisibleGamesSetting = querySettings
                .Where(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.GameSetting_OnlySupportIDGVisibleGames)
                .FirstOrDefault();

            //檢查#1 商戶只能開通特定遊戲
            if(IDGvisibleGamesSetting != null)
            {
                var parseSetting = new SystemConfigSettingParseModel(IDGvisibleGamesSetting.CustomizedSettingKey, IDGvisibleGamesSetting.SettingValue);

                if(parseSetting.SettingValueOfBoolean)
                {
                    IAdminSysValidateCheckLogicRefService.CheckMerchantOnlySupportIDGVisibleGamesAndThrow(parentMerchantID.ToListCollection(), enableGameIDs);
                }
            }

            return;
        }


        public void DeleteMerchantHierarchy(int userID, MerchantHierarchyDeleteRequestModel model)
        {
            model.RuleFor(z => z.TargetMerchantID).Rule_NotZeroNotMinus();

            var queryExist = MerchantHierarchyRepository.GetById(model.TargetMerchantID);

            if (queryExist == null)
                return;

            CheckViewMerchantPermission(userID, model.TargetMerchantID);

            var childAgents = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                ParentMerchantID = model.TargetMerchantID
            });

            if (childAgents.IsNotEmptyNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("此商戶仍有下級代理、禁止進行此操作"));
            }

            MerchantHierarchyRepository.Delete(model.TargetMerchantID);

            ExecuteProcessWhenHierarchyChanged(queryExist.RootMerchantID);

            string debugInfo = $"original:{queryExist.ParentMerchantID} , newParent:0";
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.Agent_AgentHierarchyUpdate, model.TargetMerchantID, debugInfo);

            return;
        }


        /// <summary>
        /// 每當代理樹狀有異動時。可能要一併調整的資料結構
        /// </summary>
        public void ExecuteProcessWhenHierarchyChanged(int rootMerchantID, bool isCreateNewMerchant = false)
        {
            var timeBegin = DateTimeHelper.GetCurrentTime();

            //重建 MerchantHierarchyChildMapping 資料
            GenerateHierarchyChildTreeData(rootMerchantID);

            var queryMyMerchants = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                RootMerchantID = rootMerchantID
            });

            List<int> myMerchantIDs = queryMyMerchants.Select(x => x.MerchantID).ToList();

            //異動商戶樹狀才需要 (如果是創建新下線。不需要刷新) ps. 因為是迴圈一筆一筆處理 速度有點慢
            if(!isCreateNewMerchant)
            {
                //緩存cache清除
                CacheRemoveHelper.RemoveCache_WhenMultipleCaches_WhenMerchantHierarchyChanged(myMerchantIDs);

                //清除配置資料 (IDG特殊配置)
                MerchantCustomizedSettingRepository.UpdateMultiple(myMerchantIDs, (int)MerchantCustomizedSettingKeyEnum.GameSetting_IDGNameSpace_CreateTime, "");
            }

            //商戶選擇模板
            MerchantSelectTemplateService.RegenerateAssignMerchantIDsForAgentAssignMode(myMerchantIDs);

            var timeEnd = DateTimeHelper.GetCurrentTime();

            //如果花太多時間 發通知
            AppHelper.SendNotifyWhenSpendTooMuchTime(timeBegin, timeEnd, "ExecuteProcessWhenHierarchyChanged", 7);
            return;
        }



        /// <summary>
        /// 每次代理樹狀有異動 都會重新產生此資料
        /// 這張表的存在對於報表計算SQL寫法有很大幫助
        /// </summary>
        public void GenerateHierarchyChildTreeData(int rootMerchantID)
        {
            var queryHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                RootMerchantID = rootMerchantID,
                ExcludeDeleteMerchants = true
            });

            if (queryHierarchy.IsEmptyOrNull())
                return;

            var convertToLayer = queryHierarchy.Select(x => new
            {
                MerchantID = x.MerchantID,
                Hierarchy = x.Hierarchy,
                Layer = AppHelper.GetAgentLayerLevelByHierarchy(x.Hierarchy)
            })
                .ToList();

            int maxLayer = convertToLayer.Max(x => x.Layer);

            List<MerchantHierarchyChildMappingEntity> insertItems = new List<MerchantHierarchyChildMappingEntity>();

            foreach (var highLevelAgent in convertToLayer)
            {
                var myChilds = convertToLayer
                    .Where(x => x.Hierarchy.Contains($"{highLevelAgent.MerchantID}/"))
                    .Select(x => x.MerchantID)
                    .ToList();

                var insertItemsOfThis = myChilds.Select(x => new MerchantHierarchyChildMappingEntity
                {
                    MerchantID = highLevelAgent.MerchantID,
                    ChildMerchantID = x,
                });

                insertItems.AddRange(insertItemsOfThis);
            }

            var scopeLevel = IsolationLevel.Serializable;

            using (var scope = AppHelper.InstanceTransactionScopeByLevel(scopeLevel))
            {
                MerchantHierarchyChildMappingRepository.DeleteByRootMerchantID(rootMerchantID);
                MerchantHierarchyChildMappingRepository.BulkInsert(insertItems , scopeLevel);
                scope.Complete();
            }
        }

        private void CheckTargetMerchantHierarchyCanCreateChild(int parentMerchantID)
        {
            var queryParent = MerchantHierarchyRepository.GetById(parentMerchantID);

            //如果指定總代就不檢查了 (減少查詢消耗)
            if (queryParent.RootMerchantID == parentMerchantID)
                return;

            int rootID = queryParent.RootMerchantID;

            var queryRootSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(rootID, MerchantCustomizedSettingKeyEnum.System_AgentHierarchyMaxLayer);

            int layerOfParent = AppHelper.GetAgentLayerLevelByHierarchy(queryParent.Hierarchy);
            int maxLayer = queryRootSetting.SettingValueOfInt;

            //若已經是指定的最底層級了 (e.g. 已經是第三層了)
            if (layerOfParent >= maxLayer)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_OverThanMaxHierarchyLayer);
            }
        }

    }
}
