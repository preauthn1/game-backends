using System;
using System.Linq;
using System.Collections.Generic;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.BL.Services;
using target.Models.Models;
using target.Models.ViewModels.Base;
using target.Utility.Helper.Dependency;
using target.Models.CultureText;
using target.Utility.Extensions.Web;
using Lck.Utility.Extensions.Helper;
using target.Models;

namespace target.BL.Interface
{

    public class LobbyService : BaseGameService, ILobbyService
    {

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantGameSettingSelfConfigRepository MerchantGameSettingSelfConfigRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }

        public MerchantSelectMappingItemRepository MerchantSelectMappingItemRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public AnnouncementRepository AnnouncementRepository { get; set; }

        public IPlayerWalletRefService PlayerWalletRefService { get; set; }

        public IPlayerJwtCheckService PlayerJwtCheckService { get; set; }

        public IAnnouncementGeneralService AnnouncementGeneralService { get; set; }


        /// <summary>
        /// Lobby頁面配置 (不抓gameList、資料比較輕量)
        /// </summary>
        public LobbyApiGetGamesInfoViewModel GetLobbySettings(LobbyApiGetGamesRequestModel model)
        {
            model.RuleFor(z => z.Ts).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PlayerName).Rule_NotEmptyText();
            model.RuleFor(z => z.SignKey).Rule_NotEmptyText();

            SignKeyRuleHelper.CheckIsValidPublicApiLobbySignkey(model.Ts, model.PlayerName, model.SignKey);

            string token = [REDACTED];

            var parseResult = GetTokenAndCheckValid(token);

            var parseModel = parseResult.ParsingTokenModel;

            if (parseModel.LoginName != model.PlayerName)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            //方便log紀錄篩選
            HeaderValueHelper.AppendDebugInfo2(parseModel.MerchantID.ToString());

            var queryMerchantSettings = MerchantCustomizedSettingRepository.GetMultipleMerchantSettingByCache(parseModel.MerchantID, new List<MerchantCustomizedSettingKeyEnum>
            {
                MerchantCustomizedSettingKeyEnum.Common_LobbyStyle,
                MerchantCustomizedSettingKeyEnum.Common_LobbyBGMType,
                MerchantCustomizedSettingKeyEnum.Common_LobbySoundDefaultEnable,
                MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName,
                MerchantCustomizedSettingKeyEnum.Common_LobbyEnablePlatformTransfer,
                MerchantCustomizedSettingKeyEnum.Common_MerchantTag_BrandingCategory,

                MerchantCustomizedSettingKeyEnum.GameSetting_IsUseIframeGameUrl,
                MerchantCustomizedSettingKeyEnum.GameSetting_LobbyGameOpenMode,
            });

