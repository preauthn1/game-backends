using DocumentFormat.OpenXml.Wordprocessing;
using game.Entity.Game;
using MathNet.Numerics.Distributions;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Models.StoreProcedures;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{

    public partial class StatReportService : BaseAdminSysService, IStatReportService
    {

        public CurrencyRepository CurrencyRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }
   
        public MerchantVendorConfigMigrateRecordRepository MerchantVendorConfigMigrateRecordRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public MerchantEventSummaryDataRepository MerchantEventSummaryDataRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PlayerGameDataRepository PlayerGameDataRepository { get; set; }

        public PlayerLoginDataRepository PlayerLoginDataRepository { get; set; }

        public PlayerLoginDeviceDataRepository PlayerLoginDeviceDataRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public StatReportStoreProcedureRepository StatReportStoreProcedureRepository = new StatReportStoreProcedureRepository();

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public VendorConfigMerchantMappingRepository VendorConfigMerchantMappingRepository { get; set; }

        public W88PlayerDailyWalletRemainingRepository W88PlayerDailyWalletRemainingRepository { get; set; }


        public const int DefaultMaxDays = 31;

        public const decimal MIN_DIFF_VALUE = 0.3M;

        private const int SqlTimeoutSeconds = SystemConstant.ReportQuery_SqlTimeout;

        Dictionary<int, MerchantBriefEntity> _preQueryAllMerchantDic = new Dictionary<int, MerchantBriefEntity>();


        public StatGameReportViewModel GetGameStatReport(int userID, QueryStatGameReportApiRequestParameterModel filter)
        {
            filter.RuleFor(z => z.MerchantIDs).Rule_IsNotEmptyCollection();
            filter.RuleFor(z => z.MerchantIDs).Rule_MustSingleSelected();
            filter.IncludeExtraSearchInfo = filter.IncludeExtraSearchInfo ?? new List<int>();

            var queryCurrentUser = GetCurrentOwnPermissionByCache(userID);

            if (!filter.Begin.HasValue)
                filter.Begin = DateTimeHelper.GetCurrentTime().To_00_00_00();

            if (!filter.End.HasValue)
                filter.End = DateTimeHelper.GetCurrentTime().AddDays(1).To_23_59_59();

            filter.RuleFor(x => x.Begin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.End, 62);

            filter.MerchantIDs = FilterAvailablePermissionMerchantIDs(userID, filter.MerchantIDs);
            filter.GameIDs = filter.GameIDs ?? new List<int>();

            // 2021/8 效能因素 只支援單選
            int singleMerchantID = filter.MerchantIDs.FirstOrDefault();

            // 查詢統計
            List<Stat_GameSummarySPResultModel> querySummary = null;

            bool isWantToMergeAllChildAgents = filter.IncludeExtraSearchInfo.Contains((int)StatReportSearchColumnType.GameReport_MergeAllChildAgents);

            bool isQuerySummaryByHierarchyMode = false;

            // 必須勾選查詢 + 商戶屬於代理樹狀體系 才能這樣查
            if (isWantToMergeAllChildAgents)
            {
                var queryHierarchy = MerchantHierarchyRepository.GetById(singleMerchantID);

                if (queryHierarchy != null)
                {
                    isQuerySummaryByHierarchyMode = true;
                }
            }

            if (isQuerySummaryByHierarchyMode)
            {
                var queryAgentSummary = MerchantGameSummaryDataRepository.GetAgentHierarchyGameSummaryReport(new QueryAgentHierarchyGameSummaryReportRequestModel
                {
                    IsQueryStraightCustomerOnly = false,
                    TargetMerchantID = singleMerchantID,
                    Begin = filter.Begin.Value,
                    End = filter.End.Value
                });

                querySummary = queryAgentSummary.Select(x =>
                    {
                        var convert = x.ReturnInjecteObject<Stat_GameSummarySPResultModel>();
                        convert.WinAmount = x.WinLossAmount;
                        convert.TotalRound = x.TotalRound;
                        convert.UpdateAmount = x.WinLossAmount;

                        return convert;
                    })
                    .ToList();
            }
            else
            {

                querySummary = StatReportStoreProcedureRepository.GetGameSummaryStatReport(filter.Begin.Value, filter.End.Value, singleMerchantID, filter.GameIDs);
            }

            if (HasPermission_OnlyVisibleIDGGames(userID))
            {
                querySummary = querySummary
                    .Where(x => AppHelper.IsIDGVisibleGames(x.GameID))
                    .ToList();
            }

            var queryMyMerchants = MerchantRepository.GetByIds(filter.MerchantIDs)
                .Select(x => new StatGameReportMerchantItemViewModel
                {
                    MerchantID = x.MerchantID,
                    MerchantName = x.Name,
                    Items = new List<StatGameReportItemViewModel>()
                })
                .ToList();

            var queryAllGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameIDs = querySummary.Select(x => x.GameID).ToList(),
            });

            var queryEachMerchantGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                Status = NormalActiveStatus.Enable.ToInteger(),
                MerchantIDs = filter.MerchantIDs
            });

            List<int> gameIDs = queryEachMerchantGames.Select(z => z.GameID).ToList();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs);
            var gameNameDicEN = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs, LanguageEnum.en_us);

            foreach (var eachMerchant in queryMyMerchants)
            {
                var matchMyEnableGames = queryEachMerchantGames.Where(x => x.MerchantID == eachMerchant.MerchantID).ToList();

                foreach (var eachGames in queryAllGames)
                {
                    var matchSummaryData = querySummary.FirstOrDefault(x => x.MerchantID == eachMerchant.MerchantID && x.GameID == eachGames.GameID);
                    var matchGameEnable = matchMyEnableGames.FirstOrDefault(x => x.GameID == eachGames.GameID);

                    //如果有資料 或 有開放遊戲 都會顯示數據
                    if (matchSummaryData != null || matchGameEnable != null)
                    {
                        var summaryData = new StatGameReportItemViewModel();
                        summaryData.GameID = eachGames.GameID;

                        if (matchSummaryData != null)
                        {
                            summaryData = matchSummaryData.ReturnInjecteObject<StatGameReportItemViewModel>();
                        }

                        summaryData.GameType = eachGames.GameType;
                        summaryData.GameTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)eachGames.GameType);
                        summaryData.GameName = gameNameDic.GetDictionaryOrDefaultValue(summaryData.GameID, eachGames.GameName);
                        summaryData.GameNameEN = gameNameDicEN.GetDictionaryOrDefaultValue(summaryData.GameID, eachGames.GameName);
                        summaryData.RTP = CommonLogicHelper.CalculateRTP_Percentage(summaryData.BetAmount, summaryData.PayoutAmount);
                        summaryData.MerchantWinLoss = summaryData.UpdateAmount * -1;

                        eachMerchant.Items.Add(summaryData);
                    }
                }

                eachMerchant.CategoryReportItems = eachMerchant.Items
                    .GroupBy(z => z.GameType)
                    .Select(x =>
                    {
                        var instance = x.CalculateTotalSumMergeToOneObject();
                        instance.GameType = x.Key;
                        instance.GameTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)instance.GameType);
                        instance.RTP = CommonLogicHelper.CalculateRTP_Percentage(instance.BetAmount, instance.PayoutAmount);

                        return instance;
                    })
                    .OrderBy(z => z.GameType)
                    .ToList();

                eachMerchant.Items = eachMerchant.Items.OrderBy(x => x.GameID).ToList();
                eachMerchant.TotalSummary = eachMerchant.Items.CalculateTotalSumMergeToOneObject();
                eachMerchant.TotalSummary.RTP = CommonLogicHelper.CalculateRTP_Percentage(eachMerchant.TotalSummary.BetAmount, eachMerchant.TotalSummary.PayoutAmount);
            }

            var viewModel = new StatGameReportViewModel
            {
                Merchants = queryMyMerchants,
            };

            decimal totalBetAmount = queryMyMerchants.Select(x => x.TotalSummary.BetAmount).Sum();

            if (totalBetAmount == 0 && isWantToMergeAllChildAgents == false)
            {
                viewModel.IsShowGoToAgentReportHint = IsChildAgentExist(singleMerchantID);
            }

            if (isWantToMergeAllChildAgents)
            {
                var groupByColumns = new List<string>
                {
                    "GameID"
                };

                var queryEachGame = PlayerGameDataRepository.GetEachHierarchyMerchantEachGameSummaryData(filter.Begin.Value, filter.End.Value, singleMerchantID, groupByColumns);

                foreach (var eachM in viewModel.Merchants)
                {
                    foreach (var item in eachM.Items)
                    {
                        var matchGame = queryEachGame.FirstOrDefault(z => z.GameID == item.GameID);

                        if (matchGame != null)
                        {
                            item.OnlinePlayer = matchGame.PlayerCount;
                        }
                    }
                }

                //刻意拖慢速度 (不鼓勵用戶使用此功能)
                if (!queryCurrentUser.IsSuperAdminOrg)
                {
                    Thread.Sleep(1000 * 3);
                }
            }

            viewModel.IsQuerySummaryByHierarchyMode = isQuerySummaryByHierarchyMode;
            viewModel.ServerTimeTW = GetDisplayCurrentServerTimeTW().ServerTimeTW;
            return viewModel;
        }


        public MerchantSummaryStatReportViewModel GetMerchantSummaryStatReport(int userID, QueryMerchantSummaryStatReportReportApiRequestParameterModel filter)
        {
            if (!filter.Begin.HasValue)
                filter.Begin = DateTimeHelper.GetCurrentTime().To_00_00_00();

            if (!filter.End.HasValue)
                filter.End = DateTimeHelper.GetCurrentTime().AddDays(1).To_23_59_59();

            filter.RuleFor(z => z.MerchantIDs).Rule_IsNotEmptyCollection();
            filter.RuleFor(z => z.MerchantIDs).Rule_MustSingleSelected();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter.MerchantIDs = FilterAvailablePermissionMerchantIDs(userID, filter.MerchantIDs);

            //檢查報表開關是否開啟
            SystemConfigSettingRepository.CheckReportPageEnableOrThrowException();


            filter.IncludeExtraSearchInfo = filter.IncludeExtraSearchInfo ?? new List<int>();

            //根據前台是否有勾選。決定是否要查詢完整資訊
            bool isSearchFullInfo = filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.MerchantSummry_IncludeBasicInfo);

            var queryMerchants = MerchantRepository.GetByIds(filter.MerchantIDs);

            var merchantViewModels = queryMerchants.Select(x => new MerchantSummaryStatReportItemViewModel
            {
                MerchantID = x.MerchantID,
                MerchantName = x.Name,
                Currency = x.Currency,
            })
                .ToList();

            List<int> merchantIDs = merchantViewModels.Select(x => x.MerchantID).ToList();

            bool isQueryOnlyOne = merchantViewModels.Count() == 1;

            int merchantID = merchantViewModels.FirstOrDefault().MerchantID;

            foreach (var item in merchantViewModels)
            {
                int myMerchantID = item.MerchantID;

                Stat_MerchantSummarySPResultModel query = StatReportStoreProcedureRepository.GetMerchantSummaryStatReport(filter.Begin.Value, filter.End.Value, myMerchantID, isSearchFullInfo).FirstOrDefault();

                item.BasicSummary = query.ReturnInjecteObject<MerchantSummaryStatReportItemBaseInfoViewModel>();
                item.LoginSummary = query.ReturnInjecteObject<MerchantSummaryStatReportItemLoginInfoViewModel>();
                item.GameSummary = query.ReturnInjecteObject<MerchantSummaryStatReportItemGameInfoViewModel>();
                item.BasicSummary.Currency = item.Currency;

                item.GameSummary.MerchantID = item.MerchantID;
                item.GameSummary.MerchantName = item.MerchantName;
            }

            List<MerchantGameVendorSummaryViewModel> queryGameVendorReport = GetEachGameVendorSummaryReport(merchantIDs, filter.Begin.Value, filter.End.Value);

            //查單一筆資料才會處理(進階資訊)
            //查單一筆資料才會處理(進階資訊)
            //查單一筆資料才會處理(進階資訊)
            //查單一筆資料才會處理(進階資訊)
            if (isQueryOnlyOne)
            {
                var matchFirstMerchantViewModel = merchantViewModels.FirstOrDefault();

                var queryPromotionCost = MerchantEventSummaryDataRepository.GetEachMerchantTotalSumOnDuration(merchantID.ToListCollection(), filter.Begin.Value, filter.End.Value);

                decimal totalPromotionCost = 0;

                if (queryPromotionCost.IsNotEmptyNull())
                {
                    totalPromotionCost = queryPromotionCost.Select(x => x.TotalAmount).Sum();
                    matchFirstMerchantViewModel.GameSummary.TotalPromotionCost = totalPromotionCost;
                }

                //為了加快API回應速度 有多個執行緒會同步查詢
                List<Task> asyncTask = new List<Task>();
                int waitMillisecond = 1000 * SqlTimeoutSeconds;

                bool isQueryExtraData = filter.IncludeExtraSearchInfo.Any(x => x == (int)StatReportSearchColumnType.MerchantSummry_IncludeAdvanceInfo);

                if (!isQueryExtraData)
                {

                }
                else
                {
                    List<QueryPlayerLanguageReportModel> queryPlayerLangReport = new List<QueryPlayerLanguageReportModel>();
                    List<PlayerLoginDeviceModel> queryDeviceDistributeReport = new List<PlayerLoginDeviceModel>();
                    List<QueryEachPlayerTotalBetAmountModel> queryEachPlayerTotalBet = new List<QueryEachPlayerTotalBetAmountModel>();

                    asyncTask.Add(Task.Run(() => queryPlayerLangReport = PlayerGameDataRepository.GetMerchantLanguageDistributeReport(merchantID, filter.Begin.Value, filter.End.Value)));
                    asyncTask.Add(Task.Run(() => queryDeviceDistributeReport = PlayerLoginDeviceDataRepository.GetEachPlayerLoginDeviceCount(merchantID, filter.Begin.Value, filter.End.Value)));
                    asyncTask.Add(Task.Run(() => queryEachPlayerTotalBet = PlayerGameDataRepository.GetEachPlayerTotalBetAmount(merchantID, filter.Begin.Value, filter.End.Value)));

                    //非同步執行多個查詢
                    Task.WaitAll(asyncTask.ToArray(), waitMillisecond);

                    if (queryPlayerLangReport.IsNotEmptyNull())
                    {
                        int totalPlayers = queryPlayerLangReport.Select(z => z.TotalCount).Sum();
                        totalPlayers = totalPlayers == 0 ? 1 : totalPlayers;

                        matchFirstMerchantViewModel.LanguageDistributeReport = queryPlayerLangReport
                            .Select(z => new ReportCountSummaryViewModel
                            {
                                Memo = z.LanguageCode,
                                DisplayText = GetLanguageDispalyTextOrUnknowText(z.LanguageCode),
                                TotalCount = z.TotalCount,
                                Rate = AppHelper.CalculateRate(z.TotalCount, totalPlayers),
                            })
                            .ToList();
                    }

                    if (queryDeviceDistributeReport.IsNotEmptyNull())
                    {
                        int totalPlayers = queryDeviceDistributeReport.Select(z => z.PlayerID).Distinct().Count();

                        matchFirstMerchantViewModel.DeviceDistributeReport =
                            queryDeviceDistributeReport
                               .GroupBy(z => z.HDType)
                               .Select(z =>
                               {
                                   var first = z.FirstOrDefault();
                                   int playerCount = z.Select(z => z.PlayerID).Count();
                                   var convert = new ReportCountSummaryViewModel
                                   {
                                       Memo = first.HDType.ToString(),
                                       DisplayText = MultiLanguageTextHelper.GetEnumsCultureText((HDTypeEnum)first.HDType),
                                       TotalCount = playerCount,
                                       Rate = AppHelper.CalculateRate(playerCount, totalPlayers),
                                   };
                                   return convert;
                               })
                               .ToList();
                    }

                    matchFirstMerchantViewModel.PlayerTWDBetAmountDistribute = GetBetAmountRangeDistribute(merchantID, queryEachPlayerTotalBet);
                }
            }

            foreach (var item in merchantViewModels)
            {
                item.GameVendorSummary = queryGameVendorReport.FirstOrDefault(z => z.MerchantID == item.MerchantID);

                if (item.GameVendorSummary == null)
                {
                    item.GameVendorSummary = new MerchantGameVendorSummaryViewModel
                    {
                        TotalSummary = new MerchantGameVendorSummaryItemViewModel()
                    };
                }

                int totalPlayers = item.GameSummary.TotalBetPlayer;

                //總遊玩人數不應該直接加總 而是應該跟報表總遊玩人數一致
                item.GameVendorSummary.TotalSummary.BetPlayer = totalPlayers;
                item.GameSummary.RTP = CommonLogicHelper.CalculateRTP_Percentage(item.GameSummary.BetAmount, item.GameSummary.PayoutAmount);
            }

            var viewModel = new MerchantSummaryStatReportViewModel
            {
                //為了效能考量 部分資訊 只有在查詢單一筆商戶才會有值
                Merchants = merchantViewModels.OrderByDescending(x => x.GameSummary?.TotalRound ?? 0).ToList()
            };

            viewModel.ServerTimeTW = GetDisplayCurrentServerTimeTW().ServerTimeTW;
            return viewModel;
        }

        private string GetLanguageDispalyTextOrUnknowText(string langCode)
        {
            if (string.IsNullOrWhiteSpace(langCode))
                return "Unknow";

            return MultiLanguageTextHelper.GetLanguageCodeDisplayText(langCode);
        }


        private BetAmountRangeReportViewModel GetBetAmountRangeDistribute(int merchantID, List<QueryEachPlayerTotalBetAmountModel> players)
        {
            var viewModel = new BetAmountRangeReportViewModel();

            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            var queryCurrency = CurrencyRepository.GetById(queryMerchant.Currency);

            viewModel.Currency = queryMerchant.Currency;
            viewModel.ExchangeRate = 1;

            if (queryCurrency != null)
            {
                viewModel.ExchangeRate = queryCurrency.ExchangeRateToTWD;
            }

            if (players.IsEmptyOrNull())
            {
                return viewModel;
            }

            List<BetAmountRangeItemViewModel> amountRangeList = new List<BetAmountRangeItemViewModel>();

            amountRangeList.Add(new BetAmountRangeItemViewModel(10000000, 999990000000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(5000000, 10000000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(2000000, 5000000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(1000000, 2000000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(500000, 1000000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(200000, 500000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(100000, 200000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(50000, 100000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(25000, 50000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(10000, 25000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(5000, 10000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(2500, 5000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(1000, 2500));
            amountRangeList.Add(new BetAmountRangeItemViewModel(500, 1000));
            amountRangeList.Add(new BetAmountRangeItemViewModel(0, 500));

            //統一轉成台幣(TWD) 來檢視數據
            foreach (var item in players)
            {
                item.BetAmount = item.BetAmount * viewModel.ExchangeRate;
            }

            int totalPlayers = players.Count();
            decimal totalBetAmount = players.Select(z => z.BetAmount).Sum();
            decimal maxAmountTWD = players.Select(z => z.BetAmount).Max();

            //如果數據中 沒有超過1000W、500W的人 列表上 就不顯示高金額的區間
            amountRangeList = amountRangeList.Where(z => z.MinBetAmount <= maxAmountTWD).ToList();

            foreach (var item in amountRangeList)
            {
                var matchPlayers = players.Where(z => z.BetAmount >= item.MinBetAmount && z.BetAmount < item.MaxBetAmount).ToList();

                item.TotalCount = matchPlayers.Count();
                item.TotalBetAmount = matchPlayers.Select(z => z.BetAmount).Sum();
                item.Rate = AppHelper.CalculateRate(item.TotalCount, totalPlayers);
                item.BetAmountRate = AppHelper.CalculateRate(item.TotalBetAmount, totalBetAmount);
                item.PlayerIDs = matchPlayers.Select(z => z.PlayerID).ToList();
                item.IsAnyFishingGame = matchPlayers.Any(z => z.IsAnyFishingGame);
            }

            viewModel.TotalPlayers = totalPlayers;
            viewModel.TotalBetAmount = totalBetAmount;
            viewModel.Items = amountRangeList;
            return viewModel;
        }


        private List<MerchantGameVendorSummaryViewModel> GetEachGameVendorSummaryReport(List<int> merchantIDs, DateTime begin, DateTime end)
        {
            var queryGameVendorMapping = GameListRepository.GetAllGameVendorMapping();

            var queryEachGameSummary = MerchantGameSummaryDataRepository.GetEachGameTotalSumOnDuration(merchantIDs, null, begin, end);

            var allGameVendors = AppHelper.GetReportAvailableGameVendors();

            List<MerchantGameVendorSummaryViewModel> listResult = new List<MerchantGameVendorSummaryViewModel>();

            foreach (int eachMerchant in merchantIDs)
            {
                var instance = new MerchantGameVendorSummaryViewModel();
                instance.MerchantID = eachMerchant;
                instance.Items = new List<MerchantGameVendorSummaryItemViewModel>();

                foreach (var eachVendor in allGameVendors)
                {
                    List<int> gameIDs = queryGameVendorMapping.FirstOrDefault(z => z.GameVendorID == eachVendor.ToInteger())?.GameIDs ?? new List<int>();

                    var queryData = queryEachGameSummary.Where(z => gameIDs.Contains(z.GameID) && eachMerchant == z.MerchantID).ToList();
                    int maxPlayer = queryData.Select(z => z.BetPlayer).DefaultIfEmpty().Max();

                    MerchantGameSummaryDataEntity mergeToTotalSum = queryData.CalculateTotalSumMergeToOneObject();
                    var convertToModel = mergeToTotalSum.ReturnInjecteObject<MerchantGameVendorSummaryItemViewModel>();
                    convertToModel.RTP = CommonLogicHelper.CalculateRTP_Percentage(convertToModel.BetAmount, convertToModel.PayoutAmount);
                    convertToModel.GameVendorType = eachVendor.ToInteger();
                    convertToModel.GameVendorTypeText = MultiLanguageTextHelper.GetEnumsCultureText(eachVendor);

                    //迴圈查db 不管了
                    convertToModel.BetPlayer = PlayerGameDataRepository.GetPlayerCountByMerchantAndGameIDs(begin, end, eachMerchant, gameIDs);

                    instance.Items.Add(convertToModel);
                }

                instance.TotalSummary = instance.Items.CalculateTotalSumMergeToOneObject();
                instance.TotalSummary.RTP = CommonLogicHelper.CalculateRTP_Percentage(instance.TotalSummary.BetAmount, instance.TotalSummary.PayoutAmount);

                listResult.Add(instance);
            }

            return listResult;
        }


        public MerchantDailySummaryStatReportInfoViewModel GetMerchantDailySummaryStatReport(int userID, QueryMerchantDailySummaryStatReportReportApiRequestParameterModel filter)
        {
            filter = filter ?? new QueryMerchantDailySummaryStatReportReportApiRequestParameterModel();

            filter.RuleFor(z => z.MerchantID).Rule_IsNotNull().Rule_IsNotZero();

            //防呆處理 & 檢查 (時間)
            SetDefaultBeginEndTimeAndCheckDaysRange(filter, filter.TimeZone);

            //檢查報表開關是否開啟
            SystemConfigSettingRepository.CheckReportPageEnableOrThrowException();

            List<int> merchantIDs = FilterAvailablePermissionMerchantIDs(userID, filter.MerchantID.ToIntegerListCollection());
            int merchantID = merchantIDs.FirstOrDefault();

            var queryMerchant = MerchantRepository.GetByIdOrThrowException(merchantID);

            var mercahnts = string.Join(",", merchantIDs);

            var sp_filter = new Stat_MerchantDailySummaryParameterModelV2
            {
                DateStart = filter.Begin.Value,
                DateEnd = filter.End.Value,
                MerchantIDs = merchantIDs,
                Timezone = filter.TimeZone,
                GameIDs = filter.GameIDs
            };

            List<MerchantDailySummaryResultModel> queryEachDay = StatReportStoreProcedureRepository.GetMerchantDailySummary(sp_filter);

            List<PromotionCostDailySummaryModel> queryPromotionCost = new List<PromotionCostDailySummaryModel>();

            bool isSearchW88ExtraInfo = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.DataQuery_SearchExtraW88Information_InReportPage).SettingValueOfBoolean;

            bool isSearchFishGameExtraInfo = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.DataQuery_SearchExtraFishGameInfoInReportPage).SettingValueOfBoolean;

            bool isForceSearchEventInfo = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.DataQuery_SearchExtraInfoInReportPage).SettingValueOfBoolean;

            DateTime beginUTC = filter.Begin.Value.AddHours(-1 * filter.TimeZone);
            DateTime endUTC = filter.End.Value.AddHours(-1 * filter.TimeZone);

            if (filter.IsIncludeExtraInfo || isForceSearchEventInfo)
            {
                queryPromotionCost = MerchantEventSummaryDataRepository.GetMerchantEventCostDailySummary(merchantID, beginUTC, endUTC, filter.TimeZone);
            }

            bool isQueryOnlyOne = queryEachDay.Count() <= 1;

            //有時候查 8/31 ~ 8/31 23:59 竟然會吐 9/1 
            if (!isQueryOnlyOne)
            {
                queryEachDay = queryEachDay
                .Where(z => z.Date.Date != filter.End.Value.Date) //搜尋 7/1 ~ 8/1 返回資料不會包含 8/1
                .Where(x => x.Date <= filter.End.Value)
                .ToList();
            }

            var rowItems = queryEachDay.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<MerchantDailySummaryStatReportViewModel>();
                    convert.RTP = CommonLogicHelper.CalculateRTP_Percentage(convert.BetAmount, convert.PayoutAmount);
                    convert.Date = x.Date.ToString("yyyy-MM-dd");
                    convert.DateTime = x.Date;
                    convert.Begin = x.Date.AddHours(filter.TimeZone * -1);
                    convert.End = convert.Begin.AddDays(1);
                    convert.MerchantWinLossAmount = x.WinLossAmount * -1;

                    var matchCost = queryPromotionCost.FirstOrDefault(z => z.Date == x.Date);

                    if (matchCost != null)
                    {
                        convert.PromotionCostAmount = matchCost.Amount;
                    }

                    convert.MerchantWinLossAmountWithPromotionCost = convert.MerchantWinLossAmount - convert.PromotionCostAmount.ToAbs();
                    return convert;
                })
                .ToList();

            var totalSummary = rowItems.CalculateTotalSumMergeToOneObject();
            totalSummary.RTP = CommonLogicHelper.CalculateRTP_Percentage(totalSummary.BetAmount, totalSummary.PayoutAmount);

            var result = new MerchantDailySummaryStatReportInfoViewModel
            {
                MerchantID = queryMerchant.MerchantID,
                MerchantName = queryMerchant.Name,
                MerchantCurrency = queryMerchant.Currency,
                IsAnyPromotionCost = queryPromotionCost.Count() > 0,
                IsSearchFishGameExtraInfo = isSearchFishGameExtraInfo || isSearchW88ExtraInfo,
                TotalSummary = totalSummary,
                Items = rowItems
            };

            if (queryPromotionCost.IsNotEmptyNull())
            {
                result.Is4DecimalPlacesDisplay = queryPromotionCost.Any(x => RegexRuleHelper.IsOverThanDecimalPlaces(x.Amount));
            }

            if (isSearchFishGameExtraInfo)
            {
                //搜尋W88額外資訊
                SetExtraFishInfoByApi(merchantID, filter, result);
            }

            if (isSearchW88ExtraInfo)
            {
                //搜尋W88額外資訊 (lobby轉帳版本)
                SetW88ExtraBalanceInfo(merchantID, filter, result);
            }


            var final_totalBet = rowItems.Select(x => x.BetAmount).Sum();
            var final_totalRound = rowItems.Select(x => x.TotalRound).Sum();

            if (final_totalBet == 0 && final_totalRound == 0)
            {
                result.IsShowGoToAgentReportHint = IsChildAgentExist(queryMerchant.MerchantID);
            }

            result.QueryBeginUTC = beginUTC;
            result.QueryEndUTC = endUTC;
            result.ServerTimeTW = GetDisplayCurrentServerTimeTW().ServerTimeTW;
            return result;
        }



        public void SetW88ExtraBalanceInfo(int merchantID, QueryMerchantDailySummaryStatReportReportApiRequestParameterModel filter, MerchantDailySummaryStatReportInfoViewModel viewModel)
        {
            DateTime begin = filter.Begin.Value.AddDays(-1);
            DateTime end = filter.End.Value.AddDays(1);

            var queryStat = W88PlayerDailyWalletRemainingRepository.GetDailySummaryEachMerchant(merchantID.ToListCollection(), begin, end);

            foreach (var item in viewModel.Items)
            {
                var matchData = queryStat.FirstOrDefault(x => x.StatTime == item.DateTime);

                if (matchData != null)
                {
                    item.FishGameRemainingBalance = matchData.Amount;
                }
            }
        }


        private void SetExtraFishInfoByApi(int merchantID, QueryMerchantDailySummaryStatReportReportApiRequestParameterModel filter, MerchantDailySummaryStatReportInfoViewModel viewModel)
        {
            try
            {
                var service = new AGGFishIntegrationService();

                var queryConfig = VendorConfigRepository.GetCacheByVendorTypeAndMerchant(merchantID, GameVendorTypeEnum.AGGFish.ToInteger());

                if (queryConfig == null)
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted);

                var queryThirdApi = service.FetchPlatformDailyReport(new ExternalVendorGetReportRequestParameter
                {
                    VendorConfigID = queryConfig.VendorConfigID,
                    StartTimeUTC = filter.Begin.Value,
                    EndTimeUTC = filter.End.Value
                });

                if (queryThirdApi.result.IsNotEmptyNull())
                {
                    foreach (var item in viewModel.Items)
                    {
                        var matchData = queryThirdApi.result.FirstOrDefault(x => x.dateParse == item.DateTime);

                        if (matchData != null)
                        {
                            item.FishGameRemainingBalance = matchData.inventory_balance_today;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "";
            }

        }


        public MerchantDailySummaryStatReportInfoViewModel GetMerchantEachGameDailySummaryStatReport(int userID, QueryMerchantDailySummaryStatReportReportApiRequestParameterModel filter)
        {
            filter = filter ?? new QueryMerchantDailySummaryStatReportReportApiRequestParameterModel();

            if (!filter.Begin.HasValue)
                filter.Begin = DateTimeHelper.GetCurrentTime(filter.TimeZone).AddDays(-1).To_00_00_00();

            if (!filter.End.HasValue)
                filter.End = filter.Begin.Value.To_23_59_59();

            //不開放查詢太長
            if ((filter.End.Value - filter.Begin.Value).TotalDays >= 63)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge);
            }

            List<int> merchantIDs = FilterAvailablePermissionMerchantIDs(userID, filter.MerchantID.ToIntegerListCollection());

            var mercahnts = string.Join(",", merchantIDs);

            var sp_filter = new Stat_MerchantDailySummaryParameterModelV2
            {
                DateStart = filter.Begin.Value,
                DateEnd = filter.End.Value,
                MerchantIDs = merchantIDs,
                Timezone = filter.TimeZone
            };

            List<MerchantDailySummaryResultModel> query = StatReportStoreProcedureRepository.GetMerchantDailySummary(sp_filter);

            bool isQueryOnlyOne = query.Count() <= 1;

            //有時候查 8/31 ~ 8/31 23:59 竟然會吐 9/1 
            if (!isQueryOnlyOne)
            {
                query = query
                .Where(z => z.Date.Date != filter.End.Value.Date) //搜尋 7/1 ~ 8/1 返回資料不會包含 8/1
                .Where(x => x.Date <= filter.End.Value)
                .ToList();
            }

            var rowItems = query.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<MerchantDailySummaryStatReportViewModel>();
                    convert.RTP = CommonLogicHelper.CalculateRTP_Percentage(convert.BetAmount, convert.PayoutAmount);
                    convert.Date = x.Date.ToString("yyyy-MM-dd");
                    convert.MerchantWinLossAmount = x.WinLossAmount * -1;

                    return convert;
                })
                .ToList();

            var totalSummary = rowItems.CalculateTotalSumMergeToOneObject();
            totalSummary.RTP = CommonLogicHelper.CalculateRTP_Percentage(totalSummary.BetAmount, totalSummary.PayoutAmount);

            return new MerchantDailySummaryStatReportInfoViewModel
            {
                TotalSummary = totalSummary,
                Items = rowItems
            };
        }
        public VendorConfigDateMemoListResult GetFinanceCompareDateMemo(int userID, QueryVendorConfigFinanceCompareRequestModel filter)
        {
            filter.RuleFor(z => z.EndTime).Rule_NotNullDateTime();
            filter.RuleFor(z => z.BeginTime).Rule_NotNullDateTime().Rule_StartTimeMustSmallThanEndTime(filter.EndTime);

            var beginTW = filter.BeginTime.Value.AddHours(8);
            var endTW = filter.EndTime.Value.AddHours(8);

            var splitDate = beginTW.SplitToDateListByHourRange(filter.EndTime.Value, 24);

            var queryMemoList = OneToManyCustomizedSettingItemRepository.GetVendorConfigReportDateMemo(OneToManyCustomizedSettingTypeEnum.ThirdPartyAccounting_MemoInformation, true);

            List<string> myMemoText = new List<string>();

            var items = new List<VendorConfigDateMemoItemModel>();

            foreach (var date in splitDate)
            {
                string myDateText = date.ToString("yyyy-MM-dd");

                var listOfMatch = queryMemoList
                    .Where(x => x.SubCategory == myDateText)
                    .ToList();

                if (listOfMatch.IsNotEmptyNull())
                {
                    foreach (var matchItem in listOfMatch)
                    {
                        items.Add(new VendorConfigDateMemoItemModel
                        {
                            VendorConfigID = matchItem.VendorConfigID,
                            VendorConfigName = matchItem.Name,
                            Currency = matchItem.SupportCurrency,
                            Date = myDateText,
                            Memo = matchItem.SettingValue
                        });
                    }
                }
            }

            return new VendorConfigDateMemoListResult
            {
                BeginText = beginTW.ToString("yyyy-MM-dd"),
                EndText = endTW.ToString("yyyy-MM-dd"),
                Items = items
            };
        }


        public List<VendorConfigFinanceCompareBundleViewModel> GetMultipleFinanceCompareStatReportByGameVendor(int userID, QueryMultipleFinanceCompareRequestModel model)
        {
            model.RuleFor(x => x.GameVendorID).Rule_NotMinus();

            var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorIDs = model.GameVendorID.ToListCollection(),
                IsTestEnvironment = false
            });

            List<VendorConfigFinanceCompareBundleViewModel> listResult = new List<VendorConfigFinanceCompareBundleViewModel>();

            foreach (var item in queryVendorConfig)
            {
                var myModel = GetVendorConfigFinanceCompareStatReport(userID, new QueryVendorConfigFinanceCompareRequestModel
                {
                    BeginTime = model.BeginTime,
                    EndTime = model.EndTime,
                    VendorConfigID = item.VendorConfigID,
                    IsSkipQueryExtraData = true,
                    IsIgnoreCrossDateWager = model.IsIgnoreCrossDateWager,
                });

                if(item.VendorConfigID == 50297)
                {
                    string debug = "";
                }

                listResult.Add(myModel);
            }

            return listResult;
        }



        public VendorConfigFinanceCompareBundleViewModel GetVendorConfigFinanceCompareStatReport(int userID, QueryVendorConfigFinanceCompareRequestModel filter)
        {
            filter.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();
            filter.RuleFor(z => z.EndTime).Rule_NotNullDateTime();
            filter.RuleFor(z => z.BeginTime).Rule_NotNullDateTime().Rule_StartTimeMustSmallThanEndTime(filter.EndTime);

            var queryVendorConfig = VendorConfigRepository.GetByIdOrThrowException(filter.VendorConfigID);

            //Fuma強制忽略跨日單
            if(queryVendorConfig.VendorID == (int)GameVendorTypeEnum.Fuma)
            {
                filter.IsIgnoreCrossDateWager = true;
            }

            var queryMerchants = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                IsExcludeInvalidCurrency = true,
                VendorConfigIDs = filter.VendorConfigID.ToListCollection()
            });

            var excludeMerchants = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                IsExcludeInvalidCurrencyOnly = true,
                VendorConfigIDs = filter.VendorConfigID.ToListCollection()
            });

            var hiddenSetting = VendorConfigSettingRepository.GetParseModelByUniqueKey(filter.VendorConfigID, VendorConfigSettingEnum.GameVendor_HiddenTotalAmountFinanceCompare);

            string targetMonthText = filter.BeginTime.Value.AddHours(8).ToString("yyyy-MM-dd");
            string targetMonthText2 = filter.BeginTime.Value.AddHours(8).ToString("yyyyMMdd");

            //dev環境直接吐假資料
            #region dev環境直接吐假資料

            if (SettingHelper.IsDev && !SettingHelper.IsProductionDbConnection && !filter.IsDisableFakeData)
            {
                int year = 1999;
                int month = 7;
                var instanceFake = InstanceEmptyFakeModel(new DateTime(year, month, 5));
                instanceFake.IsCompareNotEqual_ButGapSmall_WithAGGSettleDate = true;

                var result = new VendorConfigFinanceCompareBundleViewModel
                {
                    DebugSQL = "select * from test",
                    Merchants = new List<MerchantBriefViewModel>
                    {
                        new MerchantBriefViewModel() { MerchantID = 175001 , MerchantName = "天馬管理團隊" }
                    },
                    Items = new List<VendorConfigFinanceCompareViewModel>
                    {
                        InstanceEmptyFakeModel(new DateTime(year , month , 1)),
                        InstanceEmptyFakeModel(new DateTime(year , month , 2)),
                        InstanceEmptyFakeModel(new DateTime(year , month , 3)),
                        InstanceEmptyFakeModel(new DateTime(year , month , 4)),
                        instanceFake
                    }
                };

                result.VendorConfigName = queryVendorConfig.Name;
                result.TotalSummary = result.Items.Select(z => z.PgsStatDB).CalculateTotalSumMergeToOneObject();
                result.TotalSummaryAfterAdjustment = result.Items.Select(z => z.PgsAfterAdjustment).CalculateTotalSumMergeToOneObject();

                SetMemoInformation(filter.VendorConfigID, result);
                return result;
            }

            #endregion

            var viewModel = new VendorConfigFinanceCompareBundleViewModel();
            viewModel.VendorConfigName = queryVendorConfig.Name;
            viewModel.GameVendorID = queryVendorConfig.VendorID;
            viewModel.Items = new List<VendorConfigFinanceCompareViewModel>();
            viewModel.TotalSummary = new SettleDateDiffModel();
            viewModel.TotalSummaryAfterAdjustment = new SettleDateDiffModel();
            viewModel.TotalSummaryThirdParty = new SettleDateDiffModel();
            viewModel.Currency = queryVendorConfig.SupportCurrency;
            viewModel.IsHideTotalSum = hiddenSetting.SettingValueOfBoolean;
            viewModel.IsMultiCurrency = queryVendorConfig.IsMultiCurrency;

            viewModel.AssignRootAgentID = VendorConfigSettingRepository.GetSpecificVendorConfigSetting(queryVendorConfig.VendorConfigID, VendorConfigSettingEnum.OtherKeySettingValue3)?.Value;
            viewModel.AssignVendorID = VendorConfigSettingRepository.GetAssignVendorIdByVendorConfigID(queryVendorConfig.VendorConfigID)?.Value;
            viewModel.HintWarningMessages = new List<string>();

            if (queryMerchants.IsEmptyOrNull())
                return viewModel;

            int gameVendorType = queryMerchants.Select(z => z.VendorID).Distinct().FirstOrDefault();
            List<int> merchantIDs = queryMerchants.Select(z => z.MerchantID).ToList();

            //只有AGG才需要有此邏輯 
            if (queryVendorConfig.VendorID == GameVendorTypeEnum.AGG.ToInteger())
            {
                var queryJpMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
                {
                    Type = MerchantTypeEnum.JackpotMerchant.ToInteger()
                });

                var jpMerchants = queryJpMerchant.Where(x => merchantIDs.Contains(x.MerchantID)).ToList();

                //如果有拉JP專用商戶 (需要過濾掉數字)
                if (jpMerchants.IsNotEmptyNull())
                {
                    List<int> jpMerchantIDs = jpMerchants.Select(x => x.MerchantID).ToList();

                    var queryJpTotalSummary = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumOnDuration(jpMerchantIDs, filter.BeginTime.Value, filter.EndTime.Value);

                    viewModel.JackpotMerchantWinLossAdjustAmount = queryJpTotalSummary.Select(x => x.WinLossAmount).Sum();
                }
            }

            List<int> queryGameIDsOfVendor = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameVendorTypes = gameVendorType.ToListCollection()
                })
                .Select(z => z.GameID)
                .ToList();


            var vendorDef = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(queryVendorConfig.VendorID);

            //最主要的SQL查詢 根據日期group by 所有商戶數據合併
            //最主要的SQL查詢 根據日期group by 所有商戶數據合併
            //最主要的SQL查詢 根據日期group by 所有商戶數據合併
            var queryStatReport = StatReportStoreProcedureRepository.GetGameVendorStatSummary_ComapreThreeSource(new QueryGameVendorStatSummaryThreeSourceCompareModel
            {
                Begin = filter.BeginTime,
                End = filter.EndTime,
                IsSkipJoinDailyTable = filter.IsSkipQueryExtraData ? true : false,
                MerchantIDs = merchantIDs,
                ExcludeMerchantIDs = excludeMerchants.Select(x => x.MerchantID).ToList(),
                VendorConfigID = filter.VendorConfigID,
                MinGameID = gameVendorType,
                MaxGameID = gameVendorType + 9999,
                AggregativeTimezone = vendorDef.SettlementReportTimezone,
            });

            //額外查出有migrate的遊戲商 去額外扣除或校正
            //額外查出有migrate的遊戲商 去額外扣除或校正
            //額外查出有migrate的遊戲商 去額外扣除或校正
            var correctionData = GetVendorConfigMigrateCorrectionData(filter.VendorConfigID, queryVendorConfig.VendorID, filter.BeginTime.Value, filter.EndTime.Value, vendorDef.SettlementReportTimezone);

            CheckVendorConfigMigrateAndCalculateCorrection(queryStatReport.ListResult , correctionData);

            bool isAnyData = queryStatReport.ListResult.Select(x => x.Stat_BetAmount).Sum() > 0;

            List<PerGameDetailLogEntity> queryAccountingIgnoreTags = new List<PerGameDetailLogEntity>();
            List<PerGameDetailLogEntity> querySettleDiffPerGame = new List<PerGameDetailLogEntity>();

            if (isAnyData)
            {
                if(filter.IsIgnoreCrossDateWager)
                {

                }
                else
                {
                    queryAccountingIgnoreTags = PerGameDetailLogRepository.GetPerGameBySystemTag_V2
                        (
                            SystemTagEnum.PerGameDetailLog_AccountingIgnoreAtThirdFinanceCompare,
                            filter.BeginTime.Value.AddHours(-2),
                            filter.EndTime.Value.AddDays(1), //如果沒+1 會撈不到最後一天的部分校正注單
                            filter.VendorConfigID
                        );

                    querySettleDiffPerGame = PerGameDetailLogRepository.GetPerGameBySystemTag_V2
                        (
                            SystemTagEnum.PerGameDetailLog_ThirdSettleDateNotEqual,
                            filter.BeginTime.Value.AddHours(-2),
                            filter.EndTime.Value.AddDays(1), //如果沒+1 會撈不到最後一天的部分校正注單
                            filter.VendorConfigID
                        );
                }
            }

            //看執行計畫 感覺部分欄位用程式碼自己過濾會比較好...?
            querySettleDiffPerGame = querySettleDiffPerGame
                .Where(x => merchantIDs.Contains(x.MerchantID))
                .Where(x => queryGameIDsOfVendor.Contains(x.GameID))
                .ToList();

            DateTime beginTW = filter.BeginTime.Value.AddHours(8);
            DateTime endTW = filter.EndTime.Value.AddHours(8);

            List<DateTime> dateList = DateTimeHelper.GetDateListBetweenTwoDate(beginTW, endTW, isExcludeLastEqualDate: true);

            var eachDateSummary = new List<Stat_GameVendorStatSummaryThreeSourceCompareModel>();

            foreach (var date in dateList)
            {
                var summaryItem = queryStatReport.ListResult.FirstOrDefault(x => x.StatDate.Date == date.Date);

                if (summaryItem == null)
                {
                    summaryItem = new Stat_GameVendorStatSummaryThreeSourceCompareModel
                    {
                        StatDate = date
                    };
                }

                eachDateSummary.Add(summaryItem);
            }

            List<VendorConfigFinanceCompareViewModel> viewModels = eachDateSummary
                .Select(z =>
                {
                    var myConvert = z.ReturnInjecteObject<VendorConfigFinanceCompareViewModel>();

                    myConvert.PgsStatDB = new SettleDateDiffModel
                    {
                        TotalRound = z.Stat_TotalRound,
                        TotalBetAmount = z.Stat_BetAmount,
                        TotalWinLossAmount = z.Stat_WinLossAmount,
                        TotalValidAmount = z.Stat_ValidAmount
                    };

                    myConvert.PgsAggregateByThirdSettleDate = new SettleDateDiffModel
                    {
                        TotalRound = z.ThirdSettleDate_TotalRound,
                        TotalBetAmount = z.ThirdSettleDate_BetAmount,
                        TotalWinLossAmount = z.ThirdSettleDate_WinLossAmount,
                    };

                    myConvert.FetchThirdPartyStatisticReport = new SettleDateDiffModel
                    {
                        TotalRound = z.ThirdFetch_TotalRound,
                        TotalBetAmount = z.ThirdFetch_TotalBetAmount,
                        TotalWinLossAmount = z.ThirdFetch_TotalWinLoss,
                    };

                    return myConvert;
                })
                .ToList();

            foreach (var item in viewModels)
            {
                var pgsSettleThisDate_But_ThirdSettleOtherDate = querySettleDiffPerGame
                    .Where(z => z.GameTime.AddHours(SystemConstant.PgsDefaultSettlementTimezone).Date == item.StatDate)
                    .Where(z => z.ThirdSettleDate != item.StatDate)
                    .ToList();

                var thirdSettleThisDate_But_PgsOtherDate = querySettleDiffPerGame
                    .Where(z => z.ThirdSettleDate == item.StatDate)
                    .Where(z => z.GameTime.AddHours(SystemConstant.PgsDefaultSettlementTimezone).Date != item.StatDate)
                    .ToList();

                var accountingIgnoreRecords = queryAccountingIgnoreTags
                    .Where(z => z.GameTime.AddHours(SystemConstant.PgsDefaultSettlementTimezone).Date == item.StatDate)
                    .ToList();

                item.PgsSettleThisDate_But_ThirdSettleOtherDate = new SettleDateDiffModel();
                item.ThirdSettleThisDate_But_PgsSettleOtherDate = new SettleDateDiffModel();
                item.AccountingIgnoreRecordSummary = new SettleDateDiffModel();

                //校正注單的數值設置
                SetDiffSummaryObjectValue(pgsSettleThisDate_But_ThirdSettleOtherDate, item.PgsSettleThisDate_But_ThirdSettleOtherDate);
                SetDiffSummaryObjectValue(thirdSettleThisDate_But_PgsOtherDate, item.ThirdSettleThisDate_But_PgsSettleOtherDate);
                SetDiffSummaryObjectValue(accountingIgnoreRecords, item.AccountingIgnoreRecordSummary);

                //根據校正注單 調整數字(1)
                item.PgsAfterAdjustment = new SettleDateDiffModel
                {
                    TotalRound = item.PgsStatDB.TotalRound
                        + item.ThirdSettleThisDate_But_PgsSettleOtherDate.TotalRound
                        - item.PgsSettleThisDate_But_ThirdSettleOtherDate.TotalRound
                        - item.AccountingIgnoreRecordSummary.TotalRound,

                    TotalBetAmount = item.PgsStatDB.TotalBetAmount
                        + item.ThirdSettleThisDate_But_PgsSettleOtherDate.TotalBetAmount
                        - item.PgsSettleThisDate_But_ThirdSettleOtherDate.TotalBetAmount
                        - item.AccountingIgnoreRecordSummary.TotalBetAmount,

                    TotalWinLossAmount = item.PgsStatDB.TotalWinLossAmount
                        + item.ThirdSettleThisDate_But_PgsSettleOtherDate.TotalWinLossAmount
                        - item.PgsSettleThisDate_But_ThirdSettleOtherDate.TotalWinLossAmount
                        - item.AccountingIgnoreRecordSummary.TotalWinLossAmount,

                    TotalValidAmount = item.PgsStatDB.TotalValidAmount
                        + item.ThirdSettleThisDate_But_PgsSettleOtherDate.TotalValidAmount
                        - item.PgsSettleThisDate_But_ThirdSettleOtherDate.TotalValidAmount
                        - item.AccountingIgnoreRecordSummary.TotalValidAmount,
                };

                //根據校正注單 調整數字(2)
                item.PgsAggregateByThirdSettleDate = new SettleDateDiffModel
                {
                    TotalRound = item.PgsAggregateByThirdSettleDate.TotalRound - item.AccountingIgnoreRecordSummary.TotalRound,
                    TotalBetAmount = item.PgsAggregateByThirdSettleDate.TotalBetAmount - item.AccountingIgnoreRecordSummary.TotalBetAmount,
                    TotalWinLossAmount = item.PgsAggregateByThirdSettleDate.TotalWinLossAmount - item.AccountingIgnoreRecordSummary.TotalWinLossAmount,
                };

                item.TotalAdjustRecordCount = item.ThirdSettleThisDate_But_PgsSettleOtherDate.TotalRound +
                    item.PgsSettleThisDate_But_ThirdSettleOtherDate.TotalRound +
                    item.AccountingIgnoreRecordSummary.TotalRound;

                item.IsAnyIgnoreWagers = accountingIgnoreRecords.CountOrZero() > 0;
                item.IsAnyAdjustmentByDiffDate = item.TotalAdjustRecordCount > 0;

                //數據誤差boolean 標記判定
                //數據誤差boolean 標記判定
                //數據誤差boolean 標記判定
                item.IsCompareCorrect_ByGameVendorSettleDate = IsCompareValueEqual(item.PgsAfterAdjustment, item.FetchThirdPartyStatisticReport);
                item.IsRoundNotEqualOnly_ByGameVendorSettleDate = IsRoundNotEqualOnly(item.PgsAfterAdjustment, item.FetchThirdPartyStatisticReport);
                item.IsCompareCorrect_AfterAdjustment = IsCompareValueEqual(item.PgsAfterAdjustment, item.FetchThirdPartyStatisticReport);

                //如果只有局數有誤差 顯示另一種錯誤訊息
                if (item.IsRoundNotEqualOnly_ByGameVendorSettleDate)
                {
                    item.IsCompareCorrect_ByGameVendorSettleDate = true;
                }

                decimal winLossDiff = (item.PgsAfterAdjustment.TotalWinLossAmount - item.FetchThirdPartyStatisticReport.TotalWinLossAmount).ToAbs();
                item.IsCompareNotEqual_ButGapSmall_WithAGGSettleDate = winLossDiff > MIN_DIFF_VALUE && winLossDiff <= 5;

                //如果只是很微小的差距 就不顯示 [三方數據比對不相符] 的警告了
                if (item.IsCompareNotEqual_ButGapSmall_WithAGGSettleDate)
                {
                    item.IsCompareCorrect_ByGameVendorSettleDate = true;
                }


                item.IsFetchDataAllZero = item.PgsAfterAdjustment.TotalRound != 0 && (item.FetchThirdPartyStatisticReport.TotalRound == 0 && item.FetchThirdPartyStatisticReport.TotalBetAmount == 0) ;

                item.Diff_AfterAdjustment_vs_AggregateByThirdSettleDate = new SettleDateDiffModel
                {
                    TotalRound = item.PgsAfterAdjustment.TotalRound - item.PgsAggregateByThirdSettleDate.TotalRound,
                    TotalBetAmount = item.PgsAfterAdjustment.TotalBetAmount - item.PgsAggregateByThirdSettleDate.TotalBetAmount,
                    TotalWinLossAmount = (item.PgsAfterAdjustment.TotalWinLossAmount - item.PgsAggregateByThirdSettleDate.TotalWinLossAmount).ToAbs()
                };

                item.Diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport = new SettleDateDiffModel
                {
                    TotalRound = item.PgsAfterAdjustment.TotalRound - item.FetchThirdPartyStatisticReport.TotalRound,
                    TotalBetAmount = item.PgsAfterAdjustment.TotalBetAmount - item.FetchThirdPartyStatisticReport.TotalBetAmount,
                    TotalWinLossAmount = (item.PgsAfterAdjustment.TotalWinLossAmount - item.FetchThirdPartyStatisticReport.TotalWinLossAmount) //2026/06 改成不要絕對值 Abs
                };
            }

            DateTime today = DateTimeHelper.GetCurrentTime(8).Date;

            //當日的數字尚未精準
            foreach (var item in viewModels)
            {
                if (item.StatDate == today)
                {
                    item.IsToday = true;
                }
            }

            viewModel.Items = viewModels;


            //手動人工調整值 (假如有注單異動 或 出現帳務差異)
            //手動人工調整值 (假如有注單異動 或 出現帳務差異)
            //手動人工調整值 (假如有注單異動 或 出現帳務差異)
            var emptyModel = new SettleDateDiffModel();

            string tartgetSubCategory = $"{filter.VendorConfigID}_{targetMonthText2}";

            var queryExtraSetting = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.ThirdPartyAccounting_WinLossAdjustment)
                .Where(x => x.SubCategory == tartgetSubCategory)
                .FirstOrDefault();

            if (queryExtraSetting != null && queryExtraSetting.SettingValue.IsNumeric())
            {
                decimal adjustValue = decimal.Parse(queryExtraSetting.SettingValue);

                viewModel.Items.Add(new VendorConfigFinanceCompareViewModel
                {
                    StatDate = new DateTime(1970, 1, 1),
                    PgsStatDB = emptyModel,
                    FetchThirdPartyStatisticReport = emptyModel,
                    PgsAggregateByThirdSettleDate = emptyModel,
                    PgsAfterAdjustment = new SettleDateDiffModel
                    {
                        TotalBetAmount = 0,
                        TotalRound = 0,
                        TotalValidAmount = 0,
                        TotalWinLossAmount = adjustValue,
                    },
                    TotalAdjustRecordCount = 0,
                });
            }


            viewModel.VendorConfigID = filter.VendorConfigID;

            double timeZone = 8;

            foreach (var item in viewModel.Items)
            {
                item.BeginUTC = item.StatDate.AddHours(-1 * timeZone);
                item.EndUTC = item.BeginUTC.AddDays(1);
            }

            //可以指定參數 不查詢特定資料(輕量化查詢, 針對帳單匯出改善一些效能)
            if (!filter.IsSkipQueryExtraData)
            {
                var queryMerchantInfoDic = MerchantRepository
                    .GetByLargeAmountIDs(merchantIDs)
                    .ToDictionary(x => x.MerchantID, x => x);

                viewModel.Merchants = merchantIDs
                    .Select(id =>
                    {
                        var queryMerchant = queryMerchantInfoDic[id];
                        return new MerchantBriefViewModel
                        {
                            MerchantID = queryMerchant.MerchantID,
                            MerchantName = queryMerchant.Name,
                            Currency = queryMerchant.Currency
                        };
                    })
                    .ToList();

                List<MerchantGameSummaryDataEntity> queryEachMerchantSummary = new List<MerchantGameSummaryDataEntity>();

                var splitSize = merchantIDs.SplitListByArraySize(1500);

                foreach(var smallArray in splitSize)
                {
                    var query = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumOnDuration(smallArray, filter.BeginTime.Value, filter.EndTime.Value, queryGameIDsOfVendor);
                    queryEachMerchantSummary.AddRange(query);
                }

                viewModel.MerchantsDataExist = queryEachMerchantSummary
                      .Select(x =>
                      {
                          var queryMerchant = queryMerchantInfoDic[x.MerchantID];
                          return new MerchantBriefViewModel
                          {
                              MerchantID = queryMerchant.MerchantID,
                              MerchantName = queryMerchant.Name,
                              Currency = queryMerchant.Currency
                          };
                      })
                      .ToList();

                int merchantCount = queryEachMerchantSummary.Select(x => x.MerchantID).Distinct().Count();

                viewModel.EachMerchantTotalSumCustomizedDisplayFormat = new CustomizedColumnDynamicQueryResultViewModel
                {
                    ExtraDisplayName = $"每個商戶本月加總數值 (*{merchantCount})",
                    Items = new List<Dictionary<string, object>>()
                };

                foreach (var item in queryEachMerchantSummary.GroupBy(x => x.MerchantID))
                {
                    int merchantID = item.Key;

                    var myTotalSum = item.CalculateTotalSumMergeToOneObject();

                    Dictionary<string, object> dic = new Dictionary<string, object>();

                    var queryMerchant = queryMerchantInfoDic[merchantID];

                    //若有新增欄位 下面那段footerSum 也要記得加
                    dic.Add(EazyLabelTextEnum.MerchantID.ToString().ToCamelCase(), merchantID);
                    dic.Add(EazyLabelTextEnum.MerchantName.ToString().ToCamelCase(), queryMerchant?.Name);
                    dic.Add(EazyLabelTextEnum.TotalRound.ToString().ToCamelCase(), myTotalSum.TotalRound);
                    dic.Add(EazyLabelTextEnum.BetAmount.ToString().ToCamelCase(), myTotalSum.BetAmount);
                    dic.Add(EazyLabelTextEnum.PayoutAmount.ToString().ToCamelCase(), myTotalSum.PayoutAmount);
                    dic.Add(EazyLabelTextEnum.WinLossAmount.ToString().ToCamelCase(), myTotalSum.WinLossAmount);
                    dic.Add(EazyLabelTextEnum.RTP.ToString().ToCamelCase(), AppHelper.CalculateRTP_Percentage(myTotalSum.BetAmount , myTotalSum.PayoutAmount));

                    viewModel.EachMerchantTotalSumCustomizedDisplayFormat.Items.Add(dic);
                }

                viewModel.EachMerchantTotalSumCustomizedDisplayFormat.Items = viewModel.EachMerchantTotalSumCustomizedDisplayFormat
                    .Items
                    .OrderByDescending(x => x["betAmount"])
                    .ToList();

                var footerSum = queryEachMerchantSummary.CalculateTotalSumMergeToOneObject();

                Dictionary<string, object> footerDic = new Dictionary<string, object>();

                footerDic.Add(EazyLabelTextEnum.MerchantID.ToString().ToCamelCase(), "加總");
                footerDic.Add(EazyLabelTextEnum.MerchantName.ToString().ToCamelCase(), "");
                footerDic.Add(EazyLabelTextEnum.TotalRound.ToString().ToCamelCase(), footerSum.TotalRound);
                footerDic.Add(EazyLabelTextEnum.BetAmount.ToString().ToCamelCase(), footerSum.BetAmount);
                footerDic.Add(EazyLabelTextEnum.PayoutAmount.ToString().ToCamelCase(), footerSum.PayoutAmount);
                footerDic.Add(EazyLabelTextEnum.WinLossAmount.ToString().ToCamelCase(), footerSum.WinLossAmount);
                footerDic.Add(EazyLabelTextEnum.RTP.ToString().ToCamelCase(), AppHelper.CalculateRTP_Percentage(footerSum.BetAmount, footerSum.PayoutAmount));

                viewModel.EachMerchantTotalSumCustomizedDisplayFormat.Items.Add(footerDic);

                DataModelConvertHelper.ConvertToAllColumnsViewModel(viewModel.EachMerchantTotalSumCustomizedDisplayFormat);               
            }

            viewModel.TotalSummary = viewModel.Items.Select(z => z.PgsStatDB).CalculateTotalSumMergeToOneObject();
            viewModel.TotalSummaryAfterAdjustment = viewModel.Items.Select(z => z.PgsAfterAdjustment).CalculateTotalSumMergeToOneObject();
            viewModel.TotalSummaryThirdParty = viewModel.Items.Select(z => z.FetchThirdPartyStatisticReport).CalculateTotalSumMergeToOneObject();

            string sqlTime_begin = filter.BeginTime.Value.AddHours(8).ToString("yyyy-MM-dd");
            string sqlTime_end = filter.BeginTime.Value.AddHours(8 + 24).ToString("yyyy-MM-dd");

            viewModel.DebugSQL =Environment.NewLine +
                $@"
