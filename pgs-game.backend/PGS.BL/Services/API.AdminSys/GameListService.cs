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
using target.Utility;

namespace target.BL.Services
{

    public class GameListService : BaseAdminSysService, IGameListService
    {
        public GameListRepository GameListRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public LanguageTextRepository LanguageTextRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }


        public PagedList<GameListViewModel> GetByPagingFilter(QueryGameListApiRequestParameter filter)
        {
            filter.IsNotDelete = true;
            filter.AssignMerchantIDs = filter.AssignMerchantID.ToIntegerListCollection();

            //有指定array就以array為主
            if (filter.GameVendorTypes.IsEmptyOrNull())
            {
                filter.GameVendorTypes = filter.GameVendorType.ToIntegerListCollection();
            }

            //額外的篩選標籤
            SetExtraFilterType(filter);

            var query = GameListRepository.GetPagingByFilter(filter);

            if (filter.ExcludeGameID.HasValue)
            {
                query.Items = query.Items.Where(x => x.GameID != filter.ExcludeGameID).ToList();
            }

            var viewModels = ConvertToNewTypePagingObject<GameListEntity, GameListViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(filter, viewModels.Items);

            //如果是一次撈超過100筆，採用複雜order by 條件 (用底層的 orderByColumn 不太好寫)
            if (viewModels.Items.IsNotEmptyNull() && viewModels.Items.Count() > 50)
            {
                viewModels.Items = viewModels.Items
                    .OrderBy(x => x.GameID == 0 ? 1 : 9999)
                    .ThenByDescending(x => x.GameID)
                    .ToList();
            }

            return viewModels;
        }

        public GameMerchantsMappingInfoViewModel GetGameMerchantsInfo(int gameID)
        {
            var queryGame = GameListRepository.GetById(gameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            var queryMerchants = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                EnableSpecificGameID = gameID,
            } , " MerchantID , Name " );                

            var viewModel = new GameMerchantsMappingInfoViewModel
            {
                GameID = queryGame.GameID,
                GameName = DbMultiLanguageTextHelper.GetGameDisplayName(queryGame.GameID),
                GameVendorType = queryGame.GameVendorType,
                Merchants = queryMerchants.Select(z => new GameMerchantsViewModel
                {
                    MerchantID = z.MerchantID,
                    MerchantName = z.Name,
                })
                .OrderBy(z => z.MerchantID)
                .ToList()
            };

            List<int> merchantIDs = queryMerchants.Select(z => z.MerchantID).Distinct().ToList();

            var queryVendorConifgs = VendorConfigRepository.GetByVendorTypeAndMerchants(queryGame.GameVendorType, merchantIDs);

            var converateRateEnum = AppHelper.GetConvertRateCustomizedSettingKey(queryGame.GameVendorType);

