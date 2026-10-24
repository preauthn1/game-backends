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
using target.Utility.Utility.ThirdParty;
using target.Utility;

namespace target.BL.Services
{

    public class MerchantService : BaseAdminSysService, IMerchantService
    {

        public AccountingGroupCustomizedSettingRepository AccountingGroupCustomizedSettingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public VendorConfigMerchantMappingRepository VendorConfigMerchantMappingRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public ServerMerchantMappingRepository ServerMerchantMappingRepository { get; set; }

        public IMChannelCustomizedSettingRepository IMChannelCustomizedSettingRepository { get; set; }

        public AccountingAdjustmentRuleRepository AccountingAdjustmentRuleRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantHierarchyChildMappingRepository MerchantHierarchyChildMappingRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public MerchantCreditRepository MerchantCreditRepository { get; set; }

        public UserRepository UserRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        public const string UnknowPassword = [REDACTED] password already changed*";

        /// <summary>
        /// 如果這個物件是NULL 請呼叫過一次 PreQueryUrlSettingForGlobalVariable
        /// </summary>
        public PreQueryDocumentUrlGlobalVariableModel _preQueryDocumentUrl { get; set; }


        public IMerchantHierarchyService MerchantHierarchyService { get; set; }

        public IPlayerTokenLogoutRefService PlayerTokenLogoutService { get; set; }


        public PagedList<MerchantViewModel> GetByPagingFilter(QueryMerchantApiRequestParameterModel filter)
        {
            filter.OrganizationIDs = filter.OrganizationID.ToIntegerListCollection();

            //特殊標籤的篩選查詢
            SetSearchFilterBySpecialFilterTag(filter);

            if (filter.IMChannelMappingID.IsNotEmptyTextNotNull())
            {
                filter.MerchantCustomizedSetting_SettingKey = MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger();
                filter.MerchantCustomizedSetting_SettingValue = filter.IMChannelMappingID;
            }

            if (filter.Keyword.IsNotEmptyTextNotNull() && filter.Keyword.Count() > 10)
            {
                List<int> merchantIDs = AppHelper.AnalyzeIDsFromRawText(filter.Keyword);

                if (merchantIDs.IsNotEmptyNull() && merchantIDs.Count() >= 2)
                {
                    filter.MerchantIDs = merchantIDs;
                    filter.Keyword = string.Empty;
                }
            }

            var query = MerchantRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<MerchantEntity, MerchantViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            //先查出代理資訊。方便排序運算
            SetAgentHierarchyInfo(viewModels.Items);

            int maxSize = 500;

            if (filter.DataTableFetchMaxSize.NotNullNotZeroNotMinus())
            {
                maxSize = filter.DataTableFetchMaxSize.Value;
            }

            var now = DateTimeHelper.GetCurrentTime();

            if (filter.IsSpecialOrder && viewModels.Items.Count() >= maxSize)
            {
                viewModels.Items = viewModels.Items
                    .OrderByDescending(m =>
                    {
                        //總代可以顯示
                        //2、3級代理只顯示內部人員建的。不顯示代理自己建的(垃圾帳號很多)
                        if (m.HierarchyLayer <= 1)
                        {
                            return m.MerchantID;
                        }
                        else
                        {
                            if (m.CreateBy == "admin" || m.CreateBy == "marshall" ||
                                m.CreateBy == "rory" || m.CreateBy == "target-Joker")
                            {
                                return m.MerchantID;
                            }
                            else
                            {
                                //排序較後面
                                return m.MerchantID - 100000;
                            }
                        }
                    })
                    .Take(maxSize) //考慮到未來資料還會持續增長 介面上趁早限制筆數比較好
                    .ToList();
            }
            else
            {
                viewModels.Items = viewModels.Items.OrderByDescending(x => x.MerchantID).ToList();
            }

            //精簡資料筆數後。再查出額外資訊
            SetExtraProperty(viewModels.Items, filter);
            return viewModels;
        }


        public MerchantViewModel GetByID(int merchantID)
        {
            var queryMerchant = MerchantRepository.GetById(merchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            var convert = ConvertToViewModels(queryMerchant);
            return convert;
        }


        public MerchantCopyInfoPreviewViewModel GetCopyPreviewInfo(int userID, int merchantID)
        {
            var queryMerchant = MerchantRepository.GetById(merchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            CheckViewMerchantPermission(userID, merchantID);

            var convert = ConvertToViewModels(queryMerchant);

            //預先查出一些重要全域變數
            PreQueryUrlSettingForGlobalVariable();

            var queryEnableGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                Status = NormalActiveStatus.Enable.ToInteger(),
                MerchantIDs = merchantID.ToListCollection()
            });

            var queryEnableServers = ServerMerchantMappingRepository.GetByFilter(new QueryServerMerchantRelationRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
            });

            bool isAllPGSGames = queryEnableGames.All(z => !AppHelper.IsExternalVendorGame(z.GameID));

            bool isAnyPGSGames = queryEnableGames.Count(z => !AppHelper.IsExternalVendorGame(z.GameID)) > 0;

            bool isAnyThirdPartyGame = queryEnableGames.Where(z => AppHelper.IsExternalVendorGame(z.GameID)).Count() > 0;

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);

            var swTokenSettingKey = MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken;