            var iframeEnableSetting = queryMerchantSettings.GetMatchOrFirstDefault(x=> x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.GameSetting_IsUseIframeGameUrl);
            var lobbyStyle = queryMerchantSettings.GetMatchOrFirstDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_LobbyStyle);
            var gameOpenMode = queryMerchantSettings.GetMatchOrFirstDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.GameSetting_LobbyGameOpenMode);
            var soundEnable = queryMerchantSettings.GetMatchOrFirstDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_LobbySoundDefaultEnable);
            var bgmType = queryMerchantSettings.GetMatchOrFirstDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_LobbyBGMType);
            var brandingName = queryMerchantSettings.GetMatchOrFirstDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName);            
            var lobbyTransfer = queryMerchantSettings.GetMatchOrFirstDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_LobbyEnablePlatformTransfer);            
            var merchantBrandingType = queryMerchantSettings.GetMatchOrFirstDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_MerchantTag_BrandingCategory);

            bool isIframeEnable = iframeEnableSetting?.SettingValueOfBoolean ?? false;

            var viewModel = new LobbyApiGetGamesInfoViewModel
            {
                MerchantID = parseModel.MerchantID,
                PlayerID = parseModel.LoginUserID,
                PlayerName = parseModel.LoginName,
                IsIframeEnable = isIframeEnable,
                BgmType = bgmType.SettingValueOfInt,
                IsSoundDefaultEnable = soundEnable.SettingValueOfBoolean,
                IsOpenNewTab = gameOpenMode.SettingValueOfInt == GameOpenModeEnum.NewTab.ToInteger(),
                IsLobbyTransferEnable = lobbyTransfer.SettingValueOfBoolean,
                SiteDisplayName = brandingName?.SettingValue,
                LobbyStyle = lobbyStyle.SettingValueOfInt,
                IsIONSlot = merchantBrandingType.SettingValueOfInt == (int)SystemTagEnum.MerchantTag_Branding_ION,
                Games = new List<LobbyGamesViewModel>(),
                RecentlyGames = new List<LobbyGamesViewModel>(),
            };

            string backUrl = parseResult.ParsingRawRequest?.BackUrl;

            var externalLinkResult = GetExternalExitLink(backUrl);

            viewModel.ExternalExitLink = externalLinkResult.ExternalExitLink;
            viewModel.ExternalExitLinkRedirectPortalFormat = externalLinkResult.ExternalExitLinkRedirectPortalFormat;
            return viewModel;
        }
            

        public LobbyApiGetGamesInfoViewModel GetPlayerBalanceOnly(LobbyApiGetGamesRequestModel model)
        {
            model.RuleFor(z => z.Ts).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PlayerName).Rule_NotEmptyText();
            model.RuleFor(z => z.SignKey).Rule_NotEmptyText();

            SignKeyRuleHelper.CheckIsValidPublicApiLobbySignkey(model.Ts, model.PlayerName, model.SignKey);

            string token = [REDACTED];

            var parseResult = GetTokenAndCheckValid(token);

            var parseModel = parseResult.ParsingTokenModel;

            if (parseModel.LoginName != model.PlayerName)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            //方便log紀錄篩選
            HeaderValueHelper.AppendDebugInfo2(parseModel.MerchantID.ToString());

            var balanceResult = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(parseModel.LoginUserID, parseModel.SessionTokenPGS , isLockQuery: false);

            decimal balanceAmount = balanceResult.Amount;
            decimal balancePoint = ConvertMoneyToGamePoint(parseModel.MerchantID, balanceAmount);

            var viewModel = new LobbyApiGetGamesInfoViewModel
            {
                PlayerName = parseModel.LoginName,
                Balance = Math.Round(balancePoint , 2),
            };

            return viewModel;
        }


        public virtual LobbyMerchantExternalLinkResult GetExternalExitLink(string myBackUrl, string lobbyPageUrl)
        {
            if (!string.IsNullOrWhiteSpace(lobbyPageUrl))
            {
                var doaminKeyword = lobbyPageUrl.GetDoaminByUrl();

                //必須是客戶自己指定的外部網址才會返回值 (如果是大廳網址的話就不返回)
                if (!myBackUrl.Contains(doaminKeyword))
                {
                    return new LobbyMerchantExternalLinkResult
                    {
                        ExternalExitLink = myBackUrl,
                        ExternalExitLinkRedirectPortalFormat = AppHelper.GenerateLobbyRedirectPortalUrl(lobbyPageUrl, myBackUrl.Base64Encode()).UrlDecode() ,
                    };
                }
            }

            return new LobbyMerchantExternalLinkResult();
        }


        /// <summary>
        /// 抓餘額、大廳設定、遊戲清單。目前是Lobby頁面 才會用到
        /// </summary>
        public LobbyApiGetGamesInfoViewModel GetLobbyGamesAndSettings(LobbyApiGetGamesRequestModel model)
        {
            var viewModel = GetLobbySettings(model);

            string token = [REDACTED];

            var parseResult = GetTokenAndCheckValid(token);

            var parseModel = parseResult.ParsingTokenModel;

            int merchantID = parseModel.MerchantID;

            List<LobbyGamesViewModel> games = GetGamesViewModels(model, parseModel, viewModel);

            var queryInvisibleSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_LobbyGameListInvisible);

            //隱藏所有遊戲列表
            if (queryInvisibleSetting.SettingValueOfBoolean)
            {
                games = new List<LobbyGamesViewModel>();
            }

            viewModel.Games = games;
            viewModel.RecentlyGames = games.Where(z => !z.IsMaintain && z.IsRecentlyGame).OrderByDescending(z => z.LastRecentlyTime).ToList();
            viewModel.IsAllGameMaintain = games.All(z => z.IsMaintain);

            var allEnums = games.Select(z => z.GameType).Distinct().OrderBy(z => z).Select(z => (GameTypeEnum)z).ToList();
            viewModel.GameTypes = allEnums.Select(z => new LobbyGameTypeViewModel
            {
                GameType = z.ToInteger(),
                GameTypeName = MultiLanguageTextHelper.GetEnumsCultureText(z),
            })
            .ToList();

            SetAnnouncementInformation(merchantID, viewModel);
            return viewModel;
        }


        private void SetAnnouncementInformation(int merchantID , LobbyApiGetGamesInfoViewModel viewModel)
        {
            var queryAnns = AnnouncementRepository.GetByFilter(new QueryAnnouncementRequestViewModel
            {
                AssignMerchantIDs = merchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                IsNotExpired = true,
                Types = new List<int>
                {
                    AnnouncementTypeEnum.LobbyAdvertisement.ToInteger()
                },
            });

            if (queryAnns.IsNotEmptyNull())
            {
                viewModel.IsAdvertisementLayout = true;

                var annViewModel = AnnouncementGeneralService.ConvertToAnnoucementContentModel(merchantID, queryAnns);

                viewModel.AdvertisementItems = annViewModel.Items;
            }
        }


        private LobbyTagGameModel GetLobbyInvisibleGames(int merchantID)
        {
            string cacheKey = CacheKey.Lobby_InvisibleGames_Merchant(merchantID);

            var lobbyTagGameIDs = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var lobbyVisible = MerchantGameSettingSelfConfigRepository.GetByFilter(new QueryMerchantGameSettingSelfConfigRequestViewModel
                   {
                       MerchantIDs = merchantID.ToListCollection(),
                   });

                   return new LobbyTagGameModel
                   {
                       HotGameIDs = lobbyVisible.Where(x => x.IsHot).Select(z => z.GameID).ToList(),
                       NewGameIDs = lobbyVisible.Where(x => x.IsNew).Select(z => z.GameID).ToList(),
                       InvisibleGameIDs = lobbyVisible.Where(x => x.IsLobbyVisible == false).Select(z => z.GameID).ToList(),
                   };

               }, expiredMinute: 15);


            lobbyTagGameIDs.HotGameIDs = lobbyTagGameIDs.HotGameIDs ?? new List<int>();
            lobbyTagGameIDs.NewGameIDs = lobbyTagGameIDs.NewGameIDs ?? new List<int>();
            lobbyTagGameIDs.InvisibleGameIDs = lobbyTagGameIDs.InvisibleGameIDs ?? new List<int>();

            //排除2. 特殊的gameID
            lobbyTagGameIDs.InvisibleGameIDs.AddRange(SystemConstant.DisableLaunchGameIDs);

            List<int> customizedSettingGameIDs = LobbyDisabledGamesByCustomizedSetting();

            //排除3. 有標記特殊配置的game
            if (customizedSettingGameIDs.IsNotEmptyNull())
            {
                lobbyTagGameIDs.InvisibleGameIDs.AddRange(customizedSettingGameIDs);
            }

            return lobbyTagGameIDs;
        }


        private List<int> LobbyDisabledGamesByCustomizedSetting()
        {
            var cacheKey = CacheKey.Lobby_DisabledGames();

            var settingValueGameIDs = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var query = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
                   {
                       SettingKey = GameListCustomizedSettingKeyEnum.IsForbiddenDirectLaunchGame.ToInteger()
                   });

                   if (query.IsEmptyOrNull())
                       return new List<int>();

                   return query
                       .Where(q =>
                       {
                           var parse = new SystemConfigSettingParseModel(q.SettingKey, q.SettingValue);
                           return parse.SettingValueOfBoolean;
                       })
                       .Select(x=> x.GameID)
                       .ToList();

               }, 5);

            return settingValueGameIDs;
        }


        private List<LobbyGamesViewModel> GetGamesViewModels(LobbyApiGetGamesRequestModel model, PlayerTokenJwtModel parseModel, LobbyApiGetGamesInfoViewModel viewModel)
        {
            int playerID = parseModel.LoginUserID;

            var queryEnableGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = parseModel.MerchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                EnableGameOnly = true
            });

            var imageSetting = DbValueCacheHelper.GetImageDomainAndStyleType(parseModel.MerchantID);

            var lobbyTagGames = GetLobbyInvisibleGames(parseModel.MerchantID);

            List<int> lobbyDisalbeGames = lobbyTagGames.InvisibleGameIDs;

            List<int> lobbyVisibleGameIDs = queryEnableGames
                .Where(x => !lobbyDisalbeGames.Contains(x.GameID))
                .Select(x => x.GameID)
                .ToList();

            var gameBriefList = DbValueCacheHelper.GetGameNameBriefModels(lobbyVisibleGameIDs);

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(lobbyVisibleGameIDs);

            var games = gameBriefList
                        .Select(game =>
                        {
                            var convert = new LobbyGamesViewModel
                            {
                                GameID = game.GameID,
                                GameVendorID = game.GameVendorType,
                                GameType = game.GameType,
                                GameName = gameNameDic.GetDictionaryOrDefaultValue(game.GameID),
                                GameImageUrl = AppHelper.GenerateGameImageUrl(imageSetting.ImageDomainUrl, game.GameCode , imageSetting.StyleType),
                                CheckCode = AppHelper.GenerateGameIdCheckCode(game.GameID, playerID),
                                GameCreateTime = game.CreateTime,
                            };
                            return convert;
                        }).ToList();

            var queryMaintain = VendorMaintenanceRepository.GetByFilter(new QueryVendorMaintenanceRequestViewModel
            {
                IsOnProgressing = true,
            });
                    
            if (queryMaintain.IsNotEmptyNull())
            {
                List<int> templateIDs = queryMaintain
                    .Where(x => x.ExcludeMerchantTemplateID.HasValue)
                    .Select(x => x.ExcludeMerchantTemplateID.Value)
                    .ToList();

                List<int> maintainVendors = new List<int>();

                foreach (var item in queryMaintain)
                {
                    //如果有配置排除特定代理
                    if (item.ExcludeMerchantTemplateID.HasValue)
                    {
                        var excludeMerchants = MerchantSelectMappingItemRepository.GetAvailableMerchantIDsByTemplateID(item.ExcludeMerchantTemplateID.Value);

                        if (!excludeMerchants.Contains(parseModel.MerchantID))
                        {
                            maintainVendors.Add(item.VendorID);
                        }
                    }
                    else
                    {
                        maintainVendors.Add(item.VendorID);
                    }
                }
                               
                if(maintainVendors.IsNotEmptyNull())
                {
                    //如果有配置tag或商戶進階設置。即使維護中也要顯示遊戲
                    bool isPlayerSkipMaintainEnable = VendorMaintenanceRepository.IsPlayerCanSkipMaintainByTagOrSetting(playerID);

                    if(!isPlayerSkipMaintainEnable)
                    {
                        foreach (var item in games)
                        {
                            item.IsMaintain = maintainVendors.Contains(AppHelper.GetGameVendorByGameID(item.GameID));
                        }
                    }
                }
            }

            foreach (var item in games)
            {
                if(lobbyTagGames.HotGameIDs.Contains(item.GameID))
                {
                    item.IsHotGame = true;
                }

                if (lobbyTagGames.NewGameIDs.Contains(item.GameID))
                {
                    item.IsNewGame = true;
                }

                item.MustShowIframeBackButton = GameVendorDefinitionHelper.IsSupportIframePageBackButton(item.GameVendorID);
            }

            if (model.TargetMaintainVendorID.HasValue)
            {
                var matchMaintain = queryMaintain.FirstOrDefault(x => x.VendorID == model.TargetMaintainVendorID.Value);

                bool isAnyGameNotMaintain = games.Any(x => !x.IsMaintain);

                if (matchMaintain != null && isAnyGameNotMaintain)
                {
                    viewModel.MaintainMsg = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_HintMsg_SelectedGameVendorUnderMaintain");
                    string extraText = string.Empty;

                    if ((matchMaintain.EndTime - matchMaintain.StartTime).TotalHours <= 16)
                    {
                        extraText = $" (GMT+8 {matchMaintain.StartTime.AddHours(8).ToString("HH:mm")}-{matchMaintain.EndTime.AddHours(8).ToString("HH:mm")})";
                    }

                    viewModel.MaintainMsg = string.Format(viewModel.MaintainMsg, extraText);
                }
            }

            List<int> sortPriorityGameIDs = new List<int>();

            var sortSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(parseModel.MerchantID, MerchantCustomizedSettingKeyEnum.Lobby_LobbyGameSortType);

            bool isDefaultSort = sortSetting.SettingValueOfInt == (int)LobbyGameSortEnum.Default;

            //若指定用遊戲投注排名
            if (sortSetting.SettingValueOfInt == (int)LobbyGameSortEnum.BetAmountRanking)
            {
                sortPriorityGameIDs = GetBetAmountRankingGames(parseModel.MerchantID);
            }
            else if(sortSetting.SettingValueOfInt > 1000)
            {
                var querySetting = OneToManyCustomizedSettingItemRepository.GetSingleMatchParseModel(new QueryOneToManyCustomizedSettingItemRequestViewModel
                {
                    OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.PgsSystem_RecommendGamesOrder.ToInteger(),
                    SubCategory = sortSetting.SettingValueOfInt.ToString(),
                });

                sortPriorityGameIDs = querySetting.SettingValueOfListInt;
            }
            else if(isDefaultSort)
            {
                var querySetting = OneToManyCustomizedSettingItemRepository.GetSingleMatchParseModel(new QueryOneToManyCustomizedSettingItemRequestViewModel
                {
                    OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.PgsSystem_RecommendGamesOrder.ToInteger(),
                    SubCategory = SystemConstant.DefaultGameOrderID.ToString(),
                });

                sortPriorityGameIDs = querySetting.SettingValueOfListInt;
            }

            //排序遊戲
            games = games
                .OrderBy(z => z.IsMaintain ? 999 : 0)
                //.ThenBy(z => z.IsNewGame ? 0 : 200)
                //.ThenBy(z => z.IsHotGame ? 0 : 400)
                .ThenBy(z=> 
                {
                    if(sortPriorityGameIDs.Contains(z.GameID))
                    {
                        return sortPriorityGameIDs.IndexOf(z.GameID);
                    }

                    return 9999 + z.GameID;    
                })
                .ThenBy(z => z.GameName.Length <= 15 ? 0 : 500) //字數太多的顯示在後面
                .ThenBy(z => z.GameID)
                .ToList();

            int sequence = 0;
            games.ForEach(item => item.Sequence = ++sequence);
            return games;
        }


        private List<int> GetBetAmountRankingGames(int merchantID)
        {
            var cacheKey = CacheKey.Lobby_BetRankingGames(merchantID);

            var priorityGames = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   DateTime begin = DateTimeHelper.GetCurrentTime().AddDays(-30);
                   DateTime end = DateTimeHelper.GetCurrentTime();

                   var gamesSummary = MerchantGameSummaryDataRepository.GetEachGameTotalSumOnDuration(merchantID.ToListCollection(), null, begin, end);

                   return gamesSummary
                       .OrderByDescending(x => x.BetAmount)
                       .Select(x => x.GameID)
                       .ToList();
               }, 10);

            return priorityGames;
        }





        private LobbyMerchantExternalLinkResult GetExternalExitLink(string myBackUrl)
        {
            if (!string.IsNullOrWhiteSpace(myBackUrl))
            {
                var lobbyPageUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageUrl);
                return GetExternalExitLink(myBackUrl, lobbyPageUrl.SettingValue);
            }

            return new LobbyMerchantExternalLinkResult();
        }


        /// <summary>
        /// ps. MSTest有Mock這段
        /// </summary>
        protected virtual LobbyTokenParsingResultModel GetTokenAndCheckValid(string token)
        {
            var parseModel = PlayerJwtCheckService.ParsingLobbyToken(token);
            return parseModel;
        }


    }
}
