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
using Lck.Utility.Extensions.Helper;
using target.Utility;
using target.Utility.Utility.ThirdParty;
using target.Utility.Utility;
using example.invalid;
using System.Threading;
using System.Text;
using target.Models.ViewModels.Base;
using target.Utility.IOC;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class BackendButtonsService : BaseAdminSysService, IBackendButtonsService
    {
        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public LanguageTextRepository LanguageTextRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public UserTokenRecordRepository UserTokenRecordRepository { get; set; }


        public SystemSPRepository SystemSPRepository = new SystemSPRepository();

        public PlayerRepository PlayerRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }



        public IExcelFileParseService ExcelFileParseService { get; set; }

        public IBetRecordService BetRecordService { get; set; }

        public IGameLaunchService GameLaunchService { get; set; }

        public IPlayerService PlayerInfoService { get; set; }

        public IPlayerTokenLogoutRefService PlayerTokenLogoutService { get; set; }

        public IOneTimesJobService OneTimesJobService { get; set; }

        public ISystemMonitorService SystemMonitorService { get; set; }

        public IAdminSysInternalUtilityService AdminSysInternalUtilityService { get; set; }

        public IAdminSysComplexService AdminSysComplexService { get; set; }


        public BackendDefineButtonViewModel GetDefineButtons(int userID)
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<BackendButtonEnum>();

            var buttons = allEnums.Select(z => new BackendDefineButtonItemViewModel
            {
                InfoDescription = z.GetAttributeFromEnum<ExtraAttributeValue>()?.Value,
                BackendButtonActionID = z.ToInteger(),
                DisplayName = z.GetDescription() //因為是內部用功能，多語系也不重要了
            })
            .ToList();

            if (!HasTargetPermission(userID, "SystemSetting_SystemConfigSetting_Update"))
            {
                buttons = buttons
                    .Where(x => ((BackendButtonEnum)x.BackendButtonActionID).ToString().Contains("QA"))
                    .ToList();
            }

            var now_TW = DateTimeHelper.GetCurrentTime(8);
            string defaultMonthTextBegin = now_TW.ToString("yyyy-MM") + "-01";
            string defaultMonthTextEnd = now_TW.AddDays(1).ToString("yyyy-MM") + "-02";

            foreach (var item in buttons)
            {
                if (item.BackendButtonActionID == BackendButtonEnum.ResetMerchantPlayerSessionToken.ToInteger())
                {
                    item.RequiredInput = new List<string> { "merchantID" };
                }

                if (item.BackendButtonActionID == BackendButtonEnum.RemoveCacheByKeyword.ToInteger() ||
                    item.BackendButtonActionID == BackendButtonEnum.ManualTriggerDepositByQueueID.ToInteger() ||
                    item.BackendButtonActionID == BackendButtonEnum.Input_BetRecordNoPGS_RecoverToDate.ToInteger() ||
                    item.BackendButtonActionID == BackendButtonEnum.GetCacheValueByKey.ToInteger() ||
                    item.BackendButtonActionID == BackendButtonEnum.QA_GenerateTestData.ToInteger())
                {
                    item.RequiredInput = new List<string> { "text" };
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.QA_TriggerOneTimeJob.ToInteger())
                {
                    item.RequiredInput = new List<string> { "textArea" };
                    item.MemoHint = $"不用填值{Environment.NewLine}*⚠️請注意部分排程，需等候3~30分鐘後才會有效果、不要太快觸發。";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_TableSQLGenerate.ToInteger())
                {
                    item.RequiredInput = new List<string> { "targetTable", "textArea" };
                    item.MemoHint = $"填寫GameIDs (或其他table識別編號)";

                    var query = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.AdminSys_GenerateSQLTables);

                    item.MyExtraDropdown = new List<DropdownValueViewModel>();

                    if (query.IsNotEmptyNull())
                    {
                        item.MyExtraDropdown = query
                            .Where(x => !string.IsNullOrWhiteSpace(x.SettingValue))
                            .Where(x => x.SettingValue.CountOrZero() >= 3)
                            .Select(x => new DropdownValueViewModel
                            {
                                Text = x.SettingValue,
                                Value = x.SettingValue,
                            })
                            .ToList();
                    }
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_TriggerSingleWalletValidateTestProcess.ToInteger())
                {
                    item.RequiredInput = new List<string> { "text", "merchantID" };
                    item.MemoHint = "⚠️可以輸入單號 或 玩家帳號";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_MultipleAddPerGameTags_ByTransactionNo.ToInteger())
                {
                    item.RequiredInput = new List<string> { "textArea", "perGameTagID" };
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.QA_SendFreespin.ToInteger())
                {
                    item.RequiredInput = new List<string> { "gameID", "merchantID", "text", "numberText" };
                    item.MemoHint = $"欄位說明：{Environment.NewLine}⚠️文字欄位1 請輸入玩家帳號" + Environment.NewLine + "⚠️文字欄位2 請輸入發送幾次 (1~10)" + Environment.NewLine;
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.Statistic_CalculateAgain_TargetDate.ToInteger())
                {
                    item.RequiredInput = new List<string> { "date" };

                    item.MemoHint = $"--【StatDB】時間可以未來時間" + Environment.NewLine +
                                    $"DECLARE @beginUTC DATETIME = DATEADD(hour , -8 , '{defaultMonthTextBegin} 00:00:00')" + Environment.NewLine +
                                    $"DECLARE @endUTC   DATETIME = DATEADD(hour , -8 , '{defaultMonthTextEnd} 00:00:00')" + Environment.NewLine +
                                    $"USE [PGS_StatReport]" + Environment.NewLine +
                                    $"EXEC PrWeb_Stat_CalculateStatisticSummaryAgain @beginUTC --(StatDB、默認End=24小時後)" + Environment.NewLine +
                                    $"-- 重跑活動Event統計數字" + Environment.NewLine +
                                    $"EXEC [dbo].[PrJob_Stat_MerchantEventSummary_Data] 777 , @beginUTC , @endUTC , 0" + Environment.NewLine +
                                    $"-- 重跑PlayerGameSummary數字 (可能時間較久?)" + Environment.NewLine +
                                    $"EXEC [dbo].[PrJob_Stat_MerchantEventSummary_Data] 777 , @beginUTC , @endUTC , 0" + Environment.NewLine +
                                    $"-- 重跑單一商戶統計數字" + Environment.NewLine +
                                    $"EXEC PrJob_Stat_MerchantGameSummary_SingleMerchant_ReCalculate @beginUTC , @endUTC , 175XXX ";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_CompareLeftAndRightList.ToInteger())
                {
                    item.RequiredInput = new List<string> { "textArea", "textArea2" };
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_AnalyzeTextAndConvertToList.ToInteger())
                {
                    item.RequiredInput = new List<string> { "textArea" };
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_CheckPerGameExist.ToInteger())
                {
                    item.RequiredInput = new List<string> { "textArea" };
                    item.MemoHint = "請輸入批量32碼單號";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_AppendLobbyBackurl.ToInteger())
                {
                    item.RequiredInput = new List<string> { "textArea" };
                    item.MemoHint = "每行一組url";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_ForceCreateAGGHierarchyNodes.ToInteger())
                {
                    item.RequiredInput = new List<string> { "textArea" };
                    item.MemoHint = "輸入多筆商戶IDs";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_RecoverSwCallbackRetryQueue_ByTransactionNo.ToInteger())
                {
                    item.RequiredInput = new List<string> { "textArea" };
                    item.MemoHint = "請輸入32碼注單號，若不存在會嘗試從EazyTemp尋找";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_TestVendorAPI.ToInteger())
                {
                    item.RequiredInput = new List<string> { "text" };
                    item.MemoHint = "請輸入商戶ID (僅限Type=測試)";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.TechTeam_TriggerAGGForceSettle.ToInteger())
                {
                    item.RequiredInput = new List<string> { "text" };
                    item.MemoHint = "請輸入PGS注單號";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.BatchCreateThirdGameVendorGames.ToInteger())
                {
                    item.RequiredInput = new List<string> { "gameVendorType", "textArea" };
                    item.MemoHint = $"Excel格式可選2欄或3欄{Environment.NewLine}(2欄) NameCN｜GameCode  {Environment.NewLine}(3欄) NameCN｜NameEN｜GameCode ";
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.SingleWallet_UseLastTokenToLobbyPage.ToInteger())
                {
                    item.RequiredInput = new List<string> { "playerID" };
                }
                else if (item.BackendButtonActionID == BackendButtonEnum.CheckPlayerMoney.ToInteger())
                {
                    item.RequiredInput = new List<string> { "playerID" };
                }
            }

            return new BackendDefineButtonViewModel
            {
                Items = buttons
            };
        }


        public BackendButtonResultModel ExecuteDefineAction(int userID, BackendDefineButtonActionRequestModel model)
        {
            model.RuleFor(z => z.BackendButtonActionID).Rule_NotZeroNotMinus();
            model.ExtraParameter = model.ExtraParameter ?? new BackendDefineButtonActionExtraParameterModel();

            //此功能 目前只有 admin 或 CS 可以操作 (沒開給代理)
            //如果是客服只能操作部分功能 (避免被惡意API攻擊)
            if (model.IsOperateByCSOrAM)
            {
                List<int> availableFunctions = new List<int>
                {
                    (int)BackendButtonEnum.UnlockFailLoginLocker,
                    (int)BackendButtonEnum.TechTeam_TriggerSingleWalletValidateTestProcess
                };

                if (!availableFunctions.Contains(model.BackendButtonActionID))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_PagePermissionNotAllow);
                }
            }

            if (model.MaxTimesPerHour.HasValue)
            {
                int maxTimes = model.MaxTimesPerHour.Value;
                string timeText = DateTimeHelper.GetCurrentTimeYYYYMMDDHH();
                string cacheKey = $"{model.BackendButtonActionID}_{userID}_{timeText}";

                int newValue = CacheHelper.CounterMaxTimesHourly(cacheKey);

                if (newValue > maxTimes)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes, new ErrorDetailModel
                    {
                        ColumnName = $"Max {maxTimes} times per hour",
                        IsAlwaysNotify = true,
                    });
                }

                model.ExtraParameter.IsDisableNotify = true;
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var parseToEnum = (BackendButtonEnum)model.BackendButtonActionID;

            BackendButtonResultModel result = new BackendButtonResultModel();

            switch (parseToEnum)
            {
                case BackendButtonEnum.ResetAllSessionToken:
                    [REDACTED];
                    break;

                case BackendButtonEnum.ForceLogoutAdminSysAccounts:
                    ForceLogoutAdminSysAccounts();
                    break;

                case BackendButtonEnum.Test_DebugTimeout:
                    Test_DebugTimeout();
                    break;

                case BackendButtonEnum.Statistic_CalculateAgain_TargetDate:
                    Statistic_CalculateAgain_TargetDate(model.ExtraParameter);
                    break;

                //批次新增三方遊戲
                case BackendButtonEnum.BatchCreateThirdGameVendorGames:
                    result = BatchCreateThirdGameVendorGames(model.ExtraParameter);
                    break;

                case BackendButtonEnum.ResetMerchantPlayerSessionToken:
                    [REDACTED];
                    break;

                case BackendButtonEnum.CheckStatControlTableTime:
                    CheckStatControlTableTime(model.ExtraParameter);
                    break;

                case BackendButtonEnum.CheckPlayerMoney:
                    result = CheckPlayerMoney(userID, model.ExtraParameter);
                    break;

                case BackendButtonEnum.ManualTriggerDepositByQueueID:
                    ManualTriggerDepositByQueueID(userID, model.ExtraParameter);
                    break;

                case BackendButtonEnum.EnableEditEverything:
                    EnableEditEverything();
                    break;

                case BackendButtonEnum.Input_BetRecordNoPGS_RecoverToDate:
                    result = Input_BetRecordNoPGS_RecoverToDate(model.ExtraParameter);
                    break;

                case BackendButtonEnum.SingleWallet_UseLastTokenToLobbyPage:
                    result = UseNewestPlatformSessionTokenToLobbyPage(model.ExtraParameter);
                    break;

                //case BackendButtonEnum.QA_TriggerOneTimeJob:
                //    result = TriggerQAClearData(model.ExtraParameter);
                //    break;

                case BackendButtonEnum.QA_ClearData:
                    result = TriggerQAClearData(model.ExtraParameter);
                    break;

                case BackendButtonEnum.QA_GenerateTestData:
                    result = TriggerQAGenerateTestData(model.ExtraParameter);
                    break;

                case BackendButtonEnum.QA_SendFreespin:
                    result = QA_SendFreespin(model.ExtraParameter);
                    break;

                case BackendButtonEnum.RemoveCacheByKeyword:
                    RemoveCacheByKeyword(model.ExtraParameter);
                    break;

                case BackendButtonEnum.RemoveCacheAfterInsertNewGame:
                    RemoveCacheAfterInsertNewGame(model.ExtraParameter);
                    break;

                case BackendButtonEnum.GetCacheValueByKey:
                    result = GetCacheValueByKey(model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_CompareLeftAndRightList:
                    result = TechTeam_CompareLeftAndRightList(model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_AnalyzeTextAndConvertToList:
                    result = TechTeam_AnalyzeTextAndConvertToList(model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_TriggerSingleWalletValidateTestProcess:
                    result = TechTeam_TriggerSingleWalletValidateTestProcess(userID, model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_MultipleAddPerGameTags_ByTransactionNo:
                    result = TechTeam_MultipleAddPerGameTags_ByTransactionNo(userID, model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_RecoverSwCallbackRetryQueue_ByTransactionNo:
                    TechTeam_RecoverSwCallbackRetryQueue_ByTransactionNo(userID, model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_AppendLobbyBackurl:
                    TechTeam_AppendLobbyBackurl(model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_ForceCreateAGGHierarchyNodes:
                    TechTeam_ForceCreateAGGHierarchyNodes(model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_TableSQLGenerate:
                    result = TechTeam_TableSQLGenerate(model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_TestVendorAPI:
                    result = TechTeam_TestVendorAPI(model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_TriggerAGGForceSettle:
                    result = TechTeam_TriggerAGGForceSettle(model.ExtraParameter);
                    break;

                case BackendButtonEnum.TechTeam_CheckPerGameExist:
                    result = TechTeam_CheckPerGameExist(model.ExtraParameter);
                    break;

                case BackendButtonEnum.UnlockFailLoginLocker:
                    UnlockFailLoginLocker(model.ExtraParameter);
                    break;

                case BackendButtonEnum.FileResult_DownloadSingleGameWagers:
                    result = FileResult_DownloadSingleGameWagers(model.ExtraParameter);
                    break;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel(model.BackendButtonActionID.ToString()));
            }

            if (!currentUser.IsSuperAdminOrg)
            {
                var log = InstanceOperateLog(userID, UserOperateTypeEnum.TriggerBackendButton, model.BackendButtonActionID);
                WriteOperateLogAndNotifyCsOperate(userID, log);
            }

            return result;
        }


        private void UnlockFailLoginLocker(BackendDefineButtonActionExtraParameterModel para)
        {
            CacheRemoveHelper.UnlockFailLoginLocker();
        }

        private BackendButtonResultModel FileResult_DownloadSingleGameWagers(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            para.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            para.RuleFor(x => x.Begin).Rule_NotNullDateTime();
            para.RuleFor(x => x.End).Rule_NotNullDateTime();

            string selectColumns = " TOP 500000 TransactionNo , ThirdPartyBetRecordNo , GameTime , MerchantID , PlayerID , BetAmount , GameID , PayoutAmount , UpdateAmount ";

            var queryWagers = PerGameDetailLogRepository.GetByFilter(new QueryDetailLogRequestViewModel
            {
                ServerIDs = para.VendorConfigID.ToListCollection(),
                GameIDs = para.GameID.ToListCollection(),
                GameStartTimeBegin = para.Begin,
                GameStartTimeEnd = para.End

            }, selectColumns);

            List<List<string>> eachRowText = new List<List<string>>();

            eachRowText.Add(new List<string>
            {
                "PGS單號" ,
                "遊戲商單號" ,
                "遊戲時間" ,
                "商戶ID" ,
                "PlayerID" ,
                "GameCode" ,
                "BetAmount" ,
                "PayoutAmount" ,
                "WinLossAmount" ,
            });

            if (queryWagers.IsNotEmptyNull())
            {
                List<int> gameIDs = queryWagers.Select(z => z.GameID).Distinct().ToList();

                var queryGameDic = GameListRepository.GetByIds(gameIDs).ToDictionary(z => z.GameID, z => z);

                foreach (var item in queryWagers)
                {
                    var matchGame = queryGameDic.GetDictionaryByIntOrDefaultValue(item.GameID, null);

                    var myRow = new List<string>
                    {
                        item.TransactionNo,
                        item.ThirdPartyBetRecordNo,
                        item.GameTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        item.MerchantID.ToString(),
                        item.PlayerID.ToString(),
                        matchGame?.ThirdPartyGameCode,
                        item.BetAmount.ToString(),
                        item.PayoutAmount.ToString(),
                        item.UpdateAmount.ToString(),
                    };

                    eachRowText.Add(myRow);
                }
            }

            var excelGen = ExcelFileParseService.CreateNewExcelAndAppendTextV2(new NewExcelSheetV2Parameter
            {
                IsHeaderRowHighlightStyle = true,
                EachRowText = eachRowText,
                ColumnConfigs = new List<ExcelCellConfigV2>
                     {
                         new ExcelCellConfigV2(40 , false),
                         new ExcelCellConfigV2(30 , false),
                         new ExcelCellConfigV2(30 , false),
                         new ExcelCellConfigV2(15 , false),
                         new ExcelCellConfigV2(15 , false),
                         new ExcelCellConfigV2(15 , false),
                         new ExcelCellConfigV2(15 , false),
                         new ExcelCellConfigV2(15 , false),
                     }
            });

            return new BackendButtonResultModel
            {
                FileResult = new MyFileResultModel
                {
                    FileStream = excelGen,
                    FileName = $"Wagers_{para.VendorConfigID}_{para.Begin.Value.AddHours(8).ToString("MMdd")}_{para.End.Value.AddHours(8).ToString("MMdd")}.xlsx"
                }
            };
        }


        private BackendButtonResultModel GetCacheValueByKey(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.Text).Rule_NotEmptyText();

            List<object> allData = new List<object>();

            //最多抓20筆
            var keys = CacheHelper.GetCacheKeysByKeyword(para.Text).Take(20);

            foreach (var key in keys)
            {
                var myValue = CacheHelper.GetCacheValue<string>(key);
                allData.Add(new
                {
                    Key = key,
                    Value = myValue
                });
            }

            return new BackendButtonResultModel
            {
                IsSuccess = true,
                ResultListObject = allData
            };
        }


        private BackendButtonResultModel TechTeam_MultipleAddPerGameTags_ByTransactionNo(int userID, BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.TextArea).Rule_NotEmptyText();
            para.RuleFor(z => z.TagID).Rule_NotZeroNotMinus();

            var service = IOCHelper.Resolve<IPerGameDetailService>();

            var tranNoList = AppHelper.AnalyzeTextFromRawTextBySpaceChar(para.TextArea);

            if (tranNoList.IsEmptyOrNull())
                return new BackendButtonResultModel();

            service.AddTagByTransactionNoList(userID, new PerGameTagBatchAddRequestModel
            {
                TransactionNoList = tranNoList,
                TagID = para.TagID
            });

            return new BackendButtonResultModel
            {
                Message = $"共處理 {tranNoList.Count()} 資料",
            };
        }


        private BackendButtonResultModel TechTeam_TriggerSingleWalletValidateTestProcess(int userID, BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();

            para.Text = (para.Text ?? "").TrimOrNull();

            string inputText = para.Text;

            var queryFilter = new QueryDetailLogRequestViewModel
            {
                GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddHours(-48),
                GameStartTimeEnd = DateTimeHelper.GetCurrentTime(),
                MerchantID = para.MerchantID,
                PageSize = 50,
            };

            bool isQueryRecentPerGame = false;

            string playerNameTemp = "";

            if (string.IsNullOrWhiteSpace(para.Text))
            {
                isQueryRecentPerGame = true;
            }
            else if ((!RegexRuleHelper.IsPgs32TransactionNo(inputText)))
            {
                var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(inputText, para.MerchantID);

                if (queryPlayer == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);
                }

                isQueryRecentPerGame = true;
                queryFilter.PlayerIDs = queryPlayer.PlayerID.ToListCollection();
                playerNameTemp = queryPlayer.PlayerName;
            }

            string transactionNo = para.Text.Trim();

            if (isQueryRecentPerGame)
            {
                var match = PerGameDetailLogRepository.GetPagingByFilter(queryFilter)
                .Items
                .OrderBy(x =>
                {
                    if (x.GameID >= 250000 && x.GameID <= 299900)
                    {
                        return 1;
                    }

                    return 999;
                })
                .ThenByDescending(x => x.CreateTime)
                .FirstOrDefault();

                if (match == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundBetRecord, new ErrorDetailModel($"【#A1 rule】{playerNameTemp} no wager data found in 24 hours."));
                }

                transactionNo = match.TransactionNo;
            }

            if (!RegexRuleHelper.IsPgs32TransactionNo(transactionNo))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("Text"));
            }

            var service = new SingleWalletMerchantApiTestValidateService(new SwValidateTestInitialConfigParameter
            {
                MerchantID = para.MerchantID,
                BetRecordNoPGS_AlreadySettle = transactionNo,
                IsDisableNotify = para.IsDisableNotify
            });


            var twTime = DateTimeHelper.GetCurrentTime(8);

            int maxTimes = 5;
            string counterKey = $"TriggerSWTest_{para.MerchantID}_{twTime.ToString("yyyyMMdd")}";

            var counter_times = CacheHelper.CounterMaxTimesDaily(counterKey);

            //限制觸發次數
            if (counter_times > maxTimes)
            {
                throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes, new ErrorDetailModel($"Max times: {maxTimes} ({para.MerchantID})"));
            }

            //觸發各種測試情境 (最花時間的地方)
            //觸發各種測試情境 (最花時間的地方)
            //觸發各種測試情境 (最花時間的地方)
            var result = service.RunAllTestProcess();

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.TriggerSingleWalletValidateTest, para.MerchantID);
            WriteOperateLog(log);

            var excelFileService = new ExcelFileParseService();

            List<List<string>> eachRowText = new List<List<string>>();

            List<string> headers = new List<string>();
            headers.Add("MerchantID");
            headers.Add("Case");
            headers.Add("Category");
            headers.Add("API Url");
            headers.Add("RequestBody");
            headers.Add("ResponseText");
            headers.Add("IsValid");
            headers.Add("Result Description (EN)");
            headers.Add("结果描述 (中文)");
            eachRowText.Add(headers);


            string merchantIDText = para.MerchantID.ToString();

            foreach (var item in result)
            {
                List<string> eachColumns = new List<string>();

                string displaySymbol = $"※ Refer Description{Environment.NewLine}(Check Manually)";
                switch (item.ResultType)
                {
                    case TestResultType.OK:
                        displaySymbol = "✅";
                        break;

                    case TestResultType.Invalid:
                        displaySymbol = "❌";
                        break;

                    default:
                        break;
                }

                eachColumns.Add(merchantIDText);
                eachColumns.Add(item.CaseNumber);
                eachColumns.Add(item.TestCategory);
                eachColumns.Add(item.ApiUrl);
                eachColumns.Add(item.RequestBody);
                eachColumns.Add(item.ResponseText.Truncate(1000));
                eachColumns.Add(displaySymbol);
                eachColumns.Add(item.MsgEN);
                eachColumns.Add(item.MsgCN);

                eachRowText.Add(eachColumns);
            }

            var excelGen = excelFileService.CreateNewExcelAndAppendTextV2(new NewExcelSheetV2Parameter
            {
                IsHeaderRowHighlightStyle = true,
                EachRowText = eachRowText,
                RowHeight = 1700,
                ColumnConfigs = new List<ExcelCellConfigV2>
                {
                    new ExcelCellConfigV2(15 , false),
                    new ExcelCellConfigV2(10 , false),
                    new ExcelCellConfigV2(25 , false),
                    new ExcelCellConfigV2(15 , false),
                    new ExcelCellConfigV2(35 , false),
                    new ExcelCellConfigV2(35 , false),
                    new ExcelCellConfigV2(25 , false),
                    new ExcelCellConfigV2(45 , true),
                    new ExcelCellConfigV2(45 , true),
                }
            });

            return new BackendButtonResultModel
            {
                FileResult = new MyFileResultModel
                {
                    FileStream = excelGen,
                    FileName = $"SWTestResult_{para.MerchantID}_{DateTimeHelper.GetCurrentTime(8).ToString("MMddHHmmss")}",
                }
            };
        }


        private BackendButtonResultModel TechTeam_RecoverSwCallbackRetryQueue_ByTransactionNo(int userID, BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.TextArea).Rule_NotEmptyText();

            var tranNoList = AppHelper.AnalyzeTextFromRawTextBySpaceChar(para.TextArea);

            foreach (var eachNo in tranNoList)
            {
                BetRecordService.RecoverNotifyQueueData(userID, eachNo);
            }

            return new BackendButtonResultModel
            {
                Message = ""
            };
        }


        private BackendButtonResultModel TechTeam_TableSQLGenerate(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.TargetTable).Rule_NotEmptyText();

            string tableNameLower = para.TargetTable.ToLower();

            bool isQueryGameList = tableNameLower == "gamelist";

            StringBuilder sb = new StringBuilder();

            var parseIDs = AppHelper.AnalyzeIDsFromRawText(para.TextArea);

            if (isQueryGameList)
            {
                if (parseIDs.IsEmptyOrNull())
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel("gameIDs"));
                }

                var gen = AdminSysInternalUtilityService.GenerateSqlInsertCommand(new LazyCommonRequestModel
                {
                    GameIDs = parseIDs,
                    Table = para.TargetTable
                });

                sb.AppendLine(gen.SQL);
            }
            else if (parseIDs.IsNotEmptyNull())
            {
                foreach (var eachID in parseIDs)
                {
                    var gen = AdminSysInternalUtilityService.GenerateSqlInsertCommand(new LazyCommonRequestModel
                    {
                        TargetID = eachID,
                        Table = para.TargetTable
                    });

                    sb.AppendLine(gen.SQL);
                }
            }
            else
            {
                var excludeColumns = AppHelper.AnalyzeTextFromRawTextBySpaceChar(para.TextArea);

                var gen = AdminSysInternalUtilityService.GenerateSqlInsertCommand(new LazyCommonRequestModel
                {
                    ExcludeColumns = excludeColumns,
                    Table = para.TargetTable
                });

                sb.AppendLine(gen.SQL);
            }

            string finalSQL = sb.ToString();

            return new BackendButtonResultModel
            {
                Message = finalSQL,
            };
        }

        private BackendButtonResultModel TechTeam_CompareLeftAndRightList(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.TextArea).Rule_NotEmptyText();
            para.RuleFor(z => z.TextArea2).Rule_NotEmptyText();

            var list_1 = new SortedSet<string>(AppHelper.AnalyzeTextFromRawTextBySpaceChar(para.TextArea));
            var list_2 = new SortedSet<string>(AppHelper.AnalyzeTextFromRawTextBySpaceChar(para.TextArea2));

            if (list_1.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter_CollectionIsEmptyNull, new ErrorDetailModel("list_1"));

            if (list_2.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter_CollectionIsEmptyNull, new ErrorDetailModel("list_2"));

            SortedSet<string> array1_notExist = new SortedSet<string>();
            SortedSet<string> array2_notExist = new SortedSet<string>();

            foreach (var item_2 in list_2)
            {
                if (!list_1.Contains(item_2))
                {
                    array1_notExist.Add(item_2);
                }
            }

            foreach (var item_1 in list_1)
            {
                if (!list_2.Contains(item_1))
                {
                    array2_notExist.Add(item_1);
                }
            }

            string finalMsg = "";

            if (array1_notExist.IsNotEmptyNull())
            {
                finalMsg += $"以下單號不在集合一 ({array1_notExist.Count()}筆)" + Environment.NewLine + string.Join(Environment.NewLine, array1_notExist);
            }

            if (array2_notExist.IsNotEmptyNull())
            {
                finalMsg += $"以下單號不在集合二 ({array2_notExist.Count()}筆)" + Environment.NewLine + string.Join(Environment.NewLine, array2_notExist);
            }

            return new BackendButtonResultModel
            {
                Message = finalMsg,
            };
        }

        private BackendButtonResultModel TechTeam_AnalyzeTextAndConvertToList(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.TextArea).Rule_NotEmptyText();

            List<string> list_1 = AppHelper.AnalyzeTextFromRawTextBySpaceChar(para.TextArea);

            string finalMsg = "共有以下單號:" + Environment.NewLine + string.Join(Environment.NewLine, list_1);

            return new BackendButtonResultModel
            {
                Message = finalMsg,
            };
        }


        private BackendButtonResultModel TechTeam_TriggerAGGForceSettle(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(x => x.Text).Rule_NotEmptyText();

            var slotsAPIService = ServiceInstanceHelper.InstanceSlotsGameVendorService(GameVendorTypeEnum.AGG);

            string betRecordNoPGS = para.Text;

            //API 請求對方重新發出結算通知 (然後externalAPI會再次收到請求)
            var result = slotsAPIService.TriggerSettlementRequestAgain(new AGGForceTriggerSettlementRequestReqeustParameter
            {
                BetRecordNoPGS = betRecordNoPGS,
                DebugReferenceUUID = betRecordNoPGS
            });

            string responseText = result.responseText;

            return new BackendButtonResultModel
            {
                Message = "三方API回應" + responseText
            };
        }


        private BackendButtonResultModel TechTeam_CheckPerGameExist(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(x => x.TextArea).Rule_NotEmptyText();

            List<string> listOfTransactionNo = AppHelper.AnalyzeTextFromRawTextBySpaceChar(para.TextArea);

            if (listOfTransactionNo.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter_Collection, new ErrorDetailModel("listOfTransactionNo"));

            if (listOfTransactionNo.CountOrZero() > 100)
                throw new TerproException(ApiErrorCode.InvalidParameter_Collection, new ErrorDetailModel("max listOfTransactionNo: 100"));

            string finalMsg = "";

            foreach (var tranNo in listOfTransactionNo)
            {
                var query = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(tranNo);

                if (query != null)
                    continue;

                finalMsg += $"單號: {tranNo} 查無資料" + Environment.NewLine;
            }

            if (string.IsNullOrWhiteSpace(finalMsg))
            {
                finalMsg = $"✅ 指定{listOfTransactionNo.Count()}筆單號，perGame資料都有存在";
            }

            return new BackendButtonResultModel
            {
                Message = finalMsg
            };
        }

        private BackendButtonResultModel TechTeam_TestVendorAPI(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.Text).Rule_NotEmptyText();

            int myMerchantID = Int32.Parse(para.Text);

            var queryMerchant = MerchantRepository.GetByIdOrThrowException(myMerchantID);

            if (AppHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("sw mode"));

            if (queryMerchant.Type == (int)MerchantTypeEnum.Plaform_ProductionEnvironment)
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("Merchant Type"));

            var vendorAPIUrl = SystemConfigSettingRepository.GetParseModelByEnum(SystemConfigSettingEnum.DocumentUrl_VendorAPIUrl);

            if (string.IsNullOrWhiteSpace(vendorAPIUrl.SettingValue))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("DocumentUrl_VendorAPIUrl"));
            }

            string baseUrl = vendorAPIUrl.SettingValue;

            string keywordName = "marshall";

            var queryTestPlayer = PlayerRepository.GetByFilter(new QueryPlayerRequestViewModel
            {
                MerchantID = queryMerchant.MerchantID,
                PlayerNameFuzzyQuery = keywordName
            }).FirstOrDefault();

            if (queryTestPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer, new ErrorDetailModel($"需要有關鍵字:{keywordName} 的測試帳號"));

            /*
                GET｜/v1/game/link
                GET｜/v1/games
                GET｜/v1/wallet/amount
                POST｜/v1/wallet/transfer-in
                POST｜/v1/wallet/transfer-out
            */

            var queryEnableGame = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = queryMerchant.MerchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
            })
                .FirstOrDefault();

            if (queryEnableGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            var walletService = IOCHelper.Resolve<IPlayerWalletRefService>();

            var queryWallet = walletService.GetWalletAmountByLatestToken(queryTestPlayer.PlayerID);

            string url_wallet = $"/v1/wallet/amount?merchantID={queryMerchant.MerchantID}&merchantSecretKey={queryMerchant.ApiSecretKey}&playerName={queryTestPlayer.PlayerName}";
            string url_games = $"/v1/games?merchantID={queryMerchant.MerchantID}&merchantSecretKey={queryMerchant.ApiSecretKey}";
            string url_launch = $"/v1/game/link?merchantID={queryMerchant.MerchantID}&merchantSecretKey={queryMerchant.ApiSecretKey}&playerName={queryTestPlayer.PlayerName}&gameID={queryEnableGame.GameID}";
            string url_transferIn = $"/v1/wallet/transfer-in";
            string url_transferOut = $"/v1/wallet/transfer-out";

            var p_api1 = InstanceVendorAPIReqeustParam(baseUrl, url_wallet, HttpMethodType.HttpGet);
            var p_api2 = InstanceVendorAPIReqeustParam(baseUrl, url_games, HttpMethodType.HttpGet);
            var p_api3 = InstanceVendorAPIReqeustParam(baseUrl, url_launch, HttpMethodType.HttpGet);
            var p_api4 = InstanceVendorAPIReqeustParam(baseUrl, url_transferIn, HttpMethodType.HttpPost);
            var p_api5 = InstanceVendorAPIReqeustParam(baseUrl, url_transferOut, HttpMethodType.HttpPost);

            decimal plusAmount = 1;

            var paramIn = new
            {
                amount = plusAmount,
                merchantOrderNo = Guid.NewGuid().ToString(),
                playerName = queryTestPlayer.PlayerName,
                merchantID = queryMerchant.MerchantID,
                merchantSecretKey = queryMerchant.ApiSecretKey
            };

            var paramOut = new
            {
                amount = plusAmount,
                merchantOrderNo = Guid.NewGuid().ToString(),
                playerName = queryTestPlayer.PlayerName,
                merchantID = queryMerchant.MerchantID,
                merchantSecretKey = queryMerchant.ApiSecretKey
            };

            var apiResult_wallet = APIRequestHelper.RequestApiResult<ApiResponseGenericModel<WalletCashPlayerAmountViewModel>>(p_api1);
            var apiResult_games = APIRequestHelper.RequestApiResult<ApiResponseGenericModel<List<GameInfoViewModel>>>(p_api2);
            var apiResult_launch = APIRequestHelper.RequestApiResult<ApiResponseGenericModel<GameLinkGenerateResultViewModel>>(p_api3);
            var apiResult_transferIn = APIRequestHelper.RequestApiResult<ApiResponseGenericModel<WalletTransferResultViewModel>>(p_api4, paramIn);
            var apiResult_transferOut = APIRequestHelper.RequestApiResult<ApiResponseGenericModel<WalletTransferResultViewModel>>(p_api5, paramOut);

            CheckErrorResponse(apiResult_wallet);
            CheckErrorResponse(apiResult_games);
            CheckErrorResponse(apiResult_launch);
            CheckErrorResponse(apiResult_transferIn);
            CheckErrorResponse(apiResult_transferOut);

            if (apiResult_wallet.ResultObject.Data.Amount != queryWallet.Amount)
            {
                SendMessageToMultipleChannel($"API回應不符合預期。餘額錯誤 `{apiResult_wallet.ResultObject.Data.Amount}");
            }

            if (apiResult_games.ResultObject.Data.IsEmptyOrNull())
            {
                SendMessageToMultipleChannel($"API回應不符合預期。遊戲清單為空");
            }

            if (string.IsNullOrWhiteSpace(apiResult_launch.ResultObject.Data.Link))
            {
                SendMessageToMultipleChannel($"API回應不符合預期。遊戲網址為空 {apiResult_launch.ResultObject.Data.Link}");
            }

            if (apiResult_transferIn.ResultObject.Data.AfterAmount != (queryWallet.Amount + plusAmount))
            {
                SendMessageToMultipleChannel($"API回應不符合預期。轉帳後餘額怪怪的 {apiResult_transferIn.ResultObject.Data.AfterAmount}");
            }

            if (apiResult_transferOut.ResultObject.Data.AfterAmount != queryWallet.Amount)
            {
                SendMessageToMultipleChannel($"API回應不符合預期。轉帳後餘額怪怪的 {apiResult_transferOut.ResultObject.Data.AfterAmount}");
            }

            SendMessageToMultipleChannel($"測試結束。若無其他通知代表測試成功 {queryMerchant.MerchantID}");

            return new BackendButtonResultModel
            {

            };
        }



        private void CheckErrorResponse<T>(ApiRequestResultModel<ApiResponseGenericModel<T>> res)
        {
            if (res.ResponseStatusCode != 200 || res.ResultObject == null)
            {
                SendMessageToMultipleChannel($"API錯誤 `{res.RelativeUrl}` {res.ResponseText}");
            }

            if (res.ResultObject.Code >= 400001)
            {
                SendMessageToMultipleChannel($"API錯誤 `{res.RelativeUrl}` {res.ResponseText}");
            }
        }



        private RequestParameters InstanceVendorAPIReqeustParam(string baseUrl, string apiUrl, HttpMethodType method)
        {
            string relativeUrl = apiUrl.Replace("/v1/", "").Truncate(20).Split('?').FirstOrDefault();

            return new RequestParameters
            {
                BaseUrl = baseUrl,
                ApiUrl = apiUrl,
                Method = method,
                LogAction = WriteRequestLog,
                AdvancedSetting = new AdvancedSetting
                {
                    Log_SubCategory = relativeUrl,
                }
            };
        }


        protected void WriteRequestLog(ApiRequestLogTemporaryInfoModel model)
        {
            var ExternalApiRequestLogRefService = IOCHelper.Resolve<IExternalApiRequestLogRefService>();
            ExternalApiRequestLogRefService.Create(model);
        }


        private void TechTeam_AppendLobbyBackurl(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.TextArea).Rule_NotEmptyText();

            var splitUrls = AppHelper.AnalyzeTextFromRawTextBySpaceChar(para.TextArea);

            if (splitUrls.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("無法解析出網址Url"));

            //自動補上https
            splitUrls = splitUrls.Select(url =>
                {
                    if (url.Contains("http"))
                    {
                        return url;
                    }

                    return $"https://{url}";
                })
                .ToList();

            foreach (string url in splitUrls)
            {
                CheckImageUrlWork(url);
            }

            var settingKey = OneToManyCustomizedSettingTypeEnum.GameUrlDoamin_BackupUrls;
            var queryItems = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(settingKey, isExcludeEmptyValue: false);

            int maxSubCateogry = queryItems
                .Where(x => x.SubCategory.IsNumeric())
                .Select(x => Int32.Parse(x.SubCategory))
                .Max();

            var insertItems = new List<OneToManyCustomizedSettingItemEntity>();

            var matchFirst = queryItems.FirstOrDefault();

            int mySequence = maxSubCateogry;

            foreach (string url in splitUrls)
            {
                mySequence++;
                insertItems.Add(new OneToManyCustomizedSettingItemEntity
                {
                    OneToManyCustomizedSettingID = matchFirst.OneToManyCustomizedSettingID,
                    SettingKey = matchFirst.SettingKey,
                    SettingValue = url,
                    SubCategory = mySequence.ToString(),
                    IsDefault = false,
                    Memo = "",
                    Memo2 = "",
                    Memo3 = "",
                });
            }

            OneToManyCustomizedSettingItemRepository.CreateMany(insertItems);
        }


        private void TechTeam_ForceCreateAGGHierarchyNodes(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(x => x.TextArea).Rule_NotEmptyText();

            var merchantIDs = AppHelper.AnalyzeIDsFromRawText(para.TextArea);

            if (merchantIDs.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("parse merchantIDs"));

            var aggService = new AGGIntegrationService();

            foreach (int merchantID in merchantIDs)
            {
                aggService.CreateAGGNamespaceByAPI(merchantID);
            }
        }


        private void CheckImageUrlWork(string domain)
        {
            var ts = DateTimeHelper.GetCurrentTimeTimestamp();
            string targetUrl = (domain + "/game-icon/agg_ht-trainrobber.png").ReplaceDuplicateUrlChar() + $"?v={ts}";

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


        private void RemoveCacheByKeyword(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.Text).Rule_NotEmptyText();

            if (para.Text.Contains("003"))
            {
                //指定清除編號3所有cache
                RedisCache_3.RemoveCacheAll();
            }
            else
            {
                CacheHelper.RemoveCacheByKeyword(para.Text);
                RedisCache_2.RemoveCacheByKeyword(para.Text);
                RedisCache_3.RemoveCacheByKeyword(para.Text);
            }
        }

        private void RemoveCacheAfterInsertNewGame(BackendDefineButtonActionExtraParameterModel para)
        {
            RemoveCacheByKeyword(new BackendDefineButtonActionExtraParameterModel { Text = "DbEntity_LanguageTexts" });
            RemoveCacheByKeyword(new BackendDefineButtonActionExtraParameterModel { Text = "GetAllGameNameLanguageText" });
        }


        private void Statistic_CalculateAgain_TargetDate(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.Date).Rule_NotNullDateTime();

            DateTime beginUTC = para.Date.Value;

            SystemSPRepository.CalculateStatisticSummaryDataAgain(beginUTC);

            //透過SP檢查數據是否有異常
            if (SettingHelper.IsProduction)
            {
                DateTime endUTC = beginUTC.AddHours(24);
                var now = DateTimeHelper.GetCurrentTime();

                if (endUTC > now)
                {
                    endUTC = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(-1);
                }

                var spResult = SystemSPRepository.CheckVIPMerchantStatDBDataCorrect(beginUTC, endUTC);
                SendNotifyWhenSPCheckNotSuccess(spResult);
            }
        }


        /// <summary>
        /// 批次新增 三方遊戲商的遊戲(使用Excel格式)
        /// </summary>
        private BackendButtonResultModel BatchCreateThirdGameVendorGames(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.TextArea).Rule_NotEmptyText();
            para.RuleFor(z => z.GameVendorType).Rule_NotZeroNotMinus();

            List<string> lines = para.TextArea.Split(new[] { Environment.NewLine, "\t\n", "\n" }, StringSplitOptions.None)
               .Where(x => !string.IsNullOrWhiteSpace(x))
               .ToList();

            if (lines.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError);

            int targetGameVendorType = para.GameVendorType;

            var queryMaxGameID = GameListRepository.GetPagingByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameVendorTypes = targetGameVendorType.ToListCollection(),
                GameID_MinValue = targetGameVendorType,
                GameID_MaxValue = targetGameVendorType + 970, //900系列通常是lobby大廳
                OrderByColumn = "GameID",
                IsASC = false,
                PageSize = 30,
            })
            .Items
            .OrderByDescending(x => x.GameID)
            .FirstOrDefault();

            //如果是首次建立資料 (新增預設樣板)
            if (queryMaxGameID == null)
            {
                queryMaxGameID = new GameListEntity
                {
                    GameID = targetGameVendorType,
                    GameType = GameTypeEnum.None.ToInteger(),
                    GameCode = GameVendorDefinitionHelper.GetVendorCodePrefix((GameVendorTypeEnum)targetGameVendorType)
                };
            }

            var queryGamesOfVendor = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameVendorTypes = targetGameVendorType.ToListCollection(),
                IsNotDelete = false
            });


            var random_3Game = queryGamesOfVendor
                .Where(x => x.GameID % 1000 <= 800)
                .OrderByRandom()
                .Take(3)
                .ToList();

            //判斷gameCode格式是哪一種 做點防呆
            bool isGameCodeFormatIsNumber = random_3Game.All(z => z.ThirdPartyGameCode.IsNumbericOfPositiveInt32());

            List<GameListEntity> insertGames = new List<GameListEntity>();
            int loopIndex = 1;
            int duplicateCount = 0;

            DateTime now = DateTimeHelper.GetCurrentTime();

            List<LanguageTextEntity> insertLangText = new List<LanguageTextEntity>();

            string notifyMsg = "";

            foreach (var eachLine in lines)
            {
                List<string> value_eachColumns = eachLine.Split('\t').Where(z => !string.IsNullOrWhiteSpace(z)).ToList();

                bool isColumn2 = value_eachColumns.Count() == 2;
                bool isColumn3 = value_eachColumns.Count() == 3;

                if (!isColumn2 && !isColumn3)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "Excel解析 欄位數不如預期");
                }

                int gameCodePosition = isColumn2 ? 1 : 2;
                int nameCN_Position = isColumn2 ? 0 : 0;
                int nameEN_Position = isColumn2 ? 0 : 1;

                string thirdGameCode = value_eachColumns[gameCodePosition].Trim();
                string nameCN = value_eachColumns[nameCN_Position];
                string nameEN = value_eachColumns[nameEN_Position];
                string gameCodePrefix = queryMaxGameID.GameCode.Split('_')[0];
                string pgsGameCode = $"{gameCodePrefix}_{thirdGameCode}";
                string langCode = $"GameList_{pgsGameCode}";

                string errorMsg = $"GameCode: {thirdGameCode} 格式與之前不相符";

                if (isGameCodeFormatIsNumber)
                {
                    if (!thirdGameCode.IsNumbericOfPositiveInt32())
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel(errorMsg));
                    }
                }
                else
                {
                    if (thirdGameCode.IsNumbericOfPositiveInt32())
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel(errorMsg));
                    }
                }

                var matchExistData = queryGamesOfVendor
                    .Where(z => z.ThirdPartyGameCode?.Trim() == thirdGameCode)
                    .OrderBy(x => x.GameID)
                    .FirstOrDefault();

                if (matchExistData != null)
                {
                    string duplicateMsg = $"gameID:{matchExistData.GameID}、code:{matchExistData.ThirdPartyGameCode} status:{matchExistData.Status} 資料存在";
                    notifyMsg += duplicateMsg + Environment.NewLine;
                    duplicateCount++;

                    continue;
                }

                var newEntity = new GameListEntity
                {
                    GameID = queryMaxGameID.GameID + loopIndex,
                    Status = NormalActiveStatus.Enable.ToInteger(),
                    GameName = nameCN,
                    ThirdPartyGameCode = thirdGameCode,
                    GameCode = pgsGameCode,
                    LanguageCode = langCode,
                    GameVendorType = targetGameVendorType,
                    CreateTime = now,
                    GameType = queryMaxGameID.GameType,
                };

                GameListRepository.SetDefaultValueForNewInsertEntity(newEntity);
                insertGames.Add(newEntity);

                insertLangText.Add(new LanguageTextEntity
                {
                    LanguageCode = newEntity.LanguageCode,
                    Text = AppHelper.SimplifiedToTraditional(nameCN),
                    Language = LanguageEnum.zh_tw.ToString()
                });

                insertLangText.Add(new LanguageTextEntity
                {
                    LanguageCode = newEntity.LanguageCode,
                    Text = AppHelper.TraditionalToSimplified(nameCN),
                    Language = LanguageEnum.zh_cn.ToString()
                });

                insertLangText.Add(new LanguageTextEntity
                {
                    LanguageCode = newEntity.LanguageCode,
                    Text = nameEN,
                    Language = LanguageEnum.en_us.ToString()
                });

                loopIndex++;
            }

            GameListRepository.CreateMany(insertGames);

            LanguageTextRepository.CreateMany(insertLangText);

            if (!string.IsNullOrWhiteSpace(notifyMsg))
            {
                SendMessageToMultipleChannel(notifyMsg);
            }

            string finalMsg = $@"
