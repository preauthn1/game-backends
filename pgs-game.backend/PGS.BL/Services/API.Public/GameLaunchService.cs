using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Models;
using target.Utility;
using target.Models.Enums;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using target.Utility.Extensions.Web;
using game.Entity.Game;
using target.Utility.IOC;

namespace target.BL.Services
{

    public partial class GameLaunchService : BaseVendorService, IGameLaunchService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public PlayerLoginLogRepository PlayerLoginLogRepository { get; set; }

        public PlayerTotalSummaryRepository PlayerTotalSummaryRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public AnnouncementReadRepository AnnouncementReadRepository { get; set; }

        public ThirdPartySlot88MD5RecordRepository ThirdPartySlot88MD5RecordRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public ServerListRepository ServerListRepository { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }

        public PlayerCurrentServerRepository PlayerCurrentServerRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public EazyUniqueKeyValueRecordRepository EazyUniqueKeyValueRecordRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public SingleWalletMerchantSessionTokenExtraInfoRepository SingleWalletMerchantSessionTokenExtraInfoRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public LobbyTokenLogRepository LobbyTokenLogRepository { get; set; }

        public PlayerGameSummaryRepository PlayerGameSummaryRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public PlayerGameTokenLogRepository PlayerGameTokenLogRepository { get; set; }


        public GameLaunchGlobalDebugInfo _globalDebugInfo = new GameLaunchGlobalDebugInfo();


        public IPlayerJwtCheckService PlayerJwtCheckService { get; set; }

        public IPlayerCreateRefService PlayerCreateRefService { get; set; }

        public IEventProcessService EventProcessService { get; set; }

        public IPlayerRTPChangeRefService PlayerRTPChangeRefService { get; set; }



        /// <summary>
        /// 內部測試專用的API
        /// </summary>
        public GameLinkGenerateResultViewModel LaunchGameAndGetGamePageUrlByPlayerID(InternalTestBundleRequestModel model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotMinus();

            int playerID = model.PlayerID;
            int gameID = model.GameID;

            var queryPlayer = PlayerRepository.GetById(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            string sessionToken = [REDACTED];

            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                sessionToken = [REDACTED];
            }

            var resultUrl = LaunchGameAndGetGamePageUrl(new GameLaunchRequestModel
            {
                SessionToken = [REDACTED],
                GameID = gameID,
                PlayerName = queryPlayer.PlayerName,
                MerchantID = queryPlayer.MerchantID,
                BackUrl = model.BackUrl,
                Lang = model.Lang,
                IsFromIframe = model.DoNotIframe //如果有指定不要iframe
            });

            return resultUrl;
        }


        public GameLinkGenerateResultViewModel LaunchGameAndGetGamePageUrl(GameLaunchRequestModel model)
        {
            //最主要的邏輯處理在此
            var result = LaunchGameCommonProcess(model);

            if (result == null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            // Case 1. Iframe網址
            if (result.IsIframeLink)
            {
                return GenerateIframeGamePageLink(model);
            }

            // Case 2. Lobby網址
            if (result.IsLobbyLink)
            {
                return GenerateLobbyLink(model);
            }

            // Case 3. 外部遊戲商網址
            if (result.ExternalVendorInfo != null)
            {
                return InstanceLinkResult(result.ExternalVendorInfo.ExternalVendorUrl, result);
            }

            // Case 4. PGS自製遊戲 網址if
            return GeneratePgsGameRedirectUrl(model.MerchantID, model.GameID, result);
        }



        /// <summary>
        /// 遊戲啟動-通用邏輯
        /// </summary>
        public GameLaunchViewModel LaunchGameCommonProcess(GameLaunchRequestModel param)
        {
            param.RuleFor(x => x.PlayerName).Rule_NotEmptyText().Rule_IsValidAccountFormat();
            param.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            param.RuleFor(x => x.GameID).Rule_NotMinus();

            if (!string.IsNullOrWhiteSpace(param.BackUrl))
            {
                param.RuleFor(x => x.BackUrl).Rule_IsValidUrlFormat();
            }

            if (!string.IsNullOrWhiteSpace(param.ExtraLink))
            {
                param.RuleFor(x => x.ExtraLink).Rule_IsValidUrlFormat();
            }

            param.ExtraLaunchSetting = param.ExtraLaunchSetting ?? new GameLaunchExtraSettingModel();
            param.ExtraLaunchSetting.ExtraLink = param.ExtraLink;

            if (string.IsNullOrWhiteSpace(param.Lang))
            {
                var locale = HeaderValueHelper.GetLocaleHeader();
                if (!string.IsNullOrWhiteSpace(locale))
                {
                    param.Lang = locale;
                }
            }

            if (!RegexRuleHelper.IsSupportLanguage(param.Lang))
            {
                param.Lang = HeaderValueHelper.ConvertLangCodeToEnum(param.Lang).ToDashLangCode();
            }

            MerchantEntity queryMerchant = MerchantRepository.GetSingleByCache(param.MerchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            bool isSingleWalletMode = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);

            //如果是單錢包模式 有額外參數要檢查
            if (isSingleWalletMode)
            {
                param.RuleFor(z => z.SessionToken).Rule_NotEmptyText();
                param.RuleFor(z => z.SessionToken).Rule_MinLengthEnough(5);
                param.RuleFor(z => z.SessionToken).Rule_MaxLengthOK(150);

                if (string.IsNullOrWhiteSpace(param.ExtraSessionTokenRawLarge))
                {
                    param.RuleFor(z => z.ExtraSessionTokenRawLarge).Rule_MaxLengthOK(150);
                }

                //如果是比較新的商戶。檢查某些配置
                if (queryMerchant.CreateTime >= DateTimeHelper.GetCurrentTime().AddDays(-1 * 180) || queryMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
                {
                    var endpointSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);

                    if (endpointSetting == null || endpointSetting.SettingValue.IsNullOrWhiteSpace())
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_SingleWalletEndpointNotSetup, new ErrorDetailModel("SingleWalletApiDomain"));
                    }
                }

                //測試商戶 嚴格執行檢查sessionToken (正式商戶先不要 MYR、MYR100商戶會有陣痛期)
                if (queryMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
                {
                    if (!param.IsAllowDuplicateSessionToken)
                    {
                        CheckSessionTokenDuplicate(queryMerchant.MerchantID, param.SessionToken);
                    }
                }
            }

            //查詢供應商資料、檢查狀態
            this.GetMerchantAndCheckIsValid(param.MerchantID, queryMerchant.ApiSecretKey);

            //內部測試用商戶 會跳過一些檢查
            bool isInternalTestMerchant = MerchantCustomizedSettingRepository.IsInternalTestMerchant(param.MerchantID);

            if (!isInternalTestMerchant)
            {
                //檢查全系統維護 (內部測試商戶 可跳過檢查)
                SystemConfigSettingRepository.CheckSystemMaintainAndThrowException();
            }

            bool isLobbyLink = param.GameID == 0;

            LogBackUrlDomain(param);

            //如果判斷是要導向 大廳Lobby
            if (isLobbyLink)
            {
                return new GameLaunchViewModel
                {
                    IsLobbyLink = true
                };
            }

            GameListEntity queryGame = GetGameAndCheckStatus(param);

            //有些特殊gameCode是禁止透過launch進入的 (ps. 檢查順序必須在 MerchantGameEnable 之前)
            CheckGameLaunchForbiddenGames(queryGame.GameID);

            // 檢查是否有開通遊戲
            var isEnableGame = MerchantGameSettingRepository.IsMerchantGameEnableInSystem(param.MerchantID, param.GameID);

            if (!isEnableGame)
                throw new TerproException(ApiErrorCode.InvalidStatus_MerchantGameMappingDisable, new ErrorDetailModel($"{param.GameID}｜${param.MerchantID}"));

            if (isSingleWalletMode && !queryGame.IsSupportSingleWallet)
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("Seamless"));

