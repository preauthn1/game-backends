using target.DA.Repository;
using target.Utility;
using target.Models.Table;
using System;
using System.Linq;
using target.BL.Interface;
using target.Utility.Helper;
using System.Collections.Generic;
using target.Utility.Extensions;
using example.invalid;
using target.Utility.Utility.ThirdParty;
using System.Threading;
using target.Models.Models;
using target.Models.ViewModels;
using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Models;
using target.Models.CultureText;
using target.Models.ViewModels.GameVendor;
using example.invalid;
using target.Utility.IOC;

namespace target.BL.Services
{
    /// <summary>
    /// 如果有參考多個service 為了避免循環參考 可以寫在這邊
    /// </summary>
    public class AdminSysComplexService : BaseAdminSysService, IAdminSysComplexService
    {

        public AGGLiveDetailJsonRepository AGGLiveDetailJsonRepository { get; set; }

        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public OneToManyCustomizedSettingRepository OneToManyCustomizedSettingRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public UserMerchantMappingRepository UserMerchantMappingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }


        public AGGLiveIntegrationService AGGLiveIntegrationService = new AGGLiveIntegrationService();


        public IMerchantService MerchantService { get; set; }

        public IUsersService UsersService { get; set; }

        public IMerchantHierarchyService MerchantHierarchyService { get; set; }

        public IIPAddressListTemplateService IIPAddressListTemplateService { get; set; }

        public IGameLaunchService GameLaunchService { get; set; }

        public ITransactionRecordService TransactionRecordService { get; set; }

        public IPerGameDetailService PerGameDetailService { get; set; }


