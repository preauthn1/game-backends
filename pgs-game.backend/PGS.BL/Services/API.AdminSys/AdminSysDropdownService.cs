using EnumsNET;
using Lck.Utility.Extensions.Helper;
using Microsoft.Extensions.Caching.Memory;
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
using target.Models.ViewModels.API.AdminSys;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace target.BL.Services
{

    public class AdminSysDropdownService : BaseAdminSysService, IAdminSysDropdownService
    {

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public OrganizationRepository OrganizationRepository { get; set; }

        public UserRoleMappingRepository UserRoleMappingRepository { get; set; }

        public IPAddressListTemplateRepository IPAddressListTemplateRepository { get; set; }

        public GeneralTagRepository GeneralTagRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public RoleRepository RoleRepository { get; set; }

        public OrganizationRoleMappingRepository OrganizationRoleMappingRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public OrganizationCustomizedSettingRepository OrganizationCustomizedSettingRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public RoleCustomizedSettingRepository RoleCustomizedSettingRepository { get; set; }

        public AccountingInvoiceCategoryRepository AccountingInvoiceCategoryRepository { get; set; }

        public UserRepository UserRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public ServerListRepository ServerListRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public ServerMerchantMappingRepository ServerMerchantMappingRepository { get; set; }

        public DynamicQueryCommandRepository DynamicQueryCommandRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public MerchantSelectTemplateRepository MerchantSelectTemplateRepository { get; set; }


        public bool CheckIsSuperAdminOrg(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);
            return currentUser.IsSuperAdminOrg;
        }


        public List<DropdownValueViewModel> GetGames(int userID, QueryGameListApiRequestParameter filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter = filter ?? new QueryGameListApiRequestParameter();
            filter.IsNotDelete = true;  //已經被標記為 999(刪除) 的遊戲不顯示了
            filter.GameVendorTypes = filter.GameVendorType.ToIntegerListCollection();

            //部分帳號角色會限制只能查特定遊戲商
            if (filter.GameVendorTypes.IsEmptyOrNull())
            {
                filter.GameVendorTypes = GetRestrictVendorIDsByUserID(userID);
            }

            var queryGames = GameListRepository.GetByFilter(filter);

            //如果不是超級管理員 僅顯示有開通的遊戲
            if (!currentUser.IsSuperAdminOrg)
            {
                var gameFilter = new QueryMerchantGameSettingRequestViewModel
                {
                    Status = NormalActiveStatus.Enable.ToInteger(),
                    IsExcludeDeleteGames = true,
                };

                gameFilter.MerchantIDs = new List<int>();
                gameFilter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;

                string selectColumns = " Distinct GameID ";

                var enableGameIDs = MerchantGameSettingRepository.GetByFilter(gameFilter , selectColumns)
                    .Select(z => z.GameID)
                    .ToList();

                List<int> enableGameVendors = enableGameIDs.Select(gameid => AppHelper.GetGameVendorByGameID(gameid)).Distinct().ToList();

                if (filter.GameVendorTypes.IsEmptyOrNull())
                {
                    if (enableGameVendors.Contains(GameVendorTypeEnum.target.ToInteger()))
                    {
                        enableGameVendors.Add(GameVendorTypeEnum.Slot88.ToInteger());
                    }

                    filter.GameVendorTypes = enableGameVendors;
                }

                queryGames = GameListRepository.GetByFilter(filter);
            }

            List<int> gameIDs = queryGames.Where(z => z.GameID > 0).Select(z => z.GameID).ToList();

            var gameFullTextKeywordDic = GetAllGamesLanguageTextForKeywordSearch(gameIDs);

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs);
                      
            if (HasPermission_OnlyVisibleIDGGames(userID))
            {
                queryGames = queryGames
                    .Where(x => AppHelper.IsIDGVisibleGames(x.GameID))
                    .ToList();
            }

            var result = queryGames
                .Where(z => z.GameID > 0)
                .OrderBy(z => z.GameVendorType == GameVendorTypeEnum.Funky.ToInteger() ? 999 : 0)
                .ThenBy(z => z.GameID)
                .Select(game =>
                {
                    string text = gameNameDic.GetDictionaryOrDefaultValue(game.GameID, game.GameName);

                    text = $"{game.GameID}｜{text}";

                    var model = new DropdownValueViewModel
                    {
                        Text = text,
                        Value = game.GameID.ToString(),
                        ValueInt = game.GameID,
                        ExtraProperty = new
                        {
                            GameType = game.GameType,
                            GameVendor = GameVendorDefinitionHelper.ConverSlot88_To_PGS(game.GameVendorType),
                            IsSupportRTPConfig = GameVendorDefinitionHelper.IsSupportRTPConfig(game.GameVendorType),
                        }
                    };

                    if (gameFullTextKeywordDic.ContainsKey(game.GameID))
                    {
                        model.FullTextForKeywordSearch = gameFullTextKeywordDic[game.GameID];
                    }
                    else
                    {
                        model.FullTextForKeywordSearch = $"{game.GameID} {model.Text}";
                    }

                    return model;
                })
                .ToList();

            return result;
        }


        private Dictionary<int, string> GetAllGamesLanguageTextForKeywordSearch(List<int> gameIDs)
        {
            //cache 15分鐘不要太久。
            //但是稍微有誤也不至於很嚴重
            var myLang = HeaderValueHelper.GetCurrentLanguage();

            string cacheKey = CacheKey.AdminSys_GamesDropdownFullTextForKeywordSearch();

            var cacheValues = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                //因為要提供autoComplete關鍵字搜尋 需要全語系的文字
                List<LanguageEnum> targetLangs = new List<LanguageEnum>
                {
                    LanguageEnum.en_us,
                    LanguageEnum.zh_cn,
                    LanguageEnum.zh_tw,
                };

                Dictionary<LanguageEnum, Dictionary<int, string>> langMappingGameName = new Dictionary<LanguageEnum, Dictionary<int, string>>();

                foreach (var lang in targetLangs)
                {
                    langMappingGameName.Add(lang, DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs, lang));
                }

                Dictionary<int, string> gameIDMappingKeywordText = new Dictionary<int, string>();

                foreach (var gameID in gameIDs)
                {
                    string fullText = $"{gameID} {langMappingGameName[LanguageEnum.en_us].GetDictionaryOrDefaultValue(gameID).ToLower()}" +
                        $" {langMappingGameName[LanguageEnum.zh_tw].GetDictionaryOrDefaultValue(gameID)}" +
                        $" {langMappingGameName[LanguageEnum.zh_cn].GetDictionaryOrDefaultValue(gameID)}" +
                        $" {((GameVendorTypeEnum)AppHelper.GetGameVendorByGameID(gameID)).ToString().ToLower()}";

                    gameIDMappingKeywordText.Add(gameID, fullText);
                }

                return gameIDMappingKeywordText;

            }, expiredMinute: 15);

            return cacheValues ?? new Dictionary<int, string>();
        }


        public List<DropdownValueViewModel> GetServers(int userID, QueryServerListApiRequestParameterModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter = filter ?? new QueryServerListApiRequestParameterModel();
            filter.GameIDs = filter.GameID.ToIntegerListCollection();

            if (!currentUser.IsSuperAdminOrg)
            {
                List<int> merchantIDs = currentUser.AvailableMerchantIDs.ConvertLargeListToEmpty(2000);
                filter.ServerIDs = ServerMerchantMappingRepository.GetAvailableServersByMerchantIDs(merchantIDs);
            }

            var query = ServerListRepository.GetByFilter(filter);

            var result = query.Select(x => new DropdownValueViewModel
            {
                Text = x.ServerName,
                Value = x.ServerID.ToString(),
                ExtraProperty = new
                {
                    GameID = x.GameID,
                    Currency = x.Currency
                }
            })
                .ToList();

            return result;
        }


        public List<DropdownValueViewModel> GetTimeZoneAreas()
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<TimeZoneAreaEnum>();

            var result = allEnums
                .OrderBy(x => DataModelConvertHelper.GetTimeZoneByAreaID(x.ToInteger()))
                .Select(x =>
                {
                    var displayName = MultiLanguageTextHelper.GetTimeZoneDisplayName(x.ToInteger());
                    var convert = new DropdownValueViewModel
                    {
                        Text = displayName,
                        Value = x.ToIntegerString(),
                        ExtraProperty = new
                        {
                            TimeZone = DataModelConvertHelper.GetTimeZoneByAreaID(x.ToInteger())
                        }
                    };

                    return convert;
                })
                .ToList();

            return result;
        }



        public List<DropdownValueViewModel> GetAvailableRoles(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryRoles = RoleRepository.GetByFilter(new QueryRoleRequestViewModel
            {
                IsIncludeDeleteData = false
            });

            //如果非超級管理員 篩選roles
            if (!currentUser.IsSuperAdminOrg)
            {
                var queryAvailableRoles = OrganizationRoleMappingRepository.GetByFilter(new QueryOrganizationRoleMappingRequestViewModel
                {
                    OrganizationIDs = currentUser.OrganizationID.ToListCollection()
                })
                .Select(z => z.RoleID)
                .ToList();

                var queryMyExtraRoles = RoleCustomizedSettingRepository.GetByFilter(new QueryRoleCustomizedSettingRequestViewModel
                {
                    RoleIDs = currentUser.Roles,
                    SettingKeys = RoleCustomizedTypeEnum.ExtraAvailableRoles.ToInteger().ToListCollection()
                })
                    .FirstOrDefault();

                //擴增1. 可以無視組織分配。額外設定自己的roles
                bool isAllowSelectedSelfRole = HasTargetPermission(userID, "Special_CanSelectedSelfRoles");

                if (isAllowSelectedSelfRole)
                {
                    var myRoles = UserRoleMappingRepository.GetByUserID(userID);

                    queryAvailableRoles.AddRange(myRoles.Select(x => x.RoleID).ToList());
                }

                //擴增2. 透過roles setting 可以額外指定角色
                if (queryMyExtraRoles != null)
                {
                    var parse = new SystemConfigSettingParseModel(0, queryMyExtraRoles.SettingValue);

                    if (parse.SettingValueOfListInt.IsNotEmptyNull())
                    {
                        queryAvailableRoles.AddRange(parse.SettingValueOfListInt);
                    }
                }

                queryAvailableRoles = queryAvailableRoles.Distinct().ToList();

                queryRoles = queryRoles
                    .Where(x => queryAvailableRoles.Contains(x.RoleID))
                    .ToList();
            }

            var queryUserOwnPermission = GetCurrentOwnPermissionByCache(userID);

            return queryRoles
                .OrderBy(z => z.Sort)
                .Select(x => new DropdownValueViewModel
                {
                    Text = x.RoleName,
                    Value = x.RoleID.ToString(),
                    ValueInt = x.RoleID,
                })
                .ToList();
        }

        public List<DropdownValueViewModel> GetIMChannels(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryFilter = new QueryIMChannelRequestViewModel
            {
                IsIncludeDeleteData = false,
            };

            //如果org有特殊標記
            bool isQueryAllRichmondTeam = false;

            if (!currentUser.IsSuperAdminOrg)
            {
                var querySetting_richmondTeam = OrganizationCustomizedSettingRepository.GetParseModelByEnum(currentUser.OrganizationID, OrganizationCustomizedSettingKey.IsRichmondOrganization);

                bool queryDropdownPermission = HasTargetPermission(userID , "Special_Dropdown_ShowRichmondDropdowns");

                isQueryAllRichmondTeam = querySetting_richmondTeam.SettingValueOfBoolean || queryDropdownPermission;

                if (isQueryAllRichmondTeam)
                {
                    //**如果richmond 不可用一般方式篩選 如果channel尚未分配商戶 就會查不到
                    //下方有另一段程式碼用額外邏輯處理
                }
                else
                {
                    queryFilter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
                }
            }

            var queryChannels = IMChannelRepository.GetByFilter(queryFilter);

            var viewModels = queryChannels
                .Select(x => new DropdownValueViewModel
                {
                    Text = $"{x.IMChannelID}｜{x.DisplayName}",
                    Value = x.IMChannelID.ToString(),
                    ValueInt = x.IMChannelID
                })
                .ToList();

            if (!currentUser.IsSuperAdminOrg)
            {
                if (isQueryAllRichmondTeam)
                {
                    //查出 非richmond體系的頻道。排除之
                    var queryNoneRichmondChannels = GeneralTagMappingRepository.GetByTagID(GeneralTagEnum.NoneRichmondChannels.ToInteger());
                    var channelIDs = queryNoneRichmondChannels.Select(x => x.TargetID).ToList() ?? new List<int>();

                    viewModels = viewModels
                        .Where(x => !channelIDs.Contains(x.ValueInt.Value))
                        .ToList();
                }
            }

            return viewModels;
        }

        public List<DropdownValueViewModel> GetAvailableMerchants(int userID, QueryDropdownMerchantRequestViewModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var myFilter = new QueryMerchantRequestViewModel
            {
                OrganizationIDs = filter.OrganizationID.HasValue ? filter.OrganizationID.Value.ToListCollection() : new List<int>(),
                MerchantIDs = currentUser.SuperAdminOrAvailableMerchantIDs,
                IsIncludeDeleteData = false,
                Keyword = filter.Keyword
            };

            if (currentUser.IsHierarchyMerchantMode)
            {
                myFilter.HierarchyOfTargetMerchant = currentUser.ReferenceCreditMerchant;
            }

            //若有指定。會排除3級、4級代理 (垃圾資料太多)
            if (filter.IsExcludeSpecificAgent)
            {
                myFilter.IsExcludeLayer3Agents = true;
            }

            if (currentUser.IsMultipleHierarchyMode)
            {
                //如果是勾選多個root、代理商戶 則嘗試撈出所有下級代理
                myFilter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.AvailableMerchantIDs;
                myFilter.MerchantIDs = new List<int>();
            }

            string selectColumns = " MerchantID , Name , Currency , Type , CreateTime ";

            var queryMerchants = MerchantRepository.GetByFilter(myFilter, selectColumns);

            //精簡下拉選單 (商戶越來越多、但是有幽靈商戶)
            if (filter.IsSimplifyItems)
            {
                var aliveMerchants = GetAliveMerchantsByCache();

                var queryRootAgents = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    MaxLayer = 1
                })
                    .Select(x => x.MerchantID).ToList();

                DateTime days60 = DateTimeHelper.GetCurrentTime().AddDays(-1 * 60);

                //近期有資料 或 近期創建的商戶
                queryMerchants = queryMerchants
                    .Where(x => aliveMerchants.Contains(x.MerchantID) || x.CreateTime >= days60 || queryRootAgents.Contains(x.MerchantID))
                    .ToList();
            }

            List<DropdownValueViewModel> dropdownItems = new List<DropdownValueViewModel>();

            int fakeMerchantID = -1;

            string textTemplate = "{0}";

            bool isDisplayCategory = filter.IsIncludeCategory && currentUser.IsSuperAdminOrg;

            List<int> favoritMerchantIDs = new List<int>();

            if (isDisplayCategory)
            {
                var queryMerchantCategory = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
                {
                    OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.AdminSys_MerchantDropdownCategory.ToInteger()
                });

                var queryCategoryMerchants = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.AdminSys_CustomizedDropdownCategory.ToInteger().ToListCollection(),
                    SettingValueNotEqual = "0",
                    IsNotDeleteMerchant = true,
                });

                favoritMerchantIDs = queryCategoryMerchants.Select(z => z.MerchantID).ToList();

                List<string> categoryIDs = queryCategoryMerchants
                    .Select(x => x.SettingValue)
                    .Distinct()
                    .OrderBy(x =>
                    {
                        if (filter.CategoryFirstOrderID.HasValue)
                        {
                            if (filter.CategoryFirstOrderID.Value.ToString() == x)
                            {
                                return 0;
                            }
                            else
                            {
                                return 999;
                            }
                        }
                        else
                        {
                            return 0;
                        }
                    })
                    .ThenBy(x => x) //編號越前面的排越前
                    .ToList();

                //迴圈之前先篩選 減少一點複雜度
                var merchantsWithCateogry = queryMerchants
                    .Where(x => favoritMerchantIDs.Contains(x.MerchantID))
                    .ToList();


                //複製常用商戶
                if (filter.IsIncludeFavoritCopyMerchantCategory)
                {
                    var merchants = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.CopyMerchantSource_FavoritMerchants);

                    dropdownItems.Add(new DropdownValueViewModel("复制常用商户", "-5000"));

                    foreach (var item in merchants)
                    {
                        var matchMerchant = queryMerchants.FirstOrDefault(x => x.MerchantID == Int32.Parse(item.SettingValue));
                        if (matchMerchant != null)
                        {
                            dropdownItems.Add(ConvertToMerchantViewModel(matchMerchant, currentUser.IsSuperAdminOrg, filter));
                        }
                    }
                }

                foreach (string category in categoryIDs)
                {
                    var matchCategory = queryMerchantCategory.FirstOrDefault(x => x.SubCategory == category);

                    //插入分類項目的dropdown
                    if (matchCategory != null)
                    {
                        dropdownItems.Add(new DropdownValueViewModel(string.Format(textTemplate, matchCategory.SettingValue), Int32.Parse(matchCategory.SubCategory).ToString()));
                    }
                    else
                    {
                        dropdownItems.Add(new DropdownValueViewModel(string.Format(textTemplate, $"其他分類({category})"), fakeMerchantID.ToString()));
                    }

                    List<int> merchantsOfThisCategory = queryCategoryMerchants
                        .Where(x => x.SettingValue == category)
                        .Select(x => x.MerchantID)
                        .ToList();

                    var merchants = merchantsWithCateogry
                        .Where(x => merchantsOfThisCategory.Contains(x.MerchantID))
                        .OrderBy(x => x.Name)
                        .ThenBy(z => z.MerchantID)
                        .ToList();

                    //將此分類的merchants加入選單
                    foreach (var eachMerchant in merchants)
                    {
                        dropdownItems.Add(ConvertToMerchantViewModel(eachMerchant, currentUser.IsSuperAdminOrg, filter));
                    }
                }
            }

            var notFavoritMerchans = queryMerchants.Where(z => !favoritMerchantIDs.Contains(z.MerchantID)).OrderBy(z => z.MerchantID).ToList();

            var enumAll = ExtensionOfEnum.GetEnumAllValue<MerchantTypeEnum>();

            enumAll = enumAll.OrderBy(z =>
            {
                MerchantTypeEnum type = z;
                switch (type)
                {
                    case MerchantTypeEnum.Plaform_ProductionEnvironment:
                        return 100;

                    case MerchantTypeEnum.GameProvider_QATest:
                        return 200;

                    case MerchantTypeEnum.InternalTest:
                        return 300;

                    case MerchantTypeEnum.NoDataOrGhostPlatform:
                        return 350;

                    case MerchantTypeEnum.Plaform_DevelopEnvironment:
                        return 400;

                    default:
                        return 500 + z.ToInteger();
                }

            }).ToList();


            foreach (var item in enumAll)
            {
                fakeMerchantID = fakeMerchantID - 1;

                var thisMerchants = notFavoritMerchans.Where(z => z.Type == item.ToInteger()).OrderBy(z => z.MerchantID).ToList();

                if (thisMerchants.IsEmptyOrNull())
                    continue;

                string categoryText = string.Format(textTemplate, MultiLanguageTextHelper.GetEnumsCultureText(item));

                if (isDisplayCategory)
                {
                    dropdownItems.Add(new DropdownValueViewModel(categoryText, fakeMerchantID.ToString()));
                }

                foreach (var eachMerchant in thisMerchants)
                {
                    dropdownItems.Add(ConvertToMerchantViewModel(eachMerchant, currentUser.IsSuperAdminOrg, filter));
                }
            }

            //如果參數有指定要額外資訊
            if (filter.IsIncludeExtraInfo)
            {
                List<int> merchantIDs = dropdownItems.Where(z => z.ValueInt.HasValue).Select(z => z.ValueInt.Value).ToList();

                var queryGameCount = MerchantGameSettingRepository.GetEachMerchantEnableGameCount(merchantIDs);

                var queryExtraSettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    CustomizedSettingKeyIDs = new List<int>
                    {
                        MerchantCustomizedSettingKeyEnum.GameSetting_IsUseIframeGameUrl.ToInteger()
                    }
                });

                foreach (var item in dropdownItems)
                {
                    var match = queryGameCount.FirstOrDefault(z => z.MerchantID == item.ValueInt);

                    var matchIframe = queryExtraSettings.FirstOrDefault(z => z.MerchantID == item.ValueInt);

                    item.ExtraProperty = new
                    {
                        EnableGameCount = match == null ? 0 : match.TotalCount,
                        IsIframeEnable = matchIframe == null ? false : new MerchantCustomizedSettingModel(matchIframe.CustomizedSettingKey, matchIframe.SettingValue).SettingValueOfBoolean,
                    };
                }
            }

            //一般用戶 按照字母排序
            if (!currentUser.IsSuperAdminOrg)
            {
                if (currentUser.ReferenceCreditMerchant > 0)
                {
                    dropdownItems = dropdownItems
                        .OrderBy(x => x.ValueInt == currentUser.ReferenceCreditMerchant ? 0 : 999)
                        .ThenBy(z => z.Text)
                        .ToList();
                }
                else
                {
                    dropdownItems = dropdownItems.OrderBy(z => z.Text).ToList();
                }
            }

            return dropdownItems;
        }


        public List<DropdownValueViewModel> GetHierarchyMerchants(int userID, LazyCommonRequestModel filter)
        {
            filter = filter ?? new LazyCommonRequestModel();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryFilter = new QueryMerchantHierarchyRequestViewModel();

            if (!currentUser.IsSuperAdminOrg)
            {
                if (currentUser.IsHierarchyMerchantMode)
                {
                    queryFilter.HierarchyOfTargerMerchant = currentUser.ReferenceCreditMerchant;
                }
                else
                {
                    queryFilter.MerchantIDs = currentUser.AvailableMerchantIDs;
                }
            }

            if (filter.ParentMerchantID > 0)
            {
                queryFilter.ParentMerchantID = filter.ParentMerchantID;
            }

            if (filter.HierarchyOfTargerMerchant > 0)
            {
                queryFilter.HierarchyOfTargerMerchant = filter.HierarchyOfTargerMerchant;
            }

            var query = MerchantHierarchyRepository.GetByFilter(queryFilter, " MerchantID , Hierarchy ");

            List<int> merchantIDs = query.Select(z => z.MerchantID).ToList();

            string selectColumns = " MerchantID , Name , Status ";

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs, selectColumns)
                .Where(x => x.Status != (int)NormalActiveStatus.PermanentDelete)
                .ToList();

            var hierarchyDic = query.ToDictionary(x => x.MerchantID, z => z);

            if (filter.MaxLayer.HasValue)
            {
                int availableSelectedMaxLayer = filter.MaxLayer.Value;

                //如果當前登入是一般代理帳號 要'額外'限制可選的層級數 
                //不得超過自己總代所限制的最高層級
                if (currentUser.IsHierarchyMerchantMode)
                {
                    var queryMyHierarchy = MerchantHierarchyRepository.GetById(currentUser.ReferenceCreditMerchant);

                    var maxLayerSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMyHierarchy.RootMerchantID, MerchantCustomizedSettingKeyEnum.System_AgentHierarchyMaxLayer);

                    availableSelectedMaxLayer = maxLayerSetting.SettingValueOfInt - 1;
                }
                else
                {
                    //若是管理員則不限制層級數
                }

                queryMerchants = queryMerchants
                    .Where(x => AppHelper.GetAgentLayerLevelByHierarchy(hierarchyDic[x.MerchantID].Hierarchy) <= availableSelectedMaxLayer)
                    .ToList();
            }

            var viewModels = queryMerchants.Select(z =>
                {
                    var matchHierarchy = hierarchyDic[z.MerchantID];

                    var convert = new DropdownValueViewModel
                    {
                        Text = $"{z.MerchantID}｜{z.Name}",
                        Value = z.MerchantID.ToString(),
                        ValueInt = z.MerchantID,
                        Order = (matchHierarchy.Hierarchy ?? "").Length
                    };

                    return convert;
                })
                .ToList();

            viewModels = viewModels
                    .OrderBy(x => x.ValueInt == currentUser.ReferenceCreditMerchant ? 0 : 999)
                    .ThenBy(x => x.Order) //階層越高排越前面 (字數越少)
                    .ThenBy(z => z.Text)
                    .ToList();

            return viewModels;
        }

        private DropdownValueViewModel ConvertToMerchantViewModel(MerchantEntity queryMerchant, bool isSuperAdmin, QueryDropdownMerchantRequestViewModel filter)
        {
            //超級管理組織 會顯示merchantID
            string displayText = $"{queryMerchant.MerchantID}｜{queryMerchant.Name}";

            if (filter.IsIncludeCurrency)
            {
                displayText = displayText + "｜" + queryMerchant.Currency;
            }

            var convert = new DropdownValueViewModel
            {
                Text = displayText,
                Value = queryMerchant.MerchantID.ToString(),
                ValueInt = queryMerchant.MerchantID,
                FullTextForKeywordSearch = displayText,
                ExtraProperty = new
                {
                    Currency = queryMerchant.Currency,
                    IsSingleWalletMode = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode)
                }
            };

            return convert;
        }


        public List<DropdownValueViewModel> GetInternalMerchants(int userID)
        {
            var querySettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant.ToInteger().ToListCollection(),
                SettingValueMatch = "1"
            });

            if (querySettings.IsEmptyOrNull())
                return new List<DropdownValueViewModel>();

            List<int> merchantIDs = querySettings.Select(z => z.MerchantID).ToList();

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

            return queryMerchants
                .Where(x => x.Status != NormalActiveStatus.PermanentDelete.ToInteger())
                .Select(x => new DropdownValueViewModel
                {
                    Text = $"{x.MerchantID}｜{x.Name}",
                    Value = x.MerchantID.ToString()
                })
                .ToList();
        }

        public List<DropdownValueViewModel> GetMerchantsOfAllowCreateTestAccount(int userID)
        {
            var querySettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.IsAllowCreateTestAccount.ToInteger().ToListCollection(),
                SettingValueMatch = "1"
            });

            if (querySettings.IsEmptyOrNull())
                return new List<DropdownValueViewModel>();

            List<int> merchantIDs = querySettings.Select(z => z.MerchantID).ToList();

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

            return queryMerchants
                .Where(x => x.Status != NormalActiveStatus.PermanentDelete.ToInteger())
                .Select(x => new DropdownValueViewModel
                {
                    Text = $"{x.MerchantID}｜{x.Name}",
                    Value = x.MerchantID.ToString()
                })
                .ToList();
        }


        public List<DropdownValueViewModel> GetAvailableOrgs(int userID)
        {
            var queryUserOwnPermission = GetCurrentOwnPermissionByCache(userID);

            var filter = new QueryOrganizationRequestViewModel
            {
                OrganizationIDs = queryUserOwnPermission.IsSuperAdminOrg ? new List<int>() : queryUserOwnPermission.OrganizationID.ToListCollection()
            };

            var query = OrganizationRepository.GetByFilter(filter);

            var userCountQuery = UserRepository.GetEachOrganizationUserCount();

            return query
                .OrderBy(x => x.Sort)
                .Select(x => new DropdownValueViewModel
                {
                    Text = x.OrganizationName,
                    Value = x.OrganizationID.ToString(),
                    ValueInt = x.OrganizationID,
                    ExtraProperty = new
                    {
                        OrganizationCode = x.OrganizationCode,
                        UserCount = userCountQuery.FirstOrDefault(c => c.OrganizationID == x.OrganizationID)?.TotalCount ?? 0
                    }
                })
                .ToList();
        }

        public List<DropdownValueViewModel> GetCurrency(int userID)
        {
            var queryCurrency = ExtensionOfEnum.GetEnumAllValue<CurrencyEnum>();

            List<DropdownValueViewModel> enumValues = new List<DropdownValueViewModel>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                string selectColumns = " distinct Currency ";

                var query = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
                {
                    FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs

                }, selectColumns);

                List<string> availableCurrency = query.Select(x => x.Currency).ToList();

                queryCurrency = queryCurrency
                    .Where(c => availableCurrency.Contains(c.ToString()))
                    .ToList();
            }
            
            enumValues = queryCurrency.Select(x => new DropdownValueViewModel
                {
                    Text = $"{x.ToString()}｜" + MultiLanguageTextHelper.GetEnumsCultureText(x),
                    Value = x.ToString(),
                })
                .ToList();

            return enumValues;
        }

        public List<DropdownValueViewModel> GetEventCounterOrderByColumn()
        {
            var queryAll = ExtensionOfEnum.GetEnumAllValue<EventCounterOrderByEnum>();

            var enumValues = queryAll.Select(z => new DropdownValueViewModel
            {
                Text = MultiLanguageTextHelper.GetEnumsCultureText(z),
                Value = z.GetHashCode().ToString(),
                ValueInt = z.GetHashCode(),
                ExtraProperty = new
                {
                    OrderByColumn = z.GetAttributeFromEnum<ExtraAttributeValue>()?.Value
                }
            }).ToList();

            return enumValues;
        }

        public List<DropdownValueViewModel> GetMerchantSpecialFilterTypes()
        {
            var enumValues = GetByEnumAndLanguageText<SpecialMerchantFilterType>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetStaReportSearchColumns(int userID, string reportType)
        {
            var enumValues = new List<StatReportSearchColumnType>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            switch (reportType)
            {
                case "merchant-summary-report":
                    enumValues.Add(StatReportSearchColumnType.MerchantSummry_IncludeBasicInfo);
                    enumValues.Add(StatReportSearchColumnType.MerchantSummry_IncludeAdvanceInfo);
                    break;

                case "agent-total-report":

                    enumValues = ExtensionOfEnum.GetEnumAllValue<StatReportSearchColumnType>()
                        .Where(x =>
                        {
                            var category = x.GetAttributeFromEnum<DataCategoryAttribute>();
                            return category != null && category.Value == DataCategoryEnum.ExtraFilterType_AgentReport;
                        })
                        .Select(x => x)
                        .ToList();

                    break;

                case "game-report":

                    enumValues = ExtensionOfEnum.GetEnumAllValue<StatReportSearchColumnType>()
                        .Where(x =>
                        {
                            var category = x.GetAttributeFromEnum<DataCategoryAttribute>();
                            return category != null && category.Value == DataCategoryEnum.ExtraFilterType_GameReport;
                        })
                        .Select(x => x)
                        .ToList();

                    break;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }

            var convert = ConvertEnumsToDropdownModel(enumValues);
            return convert;
        }


        public List<DropdownValueViewModel> GetMerchantTypes()
        {
            var enumValues = GetByEnumAndLanguageText<MerchantTypeEnum>();
            return enumValues;
        }

        public List<DropdownValueViewModel> GetOneToManyTypes()
        {
            var enumValues = GetByEnumAndLanguageText<OneToManyCustomizedSettingTypeEnum>(isDisplayIdNumber: true);
            return enumValues;
        }

        public List<DropdownValueViewModel> GetOneTimesJobTypes()
        {
            var enumValues = GetByEnumAndLanguageText<OneTimesJobTypeEnum>();
            return enumValues;
        }

        public List<DropdownValueViewModel> GetMerchantCopyExtraSettingEnum()
        {
            var enumValues = GetByEnumAndLanguageText<MerchantCopyExtraSettingEnum>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetInvoiceExportCategory()
        {
            //某些邏輯會參考這個列舉
            var referEnum = AccountingInvoiceExportCategoryEnum.Vworld_And_Agent;

            var query = AccountingInvoiceCategoryRepository.GetByFilter(new QueryAccountingInvoiceCategoryRequestViewModel());

            return query.Select(x => new DropdownValueViewModel
            {
                Text = x.Name,
                Value = x.AccountingInvoiceCategoryID.ToString(),
                ValueInt = x.AccountingInvoiceCategoryID,
            })
                .ToList();
        }


        public List<DropdownValueViewModel> GetLanguage()
        {
            var enumValues = GetByEnumAndLanguageText<LanguageEnum>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetLanguageCodes()
        {
            var enumValues = ExtensionOfEnum.GetEnumAllValue<LanguageEnum>();
            return enumValues.Select(x => new DropdownValueViewModel
            {
                Text = MultiLanguageTextHelper.GetEnumsCultureText(x),
                Value = x.ToDashLangCode()
            }).
            ToList();
        }


        public List<DropdownValueViewModel> GetRoomLevels()
        {
            var enumValues = GetByEnumAndLanguageText<GameRoomFlagLevelEnum>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetTransactionTypes()
        {
            var enumValues = GetByEnumAndLanguageText<TransactionTypeEnum>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetGameVendorTransferTypes()
        {
            List<TransactionTypeEnum> types = new List<TransactionTypeEnum>
            {
                TransactionTypeEnum.TransferWallet_To_GameWallet,
                TransactionTypeEnum.TransferWallet_To_PlatformWallet,
                TransactionTypeEnum.GameVendorTransferWallet_To_GameVendorWallet,
                TransactionTypeEnum.GameVendorTransferWallet_To_PgsWallet,
            };

            return types.Select(x => new DropdownValueViewModel
            {
                Text = MultiLanguageTextHelper.GetEnumsCultureText(x),
                Value = x.ToIntegerString(),
            })
               .ToList();
        }


        public List<DropdownValueViewModel> GetAgentAggregativeType()
        {
            var queryAll = ExtensionOfEnum.GetEnumAllValue<AgentAggregativeTypeEnum>();

            return queryAll.Select(x => new DropdownValueViewModel
            {
                Text = MultiLanguageTextHelper.GetEnumsCultureText(x),
                Value = x.ToIntegerString(),
            })
            .ToList();
        }


        public List<DropdownValueViewModel> GetDynamicQueryItems()
        {
            var query = DynamicQueryCommandRepository.GetAll();

            return query
                .Where(z => z.IsVisible)
                .Select(x =>
                {
                    var convert = new DropdownValueViewModel
                    {
                        Text = x.DisplayName,
                        Value = x.Id.ToString(),
                    };

                    return convert;
                })
                .ToList();
        }


        public List<DropdownValueViewModel> GetInternalTestPlayerAccounts()
        {
            var queryPlayersTags = PlayerTagMappingRepository.GetByFilter(new QueryPlayerTagMappingRequestViewModel
            {
                TagIDs = SystemTagEnum.Player_FrequentUseTestAccount.ToInteger().ToListCollection()
            });

            if (queryPlayersTags.IsEmptyOrNull())
                return new List<DropdownValueViewModel>();

            List<int> playerIDs = queryPlayersTags.Select(z => z.PlayerID).ToList();

            var queryPlayers = PlayerRepository.GetByIds(playerIDs);

            List<int> merchantIDs = queryPlayers.Select(z => z.MerchantID).Distinct().ToList();

            List<int> internalMerchants = MerchantCustomizedSettingRepository.GetInternalTestMerchantIDs();

            var queryMerchantSettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantIDs,
                CustomizedSettingKeyIDs = new List<int>
                {
                    MerchantCustomizedSettingKeyEnum.GameSetting_IsUseIframeGameUrl.ToInteger(),
                    MerchantCustomizedSettingKeyEnum.GameSetting_IsAutoGenerateBackurl.ToInteger(),
                    MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant.ToInteger(),
                    MerchantCustomizedSettingKeyEnum.GameSetting_LobbyGameOpenMode.ToInteger(),
                    MerchantCustomizedSettingKeyEnum.System_IsAlwaysTrialMode.ToInteger(),
                }
            });

            var errorSimulate = CheckAnySimulateValue();

            var queryOtherTags = PlayerTagMappingRepository.GetByFilter(new QueryPlayerTagMappingRequestViewModel
            {
                PlayerIDs = playerIDs,
                TagIDs = new List<int>
                    {
                        SystemTagEnum.Player_AllowSkipMaintain.ToInteger()
                    }
            });

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

            var queryHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                MerchantIDs = merchantIDs
            });

            return queryPlayers
                .OrderBy(z => z.MerchantID)
                .Select(eachPlayer =>
                {
                    string merchantName = MerchantRepository.GetSingleByCache(eachPlayer.MerchantID)?.Name;

                    //如果是內部測試用商戶 會標記星號
                    if (internalMerchants.Contains(eachPlayer.MerchantID))
                    {
                        merchantName = "*" + merchantName;
                    }

                    var tagsOfPlayers = queryOtherTags.Where(p => p.PlayerID == eachPlayer.PlayerID).ToList();

                    var queryMySettings = queryMerchantSettings
                        .Where(my => my.MerchantID == eachPlayer.MerchantID)
                        .ToList();

                    var gameOpenMode = queryMySettings
                                .Where(setting => setting.MerchantID == eachPlayer.MerchantID)
                                .Where(setting => setting.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.GameSetting_LobbyGameOpenMode)
                                .Select(setting => new SystemConfigSettingParseModel(setting.CustomizedSettingKey, setting.SettingValue))
                                .FirstOrDefault();

                    string displayName = $"{eachPlayer.PlayerID}｜{merchantName.Truncate(30, "..")}｜{eachPlayer.PlayerName}";
                    var convert = new DropdownValueViewModel
                    {
                        Text = displayName,
                        Value = eachPlayer.PlayerID.ToString(),
                        ValueInt = eachPlayer.PlayerID,
                        ExtraProperty = new
                        {
                            isAnySimulateEnable = errorSimulate.isAnySimulateEnable,
                            isSimulateDelayResponse = errorSimulate.isSimulateDelayResponse,
                            isSimulateErrorRate = errorSimulate.isSimulateErrorRate,
                            isSimulateSingleWallet = errorSimulate.isSimulateSingleWallet,
                            simulateGameVendorName = errorSimulate.SimulateGameVendor,
                            isHierarchyMerchantMode = queryHierarchy.Any(q => q.MerchantID == eachPlayer.MerchantID),
                            isPlayerAllowSkipMaintain = tagsOfPlayers.Any(q => q.TagID == (int)SystemTagEnum.Player_AllowSkipMaintain),
                            isGameOpenNewTab = gameOpenMode == null ? false : gameOpenMode.SettingValueOfInt == GameOpenModeEnum.NewTab.ToInteger(),
                            isIframeEnable = IsSettingEnable(queryMySettings, eachPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsUseIframeGameUrl),
                            isLobbyBackUrl = IsSettingEnable(queryMySettings, eachPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsAutoGenerateBackurl),
                            isAlwaysTrialMode = IsSettingEnable(queryMySettings, eachPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.System_IsAlwaysTrialMode),
                            isInternalTestMerchant = IsSettingEnable(queryMySettings, eachPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant),
                            isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchants.FirstOrDefault(z => z.MerchantID == eachPlayer.MerchantID).WalletMode),
                        }
                    };

                    return convert;
                })
                .ToList();
        }

        private (bool isAnySimulateEnable, bool isSimulateSingleWallet, bool isSimulateErrorRate, bool isSimulateDelayResponse, string SimulateGameVendor) CheckAnySimulateValue()
        {
            var queryErrorSimulateSettings = SystemConfigSettingRepository.GetByFilter(new QuerySystemConfigSettingRequestViewModel
            {
                IDs = new List<int>
                {
                    SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet.ToInteger(),
                    SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle.ToInteger(),
                    SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi.ToInteger(),
                    SystemConfigSettingEnum.FakeSingleWalletApi_DelaySeconds_PlaceBet.ToInteger(),
                    SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_PlaceBet.ToInteger(),
                    SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_Settlement.ToInteger(),
                    SystemConfigSettingEnum.ErrorSimulate_TargetGameVendor.ToInteger(),
                    SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout.ToInteger(),
                    SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout.ToInteger(),
                    SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout.ToInteger(),
                }
            });

            bool isSimulateAnyGameVendor = IsSimulateValueEnable(queryErrorSimulateSettings, SystemConfigSettingEnum.ErrorSimulate_TargetGameVendor, 100000);

            if (!isSimulateAnyGameVendor)
            {
                return (false, false, false, false, "");
            }

            var gameVendor = queryErrorSimulateSettings.FirstOrDefault(x => x.ID == SystemConfigSettingEnum.ErrorSimulate_TargetGameVendor.ToInteger());

            var gameVendorEnum = (GameVendorTypeEnum)new SystemConfigSettingParseModel(gameVendor.ID, gameVendor.SettingValue).SettingValueOfInt;

            bool isSimulateDelayResponse = IsSimulateValueEnable(queryErrorSimulateSettings, new List<SystemConfigSettingEnum>
                {
                    SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout,
                    SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout,
                    SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout
                }, 0);


            bool isSimulateErrorRate = IsSimulateValueEnable(queryErrorSimulateSettings, new List<SystemConfigSettingEnum>
                {
                    SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi,
                    SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet,
                    SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle
                }, 0);


            bool isSimulateErrorForSingleWallet = IsSimulateValueEnable(queryErrorSimulateSettings, new List<SystemConfigSettingEnum>
                {
                    SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_PlaceBet,
                    SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_Settlement,
                    SystemConfigSettingEnum.FakeSingleWalletApi_DelaySeconds_PlaceBet,
                }, 0);

            bool isAnyEnable = isSimulateErrorForSingleWallet || isSimulateErrorRate || isSimulateDelayResponse;
            return (isAnyEnable, isSimulateErrorForSingleWallet, isSimulateErrorRate, isSimulateDelayResponse, gameVendorEnum.ToString());
        }

        private bool IsSimulateValueEnable(List<SystemConfigSettingEntity> querySettings, SystemConfigSettingEnum settingKey, int mustLargeValue)
        {
            var match = querySettings.FirstOrDefault(x => x.ID == settingKey.ToInteger());

            if (match == null)
                return false;

            var parseModel = new SystemConfigSettingParseModel(match.ID, match.SettingValue);
            return parseModel.SettingValueOfInt > mustLargeValue;
        }


        private bool IsSimulateValueEnable(List<SystemConfigSettingEntity> querySettings, List<SystemConfigSettingEnum> settingKeys, int mustLargeValue)
        {
            List<bool> booleanList = new List<bool>();

            foreach (var setting in settingKeys)
            {
                var isEnable = IsSimulateValueEnable(querySettings, setting, mustLargeValue);

                if (isEnable)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSettingEnable(List<MerchantCustomizedSettingEntity> settings, int merchantID, MerchantCustomizedSettingKeyEnum settingKey)
        {
            var match = settings
                .Where(z => z.MerchantID == merchantID)
                .Where(z => z.CustomizedSettingKey == settingKey.ToInteger())
                .FirstOrDefault();

            if (match == null)
            {
                return false;
            }

            return match.SettingValue == NormalYesOrNoEnum.Yes.ToIntegerString();
        }


        public List<DropdownValueViewModel> GetBetRecordStatus()
        {
            var enumValues = GetByEnumAndLanguageText<BetRecordStatusEnum>();
            return enumValues;
        }

        public List<DropdownValueViewModel> GetEventPrizeRecordStatusEnum()
        {
            var enumValues = GetByEnumAndLanguageText<EventPrizeRecordStatusEnum>();
            return enumValues;
        }

        public List<DropdownValueViewModel> GetPerGameTags(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryEnums = ExtensionOfEnum.GetEnumAllValue<SystemTagEnum>();

            queryEnums = queryEnums.Where(z =>
                {
                    var dataCategory = z.GetAttributeFromEnum<DataCategoryAttribute>();

                    if (dataCategory == null)
                        return false;

                    return dataCategory.Value == DataCategoryEnum.PerGameLog;
                })
                .ToList();

            //前端選擇後。會使用特殊的查詢方式
            queryEnums.Add(SystemTagEnum.SpecialTag_AccountingIgnorePerGameLogs);

            //非管理員 隱藏特定標籤
            if (!currentUser.IsSuperAdminOrg)
            {
                queryEnums = new List<SystemTagEnum>
                {
                    SystemTagEnum.PerGameDetailLog_FreeGame,
                    SystemTagEnum.PerGameDetailLog_Jackpot,
                    SystemTagEnum.PerGameDetailLog_PayoutRate_OverThan50,
                    SystemTagEnum.PerGameDetailLog_PayoutRate_OverThan100,
                    SystemTagEnum.PerGameDetailLog_AGGBuyFeature,
                };
            }

            var dropdowns = queryEnums
                .Select(x =>
                {
                    int tagID = x.ToInteger();

                    string text = MultiLanguageTextHelper.GetEnumsCultureText(x);

                    var myModel = new DropdownValueViewModel
                    {
                        Text = currentUser.IsSuperAdminOrg ? $"{tagID}｜{text}" : text,
                        Value = tagID.ToString(),
                        ValueInt = tagID
                    };

                    return myModel;
                })
               .ToList();

            return dropdowns;

        }

        /// <summary>
        /// 允許手動添加的tags
        /// </summary>
        public List<DropdownValueViewModel> GetPerGameTagsAllowManualAdd()
        {
            var queryEnums = new List<SystemTagEnum>
            {
                SystemTagEnum.PerGameDetailLog_DevelopeSpecialTag,
                SystemTagEnum.PerGameDetailLog_ThirdSettleDateNotEqual,
                SystemTagEnum.PerGameDetailLog_AccountingIgnoreAtThirdFinanceCompare,
            };

            return queryEnums.Select(x => new DropdownValueViewModel
            {
                Text = MultiLanguageTextHelper.GetEnumsCultureText(x),
                Value = x.ToIntegerString(),
                ValueInt = x.ToInteger()
            })
           .ToList();
        }


        public List<DropdownValueViewModel> GetSystemTags(int userID, DataCategoryEnum dateCategory)
        {
            var queryEnums = ExtensionOfEnum.GetEnumAllValue<SystemTagEnum>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            queryEnums = queryEnums.Where(z =>
                {
                    var dataCategory = z.GetAttributeFromEnum<DataCategoryAttribute>();

                    if (dataCategory == null)
                        return false;

                    return dataCategory.Value == dateCategory;
                })
                .ToList();

            return queryEnums.Select(x =>
            {
                var myModel = new DropdownValueViewModel
                {
                    Text = MultiLanguageTextHelper.GetEnumsCultureText(x),
                    Value = x.ToIntegerString(),
                    ValueInt = x.ToInteger()
                };

                if (currentUser.IsSuperAdminOrg)
                {
                    myModel.Text = $"{x.ToInteger()}｜{myModel.Text}";
                }

                return myModel;
            })
           .ToList();
        }



        public List<DropdownValueViewModel> GetPlayerTags(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryEnums = ExtensionOfEnum.GetEnumAllValue<SystemTagEnum>();

            if (!currentUser.IsSuperAdminOrg)
            {
                //非超級組織 僅顯示定義好的tags
                queryEnums = queryEnums
                    .Where(x => x == SystemTagEnum.Player_FrequentUseTestAccount)
                    .Where(x => x == SystemTagEnum.Player_MerchantCreateProductionAccountForUs)
                    .Where(x => x == SystemTagEnum.Player_ForbiddenRedEnvelopeEvent)
                    .Where(x => x == SystemTagEnum.Player_ForbiddenRTPEvent)
                    .ToList();
            }

            queryEnums = queryEnums.Where(z =>
                {
                    var dataCategory = z.GetAttributeFromEnum<DataCategoryAttribute>();

                    if (dataCategory == null)
                        return false;

                    return dataCategory.Value == DataCategoryEnum.PlayerTag;
                })
                .ToList();

            return queryEnums.Select(x => new DropdownValueViewModel
            {
                Text = MultiLanguageTextHelper.GetEnumsCultureText(x),
                Value = x.ToIntegerString(),
                ValueInt = x.ToInteger()
            })
           .ToList();
        }

        public List<DropdownValueViewModel> GetAllowManualAddPlayerTags(int userID)
        {
            var tags = GetPlayerTags(userID);

            tags = tags
                .Where(x => x.ValueInt != (int)SystemTagEnum.Player_SuspiciousWarning)
                .Where(x => x.ValueInt != (int)SystemTagEnum.Player_PlayerBehaviorEvent)
                .Where(x => x.ValueInt != (int)SystemTagEnum.Player_RiskScore)
                .ToList();

            return tags;
        }


        public List<DropdownValueViewModel> GetUsers(int userID, LazyCommonRequestModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryEnums = ExtensionOfEnum.GetEnumAllValue<SystemTagEnum>();

            var myFilter = new QueryUserRequestViewModel
            {
                OrganizationType = filter.OrganizationType,
            };

            if (!currentUser.IsSuperAdminOrg)
            {
                myFilter.OrganizationIDs = currentUser.OrganizationID.ToListCollection();
                myFilter.FilterReferenceCreditMerchantIDByHierarchyMerchantID = currentUser.ReferenceCreditMerchant;
            }

            var queryUsers = UserRepository.GetByFilter(myFilter);

            return queryUsers.Select(x => new DropdownValueViewModel
            {
                Text = x.UserName,
                Value = x.UserID.ToString(),
                ValueInt = x.UserID,
                ExtraProperty = new
                {
                    OrganizationCode = x.UserName.Split('-').FirstOrDefault(),
                }
            })
           .ToList();
        }



        public List<DropdownValueViewModel> GetEvents(int userID, LazyCommonRequestModel filter)
        {
            filter = filter ?? new LazyCommonRequestModel();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var myfilter = new QueryEventRequestViewModel
            {
                IsIncludeDeleteData = false
            };

            if (!currentUser.IsSuperAdminOrg)
            {
                myfilter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
            }

            if (filter.Type > 0)
            {
                myfilter.EventTypes = filter.Type.ToListCollection();
            }

            myfilter.EventTemplateType = filter.EventTemplateType;

            var queryEvents = EventRepository.GetByFilter(myfilter);

            //非超級管理員 隱藏RTP甜蜜期。避免被看到、造成一些爭議
            if (!currentUser.IsSuperAdminOrg)
            {
                queryEvents = queryEvents
                    .Where(x => x.EventType != (int)EventTypeEnum.RTPEvent)
                    .ToList();
            }

            return queryEvents
                .OrderByDescending(z => z.EndTime)
                .Select(x => new DropdownValueViewModel
                {
                    Text = DataModelAndDbValueConvertHelper.GetEventDisplayName(x),
                    Value = x.EventID.ToString(),
                })
                .ToList();
        }

        public List<DropdownValueViewModel> GetEventTypes()
        {
            var enumValues = GetByEnumAndLanguageText<EventTypeEnum>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetIPAddressTemplate(LazyCommonRequestModel filter)
        {
            var query = IPAddressListTemplateRepository.GetByFilter(new QueryIPAddressListTemplateRequestViewModel
            {

            });

            return query
                .Select(x => new DropdownValueViewModel
                {
                    Text = $"{x.Id}｜" + x.DisplayName,
                    Value = x.Id.ToString(),
                    ValueInt = x.Id,
                })
                .ToList(); ;
        }


        public List<DropdownValueViewModel> GetGeneralTags(LazyCommonRequestModel filter)
        {
            var myFilter = new QueryGeneralTagRequestViewModel
            {

            };

            if (filter.Type > 0)
            {
                myFilter.Types = filter.Type.ToListCollection();
            }

            var query = GeneralTagRepository.GetByFilter(myFilter);

            var items = query
                .OrderBy(x => x.Name.Contains("test") ? 0 : 999) //測試的優先排前面。避免恍神選錯
                .Select(x => new DropdownValueViewModel
                {
                    Text = x.Name,
                    Value = x.TagID.ToString(),
                    ValueInt = x.TagID,
                })
                .ToList();

            if (filter.Type == TagTypeEnum.IMChannel.ToInteger())
            {
                List<int> tagIDs = query.Select(x => x.TagID).ToList();

                var queryMappingCount = GeneralTagMappingRepository.GetEachTagMappingCount(tagIDs);

                foreach (var item in items)
                {
                    var match = queryMappingCount.FirstOrDefault(x => x.OwnerID == item.ValueInt.Value);
                    item.ExtraProperty = new
                    {
                        MappingCount = match == null ? 0 : match.TotalCount
                    };
                }
            }

            return items;
        }


        public List<DropdownValueViewModel> GetDropdownsByEnum<T>() where T : System.Enum
        {
            var enumValues = GetByEnumAndLanguageText<T>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetAllowManualAddEventSettings()
        {
            List<EventCustomizedSettingKeyEnum> settings = new List<EventCustomizedSettingKeyEnum>
            {
                EventCustomizedSettingKeyEnum.ReferenceContentEventID,
                EventCustomizedSettingKeyEnum.EventContentBrandingType
            };

            return ConvertEnumsToDropdownModel(settings);
        }


        public List<DropdownValueViewModel> GetEventContentColorType()
        {
            var enumValues = GetByEnumAndLanguageText<EventContentColorStypeType>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetEventTemplateTypeEnum()
        {
            var enumValues = GetByEnumAndLanguageText<EventTemplateTypeEnum>();

            enumValues = enumValues
                .Where(x => x.ValueInt != (int)EventTemplateTypeEnum.QuestEvent_AllInOne_GetLuckWheelTicket_10_Quest)
                .Where(x => x.ValueInt != (int)EventTemplateTypeEnum.QuestEvent_DailyQuest_AllInOne_GetLuckWheelTicket_10_Quest)
                .Where(x => x.ValueInt != (int)EventTemplateTypeEnum.QuestEvent_DailyQuest_SpecificGame_GetLuckWheelTicket_10_Quest)
                .Where(x => x.ValueInt != (int)EventTemplateTypeEnum.QuestEvent_SpecificGame_GetLuckWheelTicket_10_Quest)
                .ToList();

            return enumValues;
        }


        public List<DropdownValueViewModel> GetNormalActiveStatus(int userID)
        {
            var enumValues = GetByEnumAndLanguageText<NormalActiveStatus>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                enumValues = enumValues
                    .Where(x => x.ValueInt != (int)NormalActiveStatus.DisableAndForbiddenEdit)
                    .ToList();
            }

            return enumValues;
        }

        public List<DropdownValueViewModel> GetEnableDisableStatus()
        {
            var enums = new List<NormalActiveStatus>
            {
                NormalActiveStatus.Enable,
                NormalActiveStatus.Disable
            };

            var models = ConvertEnumsToDropdownModel(enums);
            return models;
        }

        public List<DropdownValueViewModel> GetNormalYesOrNoOrNorSelectEnum()
        {
            var enumValues = GetByEnumAndLanguageText<NormalYesOrNoOrNorSelectEnum>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetAGGFishMoneyRecordTypeEnum()
        {
            var enumValues = GetByEnumAndLanguageText<AGGFishMoneyRecordTypeEnum>();
            return enumValues;
        }

        public List<DropdownValueViewModel> GetAGGFreespinStatusEnum(int userID)
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<AGGFreespinStatusEnum>();

            var items = allEnums
                .Where(x=> x != AGGFreespinStatusEnum.Undefined)
                .Select(x => new DropdownValueViewModel
                {
                    Text = MultiLanguageTextHelper.GetEnumsCultureText(x),
                    Value = x.ToString(),
                })
             .ToList();

            return items;
        }


        public List<DropdownValueViewModel> GetEventContentLayoutType()
        {
            var enumValues = GetByEnumAndLanguageText<EventContentLayoutTypeEnum>();
            return enumValues;
        }


        public List<DropdownValueViewModel> GetVisibleStatusEnum()
        {
            var queryAll = ExtensionOfEnum.GetEnumAllValue<VisibleStatusEnum>()
                .Where(z => z != VisibleStatusEnum.PermanentDelete)
                .ToList();

            return queryAll.Select(x => new DropdownValueViewModel
            {
                Text = MultiLanguageTextHelper.GetEnumsCultureText(x),
                Value = x.ToIntegerString(),
                ValueInt = x.ToInteger()
            })
            .ToList();
        }


        public List<DropdownValueViewModel> GetEventPrizeType(int userID)
        {
            var enumValues = GetByEnumAndLanguageText<EventPrizeTypeEnum>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                enumValues = enumValues
                    .Where(x => EventDefinitionHelper.PrizeTypes_AdminSysAPI_PublicAPI_VisiblePrizeTypes().Contains(x.ValueInt.Value))
                    .ToList();
            }

            enumValues = enumValues
                .Where(z => z.ValueInt != EventPrizeTypeEnum.Ticket_RedEnvelope.ToInteger()) //這種先過濾掉
                .Where(z => z.ValueInt != EventPrizeTypeEnum.Default.ToInteger()) //這種先過濾掉
                .ToList();

            //隱藏一些已經淘汰的
            if(SettingHelper.IsProduction)
            {
                enumValues = enumValues
                    .Where(z => z.ValueInt != EventPrizeTypeEnum.SendDrawPointOfLuckyWheelEvent.ToInteger()) 
                    .Where(z => z.ValueInt != EventPrizeTypeEnum.FishGame_GameBonus.ToInteger())
                    .Where(z => z.ValueInt != EventPrizeTypeEnum.DrawCash_ByDrawPrizeSetting.ToInteger())
                    .Where(z => z.ValueInt != EventPrizeTypeEnum.UseSelectedMultiplePrizeSetting_RankBetAmount.ToInteger())
                    .ToList();      
            }

            return enumValues;
        }

        public List<DropdownValueViewModel> GetAGGEventPrizeType(int userID)
        {
            List<EventPrizeTypeEnum> availableTypes = EventDefinitionHelper
                .PrizeTypes_AGGReport_Page()
                .Select(x => (EventPrizeTypeEnum)x)
                .ToList();            

            var dropdownItems = ConvertEnumsToDropdownModel(availableTypes , isDisplayIdNumber: false);

            return dropdownItems
                .ToList();
        }



        public List<DropdownValueViewModel> GetLinkType()
        {
            var enumValues = GetByEnumAndLanguageText<LinkTypeEnum>();

            return enumValues
                .ToList();
        }

        public List<DropdownValueViewModel> GetAnnouncementType()
        {
            var enumValues = GetByEnumAndLanguageText<AnnouncementTypeEnum>();

            return enumValues
                .ToList();
        }

        public List<DropdownValueViewModel> GetMerchantCreditChangeTypeEnum()
        {
            var enumValues = GetByEnumAndLanguageText<MerchantCreditChangeTypeEnum>();
            return enumValues
                .ToList();
        }

        public List<DropdownValueViewModel> GetUserAssignMerchantMode()
        {
            var enumValues = GetByEnumAndLanguageText<UserAssignMerchantModeEnum>();
            return enumValues
                .ToList();
        }

        public List<DropdownValueViewModel> GetSortDirection()
        {
            var enumValues = GetByEnumAndLanguageText<SortDirectionEnum>();
            return enumValues
                .ToList();
        }


        public List<DropdownValueViewModel> GetLatestMonth(int count)
        {
            //if(SettingHelper.IsLocalhostWebSiteName)
            //{
            //    count = 36;
            //}

            var twToday = DateTimeHelper.GetCurrentTime(8);

            List<DateTime> dateList = new List<DateTime>();
            dateList.Add(new DateTime(twToday.Year, twToday.Month, 1));

            for (int i = 1; i < count; i++)
            {
                var targetDate = twToday.AddMonths(-1 * i);
                dateList.Add(new DateTime(targetDate.Year, targetDate.Month, 1));
            }

            return dateList.Select(x => new DropdownValueViewModel
            {
                Text = x.ToString("yyyy-MM"),
                Value = x.ToString("yyyy-MM-dd"),
                ExtraProperty = new
                {
                    AccountingBeginEnd = $"{x.ToString("MM/dd")}~{x.AddMonths(1).ToString("MM/dd")}"
                }
            })
            .ToList();
        }



        public List<DropdownValueViewModel> GetBOAnnouncementType()
        {
            var enumValues = GetByEnumAndLanguageText<BOAnnouncementTypeEnum>();
            return enumValues
                .ToList();
        }

        public List<DropdownValueViewModel> GetBOAnnouncementHintType()
        {
            var enumValues = GetByEnumAndLanguageText<BOAnnouncementHintTypeEnum>();
            return enumValues
                .ToList();
        }

        public List<DropdownValueViewModel> GetAdminSysAssignTypes(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var enumValues = GetByEnumAndLanguageText<AssignTypeEnum>();

            var list = enumValues
                .Where(x => x.ValueInt != AssignTypeEnum.SpecificUserCanView.ToInteger()) //部分類型先不開放、功能未完成
                .Where(x => x.ValueInt != AssignTypeEnum.SpecificReferenceMerchantHierarchyCanView.ToInteger()) //部分類型先不開放、功能未完成
                .ToList();

            //超級管理員
            if (!currentUser.IsSuperAdminOrg)
            {
                list = list
                    .Where(x => x.ValueInt != AssignTypeEnum.All.ToInteger())
                    .ToList();
            }

            return list;
        }

        public List<DropdownValueViewModel> GetAmountColumns()
        {
            return new List<DropdownValueViewModel>
            {
                new DropdownValueViewModel("BetAmount(投注额)" , "BetAmount"),
                new DropdownValueViewModel("PayoutAmount(派奖金额)" , "PayoutAmount"),
                new DropdownValueViewModel("WinLossAmount(输赢金额)" , "WinLossAmount"),
                new DropdownValueViewModel("RTP" , "RTP"),
                new DropdownValueViewModel("TotalRound(总局数)" , "TotalRound"),
            };
        }


        public List<DropdownValueViewModel> GetEnumsDropdownByTypeName(string typeName)
        {
            string typeNameAlias = (typeName + "Enum").ToLower();

            typeName = typeName.ToLower();

            //務必是這個專案底下的
            var assembly = System.Reflection.Assembly.Load("target.Models");
            var matchType = assembly.GetTypes()
                .Where(x => x.IsEnum)
                .Where(x => x.Name?.ToLower() == typeName || x.Name?.ToLower() == typeNameAlias)
                .FirstOrDefault();

            if (matchType == null)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);
            }

            List<object> enumValues = new List<object>();

            foreach (var enumValue in System.Enum.GetValues(matchType))
            {
                enumValues.Add(enumValue);
            }

            return enumValues.Select(eachValue =>
            {
                var parseEnumValue = (System.Enum)eachValue;

                var convert = new DropdownValueViewModel
                {
                    Text = MultiLanguageTextHelper.GetEnumsCultureText(parseEnumValue),
                    Value = parseEnumValue.ToIntegerString(),
                    ValueInt = parseEnumValue.ToInteger()
                };

                return convert;
            })
            .ToList();
        }


        public List<DropdownValueViewModel> GetVendorConfig(int userID, QueryVendorConfigApiRequestParameterModel filter)
        {
            filter = filter ?? new QueryVendorConfigApiRequestParameterModel();
            filter.VendorIDs = filter.VendorID.ToIntegerListCollection();
            
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            string myDefaultCurrency = "";

            //如果非超級管理員
            if (!currentUser.IsSuperAdminOrg)
            {
                filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
                filter.VendorIDs = GetRestrictVendorIDsByUserID(userID);

                if(filter.VendorIDs.IsEmptyOrNull() && filter.VendorID > 0)
                {
                    filter.VendorIDs = filter.VendorID.ToIntegerListCollection();
                }

                //為了給AM測試只好OOXXXXXX
                if(HasTargetPermission(userID, "Special_AllowToUseExperimentFeature"))
                {
                    string debug = "";
                }
                else
                {
                    filter.IsTestEnvironment = false;
                }
            }

            var query = VendorConfigRepository.GetByFilter(filter);

            if(query.CountOrZero() == 1 && !currentUser.IsSuperAdminOrg)
            {
                string selectColumns = " Distinct Currency ";

                var myCurrencyList = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
                {
                    FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs
                }, selectColumns);

                myDefaultCurrency = myCurrencyList.FirstOrDefault()?.Currency;
            }

            var values = query
                .OrderBy(z => z.VendorID == (int)GameVendorTypeEnum.AGG ? 0 : z.VendorID)
                .Select(z =>
                {
                    var model = new DropdownValueViewModel
                    {
                        Text = currentUser.IsSuperAdminOrg ? $"{z.VendorConfigID}｜{z.Name}" : z.Name,
                        Value = z.VendorConfigID.ToString(),
                        ValueInt = z.VendorConfigID,
                    };

                    if(filter.IsShowCurrencyNameOnly)
                    {
                        if(!string.IsNullOrWhiteSpace(z.SupportCurrency))
                        {
                            if(!currentUser.IsSuperAdminOrg)
                            {
                                model.Text = $"{z.SupportCurrency}";
                            }
                        }
                        else if(myDefaultCurrency.IsNotEmptyTextNotNull())
                        {
                            model.Text = myDefaultCurrency;
                        }
                    }

                    return model;
                })
                .ToList();

            if(filter.IsShowCurrencyNameOnly)
            {
                values = values.OrderBy(x => x.Text).ToList();
            }

            return values;
        }


        public List<DropdownValueViewModel> GetExtraFtilerType(int userID, LazyCommonRequestModel filter)
        {
            filter = filter ?? new LazyCommonRequestModel();

            List<AdminSysExtraFilterEnum> enums = ExtensionOfEnum.GetEnumAllValue<AdminSysExtraFilterEnum>();

            if (filter.CategoryID > 0)
            {
                var categoryEnum = (DataCategoryEnum)filter.CategoryID;

                enums = enums
                        .Where(x =>
                        {
                            var attr = x.GetAttributeFromEnum<DataCategoryAttribute>();

                            if (attr == null)
                                return false;

                            return attr.Value.ToInteger() == filter.CategoryID;
                        })
                        .ToList();

                //部份下拉選單會根據角色微調選項
                switch (categoryEnum)
                {
                    case DataCategoryEnum.ExtraFilterType_Player:

                        if (!HasSuperAdminPermission(userID))
                        {
                            enums = new List<AdminSysExtraFilterEnum>();
                        }
                        break;

                    default:
                        break;
                }
            }

            var convert = ConvertEnumsToDropdownModel(enums);
            return convert;
        }


        public List<DropdownValueViewModel> GetMerchantSelectTemplate(int userID, LazyCommonRequestModel filter)
        {
            filter = filter ?? new LazyCommonRequestModel();

            var myFilter = new QueryMerchantSelectTemplateRequestViewModel
            {

            };

            if (filter.CategoryID >= 0)
            {
                myFilter.CategoryIDs = filter.CategoryID.ToListCollection();
            }

            var query = MerchantSelectTemplateRepository.GetByFilter(myFilter);

            return query
                .Select(z => new DropdownValueViewModel
                {
                    Text = $"{z.TemplateID}｜{z.DisplayName}",
                    Value = z.TemplateID.ToString(),
                    ValueInt = z.TemplateID
                })
                .ToList();
        }

        public List<DropdownValueViewModel> GetRootAgents(int userID)
        {
            var queryRootAgents = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                ParentMerchantID = 0
            });

            return queryRootAgents
                .Select(z => new DropdownValueViewModel
                {
                    Text = $"{z.MerchantID}｜" + MerchantRepository.GetSingleByCache(z.MerchantID)?.Name,
                    Value = z.MerchantID.ToString(),
                    ValueInt = z.MerchantID
                })
                .ToList();
        }


        public List<DropdownValueViewModel> GetVendorConfigSuuportFetchReportAPI(int userID, QueryVendorConfigRequestViewModel filter)
        {
            filter = filter ?? new QueryVendorConfigRequestViewModel();
            filter.VendorIDs = new List<int>();

            foreach (var vendor in ExtensionOfEnum.GetEnumAllValue<GameVendorTypeEnum>())
            {
                var def = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(vendor);

                if (def.IsSupportCompareStatDBReport)
                {
                    filter.VendorIDs.Add(vendor.ToInteger());
                }
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //如果非超級管理員
            if (!currentUser.IsSuperAdminOrg)
            {
                filter.MerchantIDs = currentUser.AvailableMerchantIDs;
                filter.VendorIDs = GetRestrictVendorIDsByUserID(userID);
            }

            var query = VendorConfigRepository.GetByFilter(filter);

            var values = query
                .Where(x => !x.IsTestEnvironment) //測試環境的不要
                .OrderBy(z => z.VendorID == (int)GameVendorTypeEnum.AGG ? 0 : z.VendorID)
                .Select(z => new DropdownValueViewModel
                {
                    Text = $"{z.VendorConfigID}｜{z.Name}",
                    Value = z.VendorConfigID.ToString(),
                    ValueInt = z.VendorConfigID,
                    ExtraProperty = new
                    {
                        GameVendor = z.VendorID,
                        GameVendorText = MultiLanguageTextHelper.GetGameVendorDisplayName(z.VendorID),
                    }
                })
                .ToList();

            return values;
        }


        public List<DropdownValueViewModel> GetGameTypes()
        {
            var enumValues = GetByEnumAndLanguageText<GameTypeEnum>();
            return enumValues;
        }

        public List<DropdownValueViewModel> GetUserOperateTypeEnum(int userID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var allEnums = ExtensionOfEnum.GetEnumAllValue<UserOperateTypeEnum>();

            if (!currentUser.IsSuperAdminOrg)
            {
                allEnums = new List<UserOperateTypeEnum>
                {
                    UserOperateTypeEnum.User_UserLogin,
                    UserOperateTypeEnum.Player_ManualDepositWalletAmount,
                    UserOperateTypeEnum.Player_LockStatus,
                    UserOperateTypeEnum.Player_UnlockStatus,
                    UserOperateTypeEnum.UpdateMerchantCreditAmount
                };
            }

            var dropdowns = ConvertEnumsToDropdownModel(allEnums);

            dropdowns = dropdowns
                .OrderBy(x => x.Text)
                .ThenBy(x => x.ValueInt)
                .ToList();

            return dropdowns;
        }


        public List<DropdownValueViewModel> GetMerchantCustomizedSettingKeys()
        {
            var queryAll = ExtensionOfEnum.GetEnumAllValue<MerchantCustomizedSettingKeyEnum>();

            var viewModels = queryAll
                .OrderBy(z =>
                {
                    var category = z.GetAttributeFromEnum<DataCategoryAttribute>();
                    return category == null ? 0 : category.Value.ToInteger();
                })
                .Select(x =>
                {
                    string settingName = MultiLanguageTextHelper.GetEnumsCultureText(x);
                    string settingNameEN = MultiLanguageTextHelper.GetEnumsCultureText(x, LanguageEnum.en_us);
                    string settingNameCN = MultiLanguageTextHelper.GetEnumsCultureText(x, LanguageEnum.zh_cn);
                    string settingNameTW = MultiLanguageTextHelper.GetEnumsCultureText(x, LanguageEnum.zh_tw);

                    string category = MultiLanguageTextHelper.GetEnumsCultureText(x.GetAttributeFromEnum<DataCategoryAttribute>()?.Value);
                    var displayModel = new DropdownValueViewModel
                    {
                        Text = category + "｜" + settingName,
                        Value = x.ToIntegerString(),
                        ValueInt = x.ToInteger(),
                        FullTextForKeywordSearch = (settingNameEN + settingNameCN + settingNameTW + x.ToString() + x.ToIntegerString()).ToLower(),
                        ExtraProperty = new
                        {
                            ShortName = settingName
                        }
                    };

                    return displayModel;
                })
                .ToList();

            return viewModels;
        }

        public List<DropdownValueViewModel> GetGameVendorTypes(int userID)
        {
            var allEnums = GetAvailableGameVendorsByUser(userID);

            var enumDropdowns = ConvertEnumsToDropdownModel(allEnums)
                .OrderBy(z => z.ValueInt == GameVendorTypeEnum.target.ToInteger() ? 0 : 999)
                .ThenBy(z => z.ValueInt)
                .ToList();

            return enumDropdowns;
        }

        public List<DropdownValueViewModel> GetGameListExtraFilterTypes(int userID)
        {
            var enumValues = GetByEnumAndLanguageText<GameListExtraFilterTypeEnum>();

            return enumValues
                .ToList();
        }


        public List<DropdownValueViewModel> GetGameVendorTypesExcludeSlot88(int userID)
        {
            var allEnums = GetAvailableGameVendorsByUser(userID);

            List<GameVendorTypeEnum> reportAvailableVendors = AppHelper.GetReportAvailableGameVendors();

            allEnums = allEnums
                .Where(z => reportAvailableVendors.Contains(z))
                .ToList();

            var enumDropdowns = ConvertEnumsToDropdownModel(allEnums)
                .OrderBy(z => z.ValueInt == GameVendorTypeEnum.target.ToInteger() ? 0 : 999)
                .ThenBy(z => z.ValueInt)
                .ToList();

            return enumDropdowns;
        }


        public List<DropdownValueViewModel> GetThirdPartyVendors(int userID)
        {
            var allEnums = GetAvailableGameVendorsByUser(userID);

            //大於99999 才視為三方遊戲商
            allEnums = allEnums.Where(x => x.ToInteger() > 99999).ToList();

            var enumDropdowns = ConvertEnumsToDropdownModel(allEnums);
            return enumDropdowns;
        }


        private List<DropdownValueViewModel> GetByEnumAndLanguageText<T>(bool isDisplayIdNumber = false) where T : System.Enum
        {
            var queryAll = ExtensionOfEnum.GetEnumAllValue<T>();
            return ConvertEnumsToDropdownModel(queryAll, isDisplayIdNumber);
        }


        private List<GameVendorTypeEnum> GetAvailableGameVendorsByUser(int userID)
        {
            var allEnums = AppHelper.GetStillAvailableGameVendors();

            //部分帳號會限制只能查詢特定的遊戲商
            List<int> restrictVendorIDs = GetRestrictVendorIDsByUserID(userID);

            if (restrictVendorIDs.IsEmptyOrNull())
            {
                return allEnums;
            }

            return allEnums.Where(z => restrictVendorIDs.Contains(z.ToInteger())).ToList();
        }


        /// <summary>
        /// 搜尋近期還有數據資料的商戶
        /// </summary>
        /// <returns></returns>
        private List<int> GetAliveMerchantsByCache()
        {
            var cacheKey = CacheKey.GetAliveMerchants();

            DateTime begin = DateTimeHelper.GetCurrentTime().AddDays(-60);
            DateTime end = DateTimeHelper.GetCurrentTime();

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var query = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumOnDuration(null, begin, end);
                return query ?? new List<MerchantGameSummaryDataEntity>();

            }, expiredMinute: 8 * 60);

            return queryCache.Select(x => x.MerchantID).Distinct().ToList();
        }

        private List<DropdownValueViewModel> ConvertEnumsToDropdownModel<T>(IEnumerable<T> enums, bool isDisplayIdNumber = false) where T : System.Enum
        {
            return enums.Select(x => new DropdownValueViewModel
            {
                Text = (isDisplayIdNumber ? $"{x.ToInteger()}｜" : "") + MultiLanguageTextHelper.GetEnumsCultureText(x),
                Value = x.ToIntegerString(),
                ValueInt = x.ToInteger()
            })
            .ToList();
        }

    }
}
