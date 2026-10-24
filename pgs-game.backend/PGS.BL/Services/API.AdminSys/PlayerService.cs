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
using game.Entity.Game;
using target.Utility;
using Lck.Utility.Extensions.Helper;
using target.Utility.Extensions.Web;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public partial class PlayerService : BaseAdminSysService, IPlayerService
    {
        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PlayerGameSummaryRepository PlayerGameSummaryRepository { get; set; }

        public PlayerTotalSummaryRepository PlayerTotalSummaryRepository { get; set; }

        public PlayerCurrentServerRepository PlayerCurrentServerRepository { get; set; }

        public ServerListRepository ServerListRepository { get; set; }

        public ManualDepositQueueRepository ManualDepositQueueRepository { get; set; }

        public PlayerCustomizedSettingRepository PlayerCustomizedSettingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public UserOperateRecordRepository UserOperateRecordRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public GameVendorTransferRecordRepository GameVendorTransferRecordRepository { get; set; }

        public PlayerRTPConfigRepository PlayerRTPConfigRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public PlayerGameDataRepository PlayerGameDataRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public LobbyTokenLogRepository LobbyTokenLogRepository { get; set; }

        public IPlayerCreateRefService PlayerCreateRefService { get; set; }

        public IPlayerTokenLogoutRefService PlayerTokenLogoutRefService { get; set; }


        public PagedList<PlayerInfoViewModel> GetByPagingFilter(int userID, QueryPlayerApiRequestParameterModel filter)
        {
            filter.RuleFor(x => x.PlayerID).Rule_IsValidPlayerID();
            filter.RuleFor(x => x.PlayerName).Rule_MaxLengthOK(50);
            filter.RuleFor(x => x.PlayerNameFuzzyQuery).Rule_MaxLengthOK(50);

            filter.PlayerName = filter.PlayerName.ReplaceNoneAccountFormatText();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter.PlayerIDs = filter.PlayerID.ToIntegerListCollection();
            filter.StatusValues = filter.Status.ToIntegerListCollection();

            var emptyResult = new PagedList<PlayerInfoViewModel>();
            emptyResult.Items = new List<PlayerInfoViewModel>();

            //馬來代理低能太多了 防呆訊息要加強
            if (!currentUser.IsSuperAdminOrg && filter.PlayerID.HasValue)
            {
                ShowHintErrorMessageWhenSearchByPlayerID(userID, filter.PlayerID);
            }

            if (!currentUser.IsSuperAdminOrg && !SettingHelper.IsMSTest)
            {
                if (filter.PlayerID.IsNullOrZeroOrMinus() && string.IsNullOrWhiteSpace(filter.PlayerName) && string.IsNullOrWhiteSpace(filter.PlayerNameFuzzyQuery))
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_PleaseFilterAtLeastOnColumn, new ErrorDetailModel("PlayerAccount／PlayerID"));
                }             
            }

            if (filter.MerchantID.HasValue)
            {
                CheckViewMerchantPermission(userID, filter.MerchantID.Value);
                filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            }
            else
            {
                if (!currentUser.IsSuperAdminOrg)
                {
                    filter.MerchantIDs = new List<int>();
                    filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
                }
            }

            if (filter.ExtraFilterType.NotNullNotZeroNotMinus())
            {
                if (filter.ExtraFilterType.Value == AdminSysExtraFilterEnum.Player_FilterAmountNotZero.ToInteger())
                {
                    filter.WalletAmountLargeThan = 0;
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.PlayerNameFuzzyQuery))
            {
                //不篩選merchantID 資料量會太大
                if (SettingHelper.IsProduction || SettingHelper.IsProductionDbConnection)
                {
                    if(currentUser.AvailableMerchantIDs.CountOrZero() >= 2 && filter.MerchantIDs.IsEmptyOrNull())
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_MustFilterMerchantWhenKeywordSearch);
                    }
                }
            }

            if (filter.PlayerName.IsNotEmptyTextNotNull())
            {
                //超級組織支援多重帳號搜尋
                if (currentUser.IsSuperAdminOrg)
                {
                    List<string> playerNameList = AppHelper.AnalyzeTextFromRawTextBySpaceChar(filter.PlayerName);

                    if (playerNameList.IsNotEmptyNull() && playerNameList.Count() >= 2)
                    {
                        filter.PlayerNames = playerNameList;
                        filter.PlayerName = string.Empty;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.VendorSessionTokenMatch) && filter.VendorSessionTokenMatch.Length > 8)
            {
                bool isGuidFormat = filter.VendorSessionTokenMatch.IsGuid();

                filter.PlayerIDs = filter.PlayerIDs ?? new List<int>();

                var q1 = VendorSessionTokenRepository.GetByToken(filter.VendorSessionTokenMatch);

                var q2 = SingleWalletMerchantSessionTokenRepository.GetByFilter(new QuerySingleWalletMerchantSessionTokenRequestViewModel
                {
                    SessionTokenPGS = filter.VendorSessionTokenMatch
                }).FirstOrDefault();

                var q3 = SingleWalletMerchantSessionTokenRepository.GetByFilter(new QuerySingleWalletMerchantSessionTokenRequestViewModel
                {
                    SessionTokenRaw = filter.VendorSessionTokenMatch
                });

                if (isGuidFormat)
                {
                    var q4 = LobbyTokenLogRepository.GetByFilter(new QueryLobbyTokenLogRequestViewModel
                    {
                        IdentityGuid = Guid.Parse(filter.VendorSessionTokenMatch)
                    })
                        .FirstOrDefault();

                    if (q4 != null)
                    {
                        filter.PlayerIDs.Add(q4.PlayerID);
                    }
                }

                if (q1 != null)
                {
                    filter.PlayerIDs.Add(q1.PlayerID);
                }

                if (q2 != null)
                {
                    filter.PlayerIDs.Add(q2.PlayerID);
                }

                if (q3.IsNotEmptyNull())
                {
                    filter.PlayerIDs.AddRange(q3.Select(x => x.PlayerID).Distinct().ToList());
                }

                if (filter.PlayerIDs.IsEmptyOrNull())
                {
                    return emptyResult;
                }
            }

            var query = PlayerRepository.GetPagingByFilter(filter);

            var paging = ConvertToNewTypePagingObject<PlayerEntity, PlayerInfoViewModel>(query);

            paging.Items = query.Items
                .Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();


            SetPlayerWalletAmount(paging.Items);
            return paging;
        }


        public void CreateInernalTestAccount(int userID, CreateInternalTestAccountRequestModel model)
        {
            model.RuleFor(z => z.PlayerName).Rule_NotEmptyText().Rule_IsValidAccountFormat();
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            CheckViewMerchantPermission(userID, model.MerchantID);

            int playerID = PlayerCreateRefService.CreatePlayerForMerchantRequest(new PlayerCreateParameterModel
            {
                Currency = queryMerchant.Currency,
                MerchantID = queryMerchant.MerchantID,
                PlayerName = model.PlayerName
            });

            if (playerID <= 0)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail, new ErrorDetailModel($"playerID: {playerID}，可能被標記禁止用vendorAPI創帳號"));

            if (model.IsAddToInternalTestAccount)
            {
                PlayerTagMappingRepository.Create(new PlayerTagMappingEntity
                {
                    PlayerID = playerID,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    TagID = SystemTagEnum.Player_FrequentUseTestAccount.ToInteger(),
                    CreateBy = ""
                });

                PlayerTagMappingRepository.Create(new PlayerTagMappingEntity
                {
                    PlayerID = playerID,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    TagID = SystemTagEnum.Player_AllowSkipMaintain.ToInteger(),
                    CreateBy = ""
                });
            }

            return;
        }


        public PlayerManualDepositResultViewModel InternalTestAccountWalletDepositAmount(int userID, InternalTestAccountPlusWalletRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.ConfirmMerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.ConfirmPlayerName).Rule_NotEmptyText();

            //ManualDepositPlayerWallet 裡面有檢查view player permission

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            if (queryPlayer.MerchantID != model.ConfirmMerchantID)
                throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch, new ErrorDetailModel("merchantID"));

            if (queryPlayer.PlayerName != model.ConfirmPlayerName.Trim())
                throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch, new ErrorDetailModel("PlayerName"));

            if (!SettingHelper.IsDev)
            {
                var matchMerchant = MerchantRepository.GetById(queryPlayer.MerchantID);
                string merchantName = matchMerchant?.Name;
                string msg = $"Player: {queryPlayer.PlayerName} ({queryPlayer.PlayerID})｜{merchantName}({queryPlayer.MerchantID}｜{matchMerchant.Currency}) ," +
                             $" 手動加值金額: `{model.Amount}` , 操作人:{userID} {currentUser.UserName} , 參考單號:{model.ReferenceNo} ";

                SendMessageToMultipleChannel(msg);
            }

            var result = ManualDepositPlayerWallet(userID, new PlayerManualDepositRequestModel
            {
                Amount = model.Amount,
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                ReferenceNo = model.ReferenceNo,
                Memo = model.Memo,
            });

            return result;
        }

        public void SendMessageToMultipleChannel(string msg)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_error_adminsys,
                Message = msg
            });

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_common_error,
            });
        }


        /// <summary>
        /// (內部使用) 透過 ManualDepositQueue 這張表批次幫玩家補錢
        /// </summary>
        public void BatchManualDepositPlayerWalletByQueueTable(int userID, BatchPlayerManualDepositRequestModel model)
        {
            model.RuleFor(z => z.QueueID).Rule_NotZeroNotMinus();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
                throw new TerproException(ApiErrorCode.Forbidden_PagePermissionNotAllow);

            var queryQueue = ManualDepositQueueRepository.GetByFilter(new QueryManualDepositQueueRequestViewModel
            {
                QueueID = model.QueueID
            });

            if (queryQueue.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundData, "查無此QueueID");
            }

            var typeText = MultiLanguageTextHelper.GetEnumsCultureText((TransactionTypeEnum)model.TransactionType);

            long ts = DateTimeHelper.GetCurrentTimeTimestamp();

            string msg = $"預計幫{queryQueue.Count()}位玩家補錢 (可查詢金幣紀錄 type={model.TransactionType},{typeText}) , 完成後queueID會修改為 {ts}";
            SendMessageToMultipleChannel(msg);

            foreach (var item in queryQueue)
            {
                try
                {
                    var queryMyPlayer = PlayerRepository.GetBriefByID(item.PlayerID);
                    ManualDepositPlayerWallet(userID, new PlayerManualDepositRequestModel
                    {
                        Amount = item.Amount,
                        PlayerID = item.PlayerID,
                        PlayerName = queryMyPlayer.PlayerName,
                        Memo = model.Memo,

                        //因為做法是每個player groupBy起來 (假設有多張單...只會加總後一次性退款) 無法記錄單號
                        //ReferenceNo = 
                        TransactionType = model.TransactionType,
                    });
                }
                catch (Exception ex)
                {
                    string errorMsg = $"{item.PlayerID} 加錢失敗, {ex.Message}";
                    SendMessageToMultipleChannel(errorMsg);
                }
            }

            ManualDepositQueueRepository.UpdateQueueID(model.QueueID, ts);
        }


        public PlayerManualDepositResultViewModel ManualDepositPlayerWallet(int userID, PlayerManualDepositRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PlayerName).Rule_NotEmptyText();
            model.RuleFor(z => z.Amount).Rule_NotZero();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
                throw new TerproException(ApiErrorCode.Forbidden_PagePermissionNotAllow);

            var queryPlayer = PlayerRepository.GetById(model.PlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            if (queryPlayer.PlayerName != model.PlayerName)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            CheckViewPlayerPermission(userID, queryPlayer.PlayerID);

            string ip = IPHelper.GetIP();

            var queryMerchant = MerchantRepository.GetById(queryPlayer.MerchantID);

            if (queryMerchant.WalletMode != MerchantWalletModeEnum.TransferMode.ToInteger())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "手動補錢，僅針對轉帳模式玩家");

            // Step 1. 更新錢包
            var transactionType = TransactionTypeEnum.ManualDepositWalletAmount;
            if (model.TransactionType.HasValue)
            {
                transactionType = (TransactionTypeEnum)model.TransactionType;
            }

            var spResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPlayer.PlayerID, 0, model.Amount, transactionType, model.ReferenceNo ?? "", ip);

            UserOperateRecordEntity operateLog = InstanceOperateLog(userID, UserOperateTypeEnum.Player_ManualDepositWalletAmount, queryPlayer.PlayerID);
            operateLog.MerchantID = queryMerchant.MerchantID;
            operateLog.Memo = "update amount:" + model.Amount + ". " + model.Memo;

            string memoText = $"手動加值錢包: {model.Amount}";

            // Step 2. 玩家事件紀錄
            PlayerTagMappingRepository.CreatePlayerBehaviorEvent(queryPlayer.PlayerID, memoText);

            // Step 3. 寫操作紀錄
            UserOperateRecordRepository.Create(operateLog);

            return new PlayerManualDepositResultViewModel
            {
                AfterAmount = spResult.AfterAmount,
                BeforeAmount = spResult.BeforeAmount
            };
        }

        public int GetPlayerIdByPlayerNameOrID(int userID, PlayerDetailQueryRequest filter)
        {
            filter.RuleFor(x => x.PlayerNameOrID).Rule_NotEmptyText();

            string playerNameOrID = filter.PlayerNameOrID.ReplaceNoneAccountFormatText();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (string.IsNullOrWhiteSpace(playerNameOrID))
                throw new TerproException(ApiErrorCode.InvalidParameter);

            //非數字
            if (!playerNameOrID.IsNumbericOfPositiveInt32())
            {
                return FindPlayerByPlayerName(userID, filter);
            }

            long parseToNumber = Int64.Parse(playerNameOrID);

            if (RegexRuleHelper.IsValidPlayerID(parseToNumber))
            {
                //優先從playerID去查
                var query = PlayerRepository.GetById((int)parseToNumber);

                if (query != null && IsViewMerchantPermission(currentUser, query.MerchantID))
                    return query.PlayerID;

                //若查不到 改用playerName查
                return FindPlayerByPlayerName(userID, filter);
            }
            else
            {
                return FindPlayerByPlayerName(userID, filter);
            }
        }

        public PlayerW88TransferFailCheckResultViewModel CheckPlayerAnyW88TransferFail(int userID, LazyCommonRequestModel filter)
        {
            filter.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetByIdOrThrowException(filter.PlayerID);

            var queryLobbyTransferRecord = GameVendorTransferRecordRepository.GetPagingByFilter(new QueryGameVendorTransferRecordRequestViewModel
            {
                PageSize = 50,
                PlayerIDs = filter.PlayerID.ToListCollection()
            });

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            var items = queryLobbyTransferRecord.Items
                .OrderByDescending(x => x.CreateTime)
                .Select(x =>
                {
                    var convert = x.ReturnInjecteObject<LobbyTransferRecordViewModel>();
                    convert.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((TransactionTypeEnum)x.Type);
                    return convert;
                })
                .ToList();

            bool isValidateOK = true;

            if (items.Any(x => string.IsNullOrWhiteSpace(x.PlatformOrderNo)))
            {
                isValidateOK = false;
            }

            return new PlayerW88TransferFailCheckResultViewModel
            {
                IsValidateOK = isValidateOK,
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                MerchantID = queryPlayer.MerchantID,
                Currency = queryMerchant.Currency,
                MerchantName = queryMerchant.Name,
                TransferRecords = items,
            };
        }



        public PlayerInfoSummaryViewModel GetPlayerSummaryInfo(int userID, int playerID)
        {
            var queryPlayer = PlayerRepository.GetById(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            CheckViewPlayerPermission(userID, playerID);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var convert = queryPlayer.ReturnInjecteObject<PlayerInfoSummaryViewModel>();
            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            if (queryMerchant != null)
            {
                convert.MerchantName = queryMerchant.Name;
                convert.WalletMode = queryMerchant.WalletMode;
                convert.Currency = queryMerchant.Currency;
            }

            var queryAGGFishGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = queryMerchant.MerchantID.ToListCollection(),
                GameVendorIDs = GameVendorTypeEnum.AGGFish.ToInteger().ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger()
            });

            convert.IsFishWalletVisible = queryAGGFishGames.IsNotEmptyNull();
            convert.LastLoginTime = queryPlayer.LastLoginTime.HasValue ? queryPlayer.LastLoginTime.Value : queryPlayer.CreateTime;
            var queryTotalSummary = PlayerTotalSummaryRepository.GetById(playerID);

            var queryGameSummary = PlayerGameSummaryRepository.GetByFilter(new QueryPlayerGameSummaryRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection()
            });

            if(HasPermission_OnlyVisibleIDGGames(userID))
            {
                queryGameSummary = queryGameSummary
                    .Where(x => AppHelper.IsIDGVisibleGames(x.GameID))
                    .ToList();
            }

            convert.TotalSummary = new PlayerTotalSummaryViewModel
            {
                TotalLoginTimes = queryTotalSummary?.SumLoginTimes ?? 0
            };

            var queryRTPSetting = PlayerRTPConfigRepository.GetByFilter(new QueryPlayerRTPConfigRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection()
            });

            var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                MerchantIDs = queryPlayer.MerchantID.ToListCollection(),
            });

            var queryGameVendorAssignInfo = VendorConfigSettingRepository.GetAllGameVendorSettingByMerchat(queryPlayer.MerchantID, VendorConfigSettingEnum.MainAssignVendorID);

            bool isCS_role = HasCustomerServicePermission(userID);

            List<int> myGameIDs = queryGameSummary.Select(z => z.GameID).ToList();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(myGameIDs);

            Dictionary<int, string> gameNameDicCN = new Dictionary<int, string>();
            Dictionary<int, string> gameNameDicEN = new Dictionary<int, string>();

            if (isCS_role)
            {
                gameNameDicCN = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(myGameIDs, LanguageEnum.zh_cn);
                gameNameDicEN = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(myGameIDs, LanguageEnum.en_us);
            }

            var lastSession = VendorSessionTokenRepository.GetPlayerLatestToken(playerID);

            var now = DateTimeHelper.GetCurrentTime();

            string lastTokenText = "";

            if (lastSession != null)
            {
                string timeText = lastSession.CreateTime.AddHours(8).ToString("MM/dd HH:mm");
                string diffText = DateTimeHelper.GetDiffDisplayTime((int)(now - lastSession.CreateTime).TotalMinutes, false, "前", LanguageEnum.zh_cn);

                string myGameName = gameNameDic.GetDictionaryOrDefaultValue(lastSession.GameID);
                lastTokenText = $"最後登入：{timeText} ({diffText}、{myGameName})";
            }

            convert.PlayerGameSummaries = queryGameSummary.Select(x =>
                {
                    var convertTo = x.ReturnInjecteObject<PlayerGameSummaryViewModel>();

                    //客服操作的就顯示中英文遊戲名稱
                    if (isCS_role)
                    {
                        convertTo.GameName = gameNameDicCN.GetDictionaryOrDefaultValue(x.GameID) + " (" + gameNameDicEN.GetDictionaryOrDefaultValue(x.GameID) + ")";
                    }
                    else
                    {
                        convertTo.GameName = gameNameDic.GetDictionaryOrDefaultValue(x.GameID);
                    }

                    convertTo.RTP = CommonLogicHelper.CalculateRTP_Percentage(x.BetAmount, x.PayoutAmount);
                    convertTo.TotalRound = x.WinCount + x.LoseCount + x.DrawCount;
                    convertTo.LastBetTime = x.UpdateTime.AddHours(-1 * 2); //往前推一點時間才會查到比較多注單
                    convertTo.LastBetTimeTS = convertTo.LastBetTime.ConvertToUnixTimestamp();

                    int gameVendor = AppHelper.GetGameVendorByGameID(x.GameID);

                    convertTo.GameVendorType = gameVendor;
                    convertTo.GameVendorTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameVendorTypeEnum)gameVendor);

                    var matchVendorConfig = queryVendorConfig.FirstOrDefault(z => z.VendorID == gameVendor);

                    var gameVendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(gameVendor);
                    convertTo.IsSupportRTPConfig = gameVendorDef.IsSupportRTPConfig;

                    if (matchVendorConfig != null)
                    {
                        convertTo.GameVendorConfigID = matchVendorConfig.VendorID;
                        convertTo.GameVendorConfigName = matchVendorConfig.Name;

                        var matchAssignID = queryGameVendorAssignInfo.Where(x => x.VendorConfigID == matchVendorConfig.VendorConfigID).FirstOrDefault();

                        if (matchAssignID != null)
                        {
                            string extraText = "";

                            if (matchAssignID.VendorID == (int)GameVendorTypeEnum.SlotMania)
                            {
                                extraText = " (AppID)";
                            }

                            string extraInfo_walletMode = AppHelper.IsSingleWalletMode(queryMerchant.WalletMode) ? "(*SW)" : "(*tf mode)";

                            string myLastTokenText = "";

                            if (lastSession != null && AppHelper.GetGameVendorByGameID(lastSession.GameID) == matchAssignID.VendorID)
                            {
                                myLastTokenText = lastTokenText;
                            }

                            var IMChannleHint = "";
                            var vendorConfigExtraInfo = "";

                            if (!isCS_role)
                            {
                                vendorConfigExtraInfo = $"服务器名称：{matchVendorConfig.Name} ※{matchVendorConfig.VendorConfigID}";
                            }

                            if (isCS_role && !string.IsNullOrWhiteSpace(gameVendorDef.IMChannelName))
                            {
                                IMChannleHint = "請轉貼到 ⭐" + gameVendorDef.IMChannelName + Environment.NewLine +
                                                "請轉貼到 ⭐" + gameVendorDef.IMChannelName + Environment.NewLine + Environment.NewLine;
                            }

                            convertTo.GameVendorAssignInfoForCopy = AppHelper.GenerateCopyInfoAndForwardToGameVendor(
                                (GameVendorTypeEnum)gameVendor,
                                isCS_role,
                                matchAssignID.SettingValue,
                                queryMerchant.Currency,
                                queryPlayer.PlayerID,
                                new GameVendorCopyInfoExtraTextModel
                                {
                                    LastSessionTokenQuery = lastSession,
                                    VendorConfigInfo = vendorConfigExtraInfo,
                                    WalletModeInfo = extraInfo_walletMode,
                                    LastTokenText = myLastTokenText,
                                });
                        }
                    }

                    var matchRTPSetting = queryRTPSetting.FirstOrDefault(z => z.GameID == x.GameID);

                    if (matchRTPSetting != null)
                    {
                        convertTo.IsSupportRTPConfig = true;
                        convertTo.RTPConfigValue = matchRTPSetting.RTP;
                    }

                    return convertTo;
                })
                .ToList();

            convert.Memo = PlayerCustomizedSettingRepository.GetByUniqueKey(playerID, PlayerCustomizedSettingKeyEnum.Player_Memo)?.SettingValue;
            convert.PlayerTypeText = MultiLanguageTextHelper.GetEnumsCultureText((PlayerTypeEnum)queryPlayer.PlayerType);
            convert.WalletAmount = PlayerWalletRepository.GetPlayerWalletByType(playerID, WalletTypeEnum.GameWallet, isLockQuery: true).Amount;
            convert.PlayerGameSummaryRowTotal = convert.PlayerGameSummaries.CalculateTotalSumMergeToOneObject();
            convert.PlayerGameSummaryRowTotal.RTP = CommonLogicHelper.CalculateRTP_Percentage
                (
                    convert.PlayerGameSummaryRowTotal.BetAmount,
                    convert.PlayerGameSummaryRowTotal.PayoutAmount
                );

            var queryMyCurrentServer = PlayerCurrentServerRepository.GetByPlayerID(queryPlayer.PlayerID);

            if (queryMyCurrentServer != null)
            {
                var queryServer = ServerListRepository.GetBriefByCache(queryMyCurrentServer.ServerID);
                convert.PlayerCurrentGameGame = DbMultiLanguageTextHelper.GetGameDisplayName(queryServer.GameID);
                convert.PlayerCurrentServerID = queryMyCurrentServer.ServerID;
            }

            convert.IsLobbyTransferEnable = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.Common_LobbyEnablePlatformTransfer).SettingValueOfBoolean;
            convert.IsTestAccountOrMerchant = IsInternalTestAccountPlayerOrMerchantPlayer(queryPlayer.PlayerID);

            var queryTags = PlayerTagMappingRepository.GetByFilter(new QueryPlayerTagMappingRequestViewModel
            {
                PlayerIDs = queryPlayer.PlayerID.ToListCollection()
            });

            convert.PlayerRecentlyBetSummary = GetRecentDaysSummary(playerID, queryPlayer.MerchantID, currentUser.TimeZone);

            if (convert.PlayerRecentlyBetSummary.IsNotEmptyNull())
            {
                var lastBetDate = convert.PlayerRecentlyBetSummary
                    .Where(x => x.BetAmount > 0 && x.TotalRound > 0)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefault();

                if (lastBetDate != null)
                {
                    convert.LastBetDateTimestamp = lastBetDate.Date.AddHours(-8).ConvertToUnixTimestamp();
                }
            }

            convert.TransferConvertRate = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer).SettingValueOfDecimal;
            convert.PlayerTags = queryTags
                .Where(x => x.SettingValue.IsEmptyOrNull()) //有填寫value的不顯示
                .Select(z => new PlayerTagsViewModels
                {
                    TagID = z.TagID,
                    TagName = MultiLanguageTextHelper.GetEnumsCultureText((SystemTagEnum)z.TagID)
                })
                .ToList();

            convert.RiskScore = PlayerTagMappingRepository.GetPlayerRiskScore(queryPlayer.PlayerID);
            convert.PlayerBehaviorEvents = queryTags
                .Where(x => x.TagID == (int)SystemTagEnum.Player_PlayerBehaviorEvent)
                .OrderByDescending(x => x.CreateTime)
                .Take(10)
                .Select(z => new PlayerBehaviorViewModels
                {
                    Memo = z.SettingValue,
                    CreateTime = z.CreateTime
                })
                .ToList();

            if(lastSession != null)
            {
                var gameVendor = AppHelper.GetGameVendorByGameID(lastSession.GameID);

                var matchVendorConfig = queryVendorConfig.FirstOrDefault(z => z.VendorID == (int)gameVendor);

                if(matchVendorConfig != null)
                {
                    var matchAssignID = queryGameVendorAssignInfo
                        .Where(x => x.VendorConfigID == matchVendorConfig.VendorConfigID)
                        .FirstOrDefault();

                    //假如客服找不到遊戲 以最後一次進的遊戲商為主
                    string copyText = AppHelper.GenerateCopyInfoAndForwardToGameVendor(
                        (GameVendorTypeEnum)gameVendor,
                        isCS_role,
                        matchAssignID.SettingValue,
                        queryMerchant.Currency,
                        queryPlayer.PlayerID,
                        new GameVendorCopyInfoExtraTextModel
                        {
                            LastSessionTokenQuery = lastSession,
                            VendorConfigInfo = "",
                            WalletModeInfo = "",
                            LastTokenText = lastTokenText,
                        });

                    convert.CopyTextForCS = copyText;
                }
            }
            else
            {
                SetCopyTextForCS(convert);
            }

            return convert;
        }



        private void SetCopyTextForCS(PlayerInfoSummaryViewModel viewModel)
        {
            var now = DateTimeHelper.GetCurrentTime();

            var minutesDiff = ((int)(now - viewModel.CreateTime).TotalMinutes).ToAbs();

            var daysDiff = DateTimeHelper.GetDiffDisplayTime(minutesDiff, false, "前");

            viewModel.CopyTextForCS = $@"
玩家ID：{viewModel.PlayerID} (可能对应贵司的玩家帐号或昵称)
Account：{viewModel.PlayerName}
商戶：{viewModel.MerchantName} ({viewModel.MerchantID})
貨幣：{viewModel.Currency}
錢包模式：{(viewModel.WalletMode == 1 ? "Transfer" : "Seamless")}
創建時間：{DateTimeHelper.ToYYYY_MM_DD_HH_MM_SS(viewModel.CreateTime, 8)} ({daysDiff}){Environment.NewLine}";

            if (viewModel.RiskScore > 0)
            {
                viewModel.CopyTextForCS += $"RiskScore：{viewModel.RiskScore}{Environment.NewLine}";
            }

            if (viewModel.PlayerTags != null && viewModel.PlayerTags.Count() > 0)
            {
                foreach (var item in viewModel.PlayerTags)
                {
                    viewModel.CopyTextForCS += "Tag：" + item.TagName + Environment.NewLine;
                }
            }

            viewModel.CopyTextForCS += Environment.NewLine + Environment.NewLine;

            var vendorInfo_AGG = viewModel.PlayerGameSummaries.Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == (int)GameVendorTypeEnum.AGG)
                .FirstOrDefault()
                ?.GameVendorAssignInfoForCopy;

            var vendorInfo_AT = viewModel.PlayerGameSummaries.Where(x => AppHelper.GetGameVendorByGameID(x.GameID) == (int)GameVendorTypeEnum.AT)
                .FirstOrDefault()
                ?.GameVendorAssignInfoForCopy;

            if (!string.IsNullOrWhiteSpace(vendorInfo_AGG))
            {
                viewModel.CopyTextForCS += "# AGG資訊" + Environment.NewLine;
                viewModel.CopyTextForCS += $"服務器：{vendorInfo_AGG}" + Environment.NewLine + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(vendorInfo_AT))
            {
                viewModel.CopyTextForCS += "# AT資訊" + Environment.NewLine;
                viewModel.CopyTextForCS += $"服務器：{vendorInfo_AT}" + Environment.NewLine + Environment.NewLine;
            }
        }


        public List<PlayerDailyGameSummaryViewModel> GetRecentDaysSummary(int playerID, int merchantID, double timeZone)
        {
            var today = DateTimeHelper.GetCurrentTime(timeZone);
            var before7Days = today.To_00_00_00().AddDays(-14);

            DateTime beginUTC = before7Days.AddHours(timeZone * -1);
            DateTime endUTC = today.To_23_59_59().AddHours(timeZone * -1);

            var querySummary = PlayerGameDataRepository.GetEachDatePlayerGameSummary(beginUTC, endUTC, merchantID, playerID, timeZone);

            List<DateTime> dateList = DateTimeHelper.GetDateListBetweenTwoDate(beginUTC.AddHours(timeZone), endUTC.AddHours(timeZone));

            DateTime lastBetDay = dateList.OrderByDescending(z => z).FirstOrDefault();

            //以最後一個投注日為準向後顯示。如果近幾日沒投注則不顯示該日數據
            if (querySummary.IsNotEmptyNull())
            {
                var firstMatch = querySummary.Where(z => z.BetAmount > 0).OrderByDescending(z => z.Date).FirstOrDefault();

                if (firstMatch != null)
                {
                    lastBetDay = firstMatch.Date;
                }
            }

            return dateList.Select(date =>
            {
                var matchSummary = querySummary.FirstOrDefault(z => z.Date == date);
                var convert = new PlayerDailyGameSummaryViewModel();

                if (matchSummary != null)
                {
                    convert = matchSummary.ReturnInjecteObject<PlayerDailyGameSummaryViewModel>();
                    convert.RTP = CommonLogicHelper.CalculateRTP_Percentage(matchSummary.BetAmount, matchSummary.PayoutAmount);
                }
                else
                {
                    convert.Date = date;
                }
                convert.DateText = convert.Date.ToString("yyyy-MM-dd");
                return convert;
            })
            .Where(z => z.Date <= lastBetDay)
            .OrderByDescending(z => z.Date)
            .Take(10)
            .ToList();
        }


        public void UpdatePlayerStatus(int userID, PlayerUpdateStatusRequestModel model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Status).Rule_NotMinus();

            UpdateMultiplePlayerStatus(userID, new PlayerMultipleUpdateStatusRequestModel
            {
                PlayerIDs = model.PlayerID.ToListCollection(),
                Status = model.Status,
                Memo = model.Memo
            });

        }



        public void UpdateMultiplePlayerStatus(int userID, PlayerMultipleUpdateStatusRequestModel model)
        {
            if (model.PlayerIDs.IsEmptyOrNull())
                return;

            int targetStatus = model.Status;

            var queryPlayers = PlayerRepository.GetByIds(model.PlayerIDs);

            if (queryPlayers.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            foreach (var player in queryPlayers)
            {
                CheckViewPlayerPermission(userID, player.PlayerID);
            }

            foreach (var eachPlayer in queryPlayers)
            {
                //如果有權限不符合的(update別家商戶) 跳過不處理
                if (!currentUser.IsSuperAdminOrg &&
                    !currentUser.AvailableMerchantIDs.Contains(eachPlayer.MerchantID))
                    continue;

                var changeLog = new UserOperateChangeLogModel
                {
                    BeforeObject = new { Status = eachPlayer.Status },
                    AfterObject = new { Status = targetStatus }
                };

                eachPlayer.Status = targetStatus;

                //Step 1. 更新狀態
                PlayerRepository.UpdateStatus(eachPlayer.PlayerID, targetStatus);

                //Step 2. 寫異動紀錄
                var operatorType = (targetStatus == PlayerStatusEnum.Enable.ToInteger() ? UserOperateTypeEnum.Player_UnlockStatus : UserOperateTypeEnum.Player_LockStatus);
                UserOperateRecordEntity operateLog = InstanceOperateLog(userID, operatorType, eachPlayer.PlayerID, changeLog);
                operateLog.MerchantID = eachPlayer.MerchantID;
                operateLog.Memo = string.IsNullOrWhiteSpace(model.Memo) ? operateLog.Memo : model.Memo;

                UserOperateRecordRepository.Create(operateLog);

                //Step 3. 玩家事件紀錄
                PlayerTagMappingRepository.CreatePlayerBehaviorEvent(eachPlayer.PlayerID, $"修改狀態:{targetStatus}");

                CacheRemoveHelper.RemoveCache_WhenUpdatePlayer(eachPlayer.PlayerID);
            }
        }

        public void RemovePlayerRoomLock(int userID, int playerID)
        {
            var queryPlayer = PlayerRepository.GetById(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            CheckViewPlayerPermission(userID, playerID);

            var query = PlayerCurrentServerRepository.GetById(playerID);

            if (query != null)
            {
                //Step 1. 刪除資料
                PlayerCurrentServerRepository.Delete(playerID);
            }

            //Step 2. 清除token
            PlayerTokenLogoutRefService.ResetPlayerSessionToken(playerID);

            //Step 3. 寫異動紀錄
            var changeLog = new UserOperateChangeLogModel
            {
                BeforeObject = query,
                AfterObject = null
            };

            UserOperateRecordEntity operateLog = InstanceOperateLog(userID, UserOperateTypeEnum.Player_RoomLockRemove, playerID, changeLog);
            UserOperateRecordRepository.Create(operateLog);

            if (SettingHelper.IsProduction && query != null)
            {
                var queryServer = ServerListRepository.GetById(query.ServerID);
                var queryGame = GameListRepository.GetById(queryServer.GameID);

                string msg = $"已幫玩家: {queryPlayer?.PlayerName} ({query.PlayerID}) 解除卡房。最後進房遊戲:{queryGame.GameName} ({queryGame.GameID})";
                SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            }
        }


        public PlayerAGGWalletBalanceViewModel GetAGGFishWalletBalance(int userID, int playerID)
        {
            var queryPlayer = PlayerRepository.GetById(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            CheckViewPlayerPermission(userID, playerID);

            var aggService = new AGGFishIntegrationService();

            var apiResult = aggService.FetchPlayerFishWalletBalanace(queryPlayer.PlayerID);

            return new PlayerAGGWalletBalanceViewModel
            {
                PlayerID = queryPlayer.PlayerID,
                Balance = apiResult?.result?.balance ?? 0
            };
        }


        public List<CommonBriefInfoListViewModel> GetPlayerBriefInfoByIDs(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(z => z.PlayerIDs).Rule_IsNotEmptyCollection();

            //為了效能因素 最多抓500筆
            var queryPlayers = PlayerRepository.GetByIds(model.PlayerIDs.Take(500).ToList());

            List<int> merchantIDs = queryPlayers.Select(z => z.MerchantID).Distinct().ToList();

            var queryCurrentUser = GetCurrentOwnPermissionByCache(userID);

            //檢查非法查詢權限
            if (!queryCurrentUser.IsSuperAdminOrg)
            {
                foreach (var merchantID in merchantIDs)
                {
                    CheckViewMerchantPermission(userID, merchantID);
                }
            }

            return queryPlayers.Select(z => new CommonBriefInfoListViewModel
            {
                ID = z.PlayerID,
                DisplayName = z.PlayerName,
                ExtraInfo = z.MerchantID.ToString()
            })
                .ToList();
        }

        private int FindPlayerByPlayerName(int userID, PlayerDetailQueryRequest filter)
        {
            filter.PlayerNameOrID = (filter.PlayerNameOrID ?? "").Trim();
            filter.RuleFor(x => x.PlayerNameOrID).Rule_NotEmptyText();
            filter.RuleFor(x => x.PlayerNameOrID).Rule_IsValidAccountFormat();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var sqlFilter = new QueryPlayerRequestViewModel
            {
                PlayerName = filter.PlayerNameOrID.ToLower(),
                MerchantIDs = currentUser.SuperAdminOrAvailableMerchantIDs
            };

            if (currentUser.IsMultipleHierarchyMode)
            {
                //多重代理模式
                sqlFilter.MerchantIDs = new List<int>();
                sqlFilter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.SuperAdminOrAvailableMerchantIDs;
            }
            else if (currentUser.IsHierarchyMerchantMode)
            {
                sqlFilter.FilterMerchantIDByHierarchyMerchant = currentUser.ReferenceCreditMerchant;
                sqlFilter.MerchantIDs = new List<int>();
            }

            //若有指定merchantID
            if (filter.MerchantID.NotNullNotZeroNotMinus())
            {
                if (!currentUser.IsSuperAdminOrg)
                {
                    CheckViewMerchantPermission(userID, filter.MerchantID.Value);
                }

                sqlFilter.MerchantIDs = filter.MerchantID.Value.ToListCollection();
            }

            var query = PlayerRepository.GetByFilter(sqlFilter);

            if (query.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundPlayer);
            }

            if (query.Count() > 1)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID);
            }

            return query.FirstOrDefault().PlayerID;
        }

        private void SetPlayerWalletAmount(List<PlayerInfoViewModel> models)
        {
            if (models.IsEmptyOrNull())
                return;

            List<int> playerIDs = models.Select(x => x.PlayerID).ToList();

            var query = PlayerWalletRepository.GetByFilter(new QueryPlayerWalletRequestViewModel
            {
                IsLockQuery = false,
                PlayerIDs = playerIDs
            });

            foreach (var item in models)
            {
                var match = query.FirstOrDefault(x => x.PlayerID == item.PlayerID);

                if (match != null)
                {
                    item.WalletAmount = match.Amount;
                }
            }
        }



        private PlayerInfoViewModel ConvertToViewModels(PlayerEntity query)
        {
            var convert = query.ReturnInjecteObject<PlayerInfoViewModel>();
            var queryMerchant = MerchantRepository.GetSingleByCache(query.MerchantID);

            if (queryMerchant != null)
            {
                convert.MerchantName = queryMerchant.Name;
                convert.WalletMode = queryMerchant.WalletMode;
                convert.WalletModeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantWalletModeEnum)queryMerchant.WalletMode);
            }

            return convert;
        }


    }
}