        public CheckPlayerMoneyResultViewModel CheckPlayerMoney(int userID, CheckPlayerMoneyRequestModel model)
        {
            var array1 = PerGameDetailService.GetByPagingFilter(userID, new QueryPerGameDetailLogApiRequestParameterModel
            {
                PlayerID = model.PlayerID,
                GameStartTimeBegin = model.BeginTime,
                GameStartTimeEnd = model.EndTime,
                PageIndex = 1,
                PageSize = 2000
            })
            .Items
            .Where(x => x.GameID != 0)
            .ToList();

            var array2 = TransactionRecordService.GetPagingByFilter(userID, new QueryTransactionRecordRequestParameterModel
            {
                PlayerID = model.PlayerID,
                CreateTimeBegin = model.BeginTime,
                CreateTimeEnd = model.EndTime,
                PageIndex = 1,
                PageSize = 2000
            })
            .Items
            .Where(x => x.GameID != 0)
            .ToList();

            var perGameAmountByGameID = array1
                .GroupBy(x => x.GameID)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.UpdateAmount));

            var transactionAmountByGameID = array2
                .GroupBy(x => x.GameID)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.UpdateAmount));

            var gameResults = perGameAmountByGameID.Keys
                .Union(transactionAmountByGameID.Keys)
                .OrderBy(x => x)
                .Select(gameID =>
                {
                    decimal perGameAmount = perGameAmountByGameID.ContainsKey(gameID) ? perGameAmountByGameID[gameID] : 0;
                    decimal transactionAmount = transactionAmountByGameID.ContainsKey(gameID) ? transactionAmountByGameID[gameID] : 0;

                    return new CheckPlayerMoneyGameResultViewModel
                    {
                        GameID = gameID,
                        PerGameAmount = perGameAmount,
                        TransactionAmount = transactionAmount,
                        IsAmountMatched = perGameAmount == transactionAmount
                    };
                })
                .ToList();

            var myResult = new CheckPlayerMoneyResultViewModel
            {
                PlayerID = model.PlayerID,
                IsAmountMatched = gameResults.All(x => x.IsAmountMatched),
                GameResults = gameResults
            };

            string msg = $"PlayerID: {model.PlayerID}{Environment.NewLine}" +
                string.Join(Environment.NewLine, gameResults.Select(x =>
                    $"GameID: {x.GameID}｜注單金額: {x.PerGameAmount}｜金幣紀錄金額: {x.TransactionAmount}"));

            msg += Environment.NewLine + "檢查結果為：" + (myResult.IsAmountMatched ? "✅" : "❌");

            if(!SettingHelper.IsMSTest)
            {
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }

            return myResult;
        }


        public LuckyDrawResultViewModel ManualSendEventBonus(int userID, ManualSendEventBonisRequestModel model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.EventID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetByIdOrThrowException(model.PlayerID);

            var queryMerchant = MerchantRepository.GetByIdOrThrowException(queryPlayer.MerchantID);

            var queryEvent = EventRepository.GetByIdOrThrowException(model.EventID);

            if (queryEvent.EventType != (int)EventTypeEnum.BetAmountDrawRedEnvelope)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("EventType"));
            }

            //正式商戶禁止用此功能
            if (queryMerchant.Type == (int)MerchantTypeEnum.Plaform_ProductionEnvironment)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("MerchantType"));
            }

            var EventLuckyDrawService = IOCHelper.Resolve<IEventLuckyDrawService>();

            var darwResult = EventLuckyDrawService.DrawRandomPrize(new LuckyDrawRequestModel
            {
                BetRecordNoPGS = "ManualTest",
                BetAmount = 10,
                BetTime = new DateTime(2023, 1, 1),
                InitialStatus = EventPrizeRecordStatusEnum.AllocateSuccessNotReceive,
                EventID = model.EventID,
                PlayerID = model.PlayerID,
                GameID = SystemConstant.AGGDefaultGameID,
                IsUseDrawTicket = false
            });

            if (darwResult.DrawPrizeRecordID > 0)
            {
                var result = EventLuckyDrawService.UpdateDrawPrizeStatusAndSendBalance(model.PlayerID, model.EventID);
                return result;
            }
            else
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_EventRewardSendFail, "DrawRandomPrize fail");
            }
        }


        public string LaunchGameByAdminSys(int userID, InternalTestBundleRequestModel model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            CheckViewPlayerPermission(userID, model.PlayerID);

            if (SettingHelper.IsDisablePGSLobby)
            {
                if (model.GameID == 0)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("BWG & GameID=0"));
                }
            }

            var launchResult = GameLaunchService.LaunchGameAndGetGamePageUrlByPlayerID(model)?.Link;

            //寫log紀錄 & 必要的話會發通知
            if (!string.IsNullOrWhiteSpace(launchResult))
            {
                if (SettingHelper.IsProduction)
                {
                    var playerTag = PlayerTagMappingRepository.GetByFilter(new QueryPlayerTagMappingRequestViewModel
                    {
                        PlayerIDs = model.PlayerID.ToListCollection(),
                        TagIDs = SystemTagEnum.Player_FrequentUseTestAccount.ToInteger().ToListCollection()
                    });

                    bool hasTag = playerTag.IsNotEmptyNull();

                    if (!hasTag)
                    {
                        string msg = $"{userID} 從後台模擬產生玩家遊戲連結。{model.PlayerID}";
                        SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
                    }
                }

                var log = InstanceOperateLog(userID, UserOperateTypeEnum.Player_LaunchGameFromAdminSys, model.PlayerID);
                WriteOperateLog(log);
            }

            return launchResult;
        }


        public CustomerServiceUtilityResultModel GetCustmoerServiceUtilityInfo(int userID)
        {
            var defaultUserID = GetDefaultAdminUserID();

            var VendorMaintenanceService = IOCHelper.Resolve<IVendorMaintenanceService>();

            var queryMaintain = VendorMaintenanceService.GetByPagingFilter(defaultUserID, new QueryVendorMaintenanceApiRequestParameterModel
            {
                PageSize = 15,
            });

            var myResult = new CustomerServiceUtilityResultModel
            {
                WarningMessages = new List<CustomerServiceUtilityWarningItem>(),
                GroupItems = new List<CustomerServiceGroupInfo>(),
            };

            var queryMaintainItems = queryMaintain.Items
                .Where(x => !x.IsExpired)
                .Where(x => x.IsProgressing)
                .ToList();

            var globalSystemMaintain = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_IsSwtichToMaintain);
            var vendorAPIMaintain = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_IsVendorAPIMaintain);

            if (globalSystemMaintain.SettingValueOfInt == (int)SystemMaintainStatusEnum.SystemUuderMaintenance)
            {
                myResult.WarningMessages.Add(new CustomerServiceUtilityWarningItem
                {
                    IsErrorHint = true,
                    Message = "全系統停機、更新維護中"
                });
            }
            else if (vendorAPIMaintain.SettingValueOfInt == (int)SystemMaintainStatusEnum.SystemUuderMaintenance)
            {
                myResult.WarningMessages.Add(new CustomerServiceUtilityWarningItem
                {
                    IsErrorHint = true,
                    Message = "VendorAPI維護中。啟動遊戲、轉帳功能將受到限制"
                });
            }
            else if (queryMaintainItems.IsNotEmptyNull())
            {
                var maintainVendors = queryMaintainItems.Select(x => x.VendorName).Distinct().ToList();
                string myMsg = string.Join("、", queryMaintainItems.Select(z => z.VendorName).Distinct().ToList()) + "。遊戲商維護中 (Some of games under maintain)";
                myResult.WarningMessages.Add(new CustomerServiceUtilityWarningItem
                {
                    IsErrorHint = true,
                    Message = myMsg
                });
            }
            else
            {
                myResult.WarningMessages.Add(new CustomerServiceUtilityWarningItem
                {
                    Message = "系統服務目前正常(System working fine now.)"
                });
            }

            var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorIDs = new List<int>
                {
                    GameVendorTypeEnum.AGG.ToInteger(),
                    GameVendorTypeEnum.AGGLive.ToInteger(),
                    GameVendorTypeEnum.Jili.ToInteger(),
                }
            });

            var gameServerCurrency = queryVendorConfig
                .Where(x => x.SupportCurrency.IsNotEmptyTextNotNull())
                .Select(x => x.SupportCurrency.Trim())
                .Distinct()
                .ToList();

            var supportCurrencyItems = new List<CustomerServiceSupportCurrencyInfoItem>();

            foreach (string curr in gameServerCurrency)
            {
                var item = new CustomerServiceSupportCurrencyInfoItem
                {
                    Currency = curr,
                    CurrencyText = MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>(curr),
                };

                if (curr == "IDR")
                {
                    item.Memo = " IDR 1:1 & 1:1000 ";
                }

                if (curr == "VND")
                {
                    item.Memo = " VND 1:1 & 1:1000 ";
                }

                supportCurrencyItems.Add(item);
            }

            myResult.SupportCurrency = supportCurrencyItems
                .OrderBy(x =>
                {
                    if (x.Currency == "IDR")
                    {
                        return 1;
                    }
                    if (x.Currency == "VND")
                    {
                        return 2;
                    }
                    if (x.Currency == "KIDR")
                    {
                        return 3;
                    }
                    if (x.Currency == "KVND")
                    {
                        return 4;
                    }

                    return 999;
                })
                .ThenBy(x => x.Currency)
                .ToList();

            myResult.GroupItems.Add(new CustomerServiceGroupInfo
            {
                GroupName = "Support languages (遊戲支援語言)",
                Items = new List<string>
                {
                    "en-us (English)",
                    "zh-cn (中文)"
                }
            });


            var queryJiliMerchants = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                Type = MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger(),
                EnableSpecificGameIDs = new List<int>
                {
                    390086,
                    390081,
                    390016,
                    390005
                }
            });

            var query = IMChannelRepository.GetMerchantIMChannel(queryJiliMerchants.Select(x => x.MerchantID).ToList());

            if (query.IsNotEmptyNull())
            {
                //VWorld x target

                List<string> channels = query.Select(x => x.IMChannelName)
                    .Distinct()
                    .Select(name =>
                    {
                        if (name.ContainsWihtoutCase("VWorld x target"))
                        {
                            return "target (公告群｜Announcement)";
                        }

                        return name;
                    })
                    .ToList();

                myResult.GroupItems.Add(new CustomerServiceGroupInfo
                {
                    GroupName = "Jili Operator Channel(支援Jili-吉利遊戲的頻道)",
                    Items = channels
                });
            }

            var queryLobbyBackupUrlSettings = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.AllLobbyPageUrlDomainList);

            if (queryLobbyBackupUrlSettings.IsNotEmptyNull())
            {
                myResult.SpeedTestUrls_Basic = queryLobbyBackupUrlSettings.Select(z => z.SettingValue).ToList().FirstOrDefault();
                myResult.SpeedTestUrls_Newest = queryLobbyBackupUrlSettings.Select(z => z.SettingValue).ToList().LastOrDefault();
            }

            myResult.RefreshTimeTW = DateTimeHelper.GetCurrentTime(8).ToString("yyyy-MM-dd HH:mm:ss") + " (UTC+8)";

            return myResult;
        }


        public CRUDResultModel ChangeGameUrlDomainByCS(int userID)
        {
            var queryDoaminUrls = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.GameUrlDoamin_BackupUrls.ToInteger()
            })
            .Where(x => RegexRuleHelper.IsUrlFormat(x.SettingValue))
            .ToList();

            var currentTimeTW = DateTimeHelper.GetCurrentTime(8);
            string dateText_today = currentTimeTW.ToString("yyyy-MM-dd");

            bool isDuplicateCheckToday = queryDoaminUrls
                .Where(x => x.Memo == dateText_today)
                .Any();

            var nextDay = currentTimeTW.AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00 (GMT+8)";

            if (isDuplicateCheckToday)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"您今日已操作過更換域名，請勿重複操作，需到隔日時間: {nextDay} 才允許操作，若很緊急請通知技術單位並截圖。");

            var notUsedUrls = queryDoaminUrls
                .Where(x => string.IsNullOrWhiteSpace(x.Memo)) //memo有填值的代表已經使用過
                .ToList();

            if (notUsedUrls.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "請通知並截圖給技術單位。備用域名不足");

            var matchFirst = notUsedUrls.FirstOrDefault();

            string newGamePageUrl = matchFirst.SettingValue;

            //檢查圖片網址是否有效
            CheckImageUrlWork(newGamePageUrl);

            var queryMerchants = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                CurrencyList = new List<string> { "IDR", "KIDR" },
                Type = MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger()
            });

            if (queryMerchants.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            var curretUser = GetCurrentOwnPermissionByCache(userID);

            string merchantIDsText = string.Join(",", queryMerchants.Select(z => z.MerchantID));

            var MerchantCustomizedSettingService = IOCHelper.Resolve<IMerchantCustomizedSettingService>();

            // Step 1. 更新
            var result = MerchantCustomizedSettingService.BatchUpdateMerchantAdvanceSettings(userID, new MerchantBatchUpdateSettingRequestModel
            {
                MerchantIDsRawText = merchantIDsText,
                SettingKey = MerchantCustomizedSettingKeyEnum.MerchantCustomized_GamePageDomainUrl.ToInteger(),
                SettingValue = newGamePageUrl,
            });

            // Step 2. 更新memo欄位 標注已被使用過
            matchFirst.Memo = dateText_today;
            OneToManyCustomizedSettingItemRepository.Update(matchFirst);


            // Step 3. 頻道通知
            if (SettingHelper.IsProduction)
            {
                string msg = $"{curretUser.UserName} ({curretUser.UserID}) 操作了域名封鎖更換。本次使用域名 {matchFirst.SettingValue} 。備用域名還剩下 {(notUsedUrls.Count() - 1)}組";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }

            string speedTestPageUrl = (newGamePageUrl + "/lobby?page=speedTest").ReplaceDuplicateUrlChar();

            return new CRUDResultModel
            {
                Message = result.Message,
                ResultExtraString = speedTestPageUrl
            };
        }


        private void CheckImageUrlWork(string domain)
        {
            string targetUrl = (domain + "/game-icon/agg_ht-trainrobber.png").ReplaceDuplicateUrlChar();

            bool isUrlWork = false;

            string msg = $"請通知並截圖給技術單位，域名似乎失效 {domain} 、無法獲取圖片。";

            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] data = client.DownloadData(targetUrl);
                    isUrlWork = data.Length > 25000;
                }
            }
            catch
            {
                isUrlWork = false;
            }

            if (!isUrlWork && !SettingHelper.IsMSTest)
            {
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, msg);
            }
        }


        public string BatchCreateNewMerchantByEachParentAndPrefixName(int userID, MerchantBatchCopyAndCreateRequestModel model)
        {
            model.RuleFor(x => x.ExtraConfig).Rule_ModelNotNull();

            var extraParam = model.ExtraConfig;

            extraParam.RuleFor(x => x.MultipleParentMerchantIDs).Rule_IsNotEmptyCollection();
            extraParam.RuleFor(x => x.PrefixName).Rule_NotEmptyText();

            var queryMerchants = MerchantRepository.GetByIds(extraParam.MultipleParentMerchantIDs);

            int numberOfMerchants = 0;

            List<int> newMerchantIDs = new List<int>();

            foreach (var merchantID in extraParam.MultipleParentMerchantIDs)
            {
                var matchMerchants = queryMerchants.FirstOrDefault(x => x.MerchantID == merchantID);

                if (matchMerchants == null)
                    continue;

                var myCloneObject = model.ReturnInjecteObject<MerchantBatchCopyAndCreateRequestModel>();

                string myMerchantName = extraParam.PrefixName + matchMerchants.Currency;

                myCloneObject.MerchantNameList = new List<string> { myMerchantName };
                myCloneObject.Currency = matchMerchants.Currency;
                myCloneObject.ParentMerchantID = merchantID;
                myCloneObject.SourceMerchantID = merchantID;

                //如果有指定要創建帳號-帳號模式A (可以不填寫)
                if (model.CreateAccountInfo != null && model.CreateAccountInfo.RoleID > 0)
                {
                    if (!model.CreateAccountInfo.IsSingleAdminAccount)
                    {
                        myCloneObject.IsCreateUserAccount = true;
                        myCloneObject.CreateAccountInfo = model.CreateAccountInfo;
                        myCloneObject.CreateAccountInfo.Account = myMerchantName;
                    }
                }

                var result = CopyAndBatchCreateNewMerchant(userID, myCloneObject);

                if (result.ResultId > 0)
                {
                    newMerchantIDs.Add(result.ResultId);
                }

                numberOfMerchants++;
            }


            if (model.CreateAccountInfo != null && model.CreateAccountInfo.RoleID > 0)
            {
                if (model.CreateAccountInfo.IsSingleAdminAccount)
                {
                    var userModel = model.CreateAccountInfo;

                    string userName = userModel.Account;

                    if (!userName.EndsWith("Admin"))
                    {
                        userName += "Admin";
                    }

                    UsersService.Create(userID, new UserCreateModel
                    {
                        Password = [REDACTED],
                        ConfirmPassword = [REDACTED],
                        DisplayName = userName,
                        UserName = userName,
                        OrganizationID = userModel.OrganizationID,
                        RoleID = userModel.RoleID,
                        TimeZoneAreaID = TimeZoneAreaEnum.China_0800.ToInteger(),
                        AssignMerchantMode = UserAssignMerchantModeEnum.ViewSelectMerchant.ToInteger(),
                        Merchants = newMerchantIDs,
                        IsAutoAddOrgPrefix = true
                    });
                }
            }

            if (SettingHelper.IsProduction)
            {
                string msg = $"本次共新增{numberOfMerchants}個商戶 ({extraParam.PrefixName})";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }

            return $"本次共創建 {newMerchantIDs.Count()} 筆資料";
        }



        public CRUDResultModel CopyAndBatchCreateNewMerchant(int userID, MerchantBatchCopyAndCreateRequestModel model)
        {
            model.ExtraSettingFlag = model.ExtraSettingFlag ?? new List<int>();

            //先檢查帳號重複、格式等問題
            if (model.IsCreateUserAccount)
            {
                ValidateBeforeCreateAccount(userID, model);
            }

            //對於部分內部人員操作 需要加強防呆
            if (SettingHelper.IsProduction && HasTargetPermission(userID, "Special_SuperAdminOrg_ButNeedValidateWhenSubmit"))
            {
                if (model.IMChannelID.IsNullOrZeroOrMinus())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "IM頻道需填寫。若為新客戶請協助創建頻道。如無頻道請選擇 [預設] 未分類-Default");
                }
            }

            // Step 1. 創建商戶Merchant、複製配置
            var result = MerchantService.CopyAndBatchCreateNewMerchant(userID, model);

            int merchantID = result.ResultId;

            bool isCreateAsRootAgent = model.ExtraSettingFlag.Any(z => z == (int)MerchantCopyExtraSettingEnum.SetAsRootAgent);

            if (isCreateAsRootAgent && merchantID > 0)
            {
                // Step 1-2. 創建為總代
                MerchantHierarchyService.CreateAsRootMerchantHierarchy(userID, merchantID);
            }

            var resultModel = new CRUDResultModel
            {
                IsSuccess = true,
                ResultId = merchantID,
                DebugMessage = result?.DebugMessage,
            };

            //後台帳號: 僅支援單筆建立商戶時、同時新增建立
            if (model.IsCreateUserAccount && result.ResultListIDs.IsNotEmptyNull())
            {
                // Step 2. 創建用戶帳號
                model.CreateAccountInfo.MerchantID = merchantID;
                UsersService.CreateUserByQuickSetting(userID, model.CreateAccountInfo);
            }
            else
            {
                var queryNewMerchant = MerchantRepository.GetById(merchantID);

                //如果 type=[測試商戶] 自動創建帳號 (需有配置才會)
                if (queryNewMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
                {
                    var createAccountInfo = (model.CreateAccountInfo ?? new MerchantCopyCreateUserAccountExtraProperty()).ReturnInjecteObject<MerchantCopyCreateUserAccountExtraProperty>();

                    createAccountInfo.MerchantID = merchantID;
                    createAccountInfo.Account = $"demo_{merchantID}";
                    createAccountInfo.Password = [REDACTED];

                    var defaultOrgID = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_BO_DevAccount_DefaultOrgID);
                    var defaultRoleID = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_BO_DevAccount_DefaultRoleID);

                    //如果有配置 才會有創建流程
                    if (defaultOrgID.SettingValueOfInt > 0 && defaultRoleID.SettingValueOfInt > 0)
                    {
                        createAccountInfo.OrganizationID = defaultOrgID.SettingValueOfInt;
                        createAccountInfo.RoleID = defaultRoleID.SettingValueOfInt;

                        UsersService.CreateUserByQuickSetting(userID, createAccountInfo);
                    }
                }
            }

            return resultModel;
        }


        public CRUDResultModel CreateDevMerchantByAM(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.DisplayName).Rule_NotEmptyText();
            model.RuleFor(x => x.Type).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.IMChannelID).Rule_IsNotNullNotZeroNotMinus();

            if (!HasSuperAdminPermission(userID))
            {
                string cacheKey = CacheKey.AdminSys_CreateDevMerchantCounter(userID);

                int times = CacheHelper.CounterMaxTimesDaily(cacheKey);

                if (times >= 20)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes, new ErrorDetailModel("CreateDevMerchantByAM"));
                }
            }

            string finalMerchantName = model.DisplayName;

            if (!model.DisplayName.ContainsWihtoutCase("測試用") && !model.DisplayName.ContainsWihtoutCase("*Dev"))
            {
                finalMerchantName = "(*Dev) " + model.DisplayName;
            }

            int templateMerchantID = 0;

            if (model.Type == (int)MerchantWalletModeEnum.TransferMode)
            {
                templateMerchantID = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_TemplateMerchantID_Trasnfer).SettingValueOfInt;
            }
            else
            {
                templateMerchantID = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_TemplateMerchantID_SingleWallet).SettingValueOfInt;
            }

            var result = CopyAndBatchCreateNewMerchant(userID, new MerchantBatchCopyAndCreateRequestModel
            {
                SourceMerchantID = templateMerchantID,
                IMChannelID = model.IMChannelID,
                WalletMode = model.Type,
                MerchantNameList = new List<string>
                            {
                                finalMerchantName
                            }
            });

            int resultMerchantID = result.ResultId;

            UserMerchantMappingRepository.AppendNewMerchant(userID, resultMerchantID);

            CacheRemoveHelper.RemoveCache_WhenUpdateUser(userID);

            return new CRUDResultModel
            {
                IsSuccess = true,
            };
        }



        private void ValidateBeforeCreateAccount(int userID, MerchantBatchCopyAndCreateRequestModel model)
        {
            var insertModel = UsersService.ConvertToUserCreateModel(model.CreateAccountInfo);

            UsersService.ValidateBeforeCreateUser(userID, insertModel);
        }



        /// <summary>
        /// 類似狀況發生很多次了，到現在還不知道怎操作 很傻眼
        /// </summary>
        private void CheckInvalidOperateByRichmond(int userID, MerchantCopyByAgentRequestModel model)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (currentUser.IsSuperAdminOrg)
                return;

            if (model.ParentMerchantID <= 0)
                return;

            var queryForbiddenChannels = GeneralTagMappingRepository.GetByTagID((int)GeneralTagEnum.ForbiddenCreateMerchantChannels);

            if (queryForbiddenChannels.IsNotEmptyNull() && model.IMChannelID.HasValue)
            {
                int IMChannelID = model.IMChannelID.Value;

                if (queryForbiddenChannels.Any(z => z.TargetID == IMChannelID))
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel
                    {
                        IsAlwaysNotify = true,
                        ReplaceMessage = "您选择的对接频道禁止创建商户。请告知技术团队、并透过技术团队来设定"
                    });
                }
            }

            var queryMerchant = MerchantRepository.GetById(model.ParentMerchantID.Value);

            string currencyLower = queryMerchant.Currency.ToLower();

            if (queryMerchant.Name.ContainsWihtoutCase("myc-super"))
            {
                if (model.Account.ContainsWihtoutCase("super") &&
                    model.Account.ContainsWihtoutCase("sub"))
                {
                    bool isKeywordMatch1 = model.Name.ContainsWihtoutCase("super") && model.Name.ContainsWihtoutCase("sub");
                    bool isKeywordMatch2 = model.Name.ContainsWihtoutCase("super" + currencyLower);

                    if (isKeywordMatch1 || isKeywordMatch2)
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny, "操作异常、若要创建子帐号。请前往后台 《帐号管理》");
                    }
                }
            }
        }


        public CRUDResultModel CopyMerchantByAgent(int userID, MerchantCopyByAgentRequestModel model)
        {
            model.RuleFor(x => x.Name).Rule_NotEmptyText();
            model.RuleFor(x => x.RoleID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.OrganizationID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.ParentMerchantID).Rule_IsNotNullNotZeroNotMinus();

            model.Account = model.Account.TrimOrNull();
            model.RuleFor(x => x.Account).Rule_NotEmptyText().Rule_IsValidAccountFormat();
            model.RuleFor(x => x.Password).Rule_NotEmptyText().Rule_IsValidPasswordFormat();

            if (model.IMChannelID.HasValue)
            {
                model.RuleFor(x => x.IMChannelID).Rule_NotMinus().Rule_IsNotZero();
            }

            if (model.ConfirmPassword.IsNotEmptyTextNotNull())
            {
                if (model.Password != model.ConfirmPassword)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmPasswordNotEqualPassword);
                }
            }

            model.WhiteListIPAddress = (model.WhiteListIPAddress ?? "").Trim();

            bool isAppendWhiteListIPs = false;

            if (string.IsNullOrWhiteSpace(model.WhiteListIPAddress) || (model.WhiteListIPAddress.Length <= 5 && model.WhiteListIPAddress != "0"))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_WhiteListAddressEmpty);
            }

            if (model.WhiteListIPAddress != "0")
            {
                RegexRuleHelper.IsMultipleIPAddressFormat(model.WhiteListIPAddress, isThrowException: true);
                isAppendWhiteListIPs = true;
            }

            //智障使用者操作防呆
            CheckInvalidOperateByRichmond(userID, model);

            //先檢查帳號重複
            UsersService.IsAccountExistThenThrowException(model.OrganizationID, model.Account);

            bool isContainsPrefix = UsersService.IsContainsOrganizationPrefix(model.OrganizationID, model.Account);

            if (isContainsPrefix)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "输入的帐号无需包含前缀字");

            //檢查權限
            CheckViewMerchantPermission(userID, model.ParentMerchantID.Value);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //禁止創建二級代理 只能創建三、四級
            if (HasTargetPermission(userID, "Agent_MerchantCredit_DisableCreateLayer2Agent"))
            {
                var queryAgent = MerchantHierarchyRepository.GetById(model.ParentMerchantID.Value);

                if (AppHelper.GetAgentLayerLevelByHierarchy(queryAgent.Hierarchy) == 1)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_DoNotAllowCreateTargetLayerAgent, new ErrorDetailModel("master agent level"));
                }
            }

            bool isExist = MerchantService.IsMerchantNameExist(model.Name).IsExist;

            if (isExist)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist, new ErrorDetailModel("MerchantName"));


            var myParam = model.ReturnInjecteObject<MerchantBatchCopyAndCreateRequestModel>();
            myParam.SourceMerchantID = model.ParentMerchantID.Value;
            myParam.MerchantNameList = new List<string>
                {
                    model.Name
                };

            var extraConfig = new UserCreateValidateParameter
            {
                IsSkipCheckMissPrefix = true
            };

            //創建帳號前防呆檢查
            UsersService.ValidateBeforeCreateUser(userID, new UserCreateModel
            {
                UserName = model.Account,
                DisplayName = model.Account,
                OrganizationID = model.OrganizationID,
                RoleID = model.RoleID,
                Password = [REDACTED],
                ConfirmPassword = [REDACTED],
            }, extraConfig);

            //複製商戶設定、insert資料、樹狀層級、邏輯最複雜
            //複製商戶設定、insert資料、樹狀層級、邏輯最複雜
            //複製商戶設定、insert資料、樹狀層級、邏輯最複雜
            var result = MerchantService.CopyAndBatchCreateNewMerchant(userID, myParam);

            int newMerchantID = result.ResultListIDs.FirstOrDefault();

            //新增後台帳號
            UsersService.CreateUserByQuickSetting(userID, new MerchantCopyCreateUserAccountExtraProperty
            {
                Account = model.Account,
                IsHierarchyMode = true,
                MerchantID = newMerchantID,
                Password = [REDACTED],
                RoleID = model.RoleID,
                OrganizationID = model.OrganizationID
            });

            //寫入IP配置
            if (isAppendWhiteListIPs)
            {
                AppendIPsToSetting(userID, newMerchantID, model.WhiteListIPAddress);
            }

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.Agent_CreateNewMerchantByAgent, newMerchantID);
            log.Memo = $"{model.WhiteListIPAddress}";

            WriteOperateLog(log);

            CacheRemoveHelper.RemoveCache_WhenUserLoginOrAvailableMerchantChanged(userID);

            return new CRUDResultModel
            {
                ResultId = newMerchantID
            };
        }


        public LazyCommonResultViewModel AppendIPsToSetting(int userID, int merchantID, string whiteListIPs)
        {
            var query = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting);

            int ipTemplateID = query.SettingValueOfInt;

            var myResult = new LazyCommonResultViewModel
            {
                ResultID = ipTemplateID
            };

            //查無配置 直接返回
            if (ipTemplateID <= 0)
                return myResult;

            var newIPsCheck = IIPAddressListTemplateService.AnalyzeAnyNewIPs(ipTemplateID, whiteListIPs);

            if (newIPsCheck.NewIPs.IsEmptyOrNull())
                return myResult;

            var newIPs = newIPsCheck.NewIPs;

            //單一商戶白名單
            IIPAddressListTemplateService.SetWhiteListIPsForSingleMerchant(userID, merchantID, newIPs);

            //如果是超大型代理 不要編輯共用白名單
            if (!newIPsCheck.IsBigAgent)
            {
                //全體代理(該樹狀)共用白名單
                IIPAddressListTemplateService.AppendMultipleIPsToSetting(userID, new IPAddressListMultipleAppendRequestModel
                {
                    Id = ipTemplateID,
                    MultipleIPAddress = whiteListIPs,
                    IsSkipInsertWhenWildCard = true,
                    Memo = $"{merchantID}"
                });
            }

            myResult.ResultIPAddresss = newIPs;
            return myResult;
        }


        public PublicApiBetRecordReplayInfoViewModel GetBetRecordReplayInfoByGameVendor(LazyCommonRequestModel model)
        {
            //ps. token已經在API層呼叫另個function驗證了
            model.RuleFor(x => x.Token).Rule_NotEmptyText().Rule_NotUndefined();
            model.RuleFor(x => x.TransactionNo).Rule_NotEmptyText().Rule_NotUndefined();

            var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(model.TransactionNo);

            if (queryMapping == null)
                throw new TerproException(ApiErrorCode.NotFoundData);





            PublicApiBetRecordReplayInfoViewModel viewModel = null;

            //根據遊戲商會略有不同的邏輯
            if (queryMapping.GameVendorID == (int)GameVendorTypeEnum.AGGLive)
            {
                viewModel = GetAGGLiveBetRecordReplayInfoByApi(queryMapping.GameVendorID, model.TransactionNo);
            }
            else if (queryMapping.GameVendorID == (int)GameVendorTypeEnum.Galaxy)
            {
                viewModel = GetGalaxyBetRecordReplayInfoByApi(queryMapping.GameVendorID, model.TransactionNo);
            }
            else if (queryMapping.GameVendorID == (int)GameVendorTypeEnum.RG)
            {
                viewModel = GetRGSlotGameResultInfoByApi(queryMapping.GameVendorID, model.TransactionNo);
            }
            else if (queryMapping.GameVendorID == (int)GameVendorTypeEnum.target)
            {
                var PerGameDetailService = IOCHelper.Resolve<IPerGameDetailService>();
                var pgsInfo = PerGameDetailService.GetSlotHistoryByTransactionNo(SystemConstant.SkipValidateUserID, model.TransactionNo);
                viewModel = new PublicApiBetRecordReplayInfoViewModel
                {
                    PgsSlotsHistoryInfo = pgsInfo,
                    BasicInfo = new BetRecordBasicInfo
                    {
                        BetRecordNoPGS = pgsInfo.PerGameTransactionNo,
                        BetAmount = pgsInfo.BetAmount,
                        GameName = pgsInfo.GameName,
                        PlayerName = pgsInfo.PlayerName,
                        CreateTime = pgsInfo.GameTime,
                        VideoUrl = "",
                        PayoutAmount = pgsInfo.PayoutAmount,
                    }
                };
            }
            else
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
            }

            viewModel.BasicInfo.GameVendorID = queryMapping.GameVendorID;
            return viewModel;
        }


        public PublicApiBetRecordReplayInfoViewModel GetGalaxyBetRecordReplayInfoByApi(int vendorID, string transactionNo)
        {
            var vendorService = ServiceInstanceHelper.InstanceVendorService((GameVendorTypeEnum)vendorID);

            var queryRecord = vendorService.GetByPGSTransactionNo(transactionNo);

            if (queryRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (!queryRecord.VendorConfigID.HasValue)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("VendorConfigID"));

            var galaxyIntegrationService = new GalaxyIntegrationService();

            var queryInfo = galaxyIntegrationService.GetExternalBetDetailAndScreenshotInfo(new ExternalVendorBetRecordDetailInfoRequestModel
            {
                BetRecordNoPGS = queryRecord.BetRecordNoPGS,
            });

            if (queryInfo == null || queryInfo.data2 == null)
            {
                var now = DateTimeHelper.GetCurrentTime();

                if ((now - queryRecord.CreateTime).TotalDays >= 32)
                {
                    throw new TerproException(ApiErrorCode.NotFoundBetRecordOrDeleteAlready, new ErrorDetailModel($"Detail data expired : {queryRecord.CreateTime.ToString("yyyy-MM-dd")}"));
                }
            }

            var viewModel = new PublicApiBetRecordReplayInfoViewModel
            {
                SettleResultImageList = new List<RoundInfoItemModel>(),
                BetList = new List<AGGLiveBackOfficeRecordDetailApiBetList>(),
                IsShowBriefAmountOnly = true,
                BasicInfo = new BetRecordBasicInfo
                {
                    VideoUrl = "",
                    CreateTime = queryRecord.CreateTime,
                    GameName = DbMultiLanguageTextHelper.GetGameDisplayName(queryRecord.GameID),
                    BetRecordNoPGS = queryRecord.BetRecordNoPGS,
                    PlayerName = queryRecord.PlayerName,
                    BetAmount = queryRecord.BetAmount,
                    PayoutAmount = queryRecord.PayoutAmount,
                }
            };

            if (queryInfo != null && queryInfo.data2 != null)
            {
                foreach (var item in queryInfo.data2)
                {
                    string category = item.seat;

                    if (category == "B")
                    {
                        category = "Banker";
                    }
                    else if (category == "P1" || category == "P")
                    {
                        category = "Player";
                    }

                    var images = item.card.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                    viewModel.SettleResultImageList.Add(new RoundInfoItemModel
                    {
                        Category = category,
                        Images = images
                    });
                }

                viewModel.ListOfRoundResultText = new List<string>();

                foreach (var item in queryInfo.data3)
                {
                    viewModel.ListOfRoundResultText.Add(item.betCodeName);
                }


                if (queryInfo.data4.IsNotEmptyNull())
                {
                    foreach (var item in queryInfo.data4)
                    {
                        viewModel.BetList.Add(new AGGLiveBackOfficeRecordDetailApiBetList
                        {
                            bet_amount = item.amount,
                            bet_type = item.betCodeName
                        });
                    }
                }

                viewModel.BasicInfo.VideoUrl = queryInfo.data.recordingUrl;
                viewModel.BasicInfo.VideoFormat = VideoFormatTypeEnum.Mp4.ToInteger();
            }

            return viewModel;
        }

        public PublicApiBetRecordReplayInfoViewModel GetRGSlotGameResultInfoByApi(int vendorID, string transactionNo)
        {
            var vendorService = ServiceInstanceHelper.InstanceVendorService((GameVendorTypeEnum)vendorID);

            var queryRecord = vendorService.GetByPGSTransactionNo(transactionNo);

            if (queryRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (!queryRecord.VendorConfigID.HasValue)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("VendorConfigID"));

            var gameVendorIntegrationService = new RGIntegrationService();

            var queryInfo = gameVendorIntegrationService.GetExternalBetDetailAndScreenshot(new ExternalVendorBetRecordDetailInfoRequestModel
            {
                BetRecordNoPGS = queryRecord.BetRecordNoPGS,
            });

            var viewModel = new PublicApiBetRecordReplayInfoViewModel
            {
                RawHtml = queryInfo.RawJson,
                SettleResultImageList = new List<RoundInfoItemModel>(),
                BasicInfo = new BetRecordBasicInfo
                {
                    IsRawHtmlLayout = true,
                    CreateTime = queryRecord.CreateTime,
                    GameName = DbMultiLanguageTextHelper.GetGameDisplayName(queryRecord.GameID),
                    BetRecordNoPGS = queryRecord.BetRecordNoPGS,
                    PlayerName = queryRecord.PlayerName,
                    BetAmount = queryRecord.BetAmount,
                    PayoutAmount = queryRecord.PayoutAmount,
                }
            };

            return viewModel;
        }


        public PublicApiBetRecordReplayInfoViewModel GetAGinBetRecordReplayInfoByApi(int vendorID, string transactionNo)
        {
            var vendorService = ServiceInstanceHelper.InstanceVendorService((GameVendorTypeEnum)vendorID);

            var queryRecord = vendorService.GetByPGSTransactionNo(transactionNo);

            if (queryRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (!queryRecord.VendorConfigID.HasValue)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("VendorConfigID"));

            var gameVendorIntegrationService = new AGinIntegrationService();

            var queryInfo = gameVendorIntegrationService.GetExternalBetDetailAndScreenshotInfo(new ExternalVendorBetRecordDetailInfoRequestModel
            {
                BetRecordNoPGS = queryRecord.BetRecordNoPGS,
            });

            var viewModel = new PublicApiBetRecordReplayInfoViewModel
            {
                SettleResultImageList = new List<RoundInfoItemModel>(),
                BasicInfo = new BetRecordBasicInfo
                {
                    VideoUrl = "",
                    CreateTime = queryRecord.CreateTime,
                    GameName = DbMultiLanguageTextHelper.GetGameDisplayName(queryRecord.GameID),
                    BetRecordNoPGS = queryRecord.BetRecordNoPGS,
                    PlayerName = queryRecord.PlayerName,
                    BetAmount = queryRecord.BetAmount,
                    PayoutAmount = queryRecord.PayoutAmount,
                }
            };

            if (queryInfo != null)
            {
                var splitCardNumbers = AppHelper.AnalyzeIDsFromRawText(queryInfo.CardList);

                if (splitCardNumbers.IsNotEmptyNull())
                {
                    List<string> images = new List<string>();
                    foreach (var code in splitCardNumbers)
                    {
                        images.Add(code.ToString());
                    }

                    //查詢DB 對應圖片                       
                    viewModel.SettleResultImageList.Add(new RoundInfoItemModel
                    {
                        Category = "Result",
                        Images = images
                    });
                }

                viewModel.ListOfRoundResultText = new List<string>();
                viewModel.ListOfRoundResultText.Add(queryRecord.ExtraInfo1);
            }

            return viewModel;
        }



        public PublicApiBetRecordReplayInfoViewModel GetAGGLiveBetRecordReplayInfoByApi(int vendorID, string transactionNo)
        {
            var vendorService = ServiceInstanceHelper.InstanceVendorService((GameVendorTypeEnum)vendorID);

            var queryRecord = vendorService.GetByPGSTransactionNo(transactionNo);

            if (queryRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (!queryRecord.VendorConfigID.HasValue)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("VendorConfigID"));

            string imageDomain = GetAGGLiveImageDomain();

            //測試環境直接回假資料
            if (SettingHelper.IsDev && !SettingHelper.IsMSTest && !SettingHelper.IsProductionDbConnection)
            {
                var fakeData = GenerateFakeData(imageDomain);
                fakeData.BetList = GetBetListByDetailJson(transactionNo, queryRecord.GameID);
                return fakeData;
            }

            var loopTimes = AppHelper.InstanceLoopTimes(2);

            //透過API查出AGG注單的詳細記錄 (包含回放影片)
            AGGLiveBackOfficeRecordDetailApiResponse betDetailApiResult = GetAGGLiveRecordDetailByCacheOrCallApi(queryRecord.VendorConfigID.Value, queryRecord.BetRecordNoPGS);

            if (betDetailApiResult == null || betDetailApiResult.result == null || betDetailApiResult.result.detail == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected);

            var viewModel = new PublicApiBetRecordReplayInfoViewModel
            {
                BetList = betDetailApiResult.result.detail.bet_info.bet_list.ToList(),
                SettleResultImageList = new List<RoundInfoItemModel>(),
                BasicInfo = new BetRecordBasicInfo
                {
                    VideoUrl = betDetailApiResult.result.round,
                    CreateTime = queryRecord.CreateTime,
                    GameName = DbMultiLanguageTextHelper.GetGameDisplayName(queryRecord.GameID),
                    BetRecordNoPGS = queryRecord.BetRecordNoPGS,
                    PlayerName = queryRecord.PlayerName,
                    BetAmount = queryRecord.BetAmount,
                    PayoutAmount = queryRecord.PayoutAmount,
                }
            };

            if (viewModel.BetList.IsNotEmptyNull())
            {
                foreach (var item in viewModel.BetList)
                {
                    if (item.bet_type_images.IsNotEmptyNull())
                    {
                        item.bet_type_images = item.bet_type_images.Select(x => ConcateImageAndImageDomain(imageDomain, x)).ToList();
                    }
                }
            }

            var round_info = betDetailApiResult.result.detail.round_info.FirstOrDefault();

            string superNumberInfo = string.Empty;

            if (round_info.super_number.IsNotEmptyNull())
            {
                string oddsText = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_LabelColumn_odds");

                List<string> superNumberTextList = round_info.super_number
                    .Select(x => $"{x.description} {(x.super_odds.IsNotEmptyTextNotNull() ? $"{oddsText}:{x.payout}" : "")}")
                    .ToList();

                superNumberInfo = string.Join(Environment.NewLine, superNumberTextList);
            }

            ConvertToRoundInfoModelAndAppendToList(imageDomain, "Banker Card", round_info.banker_card, viewModel.SettleResultImageList);
            ConvertToRoundInfoModelAndAppendToList(imageDomain, "Player Card", TryParseObjectToListString(round_info.player_card), viewModel.SettleResultImageList);
            ConvertToRoundInfoModelAndAppendToList(imageDomain, "Player Card 1", round_info.player1_card, viewModel.SettleResultImageList);
            ConvertToRoundInfoModelAndAppendToList(imageDomain, "Player Card 2", round_info.player2_card, viewModel.SettleResultImageList);
            ConvertToRoundInfoModelAndAppendToList(imageDomain, "Player Card 3", round_info.player3_card, viewModel.SettleResultImageList);
            ConvertToRoundInfoModelAndAppendToList(imageDomain, "Dragon Card", round_info.dragon_card, viewModel.SettleResultImageList);
            ConvertToRoundInfoModelAndAppendToList(imageDomain, "Tiger Card", round_info.tiger_card, viewModel.SettleResultImageList);
            ConvertToRoundInfoModelAndAppendToList(imageDomain, "Dice Result", round_info.dice, viewModel.SettleResultImageList, superNumberInfo);

            if (!string.IsNullOrWhiteSpace(round_info.ball))
            {
                string memoInfo = $"Ball number: {round_info.ball} , SuperNumber: {superNumberInfo}";
                viewModel.SettleResultImageList.Add(new RoundInfoItemModel
                {
                    Category = "",
                    Images = new List<string>(),
                    IsFullWidthDisplay = true,
                    MemoInfo = memoInfo
                });
            }

            if (viewModel.BetList.IsEmptyOrNull())
            {
                viewModel.BetList = GetBetListByDetailJson(transactionNo, queryRecord.GameID);
            }

            viewModel.BasicInfo.VideoFormat = VideoFormatTypeEnum.XmpegURL.ToInteger();
            return viewModel;
        }


        private List<string> TryParseObjectToListString(object myObject)
        {
            if (myObject == null)
                return new List<string>();

            var parse1 = myObject.ConvertToJsonString().TryJsonConvertToModel<List<string>>();

            if (parse1.IsNotEmptyNull())
                return parse1;

            var parse2 = myObject.ConvertToJsonString().TryJsonConvertToModel<List<List<string>>>();

            if (parse2.IsNotEmptyNull())
                return parse2.SelectMany(x => x).ToList();

            return new List<string>();
        }


        protected virtual AGGLiveBackOfficeRecordDetailApiResponse GetAGGLiveRecordDetailByCacheOrCallApi(int vendorConfigID, string betRecordNoPGS)
        {
            var loop = new RetryLoopParameter
            {
                MaxTimes = 2,
                Tag = "GetBetInfoByBackOfficeApi",
                DelayMilliseconds = 100
            };

            AGGLiveBackOfficeRecordDetailApiResponse betDetailApiResult = null;

            string cacheKey = CacheKey.BetRecordScreenShotDetailInfo(betRecordNoPGS);

            var cacheValue = CacheHelper.GetCacheValue<AGGLiveBackOfficeRecordDetailApiResponse>(cacheKey);

            if (cacheValue != null)
            {
                return cacheValue;
            }

            AppHelper.ExecuteMethodAndRetryMaxTimes(loop, (param) =>
            {
                var lastToken = [REDACTED];

                betDetailApiResult = AGGLiveIntegrationService.GetBetInfoByBackOfficeApi(betRecordNoPGS, lastToken.TokenResult, vendorConfigID);

                //如果token失效 重製token
                if (betDetailApiResult.status.Contains("Token") && betDetailApiResult.status.Contains("Error"))
                {
                    DeleteAGGLiveTempTokenValue(vendorConfigID, lastToken.OneToManyCustomizedSettingID);
                    return false;
                }
                else
                {
                    return true;
                }
            });

            if (betDetailApiResult != null &&
               betDetailApiResult.code == 1000 &&
               betDetailApiResult?.result?.detail?.round_info != null)
            {
                CacheHelper.SetCacheValue(cacheKey, betDetailApiResult, new TimeSpan(0, 30, 0));
            }

            return betDetailApiResult;
        }


        private List<AGGLiveBackOfficeRecordDetailApiBetList> GetBetListByDetailJson(string transactionNoPGS, int gameID)
        {
            var queryRecord = AGGLiveDetailJsonRepository.GetById(transactionNoPGS);

            var listResult = new List<AGGLiveBackOfficeRecordDetailApiBetList>(); ;

            if (queryRecord == null)
            {
                return listResult;
            }

            var parseDetail = queryRecord.DetailJson.JsonConvertToModel<AGGLiveSettlementDetailInfoModel>();

            if (parseDetail == null || parseDetail.BetList.IsEmptyOrNull())
            {
                return listResult;
            }

            var querySettings = GameListCustomizedSettingRepository.GetByFilter(new QueryGameListCustomizedSettingRequestViewModel
            {
                GameID = gameID,
                SettingKey = GameListCustomizedSettingKeyEnum.LiveGameBetDetailMappingLanguageText.ToInteger()
            });


            var myLang = HeaderValueHelper.GetCurrentLanguage();

            bool isCN = AppHelper.IsChineseLang(myLang);

            foreach (var item in parseDetail.BetList)
            {
                var matchSetting = querySettings.FirstOrDefault(x => x.Category1 == item.type.ToString() && x.Category2 == item.index.ToString());

                var viewModel = new AGGLiveBackOfficeRecordDetailApiBetList
                {
                    bet_amount = item.amount,
                    tax_amount = item.tax,
                    win_amount = item.win,
                    no_commission = item.noCommission ? "Y" : "",
                };

                if (matchSetting == null)
                {
                    viewModel.bet_type = $"type:{item.type},index:{item.index}";
                }
                else
                {
                    if (isCN)
                    {
                        viewModel.bet_type = matchSetting.MemoCN;
                    }
                    else
                    {
                        viewModel.bet_type = matchSetting.MemoEN;
                    }

                    if (string.IsNullOrWhiteSpace(viewModel.bet_type))
                    {
                        viewModel.bet_type = matchSetting.Memo;
                    }
                }

                listResult.Add(viewModel);
            }

            //字母排序
            return listResult.OrderBy(x => x.bet_type).ToList();
        }


        private string GetAGGLiveImageDomain()
        {
            var setting = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.GameSetting_AGGLiveImageFolderPrefixUrl);
            string imageDomain = setting.SettingValue;

            if (string.IsNullOrWhiteSpace(imageDomain))
                imageDomain = "https://example.invalid/images/game";

            imageDomain = imageDomain.TruncateLastWordIfMatch('/');
            return imageDomain;
        }


        private void ConvertToRoundInfoModelAndAppendToList(string settingImageDomain, string category, List<string> imageList, List<RoundInfoItemModel> roundInfo, string extraMemoInfo = "")
        {
            if (imageList.IsEmptyOrNull())
                return;

            var instance = new RoundInfoItemModel
            {
                Category = category,
                MemoInfo = extraMemoInfo,
                Images = new List<string>(),
            };

            if (imageList.IsNotEmptyNull())
            {
                instance.Images = imageList
                    .Select(image => ConcateImageAndImageDomain(settingImageDomain, image))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(instance.MemoInfo))
            {
                instance.IsFullWidthDisplay = true;
            }

            roundInfo.Add(instance);
        }

        private string ConcateImageAndImageDomain(string settingImageDomain, string imageUrl)
        {
            return $"{settingImageDomain}/{imageUrl}".ReplaceDuplicateUrlChar();
        }


        private (AGGLiveTokenTempModel TokenResult, int OneToManyCustomizedSettingID) GetLastBackOfficeToken(int vendorConfigID)
        {
            var targetType = OneToManyCustomizedSettingTypeEnum.AGGLive_EachVendorConfigBackOfficeTokenKeep;

            var querySetting = OneToManyCustomizedSettingRepository.GetByFilter(new QueryOneToManyCustomizedSettingRequestViewModel
            {
                Types = targetType.ToInteger().ToListCollection()
            })
                .FirstOrDefault();

            if (querySetting == null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"{targetType.ToString()}"));

            var queryLastToken = [REDACTED];

            if (queryLastToken != null && !string.IsNullOrWhiteSpace(queryLastToken.Token))
                return (queryLastToken, querySetting.OneToManyCustomizedSettingID);

            var tokenResult = AGGLiveIntegrationService.LoginToBackOfficeAPI(vendorConfigID);

            if (string.IsNullOrWhiteSpace(tokenResult.Token))
            {
                throw new TerproException(ApiErrorCode.SystemMaintain);
            }

            string subCategory = vendorConfigID.ToString();

            var jsonModel = new AGGLiveTokenTempModel
            {
                Token = [REDACTED],
                OperatorID = tokenResult.OperatorID,
                ExpiredTime = DateTimeHelper.GetCurrentTime().AddHours(2)
            };

            //刪除再重建資料
            DeleteAGGLiveTempTokenValue(vendorConfigID, querySetting.OneToManyCustomizedSettingID);

            //新增資料
            OneToManyCustomizedSettingItemRepository.Create(new OneToManyCustomizedSettingItemEntity
            {
                SettingKey = 0,
                SettingValue = jsonModel.ConvertToJsonString(),
                OneToManyCustomizedSettingID = querySetting.OneToManyCustomizedSettingID,
                SubCategory = subCategory,
            });

            return (jsonModel, querySetting.OneToManyCustomizedSettingID);
        }


        private void DeleteAGGLiveTempTokenValue(int vendorConfigID, int oneToManyCustomizedSettingID)
        {
            string subCategory = vendorConfigID.ToString();

            //刪除再重建資料
            OneToManyCustomizedSettingItemRepository.DeleteByModel(new
            {
                OneToManyCustomizedSettingID = oneToManyCustomizedSettingID,
                SubCategory = subCategory
            });
        }


        private AGGLiveTokenTempModel GetLastTokenByDB(int vendorConfigID)
        {
            var queryLastToken = [REDACTED] QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.AGGLive_EachVendorConfigBackOfficeTokenKeep.ToInteger(),
                SubCategory = vendorConfigID.ToString()
            })
                .FirstOrDefault();

            if (queryLastToken == null)
                return null;

            if (string.IsNullOrWhiteSpace(queryLastToken.SettingValue))
                return null;

            var parseJson = queryLastToken.SettingValue.TryJsonConvertToModel<AGGLiveTokenTempModel>();

            if (parseJson == null)
                return null;

            if (parseJson.ExpiredTime <= DateTimeHelper.GetCurrentTime())
                return null;

            return parseJson;
        }

        private PublicApiBetRecordReplayInfoViewModel GenerateFakeData(string imageDomain)
        {
            var random = new List<AGGLiveBackOfficeRecordDetailApiBetList>
                    {
                        new AGGLiveBackOfficeRecordDetailApiBetList
                        {
                            bet_type = "Banker",
                            bet_amount = 10,
                            win_amount = 5,
                            tax_amount = 0
                        },
                        new AGGLiveBackOfficeRecordDetailApiBetList
                        {
                            is_image_display = true,
                            bet_type_images = new List<string>
                            {
                                ConcateImageAndImageDomain(imageDomain , "poker_d03.png"),
                                ConcateImageAndImageDomain(imageDomain , "poker_s12.png"),
                            },
                            bet_amount = 20,
                            win_amount = 0,
                            tax_amount = 0
                        }
                    };


            var randomFirst = random.OrderByRandom().FirstOrDefault();

            return new PublicApiBetRecordReplayInfoViewModel
            {
                BasicInfo = new BetRecordBasicInfo
                {
                    VideoUrl = "https://example.invalid/vodedge/_definst_/gcsorigin/vod-storage01/202205200137_666BAC02_main.mp4/playlist.m3u8",
                    BetRecordNoPGS = "(假資料)25J11d583a8d0d9f40bb8390a20c700c",
                    GameName = "(假資料)百家樂",
                    PlayerName = "(假資料)test001",
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    BetAmount = 10,
                    PayoutAmount = 20,
                },
                IsFakeDate = true,
                BetList = new List<AGGLiveBackOfficeRecordDetailApiBetList>
                    {
                        randomFirst
                    },

                SettleResultImageList = new List<RoundInfoItemModel>
                    {
                        new RoundInfoItemModel
                        {
                            Category = "Banker Card",
                            Images = new List<string>
                            {
                                $"{imageDomain}/poker_h07.png",
                                $"{imageDomain}/poker_c06.png",
                                $"{imageDomain}/poker_d03.png",
                            }
                        },

                        new RoundInfoItemModel
                        {
                            Category = "Player Card",
                            Images = new List<string>
                            {
                                $"{imageDomain}/poker_h13.png",
                                $"{imageDomain}/poker_s12.png",
                                $"{imageDomain}/poker_d08.png",
                            }
                        },
                    }
            };
        }

    }

}
