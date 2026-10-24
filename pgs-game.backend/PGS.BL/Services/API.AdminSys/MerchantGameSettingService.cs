using DocumentFormat.OpenXml.EMMA;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace target.BL.Services
{

    public class MerchantGameSettingService : BaseAdminSysService, IMerchantGameSettingService
    {
        public GameListRepository GameListRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantGameSettingSelfConfigRepository MerchantGameSettingSelfConfigRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public IAdminSysValidateCheckLogicRefService AdminSysCommonLogicCheckService { get; set; }



        public MerchantGameSettingViewModel GetGameSettingByMerchantID(int merchantID)
        {
            var querySettings = MerchantGameSettingRepository.GetByMerchantID(merchantID);

            List<GameListEntity> queryAllGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true
            });

            List<int> convertRatesSettingKeys = ExtensionOfEnum.GetEnumAllValue<MerchantCustomizedSettingKeyEnum>()
                .Where(z => z.ToInteger() >= 9000000)
                .Select(z => z.ToInteger())
                .ToList();

            var queryMerhant = MerchantRepository.GetById(merchantID);

            bool isSingleWallet = AppHelper.IsSingleWalletMode(queryMerhant.WalletMode);

            var queryCustomizedSettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                CustomizedSettingKeyIDs = convertRatesSettingKeys
            });

            var queryMappingConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            });

            List<Task> task = new List<Task>();

            var viewModelItems = new List<MerchantGameSettingItemViewModel>();

            string gameImageDoamin = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageGameImageUrl).SettingValue;

            queryAllGames = queryAllGames
                .Where(z => z.GameID > 0)
                .ToList();

            var gameNames_CN = DbMultiLanguageTextHelper.GetMultipleGamesDisplayName(queryAllGames, LanguageEnum.zh_cn);
            var gameNames_EN = DbMultiLanguageTextHelper.GetMultipleGamesDisplayName(queryAllGames, LanguageEnum.en_us);



            foreach (var gameEnity in queryAllGames)
            {
                var convert = new MerchantGameSettingItemViewModel
                {
                    GameType = gameEnity.GameType,
                    GameID = gameEnity.GameID,
                    GameName = gameEnity.GameName,
                    Status = NormalActiveStatus.Disable.ToInteger(),
                    ConvertRate = 1.0M,
                    ImageUrl = AppHelper.GenerateGameImageUrl(gameImageDoamin, gameEnity.GameCode)
                };

                var gameVendorID = (GameVendorTypeEnum)gameEnity.GameVendorType;
                var convertRateSetting = AppHelper.GetConvertRateCustomizedSettingKey(gameVendorID);

                if (convertRateSetting.IsDefine)
                {
                    var matchRateSetting = queryCustomizedSettings.FirstOrDefault(z => z.CustomizedSettingKey == (int)convertRateSetting.SettingKey);

                    if (matchRateSetting != null)
                    {
                        var parseModel = new SystemConfigSettingParseModel(matchRateSetting.CustomizedSettingKey, matchRateSetting.SettingValue);
                        convert.ConvertRate = parseModel.SettingValueOfDecimal;
                    }
                }

                var matchSetting = querySettings.FirstOrDefault(z => z.GameID == gameEnity.GameID);

                if (matchSetting != null)
                {
                    convert.Status = matchSetting.Status;
                }

                var matchConfig = queryMappingConfig.FirstOrDefault(z => z.VendorID == gameEnity.GameVendorType);

                if (matchConfig != null)
                {
                    convert.VendorConfigID = matchConfig.VendorConfigID;
                    convert.VendorConfigName = matchConfig.Name;
                }

                convert.GameNameCN = gameNames_CN.FirstOrDefault(z => z.GameID == gameEnity.GameID)?.LanguageText;
                convert.GameNameEN = gameNames_EN.FirstOrDefault(z => z.GameID == gameEnity.GameID)?.LanguageText;
                convert.GameTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)convert.GameType);
                convert.IsGameListDisable = gameEnity.Status != NormalActiveStatus.Enable.ToInteger();

                if (isSingleWallet)
                {
                    convert.IsEditDisabledDueToSingleWallet = !gameEnity.IsSupportSingleWallet;
                }

                viewModelItems.Add(convert);
            }

            viewModelItems = viewModelItems.OrderBy(z => z.GameID).ToList();

            var result = new MerchantGameSettingViewModel
            {
                GameSettingItems = viewModelItems
            };

            var queryHierarchy = MerchantHierarchyRepository.GetById(merchantID);

            if (queryHierarchy != null)
            {
                var queryChildMerchants = MerchantHierarchyRepository.GetMerchantIDsByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    HierarchyOfTargerMerchant = merchantID,
                    ExcludeMerchantID = merchantID
                });

                result.NumberOfChildMerchant = queryChildMerchants.Count();
            }

            return result;
        }


        public MerchantDefaultEnableGamesForSubAgnetViewModel GetEnableGamesForSubAgents(int userID, int merchantID)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryMerchant = MerchantRepository.GetByIdOrThrowException(merchantID);

            var queryEnableGameIDs = MerchantGameSettingRepository.GetByMerchantID(merchantID, NormalActiveStatus.Enable).Select(z => z.GameID).ToList();

            var querySelfConfig = MerchantGameSettingSelfConfigRepository.GetByMerchantID(merchantID);

            var systemSettingDefaultEnableForSubAgent = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.GameSetting_CreateSubAgentDefaultEnableGames);

            var now = DateTimeHelper.GetCurrentTime();

            //如果完全空值 、自動塞預設值
            if (querySelfConfig.IsEmptyOrNull())
            {
                var insertEntity = CreateDefaultSelfConfigData(merchantID, currentUser.UserName);
                querySelfConfig = insertEntity;
            }

            List<GameListEntity> queryMyGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                Status = NormalActiveStatus.Enable.ToInteger(),
            })
            .Where(x => queryEnableGameIDs.Contains(x.GameID))
            .ToList();

            var viewModel = new MerchantDefaultEnableGamesForSubAgnetViewModel();
            viewModel.DefaultEnableGameIDs = new List<int>();

            List<DropdownValueViewModel> dropdownItems = new List<DropdownValueViewModel>();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(queryEnableGameIDs);

            foreach (var game in queryMyGames)
            {
                var matchGameSetting = querySelfConfig.FirstOrDefault(z => z.GameID == game.GameID);

                if (matchGameSetting != null && matchGameSetting.IsSubAgentDefaultEnableGame)
                {
                    viewModel.DefaultEnableGameIDs.Add(game.GameID);
                }
                else if(matchGameSetting == null)
                {
                    if(systemSettingDefaultEnableForSubAgent.SettingValueOfListInt.Contains(game.GameID))
                    {
                        viewModel.DefaultEnableGameIDs.Add(game.GameID);
                    }                    
                }

                dropdownItems.Add(new DropdownValueViewModel
                {
                    Text = $"{game.GameID}｜" + gameNameDic.GetDictionaryOrDefaultValue(game.GameID, game.GameName),
                    Value = game.GameID.ToString(),
                    ValueInt = game.GameID,
                    ExtraProperty = new
                    {
                        GameType = game.GameType
                    }
                });
            }

            viewModel.AvailableGamesDropdown = dropdownItems;
            viewModel.WalletMode = queryMerchant.WalletMode;

            var queryIMChannel = IMChannelRepository.GetMerchantIMChannel(queryMerchant.MerchantID);

            if((queryIMChannel?.IMChannelID ?? "").IsNumeric())
            {
                viewModel.IMChannelID = Int32.Parse(queryIMChannel.IMChannelID);

                var channels = GeneralTagMappingRepository.GetByTagID((int)GeneralTagEnum.DoNotAutoCompleteIMChannelWhenCreateAgent);

                if(channels.Any(x=> x.TargetID == viewModel.IMChannelID))
                {
                    viewModel.IsDisableIMChannelAutoComplete = true;
                }
            }

            return viewModel;
        }


        private List<MerchantGameSettingSelfConfigEntity> CreateDefaultSelfConfigData(int merchantID , string userName)
        {
            var defaultEnableGameIDs = new List<int>();

            var queryGameIDs = SystemConfigSettingRepository.GetParseModelByEnum(SystemConfigSettingEnum.GameSetting_CreateSubAgentDefaultEnableGames);

            if(queryGameIDs.SettingValueOfListInt.IsEmptyOrNull())
            {
                #region to DO 要改成用customized開啟

                defaultEnableGameIDs = new List<int>
                {
                    290004,
                    290013,
                    290025,
                    290030,
                    290031,
                    290033,
                    290036,
                    290037,
                    290038,
                    290039,
                    290041,
                    290044,
                    290046,
                    290047,
                    290048,
                    290049,
                    290050,
                    290051,
                    290054,
                    290055,
                    290056,
                    290058,
                    290062,
                    290063,
                    290064,
                    290069,
                    290074,
                    290075,
                    290076,
                    290077,
                    290078,
                    290080,
                    290081,
                    290082,
                    290083,
                    290085,
                    290087,
                    290088,
                    290089
                };

                #endregion
            }
            else
            {
                defaultEnableGameIDs = queryGameIDs.SettingValueOfListInt;
            }

            DateTime now = DateTimeHelper.GetCurrentTime();

            var insertEntity = new List<MerchantGameSettingSelfConfigEntity>();

            foreach (var gameID in defaultEnableGameIDs)
            {
                insertEntity.Add(new MerchantGameSettingSelfConfigEntity
                {
                    IsSubAgentDefaultEnableGame = true,
                    GameID = gameID,
                    IsLobbyVisible = true,
                    UpdateBy = userName,
                    MerchantID = merchantID,
                    UpdateTime = now
                });
            }

            MerchantGameSettingSelfConfigRepository.CreateMany(insertEntity);
            return insertEntity;
        }


        /// <summary>
        /// 商戶遊戲開關 (代理自己編輯)
        /// </summary>
        public MerchantGameSettingAgentSelfEditViewModel GetGameSettingByMerchantIDForAgentSelfEdit(int userID, int merchantID)
        {
            // #001 可以開的遊戲 以上級為準。
            // #002 如果是自己編輯自己 (或是root代理 且非超級管理員)。則只能勾選autoSync的開關

            //root要禁止編輯? (只能由我方設置)...
            //非代理體系...禁止編輯

            CheckViewMerchantPermission(userID, merchantID);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var systemSettingDefaultEnableForSubAgent = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.GameSetting_CreateSubAgentDefaultEnableGames);

            var queryMerchant = MerchantRepository.GetById(merchantID);

            var myViewModel = new MerchantGameSettingAgentSelfEditViewModel();
            myViewModel.MerchantID = merchantID;
            myViewModel.MerchantName = queryMerchant?.Name;
            myViewModel.GameIDs_Slots = new List<int>();
            myViewModel.GameIDs_Live = new List<int>();
            myViewModel.GameSettingItems = new List<MerchantGameSettingAgentSelfEditItemViewModel>();

            var queryMerchantHierarchy = MerchantHierarchyRepository.GetById(merchantID);

            var querySelfConfig = MerchantGameSettingSelfConfigRepository.GetByMerchantID(merchantID);

            if(querySelfConfig.IsEmptyOrNull())
            {
                var newInsertItems = CreateDefaultSelfConfigData(merchantID, currentUser.UserName);
                querySelfConfig = newInsertItems;
            }

            bool isPermissionCanEnableOnly = HasTargetPermission(userID, "Agent_MerchantCredit_GameOpenFlag_EnableOnly");

            bool isDisableAllEditGameOpenFlag = false;

            if (queryMerchantHierarchy == null || queryMerchantHierarchy.ParentMerchantID == 0)
            {
                //root代理的開關 只能透過我方內部人員設置
                isDisableAllEditGameOpenFlag = true;
            }
            else if (currentUser.ReferenceCreditMerchant == merchantID)
            {
                //自己不能編輯自己開關
                //2023-12 移除此邏輯。如果後續有發現什麼漏洞 請說明不能自己編輯自己的原因
                //isDisableAllEditGameOpenFlag = true;
            }

            var mySelfEnableGames = MerchantGameSettingRepository.GetByMerchantID(merchantID, NormalActiveStatus.Enable);
            var mySelfEnableGameIDs = mySelfEnableGames.Select(x => x.GameID).ToList();

            //特殊案例 (e.g. Avatar)
            List<int> parentEnableGameIDs = new List<int>();
            List<int> parentNotExistGameIDs = new List<int>();

            if (queryMerchantHierarchy != null && queryMerchantHierarchy.ParentMerchantID != 0)
            {
                var parentEnableGames = MerchantGameSettingRepository.GetByMerchantID(queryMerchantHierarchy.ParentMerchantID, NormalActiveStatus.Enable);
                parentEnableGameIDs = parentEnableGames.Select(x => x.GameID).ToList();

                parentNotExistGameIDs = mySelfEnableGameIDs.Where(gameID => !parentEnableGameIDs.Contains(gameID)).ToList();
            }
            else
            {
                parentEnableGameIDs = mySelfEnableGameIDs;
            }

            if (parentEnableGameIDs.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundGame, "查無遊戲(#A0085 是否配置錯誤?)");
            }

            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameIDs = parentEnableGameIDs
            });

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(parentEnableGameIDs);

            //以parent能開放的遊戲為主
            foreach (var item in queryGames)
            {
                if(item.GameType == (int)GameTypeEnum.Slot)
                {
                    myViewModel.GameIDs_Slots.Add(item.GameID);
                }

                if (item.GameType == (int)GameTypeEnum.Live)
                {
                    myViewModel.GameIDs_Live.Add(item.GameID);
                }

                var mySelfConfig = querySelfConfig.FirstOrDefault(x => x.GameID == item.GameID);

                var gameEditItem = new MerchantGameSettingAgentSelfEditItemViewModel
                {
                    GameID = item.GameID,
                    GameName = gameNameDic.GetDictionaryOrDefaultValue(item.GameID, item.GameName),
                    IsGameOpenFlagDisableEdit = isDisableAllEditGameOpenFlag,
                    IsGameListDisable = item.Status != NormalActiveStatus.Enable.ToInteger(),
                    IsSpecialReasonDisableEdit = false,
                    MerchantGameEnableFlag = mySelfEnableGameIDs.Contains(item.GameID),
                    SubAgentDefaultEnableGameFlag = mySelfConfig?.IsSubAgentDefaultEnableGame ?? (systemSettingDefaultEnableForSubAgent.SettingValueOfListInt.Contains(item.GameID)),
                    IsLobbyVisibleFlag = mySelfConfig == null ? true : mySelfConfig.IsLobbyVisible
                };

                //如果權限 只能開啟、不能關閉
                if(gameEditItem.MerchantGameEnableFlag && isPermissionCanEnableOnly)
                {
                    gameEditItem.IsGameOpenFlagDisableEdit = true;
                }

                myViewModel.GameSettingItems.Add(gameEditItem);
            }

            //特殊獨佔遊戲 (畫面上禁止編輯) e.g. Avatar
            if (parentNotExistGameIDs.IsNotEmptyNull())
            {
                var gameNameDicOfSpecial = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(parentNotExistGameIDs);

                foreach (var gameID in parentNotExistGameIDs)
                {
                    var queryGame = GameListRepository.GetById(gameID);

                    var mySelfConfig = querySelfConfig.FirstOrDefault(x => x.GameID == gameID);

                    var gameEditItem = new MerchantGameSettingAgentSelfEditItemViewModel
                    {
                        GameID = queryGame.GameID,
                        GameName = gameNameDicOfSpecial.GetDictionaryOrDefaultValue(queryGame.GameID, queryGame.GameName),
                        SubAgentDefaultEnableGameFlag = mySelfConfig?.IsSubAgentDefaultEnableGame ?? false,
                        IsGameOpenFlagDisableEdit = isDisableAllEditGameOpenFlag,
                        IsGameListDisable = queryGame.Status != NormalActiveStatus.Enable.ToInteger(),
                        IsSpecialReasonDisableEdit = true,
                        MerchantGameEnableFlag = mySelfEnableGameIDs.Contains(queryGame.GameID),
                        IsLobbyVisibleFlag = mySelfConfig == null ? true : mySelfConfig.IsLobbyVisible
                    };

                    myViewModel.GameSettingItems.Add(gameEditItem);
                }
            }

            myViewModel.IsDisableAllEditGameOpenFlag = isDisableAllEditGameOpenFlag;
            myViewModel.IsPermissionCanEnableOnly = isPermissionCanEnableOnly;

            if (queryMerchantHierarchy != null)
            {
                var queryChildMerchantIDs = MerchantHierarchyRepository.GetMerchantIDsByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    HierarchyOfTargerMerchant = merchantID,
                    ExcludeMerchantID = merchantID
                });

                if (queryChildMerchantIDs.IsNotEmptyNull() && queryChildMerchantIDs.CountOrZero() <= 2000)
                {
                    if(queryChildMerchantIDs.CountOrZero() > 2000)
                    {
                        myViewModel.ChildAgentHintMsg = "超過2000筆子代理無法顯示";
                    }
                    else
                    {
                        var queryChilds = MerchantHierarchyRepository.GetJoinInfoByMerchantIDs(queryChildMerchantIDs);

                        myViewModel.ChildAgentDropdowns = queryChilds
                            .Select(m => new DropdownValueViewModel
                            {
                                Text = $"{m.MerchantID}｜{m.MerchantName}",
                                Value = m.MerchantID.ToString(),
                                ValueInt = m.MerchantID
                            })
                            .ToList();
                    }
                }
            }

            myViewModel.ChildAgentDropdowns = myViewModel.ChildAgentDropdowns ?? new List<DropdownValueViewModel>();
            return myViewModel;
        }


        public void BatchUpdateGameOpenFlagByAgentSelf(int userID, MerchantGameSettingAgentSelfEditViewModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameSettingItems).Rule_NotEmptyNull();

            var queryMerchant = MerchantRepository.GetByIdOrThrowException(model.MerchantID);

            CheckViewMerchantPermission(userID, model.MerchantID);

            List<int> updateMerchants = new List<int>
            {
                model.MerchantID
            };

            if (model.SyncSubAgentIDs.IsNotEmptyNull())
            {
                foreach(var mID in model.SyncSubAgentIDs)
                {
                    CheckViewMerchantPermission(userID, mID);
                    updateMerchants.Add(mID);
                }
            }

            var queryMerchants = MerchantRepository.GetByIds(updateMerchants);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var listOfUpdateEntity = new List<MerchantGameSettingAgentSelfEditEntityUpdateModel>();

            //var insertEntity = new List<MerchantGameSettingEntity>();
            //var newInsertConfigItems = new List<MerchantGameSettingSelfConfigEntity>();

            var now = DateTimeHelper.GetCurrentTime();

            foreach (var eachMerchantID in updateMerchants)
            {
                var queryExistConfig = MerchantGameSettingSelfConfigRepository.GetByMerchantID(eachMerchantID);

                var insertEntity = new List<MerchantGameSettingEntity>();
                var newInsertConfigItems = new List<MerchantGameSettingSelfConfigEntity>();

                foreach (var item in model.GameSettingItems)
                {
                    //*重要 獨家遊戲不能被更改開關
                    if (!item.IsSpecialReasonDisableEdit)
                    {
                        insertEntity.Add(new MerchantGameSettingEntity
                        {
                            GameID = item.GameID,
                            MerchantID = eachMerchantID,
                            UpdateBy = currentUser.UserName,
                            UpdateTime = now,
                            Status = item.MerchantGameEnableFlag ? NormalActiveStatus.Enable.ToInteger() : NormalActiveStatus.Disable.ToInteger(),
                        });
                    }

                    var matchConfigData = queryExistConfig.Where(x => x.GameID == item.GameID).FirstOrDefault();

                    if (matchConfigData != null)
                    {
                        var cloneEntity = matchConfigData.ReturnInjecteObject<MerchantGameSettingSelfConfigEntity>();
                        cloneEntity.IsSubAgentDefaultEnableGame = item.SubAgentDefaultEnableGameFlag;
                        cloneEntity.IsLobbyVisible = item.IsLobbyVisibleFlag;
                        cloneEntity.UpdateBy = currentUser.UserName;
                        cloneEntity.UpdateTime = now;

                        newInsertConfigItems.Add(cloneEntity);
                    }
                    else
                    {
                        newInsertConfigItems.Add(new MerchantGameSettingSelfConfigEntity
                        {
                            MerchantID = eachMerchantID,
                            GameID = item.GameID,
                            UpdateTime = now,
                            UpdateBy = currentUser.UserName,
                            IsLobbyVisible = item.IsLobbyVisibleFlag,
                            IsSubAgentDefaultEnableGame = item.SubAgentDefaultEnableGameFlag
                        });
                    }
                }

                listOfUpdateEntity.Add(new MerchantGameSettingAgentSelfEditEntityUpdateModel
                {
                    MerchantID = eachMerchantID,
                    MerchantGameSettingEntityItems = insertEntity,
                    MerchantGameSettingSelfConfigEntityItems = newInsertConfigItems
                });
            }


            //防呆檢查 (SW support)
            foreach(var item in listOfUpdateEntity)
            {
                var matchMerchant = queryMerchants.FirstOrDefault(x => x.MerchantID == item.MerchantID);

                if (matchMerchant == null)
                    throw new TerproException(ApiErrorCode.NotFoundMerchant, new ErrorDetailModel($"{item.MerchantID}"));

                if (AppHelper.IsSingleWalletMode(matchMerchant.WalletMode))
                {
                    var myGameIDs = item.MerchantGameSettingEntityItems.Where(x => x.Status == NormalActiveStatus.Enable.ToInteger()).Select(x => x.GameID).ToList();

                    var isAnyNotSupport = GameListRepository.IsAnyGamesNotSupportSingleWallet(myGameIDs);

                    if (isAnyNotSupport)
                    {
                        throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("Game not support seamless"));
                    }
                }
            }

            List<UserOperateRecordEntity> losg = new List<UserOperateRecordEntity>();

            foreach (var item in listOfUpdateEntity)
            {
                var targetMerchantIDs = item.MerchantID.ToListCollection();

                // Step 1-1. 清除資料
                MerchantGameSettingRepository.DeleteByIDs(targetMerchantIDs, item.MerchantGameSettingEntityItems.Select(z => z.GameID).ToList());

                // Step 1-2. 塞資料
                MerchantGameSettingRepository.BulkInsert(item.MerchantGameSettingEntityItems);

                // step 2-1. 刪除config資料
                MerchantGameSettingSelfConfigRepository.DeleteByIDs(targetMerchantIDs, item.MerchantGameSettingSelfConfigEntityItems.Select(z => z.GameID).ToList());

                // step 2-2. 寫入config資料
                MerchantGameSettingSelfConfigRepository.BulkInsert(item.MerchantGameSettingSelfConfigEntityItems);

                var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateGameSettingByAgentSelf, item.MerchantID, model.ConvertToJsonString());

                if (updateMerchants.CountOrZero() >= 2)
                {
                    log.Memo = "Sync Multiple";
                }

                losg.Add(log);
            }

            // step 3. 寫操作紀錄
            WriteOperateLog(losg);

            // Step 4. 清除緩存
            Task.Factory.StartNew(() =>
            {
                foreach (var eachMerchantID in updateMerchants)
                {
                    CacheRemoveHelper.RemoveCache_WhenUpdateMerchantGames(eachMerchantID);
                }
            });
        }



        public void Update(int userID, int merchantID, MerchantGameSettingUpdateRequestModel model)
        {
            (new { merchantID }).RuleFor(x => x.merchantID).Rule_NotZeroNotMinus();

            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Status).Rule_NotMinus();

            CheckViewMerchantPermission(userID, merchantID);

            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            bool isSingleWalletMode = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);


            if(model.Status == NormalActiveStatus.Enable.ToInteger())
            {
                //檢查單錢包是否支援
                if (isSingleWalletMode &&
                    !queryGame.IsSupportSingleWallet)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_SingleWalletMerchantDoNotSupportGame);
                }

                //進行多項檢查 (enable情境需要特別防呆)
                //進行多項檢查 (enable情境需要特別防呆)
                //進行多項檢查 (enable情境需要特別防呆)
                ValidateGameAvailableWhenEnbleGame(merchantID.ToListCollection(), model.GameID.ToListCollection());
            }


            CheckAlwaysDisableGame(queryGame.GameID);

            var queryExist = MerchantGameSettingRepository.GetByMerchantIDAndGameID(merchantID, model.GameID);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            string debugInfo = $"{merchantID}_{model.GameID}, {queryExist?.Status}_{model.Status}";

            if (queryExist == null)
            {
                MerchantGameSettingRepository.Create(new MerchantGameSettingEntity
                {
                    GameID = model.GameID,
                    MerchantID = merchantID,
                    Status = model.Status,
                    Sort = 0,
                    UpdateBy = currentUser.UserName,
                    UpdateTime = DateTimeHelper.GetCurrentTime(),
                });
            }
            else
            {
                queryExist.Status = model.Status;
                queryExist.UpdateBy = currentUser.UserName;
                queryExist.UpdateTime = DateTimeHelper.GetCurrentTime();

                MerchantGameSettingRepository.Update(queryExist);
            }

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateGameSettingByGameSwitchPage, merchantID, debugInfo);
            WriteOperateLog(log);

            CacheRemoveHelper.RemoveCache_WhenUpdateMerchantGames(merchantID);
        }


        public CRUDResultModel MultipleMerchantAdjustGameSetting(int userID, MultipleMerchantAdjustGameSettinRequestModel model)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            model.RuleFor(x => x.GameIDs).Rule_IsNotEmptyCollection();
            model.RuleFor(x => x.Type).Rule_NotZeroNotMinus();

            //有兩種操作方式 填寫merchantIDs 或 勾選上級代理
            List<int> merchantIDs = AppHelper.AnalyzeIDsFromRawText(model.RawText);

            if (merchantIDs.IsEmptyOrNull() && model.ParentMerchantID.NotNullNotZeroNotMinus())
            {
                var queryHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    HierarchyOfTargerMerchant = model.ParentMerchantID.Value,
                    ExcludeDeleteMerchants = true,
                });

                merchantIDs = queryHierarchy.Select(x => x.MerchantID).ToList();
            }

            if (merchantIDs.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "無法解析出商戶IDs，請確認格式是否正確");
            }

            if(merchantIDs.Any(id=> !RegexRuleHelper.IsMerchantID(id)))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "您輸入的數字有不合法的格式 (非merchantID)");
            }

            List<MerchantGameSettingEntity> newInserts = new List<MerchantGameSettingEntity>();

            var now = DateTimeHelper.GetCurrentTime();

            bool isEnableGames = model.Type == 1;

            //檢查 不支援單錢包的遊戲
            if (isEnableGames)
            {
                //若一次操作多個商戶、直接拋錯誤 (因為code寫起來比較麻煩)
                //若只操作單個商戶、自動濾掉不支援的遊戲
                if (merchantIDs.Count() == 1)
                {
                    int merchantID = merchantIDs.FirstOrDefault();
                    var isSingleWallet = AppHelper.IsSingleWalletMode(MerchantRepository.GetById(merchantID).WalletMode);

                    if (isSingleWallet)
                    {
                        model.GameIDs = AdminSysCommonLogicCheckService.ExcludeSingleWalletMerchantNotSupprtGameIDs(model.GameIDs);
                    }
                }
                else
                {
                    AdminSysCommonLogicCheckService.CheckSingleWalletMerchantNotSupprtGame(merchantIDs, model.GameIDs);
                }

                //進行多項檢查 (enable情境需要特別防呆)
                //進行多項檢查 (enable情境需要特別防呆)
                //進行多項檢查 (enable情境需要特別防呆)
                ValidateGameAvailableWhenEnbleGame(merchantIDs, model.GameIDs);
             
            }

            foreach (var merchantID in merchantIDs)
            {
                foreach (var gameID in model.GameIDs)
                {
                    CheckAlwaysDisableGame(gameID);

                    newInserts.Add(new MerchantGameSettingEntity
                    {
                        GameID = gameID,
                        MerchantID = merchantID,
                        Status = NormalActiveStatus.Enable.ToInteger(),
                        UpdateBy = currentUser?.UserName,
                        UpdateTime = now,
                        Sort = 1
                    });
                }
            }

            // Step 1. 清除資料
            MerchantGameSettingRepository.DeleteByIDs(merchantIDs, model.GameIDs);

            // Step 2. 塞資料 (若指定關閉遊戲則只會進行刪除資料)
            if (isEnableGames)
            {
                MerchantGameSettingRepository.CreateMany(newInserts);
            }

            // Step 3. 清除緩存
            Task.Factory.StartNew(() =>
            {
                foreach (var merchantID in merchantIDs)
                {
                    //清除redis
                    CacheRemoveHelper.RemoveCache_WhenUpdateMerchantGames(merchantID);
                }
            });


            var param_debug = new
            {
                MerchantIDs = merchantIDs,
                GameIDs = model.GameIDs,
            };

            string debug = param_debug.ConvertToJsonString();

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateGameSettingMultipleEdit, 0, debug);
            WriteOperateLog(log);

            return new CRUDResultModel
            {
                Message = $"更新完成。本次共有 {merchantIDs.Count()} 位商戶資料被更新。{Environment.NewLine}建議抽樣檢查幾位是否正確更新資料 ",
                ResultListIDs = merchantIDs
            };
        }


        private void ValidateGameAvailableWhenEnbleGame(List<int> merchantIDs , List<int> gameIDs)
        {
            //檢查支援貨幣
            AdminSysCommonLogicCheckService.CheckMerchantCurrencyNotSupprtGameAndThrow(merchantIDs, gameIDs);

            //檢查不想開通JP遊戲的商戶
            AdminSysCommonLogicCheckService.CheckMerchantJackpotGameAndThrow(merchantIDs, gameIDs);

            //檢查商戶是否有限定只能開通IDG visible games
            AdminSysCommonLogicCheckService.CheckMerchantOnlySupportIDGVisibleGamesAndThrow(merchantIDs, gameIDs);            
        }



        /// <summary>
        /// 有部分遊戲是不支援分配給商戶。只是純粹一個特殊分類或join報表的用途
        /// </summary>
        private void CheckAlwaysDisableGame(int gameID)
        {
            if (gameID == SystemConstant.Mario_GameID_Gameble)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"GameID: {gameID}。不支援分配給商戶使用");
            }
        }


        public void CopySettingToChilds(int userID, int parentMerchantID)
        {
            var query = MerchantHierarchyRepository.GetByIdOrThrowException(parentMerchantID);

            var queryChilds = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = parentMerchantID,
                ExcludeMerchantID = parentMerchantID
            });

            List<int> merchantIDs = queryChilds
                .Select(x => x.MerchantID)
                .ToList();

            CopySettingToMerchantIDs(userID, parentMerchantID, merchantIDs, isDeleteOriginalGames: true);
        }


        public void CopySetting(int userID, MerchantGameSettingCopyRequestModel model)
        {
            model.RuleFor(x => x.SourceMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.CopyToMerchantID).Rule_NotZeroNotMinus();

            if (!SettingHelper.IsMSTest)
            {
                model.RuleFor(x => x.CopyToMerchantID).Rule_NotEqual(model.SourceMerchantID);
            }

            CopySettingToMerchantIDs(userID, model.SourceMerchantID, model.CopyToMerchantID.ToListCollection(), model.IsDeleteOriginalGames);
        }


        public void CopySettingToMerchantIDs(int userID, int sourceMerchantID, List<int> targetMerchantIDs, bool isDeleteOriginalGames)
        {
            var queryCopySourceGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = sourceMerchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
            });

            if (queryCopySourceGames.IsEmptyOrNull())
                return;

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            List<int> gameIDs = queryCopySourceGames.Select(z => z.GameID).ToList();

            //檢查單錢包商戶不支援的遊戲
            AdminSysCommonLogicCheckService.CheckSingleWalletMerchantNotSupprtGame(targetMerchantIDs, gameIDs);

            foreach (int eachMerchant in targetMerchantIDs)
            {
                if (!isDeleteOriginalGames)
                {
                    var query = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
                    {
                        MerchantIDs = eachMerchant.ToListCollection(),
                        Status = NormalActiveStatus.Enable.ToInteger()
                    });

                    //檢查是否已有分配遊戲了
                    if (query.Count() > 0)
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_PleaseEmptyCollectionDataFirst);
                    }
                }

                var para = new
                {
                    MerchantID = eachMerchant
                };

                var queryTargetMercahnt = MerchantRepository.GetSingleByCache(eachMerchant);

                var insertItems = queryCopySourceGames.Select(z => new MerchantGameSettingEntity
                {
                    Status = NormalActiveStatus.Enable.ToInteger(),
                    GameID = z.GameID,
                    MerchantID = eachMerchant,
                    UpdateTime = DateTimeHelper.GetCurrentTime(),
                    UpdateBy = currentUser.UserName,
                    Sort = 0
                })
                    .ToList();

                // Step 1. 先清除既有資料
                MerchantGameSettingRepository.DeleteByModel(para);

                // Step 2. 塞資料
                MerchantGameSettingRepository.CreateMany(insertItems);

                //清除redis
                CacheRemoveHelper.RemoveCache_WhenUpdateMerchantGames(eachMerchant);
            }

        }


    }
}