            MerchantCustomizedSettingModel provideSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Common_IsCredentialProvideCompleted);
            MerchantCustomizedSettingModel swPgsIdentityToken = [REDACTED], swTokenSettingKey);

            decimal queryConvertRate = MerchantCustomizedSettingRepository.GetMerchantGamePointConvertRateByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_PGS);

            var queryMappingVendorInfo = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            });

            bool isProvideComplete = provideSetting == null ? false : provideSetting.SettingValueOfBoolean;

            bool isAnyPgsGameNotMappingServer = false;

            if (isAnyPGSGames)
            {
                //檢查是否有特定哪一款遊戲漏分配伺服器
                foreach (var item in queryEnableGames.Where(z => !AppHelper.IsExternalVendorGame(z.GameID)))
                {
                    //serverID的編號規範是....前四碼為gameID
                    bool isServerMappingExist = queryEnableServers.Where(z => (z.ServerID / 100000) == item.GameID).Any();

                    if (!isServerMappingExist)
                    {
                        isAnyPgsGameNotMappingServer = true;
                        break;
                    }
                }
            }

            List<MappingVendorConfigViewModel> mappings = new List<MappingVendorConfigViewModel>();

            bool isProductionPlatform = (queryMerchant.Type == (int)MerchantTypeEnum.Plaform_ProductionEnvironment || queryMerchant.Type == (int)MerchantTypeEnum.NoDataOrGhostPlatform);

            var enableGameVendorsViewModels = new List<MerchantVendorConfigSettingAndEnableGamesItemViewModel>();

            if (isAnyPGSGames)
            {
                var gameVendor = GameVendorTypeEnum.target;
                enableGameVendorsViewModels.Add(new MerchantVendorConfigSettingAndEnableGamesItemViewModel
                {
                    EnableGames = queryEnableGames.Where(z => AppHelper.IsPgsSelfGame(z.GameID)).Count(),
                    ConvertRate = queryConvertRate,
                    GameVendor = gameVendor.ToInteger(),
                    GameVendorName = MultiLanguageTextHelper.GetEnumsCultureText(gameVendor),
                    VendorConfigID = 0,
                    VendorConfigName = "PGS默認",
                });
            }

            //如果商戶是屬於正式營運商戶 要做vendorConfig配置檢查
            if (isAnyThirdPartyGame)
            {
                List<int> enableThirdGameIDs = queryEnableGames
                    .Where(z => AppHelper.IsExternalVendorGame(z.GameID))
                    .Select(z => z.GameID)
                    .ToList();

                var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameIDs = enableThirdGameIDs
                });

                List<int> enableGameVendors = queryGames.Select(z => z.GameVendorType).Distinct().ToList();

                foreach (int eachGameVendor in enableGameVendors)
                {
                    var matchVendorConfig = queryMappingVendorInfo.FirstOrDefault(z => z.VendorID == eachGameVendor);

                    GameVendorTypeEnum gameVendorEnum = (GameVendorTypeEnum)eachGameVendor;

                    if (matchVendorConfig == null)
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"{gameVendorEnum.ToString()} 服務器配置不正確 請先設定"));
                    }

                    mappings.Add(new MappingVendorConfigViewModel
                    {
                        VendorID = matchVendorConfig?.VendorID ?? 0,
                        VendorConfigID = matchVendorConfig?.VendorConfigID ?? 0,
                        VendorConfigText = matchVendorConfig?.Name ?? "",

                        //production環境才要檢查
                        IsWarning = isProductionPlatform ? (matchVendorConfig?.IsTestEnvironment ?? false) : false
                    });

                    var settingKey = AppHelper.GetConvertRateCustomizedSettingKey(eachGameVendor);
                    decimal convertRateOfVendor = MerchantCustomizedSettingRepository.GetMerchantGamePointConvertRateByCache(merchantID, settingKey.SettingKey);

                    bool isWarningVendorConfig = false;

                    //正式商戶 檢查vendorConfig是否異常
                    if (isProductionPlatform)
                    {
                        if (matchVendorConfig.IsTestEnvironment)
                        {
                            isWarningVendorConfig = true;
                        }
                        else if (matchVendorConfig.SupportCurrency.IsNotEmptyTextNotNull() && matchVendorConfig.SupportCurrency != queryMerchant.Currency)
                        {
                            isWarningVendorConfig = true;
                        }
                    }
                    else
                    {
                        //如果是平台測試 也反過來檢查是否
                        if (queryMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
                        {
                            if (matchVendorConfig != null && !matchVendorConfig.IsTestEnvironment)
                            {
                                isWarningVendorConfig = true;
                            }
                        }
                    }

                    var newInstance = new MerchantVendorConfigSettingAndEnableGamesItemViewModel
                    {
                        EnableGames = queryEnableGames
                            .Where(z => AppHelper.GetGameVendorByGameID(z.GameID) == eachGameVendor)
                            .Count(),

                        //production環境才要檢查
                        IsWarning = isWarningVendorConfig,
                        GameVendor = gameVendorEnum.ToInteger(),
                        GameVendorName = MultiLanguageTextHelper.GetEnumsCultureText(gameVendorEnum),
                        ConvertRate = convertRateOfVendor,
                    };

                    if(matchVendorConfig != null)
                    {
                        newInstance.VendorConfigID = matchVendorConfig.VendorConfigID;
                        newInstance.VendorConfigName = matchVendorConfig.Name;
                    }
                    else
                    {
                        newInstance.VendorConfigName = $"⚠️尚未配置**";
                    }

                    enableGameVendorsViewModels.Add(newInstance);
                }
            }

            var queryHierarchy = MerchantHierarchyRepository.GetById(queryMerchant.MerchantID);

            bool isRootAgent = queryHierarchy?.RootMerchantID == queryMerchant.MerchantID;

            bool isMerchantTypeForProduction = queryMerchant.Type == MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger();

            var result = new MerchantCopyInfoPreviewViewModel
            {
                ExtraHintTags = new List<string>(),
                IsMerchantHierarchy = queryHierarchy != null,
                IsProvideComplete = isProvideComplete,
                IsMerchantTypeForProduction = isMerchantTypeForProduction,
                Info = convert,
                SingleWalletIdentityToken = [REDACTED],
                EnableGameVendorGames = enableGameVendorsViewModels.OrderBy(z => z.GameVendor).ToList(),
                IsEnableGameEmpty = queryEnableGames.Count() == 0,
                IsEnablePGSGameOnlyButServerEmpty = isAnyPGSGames && queryEnableServers.Count() == 0,
                IsAnyPgsGameNotMappingServer = isAnyPgsGameNotMappingServer,
                EnablePgsGames = queryEnableGames.Where(z => !AppHelper.IsExternalVendorGame(z.GameID)).Count(),
                EnableThirdPartyGames = queryEnableGames.Where(z => AppHelper.IsExternalVendorGame(z.GameID)).Count(),
                EnablePgsGameServers = queryEnableServers.Count(),
                PGSConvertRate = queryConvertRate,
                VendorConfigMappingsText = string.Join("、", queryMappingVendorInfo.Select(z => z.Name)),
                VendorConfigMappings = mappings,
                Memo_CN = new List<string>(),
                Memo_EN = new List<string>(),
            };

            //單錢包的一些額外檢查
            if (isSingleWallet)
            {
                //補救資料 (缺少設定)
                //補救資料 (缺少設定)
                //補救資料 (缺少設定)
                RecoverSingleWalletMissingData(merchantID);

                string resultMsg = CheckNotSupportSingleWalletGame(merchantID);

                if (!string.IsNullOrWhiteSpace(resultMsg))
                {
                    result.ExtraHintTags.Add(resultMsg);
                }
            }


            var myIMChannel = IMChannelRepository.GetMerchantIMChannel(queryMerchant.MerchantID);

            if (myIMChannel != null)
            {
                result.IMChannelName = myIMChannel.IMChannelName;
                if (myIMChannel.IMChannelID.IsNumeric())
                {
                    result.IMChannelID = Int32.Parse(myIMChannel.IMChannelID);
                }
            }

            if (queryHierarchy != null)
            {
                result.IsMerchantHierarchy = true;
                result.ParentMerchantID = queryHierarchy.ParentMerchantID;
            }

            result.SingleWalletCallbackEndpointUrl = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain)?.SettingValue;

            var creditMode = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.System_IsMerchantCreditMode);

            bool isCreditMode = creditMode.SettingValueOfInt == NormalYesOrNoEnum.Yes.ToInteger();

            //如果屬於測試用商戶 (非正式)
            if (isCreditMode && queryMerchant.Type != MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger())
            {
                //前端會提示該商戶為0
                var myCredit = MerchantCreditRepository.GetById(merchantID);
                result.IsDepositCreditAfterProcess = myCredit == null || myCredit.CreditAmount == 0;
            }

            if (isSingleWallet)
            {
                var domain = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);

                if (string.IsNullOrWhiteSpace(domain.SettingValue))
                {
                    result.IsSingleWalletDoaminNotSetup = true;
                }

                var apiMode = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

                if (apiMode.SettingValueOfInt == (int)SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit)
                {
                    var w88_OperatorID = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraID);

                    if (string.IsNullOrWhiteSpace(w88_OperatorID.SettingValue))
                    {
                        result.IsW88OperatorCodeNotSetup = true;
                    }
                }

                if (string.IsNullOrWhiteSpace(swPgsIdentityToken.SettingValue))
                {
                    result.ExtraHintTags.Add("X-PROVIDER-Identity未設置");
                }

                //禁止創建單錢包商戶
                var forbiddenSingleWallet = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.ImportantSetting_ForbiddenCreateSingleWalletMerchant);

                if (forbiddenSingleWallet.SettingValueOfBoolean)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_AgentNotAllowCreateSingleWallet);
                }
            }

            if (isCreditMode)
            {
                result.Memo_CN.Add("信用额度模式");
                result.Memo_EN.Add("Credit Mode");
            }

            var transferConvertRate = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer);

            if (transferConvertRate.SettingValueOfDecimal != 1)
            {
                result.Memo_CN.Add($"轉帳金幣兌換比例:{transferConvertRate.SettingValueOfDecimal}");
            }

            var queryMerchantByCache = MerchantRepository.GetSingleByCache(merchantID);

            //檢查cache清除
            if (queryMerchantByCache.Currency != queryMerchant.Currency ||
               queryMerchantByCache.WalletMode != queryMerchant.WalletMode)
            {
                result.ExtraHintTags.Add("cache未清除(幣別/錢包類型)");
            }

            result.IsCreditMode = isCreditMode;

            //要勾選才會顯示出來的隱藏項目
            result.ExtraInfoItemsDefaultHidden = SetDefaultHiddenExtraInfo(queryMerchant.MerchantID);

            string accountInfo = string.Empty;

            var enableLobbyTransfer = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Common_LobbyEnablePlatformTransfer);

            //大廳轉帳的檢查
            if (enableLobbyTransfer.SettingValueOfBoolean)
            {
                var swEndpoint = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);
                var apiLogicMode = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);
                var lobbyStyle = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Common_LobbyStyle);

                if (string.IsNullOrWhiteSpace(swEndpoint.SettingValue))
                {
                    result.ExtraHintTags.Add("endpoint未配置");
                }

                if (apiLogicMode.SettingValueOfInt == (int)SingleWalletPlatformApiLogicTypeEnum.Default)
                {
                    result.ExtraHintTags.Add("api logic mode配置錯誤");
                }

                if (lobbyStyle.SettingValueOfInt == (int)LobbyStyleEnum.Default)
                {
                    result.ExtraHintTags.Add("大廳版型配置錯誤");
                }
            }

            var queryAccounts = UserRepository.GetByFilter(new QueryUserRequestViewModel
            {
                MerchantIDsMatchInAssignMerchantOrCreditMerchant = merchantID.ToListCollection(),
            });

            if (queryAccounts.IsNotEmptyNull())
            {
                foreach (var item in queryAccounts)
                {
                    result.ExtraInfoItemsDefaultHidden.Add(new ExtraInfoItemModel
                    {
                        CustomizedSettingKey = -1,
                        IconStyle = "person",
                        Text = item.UserName
                    });
                }
            }

            //正式商戶 才需檢查帳密有建立
            if (result.IsMerchantTypeForProduction)
            {
                if (isRootAgent)
                {
                    var accountingGroup = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.ImportantSetting_AccountingGroup);

                    if (accountingGroup.SettingValueOfInt <= 0)
                    {
                        result.ExtraHintTags.Add("帳單組別未配置");
                    }
                }

                if (queryAccounts.IsNotEmptyNull() && queryAccounts.Count() == 1)
                {
                    var matchUser = queryAccounts.FirstOrDefault();
                    var queryUser = UserRepository.GetById(matchUser.UserID);

                    bool isDefaultPassword = [REDACTED];
                    string passwordText = isDefaultPassword ? SystemConstant.Default_EazyPassword : [REDACTED];
                    accountInfo = $"{queryUser.UserName}／{passwordText}";
                }
                else
                {
                    if (queryAccounts.IsEmptyOrNull())
                    {
                        result.ExtraHintTags.Add("後台帳號未建立");
                    }

                    accountInfo = $"后台帐号／后台密码";
                }

                var ipSetting = MerchantCustomizedSettingRepository.GetMerchantSettingKeyWithoutCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting, false);

                if (ipSetting.SettingValueOfInt <= 0)
                {
                    result.ExtraHintTags.Add("IP防火牆未配置");
                }
            }
            else
            {
                if (queryAccounts.IsNotEmptyNull() && queryAccounts.Count() == 1)
                {
                    var matchUser = queryAccounts.FirstOrDefault();
                    var queryUser = UserRepository.GetById(matchUser.UserID);

                    string passwordText = AppHelper.ShwoAdminSysVisiblePassowrd(queryUser.UserName, queryUser.Password, merchantID);

                    if(string.IsNullOrWhiteSpace(passwordText))
                    {
                        passwordText = UnknowPassword;
                    }

                    accountInfo = $"{queryUser.UserName}／{passwordText}";
                }
            }

            result.VendorApiUrl = PgsVendorAPIPath(isMerchantTypeForProduction, "");
            result.GamesMaterialUrl = (result.VendorApiUrl + $"/m/{queryMerchant.MerchantGuid}").ReplaceDuplicateUrlChar();
            result.AdminsSysAccountPassword = [REDACTED];

            string NEW_LINE = Environment.NewLine;

            string message_CN = $"您好，以下为 API对接密钥 & 商户配置信息{NEW_LINE}";

            var isProduction = example.invalid.Type == MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger();
            message_CN += MerchantApiKeyCopyHelper.GenearteMessages_ThisIsForDevOrProduction(LanguageEnum.zh_cn , isProduction);

            if (isProduction)
            {
                message_CN += $"正式环境进行的投注数据会计入'帐务'。测试时请注意" + NEW_LINE;
            }

            message_CN += "--------------------------------------------------" + NEW_LINE;
            message_CN += "商戶：" + queryMerchant.Name + NEW_LINE;

            if (queryMerchant.ChargeCurrency != "BETA")
            {
                message_CN += "货币：" + ConvertCurrencyDisplayText(queryMerchant.ChargeCurrency) + NEW_LINE;
            }

            var isSwMode = AppHelper.IsSingleWalletMode(queryMerchant.WalletMode);
            var isEndpointSetup = result.SingleWalletCallbackEndpointUrl.IsNotEmptyTextNotNull();

            message_CN += "模式：" + (isSwMode ? "无缝钱包" : "转帐钱包") + NEW_LINE;

            if (result.PGSConvertRate != 1)
            {
                message_CN += "游戏币转换比例：" + result.PGSConvertRate.ToString() + NEW_LINE;
            }

            if (result.Memo_CN.IsNotEmptyNull())
            {
                message_CN += "备注：" + string.Join("、", result.Memo_CN) + NEW_LINE;
            }

            string vendorCode = SettingHelper.SeamlessAPIVendorCode;
            message_CN += "MerchantID：" + queryMerchant.MerchantID + NEW_LINE;
            message_CN += "MerchantSecret：" + example.invalid.ApiSecretKey + NEW_LINE;

            if (isSwMode)
            {
                message_CN += "VendorCode：" + vendorCode + "  (callback API url路徑 #5.1.1~*)" + NEW_LINE;
                message_CN += "X-PROVIDER-IDENTITY：" + result.SingleWalletIdentityToken + "(X-target-IDENTITY)" + NEW_LINE;

                if (isEndpointSetup)
                {
                    message_CN += "商戶endpoint：" + result.SingleWalletCallbackEndpointUrl + " (API文件5.1.1~*)" + NEW_LINE;
                }
                else
                {
                    if (result.AdminsSysAccountPassword.IsNotEmptyTextNotNull())
                    {
                        message_CN += "商戶endpoint：" + "**尚未设置**" + " (API文件5.1.1~*)" + NEW_LINE + NEW_LINE;
                        message_CN += "⚠️ 无缝钱包模式需要完成callback url设置才能正常启动游戏 (基於安全政策考量，代理需要自行做設定)。您可以登入【后台】>【代理管理】>【代理管理】>【进阶设置】 查看或修改 callback 链结设置" + NEW_LINE;
                    }
                    else
                    {
                        message_CN += "商戶endpoint：" + "**尚未设置**" + " (API文件5.1.1~*)" + NEW_LINE;
                    }
                }
            }

            message_CN += "--------------------------------------------------" + NEW_LINE;
            message_CN += "对接API: " + PgsVendorAPIPath(isMerchantTypeForProduction) + NEW_LINE;


            bool isPGSBrandingType = SettingHelper.PromoteBrandTypeEnum == PromoteBrandTypeEnum.target;

            if (!isProduction && isPGSBrandingType)
            {
                message_CN += "游戏圖片: " + result.GamesMaterialUrl + " (icon、logo、游戏清单)" + NEW_LINE + NEW_LINE;
            }

            if (!string.IsNullOrWhiteSpace(result.AdminsSysAccountPassword))
            {
                message_CN += "后台网址: " + _preQueryDocumentUrl.DocumentUrl_BOPortalUrl + NEW_LINE;
                message_CN += "后台帐号: " + result.AdminsSysAccountPassword + NEW_LINE + NEW_LINE;
            }

            if (!isProduction)
            {
                message_CN += "--------------------------------------------------" + NEW_LINE;
                var messages = MerchantApiKeyCopyHelper.GenearteMessages_WhereToDownloadDocument(LanguageEnum.zh_cn);
                message_CN += string.Join(NEW_LINE, messages) + NEW_LINE;
            }

            if (!isProduction)
            {
                message_CN += "--------------------------------------------------" + NEW_LINE;
                var messages = MerchantApiKeyCopyHelper.GenearteMessages_BetaCoinHint(LanguageEnum.zh_cn);
                message_CN += string.Join(NEW_LINE, messages) + NEW_LINE;
            }

            if (isSwMode)
            {
                message_CN += "--------------------------------------------------" + NEW_LINE + NEW_LINE;
                message_CN += " " + NEW_LINE;

                var arrayText = new List<string>();

                if (isProduction)
                {
                    arrayText.Add("- 贵司的服务器ip (outbound IP)，我方需设置防火墙白名单" + NEW_LINE);
                }

                if (arrayText.IsNotEmptyNull())
                {
                    message_CN += "贵司需提供的资讯包含以下:" + NEW_LINE;

                    foreach (var myText in arrayText)
                    {
                        message_CN += myText;
                    }

                    message_CN += NEW_LINE;
                }

                message_CN += "并需协助将以下几组IP加入贵司服务器白名单中" + NEW_LINE;
                foreach (var ip in SystemConstant.PgsServerWhiteListIPs)
                {
                    message_CN += $"- {ip}{NEW_LINE}";
                }
            }
            else
            {
                if (isProduction && result.IsCreditMode)
                {
                    message_CN += "--------------------------------------------------" + NEW_LINE;
                    message_CN += "## 信用模式 ⚠️" + NEW_LINE;
                    message_CN += "您的商户为信用模式(credit)。请联系上级代理(或代理窗口专员)。" + NEW_LINE;
                    message_CN += "告知您需要加值多少信用额度(topup)" + NEW_LINE;
                    message_CN += "--------------------------------------------------" + NEW_LINE;
                }

                if (isProduction)
                {
                    message_CN += NEW_LINE + "此外，需提供贵司的服务器ip (outbound IP)，我方需设置防火墙白名单" + NEW_LINE;
                }
            }


            string message_EN = "";

            message_EN = "Hi, the following information is API secret key. " + NEW_LINE;
            message_EN += MerchantApiKeyCopyHelper.GenearteMessages_ThisIsForDevOrProduction(LanguageEnum.en_us, isProduction);

            if (isProduction)
            {
                message_EN += "All bet data will calculate to 'billing', please note." + NEW_LINE;
            }

            message_EN += "--------------------------------------------------" + NEW_LINE;

            message_EN += "Name：" + queryMerchant.Name + NEW_LINE;

            if (queryMerchant.ChargeCurrency != "BETA")
            {
                message_EN += "Currency：" + ConvertCurrencyDisplayText(queryMerchant.ChargeCurrency) + NEW_LINE;
            }

            var walletModeEN = isSwMode ? "Seamless Mode" : "Transfer Mode";

            message_EN += "Mode：" + walletModeEN + NEW_LINE;

            if (result.PGSConvertRate != 1)
            {
                message_EN += "ConvertRate：" + result.PGSConvertRate + NEW_LINE;
            }

            if (result.Memo_EN.IsNotEmptyNull())
            {
                message_EN += "Memo：" + string.Join("、", result.Memo_EN) + NEW_LINE;
            }

            message_EN += "MerchantID：" + queryMerchant.MerchantID + NEW_LINE;
            message_EN += "MerchantSecret：" + queryMerchant.ApiSecretKey + NEW_LINE;

            if (isSwMode)
            {
                message_EN += "X-PROVIDER-IDENTITY：" + result.SingleWalletIdentityToken + "(X-target-IDENTITY)" + NEW_LINE;
                message_EN += "VendorCode：" + vendorCode + "  (callback API url router #5.1.1~*)" + NEW_LINE;

                if (isEndpointSetup)
                {
                    message_EN += "Endpoint：" + result.SingleWalletCallbackEndpointUrl + " (document API 5.1.1~* )" + NEW_LINE;
                }
                else
                {

                    if (result.AdminsSysAccountPassword.IsNotEmptyTextNotNull())
                    {
                        message_EN += "Endpoint：" + "**not set**" + " (document API 5.1.1~* )" + NEW_LINE + NEW_LINE;
                        message_EN += "⚠️ Seamless merchant need to setup callback url. (Operator need setup themself, by our security policy). You can go to 【BO】>【AgentManagement】>【Agent List】> 【advance-setting】 to view/edit the callback url" + NEW_LINE;
                    }
                    else
                    {
                        message_EN += "Endpoint：" + "**not set**" + " (document API 5.1.1~* )" + NEW_LINE;
                    }
                }
            }

            message_EN += "--------------------------------------------------" + NEW_LINE;
            message_EN += "API Path: " + PgsVendorAPIPath(isMerchantTypeForProduction) + NEW_LINE;

            if (!isProduction && isPGSBrandingType)
            {
                message_EN += "Game icon: " + result.GamesMaterialUrl + " (icon & logo)" + NEW_LINE + NEW_LINE;
            }

            if (result.AdminsSysAccountPassword.IsNotEmptyTextNotNull())
            {
                message_EN += "Backoffice: " + _preQueryDocumentUrl.DocumentUrl_BOPortalUrl + NEW_LINE;
                message_EN += "Account: " + result.AdminsSysAccountPassword + NEW_LINE;
            }

            if (!isProduction)
            {
                message_EN += "--------------------------------------------------" + NEW_LINE;
                var messages = MerchantApiKeyCopyHelper.GenearteMessages_WhereToDownloadDocument(LanguageEnum.en_us);
                message_EN += string.Join(NEW_LINE, messages) + NEW_LINE;
            }

            if (!isProduction)
            {
                message_EN += "--------------------------------------------------" + NEW_LINE;
                var messages = MerchantApiKeyCopyHelper.GenearteMessages_BetaCoinHint(LanguageEnum.en_us);
                message_EN += string.Join(NEW_LINE, messages) + NEW_LINE;
            }

            if (isSwMode)
            {
                message_EN += "--------------------------------------------------" + NEW_LINE + NEW_LINE;

                var arrayTextEN = new List<string>();

                if (isProduction)
                {
                    arrayTextEN.Add("- operator’s server outbound IP, we need to add to firewall whitelist." + NEW_LINE);
                }

                if (arrayTextEN.IsNotEmptyNull())
                {
                    message_EN += "Operator need to provide the following information:" + NEW_LINE;

                    foreach (var text in arrayTextEN)
                    {
                        message_EN += text;
                    }

                    message_EN += NEW_LINE;
                }

                message_EN += "and please add following ip address to whitelist in your system’s firewall." + NEW_LINE;

                foreach (var ip in SystemConstant.PgsServerWhiteListIPs)
                {
                    message_EN += $"- {ip}{NEW_LINE}";
                }
            }
            else
            {
                if (isProduction && result.IsCreditMode)
                {
                    message_EN += "--------------------------------------------------" + NEW_LINE;
                    message_EN += "## Credit Mode ⚠️" + NEW_LINE;
                    message_EN += "Your merchant is credit mode ,please contact to your Top-agent or our Customer service." + NEW_LINE;
                    message_EN += "and tell us how much credit you need topup." + NEW_LINE;
                    message_EN += "--------------------------------------------------" + NEW_LINE;
                }

                if (isProduction)
                {
                    message_EN += NEW_LINE + "Please provide operator’s server outbound IP, we need to add to firewall whitelist." + NEW_LINE;
                }
            }

            result.VendorCode = vendorCode;
            result.CredentialMessage_CN = message_CN;
            result.CredentialMessage_EN = message_EN;
            return result;
        }


        private string ConvertCurrencyDisplayText(string currency)
        {
            if(currency == "VND" || currency == "IDR" || 
               currency == "MMK" || currency == "COP")
            {
                return currency + " (1:1)";
            }

            return currency;
        }


        private string PgsVendorAPIPath(bool isProducutionTypeMerchant, string suffixUrl = "/v1")
        {
            return MerchantApiKeyCopyHelper.GeneratePgsVendorAPIPath(_preQueryDocumentUrl, isProducutionTypeMerchant, suffixUrl);
        }


        public MerchantStatusAndHierarchyPreviewInfoModel GetMerchantStatusAndHierarchyPreviewInfo(int userID, int merchantID)
        {
            CheckViewMerchantPermission(userID, merchantID);

            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            var queryHierarchyChilds = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = queryMerchant.MerchantID,
                ExcludeMerchantID = queryMerchant.MerchantID,
                ExcludeDeleteMerchants = true,
            });

            return new MerchantStatusAndHierarchyPreviewInfoModel
            {
                MerchantID = queryMerchant.MerchantID,
                Status = queryMerchant.Status,
                MerchantName = queryMerchant.Name,
                NumberOfChilds = queryHierarchyChilds.Select(x => x.MerchantID).Count(),
            };
        }



        public void Create(int userID, MerchantCreateRequestModel model)
        {
            model.RuleFor(x => x.Name).Rule_NotEmptyText().Rule_MaxLengthOK(50);
            model.RuleFor(x => x.Memo).Rule_MaxLengthOK(50);
            model.RuleFor(x => x.Currency).Rule_NotEmptyText().Rule_MaxLengthOK(10);
            model.RuleFor(x => x.DefaultLanguage).Rule_NotEmptyText().Rule_IsSupportLanguageCode();
            model.RuleFor(x => x.Status).Rule_NotMinus();
            model.RuleFor(x => x.WalletMode).Rule_NotZeroNotMinus();

            //為了麻煩防呆 塞預設空model
            model.ExtraSettings = model.ExtraSettings ?? new MerchantCreateExtraSettingModel();
            model.ExtraSettings.CustomizedSettings = model.ExtraSettings.CustomizedSettings ?? new List<SystemConfigSettingViewModel>();

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(model.WalletMode);

            //如果有指定ParentID
            if (model.ExtraSettings != null && model.ExtraSettings.ParentMerchantID.HasValue)
            {
                int parentMerchantID = model.ExtraSettings.ParentMerchantID.Value;

                //預先檢查一些防呆#1
                MerchantHierarchyService.ValidParentHierarchyBeforeCreateMerchantHierarchy(parentMerchantID);

                //預先檢查一些防呆#2
                MerchantHierarchyService.ValidParentBasicInfoBeforeCreateMerchant(new MerchantParentAgentValidateBeforeCreateParameter
                {
                    Currency = model.Currency,
                    MerchantType = model.Type,
                    ParentMerchantID = parentMerchantID,
                });
            }

            var newEntity = model.ReturnInjecteObject<MerchantEntity>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);
            newEntity.CreateTime = DateTimeHelper.GetCurrentTime();
            newEntity.UpdateTime = DateTimeHelper.GetCurrentTime();
            newEntity.UpdateBy = currentUser.UserName;
            newEntity.CreateBy = currentUser.UserName;
            newEntity.ChargeCurrency = model.Currency;
            newEntity.MerchantGuid = Guid.NewGuid();

            // step 1. 新增基本資料
            var newMerchantID = MerchantRepository.Create(newEntity);

            // step 2. 遊戲商配置 塞預設值
            VendorConfigMerchantMappingRepository.CreateMerchantAllDefaultValue(newMerchantID);

            // step 3. 預設的商戶個性化配置
            var balanceDedaultSetting = model?.ExtraSettings?.CustomizedSettings
                .FirstOrDefault(x => x.SettingKey == (int)MerchantCustomizedSettingKeyEnum.DefaultWalletBalance);

            if (balanceDedaultSetting == null)
            {
                //後面會處理
                model.ExtraSettings.CustomizedSettings.Add(new SystemConfigSettingViewModel
                {
                    SettingKey = MerchantCustomizedSettingKeyEnum.DefaultWalletBalance.ToInteger(),
                    Value = "0"
                });
            }

            string defaultBrandingName = model.Currency == "IDR" ? SystemConstant.BrandingName_ION : SystemConstant.BrandingName_PGS;

            // 品牌名
            CreateCustomizedSetting(newMerchantID, MerchantCustomizedSettingKeyEnum.Common_LobbyBrandingName, defaultBrandingName);
            CreateCustomizedSetting(newMerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_MinTransferAmount, "0.1");

            if (isSingleWallet)
            {
                string guidText = Guid.NewGuid().ToString();
                CreateCustomizedSetting(newMerchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken, guidText);
            }

            // step 4. 商戶信用額度
            MerchantCreditRepository.Create(new MerchantCreditEntity
            {
                MerchantID = newMerchantID,
                CreditAmount = 0
            });

            if (model.ExtraSettings != null)
            {
                // step 5. 根據額外配置塞其他資料
                CreateExtraSettingValue(userID, model, newMerchantID);
            }

        }


        public CRUDResultModel CopyAndBatchCreateNewMerchant(int userID, MerchantBatchCopyAndCreateRequestModel model)
        {
            model.RuleFor(z => z.SourceMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.MerchantNameList).Rule_NotEmptyNull();
            model.RuleFor(z => z.ParentMerchantID).Rule_IsNotMinus();

            model.ExtraSettingFlag = model.ExtraSettingFlag ?? new List<int>();

            if (model.ParentMerchantID == 0)
            {
                model.ParentMerchantID = null;
            }

            foreach (var name in model.MerchantNameList)
            {
                new { Name = name }.RuleFor(x => x.Name).Rule_NotEmptyText();
            }

            bool isCreateAsRootAgent = model.ExtraSettingFlag.Any(z => z == (int)MerchantCopyExtraSettingEnum.SetAsRootAgent);

            if (isCreateAsRootAgent && model.ParentMerchantID.NotNullNotZeroNotMinus())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "若勾選創建為總代。就不能再設定上級代理 (兩者衝突了)");
            }

            var queryItems = GeneralTagMappingRepository.GetByFilter(new QueryGeneralTagMappingRequestViewModel
            {
                TagIDs = GeneralTagEnum.ForbiddenCreateProductionMerchantForThisChannels.ToInteger().ToListCollection()
            });

            model.ExtraSettingFlag = model.ExtraSettingFlag ?? new List<int>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var querySourceMerchant = MerchantRepository.GetByIdOrThrowException(model.SourceMerchantID);
            var querySourceGameSettings = MerchantGameSettingRepository.GetByMerchantID(model.SourceMerchantID);

            querySourceGameSettings = querySourceGameSettings
                 .Where(x => x.Status == (int)NormalActiveStatus.Enable)
                 .ToList();

            //若有標記的頻道 禁止創建正式商戶
            if (queryItems.IsNotEmptyNull() && model.IMChannelID.HasValue && querySourceMerchant.Type == (int)MerchantTypeEnum.Plaform_ProductionEnvironment)
            {
                if (queryItems.Any(z => z.TargetID == model.IMChannelID))
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "您指定的频道已被标记禁止创建《正式商户》。可能原因为: 该频道包含其它游戏商(基于公司政策资安考量，禁止此行为、需另开一个正式营运频道)、或该商户有异常行为被标记。请洽技术人员");
                }
            }

            if (string.IsNullOrWhiteSpace(model.Currency))
            {
                model.Currency = querySourceMerchant.Currency;
            }

            if (model.ParentMerchantID.NotNullNotZeroNotMinus())
            {
                int parentMerchantID = model.ParentMerchantID.Value;

                //預先檢查一些防呆#1
                MerchantHierarchyService.ValidParentHierarchyBeforeCreateMerchantHierarchy(parentMerchantID);

                //預先檢查一些防呆#2
                MerchantHierarchyService.ValidParentBasicInfoBeforeCreateMerchant(new MerchantParentAgentValidateBeforeCreateParameter
                {
                    Currency = model.Currency,
                    MerchantType = querySourceMerchant.Type,
                    ParentMerchantID = parentMerchantID,
                });

                //預先檢查一些防呆#3 (若有設定僅能開通特定遊戲)
                List<int> enableGameIDs = querySourceGameSettings.Select(z => z.GameID).ToList();
                MerchantHierarchyService.ValidParentAndEnableGames(parentMerchantID, enableGameIDs);                
            }

            bool isCopyIPAddressTemplate = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.SourceMerchantID, MerchantCustomizedSettingKeyEnum.System_IsUseSameIPWhiteList_ForChildMerchant).SettingValueOfBoolean;

            bool isCopyAccountingGroup = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.SourceMerchantID, MerchantCustomizedSettingKeyEnum.ImportantSetting_AccountingGroup_AllowCopy).SettingValueOfBoolean;

            bool isForbiddenCreateSingleWallet = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.SourceMerchantID, MerchantCustomizedSettingKeyEnum.ImportantSetting_ForbiddenCreateSingleWalletMerchant).SettingValueOfBoolean;

            bool isForbiddenCreateTransfer = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.SourceMerchantID, MerchantCustomizedSettingKeyEnum.ImportantSetting_ForbiddenCreateTransferMerchant).SettingValueOfBoolean;

            var queryCustomizedSettingsBySource = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = model.SourceMerchantID.ToListCollection()
            });

            var queryConfigMapping = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                MerchantIDs = model.SourceMerchantID.ToListCollection()
            });

            var queryServers = ServerMerchantMappingRepository.GetByFilter(new QueryServerMerchantRelationRequestViewModel
            {
                MerchantIDs = model.SourceMerchantID.ToListCollection()
            });

            List<int> newMerchantIDs = new List<int>();

            var result = new CRUDResultModel
            {
                ResultListIDs = newMerchantIDs
            };

            bool isProductionMerchant = querySourceMerchant.Type == MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger();
            bool isIPTemplateEmpty = false;

            var queryDefaultVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                IsDefault = true
            });

            string myFinalCurrency = string.Empty;

            if (model.ExtraSettingFlag.Any(x => x == (int)MerchantCopyExtraSettingEnum.SetAsSeamlessMode))
            {
                model.WalletMode = MerchantWalletModeEnum.SingleWalletMode.ToInteger();
            }

            //開始寫入
            foreach (string newMerchantName in model.MerchantNameList)
            {
                DateTime now = DateTimeHelper.GetCurrentTime();

                var insertMerchant = querySourceMerchant.ReturnInjecteObject<MerchantEntity>();

                insertMerchant.Name = newMerchantName;
                insertMerchant.MerchantGuid = Guid.NewGuid();
                insertMerchant.ApiSecretKey = Guid.NewGuid().ToString("N").Truncate(15);
                insertMerchant.CreateTime = now;
                insertMerchant.Currency = model.Currency;
                insertMerchant.ChargeCurrency = model.Currency;
                insertMerchant.CreateBy = currentUser.UserName;
                insertMerchant.UpdateBy = currentUser.UserName;
                insertMerchant.Memo = ""; //改成不要複製

                if (model.WalletMode.HasValue)
                {
                    insertMerchant.WalletMode = model.WalletMode.Value;
                }

                bool isSingleWallet_NewCopyMerchant = insertMerchant.WalletMode == MerchantWalletModeEnum.SingleWalletMode.ToInteger();

                if (isForbiddenCreateSingleWallet && isSingleWallet_NewCopyMerchant)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_AgentNotAllowCreateSingleWallet);
                }

                if (isForbiddenCreateTransfer && !isSingleWallet_NewCopyMerchant)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_AgentNotAllowTransferWallet, new ErrorDetailModel("TransferMode"));
                }

                // 自動設定
                if (newMerchantName.Contains("(測試用") || newMerchantName.Contains("（測試用"))
                {
                    insertMerchant.Type = MerchantTypeEnum.Plaform_DevelopEnvironment.ToInteger();
                }

                if (!string.IsNullOrWhiteSpace(model.Memo))
                {
                    insertMerchant.Memo = model.Memo;
                }

                bool isCurrencyNotEqual = insertMerchant.Currency != querySourceMerchant.Currency;
                myFinalCurrency = insertMerchant.Currency;

                int newMerchantID = MerchantRepository.Create(insertMerchant);

                newMerchantIDs.Add(newMerchantID);

                //status = 1 等於啟用。不啟用的不需要複製寫入
                var insertGameSettings = querySourceGameSettings                   
                    .Select(x => new MerchantGameSettingEntity
                    {
                        GameID = x.GameID,
                        Sort = x.Sort,
                        Status = x.Status,
                        MerchantID = newMerchantID,
                        UpdateBy = currentUser.UserName,
                        UpdateTime = now
                    })
                    .ToList();

                model.ExtraSettingFlag = model.ExtraSettingFlag ?? new List<int>();

                //如果畫面端有勾選遊戲 以勾選的為主
                if (model.GameIDs.IsNotEmptyNull())
                {
                    insertGameSettings = insertGameSettings
                        .Where(my => model.GameIDs.Contains(my.GameID))
                        .ToList();
                }

                if (model.ExtraSettingFlag.Any(x => x == (int)MerchantCopyExtraSettingEnum.EnableGalaxyLive))
                {
                    var queryGalaxyGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                    {
                        GameVendorTypes = GameVendorTypeEnum.Galaxy.ToInteger().ToListCollection(),
                        Status = NormalActiveStatus.Enable.ToInteger()
                    });

                    foreach (var item in queryGalaxyGames)
                    {
                        if (insertGameSettings.Any(g => g.GameID == item.GameID))
                            continue;

                        insertGameSettings.Add(new MerchantGameSettingEntity
                        {
                            Status = NormalActiveStatus.Enable.ToInteger(),
                            GameID = item.GameID,
                            MerchantID = newMerchantID,
                            UpdateBy = currentUser.UserName,
                            UpdateTime = now,
                        });
                    }
                }

                bool isDisableCopyGameEnable = model.ExtraSettingFlag.Any(x => x == (int)MerchantCopyExtraSettingEnum.DisableAllGames);

                if (isDisableCopyGameEnable)
                {
                    insertGameSettings = new List<MerchantGameSettingEntity>();
                }

                //部分配置不自動copy 條件1: 正式商戶  (會有點風險)
                //部分配置不自動copy 條件1: 正式商戶  (會有點風險)
                //部分配置不自動copy 條件1: 正式商戶  (會有點風險)
                if (isProductionMerchant)
                {
                    //不sync的配置 定義於此
                    //不sync的配置 定義於此
                    //不sync的配置 定義於此
                    //不sync的配置 定義於此
                    List<int> ignoreSetting = EnumsExtraConfigHelper.MerchantCustomizedSettingKey_NotSyncWhenCopyMerchant_Production();

                    queryCustomizedSettingsBySource = queryCustomizedSettingsBySource
                        .Where(x => !ignoreSetting.Contains(x.CustomizedSettingKey))
                        .ToList();

                    //如果有透過配置 指定禁止複製settingKey (e.g. IP組別、帳單組別)
                    if (!isCopyIPAddressTemplate)
                    {
                        queryCustomizedSettingsBySource = queryCustomizedSettingsBySource
                             .Where(x => x.CustomizedSettingKey != (int)MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting)
                             .ToList();

                        //後面還有一個自動寫入IP組別的邏輯 請參考後面 #Z001

                        //如果最終為空值、且有指定特定頻道名稱
                        //嘗試查詢該是否有IPtemplate對應的頻道ID=那個頻道
                        //將此商戶的IP組別配置為那個IP組別
                    }

                    if (!isCopyAccountingGroup)
                    {
                        queryCustomizedSettingsBySource = queryCustomizedSettingsBySource
                             .Where(x => x.CustomizedSettingKey != (int)MerchantCustomizedSettingKeyEnum.ImportantSetting_AccountingGroup)
                             .ToList();
                    }
                }
                else
                {
                    queryCustomizedSettingsBySource = queryCustomizedSettingsBySource
                        .Where(x => x.CustomizedSettingKey != (int)MerchantCustomizedSettingKeyEnum.ImportantSetting_AccountingGroup)
                        .ToList();
                }


                //部分配置不自動copy 條件2: 貨幣不一致
                //部分配置不自動copy 條件2: 貨幣不一致
                //部分配置不自動copy 條件2: 貨幣不一致
                if (isCurrencyNotEqual)
                {
                    queryCustomizedSettingsBySource = queryCustomizedSettingsBySource
                            .Where(x => x.CustomizedSettingKey != (int)MerchantCustomizedSettingKeyEnum.AGGLive_DefaultVipLevel)
                            .ToList();
                }

                //部分配置不自動copy 條件3: 不分環境。一律ignore (每個商戶都要重新設定)
                //部分配置不自動copy 條件3: 不分環境。一律ignore (每個商戶都要重新設定)
                //部分配置不自動copy 條件3: 不分環境。一律ignore (每個商戶都要重新設定)
                List<int> ignoreSetting_allEnv = EnumsExtraConfigHelper.MerchantCustomizedSettingKey_NotSyncWhenCopyMerchant_AllEnv();

                queryCustomizedSettingsBySource = queryCustomizedSettingsBySource
                       .Where(x => !ignoreSetting_allEnv.Contains(x.CustomizedSettingKey))
                       .ToList();

                //準備insert資料
                var insertCustomziedSettings = queryCustomizedSettingsBySource
                    .Select(x => new MerchantCustomizedSettingEntity
                    {
                        CreateTime = now,
                        SettingValue = x.SettingValue,
                        MerchantID = newMerchantID,
                        CustomizedSettingKey = x.CustomizedSettingKey,
                    })
                    .ToList();

                var ipTemplateSetting = insertCustomziedSettings.FirstOrDefault(x => x.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting);

                //後半段 #Z001 會用到
                if (ipTemplateSetting == null ||
                   ipTemplateSetting?.SettingValue == "0" ||
                   string.IsNullOrWhiteSpace(ipTemplateSetting?.SettingValue))
                {
                    isIPTemplateEmpty = true;
                }

                //有些配置是強制寫入的 (如果source沒有 要強制塞一個)
                AppendEmptySettingIfNotExist(insertCustomziedSettings, MerchantCustomizedSettingKeyEnum.Common_MerchantCopySource);

                if (isSingleWallet_NewCopyMerchant)
                {
                    AppendEmptySettingIfNotExist(insertCustomziedSettings, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken);

                    //單錢包模式 有部分配置不要了(如果是從轉帳模式copy當作樣板)
                    List<int> mustDeleteSettings = new List<int>
                    {
                        MerchantCustomizedSettingKeyEnum.System_IsMerchantCreditMode.ToInteger()
                    };

                    insertCustomziedSettings = insertCustomziedSettings
                        .Where(x => !mustDeleteSettings.Contains(x.CustomizedSettingKey))
                        .ToList();
                }

                //部分設置不要複製、而是要重新產生新的值
                foreach (var item in insertCustomziedSettings)
                {
                    if (item.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken)
                    {
                        item.SettingValue = Guid.NewGuid().ToString();
                    }

                    if (item.CustomizedSettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_MerchantCopySource)
                    {
                        item.SettingValue = $"{querySourceMerchant.MerchantID}";
                    }
                }

                var insertConfigMapping = queryConfigMapping
                    .Select(x =>
                    {
                        var myEntity = new VendorConfigMerchantMappingEntity
                        {
                            MerchantID = newMerchantID,
                            VendorConfigID = x.VendorConfigID,
                            VendorID = x.VendorID
                        };

                        if (isCurrencyNotEqual)
                        {
                            var matchDefaultValue = queryDefaultVendorConfig.FirstOrDefault(item => item.VendorID == x.VendorID);

                            //如果複製的商戶幣別不同 服務器配置要設成預設值
                            if (matchDefaultValue != null)
                            {
                                myEntity.VendorConfigID = matchDefaultValue.VendorConfigID;
                            }
                        }

                        return myEntity;
                    })
                    .ToList();

                var insertServers = queryServers.Select(x => new ServerMerchantMappingEntity
                {
                    ServerID = x.ServerID,
                    MerchantID = newMerchantID
                })
                    .ToList();

                if (isCurrencyNotEqual)
                {
                    insertServers = new List<ServerMerchantMappingEntity>();
                }

                //排除單錢包不支援的遊戲
                if (isSingleWallet_NewCopyMerchant && insertGameSettings.IsNotEmptyNull())
                {
                    var notSupportGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                    {
                        IsSupportSingleWallet = false
                    })
                    .Select(z => z.GameID)
                    .ToList();

                    insertGameSettings = insertGameSettings
                        .Where(x => !notSupportGames.Contains(x.GameID))
                        .ToList();
                }

                MerchantGameSettingRepository.BulkInsert(insertGameSettings);
                MerchantCustomizedSettingRepository.BulkInsert(insertCustomziedSettings);
                VendorConfigMerchantMappingRepository.CreateMany(insertConfigMapping);
                ServerMerchantMappingRepository.BulkInsert(insertServers);

                MerchantCreditRepository.Create(new MerchantCreditEntity
                {
                    MerchantID = newMerchantID,
                    CreditAmount = 0,
                    IsUnlimit = false
                });

                //代理樹狀結構
                if (model.ParentMerchantID.HasValue)
                {
                    //這段可能隨著樹狀增長 越來越花時間 (因為有HierarchyNodeChanges的那段通用邏輯)
                    //這段可能隨著樹狀增長 越來越花時間 (因為有HierarchyNodeChanges的那段通用邏輯)
                    //這段可能隨著樹狀增長 越來越花時間 (因為有HierarchyNodeChanges的那段通用邏輯)
                    var hierarchyResult = MerchantHierarchyService.CreateMerchantHierarchyWithTryCatch(userID, new MerchantHierarchyCreateRequestModel
                    {
                        ParentMerchantID = model.ParentMerchantID.Value,
                        TargetMerchantID = newMerchantID
                    });

                    result.DebugMessage = hierarchyResult.IsSuccess ? "" : hierarchyResult.DebugMessage;
                }
            }

            //如果頻道有標記 不要參加活動 (且為正式商戶)
            if (model.IMChannelID.HasValue && querySourceMerchant.Type == (int)MerchantTypeEnum.Plaform_ProductionEnvironment)
            {
                var queryChannels = GeneralTagMappingRepository.GetByTagID((int)GeneralTagEnum.DoNotJoinPromotionEvent);
                var queryChannelsJP = GeneralTagMappingRepository.GetByTagID((int)GeneralTagEnum.DoNotEnableJackpotGame);

                bool isMatchChannel_event = queryChannels.Any(z => z.TargetID == model.IMChannelID);
                bool isMatchChannel_jp = queryChannels.Any(z => z.TargetID == model.IMChannelID);

                if (isMatchChannel_event)
                {
                    foreach (int eachMerchantID in result.ResultListIDs)
                    {
                        MerchantCustomizedSettingRepository.CreateOrUpdate(eachMerchantID, (int)MerchantCustomizedSettingKeyEnum.ImportantSetting_ForbiddenAllEvent, "1");
                    }

                    string myMsg = $"頻道ID: {model.IMChannelID} 創建新商戶。設定不想要活動Event。請確認是否需要手動配置 {string.Join("," , result.ResultListIDs)}";
                    SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_adminSys_api);
                }

                if (isMatchChannel_jp)
                {
                    string myMsg = $"頻道ID: {model.IMChannelID} 創建新商戶。設定不想要JP遊戲。請確認是否需要手動配置 {string.Join(",", result.ResultListIDs)}";
                    SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_important_error);
                }
            }


            if (result.ResultListIDs.Count() == 1)
            {
                //等同於merchantID
                result.ResultId = result.ResultListIDs.FirstOrDefault();

                int newMerchantID = result.ResultId;

                if (model.IMChannelID.NotNullNotZeroNotMinus())
                {
                    int settingKey = MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger();
                    MerchantCustomizedSettingRepository.CreateOrUpdate(newMerchantID, settingKey, model.IMChannelID.ToString());
                }

                if (model.AgentMemo.IsNotEmptyTextNotNull())
                {
                    int settingKey = MerchantCustomizedSettingKeyEnum.Common_AgentMemo.ToInteger();
                    MerchantCustomizedSettingRepository.CreateOrUpdate(newMerchantID, settingKey, model.AgentMemo);
                }

                // #Z001 自動填寫IP配置
                if (isIPTemplateEmpty && model.IMChannelID.HasValue && isProductionMerchant)
                {
                    var queryMatchIPTemplate = IMChannelCustomizedSettingRepository.GetSingleParseModel(model.IMChannelID.Value, IMChannelCustomizedSettingKeyEnum.ReferIPAddressTemplateID);

                    //嘗試查詢該是否有IPtemplate對應的頻道ID=那個頻道
                    //將此商戶的IP組別配置為那個IP組別

                    if (queryMatchIPTemplate != null && queryMatchIPTemplate.SettingValueOfInt > 0)
                    {
                        string templateIDText = queryMatchIPTemplate.SettingValueOfInt.ToString();
                        MerchantCustomizedSettingRepository.CreateOrUpdate(newMerchantID, (int)MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting, templateIDText);
                        MerchantCustomizedSettingRepository.CreateOrUpdate(newMerchantID, (int)MerchantCustomizedSettingKeyEnum.System_IsUseSameIPWhiteList_ForChildMerchant, "1");
                    }
                }
            }


            return result;
        }


        private void AppendEmptySettingIfNotExist(List<MerchantCustomizedSettingEntity> insertCustomziedSettings, MerchantCustomizedSettingKeyEnum key)
        {
            if (insertCustomziedSettings.Any(z => z.CustomizedSettingKey == key.ToInteger()))
            {
                return;
            }

            var matchFirst = insertCustomziedSettings.FirstOrDefault();

            insertCustomziedSettings.Add(new MerchantCustomizedSettingEntity
            {
                SettingValue = "",
                CustomizedSettingKey = key.ToInteger(),
                MerchantID = matchFirst.MerchantID,
                CreateTime = DateTimeHelper.GetCurrentTime(),
            });
        }







        private void CreateExtraSettingValue(int userID, MerchantCreateRequestModel model, int newMerchantID)
        {
            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(model.WalletMode);

            //進階設置
            foreach (var item in model.ExtraSettings.CustomizedSettings)
            {
                CreateCustomizedSetting(newMerchantID, (MerchantCustomizedSettingKeyEnum)item.SettingKey, item.Value);
            }

            if (model.ExtraSettings.CopyGameSettingFromMerchantID.HasValue)
            {
                int copyFrom = model.ExtraSettings.CopyGameSettingFromMerchantID.Value;

                var filter = new QueryMerchantGameSettingRequestViewModel
                {
                    MerchantIDs = copyFrom.ToListCollection(),
                    Status = NormalActiveStatus.Enable.ToInteger(),
                };

                if (isSingleWallet)
                {
                    filter.IsSupportSingleWalletOnly = true;
                }

                var queryGamesByCopySource = MerchantGameSettingRepository.GetByFilter(filter);
                var queryServersByCopySource = ServerMerchantMappingRepository.GetByFilter(new QueryServerMerchantRelationRequestViewModel
                {
                    MerchantIDs = copyFrom.ToListCollection()
                });

                var insertItems = queryGamesByCopySource.Select(x => new MerchantGameSettingEntity
                {
                    GameID = x.GameID,
                    Status = NormalActiveStatus.Enable.ToInteger(),
                    MerchantID = newMerchantID,
                    UpdateTime = DateTimeHelper.GetCurrentTime(),
                    UpdateBy = "system",
                    Sort = 1
                })
                    .ToList();

                var insertServerItems = queryServersByCopySource.Select(x => new ServerMerchantMappingEntity
                {
                    ServerID = x.ServerID,
                    MerchantID = newMerchantID,
                })
                    .ToList();

                MerchantGameSettingRepository.CreateMany(insertItems);
                ServerMerchantMappingRepository.CreateMany(insertServerItems);
            }

            if (model.ExtraSettings.ParentMerchantID.HasValue)
            {
                MerchantHierarchyService.CreateMerchantHierarchyWithTryCatch(userID, new MerchantHierarchyCreateRequestModel
                {
                    TargetMerchantID = newMerchantID,
                    ParentMerchantID = model.ExtraSettings.ParentMerchantID.Value
                });
            }
        }

        public void Update(int userID, MerchantUpdateRequestModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Name).Rule_NotEmptyText().Rule_MaxLengthOK(50);
            model.RuleFor(x => x.Memo).Rule_MaxLengthOK(50);
            model.RuleFor(x => x.Currency).Rule_NotEmptyText().Rule_MaxLengthOK(10);
            model.RuleFor(x => x.DefaultLanguage).Rule_NotEmptyText().Rule_IsSupportLanguageCode();
            model.RuleFor(x => x.Status).Rule_NotMinus();

            CheckViewMerchantPermission(userID, model.MerchantID);

            var queryExist = MerchantRepository.GetById(model.MerchantID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            queryExist.Name = model.Name;
            queryExist.Memo = model.Memo;
            queryExist.DefaultLanguage = model.DefaultLanguage;
            queryExist.UpdateTime = DateTimeHelper.GetCurrentTime();
            queryExist.UpdateBy = currentUser.UserName;
            queryExist.Type = model.Type;

            if (queryExist.WalletMode != model.WalletMode && model.WalletMode > 0)
            {
                if (SettingKeyEnumLogicHelper.IsSuperAdminEnableEditForFiveMinutes())
                {
                    queryExist.WalletMode = model.WalletMode;
                    SendMessageToMultipleChannel($"調整 {queryExist.MerchantID} WalletMode");
                }

                //補缺失的資料 (如果transfer 轉 singleWallet 可能會出現)
                if(AppHelper.IsSingleWalletMode(model.WalletMode))
                {
                    RecoverSingleWalletMissingData(queryExist.MerchantID);
                }
            }

            if (!queryExist.MerchantGuid.HasValue)
            {
                queryExist.MerchantGuid = Guid.NewGuid();
            }

            // Step 1. 更新資料
            MerchantRepository.Update(queryExist);

            // Step 2. 清除cache
            CacheRemoveHelper.RemoveCache_WhenUpdateMerchant(queryExist.MerchantID);

            // Step 3. 日誌
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.Merchant_UpdateMerchant, queryExist.MerchantID);
            WriteOperateLog(log);
        }


        public void UpdateStatusByAgent(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Status).Rule_NotMinus();

            if (model.Status == NormalActiveStatus.Disable.ToInteger())
            {
                model.RuleFor(x => x.YesOrNo).Rule_NotMinus();
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            CheckViewMerchantPermission(userID, model.MerchantID);

            var queryExist = MerchantRepository.GetById(model.MerchantID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            if (queryExist.Status == (int)NormalActiveStatus.DisableAndForbiddenEdit)
            {
                if (!currentUser.IsSuperAdminOrg)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("Status"));
                }
            }

            bool isFromDisableToEnable = queryExist.Status == (int)NormalActiveStatus.Disable && model.Status == (int)NormalActiveStatus.Enable;

            List<int> updateMerchantIDs = new List<int>
            {
                queryExist.MerchantID
            };

            //如果有指定同步子代理狀態（僅針對disable、enable全部自動開啟可能有風險)
            if (model.YesOrNo == NormalYesOrNoOrNorSelectEnum.Yes.ToInteger() && 
                model.Status == (int)NormalActiveStatus.Disable)
            {
                var queryChilds = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    HierarchyOfTargerMerchant = queryExist.MerchantID,
                    ExcludeMerchantID = queryExist.MerchantID,
                    ExcludeDeleteMerchants = true
                });

                if (queryChilds.IsNotEmptyNull())
                {
                    updateMerchantIDs.AddRange(queryChilds.Select(x => x.MerchantID).ToList());
                }
            }

            List<UserOperateRecordEntity> operatorLogs = new List<UserOperateRecordEntity>();

            bool isUpdateToEnable = model.Status == NormalActiveStatus.Enable.ToInteger();

            foreach (int merchantID in updateMerchantIDs)
            {
                MerchantRepository.Update(merchantID, new
                {
                    Status = model.Status,
                    UpdateTime = DateTimeHelper.GetCurrentTime(),
                    UpdateBy = currentUser.UserName,
                });

                if(isUpdateToEnable)
                {
                    var warningNotify = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.ImportantSetting_WhenEnableMerchantWarningNotify);

                    if(warningNotify.SettingValue.IsNotEmptyTextNotNull())
                    {
                        string myMsg = $"{Environment.NewLine}⚠️商戶:{merchantID} 啟用狀態已被調整, 注意CF是否有特殊設置, {warningNotify.SettingValue}";
                        SendMessageToMultipleChannel(myMsg);
                    }
                }

                //更新Cache
                CacheRemoveHelper.RemoveCache_WhenUpdateMerchant(merchantID);

                //寫log
                var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateMerchantStatus, merchantID, $"afterValue: {model.Status}");
                log.Memo = model.Memo ?? "";

                operatorLogs.Add(log);
            }

            WriteOperateLog(operatorLogs);

            if (isFromDisableToEnable)
            {
                //string myMsg = $"商戶: `{queryExist.Name} ({queryExist.MerchantID})` 從《關閉》改為《啟用》狀態 , 操作人:{currentUser.UserName} ({currentUser.UserID}) ";
                //SendMessageToMultipleChannel(myMsg);
            }
        }


        public void Delete(int userID, int merchantID, string memo = "")
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //先寫死用這個判斷
            if (!currentUser.IsSuperAdminOrg)
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);

            var queryExist = MerchantRepository.GetById(merchantID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            queryExist.Status = NormalActiveStatus.PermanentDelete.ToInteger();

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                MerchantRepository.Update(queryExist);

                MerchantGameSettingRepository.DeleteByModel(new
                {
                    MerchantID = merchantID
                });

                MerchantHierarchyRepository.DeleteByModel(new
                {
                    MerchantID = merchantID
                });

                MerchantHierarchyChildMappingRepository.DeleteByModel(new
                {
                    MerchantID = merchantID
                });

                MerchantHierarchyChildMappingRepository.DeleteByModel(new
                {
                    ChildMerchantID = merchantID
                });

                scope.Complete();
            }

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.DeleteMerchant, merchantID);
            log.Memo = $"【刪商戶:{queryExist.Name}】" + memo;

            WriteOperateLog(log, isChannelNotify: true);

            CacheRemoveHelper.RemoveCache_WhenUpdateMerchant(merchantID);
        }


        public void DoExtraOperateForMerchant(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Type).Rule_NotMinus();

            int merchantID = model.MerchantID;
            var type = (SpecialActionEnum)model.Type;

            var queryMerchant = MerchantRepository.GetById(merchantID);

            //標記API_KEY 已提供
            //發送訊息給客服
            //發送14天後的排程
            switch (type)
            {
                case SpecialActionEnum.Merchant_AddApiKeyProvidedTag:
                    {
                        string targetValue = "1";
                        MerchantCustomizedSettingRepository.CreateOrUpdate(merchantID, (int)MerchantCustomizedSettingKeyEnum.Common_IsCredentialProvideCompleted, targetValue);
                        break;
                    }

                case SpecialActionEnum.Merchant_AddDropdownCategoryToReadyToOnline:
                    {
                        string targetValue = "1050";
                        MerchantCustomizedSettingRepository.CreateOrUpdate(merchantID, (int)MerchantCustomizedSettingKeyEnum.AdminSys_CustomizedDropdownCategory, targetValue);
                        break;
                    }

                case SpecialActionEnum.Merchant_AddJobNotifyMerchantIsOnlineOrNot:
                    {
                        break;
                    }

                case SpecialActionEnum.Merchant_NotifyCustomerServiceNewChannel:
                    {
                        var IMChannel = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Common_IMChannelID);

                        if (IMChannel.SettingValueOfInt <= 0)
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("IMChannel"));

                        var queryIMChannelDetailValue = IMChannelRepository.GetByIMChannelID(IMChannel.SettingValueOfInt);

                        if (queryIMChannelDetailValue == null)
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("IMChannel"));

                        string channelType = MultiLanguageTextHelper.GetEnumsCultureText((IMChannelTypeEnum)queryIMChannelDetailValue.Type);
                        string msg = "";
                        msg += "💰💰【系統訊息】💰💰" + Environment.NewLine;
                        msg += $"新商戶即將上線： {queryMerchant.MerchantID}｜{queryMerchant.Name} ({queryMerchant.Currency})" + Environment.NewLine;
                        msg += $"客戶頻道：[{channelType}]　{queryIMChannelDetailValue.DisplayName}" + Environment.NewLine;
                        msg += $"未來維護通知也需發送此客戶 (*若未加入該頻道 請告知我們) " + Environment.NewLine;
                        msg += $"　@pgscs01 ";

                        msg = msg.Replace("&", "＆"); //TG發送似乎會因為這符號異常

                        var targetChannel = TelegramChannelEnum.internal_channel_PGS_x_CustomerService;

                        if (SettingHelper.IsDev)
                        {
                            targetChannel = TelegramChannelEnum.pgs_common_error;
                        }

                        MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                        {
                            Message = msg,
                            TelegramChannelEnum = targetChannel,
                            TelegramExtraSetting = new TelegramExtraSetting
                            {
                                IsCleanPrefixText = true
                            }
                        });
                        break;
                    }

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }
        }



        public MerchantAdvanceListAllViewModel ListAllMerchantCustomizedSettings(int userID, MerchantAdvanceListRequestModel filter)
        {
            if (filter.MerchantSettingKeys.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("MerchantSettingKeys"));
            }

            var merchantFilter = new QueryMerchantRequestViewModel
            {
                Type = filter.MerchantType,
                Currency = filter.Currency,
                HierarchyOfTargetMerchant = filter.HierarchyOfTargetMerchant
            };

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var ids = AppHelper.AnalyzeIDsFromRawText(filter.Keyword);

                if (ids.IsNotEmptyNull() && ids.Count() >= 2)
                {
                    merchantFilter.MerchantIDs = ids;
                }
                else
                {
                    merchantFilter.Keyword = filter.Keyword;
                }
            }

            var queryMerchants = MerchantRepository.GetByFilter(merchantFilter);

            List<int> merchantIDs = queryMerchants.Select(z => z.MerchantID).ToList();

            var filterOfCustomized = new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = filter.MerchantSettingKeys,
                SettingValueMatch = filter.SettingValueMatch,
                MerchantIDs = merchantIDs
            };



            bool isFilterNoneEmptyByLinq = false;

            bool isFilterQueryResultMerchantsOnly = false;

            bool isShowValueMatchMerchantOnly = false;

            //允許指定查 空白/非空白
            if (filter.SettingValueMatch == "empty" || filter.SettingValueMatch == "EMPTY")
            {
                //想要搜尋所有empty或NULL的商戶 (包含沒匹配到的)
                filterOfCustomized.SettingValueMatch = "";
                filterOfCustomized.IsSettingValueEmptyNullMatch = false;
                isFilterNoneEmptyByLinq = true;
            }
            else if (filter.SettingValueMatch.ContainsWihtoutCase("not empty") ||
                    filter.SettingValueMatch.ContainsWihtoutCase("not_empty"))
            {
                filterOfCustomized.SettingValueMatch = "";
                filterOfCustomized.IsSettingValueEmptyNullMatch = false;
                isShowValueMatchMerchantOnly = true;
            }
            else if (filter.SettingValueMatch.ContainsWihtoutCase("*"))
            {
                filterOfCustomized.SettingValueLikeQuery = filter.SettingValueMatch.Replace("*", "");
                filterOfCustomized.SettingValueMatch = "";
                isFilterQueryResultMerchantsOnly = true;
            }
            else if(filter.SettingValueMatch.CountOrZero() >= 1)
            {
                isShowValueMatchMerchantOnly = true;
            }

            //筆數太大就不篩選了
            if (filterOfCustomized.MerchantIDs.IsSizeTooLarge())
            {
                filterOfCustomized.MerchantIDs = new List<int>();
            }

            var querySettings = MerchantCustomizedSettingRepository.GetByFilter(filterOfCustomized);

            if (isFilterQueryResultMerchantsOnly)
            {
                List<int> merchantIDs_2 = querySettings.Select(z => z.MerchantID).ToList();
                queryMerchants = queryMerchants
                    .Where(x => merchantIDs_2.Contains(x.MerchantID))
                    .ToList();
            }

            if(isFilterNoneEmptyByLinq)
            {
                List<int> merchantIDs_2 = querySettings.Select(z => z.MerchantID).ToList();
                queryMerchants = queryMerchants
                    .Where(x => !merchantIDs_2.Contains(x.MerchantID))
                    .ToList();
            }

            List<int> matchSettingMerchantIDs = querySettings.Select(x => x.MerchantID).Distinct().ToList();

            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();

            foreach (var eachMerchant in queryMerchants)
            {
                var settings = querySettings.Where(z => z.MerchantID == eachMerchant.MerchantID).ToList();

                Dictionary<string, object> keyValues = new Dictionary<string, object>();

                keyValues.Add("merchantID", eachMerchant.MerchantID);
                keyValues.Add("merchantName", eachMerchant.Name);
                keyValues.Add("merchantCurrency", eachMerchant.Currency);
                keyValues.Add("merchantType", eachMerchant.Type);
                keyValues.Add("merchantTypeName", MultiLanguageTextHelper.GetEnumsCultureText((MerchantTypeEnum)eachMerchant.Type));

                bool isMatchAnyValue = false;

                foreach (var settingKey in filter.MerchantSettingKeys)
                {
                    var settingKeyEnum = (MerchantCustomizedSettingKeyEnum)settingKey;
                    var dataType = settingKeyEnum.GetAttributeFromEnum<ReferType>()?.Type;

                    var matchSetting = settings.FirstOrDefault(x => x.CustomizedSettingKey == settingKey);

                    string settingValueRaw = matchSetting?.SettingValue;
                    string settingValueDispayText = matchSetting?.SettingValue;

                    if (matchSetting == null)
                    {
                        settingValueDispayText = settingKeyEnum.GetAttributeFromEnum<SettingDefaultValue>()?.Value;
                    }

                    var parseSettinValue = new SystemConfigSettingParseModel(settingKey, settingValueRaw);

                    if (dataType != null)
                    {
                        if (dataType == typeof(NormalYesOrNoEnum))
                        {
                            settingValueDispayText = MultiLanguageTextHelper.GetEnumsCultureText((NormalYesOrNoEnum)parseSettinValue.SettingValueOfInt);
                        }
                        else if (dataType == typeof(NormalActiveStatus))
                        {
                            settingValueDispayText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)parseSettinValue.SettingValueOfInt);
                        }
                    }


                    //這邊會有這樣的寫法是因為。可能因DB根本沒塞資料。因此會無法查詢到'默認值'的商戶 因此用這樣的方法處理
                    if (isShowValueMatchMerchantOnly)
                    {
                        if (filterOfCustomized.IsSettingValueEmptyNullMatch == true)
                        {
                            if (string.IsNullOrWhiteSpace(settingValueRaw))
                            {
                                isMatchAnyValue = true;
                            }
                        }
                        else if (filterOfCustomized.IsSettingValueEmptyNullMatch == false)
                        {
                            if (!string.IsNullOrWhiteSpace(settingValueRaw))
                            {
                                isMatchAnyValue = true;
                            }
                        }
                        else if (settingValueRaw == filter.SettingValueMatch)
                        {
                            isMatchAnyValue = true;
                        }
                    }

                    keyValues.Add(settingKey.ToString(), settingValueDispayText);
                }

                if (isShowValueMatchMerchantOnly)
                {
                    if (isMatchAnyValue)
                    {
                        items.Add(keyValues);
                    }
                }
                else
                {
                    items.Add(keyValues);
                }
            }

            var headers = filter.MerchantSettingKeys
                .Select(z => new MerchantAdvanceExtraHeaderViewModel
                {
                    HeaderPropertyName = z.ToString(),
                    HeaderDisplayName = MultiLanguageTextHelper.GetEnumsCultureText((MerchantCustomizedSettingKeyEnum)z),
                })
                .ToList();

            return new MerchantAdvanceListAllViewModel
            {
                ExtraHeaders = headers,
                Items = items
            };
        }



        public MerchantCompareResultViewModel CompareTwoMerchants(int userID , MerchantCompareRequestModel model)
        {
            model.RuleFor(z => z.LeftMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.RightMerchantID).Rule_NotZeroNotMinus();

            //允許一樣 (有查看單一商戶的介面)
            //model.RuleFor(z => z.RightMerchantID).Rule_NotEqual(model.LeftMerchantID);

            PreQueryUrlSettingForGlobalVariable();

            var queryLeftMerchant = GetMerchantFullInformation(model.LeftMerchantID);
            var queryRightMerchants = GetMerchantFullInformation(model.RightMerchantID);

            var viewModels = new MerchantCompareResultViewModel();
            var compareColumns = new List<MerchantCompareItemViewModel>();
            var advanceSettings = new List<MerchantCompareItemViewModel>();

            viewModels.LeftMerchant = InstanceCompareBasicInfo(model.LeftMerchantID);
            viewModels.RightMerchant = InstanceCompareBasicInfo(model.RightMerchantID);

            compareColumns.Add(InstanceCompareItemModel("狀態", queryLeftMerchant.StatusText, queryRightMerchants.StatusText));
            compareColumns.Add(InstanceCompareItemModel("錢包模式", queryLeftMerchant.WalletModeText, queryRightMerchants.WalletModeText));
            compareColumns.Add(InstanceCompareItemModel("商戶分類", queryLeftMerchant.MerchantTypeText, queryRightMerchants.MerchantTypeText));
            compareColumns.Add(InstanceCompareItemModel("幣別", queryLeftMerchant.Currency, queryRightMerchants.Currency));
            compareColumns.Add(InstanceCompareItemModel("幣別(對帳)", queryLeftMerchant.ChargeCurrency, queryRightMerchants.ChargeCurrency));
            compareColumns.Add(InstanceCompareItemModel("開啟活動數", queryLeftMerchant.TotalEnableEvents, queryRightMerchants.TotalEnableEvents));
            compareColumns.Add(InstanceCompareItemModel("開啟遊戲數", queryLeftMerchant.TotalEnableGames, queryRightMerchants.TotalEnableGames));
            compareColumns.Add(InstanceCompareItemModel("開啟服務器數", queryLeftMerchant.TotalEnableServers, queryRightMerchants.TotalEnableServers));


            if(HasTargetPermission(userID , "System_AccountingInvoice_Basic"))                
            {
                var feeRate_l = AccountingGroupCustomizedSettingRepository.GetMerchantAllGameVendorRate(model.LeftMerchantID);
                var feeRate_r = AccountingGroupCustomizedSettingRepository.GetMerchantAllGameVendorRate(model.RightMerchantID);

                if (feeRate_l != null && feeRate_l.GameVendorFeeRate.IsNotEmptyNull())
                {
                    compareColumns.Add(InstanceCompareItemModel("帳單組別", feeRate_l.AccountingGroupID, feeRate_r.AccountingGroupID));

                    foreach (var item in feeRate_l.GameVendorFeeRate)
                    {
                        decimal matchValue_r = feeRate_r.GameVendorFeeRate.GetDictionaryOrDefaultValue(item.Key, 0);
                        compareColumns.Add(InstanceCompareItemModel($"遊戲商盤費_{item.Key}", item.Value, matchValue_r));
                    }
                }
            }

            string extraPrefix1 = "(Dev環境)";
            string extraPrefix2 = $"(正式環境) 幣別:{queryLeftMerchant.Currency}";

            var link_dev = InstanceCompareItemModel("測試連結(Dev)", ConcateApiLinkByTemplate(_preQueryDocumentUrl.ExternalAPI_VendorLaunchTest_Dev, queryLeftMerchant.MerchantGuid, extraPrefix1), ConcateApiLinkByTemplate(_preQueryDocumentUrl.ExternalAPI_VendorLaunchTest_Dev, queryRightMerchants.MerchantGuid, extraPrefix1), true, true);
            var link_pro = InstanceCompareItemModel("測試連結(Pro)", ConcateApiLinkByTemplate(_preQueryDocumentUrl.ExternalAPI_VendorLaunchTest_Pro, queryLeftMerchant.MerchantGuid, extraPrefix2), ConcateApiLinkByTemplate(_preQueryDocumentUrl.ExternalAPI_VendorLaunchTest_Pro, queryRightMerchants.MerchantGuid, extraPrefix2), true, true);

            compareColumns.Add(link_dev);
            compareColumns.Add(link_pro);

            var allMerchantSettings = ExtensionOfEnum.GetEnumAllValue<MerchantCustomizedSettingKeyEnum>();

            foreach (MerchantCustomizedSettingKeyEnum eachEnum in allMerchantSettings)
            {
                string displayName = MultiLanguageTextHelper.GetEnumsCultureText(eachEnum);
                string defaultValue = eachEnum.GetAttributeFromEnum<SettingDefaultValue>()?.Value;
                var dataType = eachEnum.GetAttributeFromEnum<ReferType>()?.Type;

                string matchLeftValue = queryLeftMerchant.AdvanceSettings.FirstOrDefault(z => z.CustomizedSettingKey == eachEnum.ToInteger())?.SettingValue;
                string matchRightValue = queryRightMerchants.AdvanceSettings.FirstOrDefault(z => z.CustomizedSettingKey == eachEnum.ToInteger())?.SettingValue;

                if (string.IsNullOrWhiteSpace(matchLeftValue))
                    matchLeftValue = defaultValue;

                if (string.IsNullOrWhiteSpace(matchRightValue))
                    matchRightValue = defaultValue;

                if (dataType != null && dataType == typeof(NormalYesOrNoEnum))
                {
                    matchLeftValue = matchLeftValue == "1" ? "是" : "否";
                    matchRightValue = matchRightValue == "1" ? "是" : "否";
                }

                advanceSettings.Add(InstanceCompareItemModel(displayName, matchLeftValue, matchRightValue));
            }

            var gameVendors = AppHelper.GetReportAvailableGameVendors()
                .Where(z => z != GameVendorTypeEnum.target)
                .Where(z => z != GameVendorTypeEnum.Slot88)
                .ToList();

            foreach (GameVendorTypeEnum eachEnum in gameVendors)
            {
                string vendorName = MultiLanguageTextHelper.GetEnumsCultureText(eachEnum);

                var matchLeftValue = queryLeftMerchant.GameVendorConfigs.FirstOrDefault(z => z.VendorID == eachEnum.ToInteger());
                var matchRightValue = queryRightMerchants.GameVendorConfigs.FirstOrDefault(z => z.VendorID == eachEnum.ToInteger());

                if (matchLeftValue == null && matchRightValue == null)
                    continue;

                compareColumns.Add(InstanceCompareItemModel($"遊戲商配置 ({vendorName})", matchLeftValue?.Name, matchRightValue?.Name));
            }

            var queryAllGames = GameListRepository.GetAll();

            var gameVendorsWithPGS = AppHelper.GetReportAvailableGameVendors()
                .Where(z => z != GameVendorTypeEnum.Slot88)
                .ToList();

            foreach (GameVendorTypeEnum eachEnum in gameVendorsWithPGS)
            {
                string vendorName = MultiLanguageTextHelper.GetEnumsCultureText(eachEnum);

                List<int> gameIDsOfVendor = queryAllGames
                    .Where(z => AppHelper.ConvertSlot88ToPgsVendorID(z.GameVendorType) == eachEnum.ToInteger())
                    .Select(z => z.GameID)
                    .ToList();

                var matchLeftValue = queryLeftMerchant.EnableGames.Where(z => gameIDsOfVendor.Contains(z.GameID)).Count();
                var matchRightValue = queryRightMerchants.EnableGames.Where(z => gameIDsOfVendor.Contains(z.GameID)).Count();

                compareColumns.Add(InstanceCompareItemModel($"{vendorName}遊戲數", matchLeftValue, matchRightValue));
            }

            viewModels.CompareColumns = compareColumns;
            viewModels.AdvanceSettings = advanceSettings;
            return viewModels;
        }


        public MerchantNameExistCheckResult IsMerchantNameExist(string merchantName, bool isIncludeDeleteData = false)
        {
            var query = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                NameMatch = merchantName,
                IsIncludeDeleteData = isIncludeDeleteData
            })
                .FirstOrDefault();

            if (query == null)
            {
                return new MerchantNameExistCheckResult
                {
                    IsExist = false,
                };
            }

            return new MerchantNameExistCheckResult
            {
                IsExist = true,
                MerchantID = query.MerchantID
            };
        }


        private MerchantCompareBasicInfoViewModel InstanceCompareBasicInfo(int merchantID)
        {
            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            return new MerchantCompareBasicInfoViewModel
            {
                MerchantID = queryMerchant.MerchantID,
                Name = queryMerchant.Name
            };
        }


        private MerchantCompareItemViewModel InstanceCompareItemModel(
            string displayName,
            object leftValue,
            object rightValue,
            bool isSkipCompare = false,
            bool isAllowCopy = false
            )
        {
            string convertStringLeft = (leftValue ?? "").ToString();
            string convertStringRight = (rightValue ?? "").ToString();

            return new MerchantCompareItemViewModel
            {
                DisplayName = displayName,
                LeftValue = leftValue,
                RightValue = rightValue,
                IsSkipCompare = isSkipCompare,
                IsAllowCopy = isAllowCopy,
                IsEqual = convertStringLeft == convertStringRight,
            };
        }

        private void RecoverSingleWalletMissingData(int merchantID)
        {
            var settingKey = MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken;

            MerchantCustomizedSettingModel swPgsIdentityToken = [REDACTED], settingKey);

            //補救資料 (通常是透過DB更改錢包模式會發生)
            if (string.IsNullOrWhiteSpace(swPgsIdentityToken.SettingValue))
            {
                MerchantCustomizedSettingRepository.CreateOrUpdate(merchantID, (int)settingKey, Guid.NewGuid().ToString());

                CacheRemoveHelper.RemoveCache_WhenUpdateMerchant(merchantID);

                swPgsIdentityToken = [REDACTED], settingKey);
            }
        }

        private string ConcateApiLinkByTemplate(string templateText, Guid? merchantGuid, string extraPrefix = "")
        {
            if (!merchantGuid.HasValue)
                return string.Empty;

            return $"{extraPrefix} 以下为测试验证用网址。每个连结同时间只支援一人使用(若有他人点击 会导致前一人被登出)。" + Environment.NewLine +
                string.Format(templateText, merchantGuid.Value, "test_001") + Environment.NewLine +
                string.Format(templateText, merchantGuid.Value, "test_002") + Environment.NewLine +
                string.Format(templateText, merchantGuid.Value, "test_003") + Environment.NewLine;
        }


        private MerchantFullInformationVIewModel GetMerchantFullInformation(int merchantID)
        {
            var queryMerchant = MerchantRepository.GetById(merchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            var infoModel = queryMerchant.ReturnInjecteObject<MerchantFullInformationVIewModel>();
            infoModel.MerchantType = queryMerchant.Type;

            var querySettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            });

            var queryEnableGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger()
            });

            var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            });

            int queryEnableServersCount = ServerMerchantMappingRepository.GetCountByFilter(new QueryServerMerchantRelationRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
            });

            var queryEventCount = EventRepository.GetCountByFilter(new QueryEventRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                StatusNotEqual = NormalActiveStatus.PermanentDelete.ToInteger()
            });

            infoModel.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)infoModel.Status);
            infoModel.WalletModeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantWalletModeEnum)infoModel.WalletMode);
            infoModel.MerchantTypeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantTypeEnum)infoModel.MerchantType);
            infoModel.TotalEnableEvents = queryEventCount;
            infoModel.TotalEnableServers = queryEnableServersCount;
            infoModel.TotalEnableGames = queryEnableGames.Count();


            infoModel.EnableGames = queryEnableGames;
            infoModel.GameVendorConfigs = queryVendorConfig;
            infoModel.AdvanceSettings = querySettings;

            return infoModel;
        }

        private List<ExtraInfoItemModel> SetDefaultHiddenExtraInfo(int merchantID)
        {
            var items = new List<ExtraInfoItemModel>();

            var settingKeyDefaultBalance = MerchantCustomizedSettingKeyEnum.DefaultWalletBalance;
            var settingKeyAllowGenTestAccount = MerchantCustomizedSettingKeyEnum.IsAllowCreateTestAccount;

            var defaultBalance = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, settingKeyDefaultBalance);
            var allowCreateTestAccount = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, settingKeyAllowGenTestAccount);

            if (defaultBalance.SettingValueOfDecimal > 0)
            {
                items.Add(new ExtraInfoItemModel
                {
                    IconStyle = "money",
                    Text = $"預設錢包金額:{((int)defaultBalance.SettingValueOfDecimal)}",
                    CustomizedSettingKey = settingKeyDefaultBalance.ToInteger(),
                });
            }
            else
            {
                items.Add(new ExtraInfoItemModel
                {
                    IconStyle = "close",
                    Text = $"無預設錢包金額",
                    CustomizedSettingKey = settingKeyDefaultBalance.ToInteger(),
                });
            }

            if (allowCreateTestAccount.SettingValueOfBoolean)
            {
                items.Add(new ExtraInfoItemModel
                {
                    IconStyle = "ok",
                    Text = $"允許建測試帳號",
                    CustomizedSettingKey = settingKeyAllowGenTestAccount.ToInteger(),
                });
            }
            else
            {
                items.Add(new ExtraInfoItemModel
                {
                    IconStyle = "close",
                    Text = $"禁止建測試帳號",
                    CustomizedSettingKey = settingKeyAllowGenTestAccount.ToInteger(),
                });
            }

            return items;
        }


        private string CheckNotSupportSingleWalletGame(int merchantID)
        {
            var queryNotSupportGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsSupportSingleWallet = false,
                AssignMerchantIDs = merchantID.ToListCollection(),
            });

            if (queryNotSupportGames.IsEmptyOrNull())
            {
                return string.Empty;
            }

            return $"遊戲不支援單錢包: {string.Join("、", queryNotSupportGames.Select(z => z.GameID).ToList())}";
        }

        private void CreateCustomizedSetting(int merchantID, MerchantCustomizedSettingKeyEnum setting, string value)
        {
            MerchantCustomizedSettingRepository.Create(new MerchantCustomizedSettingEntity
            {
                CustomizedSettingKey = setting.ToInteger(),
                MerchantID = merchantID,
                SettingValue = value,
                CreateTime = DateTimeHelper.GetCurrentTime(),
            });
        }


        private MerchantViewModel ConvertToViewModels(MerchantEntity queryMerchant)
        {
            var convert = queryMerchant.ReturnInjecteObject<MerchantViewModel>();

            convert.WalletModeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantWalletModeEnum)queryMerchant.WalletMode);
            convert.MerchantTypeName = MultiLanguageTextHelper.GetEnumsCultureText((MerchantTypeEnum)queryMerchant.Type);

            convert.CurrencyText = MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>(queryMerchant.Currency);
            convert.ChargeCurrencyText = MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>(queryMerchant.ChargeCurrency);
            return convert;
        }


        private void PreQueryUrlSettingForGlobalVariable()
        {
            var externalUrl_dev = SystemConstant.DocumentUrl_ExternalAPI_VendorLaunchText_Dev;
            var externalUrl_pro = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.DocumentUrl_ExternalAPI_VendorLaunchTest);
            var vendorUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.DocumentUrl_VendorAPIUrl);
            var vendorUrlUAT = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.DocumentUrl_VendorAPIUrl_UAT);
            var BOPortalUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.DocumentUrl_BOPortalUrl);

            _preQueryDocumentUrl = new PreQueryDocumentUrlGlobalVariableModel
            {
                ExternalAPI_VendorLaunchTest_Dev = externalUrl_dev + "/vendor-test/launch/{0}/{1}",
                ExternalAPI_VendorLaunchTest_Pro = externalUrl_pro.SettingValue + "/vendor-test/launch/{0}/{1}",
                DocumentUrl_BOPortalUrl = BOPortalUrl.SettingValue,
                DocumentUrl_VendorAPIUrl = vendorUrl.SettingValue,
                DocumentUrl_VendorAPIUrl_UAT = vendorUrlUAT.SettingValue,
            };
        }


        private void SetExtraProperty(List<MerchantViewModel> viewModels, QueryMerchantApiRequestParameterModel filter)
        {
            if (viewModels.IsEmptyOrNull())
                return;

            PreQueryUrlSettingForGlobalVariable();

            List<int> merchantIDs = viewModels.Select(x => x.MerchantID).Distinct().ToList();

            var queryCustomizedSettingsDic = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantIDs.ConvertLargeListToEmpty(),
                CustomizedSettingKeyIDs = new List<int>
                            {
                                MerchantCustomizedSettingKeyEnum.DefaultWalletBalance.ToInteger(),
                            }
            })
            .GroupBy(x => x.MerchantID)
            .ToDictionary(x => x.Key, x => x.ToList());

            var queryIMChannelDic = IMChannelRepository.GetMerchantIMChannel(merchantIDs.ConvertLargeListToEmpty(), isReturnEmptyWhenPassEmptyIDs: false)
                .ToDictionary(x => x.MerchantID, x => x);


            foreach (var item in viewModels)
            {
                item.ExternalAPI_TestAccountLink_Dev = string.Format(_preQueryDocumentUrl.ExternalAPI_VendorLaunchTest_Dev, item.MerchantGuid, "test_001");
                item.ExternalAPI_TestAccountLink_Pro = string.Format(_preQueryDocumentUrl.ExternalAPI_VendorLaunchTest_Pro, item.MerchantGuid, "test_001");

                List<MerchantCustomizedSettingModel> merchantSettings = new List<MerchantCustomizedSettingModel>();

                item.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)item.Status);

                if (queryCustomizedSettingsDic.ContainsKey(item.MerchantID))
                {
                    merchantSettings = queryCustomizedSettingsDic[item.MerchantID]
                       .Select(z => new MerchantCustomizedSettingModel(z.CustomizedSettingKey, z.SettingValue))
                       .ToList();
                }

                if (merchantSettings.IsNotEmptyNull())
                {
                    var walletSetting = merchantSettings.GetBySettingKey(MerchantCustomizedSettingKeyEnum.DefaultWalletBalance);

                    if (walletSetting != null)
                    {
                        item.DefaultBalance = walletSetting.SettingValueOfDecimal;
                    }
                }

                if (queryIMChannelDic.ContainsKey(item.MerchantID))
                {
                    var matchChannel = queryIMChannelDic[item.MerchantID];

                    if (matchChannel.IMChannelID.IsNumeric())
                    {
                        int channelID = Int32.Parse(matchChannel.IMChannelID);

                        item.IMChannelName = matchChannel.IMChannelName;
                        item.IMSoftwareType = channelID <= 19999 ? "TG" : "Skype";
                    }
                    else
                    {
                        item.IMChannelName = matchChannel.IMChannelName;
                    }
                }

                item.KeywordForJsSearch = $"{item.MerchantGuid} {item.IMSoftwareType} {item.Currency} {item.ParentMerchantName} ";
            }

            //如果有指定要查詢特定settingKey
            bool isQueryExtraMemo777 = filter.SearchExtraSettingKey == 777777;

            if (filter.SearchExtraSettingKey > 0)
            {
                if (isQueryExtraMemo777)
                {
                    var queryAccountingAdjust = AccountingAdjustmentRuleRepository.GetByFilter(new QueryAccountingAdjustmentRuleRequestViewModel
                    {
                        FilterBeginTime_Begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 90),
                        FilterBeginTime_End = DateTimeHelper.GetCurrentTime().AddDays(30),
                        IsAssignAnyMerchantIDs = true,
                    });

                    foreach (var eachAdjust in queryAccountingAdjust)
                    {
                        var adjustMerchantIDs = eachAdjust.MerchantIDs.SeperateToListInt();

                        foreach (var item in viewModels)
                        {
                            int merchantID = item.MerchantID;

                            if (adjustMerchantIDs.Contains(item.MerchantID))
                            {
                                item.Memo2 += $"{eachAdjust.NameCN}{Environment.NewLine}";
                            }
                        }
                    }
                }
                else
                {
                    var querySettingsDic = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                    {
                        CustomizedSettingKeyIDs = filter.SearchExtraSettingKey.ToListCollection(),
                        MerchantIDs = merchantIDs.ConvertLargeListToEmpty()
                    })
                    .ToDictionary(z => z.MerchantID, x => x.SettingValue);

                    foreach (var item in viewModels)
                    {
                        item.SearchExtraSettingValue = querySettingsDic.GetDictionaryOrDefaultValue(item.MerchantID, "");
                    }
                }
            }

            return;
        }

        private void SetSearchFilterBySpecialFilterTag(QueryMerchantApiRequestParameterModel filter)
        {
            if (filter.SpecialMerchantFilterType.HasValue)
            {
                if (filter.SpecialMerchantFilterType >= 10000 && filter.SpecialMerchantFilterType <= 29999)
                {
                    filter.FilterGeneralTagID = filter.SpecialMerchantFilterType.Value;
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.ExcludeLayer3Agent)
                {
                    filter.IsExcludeLayer3Agents = true;
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.SearchLayer2AgentOnly)
                {
                    filter.IsSearchLayer2AgentOnly = true;
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.Recent3Month)
                {
                    filter.CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMonths(-1 * 3);
                    filter.CreateTimeEnd = DateTimeHelper.GetCurrentTime();
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.Recent_1Month)
                {
                    filter.CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddMonths(-1);
                    filter.CreateTimeEnd = DateTimeHelper.GetCurrentTime();
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.Recent_14Days)
                {
                    filter.CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 14);
                    filter.CreateTimeEnd = DateTimeHelper.GetCurrentTime();
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.RootAgentOnly)
                {
                    filter.IsRootAgentOnly = true;
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.NotAgentOnly)
                {
                    filter.IsNotAgent = true;
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.IDR_And_KIDR_Production)
                {
                    filter.CurrencyList = new List<string> { "IDR", "KIDR" };
                    filter.Type = MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger();
                }
                else if (filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.USD_Region_Production)
                {
                    filter.CurrencyList = new List<string> { "USD", "CAD", "GBP", "EUR" };
                    filter.Type = MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger();
                }
                else if (
                    filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.RichmondMerchantOnly ||
                    filter.SpecialMerchantFilterType.Value == (int)SpecialMerchantFilterType.IONSlotMerchantOnly)
                {
                    var targetValueCompare = SystemTagEnum.MerchantTag_Branding_Richmond;
                 
                    if (filter.SpecialMerchantFilterType == (int)SpecialMerchantFilterType.IONSlotMerchantOnly)
                    {
                        targetValueCompare = SystemTagEnum.MerchantTag_Branding_ION;
                    }

                    var myFilter = new QueryMerchantRequestViewModel
                    {
                        IsSearchLayer1AgentOnly = true,
                        MerchantCustomizedSetting_SettingKey = MerchantCustomizedSettingKeyEnum.Common_MerchantTag_BrandingCategory.ToInteger(),
                        MerchantCustomizedSetting_SettingValue = targetValueCompare.ToIntegerString(),
                    };

                    var queryMerchantBrief = MerchantRepository.GetByFilter(myFilter , " MerchantID ");

                    filter.FilterMerchantIDsByMultipleHierarchyMerchant = queryMerchantBrief.Select(x => x.MerchantID).ToList();
                }
            }
        }


        private void SetAgentHierarchyInfo(List<MerchantViewModel> viewModels)
        {
            List<int> merchantIDs = viewModels.Select(x => x.MerchantID).ToList();

            var queryHierarchyDic = MerchantHierarchyRepository.GetJoinInfoByMerchantIDs(null)
                .ToDictionary(z => z.MerchantID, z => z);

            foreach (var item in viewModels)
            {
                if (queryHierarchyDic.ContainsKey(item.MerchantID))
                {
                    if (queryHierarchyDic[item.MerchantID].ParentMerchantID == 0)
                    {
                        item.ParentMerchantID = queryHierarchyDic[item.MerchantID].ParentMerchantID;
                        item.ParentMerchantName = "《*Root總代》";
                    }
                    else
                    {
                        item.ParentMerchantID = queryHierarchyDic[item.MerchantID].ParentMerchantID;
                        item.ParentMerchantName = queryHierarchyDic[item.MerchantID].ParentMerchantName;
                    }

                    item.HierarchyLayer = AppHelper.GetAgentLayerLevelByHierarchy(queryHierarchyDic[item.MerchantID].Hierarchy);
                }
            }

            return;
        }



    }
}