            if (!converateRateEnum.IsDefine)
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue, new ErrorDetailModel("ConverateRateEnum"));

            var queryConvertRateSettings = MerchantCustomizedSettingRepository.GetMultipleMerchantSettings(merchantIDs, converateRateEnum.SettingKey);

            foreach (var item in viewModel.Merchants)
            {
                item.ConvertRate = 1.0M;
                item.VendorConfigName = "-";

                var matchVendorConfig = queryVendorConifgs.FirstOrDefault(z => z.MerchantID == item.MerchantID);

                if (matchVendorConfig != null)
                {
                    item.VendorConfigID = matchVendorConfig.VendorConfigID;
                    item.VendorConfigName = matchVendorConfig.VendorConfigName;
                }

                var matchSetting = queryConvertRateSettings.FirstOrDefault(z => z.MerchantID == item.MerchantID);

                if (matchSetting != null)
                {
                    item.ConvertRate = matchSetting.SettingValueOfDecimal;
                }
            }

            return viewModel;
        }


        public GameListViewModel GetByID(int targetID)
        {
            var queryGameList = GameListRepository.GetById(targetID);

            if (queryGameList == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            var convert = ConvertToViewModels(queryGameList);
            return convert;
        }


        public void Create(GameListCreateRequestModel model)
        {
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameCode).Rule_NotEmptyText();
            model.RuleFor(x => x.GameName).Rule_NotEmptyText();

            var isExist = GameListRepository.IsExist(model.GameID);

            if (isExist)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            //檢查gameCode
            CheckGameCodeExist(model.GameCode);

            string languageCode = AppHelper.GetGameListLanguageCode(model.GameCode);

            var newEntity = model.ReturnInjecteObject<GameListEntity>();
            newEntity.LanguageCode = languageCode;

            //部分預設值統一在這邊設置(有其他function共用)
            GameListRepository.SetDefaultValueForNewInsertEntity(newEntity);

            var newGameListID = GameListRepository.Create(newEntity);


            //產生預設幾種多語系
            List<LanguageEnum> targetLanguages = new List<LanguageEnum>
            {
                LanguageEnum.zh_cn,
                LanguageEnum.zh_tw,
                LanguageEnum.en_us
            };

            foreach (var lang in targetLanguages)
            {
                string gameName = model.GameName;

                if (lang == LanguageEnum.zh_cn)
                    gameName = AppHelper.TraditionalToSimplified(model.GameName);
                else
                    gameName = AppHelper.SimplifiedToTraditional(model.GameName);

                var instance = new LanguageTextEntity
                {
                    LanguageCode = languageCode,
                    Text = gameName,
                    Language = lang.ToString()
                };

                LanguageTextRepository.Create(instance);
            }

        }

        public void Update(GameListUpdateRequestModel model)
        {
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameCode).Rule_NotEmptyText();
            model.RuleFor(x => x.GameName).Rule_NotEmptyText();

            var queryExist = GameListRepository.GetById(model.GameID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            queryExist.GameName = model.GameName;
            queryExist.GameCode = model.GameCode;
            queryExist.Status = model.Status;
            queryExist.GameType = model.GameType;
            queryExist.GameVendorType = model.GameVendorType;
            queryExist.Memo = model.Memo;

            GameListRepository.Update(queryExist);

            CacheRemoveHelper.RemoveCache_WhenUpdateGame(queryExist.GameID, queryExist.GameVendorType, queryExist.ThirdPartyGameCode);
        }


        public void Delete(int targetID)
        {
            var queryExist = GameListRepository.GetById(targetID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            queryExist.Status = NormalActiveStatus.PermanentDelete.ToInteger();
            GameListRepository.Update(queryExist);

            CacheRemoveHelper.RemoveCache_WhenUpdateGame(queryExist.GameID, queryExist.GameVendorType, queryExist.ThirdPartyGameCode);
        }


        public GameListCustomizedSettingBatchEditModel GetGameListCutomziedSettings(int userID , int gameID)
        {
            var queryGame = GameListRepository.GetById(gameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            var keys = ExtensionOfEnum.GetEnumAllValue<GameListCustomizedSettingKeyEnum>()
                .Where(x => x != GameListCustomizedSettingKeyEnum.LiveGameBetDetailMappingLanguageText)
                .ToList();

            var resultModel = new GameListCustomizedSettingBatchEditModel();
            resultModel.GameID = queryGame.GameID;
            resultModel.GameName = queryGame.GameName;
            resultModel.Items = new List<GameListCustomizedSettingItemModel>();

            var settings = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                GameID = gameID
            });

            foreach (var key in keys)
            {
                int settingKey = key.ToInteger();
                var matchValue = settings.FirstOrDefault(x => x.SettingKey == settingKey);

                var myModel = new GameListCustomizedSettingItemModel
                {
                    GameID = gameID,
                    SettingKey = settingKey,
                    SettingKeyText = MultiLanguageTextHelper.GetEnumsCultureText(key),
                };

                if (matchValue != null)
                {
                    myModel.Value = matchValue.SettingValue;
                }

                resultModel.Items.Add(myModel);
            }

            return resultModel;
        }

        public void CreateOrUpdateGameListSetting(int userID, GameListCustomizedSettingItemModel model)
        {
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();

            var queryExist = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
                {
                    GameID = model.GameID,
                    SettingKey = model.SettingKey
                })
                .FirstOrDefault();

            if (queryExist == null)
            {
                GameListCustomizedSettingRepository.Create(new GameListCustomizedSettingEntity
                {
                    GameID = model.GameID,
                    SettingKey = model.SettingKey,
                    SettingValue = model.Value,
                });
            }
            else
            {
                queryExist.SettingValue = model.Value;
                GameListCustomizedSettingRepository.Update(queryExist);
            }
        }

        public void DeleteGameListSetting(int userID, GameListCustomizedSettingItemModel model)
        {
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();

            var queryExist = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
                {
                    GameID = model.GameID,
                    SettingKey = model.SettingKey
                })
                .FirstOrDefault();

            if (queryExist != null)
            {
                GameListCustomizedSettingRepository.Delete(queryExist.Id);
            }           
        }



        private GameListViewModel ConvertToViewModels(GameListEntity queryGame)
        {
            var convert = queryGame.ReturnInjecteObject<GameListViewModel>();
            convert.GameTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)convert.GameType);
            convert.GameVendorTypeText = MultiLanguageTextHelper.GetGameVendorDisplayName(convert.GameVendorType);
            convert.IsGameListEnumDefined = true; //三方遊戲不需要定義

            var isThirdPartyGame = AppHelper.IsExternalVendorGame(queryGame.GameID);

            convert.IsThirdPartyGame = isThirdPartyGame;

            if (!isThirdPartyGame)
            {
                convert.IsGameListEnumDefined = ExtensionOfEnum.IsDefinedValue((GameListEnum)queryGame.GameID);
            }

            return convert;
        }

        private void SetExtraProperty(QueryGameListApiRequestParameter filter , List<GameListViewModel> items)
        {
            Dictionary<int, int> queryGameEnableMerchantCount = MerchantGameSettingRepository.GetEachGameEnableMerchantCount()
                .ToDictionary(x => x.GameID, x => x.TotalCount);

            var queryGameExtraSetting = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                SettingKeys = new List<int>
                            {
                                GameListCustomizedSettingKeyEnum.IsSupportRaiseProcessWhenPlaceBet.ToInteger(),
                                GameListCustomizedSettingKeyEnum.IsForbiddenDirectLaunchGame.ToInteger(),
                                GameListCustomizedSettingKeyEnum.SupportedOrientation_Landscape.ToInteger(),
                                GameListCustomizedSettingKeyEnum.SupportedOrientation_Portrait.ToInteger(),
                            }
            });

            var gameSettingDic = queryGameExtraSetting
                    .GroupBy(x => x.GameID)
                    .ToDictionary(z => z.Key, x =>
                    {
                        var settings = x.ToList();
                        return settings;
                    });

            var imageSetting = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageGameImageUrl);
            string imageDomain = imageSetting.SettingValue;

            //因為用dbCacheHelper速度不理想。改成自己查詢 (大量迴圈+cache單次單次存取 反而慢了)
            var queryAllGameLanguageText = LanguageTextRepository.GetAllGameNameLanguageTextDicByCache();

            string langCN = "zh_cn";
            string langEN = "en_us";

            var queryVendorConfigDic = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                IsIncludeDeleteData = false
            })
            .GroupBy(z => z.VendorID)
            .ToDictionary(x => x.Key, x =>
            {
                var currencyList = x.Where(x => x.SupportCurrency.IsNotEmptyTextNotNull()).Select(x => x.SupportCurrency)
                    .Where(x=> x != "IGM" && x != "TFUN" && x != "XFUN")
                    .Distinct()
                    .ToList();

                return new
                {
                    SupportCurrency = string.Join("／", currencyList)
                };
            });

            bool isBWG = SettingHelper.IsBWG;

            if(filter.ExtraQueryInfo.NotNullNotZeroNotMinus())
            {
                AdminSysGameQueryExtraInfoEnum infoQuery = (AdminSysGameQueryExtraInfoEnum)filter.ExtraQueryInfo;

                switch (infoQuery)
                {
                    case AdminSysGameQueryExtraInfoEnum.ShowPgsIcon:
                        isBWG = false;
                        break;

                    case AdminSysGameQueryExtraInfoEnum.ShowBwgIcon:
                        isBWG = true;
                        break;

                    default:
                        break;
                }
            }

            foreach (var eachGame in items)
            {
                eachGame.GameImageUrl = AppHelper.GenerateGameImageUrl(imageDomain, eachGame.GameCode);

                if(isBWG)
                {
                    eachGame.GameImageUrl2 = AppHelper.GenerateGameImageUrl(imageDomain, eachGame.GameCode, GameImageStyleTypeEnum.WithBWGIcon);
                }
                else
                {
                    eachGame.GameImageUrl2 = AppHelper.GenerateGameImageUrl(imageDomain, eachGame.GameCode, GameImageStyleTypeEnum.WithPGSIcon);
                }

                if (queryGameEnableMerchantCount.ContainsKey(eachGame.GameID))
                {
                    eachGame.NumberOfMerchants = queryGameEnableMerchantCount[eachGame.GameID];
                }

                if (AppHelper.IsPgsSelfGame(eachGame.GameID))
                {
                    eachGame.GameName_CN = DbMultiLanguageTextHelper.GetGameDisplayName(eachGame.GameID, LanguageEnum.zh_cn);
                    eachGame.GameName_EN = DbMultiLanguageTextHelper.GetGameDisplayName(eachGame.GameID, LanguageEnum.en_us);
                }
                else
                {
                    eachGame.GameName_CN = queryAllGameLanguageText.GetDictionaryOrDefaultValue($"{eachGame.LanguageCode}{langCN}");
                    eachGame.GameName_EN = queryAllGameLanguageText.GetDictionaryOrDefaultValue($"{eachGame.LanguageCode}{langEN}");
                }

                //遊戲customzied配置
                if (gameSettingDic.ContainsKey(eachGame.GameID))
                {
                    var match = gameSettingDic[eachGame.GameID];

                    eachGame.IsSupportRaiseProcess = GetBooleanValueBySettingEntity(match, GameListCustomizedSettingKeyEnum.IsSupportRaiseProcessWhenPlaceBet);
                    eachGame.IsForbiddenGameLaunch = GetBooleanValueBySettingEntity(match, GameListCustomizedSettingKeyEnum.IsForbiddenDirectLaunchGame);

                    if (GetBooleanValueBySettingEntity(match, GameListCustomizedSettingKeyEnum.SupportedOrientation_Landscape))
                    {
                        eachGame.SupportedOrientationCN = "橫版";
                        eachGame.SupportedOrientationEN = "Landscape";
                    }

                    if (GetBooleanValueBySettingEntity(match, GameListCustomizedSettingKeyEnum.SupportedOrientation_Portrait))
                    {
                        eachGame.SupportedOrientationCN = "直版";
                        eachGame.SupportedOrientationEN = "Portrait";
                    }
                }

                if (queryVendorConfigDic.ContainsKey(eachGame.GameVendorType))
                {
                    eachGame.SupportCurrency = queryVendorConfigDic[eachGame.GameVendorType].SupportCurrency;
                }

                eachGame.ExtraTextForKeywordSearch = $"{eachGame.GameVendorTypeText}_{eachGame.ThirdPartyGameCode}_{eachGame.ThirdPartySecondCode}_{eachGame.GameName_EN}";
            }
        }

        private static void SetExtraFilterType(QueryGameListApiRequestParameter filter)
        {
            if (filter.ExtraFilterType.HasValue)
            {
                if (filter.ExtraFilterType == (int)GameListExtraFilterTypeEnum.SingleWalletSupportGame_Support)
                {
                    filter.IsSupportSingleWallet = true;
                }
                else if (filter.ExtraFilterType == (int)GameListExtraFilterTypeEnum.SingleWalletSupportGame_NotSupport)
                {
                    filter.IsSupportSingleWallet = false;
                }
                else if (filter.ExtraFilterType == (int)GameListExtraFilterTypeEnum.RaiseProcessGame_Support)
                {
                    filter.IsSupportRaiseProcess = true;
                }
                else if (filter.ExtraFilterType == (int)GameListExtraFilterTypeEnum.RaiseProcessGame_NotSupport)
                {
                    filter.IsSupportRaiseProcess = false;
                }
                else if (filter.ExtraFilterType == (int)GameListExtraFilterTypeEnum.LaunchProcessAvailableGame_Support)
                {
                    filter.IsForbiddenDirectLaunchGame = false;
                }
                else if (filter.ExtraFilterType == (int)GameListExtraFilterTypeEnum.LaunchProcessAvailableGame_NotSupport)
                {
                    filter.IsForbiddenDirectLaunchGame = true;
                }
                else if (filter.ExtraFilterType == (int)GameListExtraFilterTypeEnum.AGGFreespin_Support)
                {
                    filter.FilterGameCutomizedSettingKey = GameListCustomizedSettingKeyEnum.IsSupportAGGFreespin.ToInteger();
                    filter.FilterGameCutomizedSettingValue = "1";
                }
                else if (filter.ExtraFilterType == (int)GameListExtraFilterTypeEnum.AGGFreespin_NotSupport)
                {
                    filter.FilterGameCutomizedSettingKey = GameListCustomizedSettingKeyEnum.IsSupportAGGFreespin.ToInteger();
                    filter.FilterGameCutomizedSettingValue = "!=1";
                }
            }
        }


        private void CheckGameCodeExist(string gameCode)
        {
            var queryGameCode = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameCode = gameCode
            });

            if (queryGameCode.IsNotEmptyNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
        }



        private bool GetBooleanValueBySettingEntity(List<GameListCustomizedSettingEntity> settingEntities, GameListCustomizedSettingKeyEnum settingKey)
        {
            var matchEntity = settingEntities.Where(x => x.SettingKey == (int)settingKey).FirstOrDefault();

            if (matchEntity == null)
                return false;

            return new SystemConfigSettingParseModel(matchEntity.SettingKey, matchEntity.SettingValue).SettingValueOfBoolean;
        }

    }
}