DECLARE @begin datetime = DATEADD(hour , -8 ,'{sqlTime_begin}')
DECLARE @end   datetime = DATEADD(hour , -8 ,'{sqlTime_end}')

SELECT TOP 50 * FROM [PGS_GameLog].[dbo].[PerGameDetailLog] WITH (NOLOCK)
WHERE GameTime >= @begin AND GameTime < @end AND MerchantID in
(
	SELECT MerchantID FROM [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] WITH (NOLOCK)
	WHERE VendorConfigID = {filter.VendorConfigID}
) 


SELECT PlayerID , sum(betAmount) , sum(UpdateAmount) FROM [PGS_GameLog].[dbo].[PerGameDetailLog] WITH (NOLOCK)
WHERE GameTime >= @begin AND GameTime < @end AND MerchantID in
(
	SELECT MerchantID FROM [PGS_Master].[ThirdParty].[VendorConfigMerchantMapping] WITH (NOLOCK)
	WHERE VendorConfigID = {filter.VendorConfigID}
) 
GROUP BY PlayerID
ORDER BY PlayerID

{Environment.NewLine}{Environment.NewLine}" + 
queryStatReport.SqlWithParameterValue;


            //這邊的輸贏是... 負數=玩家輸錢
            decimal totalPayout = viewModel.TotalSummary.TotalBetAmount + viewModel.TotalSummary.TotalWinLossAmount;
            viewModel.TotalRTP = AppHelper.CalculateRTP_Percentage(viewModel.TotalSummary.TotalBetAmount, totalPayout);

            if (queryVendorConfig.VendorID == GameVendorTypeEnum.AGGFish.ToInteger())
            {
                var matchSetting = VendorConfigSettingRepository.GetSpecificVendorConfigSetting(queryVendorConfig.VendorConfigID, VendorConfigSettingEnum.OtherKeySettingValue1);
                viewModel.AssignVendorID = matchSetting?.Value;
            }

            if (string.IsNullOrWhiteSpace(viewModel.AssignVendorID))
            {
                viewModel.ExportFileNamePrefix = "records_";
            }
            else
            {
                viewModel.ExportFileNamePrefix = viewModel.AssignVendorID.Trim().Replace("-", "").Replace("_", "") + "_";
            }

            //前台會根據sequence顯示最多N筆
            int sequence = 0;
            viewModel.Merchants = viewModel.Merchants ?? new List<MerchantBriefViewModel>();
            viewModel.Merchants.ForEach(x => x.Sequence = sequence++);
            viewModel.ExtraTags = new List<string>();
            viewModel.ButtonLabelTags = new List<string>();

            var now = DateTimeHelper.GetCurrentTime();

            if ((now - filter.BeginTime.Value).TotalDays >= 93)
            {
                viewModel.IsHideExportButton = true;
                viewModel.ExtraTags.Add("數據時間超過3個月");
                viewModel.ExtraTags.Add("部分校正注單已刪除");
            }

            SetMemoInformation(filter.VendorConfigID, viewModel);

            if (filter.IsDescendingOrder)
            {
                viewModel.Items = viewModel.Items.OrderByDescending(x => x.StatDate).ToList();
            }

            if (string.IsNullOrWhiteSpace(viewModel.Currency))
            {
                viewModel.Currency = viewModel.Merchants.FirstOrDefault()?.Currency;
            }

            decimal totalBetAmount = viewModel.Items.Select(x => x.PgsAfterAdjustment.TotalBetAmount).Sum();

            if (totalBetAmount > 0)
            {
                if (string.IsNullOrWhiteSpace(queryVendorConfig.SupportCurrency))
                {
                    viewModel.EachCurrencyTotalSum = MerchantGameSummaryDataRepository.GetEachCurrencyTotalSumReportByVendorConfigID(filter.VendorConfigID, filter.BeginTime.Value, filter.EndTime.Value);

                    //針對轉移vendorConfig 的資料做校正
                    //針對轉移vendorConfig 的資料做校正
                    //針對轉移vendorConfig 的資料做校正
                    CheckVendorConfigMigrateAndCalculateCorrection(viewModel.EachCurrencyTotalSum, correctionData);          
                    
                    viewModel.EachCurrencyTotalSum = viewModel.EachCurrencyTotalSum ?? new List<EachCurrencyStatReportTotalSumQueryModel>();
                }
            }

            //如果是沒限定貨幣的配置
            if (viewModel.MerchantsDataExist.IsNotEmptyNull())
            {
                List<int> dataExistMerchantIDs = viewModel.MerchantsDataExist.Select(z => z.MerchantID).ToList();

                var dataExistMerchants = MerchantRepository.GetByLargeAmountIDs(dataExistMerchantIDs);

                var noneProductionMerchant = dataExistMerchants.Where(x => x.Type != (int)MerchantTypeEnum.Plaform_ProductionEnvironment).ToList();

                if (noneProductionMerchant.CountOrZero() > 0)
                {
                    string tagMsg = $"有商戶分類為【非正式】  {string.Join("、", noneProductionMerchant.Select(x => x.MerchantID))}";
                    string notifyMsg = $"三方帳務：{filter.VendorConfigID} ,{tagMsg}";

                    viewModel.HintWarningMessages.Add(tagMsg);
                    SendMessageToMultipleChannel(notifyMsg);
                }
            }

            viewModel.GameVendorReportTimezone = vendorDef.SettlementReportTimezone;
            viewModel.TotalDiff = Math.Round(viewModel.Items.Select(x => x.Diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport).Select(x => x.TotalWinLossAmount).Sum() , 2);
            viewModel.TotalGameVendorSideWinloss = Math.Round(viewModel.Items.Select(x => x.FetchThirdPartyStatisticReport).Select(x => x.TotalWinLossAmount).Sum() , 2);
            return viewModel;
        }

              
        private void SetDiffSummaryObjectValue(List<PerGameDetailLogEntity> matchPerGames, SettleDateDiffModel targetProperty)
        {
            if (matchPerGames.IsNotEmptyNull())
            {
                var target = targetProperty;

                target.PerGameDetailLogIDs = matchPerGames.Select(z => z.Id).ToList();
                target.TotalRound = matchPerGames.Count();
                target.TotalBetAmount = matchPerGames.Sum(z => z.BetAmount);
                target.TotalWinLossAmount = matchPerGames.Sum(z => z.UpdateAmount);
                target.TotalValidAmount = matchPerGames.Sum(z => z.ValidAmount);
            }
        }


        private void SetDefaultBeginEndTimeAndCheckDaysRange<T>(T filter, double timezone, int maxDaysRange = 93) where T : class, IFilterBeginEndTime
        {
            if (!filter.Begin.HasValue)
                filter.Begin = DateTimeHelper.GetCurrentTime(timezone).AddDays(-1).To_00_00_00();

            if (!filter.End.HasValue)
                filter.End = filter.Begin.Value.To_23_59_59();

            if (filter.Begin == filter.End)
                filter.End = filter.Begin.Value.AddDays(1);

            filter.RuleFor(x => x.Begin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.End, maxDaysRange);
        }


        private void SetMemoInformation(int vendorConfigID, VendorConfigFinanceCompareBundleViewModel viewModel)
        {
            var queryMemoInfo = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                UniqueOnwerQuery = new UniqueParentCustomizedSettingQuery
                {
                    OwnerID = vendorConfigID,
                    Type = OneToManyCustomizedSettingTypeEnum.ThirdPartyAccounting_MemoInformation
                }
            });

            foreach (var item in viewModel.Items)
            {
                var matchSetting = queryMemoInfo.FirstOrDefault(x => x.SubCategory == item.StatDateText);

                if (matchSetting != null)
                {
                    item.ExtraMemoInfo = matchSetting.SettingValue;
                }
            }
        }

        private bool IsRoundNotEqualOnly(SettleDateDiffModel source, SettleDateDiffModel target)
        {
            return source.TotalRound != target.TotalRound && (source.TotalWinLossAmount - target.TotalWinLossAmount).ToAbs() <= MIN_DIFF_VALUE;
        }

        private bool IsCompareValueEqual(SettleDateDiffModel source, SettleDateDiffModel target)
        {
            if(source.TotalRound == 0 || target.TotalRound == 0)
            {
                return (source.TotalWinLossAmount - target.TotalWinLossAmount).ToAbs() <= MIN_DIFF_VALUE;
            }

            //局數、輸贏都會列入考量
            return source.TotalRound == target.TotalRound && (source.TotalWinLossAmount - target.TotalWinLossAmount).ToAbs() <= MIN_DIFF_VALUE;
        }


        public GameVendorBetSummaryReportViewModel GetGameVendorBetSummary(int userID, QueryGameVendorBetSummaryApiRequestParameterModel filter)
        {
            var query = StatReportStoreProcedureRepository.GetGameVendorBetSummary(filter);

            var queryTestEnv = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                IsTestEnvironment = true
            });

            //過濾掉 標記測試環境的
            if (queryTestEnv.IsNotEmptyNull())
            {
                query = query
                    .Where(z => !queryTestEnv.Any(x => x.MerchantID == z.MerchantID && x.VendorID == z.GameVendorType))
                    .ToList();
            }

            var groupByCurrency = query
                .GroupBy(z => new { z.GameVendorType, z.ChargeCurrency })
                .Select(z =>
                {
                    var sumOfGroup = z.ToList().CalculateTotalSumMergeToOneObject().ReturnInjecteObject<GameVendorBetSummaryReportItemViewModel>();
                    sumOfGroup.ChargeCurrency = z.Key.ChargeCurrency;
                    sumOfGroup.GameVendorType = z.Key.GameVendorType;
                    sumOfGroup.RTP = CommonLogicHelper.CalculateRTP_Percentage(sumOfGroup.BetAmount, sumOfGroup.PayoutAmount);
                    sumOfGroup.GameVendorTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameVendorTypeEnum)sumOfGroup.GameVendorType);

                    return sumOfGroup;
                })
                .ToList();

            var viewModel = new GameVendorBetSummaryReportViewModel();


            viewModel.Items = groupByCurrency;
            viewModel.TotalSummary = groupByCurrency.CalculateTotalSumMergeToOneObject();

            return viewModel;
        }



        public Stream ExportDailySummaryExcelByFilter(int userID, QueryMerchantDailySummaryStatReportReportApiRequestParameterModel filter)
        {
            var paging = GetMerchantDailySummaryStatReport(userID, filter);
            var excelStream = ExcelDataConvertHelper.ConvertToMerchantDailySummaryExcelStream(paging.Items);
            return excelStream;
        }




        private List<MerchantGameSummaryCorrectionModel> GetVendorConfigMigrateCorrectionData(
            int vendorConfigID,
            int gameVendorID,
            DateTime begin,
            DateTime end,
            double timezone)
        {          
            var queryExtra = MerchantVendorConfigMigrateRecordRepository.GetNeedCorrectionData(vendorConfigID, gameVendorID, begin, end, timezone);

            if (queryExtra.IsEmptyOrNull())
                return queryExtra;

            var queryMerchantDic = MerchantRepository.GetByIds(queryExtra.Select(x => x.MerchantID).ToList()).ToDictionary(z => z.MerchantID, x => x);
                        
            foreach(var item in queryExtra)
            {
                item.Currency = queryMerchantDic.GetDictionaryByIntOrDefaultValue(item.MerchantID, null)?.Currency;
            }

            return queryExtra;
        }


        private void CheckVendorConfigMigrateAndCalculateCorrection(List<Stat_GameVendorStatSummaryThreeSourceCompareModel> listOfQuery , List<MerchantGameSummaryCorrectionModel> correctionData)
        {          
            if (listOfQuery.IsEmptyOrNull())
                return;

            if (correctionData.IsEmptyOrNull())
                return;

            foreach(var item in listOfQuery)
            {
                var data_matchDate = correctionData.Where(z => z.StatDate == item.StatDate).ToList();

                if (data_matchDate.IsEmptyOrNull())
                    continue;

                var array_plus = data_matchDate.Where(x => !x.IsMinus).ToList();
                var array_minus = data_matchDate.Where(x => x.IsMinus).ToList();

                item.Stat_BetAmount += array_plus.Select(x => x.BetAmount).Sum();
                item.ThirdSettleDate_BetAmount += array_plus.Select(x => x.BetAmount).Sum();

                item.Stat_WinLossAmount += array_plus.Select(x => x.WinLossAmount).Sum();
                item.ThirdSettleDate_WinLossAmount += array_plus.Select(x => x.WinLossAmount).Sum();

                item.Stat_BetAmount += array_minus.Select(x => x.BetAmount).Sum() * -1;
                item.ThirdSettleDate_BetAmount += array_minus.Select(x => x.BetAmount).Sum() *-1;

                item.Stat_WinLossAmount += array_minus.Select(x => x.WinLossAmount).Sum() * -1;
                item.ThirdSettleDate_WinLossAmount += array_minus.Select(x => x.WinLossAmount).Sum() * -1;
            }
        }

        private void CheckVendorConfigMigrateAndCalculateCorrection(List<EachCurrencyStatReportTotalSumQueryModel> listOfQuery, List<MerchantGameSummaryCorrectionModel> correctionData)
        {
            if (listOfQuery.IsEmptyOrNull())
                return;

            if (correctionData.IsEmptyOrNull())
                return;

            foreach (var item in listOfQuery)
            {
                var data_matchCurrency = correctionData.Where(z => z.Currency == item.Currency).ToList();

                if (data_matchCurrency.IsEmptyOrNull())
                    continue;

                var array_plus = data_matchCurrency.Where(x => !x.IsMinus).ToList();
                var array_minus = data_matchCurrency.Where(x => x.IsMinus).ToList();

                item.TotalRound += array_plus.Select(x => x.TotalRound).Sum();
                item.BetAmount += array_plus.Select(x => x.BetAmount).Sum();
                item.WinLossAmount += array_plus.Select(x => x.WinLossAmount).Sum();

                item.TotalRound += array_minus.Select(x => x.TotalRound).Sum() * -1;
                item.BetAmount += array_minus.Select(x => x.BetAmount).Sum() * -1;
                item.WinLossAmount += array_minus.Select(x => x.WinLossAmount).Sum() * -1;
            }
        }








        private VendorConfigFinanceCompareViewModel InstanceEmptyFakeModel(DateTime targetDate)
        {
            var emptySettleDate = new SettleDateDiffModel();

            var randomeValue = new SettleDateDiffModel();
            randomeValue.TotalRound = RandomHelper.GetRandIntegerNumbers(50, 99);
            randomeValue.TotalBetAmount = RandomHelper.GetRandIntegerNumbers(1000, 5000);
            randomeValue.TotalWinLossAmount = RandomHelper.GetRandIntegerNumbers(-4000, 4000);

            return new VendorConfigFinanceCompareViewModel
            {
                StatDate = targetDate,
                PgsSettleThisDate_But_ThirdSettleOtherDate = emptySettleDate,
                ThirdSettleThisDate_But_PgsSettleOtherDate = emptySettleDate,
                Diff_AfterAdjustment_vs_AggregateByThirdSettleDate = emptySettleDate,
                Diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport = emptySettleDate,
                FetchThirdPartyStatisticReport = randomeValue,
                PgsAfterAdjustment = randomeValue,
                PgsStatDB = randomeValue,
                PgsAggregateByThirdSettleDate = randomeValue,
            };
        }


        private bool IsChildAgentExist(int targetMerchantID)
        {
            var queryMyChilds = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                ParentMerchantID = targetMerchantID
            });

            if (queryMyChilds.IsNotEmptyNull())
            {
                return true;
            }

            return false;
        }


        private (string ServerTimeTW, string LastStatTime) GetDisplayCurrentServerTimeTW(DateTime? now_utc0 = null)
        {
            if (!now_utc0.HasValue)
            {
                now_utc0 = DateTimeHelper.GetCurrentTime();
            }

            string serverTimeTW = now_utc0.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
            return (serverTimeTW, serverTimeTW); //第二個欄位可能會用到 但先不實作
        }


    }
}