本次新增:{insertGames.Count()}筆
{string.Join(Environment.NewLine, insertGames.Select(x => x.GameID))}
已存在筆數:{duplicateCount}筆";

            return new BackendButtonResultModel
            {
                Message = finalMsg
            };
        }

        private BackendButtonResultModel Input_BetRecordNoPGS_RecoverToDate(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.Text).Rule_NotEmptyText();

            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(para.Text.Trim());

            if (!recover.IsPgsOrderNo)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_IsNotValidPgsTransactionNoFormat);
            }

            string betRecordNo = para.Text.Trim();
            var parseTime = recover.Time.Value;
            string LINE = Environment.NewLine;


            string msg = $"BetRecordNoPGS = '{betRecordNo}' AND PartitionTime = '{parseTime.ToString("yyyy-MM-dd HH:mm:ss")}' {LINE}{LINE}{LINE}" +
                $"CreateTime >= '{parseTime.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss")}' AND CreateTime < '{parseTime.AddHours(2).ToString("yyyy-MM-dd HH:mm:ss")}'";

            return new BackendButtonResultModel
            {
                ResultObject = msg,
                Message = msg
            };
        }


        private BackendButtonResultModel UseNewestPlatformSessionTokenToLobbyPage(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();

            var queryMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                PlayerID = para.PlayerID
            })
                .FirstOrDefault();

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            if (!CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            var queryLastToken = [REDACTED] QuerySingleWalletMerchantSessionTokenRequestViewModel
            {
                PlayerIDs = para.PlayerID.ToListCollection(),
                PageSize = 1,
                IsASC = false
            });

            if (queryLastToken.Items.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
            }

            var newestToken = [REDACTED];

            var launchUrl = GameLaunchService.LaunchGameAndGetGamePageUrlByPlayerID(new InternalTestBundleRequestModel
            {
                GameID = 0,
                PlayerID = para.PlayerID,
                PlatformSessionToken = [REDACTED],
            });

            return new BackendButtonResultModel
            {
                ResultObject = launchUrl,
                Message = launchUrl.Link
            };
        }

        private BackendButtonResultModel QA_TriggerOneTimeJob(BackendDefineButtonActionExtraParameterModel para)
        {
            SystemSPRepository.TriggerQAClearData();
            CacheHelper.RemoveCacheAll();
            return new BackendButtonResultModel
            {

            };
        }

        private BackendButtonResultModel TriggerQAClearData(BackendDefineButtonActionExtraParameterModel para)
        {
            OneTimesJobService.CheckOneTimesJobToExecute();
            return new BackendButtonResultModel
            {

            };
        }

        private BackendButtonResultModel TriggerQAGenerateTestData(BackendDefineButtonActionExtraParameterModel para)
        {
            para.Text = para.Text.TrimOrNothing();
            SystemSPRepository.TriggerQAGenerateTestData(para.Text);
            return new BackendButtonResultModel
            {

            };
        }

        private BackendButtonResultModel QA_SendFreespin(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            para.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            para.RuleFor(x => x.Text).Rule_NotEmptyText();
            para.RuleFor(x => x.NumberText).Rule_NotEmptyText();

            var queryMerchant = MerchantRepository.GetById(para.MerchantID);

            if (SettingHelper.IsProduction)
            {
                if (queryMerchant.Type != (int)MerchantTypeEnum.Plaform_DevelopEnvironment &&
                   queryMerchant.Type != (int)MerchantTypeEnum.InternalTest &&
                   queryMerchant.Type != (int)MerchantTypeEnum.GameProvider_QATest)
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("正式環境僅限測試商戶"));
                }
            }

            var freespinService = IOCHelper.Resolve<IAGGFreespinService>();

            int rounds = Int32.Parse(para.NumberText);

            var gameSetting = freespinService.FreespinSpin_GetBetSetting(new AggGameInfoReqeustParameter
            {
                MerchantID = queryMerchant.MerchantID,
                MerchantSecretKey = queryMerchant.ApiSecretKey,
            });

            var matchGame = gameSetting.GameInfoList
                .Where(x => x.GameID == para.GameID)
                .FirstOrDefault();

            if (matchGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame, new ErrorDetailModel("Not enable game or not support freespin"));

            decimal betAmount = 1.0M;

            if (matchGame != null)
            {
                betAmount = matchGame?.BetRanges
                    .Where(x => x >= 1)
                    .FirstOrDefault() ?? 1.0M;
            }

            var result = freespinService.FreespinSpin_Set(new VendorAPIFreespinSetReqeustParameter
            {
                MerchantID = queryMerchant.MerchantID,
                MerchantSecretKey = queryMerchant.ApiSecretKey,
                StartTime = DateTimeHelper.GetCurrentTime(),
                EndTime = DateTimeHelper.GetCurrentTime().AddDays(14),
                Players = new List<string>
                {
                    para.Text
                },
                UseEndTime = DateTimeHelper.GetCurrentTime().AddDays(14),
                Rounds = rounds,
                GameSettings = new List<AGGFreespinGameListItemViewModel>
                {

                    new AGGFreespinGameListItemViewModel
                    {
                        GameID = para.GameID,
                        Bet = betAmount
                    }
                },
            });

            return new BackendButtonResultModel
            {
                Message = $"創建成功, 新資料freespinID: {result?.FreespinID}",
            };
        }

        private void EnableEditEverything()
        {
            string cacheKey = CacheKey.AdminSys_SuperAdminEableEditEverything();

            //開通5分鐘
            CacheHelper.SetCacheValue(cacheKey, "XX", new TimeSpan(0, 5, 0));
        }

        private void ResetAllSessionToken()
        {
            PlayerTokenLogoutService.ResetAllSessionToken();
        }


        private void ForceLogoutAdminSysAccounts()
        {
            UserTokenRecordRepository.ForceLogoutAdminSysAccounts();
        }


        private void Test_DebugTimeout()
        {
            Thread.Sleep(1000 * 180);
        }


        public void ResetMerchantPlayerSessionToken(BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();

            PlayerTokenLogoutService.ResetMerchantPlayerSessionToken(para.MerchantID);
        }

        public void CheckStatControlTableTime(BackendDefineButtonActionExtraParameterModel para)
        {
            SystemMonitorService.CheckReportJobStatusHealthy();
        }

        private BackendButtonResultModel CheckPlayerMoney(int userID, BackendDefineButtonActionExtraParameterModel para)
        {
            
            if(SettingHelper.IsDev && para.PlayerID == 0)
            {
                var queryAll = PerGameDetailLogRepository.GetPagingByFilter(new QueryDetailLogRequestViewModel
                {
                    GameStartTimeBegin = DateTimeHelper.GetCurrentTime().AddHours(-1),
                    GameStartTimeEnd = DateTimeHelper.GetCurrentTime(),
                    PageSize = 2000
                })
                .Items;

                if (queryAll.IsEmptyOrNull())
                    throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel("queryAll 無可用資料"));

                var players = queryAll
                    .GroupBy(x => x.PlayerID)
                    .Select(x => new
                    {
                        PlayerID = x.Key,
                        Count = x.Count(),
                    })
                    .OrderByDescending(z => z.Count)
                    .ToList();

                para.PlayerID = players.FirstOrDefault().PlayerID;
            }

            para.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();

            var myParam = new CheckPlayerMoneyRequestModel
            {
                PlayerID = para.PlayerID,
            };

            if(myParam.BeginTime.IsEmptyTime())
            {
                myParam.BeginTime = DateTimeHelper.GetCurrentTime(8).To_00_00_00().AddHours(-8);
                myParam.EndTime = myParam.BeginTime.AddHours(24);
            }

            var result = AdminSysComplexService.CheckPlayerMoney(userID, myParam);

            return new BackendButtonResultModel
            {
                ResultObject = result
            };
        }


        private void SendNotifyWhenSPCheckNotSuccess(RoutineSPResult spResult)
        {
            if (!spResult.IsSuccess)
            {
                string myMsg = spResult.ResultMsg;
                SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_adminSys_api);
            }
        }


        public void ManualTriggerDepositByQueueID(int userID, BackendDefineButtonActionExtraParameterModel para)
        {
            para.RuleFor(z => z.Text).Rule_NotEmptyText();

            long queueID = Int64.Parse(para.Text);

            PlayerInfoService.BatchManualDepositPlayerWalletByQueueTable(userID, new BatchPlayerManualDepositRequestModel
            {
                Memo = $"手動觸發補錢_{DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS()}",
                QueueID = queueID,
                TransactionType = TransactionTypeEnum.BetRecord_ManualCancelRecord_RefundWalletManual.ToInteger()
            });
        }

    }

}
