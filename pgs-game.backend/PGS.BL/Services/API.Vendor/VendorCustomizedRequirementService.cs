using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace target.BL.Services
{

    public class VendorCustomizedRequirementService : BaseVendorService, IVendorCustomizedRequirementService
    {
        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public IGameLinkService GameLinkService { get; set; }

        public IGamesService GamesService { get; set; }

        public IThirdPartyGameVendorService ThirdPartyGameVendorService { get; set; }


        public QTechPlatformGameLaunchResponseModel GenerateLaunchLinkQTech(QTechPlatformGameLaunchRequestParameter model)
        {
            model.RuleFor(x => x.currency).Rule_NotEmptyText();
            model.RuleFor(x => x.playerId).Rule_NotEmptyText();
            model.RuleFor(x => x.sessionToken).Rule_NotEmptyText();

            string secrect = HeaderValueHelper.GetHeaderValueByKey("Game-Launch-Pass-Key");

            var queryQTechMerchantIDs = GetMerchantIDsByReferCode(model.operatorId, SingleWalletPlatformApiLogicTypeEnum.QTech);

            var queryMatchMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = queryQTechMerchantIDs,
                Currency = model.currency.ToUpper(),
            });

            if (queryMatchMerchant.Count() >= 2 || queryMatchMerchant.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"currency merchant match {model.currency}"));
            }

            int merchantID = queryMatchMerchant.FirstOrDefault().MerchantID;

            var result = GameLinkService.GenerateSingleWalletLaunchLink(new GameLaunchVendorApiSingleWalletModeRequestModel
            {
                SessionToken = [REDACTED],
                PlayerUniqueID = model.playerId,
                GameID = model.gameId,
                BackUrl = model.returnUrl,
                Lang = model.language,
                MerchantID = merchantID,
                MerchantSecretKey = secrect,
                IsTrialModeV2 = model.mode == "demo" ? true : false
            });

            return new QTechPlatformGameLaunchResponseModel
            {
                url = result.Link
            };
        }


        public QTechPlatformScreenReplayResponseModel GenerateScreenshotReplayLinkQTech(QTechPlatformScreenReplayRequestParameter model)
        {
            model.RuleFor(x => x.roundId).Rule_NotEmptyText();
            model.RuleFor(x => x.playerId).Rule_NotEmptyText();

            var gameRecordService = IOCHelper.Resolve<IGameRecordService>();

            string secrect = HeaderValueHelper.GetHeaderValueByKey("game-result-pass-key");

            gameRecordService.CheckVendorAPIMaintainAndThrow();

            var result = ThirdPartyGameVendorService.GetDetailUrlByTransactionNo(SystemConstant.SkipValidateUserID, model.roundId);

            List<int> queryQTechMerchantIDs = GetMerchantIDsByReferCode(model.operatorId, SingleWalletPlatformApiLogicTypeEnum.QTech);

            if (!queryQTechMerchantIDs.Contains(result.MerchantID))
            {
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow, new ErrorDetailModel($"{result.MerchantID}"));
            }

            GetMerchantAndCheckIsValid(result.MerchantID, secrect);

            return new QTechPlatformScreenReplayResponseModel
            {
                url = result.Url
            };
        }



        public OPSPlatformGameLaunchResponseModel GenerateLaunchLinkOPS(OPSPlatformGameLaunchRequestParameter model)
        {
            model.RuleFor(x => x.game).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.token).Rule_NotEmptyText();
            model.RuleFor(x => x.player_name).Rule_NotEmptyText();

            var opsService = new SWOPSFormatCallbackRefService();

            var queryQTechMerchantIDs = GetOPSMerchantIDs();

            var authResult = opsService.GenerateAuthToken(queryQTechMerchantIDs.FirstOrDefault(), model.token);

            string targetCurrency = authResult.Currency;

            var queryMatchMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = queryQTechMerchantIDs,
                Currency = targetCurrency.ToUpper(),
            });

            if (queryMatchMerchant.Count() >= 2 || queryMatchMerchant.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"currency merchant match {targetCurrency}"));
            }

            var matchMercahnt = queryMatchMerchant.FirstOrDefault();

            int merchantID = matchMercahnt.MerchantID;

            var result = GameLinkService.GenerateSingleWalletLaunchLink(new GameLaunchVendorApiSingleWalletModeRequestModel
            {
                SessionToken = [REDACTED],
                ExtraSessionTokenRawLarge = authResult.Token,
                PlayerUniqueID = model.player_name,
                GameID = model.game,
                BackUrl = model.lobby,
                Lang = model.lang,
                MerchantID = merchantID,
                MerchantSecretKey = matchMercahnt.ApiSecretKey,
                IsTrialModeV2 = model.fun ? true : false
            });

            return new OPSPlatformGameLaunchResponseModel
            {
                url = result.Link
            };
        }


        public OPSPlatformScreenReplayResponseModel GenerateScreenshotReplayLinkOPS(OPSPlatformScreenReplayRequestParameter model)
        {
            model.RuleFor(x => x.game).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.round).Rule_NotEmptyText();

            var gameRecordService = IOCHelper.Resolve<IGameRecordService>();

            gameRecordService.CheckVendorAPIMaintainAndThrow();

            var result = ThirdPartyGameVendorService.GetDetailUrlByTransactionNo(SystemConstant.SkipValidateUserID, model.round);

            List<int> queryQTechMerchantIDs = GetOPSMerchantIDs();

            if (!queryQTechMerchantIDs.Contains(result.MerchantID))
            {
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow, new ErrorDetailModel($"{result.MerchantID}"));
            }

            return new OPSPlatformScreenReplayResponseModel
            {
                url = result.Url
            };
        }


        public THubPlatformGameLaunchResponseModel GenerateLaunchLinkTHub(THubPlatformGameLaunchRequestParameter model)
        {
            model.RuleFor(x => x.GameCode).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.AuthToken).Rule_NotEmptyText();
            model.RuleFor(x => x.ProviderCode).Rule_NotEmptyText();

            var opsService = new SWTHubFormatCallbackRefService();

            var queryThubMerchantIDs = GetMerchantIDsByReferCode(model.ProviderCode, SingleWalletPlatformApiLogicTypeEnum.THub);

            var authResult = opsService.TokenAuth(model.ProviderCode, model.AuthToken, queryThubMerchantIDs.FirstOrDefault());

            string targetCurrency = authResult.Currency;

            var queryMatchMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = queryThubMerchantIDs,
                Currency = targetCurrency.ToUpper(),
            });

            if (queryMatchMerchant.Count() >= 2 || queryMatchMerchant.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"currency merchant match {targetCurrency}"));
            }

            var matchMercahnt = queryMatchMerchant.FirstOrDefault();

            int merchantID = matchMercahnt.MerchantID;

            var result = GameLinkService.GenerateSingleWalletLaunchLink(new GameLaunchVendorApiSingleWalletModeRequestModel
            {
                SessionToken = [REDACTED],
                PlayerUniqueID = authResult.PlayerCode,
                GameID = model.GameCode,
                BackUrl = model.BackUrl,
                Lang = model.Language,
                MerchantID = merchantID,
                MerchantSecretKey = matchMercahnt.ApiSecretKey,
                IsTrialModeV2 = model.Demo ? true : false
            });

            return new THubPlatformGameLaunchResponseModel
            {
                Value = result.Link
            };
        }



        public THubPlatformGetGameReplayScreenshotResponseModel GetGameReplayScreenshotThub(THubPlatformGetGameReplayScreenshotRequestParameter model)
        {
            model.RuleFor(x => x.GameRoundId).Rule_NotEmptyText();

            string provideCode = HeaderValueHelper.GetHeaderValueByKey("ProviderCode");

            if(string.IsNullOrWhiteSpace(provideCode))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter , new ErrorDetailModel("ProviderCode"));
            }

            var gameRecordService = IOCHelper.Resolve<IGameRecordService>();

            gameRecordService.CheckVendorAPIMaintainAndThrow();

            var result = ThirdPartyGameVendorService.GetDetailUrlByTransactionNo(SystemConstant.SkipValidateUserID, model.GameRoundId);

            var queryThubMerchantIDs = GetMerchantIDsByReferCode(provideCode, SingleWalletPlatformApiLogicTypeEnum.THub);

            if(queryThubMerchantIDs.Any(mid=> mid == result.MerchantID) == false)
            {
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow, new ErrorDetailModel($"no merchant's permission"));
            }

            return new THubPlatformGetGameReplayScreenshotResponseModel
            {
                Value = new THubPlatformGetGameReplayScreenshotValueModel
                {
                    Url = result.Url
                }
            };
        }


        public THubPlatformGetGameListResponseModel GetGameListTHub(THubPlatformGetGameListRequestParameter model)
        {
            model.RuleFor(x => x.ProviderCode).Rule_NotEmptyText();
            model.RuleFor(x => x.PageNo).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PageSize).Rule_NotZeroNotMinus();

            var gameVendorService = new SWTHubFormatCallbackRefService();

            var queryReverseMerchantIDs = GetMerchantIDsByReferCode(model.ProviderCode, SingleWalletPlatformApiLogicTypeEnum.THub);

            string timestampText = HeaderValueHelper.GetHeaderValueByKey("x-ts");
            string expectSign = HeaderValueHelper.GetHeaderValueByKey("x-sign");

            if (!SettingHelper.IsLocalhostWebSiteName)
            {
                if (string.IsNullOrWhiteSpace(timestampText))
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("ts"));

                if (string.IsNullOrWhiteSpace(expectSign))
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("x-sign"));
            }

            int matchFirstMerchantID = queryReverseMerchantIDs.FirstOrDefault();

            var queryMerchant = MerchantRepository.GetById(matchFirstMerchantID);

            var signResult = gameVendorService.GenerateSignature(timestampText, matchFirstMerchantID, model);

            if (!SettingHelper.IsMSTest)
            {
                if (signResult.FinalSign != expectSign && expectSign != SystemConstant.OtherAPI_SkipCheck_MagicSignHash)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError, new ErrorDetailModel("Sign"));
                }
            }

            var queryGames = GamesService.GetMerchantEnableGames(new GameInfoRequestModel
            {
                MerchantID = queryMerchant.MerchantID,
                MerchantSecretKey = queryMerchant.ApiSecretKey
            });

            var queryGamesPageSize = queryGames
                .Skip((model.PageNo - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToList();

            int gameTypeSlot = GameTypeEnum.Slot.ToInteger();

            var querySetting = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                GameIDs = queryGamesPageSize.Select(x => x.GameID).ToList(),
                SettingKeys = new List<int>
                {
                    GameListCustomizedSettingKeyEnum.IsSupportBuyFeature.ToInteger(),
                    GameListCustomizedSettingKeyEnum.IsSupportJackpot.ToInteger(),
                }
            });

            var settings_BF = querySetting.Where(x => x.SettingKey == GameListCustomizedSettingKeyEnum.IsSupportBuyFeature.ToInteger()).ToList();
            var settings_JP = querySetting.Where(x => x.SettingKey == GameListCustomizedSettingKeyEnum.IsSupportJackpot.ToInteger()).ToList();

            var result = new THubPlatformGetGameListResponseModel
            {
                Value = new THubPlatformGameListValueModel
                {
                    Total = queryGames.Count(),
                    Games = queryGamesPageSize.Select(g => new THubPlatformGameListDetailItemModel
                    {
                        Name = g.GameNameMappings.FirstOrDefault(z => z.LanguageCode == "en-us")?.Text,
                        Code = g.GameID.ToString(),
                        Type = GameTypeConvert(g.GameType),
                        Category = GameCategoryConvert(g.GameType),
                        AddedTimestamp = g.CreateTime.ConvertToUnixTimestamp() + "000",
                        HasDemo = true,
                        HasFreespin = g.GameType == gameTypeSlot,
                        HasJackpot = settings_JP.FirstOrDefault(x => x.GameID == g.GameID)?.SettingValue == "1",
                        HasBuyFeature = settings_BF.FirstOrDefault(x => x.GameID == g.GameID)?.SettingValue == "1",
                        NameLocalizations = g.GameNameMappings.Select(x => new THubGameListNameLocalizations
                        {
                            Language = LanguageCodeConvert_Thub(x.LanguageCode),
                            Name = x.Text,
                        })
                        .ToList(),
                        Thumbnails = new List<THubGameListNameThumbnails>
                        {
                            new THubGameListNameThumbnails
                            {
                                Language = "en",
                                ImagePath = g.ImageUrl
                            }
                        }
                    })
                    .ToList()
                }
            };

            int debugggg = result.Value.Games.Count();

            return result;
        }


        public NotixGameLaunchResultViewModel GenerateLaunchLinkNotix(NotixGameLaunchRequestParameter model, NotixGameLaunchExtraParameter extraConfig)
        {
            model.RuleFor(x => x.gameId).Rule_NotEmptyText();
            model.RuleFor(x => x.sessionId).Rule_NotEmptyText();
            model.RuleFor(x => x.player).Rule_ModelNotNull();
            extraConfig.RuleFor(x => x.ProviderID).Rule_NotEmptyText();

            string providerCode = extraConfig.ProviderID;

            int targetGameID = Int32.Parse(model.gameId);

            var queryQTechMerchantIDs = GetMerchantIDsByReferCode(providerCode, SingleWalletPlatformApiLogicTypeEnum.Notix);

            string targetCurrency = model.balance?.currency;

            if (string.IsNullOrWhiteSpace(targetCurrency) && extraConfig.IsTrialMode)
            {
                targetCurrency = "MYR";
            }

            var queryMatchMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = queryQTechMerchantIDs,
                Currency = targetCurrency.ToUpper(),
            });

            if (queryMatchMerchant.Count() >= 2 || queryMatchMerchant.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"currency merchant match {targetCurrency}"));
            }

            var matchMercahnt = queryMatchMerchant.FirstOrDefault();

            int merchantID = matchMercahnt.MerchantID;

            var result = GameLinkService.GenerateSingleWalletLaunchLink(new GameLaunchVendorApiSingleWalletModeRequestModel
            {
                SessionToken = [REDACTED],
                PlayerUniqueID = model.player.id,
                GameID = targetGameID,
                BackUrl = model.redirectUrls?.returnUrl,
                Lang = model.browserSession?.language,
                MerchantID = merchantID,
                MerchantSecretKey = matchMercahnt.ApiSecretKey,
                IsTrialModeV2 = extraConfig.IsTrialMode
            });

            return new NotixGameLaunchResultViewModel
            {
                url = result.Link
            };
        }


        public NotixScreenshotAPIResultViewModel GenerateScreenshotReplayLinkNotix(NotixScreenshotAPIRequestParameter model, NotixGameLaunchExtraParameter extraConfig)
        {
            model.RuleFor(x => x.RoundId).Rule_NotEmptyText();

            var gameRecordService = IOCHelper.Resolve<IGameRecordService>();

            gameRecordService.CheckVendorAPIMaintainAndThrow();

            var result = ThirdPartyGameVendorService.GetDetailUrlByTransactionNo(SystemConstant.SkipValidateUserID, model.RoundId);

            List<int> queryReverseMerchantIDs = GetMerchantIDsByReferCode(model.ProviderCode, SingleWalletPlatformApiLogicTypeEnum.Notix);

            if (!queryReverseMerchantIDs.Contains(result.MerchantID))
            {
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow, new ErrorDetailModel($"{result.MerchantID}"));
            }

            var queryMerchant = MerchantRepository.GetById(result.MerchantID);

            GetMerchantAndCheckIsValid(result.MerchantID, queryMerchant.ApiSecretKey);

            return new NotixScreenshotAPIResultViewModel
            {
                reportUrl = result.Url
            };
        }



        public NotixGameListResultViewModel GetGameListNotix(NotixGameListRequestParameter model)
        {
            model.RuleFor(x => x.ProviderCode).Rule_NotEmptyText();

            var opsService = new SWTHubFormatCallbackRefService();

            string notixReferCode = model.ProviderCode;

            var queryQTechMerchantIDs = GetMerchantIDsByReferCode(notixReferCode, SingleWalletPlatformApiLogicTypeEnum.Notix);

            if (queryQTechMerchantIDs.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"not match providerCode"));
            }

            var queryMatchMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = queryQTechMerchantIDs,
            });

            var matchMercahnt = queryMatchMerchant.FirstOrDefault();

            int merchantID = matchMercahnt.MerchantID;

            var queryMerchant = MerchantRepository.GetById(merchantID);

            var gameEnableStatus = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
            });

            List<int> enableGameVendors = gameEnableStatus.Select(x => AppHelper.GetGameVendorByGameID(x.GameID)).Distinct().ToList();

            var enableGames = GamesService.GetMerchantEnableGames(new GameInfoRequestModel
            {
                MerchantID = merchantID,
                MerchantSecretKey = queryMerchant.ApiSecretKey,

                ExtraParameter = new GameListExtraParameter
                {
                    IsIncludeMerchantDisableGame = true,
                    FilterGameVendorIDs = enableGameVendors
                }
            });

            var queryAGGServer = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorIDs = GameVendorTypeEnum.AGG.ToInteger().ToListCollection(),
                IsTestEnvironment = false
            })
            .Where(x => x.SupportCurrency.IsNotEmptyTextNotNull())
            .ToList();

            List<string> supportCurrency = queryAGGServer
                .Select(x => x.SupportCurrency)
                .ToList();

            if (SettingHelper.IsDev)
            {
                supportCurrency.Add("EUR");
                supportCurrency = supportCurrency.Distinct().ToList();
            }
            else if (SettingHelper.IsProduction)
            {
                //正式環境 根據開多少商戶為基準
                supportCurrency = queryMatchMerchant
                    .Select(x => x.Currency)
                    .Distinct()
                    .ToList();
            }

            return new NotixGameListResultViewModel
            {
                games = enableGames.Select(g =>
                {
                    var gameType = ((GameTypeEnum)g.GameType);
                    string gameTypeText = "";

                    switch (gameType)
                    {
                        case GameTypeEnum.Slot:
                            gameTypeText = "SLOT";
                            break;

                        case GameTypeEnum.BoardGame:
                            gameTypeText = "TABLE_GAMES";
                            break;

                        case GameTypeEnum.Live:
                            gameTypeText = "LIVE_BACCARAT";
                            break;

                        case GameTypeEnum.Fishing:
                            gameTypeText = "FISHING";
                            break;

                        default:
                            gameTypeText = "OTHER";
                            break;
                    }

                    var isGameEnable = gameEnableStatus.FirstOrDefault(x => x.GameID == g.GameID && x.Status == NormalActiveStatus.Enable.ToInteger()) != null;

                    var myResult = new NotixGameListItemViewModel
                    {
                        id = g.GameID.ToString(),
                        name = g.GameNameMappings.FirstOrDefault(z => z.LanguageCode == "en-us")?.Text,
                        type = gameTypeText,
                        description = "",
                        isEnabled = isGameEnable,
                        isLive = gameType == GameTypeEnum.Live,
                        mobileHorizontalStatus = "NOT_DEFINED",
                        imageUrl = g.ImageUrl,
                        currencies = supportCurrency,
                        promoFeatures = GeneratePromotionFeatureItems(g.GameType, g.GameID),
                        languages = g.GameNameMappings.Select(x => LanguageCodeConvert_Notix(x.LanguageCode)).ToList(),
                        restrictedCountries = new List<string>(),
                        releaseDate = g.CreateTime
                    };

                    return myResult;
                })
                .ToList()
            };
        }


        private List<NotixPromoFeaturesModel> GeneratePromotionFeatureItems(int gameType, int gameID)
        {
            var gameVendor = (GameVendorTypeEnum)AppHelper.GetGameVendorByGameID(gameID);
            var gameTypeEnum = (GameTypeEnum)gameType;

            var listResult = new List<NotixPromoFeaturesModel>();

            listResult.Add(new NotixPromoFeaturesModel
            {
                type = "DEMO_MODE",
                name = "DEMO_MODE",
                status = "ENABLED",
            });


            if (gameTypeEnum == GameTypeEnum.Slot)
            {
                //listResult.Add(new NotixPromoFeaturesModel
                //{
                //    type = "FREESPINS",
                //    name = "FREESPINS",
                //    status = "ENABLED",
                //});  

                if (gameVendor == GameVendorTypeEnum.Funky)
                {

                }
                else
                {
                    //listResult.Add(new NotixPromoFeaturesModel
                    //{
                    //    type = "PROGRESSIVE_JACKPOT",
                    //    name = "PROGRESSIVE_JACKPOT",
                    //    status = "ENABLED",
                    //});
                }
            }

            if (gameVendor == GameVendorTypeEnum.Funky)
            {

            }
            else if (gameVendor == GameVendorTypeEnum.AGG)
            {

            }

            return listResult;
        }


        private string LanguageCodeConvert_Thub(string langCode)
        {
            var langEnum = HeaderValueHelper.ConvertLangCodeToEnum(langCode);

            switch (langEnum)
            {
                case LanguageEnum.zh_cn:
                    return "zh";

                case LanguageEnum.zh_tw:
                    return "zh-tw";

                default:
                    return "en";
            }
        }

        private string LanguageCodeConvert_Notix(string langCode)
        {
            var langEnum = HeaderValueHelper.ConvertLangCodeToEnum(langCode);

            switch (langEnum)
            {
                case LanguageEnum.zh_tw:
                case LanguageEnum.zh_cn:
                    return "zh";

                default:
                    return "en";
            }
        }



        private int GameTypeConvert(int gameType)
        {
            var gameTypeEnum = (GameTypeEnum)gameType;

            switch (gameTypeEnum)
            {
                case GameTypeEnum.Slot:
                    return 1;

                case GameTypeEnum.Live:
                    return 2;

                case GameTypeEnum.Fishing:
                    return 6;

                default:
                    return 1;
            }
        }

        private int GameCategoryConvert(int gameType)
        {
            var gameTypeEnum = (GameTypeEnum)gameType;

            switch (gameTypeEnum)
            {
                case GameTypeEnum.Slot:
                    return 13;

                case GameTypeEnum.Live:
                    return 10;

                case GameTypeEnum.Fishing:
                    return 8;

                default:
                    return 13;
            }
        }



        private List<int> GetMerchantIDsByReferCode(string referCode, SingleWalletPlatformApiLogicTypeEnum logicType)
        {
            var querySettingMatchMerchants = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                SettingValueMatch = referCode,
                MatchMerchantByLogicType = logicType.ToInteger(),
                CustomizedSettingKeyIDs = new List<int>
                {
                    MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ReverseReferCode.ToInteger()
                }
            });

            return querySettingMatchMerchants.Select(x => x.MerchantID).ToList();
        }


        private List<int> GetOPSMerchantIDs()
        {
            var envCheck = CheckApiDomainReferEnv();

            if (envCheck.IsStaging)
            {
                var queryMappingMerchants = GeneralTagMappingRepository.GetByTagID(GeneralTagEnum.OPS_Merchants_StagingEnv.ToInteger());
                return queryMappingMerchants.Select(x => x.TargetID).ToList();
            }
            else if (envCheck.IsProduction)
            {
                var queryMappingMerchants = GeneralTagMappingRepository.GetByTagID(GeneralTagEnum.OPS_Merchants_ProductionEnv.ToInteger());
                return queryMappingMerchants.Select(x => x.TargetID).ToList();
            }
            else
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("undefined domainUrl "));
            }
        }


        private (bool IsStaging, bool IsProduction) CheckApiDomainReferEnv()
        {
            string urlLower = HeaderValueHelper.GetCurrentRequestDomain();
            urlLower = (urlLower ?? "").ToLower();

            var uat_domains = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.ReverseAPI_UAT_Domains)
                .Select(x => x.SettingValue)
                .Select(x => x.Replace("http://", "").Replace("https://", ""))
                .Select(x => x.ToLower())
                .ToList();

            var pro_domains = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.ReverseAPI_Production_Domains)
                .Select(x => x.SettingValue)
                .Select(x => x.Replace("http://", "").Replace("https://", ""))
                .Select(x => x.ToLower())
                .ToList();


            if (uat_domains.Any(value => value == urlLower))
            {
                return (true, false);
            }
            else if (pro_domains.Any(value => value == urlLower))
            {
                return (false, true);
            }

            return (false, false);
        }
    }
}