            //檢查vendorConfig配置
            CheckVendorConfigMappingEnvironmentIsValid(queryMerchant, param.GameID);

            //判斷是否要強制導向大廳 (商戶有配置的話)
            if (IsRequestFromVendorAPI(param))
            {
                var forceLobbySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_ForceToLobbyWhenLaunchFromVendor);

                if (forceLobbySetting.SettingValueOfBoolean)
                {
                    return new GameLaunchViewModel
                    {
                        IsLobbyLink = true
                    };
                }
            }

            var defineSetting = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(queryGame.GameVendorType);

            if (defineSetting.IsSuuportSkipModeSingleWalletOnly)
            {
                var merchantSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

                if (merchantSetting.SettingValueOfInt != SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "若要進行此款捕魚遊戲。商戶配置需設置 GameSetting_SingleWalletApiLogicMode");
                }
            }

            bool isExternalVendorGame = AppHelper.IsExternalVendor(queryGame.GameVendorType);

            //若指定試玩模式 強制覆蓋參數 (把玩家建成另個商戶的資料)
            if (param.IsTrialModeV2 || param.IsTrialModeToLaunch)
            {
                param.IsTrialModeToLaunch = true;

                //為什麼有些遊戲要禁止 (e.g. Jili) 照理說只要分配sandbox服務器給他就能玩吧? (忘記最初的考量是什麼了...)
                if (!queryGame.IsSupportTrialMerchantLaunch)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("TrialModeToLaunch"));
                }

                var TrialModeMerchant = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.GameSetting_TrialModeDefaultMerchant);

                int trialMerchantID = TrialModeMerchant.SettingValueOfInt;

                if (trialMerchantID <= 0)
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("TrialModeDefaultMerchant"));

                int originalMerchantID = param.MerchantID;
                queryMerchant = MerchantRepository.GetSingleByCache(trialMerchantID);

                string newPlayerName = $"{originalMerchantID}_{param.PlayerName.Trim()}";
                param.PlayerName = newPlayerName;

                AutoDepositTrialWallet(newPlayerName, queryMerchant);

                //如果長度太長 強制使用MD5的編碼 (缺點是看不出追蹤這是哪位了)
                if (param.PlayerName.Length > 50)
                {
                    param.PlayerName = param.PlayerName.MD5Hash();
                }

                param.MerchantID = queryMerchant.MerchantID;
            }

            // 若是第一次launch、新建帳號、相關資料，並返回playerID
            int playerID = FirstTimeLaunchThenCreateNewPlayerData(param.PlayerName, queryMerchant.MerchantID, queryMerchant.Currency);

            param.ExtraLaunchSetting.PlayerID = playerID;

            if (!isInternalTestMerchant)
            {
                //檢查遊戲商維護 (內部測試商戶 可跳過檢查)
                bool isRedirectToLobby = CheckVendorMaintenanceOrRedirectToLobby(queryGame.GameVendorType, playerID, param.MerchantID);

                if (isRedirectToLobby)
                {
                    //設置額外參數 後面會用到
                    param.ExtraLaunchSetting.LobbyLinkWithGameVendorID = queryGame.GameVendorType;

                    return new GameLaunchViewModel
                    {
                        IsLobbyLink = true,
                    };
                }
            }

            //不支援帳務排除模式
            bool isSkipAccountingData = PlayerTagMappingRepository.IsPlayerTagExistByCache(playerID, SystemTagEnum.Player_GameVendorAcoountingIgnoreThisPlayer.ToInteger());

            if (isSkipAccountingData && !queryGame.IsSupportForAccountingIgnorePlayer)
            {
                throw new TerproException(ApiErrorCode.Forbidden_PlayerNotSupportThisGame, new ErrorDetailModel("SupportForAccountingIgnorePlayer"));
            }

            param.ExtraLaunchSetting.IsAccountingIgnoreLaunch = isSkipAccountingData;

            //規格先設計成:
            // 1. 使用VendorAPI、AdminSysAPI 請求產生的網址 一律產生iframe版本的網址 (如果有開啟配置)
            // 2. 透過Lobby或Iframe中轉頁  請求產生的網址 一律為標準遊戲網址
            if (IsRequestFromVendorAPI(param) && !param.InternalParameter_DoNotIframeUrl && param.GameID != 0)
            {
                var iframeSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(param.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsUseIframeGameUrl);

                bool isIframeModeEnable = iframeSetting != null && iframeSetting.SettingValueOfBoolean;

                if (isIframeModeEnable)
                {
                    return new GameLaunchViewModel
                    {
                        IsIframeLink = true
                    };
                }
            }

            if (isExternalVendorGame)
            {
                if (isExternalVendorGame)
                {
                    //三方遊戲的額外參數檢查
                    OverwritLiveCasinoLevelID(param);
                }

                // 判斷為 三方遊戲商
                // 判斷為 三方遊戲商
                // 判斷為 三方遊戲商
                // 判斷為 三方遊戲商
                // 判斷為 三方遊戲商
                // 判斷為 三方遊戲商

                //執行 [外部遊戲商] 的啟動模式 (打對方的API)
                return LaunchGame_ThirdGameVendor(param, queryGame, queryMerchant, playerID);
            }

            string pgsSessionToken = [REDACTED];

            if (IsRequireSingleWalletTokenProcess(isSingleWalletMode, param))
            {
                CreateSingleWalletMerchantToken(param, param.MerchantID, playerID, pgsSessionToken);
            }

            //pgs遊戲只有簡體&英文
            if (AppHelper.IsChineseLang(param.Lang))
            {
                param.Lang = LanguageEnum.zh_cn.ToDashLangCode();
            }
            else
            {
                param.Lang = LanguageEnum.en_us.ToDashLangCode();
            }

            var GameServerDistributeRefService = IOCHelper.ResolveServiceByCode<IGameServerDistributeRefService>();

            //由後端處理 房間分配策略
            var chooseGameServer = GameServerDistributeRefService.ChooseAvailableGameServer(playerID, param.MerchantID, param.GameID, new ServerDistributeOptionalParameter
            {
                RoomFlagLevelID = param.RoomLevelFlagID,
                SecretParameter_ServerID = param.SecretParameter_ServerID
            });

            string myIP = IPHelper.GetIP();

            //寫入一筆登入紀錄
            WritePlayerLoginLog(param, playerID);

            //產生jwtToken 供H5使用
            var jwtResult = MyJwtHelper.GeneratePlayerJwt(playerID, pgsSessionToken, param);

            string jwtHash = jwtResult.JwtToken.MD5Hash();

            //讓所有三方session失效
            ResetPlayerVendorSessionToken(playerID);

            //更新token (讓前次的token自動過期)
            PlayerRepository.UpdateLastLoginTime(playerID);

            var insertToken = [REDACTED] PlayerGameTokenLogEntity
            {
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ExpiredTime = ((double)jwtResult.JwtModel.TokenExpiredTime).UnixTimeStampToDateTime(),
                IPAddress = myIP,
                MerchantID = param.MerchantID,
                PlayerID = playerID,
                TokenHash = jwtHash,
            };

            //寫token紀錄
            PlayerGameTokenLogRepository.ResetTokenAndCreateNew(insertToken);

            //更新登錄次數
            PlayerTotalSummaryRepository.UpdateLoginTimes(playerID);


            //部分設置是存在 CustomizedSetting 這邊
            List<MerchantCustomizedSettingModel> customizedSettings = GetMerchantCustomizedSettings(param.MerchantID);

            var resultViewModel = new GameLaunchViewModel
            {
                PlayerJwtModel = jwtResult.JwtModel,
                IsMultipleServers = chooseGameServer.IsRedirectToHall,
                Info = new GameLaunchRequiredInfoViewModel
                {
                    ServerID = chooseGameServer.ServerID,
                    ServerConnection = chooseGameServer.ServerIPAddress,
                    GameID = queryGame.GameID,
                    Lang = param.Lang,
                    PlayerID = playerID,
                    PlayerName = param.PlayerName,
                    GameCode = queryGame.GameCode?.ToLower(),
                    Access_Token = [REDACTED],
                }
            };

            //客製customized設置
            AssignCustomizedSettingsValueToInfo(customizedSettings, example.invalid);

            //slot88特殊參數設置
            SetSlot88Information(resultViewModel, queryGame);

            return resultViewModel;
        }

        private long WritePlayerLoginLog(GameLaunchRequestModel param, int playerID)
        {
            string myIP = IPHelper.GetIP();

            long logResultID = PlayerLoginLogRepository.Create(new PlayerLoginLogEntity
            {
                Status = NormalActiveStatus.Enable.ToInteger(),
                GameID = param.GameID,
                PlayerID = playerID,
                MerchantID = param.MerchantID,
                MerchantSession = "",
                PlayerName = param.PlayerName,
                CreateIPAddress = myIP,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                HDType = param.DeviceType,
                HDCode = "",
                UserAgent = "", //HeaderValueHelper.GetUserAgent(), 有需要的話自己去查log吧
                LanguageCode = param.Lang.ToLower()
            });

            //這邊紀錄langCode。會作為統計分析使用 (2025/09 已移除) 假如真的有此需求、請重新設計另個table 不要讓Player資料表文字欄位越來越多、影響index負擔
            //var langCode = HeaderValueHelper.ConvertLangCodeToDashFormat(param.Lang);
            //PlayerRepository.UpdateLanguageCode(playerID, langCode);

            return logResultID;
        }


        private bool CheckVendorMaintenanceOrRedirectToLobby(int vendorID, int playerID, int merchantID)
        {
            bool isRedirectToLobby = false;

            try
            {
                //檢查遊戲商維護 (內部測試商戶 可跳過檢查)
                VendorMaintenanceRepository.CheckVendorMaintenanceAndSkipTagThenThrowException(vendorID, playerID);
                return isRedirectToLobby;
            }
            catch (TerproException ex)
            {
                if (ex.ErrroCode == ApiErrorCode.GameUnderMaintain)
                {
                    var hideLobbyGame = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_LobbyGameListInvisible);

                    //如果有指定隱藏大廳遊戲 就不要導過去了(導過去也只是看到空陣列)
                    if (hideLobbyGame.SettingValueOfBoolean)
                    {
                        throw;
                    }

                    //檢查 是否有其他遊戲商的遊戲可玩
                    var queryMaintain = VendorMaintenanceRepository.GetByFilter(new QueryVendorMaintenanceRequestViewModel
                    {
                        IsOnProgressing = true
                    });

                    var queryMyGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
                    {
                        MerchantIDs = merchantID.ToListCollection(),
                        EnableGameOnly = true,
                        Status = NormalActiveStatus.Enable.ToInteger()
                    });

                    List<int> maintainVendors = queryMaintain.Select(z => z.VendorID).ToList();

                    queryMyGames = queryMyGames
                        .Where(z => !maintainVendors.Contains(AppHelper.GetGameVendorByGameID(z.GameID)))
                        .ToList();

                    isRedirectToLobby = queryMyGames.Count() > 0;
                }

                if (!isRedirectToLobby)
                {
                    throw;
                }
            }

            return isRedirectToLobby;
        }

        /// <summary>
        /// 三方遊戲商的啟用遊戲模式
        /// </summary>
        private GameLaunchViewModel LaunchGame_ThirdGameVendor(GameLaunchRequestModel param, GameListEntity queryGame, MerchantEntity queryMerchant, int playerID)
        {
            //檢查 pgs自製遊戲 卡房
            var queryCurrentServer = PlayerCurrentServerRepository.GetByPlayerID(playerID);

            if (queryCurrentServer != null)
            {
                var querySettings = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsSlotsGameRoomLock);

                //如果商戶設置有要求檢查卡房
                bool isCheckRoomLock = querySettings.SettingValueOfBoolean;

                if (isCheckRoomLock)
                {
                    int gameID = AppHelper.GetGameIdByServerID(queryCurrentServer.ServerID);

                    if (gameID == GameListEnum.XLHD_新龍虎鬥.ToInteger() ||
                       gameID == GameListEnum.SML_射門樂.ToInteger() ||
                       gameID == GameListEnum.JZYQ_決戰羽球.ToInteger())
                    {

                    }
                    else
                    {
                        throw new TerproException(ApiErrorCode.Forbidden_DuplicateEnterGameServer);
                    }
                }
            }

            //寫入一筆登入紀錄
            string myIP = IPHelper.GetIP();

            DateTime now = DateTimeHelper.GetCurrentTime();
            DateTime afterOneDay = now.AddDays(1);

            bool isSingleWalletMode = queryMerchant.WalletMode == MerchantWalletModeEnum.SingleWalletMode.ToInteger();
            string pgsSessionToken = [REDACTED];

            //若平台方 採用 [單錢包模式]
            if (IsRequireSingleWalletTokenProcess(isSingleWalletMode, param))
            {
                CreateSingleWalletMerchantToken(param, queryMerchant.MerchantID, playerID, pgsSessionToken);
            }

            //讓所有三方session失效
            ResetPlayerVendorSessionToken(playerID);

            bool isTrialMode = param.IsTestMode || IsAlwaysTrialModeEnable(queryMerchant.MerchantID);

            if (param.ExtraLaunchSetting.IsAccountingIgnoreLaunch)
            {
                isTrialMode = true;
            }

            param.ExtraLaunchSetting.IsTestMode = isTrialMode;

            this._globalDebugInfo.VendorSessionToken = [REDACTED];

            VendorSessionTokenRepository.Create(new VendorSessionTokenEntity
            {
                CreateTime = now,
                UpdateTime = now,
                ExpiredTime = afterOneDay,
                GameID = queryGame.GameID,
                PlayerID = playerID,
                SessionToken = [REDACTED],
                VendorID = queryGame.GameVendorType,
                IPAddress = IPHelper.GetIP(),
                ExtraSettingJson = param.ExtraLaunchSetting?.ConvertToJsonString()
            });

            long resultID = WritePlayerLoginLog(param, playerID);

            if (SystemConstant.CreateAllGameSummaryTableGameIDs.Contains(param.GameID))
            {
                var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    AssignMerchantIDs = param.MerchantID.ToListCollection(),
                    Status = NormalActiveStatus.Enable.ToInteger(),
                    GameVendorTypes = AppHelper.GetGameVendorByGameID(param.GameID).ToListCollection()
                });

                PlayerGameSummaryRepository.CreateDefaultGameSummaryData(playerID, queryGames.Select(z => z.GameID).ToList());
            }
            else
            {
                //新增 玩家遊戲統計統計表
                PlayerGameSummaryRepository.CreateDefaultGameSummaryData(playerID, param.GameID);
            }


            //根據不同遊戲商 實例化不同service實作
            IGameVendorIntegrationService vendorService = ServiceInstanceHelper.InstanceVendorService(queryGame.GameVendorType);

            string launchUrl = string.Empty;

            bool isForceBackUrlPGSLobby = false;
            bool isShowHeaderHomeButton = false;

            try
            {
                // step 1. 先產生三方廠商的遊戲帳號
                vendorService.CreateVendorPlayerAccountOrNot(new ExternalVendorCreateAccountRequestParameter
                {
                    PlayerID = playerID,
                    MerchantID = param.MerchantID,
                    PlayerName = param.PlayerName,
                    Currency = queryMerchant.Currency
                });

                string currencyParse = queryMerchant.Currency;

                var targetLang = AppHelper.ConvertLangToEnum(param.Lang);

                string backUrl = param.BackUrl;

                var forceUrlSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(param.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_ThirdPartyGameForceShowLobbyUrlHomeButton);

                if (forceUrlSetting.SettingValueOfBoolean)
                {
                    backUrl = GenerateLobbyLink(param, isFromGenerateBackurlProcess: true)?.Link.UrlEncode();
                    isForceBackUrlPGSLobby = true;
                }
                else if (string.IsNullOrWhiteSpace(backUrl))
                {
                    bool isShowBackButtonInIframeGamePage = IsShowBackButtonInIframeGamePage(param.MerchantID, queryGame.GameVendorType);
                    bool isAutoGen = IsAutoGenerateBackurl(param.MerchantID);

                    //若啟用在iframe嵌入pgs自製的返回按鈕 則不需帶參數給三方(不過目前實際上只有pgs自製遊戲使用此機制) 
                    if ((isAutoGen && !isShowBackButtonInIframeGamePage))
                    {
                        backUrl = GenerateLobbyLink(param, isFromGenerateBackurlProcess: true)?.Link.UrlEncode();
                        isForceBackUrlPGSLobby = true;
                    }
                }

                string backUrlShort = backUrl;

                //如果返回網址是平台方自己指定的 (非pgs大廳網址)
                if (!string.IsNullOrWhiteSpace(backUrl))
                {
                    string backUrlBase64 = backUrl.UrlDecode().Base64Encode();
                    string lobbyPageUrlDomain = GetLobbyPageUrl(param.MerchantID, param.GameID).Url;

                    if (param.IsFromIframe && AppHelper.GetGameVendorByGameID(param.GameID) == (int)GameVendorTypeEnum.AGG)
                    {
                        //為何這種情況才要檢查 (忘記原因了...?)
                        //如果不是force target lobby 就不會顯示了? (三方返回按鈕 強制使用我方大廳網址)
                        if (isForceBackUrlPGSLobby)
                        {
                            var queryEnableHeaderHomeButton = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsEnableHeaderdHomeButtonForAggGame);
                            if (queryEnableHeaderHomeButton.SettingValueOfBoolean)
                            {
                                isShowHeaderHomeButton = true;
                            }
                        }

                        //若指定開啟。改成自己產生一個按鈕 不透過AGG顯示
                        if (isShowHeaderHomeButton)
                        {
                            backUrl = string.Empty;
                        }
                        else
                        {
                            //backUrl = backUrl;
                        }
                    }
                    else
                    {
                        if (SettingHelper.IsBWG)
                        {
                            //BWG 沒有大廳。不使用這個機制
                        }
                        else
                        {
                            //如果用戶本身已經在iframe中了 直接redirect似乎會有瀏覽器安全問題 因此需要中轉頁這個機制 (最初的設計考量)
                            backUrl = AppHelper.GenerateLobbyRedirectPortalUrl(lobbyPageUrlDomain, backUrlBase64);
                        }
                    }
                }

                // step 2. 取得遊戲網址
                var launchResult = vendorService.GetGameLaunchUrl(new ExternalVendorLaunchUrlRequestModel
                {
                    DeviceType = param.DeviceType,
                    Language = targetLang,
                    Currency = currencyParse,
                    ThirdPartyGameCode = queryGame.ThirdPartyGameCode,
                    GameID = queryGame.GameID,
                    MerchantID = param.MerchantID,
                    PlayerName = param.PlayerName,
                    PlayerID = playerID,
                    SessionToken = [REDACTED],
                    IsTrialMode = isTrialMode,
                    BackUrlEncode = backUrl,
                    BackUrlShort = backUrlShort,
                    LiveCasinoLevelID = param.LiveCasinoLevelID,
                    DebugReferenceUUID = AppHelper.GenerateGameLaunchRequestUUID(resultID)
                });

                launchUrl = launchResult.Url;
            }
            catch (Exception ex)
            {
                PlayerLoginLogRepository.UpdateStatus(resultID, NormalSuccessStatusEnum.Fail);
                throw;
            }

            //產生jwtToken 供H5使用
            var jwtResult = MyJwtHelper.GeneratePlayerJwt(playerID, pgsSessionToken, param);

            string jwtHash = jwtResult.JwtToken.MD5Hash();

            //更新token (讓前次的token自動過期)
            PlayerRepository.UpdateLastLoginTime(playerID);

            //寫token紀錄
            PlayerGameTokenLogRepository.ResetPlayerToken(playerID);

            //更新登錄次數
            PlayerTotalSummaryRepository.UpdateLoginTimes(playerID);

            //活動獎勵相關流程
            EventProcessService.WhenEnterGame(new EventProcessModel
            {
                GameID = queryGame.GameID,
                PlayerID = playerID
            });

            //進入遊戲前 修改RTP
            PlayerRTPChangeRefService.WhenEnterGameChangeRTP(new PlayerRTPChangeWhenEnterGameParameter
            {
                GameID = queryGame.GameID,
                PlayerID = playerID,
                MerchantID = queryMerchant.MerchantID
            });

            return new GameLaunchViewModel
            {
                PlayerID = playerID,
                IsShowGamePageHeader = isShowHeaderHomeButton,
                ExternalVendorInfo = new ExternalVendorLaunchInfoViewModel
                {
                    ExternalVendorUrl = launchUrl
                }
            };
        }


        private bool IsShowBackButtonInIframeGamePage(int merchantID, int gameVendorType, MerchantCustomizedSettingModel preQuerySetting = null)
        {
            bool isShowButton = false;

            if (preQuerySetting == null)
            {
                preQuerySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsShowBackbuttonInIframeGame);
            }

            isShowButton = preQuerySetting.SettingValueOfBoolean;

            if (!isShowButton)
                return false;

            //並非每個遊戲商都支援使用iframe頁的返回按鈕 (e.g. 位置不適合放、或 使用對方自訂的按鈕方案 會更好)
            var isSupportIframePageBackButton = GameVendorDefinitionHelper.IsSupportIframePageBackButton(gameVendorType);
            return isSupportIframePageBackButton;
        }


        private bool IsAutoGenerateBackurl(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsAutoGenerateBackurl);

            if (querySetting == null)
            {
                return false;
            }

            return querySetting.SettingValueOfBoolean;
        }


        /// <summary>
        /// 是否需要建立單錢包商戶token資料
        /// 情境1: 單錢包商戶(必要*)
        /// 情境2: 轉帳商戶(且有開啟特殊配置+有傳sessionToken)
        /// </summary>
        private bool IsRequireSingleWalletTokenProcess(bool isSingleWallet, GameLaunchRequestModel model)
        {
            if (isSingleWallet)
                return true;

            if (model.SessionToken.IsNotEmptyTextNotNull())
            {
                var transferSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.Common_LobbyEnablePlatformTransfer);

                if (transferSetting.SettingValueOfBoolean)
                    return true;
            }

            return false;
        }


        private void CheckSessionTokenDuplicate(int merchantID, string sessionToken)
        {
            bool isSessionTokenExist = IsSingleWalletMerchantSessionTokenAlreadyExist(merchantID, sessionToken);

            if (isSessionTokenExist)
            {
                var duplicateAllow = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_AllowDuplicateSessionToken);

                if (!duplicateAllow.SettingValueOfBoolean)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateToken, new ErrorDetailModel("SessionToken"));
                }
            }
        }


        private void CreateSingleWalletMerchantToken(GameLaunchRequestModel model, int merchantID, int playerID, string pgsSessionToken)
        {
            //只有在Lobby模式下 才允許使用同一組sessionToken (ps. 其實重複也不會有太嚴重的問題)
            if (!model.IsAllowDuplicateSessionToken)
            {
                CheckSessionTokenDuplicate(merchantID, model.SessionToken);
            }

            DateTime now = DateTimeHelper.GetCurrentTime();

            //實際上在 通知商戶 投注/結算 的時候，帶給對方的sessionToken是怎麼來的...?
            // 1. PGS自製遊戲 -> 解析jwtToken取得
            // 2. 三方遊戲 -> 三方token跟這邊的pgsSessionToken 會使用同一組。所以三方通知投注的時候，帶來的token可以用來反查商戶的token (請參考 SingleWalletPlatformApiRequestRefService)
            var insertSessionToken = [REDACTED] SingleWalletMerchantSessionTokenEntity
            {
                SessionTokenPGS = pgsSessionToken,
                SessionTokenRaw = model.SessionToken,
                CreateTime = now,
                PlayerDisplayName = model.PlayerName,
                GameID = model.GameID,
                MerchantID = model.MerchantID,
                PlayerID = playerID,
                UpdateTime = now,
            };

            if (!string.IsNullOrWhiteSpace(model.ExtraSessionTokenRawLarge))
            {
                insertSessionToken.SessionTokenRaw = model.ExtraSessionTokenRawLarge.MD5Hash();

                SingleWalletMerchantSessionTokenExtraInfoRepository.Create(new SingleWalletMerchantSessionTokenExtraInfoEntity
                {
                    SessionTokenPGS = pgsSessionToken,
                    SessionTokenRawLarge = model.ExtraSessionTokenRawLarge,
                    PlayerID = playerID,
                });
            }

            SingleWalletMerchantSessionTokenRepository.Create(insertSessionToken);

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            //請注意這邊的調整可能會改壞W88那種特殊模式 的API 調用
            if (AppHelper.IsSingleWalletMode(queryMerchant.WalletMode))
            {
                List<string> majorCurrency = new List<string>
                {
                    "MYR" , "MYR100" , "THB" , "IDR" , "SGD"
                };

                bool cond_1 = queryMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment;
                bool cond_2 = queryMerchant.CreateTime >= DateTimeHelper.GetCurrentTime().AddDays(-15);
                bool cond_3 = queryMerchant.CreateTime >= DateTimeHelper.GetCurrentTime().AddDays(-35) && !majorCurrency.Contains(queryMerchant.Currency); //30天內創建、且屬於冷門貨幣

                //符合條件的話就預先打callback 確認能打通 (避免到了AGG畫面才出現錯誤...會不好debug)
                if (cond_1 || cond_2 || cond_3)
                {
                    var disablePreCheck = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.DataQuery_VendorAPI_DisablePreCheck_SeamlessCallback);

                    bool isEnablePreCheck = !disablePreCheck.SettingValueOfBoolean;

                    if (isEnablePreCheck)
                    {
                        var queryApiType = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

                        var apiType = (SingleWalletPlatformApiLogicTypeEnum)queryApiType.SettingValueOfInt;

                        var swCallbackInstance = ServiceInstanceHelper.InstanceCallbackService(apiType);

                        //嘗試呼叫balance API 驗證基礎callback是否正常 (sw客戶常常自己沒設定好 在那邊哭妖)
                        swCallbackInstance.GetWalletBalance(new SingleWalletModeWalletApiRequestParameter
                        {
                            SessionTokenPGS = pgsSessionToken,
                            PlayerID = playerID
                        });
                    }
                }
            }

            //產生w88平台的token
            GenerateW88PlatformToken(model, merchantID, pgsSessionToken, playerID);
        }


        private void GenerateW88PlatformToken(GameLaunchRequestModel model, int merchantID, string pgsSessionToken, int playerID)
        {
            var queryMerchantSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

            var apiType = (SingleWalletPlatformApiLogicTypeEnum)queryMerchantSetting.SettingValueOfInt;

            if (apiType != SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit &&
                apiType != SingleWalletPlatformApiLogicTypeEnum.W88)
            {
                return;
            }


            try
            {
                switch (apiType)
                {
                    case SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit:
                    case SingleWalletPlatformApiLogicTypeEnum.W88:

                        //產生w88的token
                        var service = ServiceInstanceHelper.InstanceW88Service();
                        service.GenerateApiToken(new SingleWalletW88WalletApiRequestParameter
                        {
                            SessionTokenPGS = pgsSessionToken,
                            PlayerID = playerID
                        });

                        break;

                    default:
                        throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel("SingleWalletPlatformApiTypeEnum"));
                }

            }
            catch (Exception ex)
            {
                //透過vendorAPI訪問的話要即刻拋錯誤
                if (IsRequestFromVendorAPI(model))
                {
                    throw;
                }
            }
        }

        private bool IsRequestFromVendorAPI(GameLaunchRequestModel model)
        {
            return !model.IsFromIframe && !model.IsFromLobbyPage;
        }

        private bool IsSingleWalletMerchantSessionTokenAlreadyExist(int merchantID, string sessionToken)
        {
            var query = SingleWalletMerchantSessionTokenRepository.GetByFilter(new QuerySingleWalletMerchantSessionTokenRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                SessionTokenRaw = sessionToken
            });

            return query.IsNotEmptyNull();
        }

        private GameLinkGenerateResultViewModel GenerateIframeGamePageLink(GameLaunchRequestModel param)
        {
            var queryMerchant = MerchantRepository.GetSingleByCache(param.MerchantID);

            //若是第一次launch、新建帳號、相關資料，並返回playerID
            int playerID = param.ExtraLaunchSetting.PlayerID.HasValue ? param.ExtraLaunchSetting.PlayerID.Value : 0;

            if (!param.ExtraLaunchSetting.PlayerID.HasValue)
            {
                playerID = FirstTimeLaunchThenCreateNewPlayerData(param.PlayerName, queryMerchant.MerchantID, queryMerchant.Currency);
            }

            //產生jwtToken 供H5使用
            var jwtResult = MyJwtHelper.GeneratePlayerJwt(playerID, "", param);

            string jwtHash = jwtResult.JwtToken.MD5Hash();

            var guidToken = [REDACTED];

            LobbyTokenLogRepository.ResetPlayerLobbyToken(playerID);

            var insertModel = new LobbyTokenLogEntity
            {
                Type = 0,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ExpiredTime = DateTimeHelper.GetCurrentTime().AddDays(2),
                MerchantID = param.MerchantID,
                PlayerID = playerID,
                IdentityHash = jwtHash,
                IdentityGuid = guidToken,
                RawJwtToken = [REDACTED],
                RawRequestInfo = param.ConvertToJsonString()
            };

            CreateLobbyToken(insertModel);

            var pageUrl = GetIframeGamePageUrl(param.MerchantID, param.GameID);

            string tokenParamValue = GetTokenParameterValue(pageUrl.ApiDomainVersion, guidToken);

            //因為之前舊版有跟前端約定好會做base64Encode，新版需保留一樣的模式
            string checkCode = AppHelper.GenerateGameIdCheckCode(param.GameID, playerID);

            string myLink = $"{pageUrl.Url}/?page=game&playerName={param.PlayerName}&gameID={param.GameID}&lang={param.Lang.ToLower()}&checkCode={checkCode}{tokenParamValue}";
            return InstanceLinkResult(myLink, null);
        }


        private string GetTokenParameterValue(LobbyMainApiVersionEnum apiDomainVersion, Guid tokenGuid)
        {
            string tokenText = tokenGuid.ToString();
            string signHash = $"{tokenText}_pgs_{apiDomainVersion.ToInteger()}".MD5Hash();

            //註* 這邊雖然叫做v3Token。但只是參數名稱而已(跟配置version無關)。實際上前端是根據hash值來判斷是v3 v4 v5
            return $"&v3Token=[REDACTED]"; //不能直接用paramKey來識別。因為攻擊者可以自己改參數名稱
        }


        private GameLinkGenerateResultViewModel InstanceLinkResult(string myUrl, GameLaunchViewModel resultViewModel)
        {
            var result = new GameLinkGenerateResultViewModel
            {
                Link = myUrl.ReplaceDuplicateUrlChar(),
                DebugInfo = _globalDebugInfo
            };

            if (resultViewModel != null)
            {
                result.IsShowGamePageHeader = resultViewModel.IsShowGamePageHeader;
                result.PlayerID = resultViewModel.PlayerID;
            }

            if (SettingHelper.IsDev || IPHelper.IsCompanyIP())
            {
                result.DebugInfo = _globalDebugInfo;
            }

            return result;
        }


        private void CreateLobbyToken(LobbyTokenLogEntity insertModel)
        {
            LobbyTokenLogRepository.Create(insertModel);

            //debug使用
            _globalDebugInfo.LobbyToken = [REDACTED];
        }


        private LobbyUrlGenerateModel GetIframeGamePageUrl(int merchantID, int gameID)
        {
            return GetLobbyPageOrIframeUrl(merchantID, SystemConfigSettingEnum.IframePageUrl, gameID);
        }


        private LobbyUrlGenerateModel GetLobbyPageUrl(int merchantID, int gameID)
        {
            return GetLobbyPageOrIframeUrl(merchantID, SystemConfigSettingEnum.LobbyPageUrl, gameID);
        }


        private LobbyUrlGenerateModel GetLobbyPageOrIframeUrl(int merchantID, SystemConfigSettingEnum settingKey, int gameID)
        {
            string merchantSettingUrl = string.Empty;

            //PGS自製遊戲 有專門自訂的域名 (若有指定的話)
            if (AppHelper.IsPgsSelfGame(gameID))
            {
                merchantSettingUrl = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.MerchantCustomized_GamePageDomainUrlForPgsGameOnly)?.SettingValue;
            }

            if (string.IsNullOrWhiteSpace(merchantSettingUrl))
            {
                merchantSettingUrl = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.MerchantCustomized_GamePageDomainUrl)?.SettingValue;
            }

            string myGameLobbyUrl = string.Empty;

            if (!string.IsNullOrWhiteSpace(merchantSettingUrl))
            {
                //case 1. 商戶有設置 優先使用商戶配置
                myGameLobbyUrl = merchantSettingUrl + "/lobby";
            }
            else
            {
                //case 2. 使用全域設置
                var lobbyPageUrl = SystemConfigSettingRepository.GetParseModelByCache(settingKey);

                if (lobbyPageUrl == null || lobbyPageUrl.SettingValue.IsNullOrWhiteSpace())
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel { ColumnName = "LobbyPageDomain" });

                myGameLobbyUrl = lobbyPageUrl.SettingValue;
            }

            //此邏輯已棄用
            //az開頭的網址 (代表azure服務器。網址路徑不會有lobby router)
            //if (myGameLobbyUrl.Contains("https://az.") || myGameLobbyUrl.Contains("http://az."))
            //{
            //    myGameLobbyUrl = myGameLobbyUrl.Replace("/lobby", "");
            //}

            var tokenVersion = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPage_ApiDomainVersion);

            Uri uri = new Uri(myGameLobbyUrl);

            return new LobbyUrlGenerateModel
            {
                ApiDomainVersion = (LobbyMainApiVersionEnum)tokenVersion.SettingValueOfInt,
                Url = myGameLobbyUrl,
                UrlWithoutFolder = $"{uri.Scheme}://{uri.Host}"
            };
        }


        private GameLinkGenerateResultViewModel GenerateLobbyLink(GameLaunchRequestModel param, bool isFromGenerateBackurlProcess = false)
        {
            var queryMerchant = MerchantRepository.GetSingleByCache(param.MerchantID);

            //若是第一次launch、新建帳號、相關資料，並返回playerID
            int playerID = FirstTimeLaunchThenCreateNewPlayerData(param.PlayerName, queryMerchant.MerchantID, queryMerchant.Currency);

            Guid? guidToken = [REDACTED];

            bool isRedirectToLobbyPage = !isFromGenerateBackurlProcess;

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);

            //如果是單錢包需要把 平台方token對應建起來 否則在lobby頁會抓不到錢包金額
            if (isRedirectToLobbyPage && IsRequireSingleWalletTokenProcess(isSingleWallet, param))
            {
                string randomToken = [REDACTED];
                param.IsAllowDuplicateSessionToken = [REDACTED];

                CreateSingleWalletMerchantToken(param, queryMerchant.MerchantID, playerID, randomToken);
            }

            // ---------------------------
            // 呼叫這支function有兩種情境
            // 情境1: 真的要進去Lobby大廳頁
            // 情境2: 只是要自動產生大廳網址(返回按鈕用)
            // ---------------------------

            // 情境2 使用既有token (如果有查到的話)
            // 若重新刷新token會導致 Lobby頁被登出，因此要優先使用既有的token
            if (isFromGenerateBackurlProcess)
            {
                var queryExistToken = [REDACTED] QueryLobbyTokenLogRequestViewModel
                {
                    PlayerIDs = playerID.ToListCollection(),
                    IsNotExpired = true
                })
                    .OrderByDescending(z => z.CreateTime)
                    .FirstOrDefault();

                if (queryExistToken != null)
                {
                    guidToken = [REDACTED];
                }
            }

            bool isGenerateNewToken = [REDACTED] || !guidToken.HasValue;

            // 情境1: 每次都會產一個新大廳token (或是情境2  大廳token不存在)
            if (isGenerateNewToken)
            {
                //產生jwtToken 供H5使用
                var jwtResult = MyJwtHelper.GeneratePlayerJwt(playerID, "", param);

                string jwtHash = jwtResult.JwtToken.MD5Hash();

                LobbyTokenLogRepository.ResetPlayerLobbyToken(playerID);

                guidToken = [REDACTED];

                CreateLobbyToken(new LobbyTokenLogEntity
                {
                    Type = 0,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    ExpiredTime = DateTimeHelper.GetCurrentTime().AddDays(2),
                    MerchantID = param.MerchantID,
                    PlayerID = playerID,
                    IdentityHash = jwtHash,
                    IdentityGuid = guidToken,
                    RawJwtToken = [REDACTED],
                    RawRequestInfo = param.ConvertToJsonString()
                });
            }

            var lobbyPageUrl = GetLobbyPageUrl(param.MerchantID, param.GameID);

            string tokenParamName = GetTokenParameterValue(lobbyPageUrl.ApiDomainVersion, guidToken.Value);

            string maintainVendorParam = string.Empty;
            string categoryParam = string.Empty;

            if (param.ExtraLaunchSetting != null && param.ExtraLaunchSetting.LobbyLinkWithGameVendorID.HasValue)
            {
                maintainVendorParam = $"&targetMaintainVendorID={param.ExtraLaunchSetting?.LobbyLinkWithGameVendorID}";
            }

            if (param.LobbyDefaultCategory.HasValue)
            {
                categoryParam = $"&gameType={param.LobbyDefaultCategory.Value}";
            }

            string link = $"{lobbyPageUrl.Url}/?page=lobby&playerName={param.PlayerName}{tokenParamName}&lang={param.Lang}{maintainVendorParam}{categoryParam}";
            return InstanceLinkResult(link, null);
        }


        private bool IsAlwaysTrialModeEnable(int merchantID)
        {
            var settintType = MerchantCustomizedSettingKeyEnum.System_IsAlwaysTrialMode;

            var query = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, settintType);

            if (query != null)
                return query.SettingValueOfBoolean;

            var defaultValue = SettingKeyEnumLogicHelper.GetMerchantSettingDefaultValueByType(settintType);

            var parsingModel = new MerchantCustomizedSettingModel
            {
                SettingValue = defaultValue,
                CustomizedSettingKey = settintType.ToInteger()
            };

            return parsingModel.SettingValueOfBoolean;
        }

        /// <summary>
        /// 真人視訊的參數檢查
        /// </summary>
        private void OverwritLiveCasinoLevelID(GameLaunchRequestModel param)
        {
            int gameVendorID = AppHelper.GetGameVendorByGameID(param.GameID);

            int aggLiveVendor = GameVendorTypeEnum.AGGLive.ToInteger();
            int galaxyLiveVendor = GameVendorTypeEnum.Galaxy.ToInteger();

            if (gameVendorID != aggLiveVendor && gameVendorID != galaxyLiveVendor)
                return;

            param.ExtraLaunchSetting = param.ExtraLaunchSetting ?? new GameLaunchExtraSettingModel();

            if (string.IsNullOrWhiteSpace(param.LiveCasinoLevelID))
            {
                var replaceSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(param.MerchantID, MerchantCustomizedSettingKeyEnum.AGGLive_DefaultVipLevel);

                if (string.IsNullOrWhiteSpace(replaceSetting.SettingValue))
                    return;

                //如果商戶設定有指定要強制使用某組別
                param.LiveCasinoLevelID = replaceSetting.SettingValue?.Trim();

                if (gameVendorID == galaxyLiveVendor)
                {
                    //galaxy後面不需檢查了
                    return;
                }
            }

            var queryVendorConfig = VendorConfigRepository.GetCacheByVendorTypeAndMerchant(param.MerchantID, aggLiveVendor);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig);

            int vendorConfigID = queryVendorConfig.VendorConfigID;

            #region v1版本的資料儲存方式

            //var querySettings = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            //{
            //    UniqueOnwerQuery = new UniqueParentCustomizedSettingQuery
            //    {
            //        OwnerID = vendorConfigID,
            //        Type = OneToManyCustomizedSettingTypeEnum.LiveCasinoVipLevels_AGGLive
            //    }
            //});

            //if (querySettings.IsEmptyOrNull())
            //    return;

            //param.LiveCasinoLevelID = param.LiveCasinoLevelID.ToUpper();

            //var match = querySettings.FirstOrDefault(z => z.SubCategory?.ToUpper() == param.LiveCasinoLevelID);

            //if (match == null)
            //{
            //    return;
            //}

            ////真實AGGLive對應的數值是table存的這個欄位
            //param.ExtraLaunchSetting.LiveCasinoLevelMappingValue = match.SettingValue;


            #endregion

            //檢查配置是否異常 (v2版本)
            var queryLevelSettings = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.LiveCasinoVipLevels_AGGLiveV2.ToInteger(),
            });

            var matchSetting = queryLevelSettings.FirstOrDefault(x => x.SubCategory == vendorConfigID.ToString());

            var vipNumberText = param.LiveCasinoLevelID.ReplaceNonNumeric();

            if (!vipNumberText.IsNumeric())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("LiveCasinoLevelID"));
            }

            int vipCardID = Int32.Parse(vipNumberText);

            if (matchSetting == null)
            {
                //預設只支援0~4 超過編號5需要定義
                if (vipCardID >= 7)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("Not defined vipLevelIP"));
                }
            }
            else
            {
                var myLevelIDs = new SystemConfigSettingParseModel(0, matchSetting.SettingValue).SettingValueOfListInt;

                if (!myLevelIDs.Contains(vipCardID))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("Not defined vipLevelIP"));
                }
            }

            //真實AGGLive對應的數值是table存的這個欄位
            param.ExtraLaunchSetting.LiveCasinoLevelMappingValue = vipCardID.ToString();

        }


        private void ResetPlayerVendorSessionToken(int playerID)
        {
            VendorSessionTokenRepository.ResetPlayerAllSessionToken(playerID);
        }


        private GameListEntity GetGameAndCheckStatus(GameLaunchRequestModel model)
        {
            var queryGame = GameListRepository.GetById(model.GameID);

            GameListRepository.CheckNullOrStatusThenException(queryGame);

            return queryGame;
        }


        private int FirstTimeLaunchThenCreateNewPlayerData(string playerName, int merchantID, string merchantCurrency)
        {
            var newUserModel = new PlayerCreateParameterModel
            {
                PlayerName = playerName.Trim(),
                MerchantID = merchantID,
                Currency = merchantCurrency,
                IsCheckAccountStatus = true,
            };

            //新建新用戶或返回已存在playerID
            int playerID = PlayerCreateRefService.CreatePlayerOrReturnPlayerID(newUserModel);
            return playerID;
        }


        private void AssignCustomizedSettingsValueToInfo(List<MerchantCustomizedSettingModel> customizedSettings, GameLaunchRequiredInfoViewModel info)
        {
            var styleSetting = customizedSettings.FirstOrDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Style_LoadingStyleMode);
            var clientLogSetting = customizedSettings.FirstOrDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.System_ClientLogEnable);

            info.StyleMode = styleSetting.SettingValueOfInt;
            info.Log = clientLogSetting.SettingValueOfInt;
        }


        /// <summary>
        /// 如果是平台測試商戶。檢查是否配置到錯誤的服務器 (e.g. 配置到正式環境導致要收盤費)
        /// </summary>
        private void CheckVendorConfigMappingEnvironmentIsValid(MerchantEntity queryMerchant, int gameID)
        {
            if (gameID <= 0)
                return;

            if (AppHelper.IsPgsSelfGame(gameID))
                return;

            if (!SettingHelper.IsProduction)
                return;

            //只針對平台測試商戶
            if (queryMerchant.Type != (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
                return;

            int gameVendorID = AppHelper.GetGameVendorByGameID(gameID);

            var query = VendorConfigRepository.GetCacheByVendorTypeAndMerchant(queryMerchant.MerchantID, gameVendorID);

            if (query == null || !query.IsTestEnvironment)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataIncorrect, new ErrorDetailModel("VendorConfig Env"));
            }
        }


        private List<MerchantCustomizedSettingModel> GetMerchantCustomizedSettings(int merchantID)
        {
            List<MerchantCustomizedSettingKeyEnum> settings = new List<MerchantCustomizedSettingKeyEnum>
            {
                MerchantCustomizedSettingKeyEnum.Style_LoadingStyleMode,
                MerchantCustomizedSettingKeyEnum.System_ClientLogEnable
            };

            var queryStyleSetting = MerchantCustomizedSettingRepository.GetMerchantMultipleSettings(merchantID, settings);


            List<MerchantCustomizedSettingModel> results = new List<MerchantCustomizedSettingModel>();


            foreach (var eachEnum in settings)
            {
                var matchSetting = queryStyleSetting.FirstOrDefault(x => x.CustomizedSettingKey == eachEnum.ToInteger());
                var defaultValueResult = SettingKeyEnumLogicHelper.GetMerchantSettingOrDefaultValue(eachEnum, matchSetting);
                results.Add(defaultValueResult);
            }

            return results;
        }


        /// <summary>
        /// 針對特定三方遊戲商有特殊欄位要帶入
        /// </summary>
        /// <param name="resultViewModel"></param>
        /// <param name="queryGame"></param>
        private void SetSlot88Information(GameLaunchViewModel resultViewModel, GameListEntity queryGame)
        {
            var vendorType = (GameVendorTypeEnum)queryGame.GameVendorType;

            if (vendorType == GameVendorTypeEnum.target)
                return;

            if (vendorType == GameVendorTypeEnum.Slot88)
            {
                string guid = RandomHelper.GetGuidWithoutDash();


                var launchInfo = example.invalid;

                var info = new Slot88MD5InfoResultModel
                {
                    PlayerInfo = new Slot88MD5PlayerInfoModel
                    {
                        Access_Token = [REDACTED],
                        Lang = launchInfo.Lang,
                        PlayerID = launchInfo.PlayerID,
                        PlayerName = launchInfo.PlayerName
                    },
                    ServerInfo = new Slot88MDServerInfoModel
                    {
                        ServerConnection = launchInfo.ServerConnection,
                        ServerID = launchInfo.ServerID,
                        GameID = queryGame.GameID,
                        GameVendorType = queryGame.GameVendorType,
                        ConnType = "Websocket"
                    },
                    ExpiredTime = DateTimeHelper.GetCurrentTime().AddMinutes(SettingHelper.PublicApiSetting.Slot88MD5ExpiredMinutes),
                    RandomID = guid
                };

                string beforeMD5 = (guid + SystemConstant.ThirdPartyMD5_SaltKey);
                string MD5 = beforeMD5.MD5Hash();

                ThirdPartySlot88MD5RecordRepository.Create(new ThirdPartySlot88MD5RecordEntity
                {
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    GameID = queryGame.GameID,
                    MD5 = MD5,
                    OriginalJson = info.ConvertToJsonString(),
                    PlayerID = launchInfo.PlayerID,
                });

                launchInfo.ThirdPartyInfo = new ThirdPartyExtraInfoModel
                {
                    GameVendorID = queryGame.GameVendorType,
                    MD5Text = MD5
                };
            }
        }


        private string GetGameLinkDomain(int merchantID, int gameID)
        {
            var lobbyDomain = GetLobbyPageUrl(merchantID, gameID);
            return lobbyDomain.UrlWithoutFolder;

            //如果商戶有自行配置 吃自訂的
            //var queryMerchantSettings = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.MerchantCustomized_GamePageDomainUrl);

            //if (queryMerchantSettings != null && !string.IsNullOrWhiteSpace(queryMerchantSettings.SettingValue))
            //    return queryMerchantSettings.SettingValue;

            ////全系統通用的配置
            //var setting = SystemConfigSettingRepository.GetEntityByEnum(SystemConfigSettingEnum.GamePlayDomain);

            //if (setting == null || setting.SettingValue.IsEmptyOrNull())
            //    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted);

            //return setting.SettingValue;


        }


        /// <summary>
        /// 有些遊戲是禁止透過launch進入的。可能只是作為報表join使用
        /// </summary>
        private void CheckGameLaunchForbiddenGames(int gameID)
        {
            if (SystemConstant.DisableLaunchGameIDs.Contains(gameID))
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportGameLaunch);
            }

            var gameVendor = AppHelper.GetGameVendorByGameID(gameID);

            //減少運算判斷。只檢查特定遊戲商
            if (gameVendor != GameVendorTypeEnum.AGG.ToInteger() &&
               gameVendor != GameVendorTypeEnum.target.ToInteger())
            {
                var setting = GameListCustomizedSettingRepository.GetGameSettingParseModel(gameID, GameListCustomizedSettingKeyEnum.IsForbiddenDirectLaunchGame);

                if (setting.SettingValueOfBoolean)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportGameLaunch);
                }
            }
        }


        private GameLinkGenerateResultViewModel GeneratePgsGameRedirectUrl(int merchantID, int gameID, GameLaunchViewModel result)
        {
            string domain = GetGameLinkDomain(merchantID, gameID);
            string redirectUrl = GenerateRedirectUrl(domain, result);
            return InstanceLinkResult(redirectUrl, result);
        }


        private string GenerateRedirectUrl(string settingUrl, GameLaunchViewModel result)
        {
            //實際上只有info這個物件會包給client & servers使用
            var launchInfo = example.invalid;

            var myObject = new
            {
                data = launchInfo
            };

            string json64Encode = myObject.ConvertToJsonStringCamelCase().Base64Encode();
            string domain = settingUrl.TruncateLastWordIfMatch('/');
            string langCode = launchInfo.Lang?.ToUpper();

            LanguageEnum langConvert = HeaderValueHelper.ConvertLangCodeToEnum(launchInfo.Lang);

            //pgs遊戲只支援兩種語系
            if (langConvert == LanguageEnum.zh_cn || langConvert == LanguageEnum.zh_tw)
            {
                langCode = LanguageEnum.zh_cn.ToDashLangCode().ToUpper();
            }
            else
            {
                langCode = LanguageEnum.en_us.ToDashLangCode().ToUpper();
            }

            string folderName = SettingHelper.IsDev ? "web-mobile-dev" : "web-mobile"; //懶得多加config設定 所以這樣處理

            //導到選廳頁
            if (result.IsMultipleServers)
                return $"{domain}/{folderName}/hall/{langCode}/?gameID={launchInfo.GameID}&styleMode={launchInfo.StyleMode}&token=[REDACTED]";

            //直接導入遊戲頁
            string redirectUrl = $"{domain}/{folderName}/{launchInfo.GameCode.ToLower()}/{langCode}/?{json64Encode}";
            return redirectUrl;
        }


        /// <summary>
        /// 僅供試玩商戶使用的錢包餘額刷新邏輯
        /// </summary>
        /// <returns>回應空值等於沒執行。回應數值等於當前餘額或更新後餘額</returns>
        public string AutoDepositTrialWallet(string playerName, MerchantEntity queryMerchant)
        {
            string defaultResult = "";

            if (queryMerchant.WalletMode != (int)MerchantWalletModeEnum.TransferMode)
                return defaultResult;

            if (queryMerchant.Type == (int)MerchantTypeEnum.Plaform_ProductionEnvironment)
                return defaultResult;

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(playerName, queryMerchant.MerchantID);

            if (queryPlayer == null)
                return defaultResult;

            if (queryPlayer.MerchantID != queryMerchant.MerchantID)
                return defaultResult;

            var defaultBalance = MerchantCustomizedSettingRepository.GetMerchantSettingKeyWithoutCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.DefaultWalletBalance);

            if (defaultBalance.SettingValueOfDecimal > 0)
            {
                try
                {
                    var queryWallet = PlayerWalletRepository.GetPlayerWalletByType(queryPlayer.PlayerID, WalletTypeEnum.GameWallet, isLockQuery: false);

                    if (queryWallet == null)
                        return defaultResult;

                    //錢包餘額少於一半才自動補款 (避免每次進入遊戲都補款)
                    if (queryWallet.Amount <= (defaultBalance.SettingValueOfDecimal / 2))
                    {
                        PlayerWalletRepository.UpdatePlayerWalletForTrialMode(queryPlayer.PlayerID, defaultBalance.SettingValueOfDecimal);
                        return defaultBalance.SettingValueOfDecimal.ToString();
                    }
                    else
                    {
                        return $"{queryWallet.Amount}";
                    }
                }
                catch
                {

                }
            }

            return defaultResult;
        }


        private void LogBackUrlDomain(GameLaunchRequestModel param)
        {
            if (string.IsNullOrWhiteSpace(param.BackUrl))
                return;

            if (!RegexRuleHelper.IsUrlFormat(param.BackUrl))
                return;

            //Newry 泰國商戶
            if (param.MerchantID != 176091)
                return;

            try
            {
                var uri = new Uri(param.BackUrl);

                EazyUniqueKeyValueRecordRepository.CreateIfNotExist(new EazyUniqueKeyValueRecordEntity
                {
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    SubCategory = uri.Host,
                    OwnerID = param.MerchantID,
                    Type = EazyUniqueKeyValueTypeEnum.Merchant_BackUrlDomain.ToInteger(),
                    Memo = ""
                });

            }
            catch
            {

            }
        }



    }
}
